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
' 	通知接口参数设置
'
' 	name: NotifierSetting
' 	create: 2024-03-05
' 	memo: 通知接口参数设置
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations

Namespace Setting

	''' <summary>通知接口参数设置</summary>
	Public Class NotifierSetting
		Inherits DbSettingBase(Of NotifierSetting)
		Implements INotifierSetting

#Region "邮箱参数"

		''' <summary>服务器地址</summary>
		<Description("服务器地址")>
		Public Property Email_Host As String Implements INotifierSetting.Email_Host

		''' <summary>端口</summary>
		<Description("邮件服务器端口")>
		<Range(1, 65536)>
		Public Property Email_Port As Integer = 25 Implements INotifierSetting.Email_Port

		''' <summary>是否开启 SSL</summary>
		<Description("是否开启 SSL")>
		Public Property Email_SSL As Boolean = False Implements INotifierSetting.Email_SSL

		''' <summary>发件人账号</summary>
		<Description("发件人账号")>
		<FieldType(FieldValidateEnum.EMAIL)>
		Public Property Email_Account As String Implements INotifierSetting.Email_Account

		''' <summary>发件人密码</summary>
		<Description("发件人密码")>
		<FieldEncode>
		Public Property Email_Password As String Implements INotifierSetting.Email_Password

		''' <summary>发件人名称</summary>
		<Description("发件人名称")>
		Public Property Email_Name As String Implements INotifierSetting.Email_Name

#End Region

#Region "短信参数"

		''' <summary>短信平台账号</summary>
		<Description("短信平台账号")>
		<FieldType(FieldValidateEnum.USERNAME_ENDOT)>
		Public Property SMS_Account As String Implements INotifierSetting.SMS_Account

		''' <summary>短信平台密码</summary>
		<Description("短信平台密码")>
		<FieldType(FieldValidateEnum.GUID)>
		<FieldEncode>
		Public Property SMS_Password As String Implements INotifierSetting.SMS_Password

		''' <summary>短信平台地址。设置为网址则使用远程 API 方式访问，注意使用的是 v2 接口</summary>
		<Description("短信平台地址。设置为网址则使用远程 API 方式访问，注意使用的是 v2 接口")>
		<FieldType(FieldValidateEnum.URL)>
		Public Property SMS_Address As String Implements INotifierSetting.SMS_Address

#End Region

#Region "Webhook 参数"

		''' <summary>平台服务器地址</summary>
		<Description("平台服务器地址")>
		<FieldType(FieldValidateEnum.URL)>
		Public Property Webhook_Address As String Implements INotifierSetting.Webhook_Address

#End Region

#Region "验证码参数"

		''' <summary>验证码长度，类型为 GUID 是固定 36 位长度</summary>
		<Range(4, 64)>
		<Description("验证码长度")>
		Public Property Captcha_Length As Integer = 6 Implements INotifierSetting.Captcha_Length

		''' <summary>验证码有效时长（分钟）</summary>
		<Description("验证码有效时长（分钟）")>
		<Range(1, 1440)>
		Public Property Captcha_Timeout As Integer = 5 Implements INotifierSetting.Captcha_Timeout

		''' <summary>验证码重新生成、发送间隔时间（分钟）</summary>
		<Description("验证码重新生成、发送间隔时间（分钟）")>
		<Range(1, 1440)>
		Public Property Captcha_Delay As Integer = 1 Implements INotifierSetting.Captcha_Delay

		''' <summary>验证码次数超过此次数将暂时停止此验证码发送，直到间隔达到下次允许发送时长</summary>
		<Description("验证码次数超过此次数将暂时停止此验证码发送，直到间隔达到下次允许发送时长")>
		<Range(1, 9999999999)>
		Public Property Captcha_Count As Integer = 5 Implements INotifierSetting.Captcha_Count

		''' <summary>当验证码发送达到指定最大次数时，将暂时停止此验证码发送时长（分钟）,必须大于重发间隔时长</summary>
		<Description("当验证码发送达到指定最大次数时，将暂时停止此验证码发送时长（分钟）,必须大于重发间隔时长")>
		<Range(1, 1440)>
		Public Property Captcha_Count_Delay As Integer = 360 Implements INotifierSetting.Captcha_Count_Delay

		''' <summary>验证码类型</summary>
		<Description("验证码类型（0：纯数组，1：纯字母，2：字母数字组合，3：中文，4：GUID）")>
		<Range(0, 4)>
		Public Property Captcha_Mode As CaptchaModeEnum = CaptchaModeEnum.NUMBER Implements INotifierSetting.Captcha_Mode

		''' <summary>短信验证码内容模板，支持标签：{code} {user} {time} {timeout}</summary>
		<Description("短信验证码内容模板，支持标签（{code}验证码，{user}接收号码，{time}发送时间，{timeout}验证码有效期）")>
		Public Property Captcha_Template_SMS As String Implements INotifierSetting.Captcha_Template_SMS

		''' <summary>邮件验证码内容模板，支持标签：{code} {user} {time} {timeout}</summary>
		<Description("邮件验证码内容模板，支持标签（{code}验证码，{user}接收号码，{time}发送时间，{timeout}验证码有效期），第一行为邮件标题，第二行为正文")>
		Public Property Captcha_Template_Email As String Implements INotifierSetting.Captcha_Template_Email

		''' <summary>Webhook 验证码内容模板，支持标签：{code} {user} {time} {timeout}</summary>
		<Description("Webhook 验证码内容模板，支持标签（{code}验证码，{user}接收号码，{time}发送时间，{timeout}验证码有效期）")>
		Public Property Captcha_Template_Webhook As String Implements INotifierSetting.Captcha_Template_Webhook

#End Region

		Protected Overrides Sub Initialize(provider As ISettingProvider)
			' 清空通知器，以便重新赋值参数
			NotifierHelper.SMS = Nothing
			NotifierHelper.Email = Nothing
			NotifierHelper.WebHook = Nothing

			If Captcha_Mode = CaptchaModeEnum.GUID Then Captcha_Length = 36
			If Captcha_Delay > Captcha_Timeout Then Captcha_Delay = Captcha_Timeout
			If Captcha_Count_Delay < Captcha_Delay Then Captcha_Count_Delay = Captcha_Delay
		End Sub

	End Class

End Namespace