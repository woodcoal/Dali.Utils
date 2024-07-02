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
' 	错误消息数据
'
' 	name: Model.ErrorMessage
' 	create: 2023-02-14
' 	memo: 错误消息数据
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>错误数据</summary>
	Public Class ErrorMessage

		''' <summary>消息数据</summary>
		Public ReadOnly Messages As New NameValueDictionary

		''' <summary>异常消息内容，输出时优先级高于消息数据</summary>
		Public Exception As Exception

		''' <summary>错误编码，需要自定义时使用</summary>
		Public Code As Integer?

		''' <summary>多语言</summary>
		Protected ReadOnly Loc As ILocalizerProvider

		Public Sub New(Optional loc As ILocalizerProvider = Nothing)
			Me.Loc = If(loc, SYS.GetService(Of ILocalizerProvider))
		End Sub

		''' <summary>通过 Enum 值来定义错误</summary>
		Public WriteOnly Property ErrEnum As [Enum]
			Set(value As [Enum])
				Code = Convert.ToInt32(value)
				Notification = value.Description.EmptyValue(value.Name)
			End Set
		End Property

		''' <summary>通知消息，非校验错误时，提示的消息，优先消息数据，低于异常消息</summary>
		Public Property Notification As String
			Get
				Return Messages("_Notification_")
			End Get
			Set(value As String)
				If Loc IsNot Nothing Then value = Loc.TranslateWithPrefix(value, "Error.")
				Messages("_Notification_") = value
			End Set
		End Property

		''' <summary>成功通知消息</summary>
		Private _SuccessMessage As String

		''' <summary>成功通知消息，仅 IsPass 才生效</summary>
		Public Property SuccessMessage As String
			Get
				Return _SuccessMessage
			End Get
			Set(value As String)
				If Loc IsNot Nothing Then value = Loc.TranslateWithPrefix(value, "Success.")
				_SuccessMessage = value
			End Set
		End Property

		''' <summary>添加错误信息</summary>
		Public Function Add(name As String, message As String)
			If Loc IsNot Nothing Then message = Loc.TranslateWithPrefix(message, "Error.")
			Messages.Add(name, message)
			Return Me
		End Function

		''' <summary>是否验证通过</summary>
		Public ReadOnly Property IsPass As Boolean
			Get
				Return Messages.Count < 1 AndAlso Exception Is Nothing
			End Get
		End Property

		''' <summary>是否存在异常数据</summary>
		Public ReadOnly Property HasException As Boolean
			Get
				Return Exception IsNot Nothing
			End Get
		End Property

		''' <summary>重置</summary>
		Public Function Reset()
			Messages.Clear()
			Exception = Nothing
			SuccessMessage = ""

			Return Me
		End Function

		''' <summary>从其他错误对象赋值数据</summary>
		Public Function Copy(err As ErrorMessage)
			Call Reset()

			SuccessMessage = err.SuccessMessage
			Exception = err.Exception
			Messages.AddRangeFast(err.Messages)

			Return Me
		End Function

		''' <summary>获取错误值</summary>
		Public Function GetError() As Object
			If IsPass Then
				Return Nothing
			ElseIf HasException Then
				Return Exception
			ElseIf Notification.NotEmpty Then
				Return Notification
			Else
				Return Messages
			End If
		End Function

		''' <summary>获取内容</summary>
		Public Overrides Function ToString() As String
			If IsPass Then
				Return Nothing
			ElseIf HasException Then
				Return Exception.ToString
			ElseIf Notification.NotEmpty Then
				Return Notification
			Else
				Return Messages.Select(Function(x) $"{x.Key}:{x.Value}").JoinString("；")
			End If
		End Function
	End Class
End Namespace
