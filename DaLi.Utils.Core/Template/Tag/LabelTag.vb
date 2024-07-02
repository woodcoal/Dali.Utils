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
' 	页面值标签
'
' 	name: Tag.LabelTag
' 	create: 2020-04-18
' 	memo: 页面值标签，{ff:} ＜ff: /＞
' 	
' ------------------------------------------------------------

Namespace Template.Tag

	''' <summary>值标签，{ff:} ＜ff: /＞</summary>
	Public Class LabelTag
		Inherits TagBase

		''' <summary>值标签类型，类似 {ff:aa.bb} 中的 bb；其中 aa 用 Name 表示</summary>
		Public ReadOnly Type As String

		Public Sub New(node As String)
			' 原始内容
			Raw = node
			Name = ""

			' 检查是否有效标签
			If node.IsEmpty Then Exit Sub

			' 只允许两种标签，{ff:} <ff: />
			Dim Len = PREFIX.Length + 1
			If node.StartsWith("{" & PREFIX, StringComparison.OrdinalIgnoreCase) AndAlso node.EndsWith("}") Then
				node = node.Substring(Len, node.Length - Len - 1)
			ElseIf node.StartsWith("<" & PREFIX, StringComparison.OrdinalIgnoreCase) AndAlso node.EndsWith("/>") Then
				node = node.Substring(Len, node.Length - Len - 2)
			End If

			' 非有效标签，退出检测
			If node.IsEmpty Then Exit Sub

			' 符合条件的标签
			node = node.Trim
			Dim Ps = node.Split(" "c, StringSplitOptions.RemoveEmptyEntries)
			If Ps?.Length > 0 Then
				' 分析到实际有效的内容
				Name = Ps(0).Trim.ToLower

				' 分析属性
				GetAttributes(node)

				' 对于标签名称包含.的，且不含type属性的，需要将标签转换为type属性。如：<a.b /> => <a type="b" />
				Type = Attributes("type")
				If String.IsNullOrEmpty(Type) AndAlso Name.Contains("."c) Then
					Dim Ns = Name.Split("."c, StringSplitOptions.RemoveEmptyEntries)
					If Ns?.Length > 1 Then
						Type = Ns(1)
						Name = Ns(0)
					End If
				End If
				If Not String.IsNullOrEmpty(Type) Then Type = Type.ToLower
			End If
		End Sub

		''' <summary>获取属性列表</summary>
		Public Shared Function GetTags(html As String, Optional tagName As String = "") As List(Of LabelTag)
			Dim R As New List(Of LabelTag)

			If Not String.IsNullOrWhiteSpace(html) Then
				Dim Tags As New List(Of String)

				Dim defName = PREFIX & tagName

				Dim TagList As String() = html.Cut("{" & defName, "}", True, True)
				If TagList?.Length > 0 Then Tags.AddRange(TagList)

				TagList = html.Cut("<" & defName, ">", True, True)
				If TagList?.Length > 0 Then Tags.AddRange(TagList.Where(Function(x) x.EndsWith("/>")))

				If Tags.Count > 0 Then
					If Not String.IsNullOrEmpty(tagName) Then tagName = tagName.ToLower

					For Each Item In Tags
						Dim Tag As New LabelTag(Item)
						If Tag IsNot Nothing AndAlso Not String.IsNullOrEmpty(Tag.Name) Then
							If String.IsNullOrEmpty(tagName) OrElse Tag.Name = tagName Then R.Add(Tag)
						End If
					Next
				End If

				' 去掉重复内容
				R = R.Distinct.ToList
			End If

			Return R
		End Function

		''' <summary>替换模板数据</summary>
		Public Overloads Shared Function Replace(template As String, key As String, value As String, Optional tags As List(Of LabelTag) = Nothing, Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing) As String
			If template.NotEmpty AndAlso Not String.IsNullOrWhiteSpace(key) Then
				If tags Is Nothing Then tags = GetTags(template)
				If tags?.Count > 0 Then
					key = key.ToLower

					Dim Ts As IEnumerable(Of LabelTag) = Nothing
					If key.Contains("."c) Then
						Dim Ns = key.Split({"."c}, StringSplitOptions.RemoveEmptyEntries)
						If Ns?.Length > 1 Then
							Ts = tags.Where(Function(x) x.Name.Equals(Ns(0), StringComparison.OrdinalIgnoreCase) AndAlso x.Type.Equals(Ns(1), StringComparison.OrdinalIgnoreCase))
						End If
					Else
						Ts = tags.Where(Function(x) x.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
					End If

					If Ts?.Count > 0 Then
						For Each t In Ts
							template = t.Replace(template, value, replaceFunc)
						Next
					End If
				End If
			End If

			Return template
		End Function

		''' <summary>替换模板数据</summary>
		Public Overloads Shared Function Replace(template As String, values As NameValueDictionary, Optional tags As List(Of LabelTag) = Nothing, Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing) As String
			If template.NotEmpty AndAlso values?.Count > 0 Then
				If tags Is Nothing Then tags = GetTags(template)
				If tags?.Count > 0 Then
					For Each kv In values.ToList
						template = Replace(template, kv.Key, kv.Value, tags, replaceFunc)
					Next
				End If
			End If

			Return template
		End Function

		''' <summary>替换模板数据</summary>
		''' <param name="template">模板</param>
		''' <param name="key">标签名</param>
		''' <param name="values">值列表，根据 type 替换</param>
		''' <param name="tags">已经获取到的标签，不设置则自动根据 key 来分析</param>
		''' <param name="replaceFunc">其他替换函数</param>
		''' <param name="replaceALL">是否替换所有key标签，如果type不在Values列表中也用空白替换，否则不替换不存在标签</param>
		Public Overloads Shared Function Replace(template As String, key As String, values As NameValueDictionary, Optional tags As List(Of LabelTag) = Nothing, Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing, Optional replaceALL As Boolean = False) As String
			If template.NotEmpty AndAlso Not String.IsNullOrWhiteSpace(key) AndAlso values?.Count > 0 Then
				If tags Is Nothing Then tags = GetTags(template)
				If tags?.Count > 0 Then
					Dim Ts = tags.Where(Function(x) x.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
					If Ts?.Count > 0 Then
						For Each T In Ts
							If replaceALL OrElse values.ContainsKey(T.Type) Then
								template = T.Replace(template, values(T.Type), replaceFunc)
							End If
						Next
					End If
				End If
			End If

			Return template
		End Function

	End Class

End Namespace