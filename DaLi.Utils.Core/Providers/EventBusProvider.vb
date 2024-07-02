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
' 	事件总线
'
' 	name: Provider.EventBusProvider
' 	create: 2023-02-16
' 	memo: 事件总线
'
' ------------------------------------------------------------

Imports System.Collections.Immutable

Namespace Provider

	''' <summary>事件总线</summary>
	Public Class EventBusProvider

		''' <summary>内部事件列表</summary>
		Protected Instance As ImmutableDictionary(Of String, ImmutableList(Of EventAction)) = ImmutableDictionary.Create(Of String, ImmutableList(Of EventAction))

		''' <summary>锁定项目</summary>
		Private ReadOnly _Lock As New Object

#Region "AOP 事件"

		''' <summary>命令注册前操作事件</summary>
		Public Event RegisterBefore(name As String, action As EventAction)

		''' <summary>命令注册后操作事件</summary>
		Public Event RegisterAfter(name As String, action As EventAction)

		''' <summary>命令注销前操作事件</summary>
		Public Event UnregisterBefore(name As String, action As EventAction)

		''' <summary>命令注销后操作事件</summary>
		Public Event UnregisterAfter(name As String, action As EventAction)

		''' <summary>提交执行前操作事件</summary>
		Public Event SubmitBefore(command As String, args As Object, isAsync As Boolean, isWait As Boolean)

		''' <summary>提交执行后操作事件</summary>
		Public Event SubmitAfter(command As String, args As Object, isAsync As Boolean, isWait As Boolean)

#End Region

		''' <summary>获取/设置内置事件</summary>
		Public Property Actions(command As String) As ImmutableList(Of EventAction)
			Get
				If command.NotEmpty Then
					command = command.Trim.ToLower
					If Instance.ContainsKey(command) Then Return Instance(command)
				End If

				Return ImmutableList.Create(Of EventAction)
			End Get
			Private Set(value As ImmutableList(Of EventAction))
				If command.NotEmpty Then
					command = command.Trim.ToLower

					SyncLock _Lock
						If value Is Nothing Then
							Instance = Instance.Remove(command)
						Else
							If Instance.ContainsKey(command) Then
								Instance = Instance.SetItem(command, value)
							Else
								Instance = Instance.Add(command, value)
							End If
						End If
					End SyncLock
				End If
			End Set
		End Property

		''' <summary>注册命令</summary>
		''' <param name="command">命令</param>
		''' <param name="action">要执行的操作</param>
		''' <param name="delay">执行方式。0：收到请求立即执行；大于0：防抖，时间到后才执行；小于0：防抖，立即执行；单位：毫秒，默认延时 5 秒</param>
		Public Overridable Sub Register(command As String, action As [Delegate], Optional delay As Integer = 5000)
			If command.IsEmpty OrElse action Is Nothing Then Return

			Dim actions = Me.Actions(command)

			If Not actions.Any(Function(x) x.Equals(action)) Then
				Dim eventAction As New EventAction(action, delay)

				RaiseEvent RegisterBefore(command, eventAction)
				SyncLock _Lock
					Me.Actions(command) = actions.Add(eventAction)
				End SyncLock
				RaiseEvent RegisterAfter(command, eventAction)
			End If
		End Sub

		'''  <summary>移除命令</summary>
		''' <param name="command">命令</param>
		''' <param name="action">要移除的操作事件，如果不存在则全部移除</param>
		Public Overridable Sub Unregister(command As String, Optional action As [Delegate] = Nothing)
			If command.IsEmpty Then Return

			If action Is Nothing Then
				' 全部移除
				RaiseEvent UnregisterBefore(command, Nothing)

				SyncLock _Lock
					Instance = Instance.Remove(command)
				End SyncLock

				RaiseEvent UnregisterAfter(command, Nothing)
			Else
				' 部分移除
				Dim actions = Me.Actions(command)
				Dim eventAction = actions.Where(Function(x) x.Equals(action)).FirstOrDefault

				If eventAction IsNot Nothing Then
					RaiseEvent UnregisterBefore(command, eventAction)

					SyncLock _Lock
						Me.Actions(command) = actions.Remove(eventAction)
					End SyncLock

					RaiseEvent UnregisterAfter(command, eventAction)
				End If
			End If
		End Sub

		''' <summary>提交命令</summary>
		''' <param name="command">命令</param>
		''' <param name="args">提交的数据</param>
		''' <param name="isAsync">是否异步执行，True 所有相同命令下的操作都同时执行，False 按提交顺序依次执行</param>
		''' <param name="isWait">是否等待全部执行完成</param>
		''' <returns>返回任务状态，注意如果是异步任务即算返回状态，其结果也实际无意义</returns>
		Public Overridable Function Submit(command As String, args As Object, Optional isAsync As Boolean = False, Optional isWait As Boolean = True) As List(Of EventAction.ActionResult)
			' 无数据，不执行
			Dim actions = Me.Actions(command)
			If actions.IsEmpty Then Return Nothing

			RaiseEvent SubmitBefore(command, args, isAsync, isWait)

			Dim result As New List(Of EventAction.ActionResult)
			Dim Execute = Sub(action As EventAction, params As Object)
							  action.Execute(params)
							  result.Add(action.Result)
						  End Sub

			' 所有任务都同时执行
			If isAsync Then
				Dim tasks = actions.Select(Function(x) Task.Run(Sub() Execute(x, args))).ToArray
				If isWait Then Task.WaitAll(tasks)
			Else
				' 任务按顺序执行
				Dim tasks = Task.Run(Sub() actions.ForEach(Sub(x) Execute(x, args)))
				If isWait Then tasks.Wait()
			End If

			RaiseEvent SubmitAfter(command, args, isAsync, isWait)

			Return result
		End Function

	End Class

End Namespace