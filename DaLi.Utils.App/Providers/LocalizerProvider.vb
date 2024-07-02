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
' 	本地化语言
'
' 	name: Provider.LocalizerProvider
' 	create: 2023-02-14
' 	memo: 本地化语言
'
' ------------------------------------------------------------

Imports Microsoft.Extensions.Localization

Namespace Provider

	''' <summary>本地化语言</summary>
	Public Class LocalizerProvider
		Implements ILocalizerProvider

		''' <summary>语言接口</summary>
		Private ReadOnly _Instance As New List(Of IStringLocalizer)

		''' <summary>设置语言接口</summary>
		Public Sub SetLocalizer(instance As IStringLocalizer) Implements ILocalizerProvider.SetLocalizer
			If instance IsNot Nothing AndAlso Not _Instance.Contains(instance) Then _Instance.Add(instance)
		End Sub

		''' <summary>通过指定前缀获取项目后转换翻译</summary>
		Public ReadOnly Property TranslateWithPrefix(name As String, prefix As String) As String Implements ILocalizerProvider.TranslateWithPrefix
			Get
				If name.IsEmpty Then Return ""
				If prefix.IsEmpty Then Return Translate(name)

				Dim prefixName = prefix & name
				Dim convertValue = Translate(prefixName)

				If convertValue = prefixName Then
					Return Translate(name)
				Else
					Return convertValue
				End If
			End Get
		End Property

		''' <summary>翻译</summary>
		Default Public ReadOnly Property Translate(name As String) As String Implements ILocalizerProvider.Translate
			Get
				If name.IsEmpty Then Return ""
				If _Instance.IsEmpty Then Return name

				For Each ins In _Instance
					Dim ret = ins(name)
					If Not ret.ResourceNotFound Then Return ret
				Next

				Return name
			End Get
		End Property

		''' <summary>翻译</summary>
		Default Public ReadOnly Property Translate(name As String, ParamArray args() As Object) As String Implements ILocalizerProvider.Translate
			Get
				If name.IsEmpty Then Return ""
				If _Instance.IsEmpty Then Return String.Format(name, args)

				For Each ins In _Instance
					Dim ret = ins(name, args)
					If Not ret.ResourceNotFound Then Return ret
				Next

				Return String.Format(name, args)
			End Get
		End Property

	End Class

End Namespace