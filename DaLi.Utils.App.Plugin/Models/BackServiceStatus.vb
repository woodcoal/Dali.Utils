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
' 	后台服务状态
'
' 	name: Model.BackServiceStatus
' 	create: 2023-02-14
' 	memo: 后台服务状态
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>后台服务状态</summary>
	Public Class BackServiceStatus

		Public Sub New()
			Device = SYS.ID
		End Sub

		Public Sub New(sev As BackServiceBase)
			Me.New

			If sev IsNot Nothing Then
				Name = sev.Name
				ID = sev.ID
				Type = sev.GetType.FullName
			End If
		End Sub

		Public Sub New(id As String, name As String, type As String)
			Me.New

			Me.Name = name
			Me.ID = id
			Me.Type = type
		End Sub

		''' <summary>服务标识</summary>
		Public Property ID As String

		''' <summary>服务名称</summary>
		Public Property Name As String

		''' <summary>类型</summary>
		Public Property Type As String

		''' <summary>服务所在设备标识</summary>
		Public Property Device As Long

		''' <summary>开始时间</summary>
		Public Property TimeStart As Date

		''' <summary>任务最后启动事件</summary>
		Public Property TimeRun As Date

		''' <summary>任务最后完成事件</summary>
		Public Property TimeStop As Date

		''' <summary>最后执行时间</summary>
		Public Property TimeLast As Date

		''' <summary>累计运行次数</summary>
		Public Property TotalCount As Integer

		''' <summary>累计运行时间</summary>
		Public Property TotalTime As Long

		''' <summary>是否在忙</summary>
		Public Property IsBusy As Boolean

		''' <summary>是否手动关闭，点击结束事件 StopAsync</summary>
		Public Property IsForceStop As Boolean

		''' <summary>运行消息</summary>
		Public Property Messages As New List(Of String)

		''' <summary>执行错误</summary>
		Public Property Exception As Exception

		''' <summary>定时器</summary>
		Public Property Interval As String

		Public ReadOnly Property IntervalDesc As String
			Get
				If Interval.IsEmpty Then Return ""
				Return Misc.Cron.Expression.Description(Interval.Split("|"c))
			End Get
		End Property

		''' <summary>设置消息</summary>
		Public Sub MessageInsert(content As String)
			SyncLock Messages
				TimeLast = SYS_NOW_DATE
				Messages.Add($"[{SYS_NOW_DATE:HH:mm:ss}] {content}")
			End SyncLock
		End Sub

		''' <summary>重置消息，保留最后 N 条</summary>
		Public Sub MessageReset(Optional maxLength As Integer = 10)
			SyncLock Messages
				If Messages.Count > maxLength Then Messages.RemoveRange(0, Messages.Count - maxLength)
			End SyncLock
		End Sub
	End Class

End Namespace