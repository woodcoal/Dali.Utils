' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	通用操作函数
'
' 	name: Helper.CommonHelper
' 	create: 2023-02-14
' 	memo: 通用操作函数
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports System.Reflection

Namespace Helper
	''' <summary>通用操作函数</summary>
	Public NotInheritable Class CommonHelper

		''' <summary>更新名称</summary>
		''' <param name="name">名称</param>
		''' <param name="prefix">通用前缀</param>
		Public Shared Function UpdateName(name As String, prefix As String) As String
			If name.IsEmpty Then Return Nothing
			If prefix.IsEmpty Then Return name

			If name.EndsWith(prefix, StringComparison.OrdinalIgnoreCase) Then name = name.Substring(0, name.Length - prefix.Length)
			If name.Contains($".{prefix}.", StringComparison.OrdinalIgnoreCase) Then name = name.Replace($".{prefix}.", ".", StringComparison.OrdinalIgnoreCase)
			name = name.EmptyValue(prefix).ToUpper

			Return name
		End Function

		''' <summary>获取所有属性名称与注释列表</summary>
		Public Shared Function GetPropertyTitles(this As Type) As NameValueDictionary
			If this Is Nothing Then Return Nothing

			' 分析字段别名
			Dim ret As New NameValueDictionary
			Dim pros = this.GetAllProperties
			Dim loc = SYS.GetService(Of ILocalizerProvider)

			For Each pro In pros
				Dim fieldName = ""

				Dim attrDisplay = pro.GetCustomAttribute(Of DisplayAttribute)
				If attrDisplay IsNot Nothing Then fieldName = attrDisplay.Name

				If fieldName.IsEmpty Then
					Dim attrDescription = pro.GetCustomAttribute(Of DescriptionAttribute)
					If attrDescription IsNot Nothing Then fieldName = attrDescription.Description
				End If

				If fieldName.IsEmpty Then fieldName = pro.Name

				' 翻译
				fieldName = loc.TranslateWithPrefix(fieldName, "Model.Name.")

				' 检查重名
				Dim idx = 0
				While ret.ContainsValue(fieldName)
					idx += 1
					fieldName = $"{fieldName}({idx})"
				End While

				ret.Add(pro.Name, fieldName)
			Next

			Return ret
		End Function

	End Class
End Namespace