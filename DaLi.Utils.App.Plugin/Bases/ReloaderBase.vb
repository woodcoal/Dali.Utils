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
' 	自动重载数据基类
'
' 	name: Base.ReloaderBase
' 	create: 2023-02-14
' 	memo: 自动重载数据基类
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Threading
Imports Microsoft.Extensions.Logging

Namespace Base

	''' <summary>自动重载数据基类</summary>
	Public MustInherit Class ReloaderBase(Of Key, Value)
		Implements IReloader

		''' <summary>内部驱动</summary>
		Public Instance As ImmutableDictionary(Of Key, Value) = ImmutableDictionary.Create(Of Key, Value)

		''' <summary>最后更新时间</summary>
		Private _Last As Date

		''' <summary>最后更新时间</summary>
		Public ReadOnly Property Last As Date Implements IReloader.Last
			Get
				Return _Last
			End Get
		End Property

		''' <summary>运行次数</summary>
		Private _Times As Integer

		''' <summary>运行次数</summary>
		Public ReadOnly Property Times As Integer
			Get
				Return _Times
			End Get
		End Property

		''' <summary>日志对象</summary>
		Protected Log As ILogger(Of IReloader)

		''' <summary>定时检查事件</summary>
		Protected Event ReloadCheck()

		''' <summary>延时操作时长，两次重新加载数据的最短时间间隔，默认 5 分钟，单位：秒</summary>
		Public Overridable ReadOnly Property Delay As Integer = 300

		''' <summary>构造</summary>
		Public Sub New(log As ILogger(Of IReloader))
			Me.Log = log

			Call UpdateReloadLast()
			Call UpdateReloadStatus()
		End Sub

#Region "重置更新状态"

		''' <summary>重置最后操作时间，以便快速重启任务</summary>
		Protected Sub UpdateReloadLast()
			_Last = New Date
		End Sub

		''' <summary>数据发生变化，需要更新数据</summary>
		Protected Property ChangedStatus As Boolean

		''' <summary>更新状态，通知重载数据</summary>
		Public Sub UpdateReloadStatus() Implements IReloader.UpdateReloadStatus
			ChangedStatus = True
		End Sub

#End Region

#Region "执行数据重载"

		''' <summary>加锁对象</summary>
		Private ReadOnly _Lock As New Object

		''' <summary>重载基础数据操作</summary>
		Protected MustOverride Function ReloadInstance() As Dictionary(Of Key, Value)

		''' <summary>重载数据，可后台定时更新此任务, 10 分钟最多允许更新一次</summary>
		Public Async Function ReloadAsync(stoppingToken As CancellationToken) As Task Implements IReloader.ReloadAsync
			' 时间未到，不操作
			If Last.AddSeconds(Delay) > SYS_NOW_DATE Then Return

			' 间隔时间到，检查事件
			RaiseEvent ReloadCheck()

			' 非有效状态，不更新数据
			If Not ChangedStatus Then Return

			' 执行任务
			Await Task.Run(Sub()
							   ChangedStatus = False
							   _Last = SYS_NOW_DATE
							   Interlocked.Increment(_Times)

							   SyncLock _Lock
								   Try
									   Dim dic = ReloadInstance()
									   If dic Is Nothing Then
										   Instance = ImmutableDictionary.Create(Of Key, Value)
									   Else
										   Instance = dic.ToImmutableDictionary
									   End If
								   Catch ex As Exception
									   Log.LogError(ex, "重载系统数据 {name} 失败", [GetType].FullName)
								   End Try
							   End SyncLock
						   End Sub, stoppingToken)
		End Function

		''' <summary>强制重新更新数据，不建议使用，最好定时后台调用 ReloadAsync 异步操作</summary>
		''' <param name="must">是否立即重载，True 立即操作；False 如果间隔时间小于指定时间，则不进行重载</param>
		Public Sub Reload(Optional must As Boolean = False) Implements IReloader.Reload
			If must Then
				Call UpdateReloadLast()
				Call UpdateReloadStatus()
			End If

			Call ReloadAsync(Nothing)
		End Sub

#End Region

#Region "获取，更新数据"

		''' <summary>获取对象数据</summary>
		Default Public ReadOnly Property Item(key As Key) As Value
			Get
				If key IsNot Nothing AndAlso Instance.ContainsKey(key) Then
					Return Instance(key)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>手动更新数据，key不存在则添加，val为空则删除，不建议此操作，不方便整体 reload 数据</summary>
		Protected Sub Update(key As Key, val As Value)
			' 键无效，不操作
			If key Is Nothing Then Exit Sub

			' 删除模式，如果键不存在也不进行操作
			If val Is Nothing AndAlso Not Instance.ContainsKey(key) Then Exit Sub

			' 先移除，Key 不存在则返回本身
			Instance = Instance.Remove(key)

			' 后添加
			If val IsNot Nothing Then Instance = Instance.Add(key, val)
		End Sub

#End Region

	End Class

End Namespace