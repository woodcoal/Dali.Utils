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
' 	块标签
'
' 	name: Tag.BlockTag
' 	create: 2020-04-18
' 	memo: 块标签
' 	
' ------------------------------------------------------------

Namespace Template.Tag

	''' <summary>页面块片段</summary>
	Public Class BlockTag
		Inherits TagBase

		''' <summary>父级标签</summary>
		Public ReadOnly Property Parent As BlockTag

		''' <summary>标签前缀</summary>
		Public ReadOnly Label As String

		''' <summary>解析的内容</summary>
		Public Property Content As String

		''' <summary>属性中存在未解析的标签</summary>
		Public ReadOnly HasAttributeLabel As Boolean

		''' <summary>构造，初始化</summary>
		Public Sub New(node As String, Optional parent As BlockTag = Nothing)
			' 原始内容
			Raw = node
			Content = ""
			Name = ""

			' 检查是否有效标签
			If node.IsEmpty Then Exit Sub

			' 只允许标签，<ff:xxx></ff:xxx>
			If node.StartsWith("<" & PREFIX, StringComparison.OrdinalIgnoreCase) AndAlso node.EndsWith(">") Then node = node.Cut("<" & PREFIX, ">", False, False)

			' 非有效标签，退出检测
			If node.IsEmpty Then Exit Sub

			node &= " "
			Dim Len = node.IndexOf(" ")
			If Len < 1 Then Exit Sub

			' 分析当前标签的名称
			Dim tagName = node.Substring(0, Len)
			Dim tagEnd = "</" & PREFIX & tagName & ">"

			' 分析到有效的块标签
			If Raw.EndsWith(tagEnd, StringComparison.OrdinalIgnoreCase) Then
				Name = tagName.ToLower

				Dim s = Raw.IndexOf(">") + 1
				Dim l = Raw.Length - tagEnd.Length - s
				Content = Raw.Substring(s, l)

				' 属性是否存在为解析的标签
				HasAttributeLabel = node.Like("{" & PREFIX & "*}")

				' Attributes 属性分析
				GetAttributes(node)

				Me.Parent = parent

				' 前缀
				Label = Attribute("Label")
			End If
		End Sub

		''' <summary>获取所有块标签</summary>
		Public Shared Function GetTags(template As String, Optional allLevel As Boolean = True, Optional defNames As String() = Nothing) As List(Of BlockTag)
			Dim R As List(Of BlockTag) = Nothing
			GetTags(R, template, defNames, allLevel, Nothing)
			Return R
		End Function

		''' <summary>获取指定名称的块标签(仅当前层)</summary>
		Public Shared Function GetTags(template As String, name As String, Optional defNames As String() = Nothing) As List(Of BlockTag)
			If String.IsNullOrEmpty(name) Then
				Return GetTags(template, False, defNames)
			Else
				Return GetTags(template, False, defNames)?.Where(Function(x) x.Name = name.ToLower).ToList
			End If
		End Function

		''' <summary>获取所有块标签，不建议直接搜索 defName 方式获取指定标签名的块标签，这样将导致获取到子节点下的数据</summary>
		''' <param name="list">结果返回的列表</param>
		''' <param name="template">模板数据</param>
		''' <param name="defNames">默认必须包含的标签名称</param>
		''' <param name="allLevel">递归获取所有层级数据</param>
		''' <param name="parent">获取到的标签的上级</param>
		Private Shared Sub GetTags(ByRef list As List(Of BlockTag), template As String, Optional defNames As String() = Nothing, Optional allLevel As Boolean = True, Optional parent As BlockTag = Nothing)
			If String.IsNullOrWhiteSpace(template) Then Exit Sub

			' 默认其实标签
			Dim def = "<" & PREFIX
			If template.IndexOf(def, StringComparison.OrdinalIgnoreCase) < 0 Then Exit Sub

			' 分析
			list = If(list, New List(Of BlockTag))

			' 分析存在的所有块
			Dim s = 0
			Dim len = template.Length

			While s > -1 AndAlso s < len

				' 寻找头部，如果找不到退出
				s = template.IndexOf(def, s, StringComparison.OrdinalIgnoreCase)
				If s < 0 Then Exit While

				' 寻找当前标签的尾部，非块尾部，找不到(e:-1)退出
				Dim e = template.IndexOf(">", s, StringComparison.OrdinalIgnoreCase)
				If e < s Then Exit While

				' 分析获取到的标签行
				Dim name = template.Substring(s + 1, e - s - 1)

				' 分析名称，并过滤掉 <ff:xxx /> 这类标签
				If Not name.EndsWith("/") Then
					Dim spaceStart = name.IndexOf(" ")
					If spaceStart > 0 Then name = name.Substring(0, spaceStart)
					'name = name.Split(" "c)(0)

					' 分析到 ff:xxx
					If Not String.IsNullOrEmpty(name) AndAlso name.Length > def.Length Then

						'如果存在 defName 则还需要分析两者名字是否一致
						If defNames Is Nothing OrElse defNames.Length < 1 OrElse defNames.Any(Function(x) name.ToLower = (PREFIX & x).ToLower) Then
							' 分析结尾
							Dim fin = "</" & name & ">"
							Dim node = GetTag(template, "<" & name, fin, s, e)

							If Not String.IsNullOrEmpty(node) Then
								Dim Tag = New BlockTag(node, parent)
								If Tag IsNot Nothing AndAlso Not String.IsNullOrEmpty(Tag.Name) Then
									list.Add(Tag)

									' 是否需要分析子级数据
									If allLevel Then GetTags(list, Tag.Content, defNames, True, Tag)
								End If

								s += node.Length
							End If
						End If
					End If
				End If

				' 继续寻找下一个
				s += 1
			End While
		End Sub

		''' <summary>获取标签实际区域，过滤掉套嵌标签，如： ＜ff:xxx＞...＜ff:xxx＞...＜/ff:xxx＞...＜/ff:xxx＞</summary>
		Private Shared Function GetTag(template As String, tagBefore As String, tagEnd As String, startIndex As Integer, ByRef endIndex As Integer) As String
			' 因为这是来自程序的子过程，所以不做是否为空，长度等验证，之前已经做了
			Dim Ret = ""

			' 找第一个结尾
			Dim len = template.Length
			Dim lenStart = tagBefore.Length
			Dim lenEnd = tagEnd.Length

			If startIndex < 0 Then startIndex = 0
			If endIndex < startIndex Then endIndex = startIndex + lenStart
			endIndex = template.IndexOf(tagEnd, endIndex, StringComparison.OrdinalIgnoreCase)

			If endIndex > startIndex AndAlso endIndex <= len - lenEnd Then
				' 存在数据
				Ret = template.Substring(startIndex, endIndex - startIndex + lenEnd)

				' 分析套嵌量
				Dim Heads As String() = Ret.Cut(tagBefore, ">", True, True)
				Dim HeadCount = Heads?.Where(Function(x) Not x.EndsWith("/>")).Count

				' 除开当前头部还是存在其他头部
				If HeadCount > 1 Then
					' 存在多个套嵌，检查结尾数量
					' 如果结尾数量大于套嵌数量则有效，否则继续查找
					Dim EndCount = (Ret.Length - Ret.Replace(tagEnd, "").Length) / lenEnd
					If EndCount < HeadCount Then
						' 继续查找下一个
						' 如果存在下一个尾部则继续找
						Ret = GetTag(template, tagBefore, tagEnd, startIndex, endIndex + 1)
					End If
				End If
			End If

			Return Ret
		End Function

		''' <summary>替换内容下的标签</summary>
		Public Function ReplaceLabels(values As NameValueDictionary, Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing) As String
			Return ReplaceLabels(Content, values, replaceFunc)
		End Function

		''' <summary>替换内容下的标签</summary>
		Public Function ReplaceLabels(template As String, values As NameValueDictionary, Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing) As String
			If template.NotEmpty Then
				Dim tagName = Label.EmptyValue(Name)
				If values?.Count > 0 Then
					template = LabelTag.Replace(template, tagName, values,, replaceFunc, False)
				Else
					template = LabelTag.Replace(template, tagName, "",, replaceFunc)
				End If
			End If

			Return template
		End Function

	End Class

End Namespace
