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
' 	系统通用扩展
'
' 	name: Extension.SystemExtension
' 	create: 2022-01-03
' 	memo: 系统通用扩展
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Runtime.CompilerServices

Namespace Extension
	''' <summary>系统通用扩展</summary>
	Public Module SystemExtension

		''' <summary>数据交换</summary>
		Public Sub Swap(ByRef source As Object, ByRef target As Object)
			Dim x = source
			source = target
			target = x
		End Sub

		''' <summary>延时操作</summary>
		<Extension>
		Public Function SetTimeout(this As Action(Of Timers.ElapsedEventArgs), timeout As Double) As Timers.Timer
			If this IsNot Nothing AndAlso timeout > 0 Then
				Dim timer As New Timers.Timer(timeout)

				AddHandler timer.Elapsed, Sub(sender As Object, e As Timers.ElapsedEventArgs)
											  timer.Enabled = False
											  this.Invoke(e)
											  timer.Dispose()
										  End Sub

				timer.Enabled = True

				Return timer
			End If

			Return Nothing
		End Function

		''' <summary>周期操作</summary>
		<Extension>
		Public Function SetInterval(this As Action(Of Timers.ElapsedEventArgs), timeout As Double) As Timers.Timer
			If this IsNot Nothing AndAlso timeout > 0 Then
				Dim timer As New Timers.Timer(timeout)
				AddHandler timer.Elapsed, Sub(sender As Object, e As Timers.ElapsedEventArgs) this.Invoke(e)

				timer.Enabled = True

				Return timer
			End If

			Return Nothing
		End Function

		''' <summary>关闭延时，周期</summary>
		<Extension>
		Public Sub ClearTimer(ByRef this As Timers.Timer)
			If this IsNot Nothing Then
				this.Enabled = False
				this.Dispose()
				this = Nothing
			End If
		End Sub

		''' <summary>函数防抖</summary>
		''' <param name="this">目标函数</param>
		''' <param name="timeout">延迟执行毫秒数</param>
		''' <param name="immediate">true - 立即执行， false - 延迟执行</param>
		<Extension>
		Public Function Debounce(this As Action, timeout As Double, Optional immediate As Boolean = False) As Action
			Dim timer As Timers.Timer = Nothing

			Return Sub()
					   ' 定时器是否已经设置过
					   Dim emptyTimer = timer Is Nothing

					   ' 清空原定时器
					   ClearTimer(timer)

					   If immediate Then
						   ' 立即执行
						   ' 重新创建定时器，到指定时间结束定时
						   timer = SetTimeout(Sub() ClearTimer(timer), timeout)

						   ' 如果之前没有设置过定时器，表示第一次执行，则立即执行
						   If emptyTimer Then this.Invoke
					   Else
						   ' 延时执行，重新创建定时器
						   timer = SetTimeout(Sub() this.Invoke, timeout)
					   End If
				   End Sub
		End Function

		''' <summary>函数防抖</summary>
		''' <param name="this">目标函数</param>
		''' <param name="timeout">延迟执行毫秒数</param>
		''' <param name="immediate">true - 立即执行， false - 延迟执行</param>
		<Extension>
		Public Function Debounce(Of T)(this As Action(Of T), timeout As Double, Optional immediate As Boolean = False) As Action(Of T)
			Dim timer As Timers.Timer = Nothing

			Return Sub(args As T)
					   ' 定时器是否已经设置过
					   Dim emptyTimer = timer Is Nothing

					   ' 清空原定时器
					   ClearTimer(timer)

					   If immediate Then
						   ' 立即执行
						   ' 重新创建定时器，到指定时间结束定时
						   timer = SetTimeout(Sub() ClearTimer(timer), timeout)

						   ' 如果之前没有设置过定时器，表示第一次执行，则立即执行
						   If emptyTimer Then this.Invoke(args)
					   Else
						   ' 延时执行，重新创建定时器
						   timer = SetTimeout(Sub() this.Invoke(args), timeout)
					   End If
				   End Sub
		End Function

		''' <summary>锁列表</summary>
		Private ReadOnly _Lockers As New ConcurrentDictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)

		''' <summary>全局锁操作</summary>
		''' <param name="this">锁定后的操作</param>
		''' <param name="isGlobal">是否全局锁定，及整个项目都锁定，否则仅当前函数内锁定</param>
		Public Sub Locker(this As Action, isGlobal As Boolean)
			Locker(this, If(isGlobal, "", Nothing))
		End Sub

		''' <summary>全局锁操作</summary>
		''' <param name="this">锁定后的操作</param>
		''' <param name="obj">锁定对象，注意仅使用此对象的类名作为锁名称，而非此对象本身。为空则使用全局对象</param>
		''' <param name="name">附加名称，将与锁定对象类名组合，方便同类多项目锁定</param>
		Public Sub Locker(this As Action, obj As Object, Optional name As String = Nothing)
			name = name.EmptyValue

			If obj Is Nothing Then
				Locker(this, name)
			Else
				Locker(this, obj.GetType.Name & name)
			End If
		End Sub

		''' <summary>全局锁操作</summary>
		''' <param name="this">锁定后的操作</param>
		''' <param name="lockerName">锁名称，所有同名的项目都使用同一个锁；为空则仅当前函数内锁定</param>
		Public Sub Locker(this As Action, Optional lockerName As String = Nothing)
			If this Is Nothing Then Return

			' 未设置锁定名，使用当前函数内锁定
			If lockerName Is Nothing Then
				Try
					If this.Target IsNot Nothing Then
						If this.Target.GetType.BaseType = GetType(Object) Then
							lockerName = this.Target.GetType.FullName
						Else
							lockerName = this.Target.Method.DeclaringType.FullName
						End If
					Else
						lockerName = this.Method.DeclaringType.FullName
					End If
				Catch ex As Exception
					lockerName = this.Method.DeclaringType.FullName
				End Try
			End If

			' 寻找锁对象，不存在则创建
			Dim obj = _Lockers.GetOrAdd(lockerName.EmptyValue, New Object)

			' 锁定	
			SyncLock obj
				this.Invoke
			End SyncLock
		End Sub

	End Module
End Namespace