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
' 	字符串中简易标签
'
' 	name: Tag.SimpleTag
' 	create: 2020-04-18
' 	memo: 字符串中简易标签，{}
'		  本标签仅支持 {} 模式，且批量替换由 NameValueDictionar 改成了 IDictionary(of String, Object)
'		  仅本标签进行了调整，其他块与静态标签未做修改！！！
' 	
' ------------------------------------------------------------

Namespace Template.Tag

	''' <summary>字符串中简易标签，{}</summary>
	<Obsolete("请使用 SimpleTemplate 替换")>
	Public Class SimpleTag
		Inherits TagBase

		''' <summary>去掉前缀</summary>
		Private Shadows Const PREFIX = ""

		''' <summary>值标签类型，类似 {ff:aa.bb} 中的 bb；其中 aa 用 Name 表示</summary>
		Public ReadOnly Type As String

		Public Sub New(node As String)
			' 原始内容
			Raw = node
			Name = ""

			' 检查是否有效标签
			If node.IsEmpty Then Exit Sub

			Dim Len = PREFIX.Length + 1
			If node.Like($"{{{PREFIX}*}}") Then
				node = node.Substring(Len, node.Length - Len - 1).Trim
			Else
				Return
			End If

			' 非有效标签，退出检测
			If node.IsEmpty Then Exit Sub

			' 符合条件的标签
			Dim Ps = node.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
			If Ps?.Length > 0 Then
				' 分析到实际有效的内容
				Name = Ps(0).Trim.ToLower

				' 分析属性
				GetAttributes(node)

				' 对于标签名称包含.的，且不含type属性的，需要将标签转换为type属性。如：<a.b /> => <a type="b" />
				Type = Attributes("type")
				If Type.IsEmpty AndAlso Name.Contains("."c) Then
					Dim Ns = Name.Split({"."c}, StringSplitOptions.RemoveEmptyEntries)
					If Ns?.Length > 1 Then
						Type = Ns(1)
						Name = Ns(0)
					End If
				End If

				If Type.NotEmpty Then Type = Type.ToLower
			End If
		End Sub

		''' <summary>获取标签列表</summary>
		''' <param name="source">原始内容</param>
		''' <param name="tagName">默认标签</param>
		Public Shared Function GetTags(source As String, Optional tagName As String = "") As List(Of SimpleTag)
			' 无内容直接返回
			If source.IsEmpty Then Return Nothing

			' 检查是否有标签
			Dim Tags As String() = source.Cut($"{{{PREFIX}{tagName}", "}", True, True)
			If Tags.IsEmpty Then Return Nothing

			' 结果列表
			Dim Ret As New List(Of SimpleTag)

			' 默认标签存在则使用小写
			If tagName.NotEmpty Then tagName = tagName.ToLower

			' 获取标签
			For Each Item In Tags.Distinct
				Dim Tag As New SimpleTag(Item)
				If Tag IsNot Nothing AndAlso Tag.Name.NotEmpty Then
					If tagName.IsEmpty OrElse Tag.Name = tagName Then Ret.Add(Tag)
				End If
			Next

			Return Ret
		End Function

		''' <summary>替换模板数据</summary>
		''' <param name="template">模板数据</param>
		''' <param name="key">标签名称</param>
		''' <param name="value">替换值</param>
		''' <param name="tags">已经获取过的标签列表</param>
		''' <param name="replaceFunc">其他标签替换处理函数</param>
		Public Overloads Shared Function Replace(
												template As String,
												key As String,
												value As Object,
												Optional tags As List(Of SimpleTag) = Nothing,
												Optional replaceFunc As Func(Of NameValueDictionary, String, String
												） = Nothing) As String
			If template.IsEmpty OrElse key.IsEmpty Then Return template

			' 分离 key / type
			Dim tagName = key
			Dim tagType = ""

			If key.Contains("."c) Then
				Dim keys = key.Split("."c, StringSplitOptions.RemoveEmptyEntries)
				If keys.NotEmpty AndAlso keys.Length > 1 Then
					tagName = keys(0)
					tagType = keys(1)
				End If
			End If

			' 标签列表
			If tags Is Nothing Then
				tags = GetTags(template, tagName)
			Else
				tags = tags.Where(Function(x) x.Name.IsSame(tagName)).ToList
			End If
			If tags.IsEmpty Then Return template

			' 分析类型
			tags = tags.Where(Function(x) x.Type.IsSame(tagType)).ToList
			If tags.IsEmpty Then Return template

			For Each t In tags
				template = t.Replace(template, value?.ToString, replaceFunc)
			Next

			Return template
		End Function

		''' <summary>替换模板数据</summary>
		''' <param name="values">需要替换的标签名与值的列表</param>
		''' <param name="tags">已经获取过的标签列表</param>
		''' <param name="replaceFunc">其他标签替换处理函数</param>
		Public Overloads Shared Function Replace(
												template As String,
												values As IDictionary(Of String, Object),
												Optional tags As List(Of SimpleTag) = Nothing,
												Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing
												) As String
			If template.IsEmpty OrElse values.IsEmpty Then Return template

			' 标签列表
			If tags Is Nothing Then tags = GetTags(template)
			If tags.IsEmpty Then Return template


			' 存在标签进行替换
			For Each kv In values.ToList
				template = Replace(template, kv.Key, kv.Value, tags, replaceFunc)
			Next

			Return template
		End Function

		''' <summary>替换指定标签类型数据</summary>
		''' <param name="template">模板</param>
		''' <param name="key">标签名</param>
		''' <param name="values">值列表，根据 type 替换</param>
		''' <param name="tags">已经获取到的标签，不设置则自动根据 key 来分析</param>
		''' <param name="replaceFunc">其他替换函数</param>
		''' <param name="replaceALL">是否替换所有key标签，如果type不在Values列表中也用空白替换，否则不替换不存在标签</param>
		Public Overloads Shared Function Replace(
												template As String,
												key As String,
												values As IDictionary(Of String, Object),
												Optional tags As List(Of SimpleTag) = Nothing,
												Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing,
												Optional replaceALL As Boolean = False
												) As String
			If template.IsEmpty OrElse key.IsEmpty OrElse values.IsEmpty Then Return template

			' 获取标签列表
			tags = If(tags.IsEmpty, GetTags(template, key), tags.Where(Function(x) x.Name.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList)
			If tags.IsEmpty Then Return template

			' 替换属性
			For Each T In tags
				If replaceALL OrElse values.ContainsKey(T.Type) Then
					template = T.Replace(template, values(T.Type), replaceFunc)
				End If
			Next

			Return template
		End Function
	End Class

End Namespace