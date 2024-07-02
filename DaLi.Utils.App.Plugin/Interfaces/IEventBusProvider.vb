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
' 	总线事件接口
'
' 	name: Interface.IEventBusProvider
' 	create: 2023-02-14
' 	memo: 总线事件接口
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>总线事件接口</summary>
	Public Interface IEventBusProvider
		Inherits IDisposable

		''' <summary>注册命令</summary>
		''' <param name="command">命令</param>
		''' <param name="action">要执行的操作</param>
		''' <param name="delay">执行方式。0：收到请求立即执行；大于0：防抖，时间到后才执行；小于0：防抖，立即执行；单位：毫秒，默认延时 5 秒</param>
		Sub Register(command As String, action As [Delegate], Optional delay As Integer = 5000)

		'''  <summary>移除命令</summary>
		''' <param name="command">命令</param>
		''' <param name="action">要移除的操作事件，如果不存在则全部移除</param>
		Sub Unregister(command As String, Optional action As [Delegate] = Nothing)

		''' <summary>提交命令</summary>
		''' <param name="command">命令</param>
		''' <param name="args">提交的数据</param>
		''' <param name="isAsync">是否异步执行，True 所有相同命令下的操作都同时执行，False 按提交顺序依次执行</param>
		''' <param name="isWait">是否等待全部执行完成</param>
		''' <returns>是否提交成功</returns>
		Function Submit(command As String, args As Object, Optional isAsync As Boolean = False, Optional isWait As Boolean = True) As List(Of EventAction.ActionResult)

		''' <summary>发布远程命令</summary>
		''' <param name="command">命令</param>
		''' <returns>是否提交成功</returns>
		Function Publish(command As String, Optional sysIDs() As Long = Nothing) As Boolean

		''' <summary>发布远程命令</summary>
		''' <param name="command">命令</param>
		''' <param name="args">提交的数据</param>
		''' <param name="isAsync">是否异步执行，True 所有相同命令下的操作都同时执行，False 按提交顺序依次执行</param>
		''' <returns>是否提交成功</returns>
		Function Publish(Of T)(command As String, args As T, Optional isAsync As Boolean = False, Optional sysIDs() As Long = Nothing) As Boolean

		''' <summary>发布远程命令，如果未开启 Redis 则将自动转换为本机操作</summary>
		''' <param name="executeData">操作指令</param>
		Function Publish(executeData As EventRemote) As Boolean

	End Interface
End Namespace
