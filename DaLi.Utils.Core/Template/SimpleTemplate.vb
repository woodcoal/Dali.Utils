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
' 	简单模板
'
' 	name: SimpleTemplate
' 	create: 2022-12-14
' 	memo: 简单模板，仅支持大括号包含内容
'
' ------------------------------------------------------------

Imports System.Text.RegularExpressions

Namespace Template
	''' <summary>简单模板</summary>
	Public Class SimpleTemplate

		''' <summary>标签</summary>
		Public Class Tag

#Region "属性"

			''' <summary>标签名称</summary>
			Public Property Name As String

			''' <summary>值标签类型，类似 {aa.bb} 中的 bb；其中 aa 用 Name 表示</summary>
			Public ReadOnly Type As String

			''' <summary>原始内容</summary>
			Public ReadOnly Property Raw As String

			''' <summary>原始内容</summary>
			Public ReadOnly Property Attributes As IEnumerable(Of (Key As String, Value As String))

			''' <summary>原始内容的HASH</summary>
			Public ReadOnly Property RawHash As String
				Get
					Return Raw.MD5
				End Get
			End Property

#End Region

			Public Sub New(template As String)
				Raw = ""
				Name = ""
				Type = ""

				' 检查是否有效标签
				If template.IsEmpty Then Return

				Dim match = Regex.Match(template, "\{([a-zA-Z0-9\.\-_]+)([^\}]*)\}")
				If Not match.Success Then Return

				' 原始内容
				Raw = match.Value

				' 名称
				Name = match.Groups(1).Value.ToLower
				If Name.Contains("."c) Then
					Dim path = Name.IndexOf("."c)
					Type = Name.Substring(path + 1)
					Name = Name.Substring(0, path)
				End If

				' 分析属性
				Attributes = TemplateHelper.GetAttributes(match.Groups(2).Value)

				' 类型，如果不存在，从属性中获取
				If Type.IsEmpty AndAlso Attributes.NotEmpty Then
					Type = Attributes.
						Where(Function(x) x.Key = "type").
						Select(Function(x) x.Value).
						FirstOrDefault
				End If
			End Sub

			''' <summary>替换指定内容</summary>
			''' <param name="template">原始模板数据</param>
			''' <param name="value">标签值</param>
			Public Function Replace(template As String, value As Object) As String
				If template.IsEmpty Then Return ""

				If value Is Nothing Then
					Return template.Replace(Raw, "")
				Else
					Return template.Replace(Raw, TemplateAction.Default.Execute(value, Attributes))
				End If
			End Function

			''' <summary>获取 HASH</summary>
			Public Overloads Function GetHashCode() As String
				Dim attrs = Attributes?.Select(Function(x) $"{x.Key}={x.Value}").JoinString(vbCrLf)
				Return $"{Name}.{Type} {attrs}".MD5
			End Function

		End Class

		''' <summary>获取标签列表</summary>
		''' <param name="template">原始内容</param>
		''' <param name="tagName">默认标签</param>
		Public Shared Function GetTags(template As String, Optional tagName As String = "") As List(Of Tag)
			' 无内容直接返回
			If template.IsEmpty Then Return Nothing

			If tagName.IsEmpty Then
				tagName = "[a-zA-Z0-9\.\-_]+"
			Else
				tagName = Regex.Escape(tagName)
			End If

			Dim matches = Regex.Matches(template, "\{(" & tagName & ＂)[^\}]*\}")
			If matches.IsEmpty Then Return Nothing

			' 过滤掉相同内容后返回所有标签
			Return matches.
				Select(Function(x) x.Value).
				Distinct.
				Select(Function(x) New Tag(x)).
				ToList
		End Function

		''' <summary>模板数据格式化</summary>
		''' <param name="template">模板数据</param>
		''' <param name="key">标签名称</param>
		''' <param name="value">替换值</param>
		''' <param name="tags">已经获取过的标签列表</param>
		Public Shared Function Format(template As String, key As String, value As Object, Optional tags As List(Of Tag) = Nothing) As String
			If template.IsEmpty OrElse key.IsEmpty Then Return template

			' 分离 key / type
			Dim tagName = key.ToLower
			Dim tagType = ""

			If tagName.Contains("."c) Then
				Dim path = tagName.IndexOf("."c)
				tagType = tagName.Substring(path + 1)
				tagName = tagName.Substring(0, path)
			End If

			' 标签列表
			If tags Is Nothing Then
				tags = GetTags(template, tagName)
			Else
				tags = tags.Where(Function(x) x.Name = tagName).ToList
			End If
			If tags.IsEmpty Then Return template

			' 分析类型
			tags = tags.Where(Function(x) x.Type = tagType).ToList
			If tags.IsEmpty Then Return template

			For Each t In tags
				template = t.Replace(template, value?.ToString)
			Next

			Return template
		End Function

		''' <summary>替换模板数据</summary>
		''' <param name="values">需要替换的标签名与值的列表</param>
		''' <param name="tags">已经获取过的标签列表</param>
		Public Shared Function Format(template As String, values As IDictionary(Of String, Object), Optional tags As List(Of Tag) = Nothing) As String
			If template.IsEmpty OrElse values.IsEmpty Then Return template

			' 标签列表
			If tags Is Nothing Then tags = GetTags(template)
			If tags.IsEmpty Then Return template

			'' 格式化值
			'Dim formatValue = Function(source As String, key As String, value As Object) As String
			'					  If value Is Nothing Then Return source

			'					  If value.GetType.IsDictionary(Of String, Object) Then
			'						  Dim dic = TryCast(value, IDictionary(Of String, Object))
			'						  source = Format(source, key, dic, tags)
			'					  Else
			'						  source = Format(source, key, value, tags)
			'					  End If

			'					  Return source
			'				  End Function

			'' 格式化标签
			'For Each tag In tags
			'	Dim key = ""
			'	Dim value = Nothing

			'	Try
			'		If tag.Type.NotEmpty Then
			'			key = $"{tag.Name}.{tag.Type}"
			'			value = values.Where(Function(x) x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault
			'			template = formatValue(template, key, value)
			'			If template.IsEmpty Then Exit For
			'		End If

			'		key = tag.Name
			'		value = values.Where(Function(x) x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault
			'		template = formatValue(template, key, value)
			'		If template.IsEmpty Then Exit For
			'	Catch ex As Exception

			'	End Try
			'Next

			' 存在标签进行替换
			For Each kv In values.ToList
				If kv.Value IsNot Nothing AndAlso kv.Value.GetType.IsDictionary(Of String, Object) Then
					Dim dic = TryCast(kv.Value, IDictionary(Of String, Object))
					template = Format(template, kv.Key, dic, tags)
				Else
					template = Format(template, kv.Key, kv.Value, tags)
				End If

				If template.IsEmpty Then Exit For
			Next

			Return template
		End Function

		''' <summary>替换指定标签类型数据</summary>
		''' <param name="template">模板</param>
		''' <param name="key">标签名</param>
		''' <param name="values">值列表，根据 type 替换</param>
		''' <param name="tags">已经获取到的标签，不设置则自动根据 key 来分析</param>
		''' <param name="replaceALL">是否替换所有key标签，如果type不在Values列表中也用空白替换，否则不替换不存在标签</param>
		Public Shared Function Format(template As String, key As String, values As IDictionary(Of String, Object), Optional tags As List(Of Tag) = Nothing, Optional replaceALL As Boolean = False) As String
			If template.IsEmpty OrElse key.IsEmpty OrElse values.IsEmpty Then Return template

			' 获取标签列表
			key = key.ToLower
			If tags.IsEmpty Then
				tags = GetTags(template, key)
			Else
				tags = tags.Where(Function(x) x.Name = key).ToList
			End If

			If tags.IsEmpty Then Return template

			' 替换属性
			Dim dic As New KeyValueDictionary(values)
			For Each T In tags
				If replaceALL OrElse dic.ContainsKey(T.Type) Then
					template = T.Replace(template, dic(T.Type)?.ToString)
				End If
			Next

			Return template
		End Function
	End Class
End Namespace