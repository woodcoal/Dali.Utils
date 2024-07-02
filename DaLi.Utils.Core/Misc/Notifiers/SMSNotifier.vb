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
' 	短信通知
'
' 	name: SMSNotifier
' 	create: 2024-03-04
' 	memo: 短信通知
'
' ------------------------------------------------------------

Imports DaLi.Utils.Http

Namespace Misc.Notifier

	''' <summary>短信通知</summary>
	Public Class SMSNotifier
		Implements INotifier

		''' <summary>短信平台账号</summary>
		Public Property Account As String

		''' <summary>短信平台密码</summary>
		Public Property Password As String

		''' <summary>短信平台地址。设置为网址则使用远程 API 方式访问，注意使用的是 v2 接口</summary>
		Public Property Address As String

		''' <summary>构造</summary>
		Public Sub New(parameters As KeyValueDictionary)
			If parameters Is Nothing Then Return

			Account = parameters.GetValue("Account")
			Password = parameters.GetValue("Password")
			Address = parameters.GetValue("Address")
		End Sub

		''' <summary>构造</summary>
		Public Sub New(account As String, password As String, address As String)
			Me.Account = account
			Me.Password = password
			Me.Address = address
		End Sub

		''' <summary>发送消息</summary>
		''' <param name="message">发送的消息</param>
		''' <param name="receiver">接收人</param>
		''' <param name="exts">扩展消息、参数</param>
		''' <param name="errorMessage">错误信息</param>
		Public Function Send(message As String, receiver As String, ByRef Optional errorMessage As String = "", Optional exts As KeyValueDictionary = Nothing) As Boolean Implements INotifier.Send
			' 验证参数
			Dim msg As New List(Of String)
			If Not Address.IsUrl Then msg.Add("短信平台地址无效")
			If Account.IsEmpty Then msg.Add("短信发送账号必须设置")
			If Password.IsEmpty Then msg.Add("短信发送密码必须设置")
			If message.IsEmpty Then msg.Add("请设置发送消息的内容")
			If Not receiver.IsPhone Then msg.Add("接收号码非有效的手机号码")
			If msg.NotEmpty Then
				errorMessage = msg.JoinString("；")
				Return False
			End If

			' API 方式发送
			Dim Nvs As New NameValueDictionary From {
					{"Account", Account},
					{"Password", Password},
					{"Content", message},
					{"Users", receiver}
				}

			' 添加签名
			Dim Sign = SecurityHelper.Sign.ParameterSortString(Nvs).MD5
			Nvs.Add("Sign", Sign)

			' 发送请求
			Dim Api As New ApiClient
			errorMessage = Api.Execute(Http.Model.HttpMethodEnum.POST, Address, Nvs, True)

			Return Api.StatusCode = Net.HttpStatusCode.OK
		End Function

	End Class

End Namespace