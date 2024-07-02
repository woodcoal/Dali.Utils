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
' 	后台任务服务
'
' 	name: Provider.BackServiceProvider
' 	create: 2023-02-15
' 	memo: 后台任务服务
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Threading
Imports FreeRedis
Imports Microsoft.Extensions.Hosting

Namespace Provider

	''' <summary>后台任务服务</summary>
	Public Class BackServiceProvider
		Inherits BackgroundService

		''' <summary>每隔 N 秒检测一次任务状态，如果存在变化则更新</summary>
		Private Const REDIS_INTERVAL = 30

		''' <summary>任务离线时长，如果最后时间超过 N 小时，则移除状态</summary>
		Private Const REDIS_OFFLINE_INTERVAL = 24

		''' <summary>Redis 客户端</summary>
		Private Shared ReadOnly Property Redis As RedisClient
			Get
				Return SYS.GetService(Of RedisClient)
			End Get
		End Property

		'Public Sub New()
		'	Add(SYS.Plugins.GetInstances(Of BackServiceBase))

		'	' 注册全局事件，以便注册任务
		'	SYS.Events.Register(E_BACKSERVICE_ADD, Sub(service As BackServiceBase) Add(service))
		'	SYS.Events.Register(E_BACKSERVICE_REMOVE, Sub(id As String) Remove(id))
		'	SYS.Events.Register(E_BACKSERVICE_START, Sub(id As String) ServiceStart(id))
		'	SYS.Events.Register(E_BACKSERVICE_STOP, Sub(id As String) ServiceStop(id))
		'End Sub

		''' <summary>构造，并从系统实例中加载后台任务数据</summary>
		Public Sub New()
			Add(SYS.GetServices(Of BackServiceBase))

			' 注册全局事件，以便注册任务
			SYS.Events.Register(E_BACKSERVICE_ADD, Sub(service As BackServiceBase) Add(service))
			SYS.Events.Register(E_BACKSERVICE_REMOVE, Sub(id As String) Remove(id))
			SYS.Events.Register(E_BACKSERVICE_START, Sub(id As String) ServiceStart(id))
			SYS.Events.Register(E_BACKSERVICE_STOP, Sub(id As String) ServiceStop(id))
		End Sub

		''' <summary>执行操作</summary>
		Protected Overrides Function ExecuteAsync(stoppingToken As CancellationToken) As Task
			'Call TimeCheckAsync(stoppingToken)
			Call ServiceCheckAsync(stoppingToken)
			Call ServiceStatusAsync(stoppingToken)

			Return Task.CompletedTask
		End Function

		''' <summary>注销</summary>
		Public Overrides Sub Dispose()
			' 移除所有远程服务状态
			ServiceStatusRemove()

			GC.SuppressFinalize(Me)

			MyBase.Dispose()
		End Sub

#Region "任务操作"

		''' <summary>任务列表</summary>
		Private Shared ReadOnly _Instance As New ConcurrentDictionary(Of String, BackServiceBase)(StringComparer.OrdinalIgnoreCase)

		''' <summary>任务列表</summary>
		Private Shared ReadOnly _CacheStatus As New Dictionary(Of String, Date)(StringComparer.OrdinalIgnoreCase)

		''' <summary>任务列表</summary>
		Public Shared ReadOnly Property List As List(Of BackServiceBase)
			Get
				Return _Instance.Values.ToList
			End Get
		End Property

		''' <summary>获取后台任务</summary>
		Public Shared ReadOnly Property Item(id As String) As BackServiceBase
			Get
				If id.NotEmpty AndAlso _Instance.ContainsKey(id) Then
					Return _Instance(id)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>添加后台任务</summary>
		Public Shared Function Add(service As BackServiceBase) As Boolean
			If Not _Instance.ContainsKey(service.ID) Then
				Return _Instance.TryAdd(service.ID, service)
			Else
				Return False
			End If
		End Function

		''' <summary>批量添加后台任务</summary>
		Public Shared Sub Add(services As IEnumerable(Of BackServiceBase))
			If services?.Count > 0 Then
				For Each sev In services
					Add(sev)
				Next
			End If
		End Sub

		''' <summary>移除后台任务</summary>
		Public Shared Function Remove(id As String) As Boolean
			Dim Service = Item(id)

			If Service IsNot Nothing Then
				Service.StopAsync(Nothing).Wait()

				Return _Instance.TryRemove(id, Nothing)
			Else
				Return False
			End If
		End Function

		''' <summary>启动任务</summary>
		Public Shared Function ServiceStart(id As String) As BackServiceStatus
			Dim Service = Item(id)
			If Service Is Nothing Then Return Nothing

			Service.StartAsync(False)
			Return Service.Status
		End Function

		''' <summary>结束任务</summary>
		Public Shared Function ServiceStop(id As String) As BackServiceStatus
			Dim Service = Item(id)
			If Service Is Nothing Then Return Nothing

			Service.StopAsync(Nothing)
			Return Service.Status
		End Function

#End Region

#Region "远程任务"

		''' <summary>远程服务列表</summary>
		Public Shared Function Services() As List(Of BackServiceStatus)
			Dim dic = StatusCacheProvider.BackServiceStatus.GET
			If dic.IsEmpty Then Return Nothing

			' 检测是否超时，超时则移除
			Dim last = Now.AddHours(0 - REDIS_OFFLINE_INTERVAL)
			Dim keys = dic.Where(Function(x) x.Value.TimeLast < last).Select(Function(x) x.Key).ToArray
			If keys.IsEmpty Then
				Return dic.Select(Function(x) x.Value).ToList
			Else
				' 移除超时键
				StatusCacheProvider.BackServiceStatus.DEL(keys)

				Return dic.Where(Function(x) Not keys.Contains(x.Key)).Select(Function(x) x.Value).ToList
			End If
		End Function

		''' <summary>根据类型获取远程服务列表</summary>
		Public Shared Function Services(type As String) As List(Of BackServiceStatus)
			If type.IsEmpty Then Return Nothing

			Dim list = Services()
			If list Is Nothing Then Return Nothing

			Return list.Where(Function(x) x.Type.IsSame(type)).ToList
		End Function

		'''' <summary>远程服务标识列表</summary>
		'Public Shared Function ServiceIDs() As String()
		'	Return StatusCacheProvider.BackServiceStatus?.Keys
		'End Function

		''' <summary>远程服务</summary>
		Public Shared Function Service(id As String) As BackServiceStatus
			If Redis Is Nothing Then Return Nothing

			Dim status = StatusCacheProvider.BackServiceStatus.GET(id)
			If status Is Nothing Then Return Nothing

			' 检测是否超时，超时则移除
			Dim last = Now.AddHours(0 - REDIS_OFFLINE_INTERVAL)
			If status.TimeLast < last Then
				StatusCacheProvider.BackServiceStatus.DEL(id)
				Return Nothing
			End If

			Return status
		End Function

		''' <summary>清除所有记录</summary>
		Public Shared Sub ServiceClear()
			StatusCacheProvider.BackServiceStatus.CLEAR()
		End Sub

