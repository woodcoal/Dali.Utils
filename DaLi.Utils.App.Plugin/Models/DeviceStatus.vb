'' ------------------------------------------------------------
''
'' 	Copyright © 2021 湖南大沥网络科技有限公司.
'' 	Dali.Utils Is licensed under Mulan PSL v2.
''
'' 	  author:	木炭(WOODCOAL)
'' 	   email:	i@woodcoal.cn
'' 	homepage:	http://www.hunandali.com/
''
'' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
''
'' ------------------------------------------------------------
''
'' 	应用状态
''
'' 	name: Model.AppStatus
'' 	create: 2024-07-01
'' 	memo: 应用状态
''
'' ------------------------------------------------------------

'Namespace Model

'	''' <summary>后台服务状态</summary>
'	Public Class DeviceStatus

'		Public Sub New(id As String, name As String, type As String)
'			Me.Name = name
'			Me.ID = id
'			Me.Type = type
'		End Sub

'		''' <summary>服务标识</summary>
'		Public Property ID As Long

'		''' <summary>服务名称</summary>
'		Public Property Name As String

'		''' <summary>类型</summary>
'		Public Property Type As String

'		''' <summary>运行消息</summary>
'		Public Property Messages As New List(Of String)

'		''' <summary>历史运行记录(开始结束时间)</summary>
'		Public Property History As New Dictionary(Of Date, Date)

'		''' <summary>启动时间</summary>
'		Public Property TimeStart As Date

'		''' <summary>最后执行时间</summary>
'		Public Property TimeLast As Date

'		''' <summary>设备状态信息</summary>
'		Public Property Information As Dictionary(Of String, Object)

'		'''' <summary>设置消息</summary>
'		'Public Sub MessageInsert(content As String)
'		'	SyncLock Messages
'		'		TimeLast = SYS_NOW_DATE
'		'		Messages.Add($"[{SYS_NOW_DATE:HH:mm:ss}] {content}")
'		'	End SyncLock
'		'End Sub

'		'''' <summary>重置消息，保留最后 N 条</summary>
'		'Public Sub MessageReset(Optional maxLength As Integer = 100)
'		'	SyncLock Messages
'		'		If Messages.Count > maxLength Then Messages.RemoveRange(0, Messages.Count - maxLength)
'		'	End SyncLock
'		'End Sub
'	End Class

'End Namespace