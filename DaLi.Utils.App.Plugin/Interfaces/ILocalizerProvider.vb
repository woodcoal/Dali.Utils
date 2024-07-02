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
' 	多语言翻译接口
'
' 	name: Interface.ILocalizerProvider
' 	create: 2023-02-14
' 	memo: 多语言翻译接口
'
' ------------------------------------------------------------

Imports Microsoft.Extensions.Localization

Namespace [Interface]
	''' <summary>多语言翻译接口</summary>
	Public Interface ILocalizerProvider

		''' <summary>设置语言接口</summary>
		Sub SetLocalizer(instance As IStringLocalizer)

		''' <summary>通过指定前缀获取项目后转换翻译</summary>
		ReadOnly Property TranslateWithPrefix(name As String, prefix As String) As String

		''' <summary>翻译</summary>
		Default ReadOnly Property Translate(name As String) As String

		''' <summary>翻译</summary>
		Default ReadOnly Property Translate(name As String, ParamArray args() As Object) As String

	End Interface
End Namespace
