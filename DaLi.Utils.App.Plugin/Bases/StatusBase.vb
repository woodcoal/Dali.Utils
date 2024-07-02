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
' 	title
'
' 	name: StatusBase.vb
' 	create: 2024--
' 	memo: introduce
'
' ------------------------------------------------------------

Namespace Base

	''' <summary>状态基类</summary>
	Public Class StatusBase


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
		Public Property Message As New List(Of String)

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
			SyncLock Message
				TimeLast = SYS_NOW_DATE
				Message.Add($"[{SYS_NOW_DATE:HH:mm:ss}] {content}")
			End SyncLock
		End Sub

		''' <summary>重置消息，保留最后 N 条</summary>
		Public Sub MessageReset(Optional maxLength As Integer = 10)
			SyncLock Message
				If Message.Count > maxLength Then Message.RemoveRange(0, Message.Count - maxLength)
			End SyncLock
		End Sub
	End Class

End Namespace