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
' 	邮件通知接口
'
' 	name: EmailNotifier
' 	create: 2024-03-04
' 	memo: 邮件通知接口
'
' ------------------------------------------------------------

Imports System.Net
Imports System.Net.Mail
Imports System.Text

Namespace Misc.Notifier
	''' <summary>邮件通知接口</summary>
	Public Class EmailNotifier
		Implements INotifier

		''' <summary>服务器地址</summary>
		Public Property Host As String

		''' <summary>端口</summary>
		Public Property Port As Integer = 25

		''' <summary>是否开启 SSL</summary>
		Public Property SSL As Boolean = False

		''' <summary>发件人账号</summary>
		Public Property Account As String

		''' <summary>发件人密码</summary>
		Public Property Password As String

		''' <summary>发件人名称</summary>
		Public Property Name As String

		''' <summary>构造</summary>
		Public Sub New(parameters As KeyValueDictionary)
			If parameters Is Nothing Then Return

			Host = parameters.GetValue("Host")
			Port = parameters.GetValue("Port", 25).Range(1, 65536)
			SSL = parameters.GetValue("SSL", False)
			Account = parameters.GetValue("Account")
			Password = parameters.GetValue("Password")
			Name = parameters.GetValue("Name").EmptyValue(Account)
		End Sub

		''' <summary>构造</summary>
		Public Sub New(host As String, account As String, password As String, Optional name As String = "", Optional port As Integer = 25, Optional ssl As Boolean = False)
			Me.Host = host
			Me.Port = port.Range(1, 65536)
			Me.SSL = ssl
			Me.Account = account
			Me.Password = password
			Me.Name = name.EmptyValue(account)
		End Sub

		''' <summary>发送消息</summary>
		''' <param name="message">发送的消息</param>
		''' <param name="receiver">接收人</param>
		''' <param name="exts">扩展消息</param>
		''' <param name="errorMessage">错误信息</param>
		Public Function Send(message As String, receiver As String, ByRef Optional errorMessage As String = "", Optional exts As KeyValueDictionary = Nothing) As Boolean Implements INotifier.Send
			' 验证参数
			Dim msg As New List(Of String)
			If Host.IsEmpty Then msg.Add("邮件服务器未设置")
			If Not Account.IsEmail Then msg.Add("无效邮箱账号，需要邮箱格式")
			If Password.IsEmpty Then msg.Add("发送邮箱密码为设置")
			If message.IsEmpty Then msg.Add("请设置发送消息的内容")
			If Not receiver.IsEmail Then msg.Add("无效邮箱账号，需要邮箱格式")
			If msg.NotEmpty Then
				errorMessage = msg.JoinString("；")
				Return False
			End If

			' 执行操作
			Using client As New SmtpClient(Host, Port)
				client.EnableSsl = SSL

				' 发送邮件
				Try
					Using email As New MailMessage()
						email.From = New MailAddress(Account, Name, Encoding.UTF8)

						' 处理内容
						Dim title = ""
						Dim body = message

						' 尝试使用 JSON 分析
						Dim Nvs = NameValueDictionary.FromJson(message)
						If Nvs.NotEmpty Then
							title = Nvs("title").EmptyValue(Nvs("subject"))
							body = Nvs("body").EmptyValue(Nvs("content"))
						Else
							Dim bodyClear = body.ClearHtml("all").Trim.ClearSpace

							Dim idx = bodyClear.IndexOf(vbLf)
							If idx > 1 AndAlso idx < 51 Then
								' 存在分行，且第一行文字少于 50 字则用于标题
								title = bodyClear.Substring(0, idx)
							Else
								' 自动取 50 字概要
								title = bodyClear.ShortShow(50)
							End If
						End If

						email.Subject = title
						email.SubjectEncoding = Encoding.UTF8
						email.Body = body
						email.BodyEncoding = Encoding.UTF8
						email.IsBodyHtml = True

						' 收件人
						email.To.Add(receiver)

						client.Credentials = New NetworkCredential(Account, Password)
						client.Send(email)

						email.Dispose()
					End Using
				Catch ex As Exception
					errorMessage = ex.Message
				End Try
			End Using

			Return errorMessage.IsEmpty
		End Function

	End Class

End Namespace