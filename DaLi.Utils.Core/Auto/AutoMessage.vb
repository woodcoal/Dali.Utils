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
' 	操作消息结构
'
' 	name: Auto.AutoMessage
' 	create: 2023-01-06
' 	memo: 操作消息结构
'
' ------------------------------------------------------------


Namespace Auto
	''' <summary>操作消息结构</summary>
	Public Class AutoMessage

		''' <summary>是否操作成功</summary>
		Public Property Success As Boolean

		''' <summary>操作时间</summary>
		Public Property Time As Date

		''' <summary>规则名称</summary>
		Public Property Name As String

		''' <summary>规则类型</summary>
		Public Property Type As String

		''' <summary>规则输出</summary>
		Public Property Output As String

		''' <summary>执行结果</summary>
		Public Property Result As Object

		''' <summary>消息</summary>
		Public Property Message As String

		''' <summary>子消息（循环操作相关消息）</summary>
		Public Property Children As List(Of AutoMessage)

		Public Sub New()
			Success = False
			Time = SYS_NOW_DATE
			Children = New List(Of AutoMessage)
		End Sub

		Public Sub New(Optional rule As IRule = Nothing)
			If rule IsNot Nothing Then
				Name = rule.Name
				Type = rule.Type
				Output = rule.Output
			End If

			Success = False
			Time = SYS_NOW_DATE
			Children = New List(Of AutoMessage)
		End Sub

		''' <summary>设置消息状态</summary>
		''' <param name="success">是否成功</param>
		''' <param name="message">消息内容，失败时如果消息未设置则不处理，成功时如果未设置则清空原始消息</param>
		Public Sub SetSuccess(Optional success As Boolean = True, Optional message As String = Nothing)
			Me.Success = success

			If success Then
				Me.Message = message
			Else
				If message.NotEmpty Then Me.Message = message
			End If
		End Sub

		''' <summary>复制规则</summary>
		Public Sub Copy(msg As AutoMessage)
			If msg Is Nothing Then Return

			Name = msg.Name
			Type = msg.Type
			Output = msg.Output

			Success = msg.Success
			Time = msg.Time
			Message = msg.Message
			Children = msg.Children
		End Sub

		''' <summary>复制规则</summary>
		Public Sub Add(msg As AutoMessage)
			If msg Is Nothing Then Return

			Children = If(Children, New List(Of AutoMessage))

			SyncLock Children
				Children.Add(msg)
			End SyncLock
		End Sub

		''' <summary>获取结果列表</summary>
		Public Function GetMessage(Optional level As Integer = 0) As String
			Dim succ = "😈"
			If Message?.StartsWith("调试") Then
				succ = "👽"
			ElseIf Success Then
				succ = "😊"
			End If

			Dim display = ""
			If Name.IsEmpty Then
				display = Type
			Else
				display = Name
				If Type.NotEmpty Then display &= $"({Type})"
			End If

			Dim info = Message
			If Children.NotEmpty Then
				SyncLock Children
					level += 1
					info &= vbCrLf & Children.Select(Function(x) New String(vbTab, level) & x.GetMessage(level)).ToArray.JoinString(vbCrLf)
				End SyncLock
			End If

			Return $"{succ} [{Time:HH:mm:ss}] {name} {info}"
		End Function

	End Class
End Namespace
