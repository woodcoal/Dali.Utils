' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' ------------------------------------------------------------
'
' 	验证码模式
'
' 	name: CaptchaMode
' 	create: 2024-03-03
' 	memo: 验证码模式
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>验证码模式</summary>
	Public Enum CaptchaModeEnum

		''' <summary>纯数字</summary>
		<Description("纯数字")>
		NUMBER = 0

		''' <summary>字母</summary>
		<Description("字母")>
		LETTER = 1

		''' <summary>混合，字母数字</summary>
		<Description("混合，字母数字")>
		MIX = 2

		''' <summary>中文</summary>
		<Description("中文")>
		CHINESE = 3

		''' <summary>GUID</summary>
		<Description("GUID")>
		GUID = 4

	End Enum

End Namespace