#End Region

#Region "后台服务"

		''' <summary>任务检查服务</summary>
		Private Shared Function ServiceCheckAsync(stoppingToken As CancellationToken) As Task
			Return Task.Run(Sub()
								Dim s As New Stopwatch
								While True
									s.Restart()

									Dim Services = List.Where(Function(x) x.AutoStart AndAlso Not x.Status.IsBusy AndAlso Not x.Status.IsForceStop AndAlso Not x.Disabled).ToList

									For Each sev In Services
										Call sev.StartAsync(True, stoppingToken)
										If stoppingToken.IsCancellationRequested Then Exit For
									Next

									If stoppingToken.IsCancellationRequested Then Exit While

									s.Stop()

									' 5 秒轮询一次
									Dim delay = 4999 - s.ElapsedMilliseconds
									If delay > 0 Then Thread.Sleep(delay)
								End While
							End Sub, stoppingToken)
		End Function

		''' <summary>任务状态更新</summary>
		Private Shared Function ServiceStatusAsync(stoppingToken As CancellationToken) As Task
			If Redis Is Nothing Then Return Task.CompletedTask

			Return Task.Run(Sub()
								While True
									SyncLock _CacheStatus
										SyncLock _Instance
											For Each sev In _Instance
												If Not _CacheStatus.ContainsKey(sev.Key) Then _CacheStatus.Add(sev.Key, New Date)

												' 状态发生变化
												If _CacheStatus(sev.Key) < sev.Value.Status.TimeLast Then
													StatusCacheProvider.BackServiceStatus.SET(sev.Value.ID, sev.Value.Status)
													_CacheStatus(sev.Key) = SYS_NOW_DATE
												End If

												If stoppingToken.IsCancellationRequested Then Exit For
											Next
										End SyncLock
									End SyncLock

									If stoppingToken.IsCancellationRequested Then Exit While

									' 延时 N 秒，然后重新轮询一次任务
									For I = 1 To REDIS_INTERVAL
										If stoppingToken.IsCancellationRequested Then Exit For
										Thread.Sleep(1000)
									Next
								End While
							End Sub, stoppingToken)
		End Function

		''' <summary>移除所有后台服务</summary>
		Private Shared Sub ServiceStatusRemove()
			Dim redis = SYS.GetSetting(Of RedisSetting).CreateClient
			If redis Is Nothing Then Return

			For Each sev In _Instance
				StatusCacheProvider.BackServiceStatus.DEL(sev.Value.ID)
			Next
		End Sub
#End Region

	End Class

End Namespace

