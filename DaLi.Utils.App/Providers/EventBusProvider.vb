' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	总线事件
'
' 	name: Provider.EventBusProvider
' 	create: 2023-02-14
' 	memo: 总线事件
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports FreeRedis
Imports Microsoft.Extensions.Logging

Namespace Provider

	''' <summary>总线事件</summary>
	Public Class EventBusProvider
		Inherits Utils.Provider.EventBusProvider
		Implements IEventBusProvider

		Private Const EVENT_REMOTE_COMMAND = "_EVENTBUS_REMOTE_DALI_"

		''' <summary>Redis 对象</summary>
		Protected ReadOnly Redis As RedisClient

		''' <summary>Redis 订阅对象</summary>
		Protected ReadOnly SubscribeObject As IDisposable

		''' <summary>日志组件</summary>
		Protected LOG As ILogger(Of EventBusProvider)

#Region "AOP 事件"

		''' <summary>远程命令前操作事件</summary>
		Public Event PublishBefore(command As String, executeData As EventRemote)

		''' <summary>远程命令后操作事件</summary>
		Public Event PublishAfter(command As String, executeData As EventRemote)

#End Region

		Public Sub New(Optional log As ILogger(Of EventBusProvider) = Nothing, Optional redis As RedisClient = Nothing)
			log = If(log, SYS.GetService(Of ILogger(Of EventBusProvider)))
			redis = If(redis, SYS.GetService(Of RedisClient))

			If redis IsNot Nothing Then
				Me.Redis = redis
				SubscribeObject = redis.Subscribe(EVENT_REMOTE_COMMAND, AddressOf Subscribe)
			Else
				log.LogInformation("Redis 无效，远程事件禁用")
			End If

			Me.LOG = log
		End Sub

		''' <summary>注册命令</summary>
		''' <param name="command">命令</param>
		''' <param name="action">要执行的操作</param>
		''' <param name="delay">执行方式。0：收到请求立即执行；大于0：防抖，时间到后才执行；小于0：防抖，立即执行；单位：毫秒，默认延时 5 秒</param>
		Public Overrides Sub Register(command As String, action As [Delegate], Optional delay As Integer = 5000) Implements IEventBusProvider.Register
			If command.IsEmpty OrElse action Is Nothing Then Return

			MyBase.Register(command, action, delay)

			LOG.LogInformation("事件总线 {command} 增加操作：{action}", command, action.Method.Name)
		End Sub

		'''  <summary>移除命令</summary>
		''' <param name="command">命令</param>
		''' <param name="action">要移除的操作事件，如果不存在则全部移除</param>
		Public Overrides Sub Unregister(command As String, Optional action As [Delegate] = Nothing) Implements IEventBusProvider.Unregister
			If command.IsEmpty Then Return

			MyBase.Unregister(command, action)

			If action Is Nothing Then
				LOG.LogInformation("事件总线 {command} 移除了所有操作", command)
			Else
				LOG.LogInformation("事件总线 {command} 移除操作：{action}", command, action.Method.Name)
			End If
		End Sub

		''' <summary>提交命令</summary>
		''' <param name="command">命令</param>
		''' <param name="args">提交的数据</param>
		''' <param name="isAsync">是否异步执行，True 所有相同命令下的操作都同时执行，False 按提交顺序依次执行</param>
		''' <param name="isWait">是否等待全部执行完成</param>
		''' <returns>是否提交成功</returns>
		Public Overrides Function Submit(command As String, args As Object, Optional isAsync As Boolean = False, Optional isWait As Boolean = True) As List(Of EventAction.ActionResult) Implements IEventBusProvider.Submit
			Dim result = MyBase.Submit(command, args, isAsync, isWait)
			If result Is Nothing Then Return Nothing

			LOG.LogInformation("事件总线 {command} 执行中，需操作 {count} 条", command, result.Count)

			Return result
		End Function

		''' <summary>发布远程命令</summary>
		''' <param name="command">命令</param>
		''' <returns>是否提交成功</returns>
		Public Function Publish(command As String, Optional sysIDs() As Long = Nothing) As Boolean Implements IEventBusProvider.Publish
			Return Publish(New EventRemote With {
				.Command = command,
				.IsAsync = False,
				.DevIds = sysIDs
			})
		End Function

		''' <summary>发布远程命令</summary>
		''' <param name="command">命令</param>
		''' <param name="args">提交的数据</param>
		''' <param name="isAsync">是否异步执行，True 所有相同命令下的操作都同时执行，False 按提交顺序依次执行</param>
		''' <returns>是否提交成功</returns>
		Public Function Publish(Of T)(command As String, args As T, Optional isAsync As Boolean = False, Optional sysIDs() As Long = Nothing) As Boolean Implements IEventBusProvider.Publish
			Return Publish(New EventRemote With {
				.Command = command,
				.Params = args,
				.IsAsync = isAsync,
				.DevIds = sysIDs
			})
		End Function

		''' <summary>发布远程命令，如果未开启 Redis 则将自动转换为本机操作</summary>
		''' <param name="executeData">操作指令</param>
		Public Function Publish(executeData As EventRemote) As Boolean Implements IEventBusProvider.Publish
			If executeData Is Nothing OrElse executeData.Command Is Nothing Then Return False

			' 未设置 Redis 且 sysID 与本机一致，转本机操作
			If Redis Is Nothing Then
				If executeData.DevIds Is Nothing OrElse executeData.DevIds.Contains(SYS.ID) Then
					Dim ret = Submit(executeData.Command, executeData.Params, executeData.IsAsync, False)
					Return ret IsNot Nothing
				Else
					Return False
				End If
			End If

			RaiseEvent PublishBefore(executeData.Command, executeData)

			Dim data = Utils.Extension.ToJson(executeData, False)
			Redis.Publish(EVENT_REMOTE_COMMAND, data)
			LOG.LogInformation("远程事件 {command} 发布，参数 {data}", executeData.Command, data)

			RaiseEvent PublishAfter(executeData.Command, executeData)

			Return True
		End Function

		''' <summary>订阅命令</summary>
		Private Sub Subscribe(channel As String, body As String)
			If body.IsEmpty Then Return

			Dim executeData = body.ToJsonObject(Of EventRemote)
			If executeData Is Nothing OrElse executeData.Command Is Nothing Then Return

			Dim actions = Me.Actions(executeData.Command)
			If actions.Count < 1 Then Return

			' 指定机器运行
			If executeData.DevIds.NotEmpty AndAlso Not executeData.DevIds.Contains(SYS.ID) Then Return

			LOG.LogInformation("远程事件 {command} 接收到数据 {data}；准备执行，需操作 {count} 条", executeData.Command, body, actions.Count)

			' Params 解析默认为 JSON 对象，需要进一步分析处理
			'executeData.Params = JsonElementParse(executeData.Params, True)

			' 所有任务都同时执行
			If executeData.IsAsync Then
				Dim tasks = actions.Select(Function(x) Task.Run(Sub() x.Execute(executeData.Params))).ToArray
				Task.WaitAll(tasks)
			Else
				' 任务按顺序执行
				Task.Run(Sub() actions.ForEach(Sub(x) x.Execute(executeData.Params))).Wait()
			End If
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			SubscribeObject?.Dispose()
			GC.SuppressFinalize(Me)
		End Sub
	End Class

End Namespace