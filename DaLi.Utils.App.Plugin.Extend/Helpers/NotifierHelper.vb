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
' 	通知操作
'
' 	name: NotifierHelper
' 	create: 2024-03-05
' 	memo: 通知操作
'
' ------------------------------------------------------------

Imports DaLi.Utils.Misc.Notifier
Imports Microsoft.Extensions.Caching.Distributed

Namespace Helper
	''' <summary>通知操作</summary>
	Public NotInheritable Class NotifierHelper

		''' <summary>短信通知接口</summary>
		Private Shared _SMS As SMSNotifier

		''' <summary>WebHook 通知接口</summary>
		Private Shared _WebHook As WebhookNotifier

		''' <summary>邮箱通知接口</summary>
		Private Shared _Email As EmailNotifier

		''' <summary>短信通知接口</summary>
		Public Shared Property SMS As SMSNotifier
			Get
				If _SMS Is Nothing Then
					Dim setting = SYS.GetSetting(Of INotifierSetting)
					_SMS = New SMSNotifier(setting.SMS_Account, setting.SMS_Password, setting.SMS_Address)
				End If

				Return _SMS
			End Get
			Set(value As SMSNotifier)
				_SMS = value
			End Set
		End Property

		''' <summary>WebHook 通知接口</summary>
		Public Shared Property WebHook As WebhookNotifier
			Get
				If _WebHook Is Nothing Then
					Dim setting = SYS.GetSetting(Of INotifierSetting)
					_WebHook = New WebhookNotifier(setting.Webhook_Address)
				End If

				Return _WebHook
			End Get
			Set(value As WebhookNotifier)
				_WebHook = value
			End Set
		End Property

		''' <summary>邮箱通知接口</summary>
		Public Shared Property Email As EmailNotifier
			Get
				If _Email Is Nothing Then
					Dim setting = SYS.GetSetting(Of INotifierSetting)
					_Email = New EmailNotifier(setting.Email_Host, setting.Email_Account, setting.Email_Password, setting.Email_Name, setting.Email_Port, setting.Email_SSL)
				End If

				Return _Email
			End Get
			Set(value As EmailNotifier)
				_Email = value
			End Set
		End Property

		''' <summary>发送消息</summary>
		''' <param name="notifier">信息发送接口</param>
		''' <param name="message">发送的消息</param>
		''' <param name="receiver">接收人</param>
		''' <param name="exts">扩展消息</param>
		''' <param name="errorMessage">错误信息</param>
		Public Shared Function Send(notifier As NotifierInterfaceEnum, message As String, receiver As String, ByRef Optional errorMessage As String = "", Optional exts As KeyValueDictionary = Nothing) As Boolean
			Select Case notifier
				Case NotifierInterfaceEnum.EMAIL
					Return Email.Send(message, receiver, errorMessage, exts)

				Case NotifierInterfaceEnum.SMS
					Return SMS.Send(message, receiver, errorMessage, exts)

				Case Else
					Return WebHook.Send(message, receiver, errorMessage, exts)
			End Select
		End Function

		''' <summary>生成随机验证码</summary>
		Public Shared Function MakeCaptchaCode() As String
			Dim setting = SYS.GetSetting(Of INotifierSetting)

			Select Case setting.Captcha_Mode
				Case CaptchaModeEnum.LETTER
					Return RandomHelper.Chars(setting.Captcha_Length)

				Case CaptchaModeEnum.MIX
					Return RandomHelper.Mix(setting.Captcha_Length)

				Case CaptchaModeEnum.CHINESE
					Return RandomHelper.ChineseWords(setting.Captcha_Length)

				Case CaptchaModeEnum.GUID
					Return RandomHelper.Guid

				Case Else
					Return RandomHelper.Number(setting.Captcha_Length)
			End Select
		End Function

		''' <summary>验证码</summary>
		Private Class Captcha

			''' <summary>缓存</summary>
			Private _Cache As IDistributedCache

			''' <summary>缓存</summary>
			Private ReadOnly Property Cache As IDistributedCache
				Get
					If _Cache Is Nothing Then _Cache = SYS.GetService(Of IDistributedCache)
					Return _Cache
				End Get
			End Property

			''' <summary>创建缓存键</summary>
			Private Shared Function MakeKey(user As String) As String
				Return $"DALI.Notifier.Captcha.{user}"
			End Function

			''' <summary>生成随机验证码</summary>
			Public Shared Function MakeCaptchaCode() As String
				Dim setting = SYS.GetSetting(Of INotifierSetting)

				Select Case setting.Captcha_Mode
					Case CaptchaModeEnum.LETTER
						Return RandomHelper.Chars(setting.Captcha_Length)

					Case CaptchaModeEnum.MIX
						Return RandomHelper.Mix(setting.Captcha_Length)

					Case CaptchaModeEnum.CHINESE
						Return RandomHelper.ChineseWords(setting.Captcha_Length)

					Case CaptchaModeEnum.GUID
						Return RandomHelper.Guid

					Case Else
						Return RandomHelper.Number(setting.Captcha_Length)
				End Select
			End Function

			''' <summary>发送验证码</summary>
			''' <param name="user">用户</param>
			''' <param name="notifier">接口</param>
			''' <param name="ip">IP</param>
			''' <param name="errorMessage">错误反馈消息</param>
			''' <remarks>
			''' 关于发送次数限制与超时的说明
			''' 正常验证码发送情况下：
			''' 1. 申请产生验证码
			''' 2. 发送验证码
			''' 3. 记录验证码到缓存，有效时长 setting.Captcha_Timeout 分钟
			''' 
			''' 用户未收到验证码重试：
			''' 1. 获取缓存验证码信息
			''' 2. 不存在按正常生成验证码流程处理
			''' 3. 存在则先检测重试次数是否超出限制，超过限制则暂停 setting.Captcha_Count_Delay 分钟后才能继续重试，此次操作结束
			''' 4. 未超出次数，检查两次重发时长，如果未达到 setting.Captcha_Delay 分钟则暂停此次发送，此次操作结束
			''' 5. 超过两次重发时长，按正常发送流程处理，记录发送次数
			''' 6. 如果发送次数超出限制，将缓存时长设置为 setting.Captcha_Count_Delay 分钟，防止此时段内再次重发
			''' 7. 如果发送次数未超出限制，则整行缓存 setting.Captcha_Timeout 分钟
			''' 
			''' </remarks>
			Public Function Send(notifier As NotifierInterfaceEnum, user As String, Optional ip As String = "", Optional ByRef errorMessage As String = "") As Boolean
				errorMessage = ""

				' 用户检查
				Select Case notifier
					Case NotifierInterfaceEnum.SMS
						If Not user.IsMobilePhone Then errorMessage = "验证码接收必须为手机号"

					Case NotifierInterfaceEnum.EMAIL
						If Not user.IsEmail Then errorMessage = "验证码接收必须为邮箱"

				End Select

				If errorMessage.NotEmpty Then Return False

				' 参数
				Dim setting = SYS.GetSetting(Of INotifierSetting)

				' 检查上次发送时间
				Dim cacheKey = MakeKey(user)
				Dim cacheValue = Cache.Read(Of CaptcheValue)(cacheKey)

				' 存在数据，表示信息已经发送过，当前为重发
				If cacheValue IsNot Nothing Then
					' 超过最大错误次数
					If cacheValue.Count >= setting.Captcha_Count Then
						Dim time = Math.Ceiling(cacheValue.Last.AddMinutes(setting.Captcha_Count_Delay).Subtract(SYS_NOW_DATE).TotalMinutes)
						If time > 0 Then
							errorMessage = $"验证码发送过于频繁，请 {time} 分钟后再试"
							Return False
						End If
					End If

					' 检测发送频率
					Dim last = cacheValue.Last.AddMinutes(setting.Captcha_Delay)
					If last > SYS_NOW Then
						Dim time = Math.Ceiling(last.Subtract(SYS_NOW_DATE).TotalMinutes)
						errorMessage = $"验证码正在处理中，请 {time} 分钟后尝试重新发送"
						Return False
					End If
				End If


				' 原始缓存不存在，新建缓存数据
				If cacheValue Is Nothing Then cacheValue = New CaptcheValue

				' 生成验证码
				cacheValue.Code = MakeCaptchaCode()
				cacheValue.Last = SYS_NOW_DATE
				cacheValue.IP = ip
				cacheValue.Count += 1

				' 缓存时长
				Dim cacheTime = If(cacheValue.Count >= setting.Captcha_Count, setting.Captcha_Count_Delay, setting.Captcha_Timeout)

				' 缓存数据
				Cache.Save(cacheKey, cacheValue, cacheTime * 60, False)

				' 生成内容
				Dim Nvs As New KeyValueDictionary From {
					{"code", cacheValue.Code},
					{"user", user},
					{"time", SYS_NOW_DATE},
					{"timeout", setting.Captcha_Timeout}
				}

				Dim content = setting.Captcha_Template_Webhook
				Select Case notifier
					Case NotifierInterfaceEnum.SMS
						content = setting.Captcha_Template_SMS

					Case NotifierInterfaceEnum.EMAIL
						content = setting.Captcha_Template_Email

				End Select
				content = content.EmptyValue("你当前的验证码为：{code}，有效期为 {timeout} 分钟，请勿泄露。").FormatTemplate(Nvs)

				' 发送
				Return NotifierHelper.Send(notifier, content, user, errorMessage)
			End Function

			''' <summary>验证码检查</summary>
			''' <param name="code">用户填写的验证码</param>
			''' <param name="user">用户</param>
			''' <param name="notifier">接口</param>
			''' <param name="ip">IP</param>
			Public Function Validate(notifier As NotifierInterfaceEnum, user As String, code As String, Optional ip As String = "", Optional ByRef errorMessage As String = "") As Boolean
				errorMessage = ""

				' 用户检查
				Select Case notifier
					Case NotifierInterfaceEnum.SMS
						If Not user.IsMobilePhone Then errorMessage = "验证码接收必须为手机号"

					Case NotifierInterfaceEnum.EMAIL
						If Not user.IsEmail Then errorMessage = "验证码接收必须为邮箱"

				End Select

				If errorMessage.NotEmpty Then Return False

				' 检查验证码
				Dim cacheKey = MakeKey(user)
				Dim cacheValue = Cache.Read(Of CaptcheValue)(cacheKey)
				If cacheValue Is Nothing OrElse cacheValue.Code <> code OrElse cacheValue.IP <> ip Then
					errorMessage = "验证码错误或者已经失效"
					Return False
				End If

				' 移除缓存
				Cache.Remove(cacheKey)
				Return True
			End Function
		End Class

		''' <summary>发送验证码</summary>
		''' <param name="user">用户</param>
		''' <param name="notifier">接口</param>
		''' <param name="ip">IP</param>
		''' <param name="errorMessage">错误反馈消息</param>
		Public Shared Function Captcha_Send(notifier As NotifierInterfaceEnum, user As String, Optional ip As String = "", Optional ByRef errorMessage As String = "") As Boolean
			Return New Captcha().Send(notifier, user, ip, errorMessage)
		End Function

		''' <summary>验证码检查</summary>
		''' <param name="code">用户填写的验证码</param>
		''' <param name="user">用户</param>
		''' <param name="notifier">接口</param>
		''' <param name="ip">IP</param>
		Public Shared Function Captcha_Validate(notifier As NotifierInterfaceEnum, user As String, code As String, Optional ip As String = "", Optional ByRef errorMessage As String = "") As Boolean
			Return New Captcha().Validate(notifier, user, code, ip, errorMessage)
		End Function

	End Class
End Namespace