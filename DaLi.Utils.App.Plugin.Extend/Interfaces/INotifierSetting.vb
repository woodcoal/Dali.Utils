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
' 	通知设置接口
'
' 	name: INotifierSetting
' 	create: 2024-03-04
' 	memo: 通知设置接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>通知设置接口</summary>
	Public Interface INotifierSetting
		Inherits ISetting

#Region "邮箱参数"

		''' <summary>服务器地址</summary>
		Property Email_Host As String

		''' <summary>端口</summary>
		Property Email_Port As Integer

		''' <summary>是否开启 SSL</summary>
		Property Email_SSL As Boolean

		''' <summary>发件人账号</summary>
		Property Email_Account As String

		''' <summary>发件人密码</summary>
		Property Email_Password As String

		''' <summary>发件人名称</summary>
		Property Email_Name As String

#End Region

#Region "短信参数"

		''' <summary>短信平台账号</summary>
		Property SMS_Account As String

		''' <summary>短信平台密码</summary>
		Property SMS_Password As String

		''' <summary>短信平台地址。设置为网址则使用远程 API 方式访问，注意使用的是 v2 接口</summary>
		Property SMS_Address As String

#End Region

#Region "Webhook 参数"

		''' <summary>平台服务器地址</summary>
		Property Webhook_Address As String

#End Region

#Region "验证码参数"

		''' <summary>验证码长度</summary>
		Property Captcha_Length As Integer


		''' <summary>验证码超时（分钟）</summary>
		Property Captcha_Timeout As Integer


		''' <summary>验证码重新生成，发送间隔时间（分钟）</summary>
		Property Captcha_Delay As Integer

		''' <summary>验证码次数，超过此次数将暂时停止此验证码发送，直到间隔达到下次允许发送时长</summary>
		Property Captcha_Count As Integer


		''' <summary>当验证码发送达到指定最大次数时，将暂时停止此验证码发送时长（分钟）</summary>
		Property Captcha_Count_Delay As Integer

		''' <summary>验证码重新生成，发送间隔时间（分钟）</summary>
		Property Captcha_Mode As CaptchaModeEnum

		''' <summary>短信验证码内容模板</summary>
		Property Captcha_Template_SMS As String


		''' <summary>邮件验证码内容模板</summary>
		Property Captcha_Template_Email As String


		''' <summary>Webhook 验证码内容模板</summary>
		Property Captcha_Template_Webhook As String

#End Region

	End Interface
End Namespace
