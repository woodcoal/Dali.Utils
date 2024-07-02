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
' 	Webhook 通知
'
' 	name: WebhookNotifier
' 	create: 2024-03-04
' 	memo: Webhook 通知
'
' ------------------------------------------------------------

Imports DaLi.Utils.Http

Namespace Misc.Notifier

	''' <summary>Webhook 通知</summary>
	Public Class WebhookNotifier
		Implements INotifier

		''' <summary>WebHook 服务器地址</summary>
		Public ReadOnly Property Address As String

		''' <summary>构造</summary>
		Public Sub New(parameters As KeyValueDictionary)
			Address = parameters?.GetValue("Address")
		End Sub

		''' <summary>构造</summary>
		Public Sub New(address As String)
			Me.Address = address
		End Sub

		''' <summary>发送消息</summary>
		''' <param name="message">发送的消息</param>
		''' <param name="receiver">接收人，由于不同平台协议不一致，此参数暂时无效</param>
		''' <param name="exts">扩展消息、参数</param>
		''' <param name="errorMessage">错误信息</param>
		Public Function Send(message As String, receiver As String, ByRef Optional errorMessage As String = "", Optional exts As KeyValueDictionary = Nothing) As Boolean Implements INotifier.Send
			' 验证参数
			If Not Address.IsUrl Then
				errorMessage = "Webhook 地址错误"
				Return False
			End If

			If message.IsEmpty Then
				errorMessage = "请设置发送消息的内容"
				Return False
			End If

			' 发送内容
			Dim Content = New KeyValueDictionary

			' 是否 Markdown
			Dim IsMarkdown = exts?.GetValue("Markdown", False)
			If IsMarkdown.HasValue AndAlso IsMarkdown.Value Then
				' Markdown
				Content.Add("msgtype", "markdown")
				Content.Add("markdown", New NameValueDictionary From {{"content", message}})
			Else
				' Text
				Content.Add("msgtype", "text")
				Content.Add("text", New NameValueDictionary From {{"content", message}})
			End If

			Dim Api As New ApiClient
			errorMessage = Api.Execute(Http.Model.HttpMethodEnum.POST, Address, Content.ToJson)

			If Api.StatusCode = Net.HttpStatusCode.OK Then
				Return True
			Else
				Return False
			End If
		End Function

		''' <summary>自定义格式发送消息</summary>
		''' <param name="message">发送的消息，注意平台之间的区别，JSON 格式内容</param>
		''' <param name="errorMessage">错误信息</param>
		Public Function Send(message As String, ByRef Optional errorMessage As String = "") As Boolean
			' 验证参数
			If Not Address.IsUrl Then
				errorMessage = "Webhook 地址错误"
				Return False
			End If

			If message.IsEmpty Then
				errorMessage = "请设置发送消息的内容"
				Return False
			End If

			' 发送内容
			Dim Api As New ApiClient
			errorMessage = Api.Execute(Http.Model.HttpMethodEnum.POST, Address, message)

			If Api.StatusCode = Net.HttpStatusCode.OK Then
				Return True
			Else
				Return False
			End If
		End Function
	End Class

End Namespace