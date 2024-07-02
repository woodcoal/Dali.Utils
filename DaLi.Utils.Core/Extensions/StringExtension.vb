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
' 	字符扩展操作
'
' 	name: Extension.StringExtension
' 	create: 2020-10-23
' 	memo: 字符扩展操作
' 	
' ------------------------------------------------------------

Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports DaLi.Utils.Template

Namespace Extension

	''' <summary>字符扩展操作</summary>
	Public Module StringExtension

#Region "1. 常用字符操"

		''' <summary>获取字符串的实际长度(按单字节)</summary>
		<Extension>
		Public Function UnicodeLength(this As String) As Integer
			Dim R = 0

			If this.NotNull Then
				For Each C In this
					If AscW(C) > 127 Then
						R += 2
					Else
						R += 1
					End If
				Next
			End If

			Return R
		End Function

		''' <summary>计算目的字符串在源字符串中出现的次数</summary>
		<Extension>
		Public Function Times(this As String, target As String) As Integer
			If this.NotEmpty AndAlso target.NotEmpty Then
				'Return (this.Length - this.Replace(target, "", StringComparison.OrdinalIgnoreCase).Length) / target.Length
				Return Regex.Matches(this, Regex.Escape(target), RegexOptions.IgnoreCase)?.Count
			Else
				Return 0
			End If
		End Function

		''' <summary>重复指定次数相同的字符串</summary>
		<Extension>
		Public Function Duplicate(this As String, duplicateTimes As Integer) As String
			If duplicateTimes > 0 AndAlso this.NotNull Then
				With New Text.StringBuilder
					For I = 1 To duplicateTimes
						.Append(this)
					Next

					Return .ToString
				End With
			Else
				Return ""
			End If
		End Function

		'''' <summary>生成指定长度空格</summary>
		'Public Function Space(spaceTimes As Integer) As String
		'	Return New String(" "c, spaceTimes)
		'End Function

		''' <summary>如果原始数据为空则返回后者数据，直到最后一个数据</summary>
		<Extension>
		Public Function NullValue(this As String, ParamArray moreValues As String()) As String
			If this.NotNull Then Return this

			If moreValues?.Length > 0 Then
				For Each Value In moreValues
					If Value.NotNull Then Return Value
				Next
			End If

			Return ""
		End Function

		''' <summary>如果原始数据为空/空格等则返回后者数据，直到最后一个数据</summary>
		<Extension>
		Public Function EmptyValue(this As String, ParamArray moreValues As String()) As String
			If this.NotEmpty Then Return this

			If moreValues?.Length > 0 Then
				For Each Value In moreValues
					If Value.NotEmpty Then Return Value
				Next
			End If

			Return ""
		End Function

		''' <summary>将字符串反转</summary> 
		<Extension>
		Public Function Reverse(this As String) As String
			If this.IsEmpty Then
				Return ""
			ElseIf this.Length < 2 Then
				Return this
			Else
				Return New String(this.ToCharArray.Reverse.ToArray)
				'Return Text.Encoding.Unicode.GetString(Text.Encoding.Unicode.GetBytes(this).Reverse)
			End If
		End Function

		''' <summary>计算 MD5 值</summary>
		''' <param name="mode">返回模式，16位(False)或者32位(True,默认)</param>
		<Extension>
		Public Function MD5(this As String, Optional mode As Boolean = True, Optional encoding As Text.Encoding = Nothing) As String
			Return HashHelper.MD5(this, mode, encoding)
		End Function

		''' <summary>计算 SHA1 值</summary>
		<Extension>
		Public Function SHA1(this As String, Optional encoding As Text.Encoding = Nothing) As String
			Return HashHelper.SHA1(this, encoding)
		End Function

		''' <summary>长字符串缩短展示，显示头尾，中间省略</summary>
		<Extension>
		Public Function ShortShow(this As String, Optional maxLength As Integer = 100, Optional hideChats As String = "……") As String
			If maxLength > 5 AndAlso this?.Length > maxLength Then
				Dim Len As Integer = Math.Floor(maxLength / 2) - 1
				Return this.Left(Len) & hideChats & this.Right(Len)
			Else
				Return this
			End If
		End Function

		''' <summary>隐藏字符中的内容</summary>
		<Extension>
		Public Function Mask(this As String, Optional maskChar As Char = "*"c) As String
			If this.IsEmpty Then Return this

			Dim masks = New String(maskChar, 4)
			Select Case this.Length
				Case Is >= 11
					Return Regex.Replace(this, "(.{3}).*(.{4})", $"$1{masks}$2")

				Case 10
					Return Regex.Replace(this, "(.{3}).*(.{3})", $"$1{masks}$2")

				Case 9
					Return Regex.Replace(this, "(.{2}).*(.{3})", $"$1{masks}$2")

				Case 8
					Return Regex.Replace(this, "(.{2}).*(.{2})", $"$1{masks}$2")

				Case 7
					Return Regex.Replace(this, "(.{1}).*(.{1})", $"$1{masks}$2")

				Case 6
					Return Regex.Replace(this, "(.{1}).*(.{1})", $"$1{masks}$2")

				Case Else
					Return Regex.Replace(this, "(.{1}).*", $"$1{masks}")
			End Select
		End Function


		''' <summary>验输入字符串是否长于指定长度</summary> 
		<Extension>
		Public Function LengthLargen(this As String, len As Integer) As Boolean
			Return this?.Length > len
		End Function

		''' <summary>验输入字符串是否短于指定长度</summary> 
		<Extension>
		Public Function LengthLess(this As String, len As Integer) As Boolean
			Return this.IsEmpty OrElse this.Length < len
		End Function

#End Region

#Region "2. 清除多余数据"

		''' <summary>过滤ASCII码中的控制字符</summary>
		''' <param name="isClear">是否清理掉控制符，否则用空格代替</param>
		<Extension>
		Public Function ClearControl(this As String, Optional isClear As Boolean = False) As String
			If this.IsNull Then
				Return ""
			Else
				If isClear Then
					Return Regex.Replace(this, "[\x00-\x1E]", "", RegexOptions.IgnoreCase)
				Else
					Return Regex.Replace(this, "[\x00-\x1E]", " ", RegexOptions.IgnoreCase)
				End If
			End If
		End Function

		''' <summary>删除两个以上多余的空字符，包括空格\f\n\r\t\v</summary>
		<Extension>
		Public Function ClearSpace(this As String) As String
			If this.IsEmpty Then
				Return ""
			Else
				Return Regex.Replace(this, "(\s){2,}", "$1").Trim
			End If
		End Function

		''' <summary>过滤 Xml 文档中 ASCII码中的控制字符，防止反序列化错误,仅保留回车换行</summary>
		''' <param name="this">要操作的字符串</param>
		<Extension>
		Public Function ClearLowAscii(this As String, Optional encode As Boolean = True) As String
			If this.IsNull Then
				Return ""
			ElseIf encode Then
				Return Regex.Replace(this, "&#x([0-9BCEF]{1}|1[0-9A-F]{1});", "", RegexOptions.IgnoreCase).Trim
			Else
				Return Regex.Replace(this, "[\x00-\x09\x0B\x0C\x0E-\x1F]", "", RegexOptions.IgnoreCase).Trim
			End If
		End Function

		''' <summary>过滤Html标签</summary>
		''' <param name="this">要操作的字符串</param>
		''' <param name="tags">要过滤的HTML标签数组，或者用逗号间隔的字符串</param>
		<Extension>
		Public Function ClearHtml(this As String, ParamArray tags() As String) As String
			If this.NotEmpty Then
				' 仅一个且用逗号间隔
				If tags.Length = 1 AndAlso tags(0).Contains(","c) Then tags = tags(0).SplitDistinct

				' 去掉多余标签，并转化成小写
				tags = tags.Select(Function(x) x.Trim.ToLower).Distinct.ToArray

				If tags?.Length > 0 Then
					For Each tagName In tags
						Select Case tagName
							Case "空间命名", "namespace"
								this = Regex.Replace(this, "<\/?[a-z]+[^>]*>", "")
							Case "xml"
								this = Regex.Replace(this, "<\/?xml[^>]*>", "")
								this = Regex.Replace(this, "<\!DOCTYPE[^>]*>", "")
							Case "style"
								this = Regex.Replace(this, "\s*(style|class|id)=""[^""]*""", "")
								this = Regex.Replace(this, "\s*(style|class|id)='[^']*'", "")
								'This = Regex.Replace(This, "(<[^>]+) [style|class]=[^ |^>]*([^>]*>)", "$1 $2")
								this = Regex.Replace(this, "<style((.|\n)+?)</style>", "")
							Case "script"
								this = Regex.Replace(this, "(on(load|click|dbclick|mouseover|mousedown|mouseup)=""[^""]+"")", "")
								this = Regex.Replace(this, "<script((.|\n)+?)</script>", "")
							Case "注释", "comment"
								this = Regex.Replace(this, "<\!--([\w\W]*?)-->", "")
							Case "all"
								' 去掉之前多余的标签
								this = this.ClearHtml("namespace,xml,script,style,comment")

								this = Regex.Replace(this, "&nbsp;", " ")
								this = Regex.Replace(this, "&quot;", """")
								this = Regex.Replace(this, "&amp;", "&")
								this = Regex.Replace(this, "&lt;", "<")
								this = Regex.Replace(this, "&gt;", ">")

								this = Net.WebUtility.HtmlDecode(this)

								this = Regex.Replace(this, "<[^>]*?>", "")

							Case "enter"
								this = this.Replace(vbCr, "").Replace(vbLf, "")

							Case "tab"
								this = this.Replace(vbTab, "")

							Case "space"
								this = this.Replace(" ", "").Replace("　", "")

							Case "trim"
								this = this.TrimFull

							Case Else
								this = Regex.Replace(this, "(<" & tagName & "([^>])*>|</" & tagName & "([^>])*>)", "")
						End Select

						If this.IsEmpty Then Exit For
					Next
				End If
			End If

			Return this
		End Function

		''' <summary>清除整个字符串中多余的空格和Ascii控制字符</summary>
		<Extension>
		Public Function TrimFull(this As String) As String
			If this.IsEmpty Then
				Return ""
			Else
				Return this.ClearControl.ClearSpace
			End If
		End Function

