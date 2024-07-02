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
' 	验证码接口
'
' 	name: Interface.ICaptcha
' 	create: 2023-12-12
' 	memo: 验证码接口
'
' ------------------------------------------------------------

Imports System.IO

Namespace [Interface]

	''' <summary>验证码接口</summary>
	Public Interface ICaptcha
		Inherits IBase

		''' <summary>接口唯一标识，当使用多种验证码方式时以便区分</summary>
		ReadOnly Property Name As String

		''' <summary>生成验证码图形与验证码文本</summary>
		Function MakeCaptcha(Optional params As IDictionary(Of String, Object) = Nothing) As (Image As MemoryStream, Code As String)

		''' <summary>验证验证码</summary>
		Function ValidateCaptcha(code As String) As Boolean

	End Interface

End Namespace