#End Region

#Region "3. 替换字符串"

		''' <summary>正则表达式替换</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="oldValue"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换"）</param>
		''' <param name="newValue">替换的内容</param>
		<Extension>
		Public Function ReplaceRegex(this As String, oldValue As String, Optional newValue As String = "") As String
			If this.NotNull AndAlso oldValue.NotNull Then
				If newValue.IsNull Then newValue = ""

				' 更新并判断正则条件
				Dim Pattern = PatternUpdate(oldValue)
				If Pattern.IsPattern Then
					Return Regex.Replace(this, Pattern.Pattern, newValue, RegexOptions.IgnoreCase)
				Else
					Return this.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase)
				End If
			End If

			Return this
		End Function

		''' <summary>替换一组数据</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="oldValues"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换，一个字符只能包含一个"[*]"）</param>
		''' <param name="newValue">替换的内容</param>
		<Extension>
		Public Function ReplaceMutli(this As String, oldValues As String(), Optional newValue As String = "") As String
			If this.NotNull AndAlso oldValues?.Length > 0 Then
				For Each oldValue As String In oldValues
					this = this.ReplaceRegex(oldValue, newValue)
					If this.IsNull Then Exit For
				Next
			End If

			Return this
		End Function

		''' <summary>内链替换</summary>
		''' <param name="this">源内容</param>
		''' <param name="formatLink">格式，Markdown文档则使用默认格式：关键词用[key]表示；链接用[link]表示；为空时，自动分析，如果Link为网址则生成连接，否则直接输出Link</param>
		''' <param name="count">替换数量</param>
		''' <param name="isMarkdown">是否Markdown文档</param>
		<Extension>
		Public Function ReplaceLink(this As String, links As NameValueDictionary, Optional formatLink As String = "", Optional count As Integer = 10, Optional isMarkdown As Boolean = False) As String
			' 数据无效或者链接不包含{link}，直接返回
			If links.IsEmpty OrElse this.IsEmpty Then Return this
			If formatLink.NotEmpty AndAlso Not formatLink.Contains("{link}", StringComparison.OrdinalIgnoreCase) Then Return this

			' 默认链接格式
			Dim LinkFormat As String
			If formatLink.IsEmpty Then
				LinkFormat = If(isMarkdown, "[{key}]({link})", "<a href=""{link}"" target=""_blank"">{key}</a>")
			Else
				LinkFormat = formatLink
			End If

			' 默认替换数量
			If count < 1 Then count = Integer.MaxValue

			' 返回结果
			Dim Ret = this

			'所有标签内与链接中不能添加链接
			'<[^>]*>
			'<a [^>]*>(.*?)</a>
			Dim Tags As New NameValueDictionary
			Dim Key = ":" & RandomHelper.Mix(6) & ":"
			Dim Idx = 0

			If Not isMarkdown Then
				Dim TagExp As String = "<a [*]</a>"
				Dim TagList As String() = this.Cut(TagExp, 0, True)
				If TagList.NotEmpty Then
					For Each Tag As String In TagList
						Idx += 1
						Dim Hash As String = Key & Idx
						If Not Tags.ContainsKey(Hash) Then
							Tags.AddFast(Hash, Tag)
							Ret = Ret.Replace(Tag, Hash)
						End If
					Next
				End If

				TagExp = "<[^>]*>"
				TagList = this.Cut(TagExp, 0, True)
				If TagList.NotEmpty Then
					For Each Tag As String In TagList
						Idx += 1
						Dim Hash As String = Key & Idx
						If Not Tags.ContainsKey(Hash) Then
							Tags.AddFast(Hash, Tag)
							Ret = Ret.Replace(Tag, Hash)
						End If
					Next
				End If
			End If

			'过滤掉所有链接
			For Each name As String In links.Keys
				Dim link = links(name)
				If formatLink.NotEmpty OrElse link.IsUrl Then link = LinkFormat.Replace("{key}", name).Replace("{link}", link)

				Idx += 1
				Dim Hash As String = Key & Idx
				Tags.AddFast(Hash, link)

				' 次数不限，直接替换
				If count = Integer.MaxValue Then
					Ret = Ret.Replace(name, Hash)
				Else
					' 次数有限，只替换其中一个
					' 总次数不能超过

					Dim b = Ret.IndexOf(name, StringComparison.OrdinalIgnoreCase)
					If b > -1 Then
						Dim e = b + name.Length
						If e <= Ret.Length Then
							Ret = String.Concat(Ret.AsSpan(0, b), Hash, Ret.AsSpan(e))

							count -= 1
							If count < 1 Then Exit For
						End If
					End If
				End If
			Next

			' 还原链接
			For Each Hash In Tags.Keys
				Ret = Ret.Replace(Hash, Tags(Hash))
			Next

			Return Ret
		End Function

		''' <summary>使用简单标签替换，即{}包含的文本</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="key">标签名称，如果包含小数点，则替换对应标签的类型</param>
		''' <param name="value">替换值</param>
		<Extension>
		Public Function FormatTemplate(this As String, key As String, value As String) As String
			Return SimpleTemplate.Format(this, key, value)
		End Function

		''' <summary>使用简单标签替换，即{}包含的文本</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="values">替换标签名值字典数据</param>
		''' <param name="clearTag">是否清除所有未适配的标签，为防止错误清除，可以将 {{ 代替 {，}} 代替 }</param>
		<Extension>
		Public Function FormatTemplate(this As String, values As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As String
			If this.IsEmpty Then Return this
			If Not Regex.IsMatch(this, "\{[^\}]*\}") Then Return this

			' 如果需要替换全部标签，则先将转义的{}替换成其他标签，防止被错误替换
			Dim tick = Date.Now.Subtract(New Date(2020, 1, 1)).TotalHours
			Dim s1 = $"$[[{tick}"
			Dim s2 = $"{tick}]]$"

			this = this.Replace("{{", s1).Replace("}}", s2)
			this = SimpleTemplate.Format(this, values)

			If this.NotEmpty Then
				If clearTag Then this = Regex.Replace(this, "\{[^\}]*\}", "")
				this = this.Replace(s1, "{").Replace(s2, "}")
			End If

			Return this
		End Function

		''' <summary>使用自定义前后缀标签替换</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="values">替换标签名值字典数据</param>
		''' <param name="prefix">前缀</param>
		''' <param name="suffix">后缀</param>
		<Extension>
		Public Function FormatTemplateEx(this As String, values As IDictionary(Of String, Object), Optional prefix As String = "${", Optional suffix As String = "}") As String
			If this.IsEmpty OrElse prefix.IsEmpty OrElse suffix.IsEmpty Then Return this

			Dim pre = Regex.Escape(prefix)
			Dim suf = Regex.Escape(suffix)

			Dim matches = Regex.Matches(this, $"{pre}((.|\n)*?){suf}")
			If matches.IsEmpty Then Return this

			' 处理重复数据
			Dim ms = matches.Select(Function(x) x.Groups(1).Value).Distinct.ToList
			If ms.IsEmpty Then Return this

			' 分类标签与属性
			' 1. 分析名称与属性，属性按标题排序
			' 2. 获取标签 Hash
			' 3. Hash 分组，将同组的标签合并标记
			Dim tags = ms.Select(Function(x)
									 Dim s = x.TrimFull & " "
									 Dim name = s.Split(" ")(0)
									 Dim attrs = TemplateHelper.
												GetAttributes(s.Substring(name.Length))?.
												OrderBy(Function(a) a.Key).
												ToList
									 Dim hash = $"{name}|{attrs.ToJson}".MD5

									 Return New With {.source = x, name, attrs, hash}
								 End Function).
								 GroupBy(Function(x) x.hash).
								 ToList.
								 Select(Function(x)
											Dim sources = x.Select(Function(t) $"{prefix}{t.source}{suffix}").ToList
											Dim item = x(0)
											Return New With {item.name, item.attrs, sources}
										End Function).
								ToList

			' 复制并替换
			Dim data = values.ToJson.ToJsonNameValues
			tags.ForEach(Sub(tag)
							 ' 获取值
							 Dim value = data(tag.name)
							 If value IsNot Nothing Then value = TemplateAction.Default.Execute(value, tag.attrs)

							 ' 替换
							 tag.sources.ForEach(Sub(source) this = this.Replace(source, value, StringComparison.OrdinalIgnoreCase))
						 End Sub)

			Return this
		End Function

		''' <summary>使用简单标签替换，即{}包含的文本</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="value">替换的对象</param>
		<Extension>
		Public Function Format(Of T As Class)(this As String, value As T, <CallerArgumentExpression("value")> Optional name As String = Nothing) As String
			If this.IsEmpty OrElse value Is Nothing OrElse name.IsEmpty Then Return this

			Return SimpleTemplate.Format(this, name, value.ToDictionary(False))
		End Function

#End Region

#Region "4. 是否包含"

		''' <summary>数据是否存在指定内容，仅作简单比较，复杂比较请使用 Include 函数</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="values">
		''' 要检查的字符串数组，不区分大小写；
		''' 星号表示变化部分，不存在则需要整个字符完全匹配；
		''' * 表示 表示 不为空的数据；
		''' *xxxx 表示 以 xxxx 结尾数据；
		''' xxxx* 表示 以 xxxx 开头的数据；
		''' xxxx*yyyy 表示 以 xxxx 开头，yyyy 结尾的数据；
		''' *xxxx* 表示 数据中存在 xxxx；
		''' *xxxx*yyyy* 表示 数据中存在 xxxx开头，yyyy结尾的数据；
		''' 使用圆括号起始则使用正则表达式
		''' </param>
		<Extension>
		Public Function [Like](this As String, ParamArray values As String()) As Boolean
			If this.NotEmpty AndAlso values.NotEmpty Then
				Dim Flag = False

				For Each value In values.Where(Function(x) x.NotEmpty).Select(Function(x) x.ToLower).Distinct.ToList
					If value = "*" Then
						Flag = True

					ElseIf value.StartsWith("*"c) AndAlso value.EndsWith("*"c) Then
						value = value.Substring(1, value.Length - 2)
						Dim pattern = value.Trim("*"c)
						If pattern.NotEmpty AndAlso pattern.Contains("*"c) Then
							' 包含区域内容其实
							pattern = Regex.Escape(pattern).Replace("\*", "((.|\n)*?)")
							Flag = Regex.IsMatch(this, pattern, RegexOptions.IgnoreCase)

						Else
							Flag = this.Contains(value, StringComparison.OrdinalIgnoreCase)
						End If

					ElseIf value.StartsWith("*"c) AndAlso Not value.EndsWith("*"c) Then
						Flag = this.EndsWith(value.Substring(1), StringComparison.OrdinalIgnoreCase)

					ElseIf Not value.StartsWith("*"c) AndAlso value.EndsWith("*"c) Then
						Flag = this.StartsWith(value.Substring(0, value.Length - 1), StringComparison.OrdinalIgnoreCase)

					ElseIf value.Contains("*"c) Then
						Dim Vs = value.Split("*"c)
						If Vs.Length = 2 Then Flag = this.StartsWith(Vs(0), StringComparison.OrdinalIgnoreCase) AndAlso this.EndsWith(Vs(1), StringComparison.OrdinalIgnoreCase)

					ElseIf value.StartsWith("("c) AndAlso value.EndsWith(")") Then
						value = value.Substring(1, value.Length - 2)
						If value.NotEmpty Then Flag = Regex.IsMatch(this, value, RegexOptions.IgnoreCase)

					End If

					' 全部内容再匹配一次
					If Not Flag Then Flag = this.Equals(value, StringComparison.OrdinalIgnoreCase)

					If Flag Then Exit For
				Next

				Return Flag
			Else
				Return True
			End If
		End Function

		''' <summary>用正则表达式检查字符串中包含指定内容</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="value">要检查的正则表达式</param>
		<Extension>
		<Obsolete("请使用 like 代替")>
		Public Function Include(this As String, value As String) As Boolean
			If this.NotNull Then
				Dim Pattern = PatternUpdate(value)
				If Pattern.IsPattern Then
					Return Regex.IsMatch(this, Pattern.Pattern, RegexOptions.IgnoreCase)
				Else
					Return this.Contains(value, StringComparison.OrdinalIgnoreCase)
				End If
			End If

			Return False
		End Function

		''' <summary>用指定字符串组检查字符串中包含指定内容</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="values">是否包含的字符串组</param>
		<Extension>
		<Obsolete("请使用 like 代替")>
		Public Function Include(this As String, ParamArray values As String()) As Boolean
			If this.NotNull AndAlso values?.Length > 0 Then
				For Each value In values.Select(Function(x) x.ToLower).Distinct
					If this.Include(value) Then Return True
				Next
			End If

			Return False
		End Function

#End Region

#Region "5. 截取字符串"

		''' <summary>保留左侧字符长度</summary>
		<Extension>
		Public Function Left(this As String, stringLength As Integer, Optional lastAppend As String = "") As String
			If this?.Length > stringLength AndAlso stringLength > 0 Then
				If lastAppend.IsEmpty Then
					Return this.Substring(0, stringLength)
				Else
					stringLength -= lastAppend.Length
					If stringLength < 1 Then Return lastAppend

					Return String.Concat(this.AsSpan(0, stringLength), lastAppend)
				End If
			Else
				Return this
			End If
		End Function

		''' <summary>保留右侧字符长度</summary>
		<Extension>
		Public Function Right(this As String, stringLength As Integer) As String
			If this?.Length > stringLength AndAlso stringLength > 0 Then
				Return this.Substring(this.Length - stringLength, stringLength)
			Else
				Return this
			End If
		End Function

		''' <summary>获取指定长度字符，一个汉字占用两个字符</summary>
		''' <param name="This">要获取的源字符串</param>
		''' <param name="unicodeLength">截取的长度，注意汉字为双字节</param>
		''' <param name="lastAppend">末尾添加字符串</param>
		<Extension>
		Public Function Cut(this As String, unicodeLength As Integer， Optional lastAppend As String = "…") As String
			lastAppend = lastAppend.EmptyValue("")
			unicodeLength -= lastAppend.Length

			If unicodeLength > 0 AndAlso this.NotNull Then
				Dim I As Integer = 0
				Dim J As Integer = 0

				'为汉字或全脚符号长度加2否则加1
				For Each C In this
					If AscW(C) > 127 Then
						I += 2
					Else
						I += 1
					End If

					If I >= unicodeLength Then
						this = String.Concat(this.AsSpan(0, J), lastAppend)
						Exit For
					End If

					J += 1
				Next
			End If

			Return this
		End Function

		''' <summary>通过指定的正则表达试来获取内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="partNumber"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换，一个字符只能包含一个"[*]"）</param>
		''' <param name="moreValue">是否返回多项内容，默认为True，多项内容为数组格式</param>
		<Extension>
		Public Function Cut(this As String, pattern As String, Optional partNumber As Integer = 0, Optional moreValue As Boolean = True) As Object
			If this.NotNull Then
				Dim Query = PatternUpdate(pattern)
				If Query.IsPattern Then
					If partNumber < 1 Then partNumber = 0

					Dim Res As New List(Of String)

					Try
						Dim Ms = Regex.Matches(this, Query.Pattern, RegexOptions.IgnoreCase)
						If Ms?.Count > 0 Then
							For I = 0 To Ms.Count - 1
								Dim Ret As String = Nothing

								If Ms(I)?.Groups?.Count > partNumber Then
									Ret = Ms(I).Groups(partNumber).Value
								ElseIf Ms(I)?.Length > 0 Then
									Ret = Ms(I).Value
								End If

								If Ret.StartsWith("|_S.T.A.R.T_|") Then Ret = Ret.Substring(13)
								If Ret.EndsWith("|_F.I.N.I.S.H_|") Then Ret = Ret.Substring(0, Ret.Length - 15)

								If Ret IsNot Nothing Then Res.Add(Ret)
								If Not moreValue Then Exit For
							Next
						End If
					Catch ex As Exception
					End Try

					If Res.Count > 0 Then Return If(moreValue, Res.ToArray, Res(0))
				End If
			End If

			Return If(moreValue, Nothing, "")
		End Function

		''' <summary>通过指定的表达试来获取内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="begin">开始部分</param>
		''' <param name="last">结束部分，全部内容为 0</param>
		''' <param name="moreValue">是否返回多项内容，默认为True，多项内容为数组格式</param>
		''' <param name="isIncludeSelf">是否返回包含本身，默认为False</param>
		<Extension>
		Public Function Cut(this As String, begin As String, last As String, Optional moreValue As Boolean = True, Optional isIncludeSelf As Boolean = False) As Object
			If this.IsNull Then Return Nothing
			If begin.IsNull AndAlso last.IsNull Then Return this

			Dim strBegin As String = "|_S.T.A.R.T_|"
			Dim strEnd As String = "|_F.I.N.I.S.H_|"

			If begin.IsNull Then this = strBegin & this Else strBegin = begin
			If last.IsNull Then this &= strEnd Else strEnd = last

			Dim strPattern As String = strBegin & "[*]" & strEnd

			If isIncludeSelf Then
				Return Cut(this, strPattern, 0, moreValue)
			Else
				Return Cut(this, strPattern, 1, moreValue)
			End If
		End Function

		''' <summary>通过指定的表达试来获取内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="pattern"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换，一个字符只能包含一个"[*]"）</param>
		''' <param name="moreValue">是否返回多项内容，默认为True，多项内容为数组格式</param>
		<Extension>
		Public Function Cut(this As String, pattern As String, moreValue As Boolean) As Object
			Return Cut(this, pattern, 1, moreValue)
		End Function


		''' <summary>以指定分割符作为标志，剪切文本到到最大长度</summary>
		''' <param name="separator">分隔字符</param>
		''' <param name="maxLength">返回最大长度</param>
		<Extension>
		Public Function Cut(this As String, separator As String, maxLength As Integer) As String
			If this.IsEmpty OrElse maxLength < 1 OrElse this.Length < maxLength Then Return this
			If separator.IsEmpty Then Return this.Substring(0, maxLength)

			Dim path = this.IndexOf(separator, maxLength)
			If path > maxLength Then
				this = this.Substring(0, path)
			Else
				this = this.Substring(0, maxLength)
			End If

			maxLength = this.LastIndexOf(separator)
			If maxLength > -1 Then
				Return this.Substring(0, maxLength)
			Else
				Return this
			End If
		End Function

#End Region

#Region "6. 判断类型"

		''' <summary>不为空</summary>
		<Extension>
		Public Function NotNull(this As String) As Boolean
			Return Not this.IsNull
		End Function

		''' <summary>包含文本内容</summary>
		<Extension>
		Public Function NotEmpty(this As String) As Boolean
			Return Not this.IsEmpty
		End Function

		''' <summary>是否为空</summary>
		<Extension>
		Public Function IsNull(this As String) As Boolean
			Return String.IsNullOrEmpty(this)
		End Function

		''' <summary>是否为空或者空格</summary>
		<Extension>
		Public Function IsEmpty(this As String) As Boolean
			Return String.IsNullOrWhiteSpace(this)
		End Function

		''' <summary>两字符串是否相同</summary>
		''' <param name="CheckCase">是否比较大小写</param>
		<Extension>
		Public Function IsSame(this As String, target As String, Optional checkCase As Boolean = False) As Boolean
			If this.IsNull Then
				Return target.IsNull
			Else
				If target.IsNull Then
					Return False
				Else
					If checkCase Then
						Return this = target
					Else
						Return this.Equals(target, StringComparison.OrdinalIgnoreCase)
					End If
				End If
			End If
		End Function

		''' <summary>验证 Email</summary>
		<Extension>
		Public Function IsEmail(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$", RegexOptions.IgnoreCase)
		End Function

		''' <summary>验证 GUID</summary>
		<Extension>
		Public Function IsGUID(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$")
		End Function

		''' <summary>验证 IP</summary>
		<Extension>
		Public Function IsIP(this As String) As Boolean
			Return this.IsIPv4 OrElse this.IsIPv6
		End Function

		''' <summary>验证 IPv4</summary>
		<Extension>
		Public Function IsIPv4(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$")
		End Function

		''' <summary>验证 IPv6</summary>
		<Extension>
		Public Function IsIPv6(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^(([0-9a-fA-F]{1,4}:){7}([0-9a-fA-F]{1,4}|:))|(([0-9a-fA-F]{1,4}:){6}(((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3})|:[0-9a-fA-F]{1,4}|:)|(([0-9a-fA-F]{1,4}:){5}(:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|((:[0-9a-fA-F]{1,4}){1,3})|:)|(([0-9a-fA-F]{1,4}:){4}(((:[0-9a-fA-F]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,3})|:)|(([0-9a-fA-F]{1,4}:){3}(((:[0-9a-fA-F]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,4})|:)|(([0-9a-fA-F]{1,4}:){2}(((:[0-9a-fA-F]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,5})|:)|(([0-9a-fA-F]{1,4}:){1}(((:[0-9a-fA-F]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,6})|:)|(:(((:[0-9a-fA-F]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,7})|:)$")
		End Function

		''' <summary>验证电话</summary>
		<Extension>
		Public Function IsPhone(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^(((0[0-9]{2,3}(\-| ))?([1-9][0-9]{6,7})+((\-| )[0-9]{1,4})?)|(1[3-9]\d{9}|^1060[1-9]\d{1,2}\d{7,8}))$")
		End Function

		''' <summary>验证电话</summary>
		<Extension>
		Public Function IsMobilePhone(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^1[3-9]\d{9}$")
		End Function

		''' <summary>验证身份证</summary>
		<Extension>
		Public Function IsCardID(this As String) As Boolean
			Return CardIDHelper.Validate(this)
		End Function

		''' <summary>验证营业执照</summary>
		<Extension>
		Public Function IsBusinessID(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "(^(?:(?![IOZSV])[\dA-Z]){2}\d{6}(?:(?![IOZSV])[\dA-Z]){10}$)|(^\d{15}$)")
		End Function

		''' <summary>验证护照</summary>
		<Extension>
		Public Function IsPassport(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^((1[45]\d{7})|(G\d{8})|(P\d{7})|(S\d{7,8}))?$")
		End Function

		''' <summary>验证港澳居民通行证</summary>
		<Extension>
		Public Function IsHKMO(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[HMhm]{1}([0-9]{10}|[0-9]{8})$")
		End Function

		''' <summary>验证台湾居民来往大陆通行证</summary>
		<Extension>
		Public Function IsTaiWan(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[0-9]{8}$")
		End Function

		''' <summary>判断字符串是否为正整数</summary> 
		<Extension>
		Public Function IsUInt(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^\d*$")
		End Function

		''' <summary>判断字符串是否为整形数字</summary> 
		<Extension>
		Public Function IsInteger(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[+-]?\d*$")
		End Function

		'''' <summary>判断字符串是否为浮点数字</summary> 
		'<Extension>
		'Public Function IsFloat(this As String) As Boolean
		'    Return (This.NotEmpty AndAlso Regex.IsMatch(This, "[0-9\.\-]"))
		'End Function

		''' <summary>判断字符串是否为数字</summary> 
		<Extension>
		Public Function IsNumber(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[+-]?\d*[.]?\d*$")
		End Function

		''' <summary>判断字符串是否为A-Z、0-9及下划线以内的字符</summary> 
		<Extension>
		Public Function IsLetterNumber(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[a-zA-Z0-9_\-]+$")
		End Function

		''' <summary>判断是否有效用户名，字母开头，0-9/A-Z，最少2个字符，最多24个字符</summary> 
		''' <param name="maxLength">最多字符数，必须大于6</param>
		''' <param name="enDot">是否允许包含小数点</param>
		<Extension>
		Public Function IsUserName(this As String, Optional maxLength As Integer = 24, Optional enDot As Boolean = False) As Boolean
			If maxLength < 6 Then maxLength = 6
			maxLength -= 1

			Dim Pattern As String
			If enDot Then
				Pattern = "^[a-zA-Z]{1,1}[0-9a-zA-Z_\-\.]{1," & maxLength & "}$"
			Else
				Pattern = "^[a-zA-Z]{1,1}[0-9a-zA-Z_\-]{1," & maxLength & "}$"
			End If

			Return this.NotEmpty AndAlso Regex.IsMatch(this, Pattern)
		End Function

		''' <summary>判断密码是否有效（6-20位长度；必须包含字母）</summary> 
		<Extension>
		Public Function IsPassword(this As String) As Boolean
			Return this.NotEmpty AndAlso this.Length > 5 AndAlso this.Length < 21 AndAlso Regex.IsMatch(this, "[a-zA-Z]")
		End Function

		''' <summary>判断密码是否有效（6-20位长度；必须包含数字或大写字母）</summary> 
		<Extension>
		Public Function IsPasswordNumberLetter(this As String) As Boolean
			Return this.IsPassword AndAlso Regex.IsMatch(this, "[0-9A-Z]")
		End Function

		''' <summary>判断密码是否有效（6-20位长度；必须包含数字；必须包含小写或大写字母；必须包含特殊符号）</summary> 
		<Extension>
		Public Function IsPasswordComplex(this As String) As Boolean
			Return this.IsPasswordNumberLetter AndAlso Regex.IsMatch(this, "[\x21-\x7e]")
		End Function

		''' <summary>判断是否网址</summary> 
		<Extension>
		Public Function IsUrl(this As String) As Boolean
			If this.NotEmpty Then
				Try
					Dim Url = New Uri(this)
					Select Case Url.Scheme
						Case "http", "https", "ftp"
							Return Not Url.Host.IsEmpty
					End Select
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		''' <summary>判断MD5Hash</summary>
		''' <param name="isLong">True：32位； False：16位</param>
		<Extension>
		Public Function IsMD5Hash(this As String, Optional isLong As Boolean = True) As Boolean
			If isLong Then
				Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[0-9a-fA-F]{32,32}$", RegexOptions.IgnorePatternWhitespace)
			Else
				Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[0-9a-fA-F]{16,16}$", RegexOptions.IgnorePatternWhitespace)
			End If
		End Function

		''' <summary>路径是否合法，不能否含有“/\&gt;>:.?*|$]”特殊字符</summary> 
		<Extension>
		Public Function IsPath(this As String) As Boolean
			' 非法字符有：:<>?*"|\/
			' Return This.IndexOfAny(IO.Path.GetInvalidFileNameChars)
			Return this.NotEmpty AndAlso Not Regex.IsMatch(this, "[/\\:\*\?""<>\|]")
		End Function

		''' <summary>验输入字符串是否全为中文/日文/韩文字符</summary> 
		<Extension>
		Public Function IsChinese(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[\u4E00-\u9FA5]+$")
		End Function

		''' <summary>验输入字符串是否全为 ASCII 字符</summary> 
		<Extension>
		Public Function IsAscii(this As String) As Boolean
			Return this.NotEmpty AndAlso Regex.IsMatch(this, "^[\x00-\x7F]+$")
		End Function

		''' <summary>验输入字符串是否为车牌</summary> 
		<Extension>
		Public Function IsCar(this As String) As Boolean
			Return this.ToCar.NotNull
		End Function

		''' <summary>验输入字符串是否可以转换成时间</summary> 
		<Extension>
		Public Function IsDateTime(this As String) As Boolean
			Return this.NotEmpty AndAlso Date.TryParse(this, Nothing)
		End Function

		''' <summary>验输入字符串是否可以转换成时间</summary>
		''' <param name="sp">字符串中日期分隔符</param>
		<Extension>
		Public Function IsDate(this As String, Optional sp As String = "-") As Boolean
			If this.NotEmpty AndAlso this.Length > 5 AndAlso this.Length < 9 Then
				sp = sp.EmptyValue("-")
				sp = Regex.Escape(sp)

				Return Regex.IsMatch(this, "^\d{2,4}" + sp + "\d{1,2}" + sp + "\d{1,2}")
			Else
				Return False
			End If

		End Function

		''' <summary>验输入字符串是否可以转换成时间</summary> 
		<Extension>
		Public Function IsTime(this As String) As Boolean
			Return this.NotEmpty AndAlso this.Length > 4 AndAlso this.Length < 9 AndAlso Regex.IsMatch(this, "^\d{1,2}:\d{1,2}:\d{1,2}")
		End Function

		''' <summary>验输入字符串是否有效的 JSON</summary> 
		<Extension>
		Public Function IsJson(this As String) As Boolean
			If this.NotEmpty Then
				Return this.ToJsonCollection.Value IsNot Nothing
			End If

			Return False
		End Function

		''' <summary>验输入字符串是否有效的XML内容</summary> 
		<Extension>
		Public Function IsXml(this As String) As Boolean
			If this.NotEmpty Then
				Dim Doc As New Xml.XmlDocument
				Doc.LoadXml("<xml />")

				Try
					Doc.DocumentElement.InnerXml = this
					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

#End Region

#Region "7. 转换格式"

		''' <summary>转换成全角</summary>
		<Extension>
		Public Function ToSBC(this As String) As String
			If this.IsNull Then Return ""

			Dim arr = this.ToCharArray()
			For I = 0 To arr.Length - 1
				Dim C As Integer = AscW(arr(I))
				If C = 32 Then
					arr(I) = ChrW(12288)
				ElseIf C < 127 Then
					arr(I) = ChrW(C + 65248)
				End If
			Next
			Return New String(arr)
		End Function

		''' <summary>转换成半角</summary>
		<Extension>
		Public Function ToDBC(this As String) As String
			If this.IsNull Then Return ""

			Dim arr As Char() = this.ToCharArray()
			For I = 0 To arr.Length - 1
				Dim C As Integer = AscW(arr(I))
				If C = 12288 Then
					arr(I) = ChrW(32)
				ElseIf C > 65280 And C < 65375 Then
					arr(I) = ChrW(C - 65248)
				End If
			Next
			Return New String(arr)
		End Function

		''' <summary>转换为 Boolean</summary>
		<Extension>
		Public Function ToBoolean(this As String) As Boolean
			If this.IsEmpty Then Return False

			Select Case this.ToDBC.Trim.ToLower
				Case "1", "true", "yes", "ok", "success", "on", "right", "good", "big", "more", "long", "wide", "tall", "high", "fat", "是", "有", "真", "对", "好", "正确", "女", "大", "多", "高", "长", "宽", "胖"
					Return True
				Case Else
					Return False
			End Select
		End Function

		''' <summary>转换为 TriState 三态</summary>
		<Extension>
		Public Function ToTriState(this As String) As TristateEnum
			If this.IsEmpty Then Return TristateEnum.DEFAULT

			this = this.ToDBC.Trim
			Select Case this.ToLower
				Case "false", "no", "err", "error", "off", "left", "bad", "非", "无", "假", "错", "坏", "错误", "男"
					Return TristateEnum.FALSE
				Case "true", "yes", "ok", "success", "on", "right", "good", "是", "有", "真", "对", "好", "正确", "女"
					Return TristateEnum.TRUE
				Case "usedefault", "default", "other", "unknow", "unknown", "默认", "未知", "其他", "待定"
					Return TristateEnum.DEFAULT
				Case Else
					Select Case this.ToInteger(False)
						Case Is < 0
							Return TristateEnum.FALSE
						Case Is > 0
							Return TristateEnum.TRUE
						Case Else
							Return TristateEnum.DEFAULT
					End Select
			End Select
		End Function

		''' <summary>转换为 DateTime</summary>
		''' <param name="This">6位：yyMMdd / 8位：yyyyMMdd / 10,13位：Js Timer / 12位：yyMMddHHmmss / 14位：yyyyMMddHHmmss / 其他数字转换成Timespan / 字符则系统自动转换</param>
		<Extension>
		Public Function ToDateTime(this As String, Optional defaultDate As Date = Nothing) As Date
			Dim R = defaultDate
			If this.IsEmpty Then Return R

			this = this.ToDBC.Trim

			' 检查是否全数字，全数字按长度分析，否则直接由系统转换
			If this.IsUInt Then
				' 纯数字
				Select Case this.Length
					Case 6, 8, 12, 14
						Dim Y As Integer = defaultDate.Year
						Dim M As Integer = defaultDate.Month
						Dim D As Integer = defaultDate.Day
						Dim hh As Integer = defaultDate.Hour
						Dim mm As Integer = defaultDate.Minute
						Dim ss As Integer = defaultDate.Second

						Select Case this.Length
							Case 6, 12
								Y = 2000 + this.Substring(0, 2).ToInteger(False)
								M = this.Substring(2, 2).ToInteger(False)
								D = this.Substring(4, 2).ToInteger(False)
							Case 8, 14
								Y = this.Substring(0, 4).ToInteger(False)
								M = this.Substring(4, 2).ToInteger(False)
								D = this.Substring(6, 2).ToInteger(False)
						End Select

						Select Case this.Length
							Case 12
								hh = this.Substring(6, 2).ToInteger(False)
								mm = this.Substring(8, 2).ToInteger(False)
								ss = this.Substring(10, 2).ToInteger(False)
							Case 14
								hh = this.Substring(8, 2).ToInteger(False)
								mm = this.Substring(10, 2).ToInteger(False)
								ss = this.Substring(12, 2).ToInteger(False)
						End Select

						Y = Y.Range(1, 9999)
						M = M.Range(1, 12)
						D = D.Range(1, 31)
						hh = hh.Range(0, 59)
						mm = mm.Range(0, 59)
						ss = ss.Range(0, 59)

						R = New Date(Y, M, D, hh, mm, ss)
					Case 10, 13
						Dim t As Long = (this & "00000000000000000").Substring(0, 17).ToLong(False)
						R = TimeZoneInfo.ConvertTimeFromUtc(New Date(1970, 1, 1), TimeZoneInfo.Local).Add(New TimeSpan(t))
					Case Else
						R = New DateTime(this.ToLong(False))
				End Select
			Else
				Try
					R = Convert.ToDateTime(this)
				Catch ex As Exception
					R = defaultDate
				End Try
			End If

			Return R
		End Function

		''' <summary>调整日期字符串组，将重复或者不规范的日期过滤</summary>
		''' <param name="This">日期数据，格式：YYYY-MM-dd 多个用逗号间隔</param>
		''' <remarks>2016-09-25</remarks>
		<Extension>
		Public Function ToDateList(this As String, Optional defaultDate As Date = Nothing) As List(Of Date)
			If this.IsEmpty Then Return Nothing

			Dim Days As New List(Of Date)
			For Each d In this.SplitDistinct({",", "，", ";", "；"})
				Dim Dt As Date = d.ToDateTime(defaultDate)
				If Not Days.Contains(Dt) Then Days.Add(Dt)
			Next
			Return Days
		End Function

		''' <summary>转换为数字，仅包含0-9，负号和小数点</summary>
		''' <param name="coverALL">是否转换所有字符，True：整个字符非数字全过滤并合并成一个数字，False：过滤后仅保留第一段数字</param>
		''' <param name="toDBC">是否将全角数字转换为半角数字</param>
		''' <param name="incluedPointer">是否包含小数点后的数，默认包含</param>
		<Extension>
		Public Function ToNumber(this As String, Optional coverALL As Boolean = False, Optional toDBC As Boolean = True, Optional incluedPointer As Boolean = True) As Decimal
			If this.NotEmpty Then
				' 全角数字转换为半角数字
				If toDBC Then this = this.ToDBC

				If coverALL Then
					this = Regex.Replace(this, "[^0-9\.\-\+]", "")

					' 正负号仅用于第一个字符
					If this.Contains("+"c) AndAlso Not this.StartsWith("+"c) Then this = this.Split("+"c)(0)

					' 去掉正号，因为数据转换后自动会变成正数
					this = this.Replace("+"c, "")

					' 正负号仅用于第一个字符
					If this.Contains("-"c) AndAlso Not this.StartsWith("-"c) Then this = this.Split("-"c)(0)
				Else
					this = Regex.Replace(this, "[^0-9\.\-]", " ")

					' 去掉首尾及多余的空格
					this = Regex.Replace(this, "(\s){2,}", " ").Trim

					' 取第一段数据
					If this.NotNull AndAlso this.Contains(" "c) Then this = this.Split(" "c)(0)

					' 过滤掉仅有负号的数据
					If this.Contains("-"c) AndAlso Not this.StartsWith("-"c) Then this = this.Split("-"c)(0).Trim
				End If

				' 只能保留一个小数点
				If this.Contains("."c) Then
					Dim dotArr = $"0{this}0".Split("."c)

					'' 首位为空则补零
					'If dotArr(0).IsEmpty Then dotArr(0) = 0

					this = dotArr(0) & "." & dotArr(1)
				End If
			End If

			If this.IsEmpty Then
				Return 0
			Else
				If Not incluedPointer AndAlso this.Contains("."c) Then this = this.Split("."c)(0)
				Return Convert.ToDecimal(this)
			End If
		End Function

		''' <summary>转换为 Double</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToDouble(this As String, Optional changeThis As Boolean = False) As Double
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToDouble(this.ToNumber)
				Else
					Return Double.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 Single</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToSingle(this As String, Optional changeThis As Boolean = False) As Single
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToSingle(this.ToNumber)
				Else
					Return Single.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 Int64</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToLong(this As String, Optional changeThis As Boolean = False) As Long
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToInt64(this.ToNumber)
				Else
					Return Long.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 Int32</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToInteger(this As String, Optional changeThis As Boolean = False) As Integer
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToInt32(this.ToNumber)
				Else
					Return Integer.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 Int16</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToShort(this As String, Optional changeThis As Boolean = False) As Short
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToInt16(this.ToNumber)
				Else
					Return Short.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 Char，将字符串中的数字转换成十进制数进行转换</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToChar(this As String, Optional changeThis As Boolean = False) As Char
			If this.IsEmpty Then this = "0"
			this.IsNumber
			Try
				If changeThis Then
					Return Convert.ToChar(this.ToNumber)
				Else
					Return Char.Parse(this)
				End If
			Catch ex As Exception
				Return New Char
			End Try
		End Function

		''' <summary>转换为 Byte</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToByte(this As String, Optional changeThis As Boolean = False) As Byte
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToByte(this.ToNumber)
				Else
					Return Byte.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 UInt64</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToULong(this As String, Optional changeThis As Boolean = False) As ULong
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToUInt64(this.ToNumber)
				Else
					Return ULong.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 UInt32</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToUInteger(this As String, Optional changeThis As Boolean = False) As UInteger
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToUInt32(this.ToNumber)
				Else
					Return UInteger.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 UInt16</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToUShort(this As String, Optional changeThis As Boolean = False) As UShort
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToUInt16(this.ToNumber)
				Else
					Return UShort.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换为 SByte</summary>
		''' <param name="changeThis">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToSByte(this As String, Optional changeThis As Boolean = False) As SByte
			If this.IsEmpty Then Return 0

			Try
				If changeThis Then
					Return Convert.ToSByte(this.ToNumber)
				Else
					Return SByte.Parse(this)
				End If
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>转换 GUID</summary>
		<Extension>
		Public Function ToGuid(this As String, Optional changeThis As Boolean = False) As Guid
			If this.IsEmpty Then Return Guid.Empty

			this = If(changeThis, this.ToDBC.Replace("[^0-9a-fA-F\{\}\-]", ""), this)
			this = this.Trim

			Dim Ret As Guid
			If Guid.TryParse(this, Ret) Then
				Return Ret
			Else
				Return Guid.Empty
			End If
		End Function

		''' <summary>转换成有效的车牌</summary>
		''' <remarks>
		''' 1、传统车牌
		''' 第1位为省份简称（汉字），第二位为发牌机关代号（A-Z的字母）第3到第7位为序号（由字母或数字组成，但不存在字母I和O，防止和数字1、0混淆，另外最后一位可能是“挂学警港澳使领”中的一个汉字）。
		''' 
		''' 2、新能源车牌
		''' 第1位和第2位与传统车牌一致，第3到第8位为序号（比传统车牌多一位）。新能源车牌的序号规则如下：
		''' 小型车：第1位只能是字母D或F，第2为可以是数字或字母，第3到6位必须是数字。
		''' 大型车：第1位到第5位必须是数字，第6位只能是字母D或F。
		''' 
		''' ^(([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-Z](([0-9]{5}[DF])|([DF]([A-HJ-NP-Z0-9])[0-9]{4})))|([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-Z][A-HJ-NP-Z0-9]{4}[A-HJ-NP-Z0-9挂学警港澳使领]))$
		''' 
		''' </remarks>
		<Extension>
		Public Function ToCar(this As String) As String
			this = this.ToDBC.TrimFull
			If Not this.IsEmpty Then
				this = this.Replace(" ", "").Replace("O", "0").Replace("I", 1).Replace(".", "").Replace("·", "").Replace("﹐", "")
				this = this.ToUpper

				If this.Length = 7 Or this.Length = 8 Then
					If Regex.IsMatch(this, "^(([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-Z](([0-9]{5}[DF])|([DF]([A-HJ-NP-Z0-9])[0-9]{4})))|([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-Z][A-HJ-NP-Z0-9]{4}[A-HJ-NP-Z0-9挂学警港澳使领]))$", Text.RegularExpressions.RegexOptions.IgnoreCase) Then
						Return String.Concat(this.AsSpan(0, 2), "·", this.AsSpan(2))
					End If
				End If
			End If

			Return ""
		End Function

		''' <summary>获取有效的电话字符</summary>
		<Extension>
		Public Function ToPhone(this As String) As String
			If this.IsEmpty Then
				Return ""
			Else
				Return Regex.Replace(this, "[^0-9\.\-\+\(\) ]", "")
			End If
		End Function

		''' <summary>获取有效的手机号码</summary>
		<Extension>
		Public Function ToMobilePhone(this As String) As String
			If this.IsMobilePhone Then
				Return this
			Else
				Return ""
			End If
		End Function

		''' <summary>转换成文件路径，去除无效的字符</summary>
		<Extension>
		Public Function ToPath(this As String) As String
			If this.NotEmpty Then
				For Each C In IO.Path.GetInvalidPathChars
					this = this.Replace(C, "")
				Next

				Return this.TrimFull
			Else
				Return ""
			End If
		End Function

		''' <summary>转换成文件名，去除无效的字符</summary>
		<Extension>
		Public Function ToFileName(this As String) As String
			If this.NotEmpty Then
				For Each C In IO.Path.GetInvalidFileNameChars
					this = this.Replace(C, "")
				Next

				Return this.TrimFull
			Else
				Return ""
			End If
		End Function

		''' <summary>转换成URL路径</summary>
		<Extension>
		Public Function ToUrl(this As String) As String
			Try
				Return New Uri(this).AbsoluteUri
			Catch ex As Exception
				Return ""
			End Try
		End Function

		''' <summary>大写转换成小写驼峰</summary>
		<Extension>
		Public Function ToCamelCase(this As String, Optional isLine As Boolean = False) As String
			If this.NotEmpty Then
				this = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this)
				this = String.Concat(this.Substring(0, 1).ToLower, this.AsSpan(1))

				If isLine Then
					With New Text.StringBuilder
						For Each c In this
							' 大写则添加横线
							If c >= "A"c AndAlso c <= "Z"c Then .Append("-"c)
							.Append(c)
						Next

						this = .ToString
					End With
				End If
			End If

			Return this
		End Function

		''' <summary>转换成有效的XML字符串</summary>
		<Extension>
		Public Function ToXML(this As String) As String
			Dim Doc As New Xml.XmlDocument
			Doc.LoadXml("<xml />")

			Try
				Doc.DocumentElement.InnerXml = this
			Catch ex As Exception
				Doc.DocumentElement.InnerText = this
			End Try

			Return Doc.DocumentElement.InnerXml
		End Function

#Region "拼音"

		Private ReadOnly _PYV As Integer() = New Integer() {-20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, -20051, -20036, -20032, -20026, -20002, -19990, -19986, -19982, -19976, -19805, -19784, -19775, -19774, -19763, -19756, -19751, -19746, -19741, -19739, -19728, -19725, -19715, -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, -19289, -19288, -19281, -19275, -19270, -19263, -19261, -19249, -19243, -19242, -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018, -19006, -19003, -18996, -18977, -18961, -18952, -18783, -18774, -18773, -18763, -18756, -18741, -18735, -18731, -18722, -18710, -18697, -18696, -18526, -18518, -18501, -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, -18211, -18201, -18184, -18183, -18181, -18012, -17997, -17988, -17970, -17964, -17961, -17950, -17947, -17931, -17928, -17922, -17759, -17752, -17733, -17730, -17721, -17703, -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, -17468, -17454, -17433, -17427, -17417, -17202, -17185, -16983, -16970, -16942, -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474, -16470, -16465, -16459, -16452, -16448, -16433, -16429, -16427, -16423, -16419, -16412, -16407, -16403, -16401, -16393, -16220, -16216, -16212, -16205, -16202, -16187, -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, -15915, -15903, -15889, -15878, -15707, -15701, -15681, -15667, -15661, -15659, -15652, -15640, -15631, -15625, -15454, -15448, -15436, -15435, -15419, -15416, -15408, -15394, -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, -15165, -15158, -15153, -15150, -15149, -15144, -15143, -15141, -15140, -15139, -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933, -14930, -14929, -14928, -14926, -14922, -14921, -14914, -14908, -14902, -14894, -14889, -14882, -14873, -14871, -14857, -14678, -14674, -14670, -14668, -14663, -14654, -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, -14353, -14345, -14170, -14159, -14151, -14149, -14145, -14140, -14137, -14135, -14125, -14123, -14122, -14112, -14109, -14099, -14097, -14094, -14092, -14090, -14087, -14083, -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, -13878, -13870, -13859, -13847, -13831, -13658, -13611, -13601, -13406, -13404, -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356, -13343, -13340, -13329, -13326, -13318, -13147, -13138, -13120, -13107, -13096, -13095, -13091, -13076, -13068, -13063, -13060, -12888, -12875, -12871, -12860, -12858, -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594, -12585, -12556, -12359, -12346, -12320, -12300, -12120, -12099, -12089, -12074, -12067, -12058, -12039, -11867, -11861, -11847, -11831, -11798, -11781, -11604, -11589, -11536, -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, -11055, -11052, -11045, -11041, -11038, -11024, -11020, -11019, -11018, -11014, -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544, -10533, -10519, -10331, -10329, -10328, -10322, -10315, -10309, -10307, -10296, -10281, -10274, -10270, -10262, -10260, -10256, -10254}

		Private ReadOnly _PYS As String() = New String() {"A", "Ai", "An", "Ang", "Ao", "Ba", "Bai", "Ban", "Bang", "Bao", "Bei", "Ben", "Beng", "Bi", "Bian", "Biao", "Bie", "Bin", "Bing", "Bo", "Bu", "Ca", "Cai", "Can", "Cang", "Cao", "Ce", "Ceng", "Cha", "Chai", "Chan", "Chang", "Chao", "Che", "Chen", "Cheng", "Chi", "Chong", "Chou", "Chu", "Chuai", "Chuan", "Chuang", "Chui", "Chun", "Chuo", "Ci", "Cong", "Cou", "Cu", "Cuan", "Cui", "Cun", "Cuo", "Da", "Dai", "Dan", "Dang", "Dao", "De", "Deng", "Di", "Dian", "Diao", "Die", "Ding", "Diu", "Dong", "Dou", "Du", "Duan", "Dui", "Dun", "Duo", "E", "En", "Er", "Fa", "Fan", "Fang", "Fei", "Fen", "Feng", "Fo", "Fou", "Fu", "Ga", "Gai", "Gan", "Gang", "Gao", "Ge", "Gei", "Gen", "Geng", "Gong", "Gou", "Gu", "Gua", "Guai", "Guan", "Guang", "Gui", "Gun", "Guo", "Ha", "Hai", "Han", "Hang", "Hao", "He", "Hei", "Hen", "Heng", "Hong", "Hou", "Hu", "Hua", "Huai", "Huan", "Huang", "Hui", "Hun", "Huo", "Ji", "Jia", "Jian", "Jiang", "Jiao", "Jie", "Jin", "Jing", "Jiong", "Jiu", "Ju", "Juan", "Jue", "Jun", "Ka", "Kai", "Kan", "Kang", "Kao", "Ke", "Ken", "Keng", "Kong", "Kou", "Ku", "Kua", "Kuai", "Kuan", "Kuang", "Kui", "Kun", "Kuo", "La", "Lai", "Lan", "Lang", "Lao", "Le", "Lei", "Leng", "Li", "Lia", "Lian", "Liang", "Liao", "Lie", "Lin", "Ling", "Liu", "Long", "Lou", "Lu", "Lv", "Luan", "Lue", "Lun", "Luo", "Ma", "Mai", "Man", "Mang", "Mao", "Me", "Mei", "Men", "Meng", "Mi", "Mian", "Miao", "Mie", "Min", "Ming", "Miu", "Mo", "Mou", "Mu", "Na", "Nai", "Nan", "Nang", "Nao", "Ne", "Nei", "Nen", "Neng", "Ni", "Nian", "Niang", "Niao", "Nie", "Nin", "Ning", "Niu", "Nong", "Nu", "Nv", "Nuan", "Nue", "Nuo", "O", "Ou", "Pa", "Pai", "Pan", "Pang", "Pao", "Pei", "Pen", "Peng", "Pi", "Pian", "Piao", "Pie", "Pin", "Ping", "Po", "Pu", "Qi", "Qia", "Qian", "Qiang", "Qiao", "Qie", "Qin", "Qing", "Qiong", "Qiu", "Qu", "Quan", "Que", "Qun", "Ran", "Rang", "Rao", "Re", "Ren", "Reng", "Ri", "Rong", "Rou", "Ru", "Ruan", "Rui", "Run", "Ruo", "Sa", "Sai", "San", "Sang", "Sao", "Se", "Sen", "Seng", "Sha", "Shai", "Shan", "Shang", "Shao", "She", "Shen", "Sheng", "Shi", "Shou", "Shu", "Shua", "Shuai", "Shuan", "Shuang", "Shui", "Shun", "Shuo", "Si", "Song", "Sou", "Su", "Suan", "Sui", "Sun", "Suo", "Ta", "Tai", "Tan", "Tang", "Tao", "Te", "Teng", "Ti", "Tian", "Tiao", "Tie", "Ting", "Tong", "Tou", "Tu", "Tuan", "Tui", "Tun", "Tuo", "Wa", "Wai", "Wan", "Wang", "Wei", "Wen", "Weng", "Wo", "Wu", "Xi", "Xia", "Xian", "Xiang", "Xiao", "Xie", "Xin", "Xing", "Xiong", "Xiu", "Xu", "Xuan", "Xue", "Xun", "Ya", "Yan", "Yang", "Yao", "Ye", "Yi", "Yin", "Ying", "Yo", "Yong", "You", "Yu", "Yuan", "Yue", "Yun", "Za", "Zai", "Zan", "Zang", "Zao", "Ze", "Zei", "Zen", "Zeng", "Zha", "Zhai", "Zhan", "Zhang", "Zhao", "Zhe", "Zhen", "Zheng", "Zhi", "Zhong", "Zhou", "Zhu", "Zhua", "Zhuai", "Zhuan", "Zhuang", "Zhui", "Zhun", "Zhuo", "Zi", "Zong", "Zou", "Zu", "Zuan", "Zui", "Zun", "Zuo"}

		''' <summary>汉字转拼音</summary>
		''' <param name="This">汉字字符串</param>
		''' <param name="delAscii">是否删除所有Ascii字符</param>
		''' <param name="OnlyFirstChar">只返回拼音首字母</param>
		<Extension>
		Public Function ToPinYin(this As String, Optional delAscii As Boolean = True, Optional onlyFirstChar As Boolean = False) As String
			If this.IsEmpty Then Return ""

			With New Text.StringBuilder
				For Each C In this
					Dim Bs As Byte() = GB2312.GetBytes(C)
					Dim Code As Integer

					If Bs?.Length > 1 Then
						Code = (Bs(0) * 256) + Bs(1) - 65536
					Else
						Code = Bs(0)
					End If

					' 英文
					If Code > 0 Then
						.Append(If(delAscii, " ", C))
					Else
						' 中文
						For J As Integer = _PYV.Length - 1 To 0 Step -1
							If _PYV(J) <= Code Then
								.Append(If(onlyFirstChar, _PYS(J).Substring(0, 1), _PYS(J)))
								Exit For
							End If
						Next
					End If
				Next

				Return .ToString
			End With
		End Function

#End Region

#End Region

#Region "8. 获取格式化内容"

		''' <summary>获取 ASCII</summary>
		''' <param name="This">要操作的字符串</param>
		<Extension>
		Public Function GetAscii(this As String) As String
			If this.IsNull Then Return ""
			Return Regex.Replace(this, "[^\x21-\x7E]", "").Trim
		End Function

		''' <summary>获取任意字母，数字，下划线，汉字的字符</summary>
		''' <param name="This">要操作的字符串</param>
		<Extension>
		Public Function GetChars(this As String) As String
			If this.IsNull Then Return ""
			Return Regex.Replace(this, "[\W]", "").Trim
		End Function

		''' <summary>获取汉字的字符</summary>
		''' <param name="This">要操作的字符串</param>
		<Extension>
		Public Function GetChinese(this As String) As String
			If this.IsEmpty Then Return ""
			Return Regex.Replace(this, "[^\u4e00-\u9fa5]", "").Trim
		End Function

		''' <summary>获取所有大写字母</summary>
		''' <param name="This">要操作的字符串</param>
		<Extension>
		Public Function GetUpper(this As String) As String
			If this.IsEmpty Then Return ""
			Return Regex.Replace(this, "[^\x41-\x5A]", "").Trim
		End Function

		''' <summary>转换时间格式</summary>
		<Extension>
		Public Function GetDateTime(this As String, Optional strFormat As String = "") As String
			Return this.ToDateTime(SYS_NOW_DATE).ToString(strFormat.EmptyValue("yyyy-MM-dd HH:mm:ss"))
		End Function

		''' <summary>转换时间格式</summary>
		''' <param name="prefix">前缀</param>
		''' <param name="suffix">后缀</param>
		''' <remarks>仅支持日期格式字符串，参考：https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-date-and-time-format-strings</remarks>
		<Extension>
		Public Function GetDateTime(this As String, prefix As String, suffix As String, Optional dateAction As Date? = Nothing) As String
			If prefix.IsEmpty Then Return this
			If suffix.IsEmpty Then Return this

			If dateAction Is Nothing Then dateAction = SYS_NOW_DATE

			Dim pre = Regex.Escape(prefix)
			Dim suf = Regex.Escape(suffix)

			Dim matches = Regex.Matches(this, $"{pre}([a-z\:\-_年月日]*){suf}", RegexOptions.IgnoreCase)
			If matches.IsEmpty Then Return this

			' 处理重复数据
			Dim ms = matches.Select(Function(x) x.Groups(1).Value).Distinct.ToList
			If ms.IsEmpty Then Return this

			' 替换操作
			ms.ForEach(Sub(m)
						   Try
							   Dim v = dateAction.Value.ToString(m)
							   If v.NotEmpty Then this = this.Replace($"{prefix}{m}{suffix}", v)
						   Catch ex As Exception
						   End Try
					   End Sub)

			Return this
		End Function

		''' <summary>格式化日期类标题</summary>
		''' <param name="this">要格式化的内容</param>
		''' <param name="dateTimeName">标签前缀，使用时使用点间隔</param>
		''' <param name="dateAction">操作时间</param>
		''' <param name="dayBegin">提前计算天数</param>
		''' <param name="dayEnd">结束计算天数</param>
		<Extension>
		Public Function GetDateTime(this As String, dateAction As Date?, Optional dateTimeName As String = "", Optional dayBegin As Integer = -1, Optional dayEnd As Integer = 1) As String
			If this.IsEmpty OrElse dateAction Is Nothing Then Return this

			' 检查前缀是否存在
			Dim hasReplace = True
			If dateTimeName.NotEmpty Then
				dateTimeName = "[" & dateTimeName.ToLower & "."

				' 存在前缀，不区分大小写，替换成标准前缀
				If this.Contains(dateTimeName, StringComparison.OrdinalIgnoreCase) Then
					this = this.Replace(dateTimeName, dateTimeName, StringComparison.OrdinalIgnoreCase)
					dateTimeName = dateTimeName.Substring(1)
				Else
					' 默认需要替换，但是如果存在默认前缀时，但是内容中无前缀，则无需替换操作
					hasReplace = False
				End If
			ElseIf Not this.Contains("["c) OrElse Not this.Contains("]"c) Then
				' 不存在替换标签
				hasReplace = False
			End If

			' 需要替换操作
			If hasReplace Then
				Dim dateNow As Date = dateAction
				If dateNow < New Date(1900, 1, 1) Then dateNow = Date.Now

				Dim dS As Integer = Math.Min(dayBegin, dayEnd)
				Dim dE As Integer = Math.Max(dayBegin, dayEnd)

				For I As Integer = dS To dE
					Dim N = ""
					If I > 0 Then N = "+" & I
					If I < 0 Then N = I

					Dim d As Date = dateNow.AddDays(I)

					this = this.Replace("[" & dateTimeName & "YYYY" & N & "]", d.Year, StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "MM" & N & "]", d.ToString("MM"))
					this = this.Replace("[" & dateTimeName & "DD" & N & "]", d.ToString("dd"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "YY" & N & "]", d.ToString("yy"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "M" & N & "]", d.Month)
					this = this.Replace("[" & dateTimeName & "D" & N & "]", d.Day, StringComparison.OrdinalIgnoreCase)

					this = this.Replace("[" & dateTimeName & "hh" & N & "]", d.ToString("HH"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "mm" & N & "]", d.ToString("mm"))
					this = this.Replace("[" & dateTimeName & "ss" & N & "]", d.ToString("ss"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "h" & N & "]", d.Hour, StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "m" & N & "]", d.Minute)
					this = this.Replace("[" & dateTimeName & "s" & N & "]", d.Second, StringComparison.OrdinalIgnoreCase)

					this = this.Replace("[" & dateTimeName & "DATE" & N & "]", d.ToLongDateString)
					this = this.Replace("[" & dateTimeName & "TIME" & N & "]", d.ToLongTimeString)
					this = this.Replace("[" & dateTimeName & "date" & N & "]", d.ToShortDateString)
					this = this.Replace("[" & dateTimeName & "time" & N & "]", d.ToShortTimeString)

					Select Case d.DayOfWeek
						Case DayOfWeek.Monday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 1)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "一")
						Case DayOfWeek.Tuesday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 2)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "二")
						Case DayOfWeek.Wednesday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 3)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "三")
						Case DayOfWeek.Thursday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 4)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "四")
						Case DayOfWeek.Friday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 5)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "五")
						Case DayOfWeek.Saturday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 6)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "六")
						Case DayOfWeek.Sunday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 7)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "日")
					End Select
				Next

				this = this.Replace("[rnd]", dateNow.Ticks)
				this = this.Replace("[RND]", Guid.NewGuid.ToString)
			End If

			Return this
		End Function

		''' <summary>调整日期字符串组，将重复或者不规范的日期过滤</summary>
		''' <param name="this">日期数据，格式：YYYY-MM-dd 多个用逗号间隔</param>
		''' <param name="onlyDate">只返回日期部分</param>
		<Extension>
		Public Function GetDateList(this As String, Optional defaultDate As Date = Nothing, Optional onlyDate As Boolean = True) As String
			Dim Ds = this.ToDateList(defaultDate)
			Dim Df = If(onlyDate, "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss")
			If Ds?.Count > 0 Then
				Return String.Join(",", Ds.Select(Function(x) x.ToString(Df)))
			Else
				Return ""
			End If
		End Function

		''' <summary>分析并调整关键词类数组组合成的字符串，防止其超过最大长度</summary>
		<Extension>
		Public Function GetArrayString(this As String, Optional maxLength As Integer = Integer.MaxValue, Optional separator As String = Nothing) As String
			If this.IsEmpty Or maxLength < 1 Then Return ""

			Dim joinString = separator.NullValue(",")

			Dim list = this.SplitDistinct(separator)
			If list.IsEmpty Then Return ""

			Array.Sort(list)

			Dim ret = list.JoinString(joinString)
			If ret.Length > maxLength Then
				Dim Last = ret.LastIndexOf(joinString, maxLength)
				If Last > 0 Then
					ret = ret.Substring(0, Last)
				Else
					ret = ret.Substring(0, maxLength)
				End If
			End If

			Return Ret
		End Function


		''' <summary>分析并调整关键词类数组组合成的字符串，防止其超过最大长度</summary>
		''' <remarks>一个标准 GUID 为36个字节</remarks>
		<Extension>
		Public Function GetGuidString(this As String, Optional maxLength As Integer = Integer.MaxValue, Optional separator As String = "") As String
			If this.IsEmpty Or maxLength < 36 Then Return ""

			Dim Arr = this.ToGuidList(separator)
			If Arr.NotEmpty Then
				separator = separator.NullValue(",")

				Dim len = 36 + separator.Length
				Dim max As Integer = Math.Floor(maxLength / len)

				Return Arr.Take(max).JoinString(separator)
			Else
				Return ""
			End If
		End Function

#End Region

#Region "0. 公共调用内置操作"

		''' <summary>更新当前正则参数，并返回参数类型</summary>
		''' <param name="this">要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换"）</param>
		''' <returns>
		''' 1. 如果括号起始表示为正则表达参数
		''' 2. 包含 [*] 字符串表示需要替换参数的正则参数
		''' 3. 其他类型为通用字符串
		''' </returns>
		Private Function PatternUpdate(this As String) As (IsPattern As Boolean, Pattern As String)
			If this.NotEmpty AndAlso this.Length > 2 Then
				If this.StartsWith("(") AndAlso this.EndsWith(")") AndAlso this.Length > 2 Then
					this = this.Substring(1, this.Length - 2)
					Return (True, this)
				ElseIf this.Contains("[*]") Then
					this = Regex.Escape(this)
					this = this.Replace("\[\*]", "((.|\n)*?)")
					Return (True, this)
				End If
			Else
				If this.IsNull Then this = ""
			End If

			Return (False, this)
		End Function

#End Region

	End Module
End Namespace
