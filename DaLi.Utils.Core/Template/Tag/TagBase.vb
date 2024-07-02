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
' 	标签基类
'
' 	name: Tag.TagBase
' 	create: 2020-04-18
' 	memo: 标签基类
' 	
' ------------------------------------------------------------

Imports System.Data
Imports System.Text.RegularExpressions

Namespace Template.Tag

	''' <summary>标签基类</summary>
	Public MustInherit Class TagBase

		''' <summary>标签前缀</summary>
		Public Shared PREFIX As String = ""

		''' <summary>页面默认编码</summary>
		Public Shared CHARSET As Text.Encoding = Text.Encoding.UTF8

#Region "属性"

		''' <summary>标签名称</summary>
		Public Property Name As String

		''' <summary>原始内容</summary>
		Public Property Raw As String

		''' <summary>原始内容的HASH</summary>
		Public ReadOnly Property RawHash As String
			Get
				Return Raw.MD5
			End Get
		End Property

		''' <summary>属性列表</summary>
		Public ReadOnly Property Attributes As New NameValueDictionary

		''' <summary>属性列表</summary>
		Public ReadOnly Property Attribute(name As String) As String
			Get
				Return Attributes(name)
			End Get
		End Property

#End Region

		''' <summary>分析属性</summary>
		Protected Sub GetAttributes(strAttrs As String)
			Attributes.Clear()

			' Attributes 属性分析
			Dim MatchCollection = Regex.Matches(strAttrs, "([^\s=]+)=(['""\s]?)([^'""]+)\2(?=\s|$|>)")
			For Each Match As Match In MatchCollection
				Attributes(Match.Groups(1).Value) = Match.Groups(3).Value.DecodeHtml
			Next

			' 再次匹配，以保证双引号内单引号数据能正常选中，如 a="b'c'd"
			MatchCollection = Regex.Matches(strAttrs, "([^\s=]+)=([""\s]?)([^""]+)\2(?=\s|$|>)")
			For Each Match As Match In MatchCollection
				If String.IsNullOrEmpty(Attributes(Match.Groups(1).Value)) Then Attributes(Match.Groups(1).Value) = Match.Groups(3).Value.DecodeHtml
			Next
		End Sub

		''' <summary>替换指定内容</summary>
		''' <param name="template">原始模板数据</param>
		''' <param name="tagValue">标签值</param>
		''' <param name="attrReplace">扩展属性替换操作</param>
		Public Function Replace(template As String, tagValue As String, Optional attrReplace As Func(Of NameValueDictionary, String, String） = Nothing) As String
			tagValue = AttributesReplace(tagValue, Attributes, attrReplace)
			Return template.Replace(Raw, tagValue)
		End Function

		''' <summary>通用属性替换</summary>
		Public Shared Function AttributesReplace(value As String, attrs As NameValueDictionary, Optional replaceFunc As Func(Of NameValueDictionary, String, String） = Nothing) As String
			If attrs?.Count > 0 AndAlso Not String.IsNullOrEmpty(value) Then
				CHARSET = If(CHARSET, Text.Encoding.UTF8)

				'清理 Html
				Dim strClear = attrs("clear")
				If Not String.IsNullOrEmpty(strClear) Then
					Select Case strClear.ToLower
						Case "trim"
							If Not String.IsNullOrEmpty(value) Then value = value.Trim

						Case "trimall"
							value = value.TrimFull

						Case "space"
							value = value.ClearSpace

						Case "control"
							value = value.ClearControl

						Case Else
							value = value.ClearHtml(strClear)
					End Select
				End If

				'替换
				'为防止标签替换产生异常，正在表达式中 {} 由 <> 代替
				Dim strReplace = attrs("replace")
				Dim strTo = attrs("to")
				If Not String.IsNullOrEmpty(strReplace) Then
					If strReplace.StartsWith("(") AndAlso strReplace.EndsWith(")") Then strReplace = strReplace.Replace("<", "{").Replace(">", "}")
					value = value.ReplaceRegex(strReplace, strTo)
				End If

				' 截取，不包含本身
				Dim strCut = attrs("cut")
				If strCut.NotEmpty Then value = value.Cut(strCut, 1, False)

				' 截取，包含本身
				Dim strCuts = attrs("cuts")
				If strCuts.NotEmpty Then value = value.Cut(strCut, 0, False)

				'截取左侧字符串
				Dim strLeft = attrs("left").ToInteger
				If strLeft > 0 Then value = value.Left(strLeft)

				'截取中间字符串 substring(a,b)
				Dim strMid = attrs("mid").EmptyValue(attrs("middle"))
				If Not String.IsNullOrEmpty(strMid) Then
					Dim ns = (strMid & ",0").Split(","c)
					Dim na = ns(0).ToInteger
					Dim nb = ns(1).ToInteger
					If na >= 0 Then
						If nb > 0 Then
							value = value.Substring(na, nb)
						Else
							value = value.Substring(na)
						End If
					End If
				End If

				'截取右侧字符串
				Dim strRight = attrs("right").ToInteger
				If strRight > 0 Then value = value.Right(strRight)
				attrs.Remove("right")

				' 随机码，数字结果加上这个值，文字结果附加此值
				Dim strRnd = attrs("rnd")
				If Not String.IsNullOrEmpty(strRnd) Then
					If value.IsNumber Then
						'当前值为数字
						value = value.ToNumber + strRnd.ToNumber(False)

					ElseIf value.IsDateTime Then
						'当前为时间，则天与秒都加上此数字
						Dim rnd = strRnd.ToInteger(False)
						If rnd <> 0 Then value = value.ToDateTime.AddDays(rnd).AddMinutes(rnd)

					Else
						'文本，Xor 异或加密
						value = value.XorCoder(strRnd)
					End If
				End If

				' 从字典中获取值
				' k1:v1|k2:v2|……|kn:Vn
				Dim strFrom = attrs("from")
				If Not String.IsNullOrEmpty(strFrom) Then
					Dim Ls = strFrom.DecodeHtml.SplitDistinct("|")
					If Ls.NotEmpty Then
						Dim Nvs As New NameValueDictionary

						For Each L In Ls
							If Not String.IsNullOrWhiteSpace(L) AndAlso L.Contains(":"c) Then
								Dim P = L.IndexOf(":"c)
								If P > 0 Then
									Dim K = L.Substring(0, P).Trim
									Dim V = L.Substring(P + 1).Trim

									If Not String.IsNullOrEmpty(K) Then
										Nvs(K) = V.DecodeLine
									End If
								End If
							End If
						Next

						If Nvs.ContainsKey(value) Then value = Nvs(value)
					End If
				End If

				' 强制格式化成日期及其相关操作
				' 时间值最后一个字符为操作类型：s秒 i分钟 h小时 d日 m月 y年，默认：日
				Dim strDate = attrs("date")
				Dim strDateValue = attrs("delay")
				If Not String.IsNullOrEmpty(strDate) Then
					Dim Dt = value.ToDateTime(New Date(2000, 1, 1))

					If strDateValue.NotEmpty Then
						Dim v = strDateValue.ToInteger(True)
						Dim t = strDateValue.Substring(strDateValue.Length - 1)

						Select Case t.ToLower
							Case "s"
								Dt = Dt.AddSeconds(v)
							Case "i"
								Dt = Dt.AddMinutes(v)
							Case "h"
								Dt = Dt.AddHours(v)
							Case "m"
								Dt = Dt.AddMonths(v)
							Case "y"
								Dt = Dt.AddYears(v)
							Case Else
								Dt = Dt.AddDays(v)
						End Select
					End If

					value = Dt.ToString(strDate)
				End If

				' 作为日期计算参数，强制转换成日期
				' 支持 year month day hour minute second
				' now 为 计算的时间，如果未设置则使用当前时间
				Dim strNow = attrs("now")
				If strNow.NotEmpty Then value = ReplaceDate(value, strNow.ToDateTime(SYS_NOW_DATE), attrs)

				'格式化，可以加随机码干扰，对于数字会自动加上这个值，对于文字则直接附加此数据
				Dim strFormat = attrs("format")
				If strFormat.NotEmpty Then
					Try
						Select Case strFormat.ToLower
							Case "date"
								value = value.GetDateTime("yyyy-MM-dd")
							Case "time"
								value = value.GetDateTime("HH:mm:ss")
							Case "datetime"
								value = value.GetDateTime("yyyy-MM-dd HH:mm:ss")
							Case "ticks"
								value = value.ToDateTime.Ticks
							Case "jsticks"
								value = value.ToDateTime.JsTicks
							Case "unixticks"
								value = value.ToDateTime.UnixTicks

							Case "num", "number", "decimal"
								value = value.ToNumber
							Case "int", "integer"
								value = value.ToInteger
							Case "long"
								value = value.ToLong
							Case "size"
								value = value.ToDouble.ToSize

							Case "ucase", "upper"
								value = value.ToUpper
							Case "lcase", "lower"
								value = value.ToLower
							Case "pinyin"
								value = value.ToPinYin
							Case "ascii"
								value = value.GetAscii
							Case "dbc"
								value = value.ToDBC
							Case "sbc"
								value = value.ToSBC

							Case "car"
								value = value.ToCar
							Case "phone"
								value = value.ToPhone
							Case "mobi", "mobile", "mobilephone"
								value = value.ToMobilePhone

							Case "len"
								' 文字长度
								value = value.ToString.Length

							Case Else
								If value.IsDateTime Then
									value = value.GetDateTime(strFormat)
								ElseIf value.IsNumber Then
									value = value.ToNumber.ToString(strFormat)
								Else
									value = String.Format(strFormat, value)
								End If
						End Select
					Catch ex As Exception
					End Try
				End If

				' 数组
				Dim strArray = attrs("array")
				Dim strChar = attrs("char")
				If strArray.NotEmpty Then
					Dim List = value.SplitDistinct(strChar)
					If List?.Length > 0 Then
						If Not strArray.Contains("[*]") Then strArray &= "[*]"

						For I As Integer = 0 To List.Length - 1
							List(I) = List(I).TrimFull
							List(I) = strArray.Replace("[*]", List(I))
						Next

						value = String.Join(vbCrLf, List)
					End If
				End If

				' 取数组中的项目
				Dim strSplit = attrs("split").ToInteger
				If strSplit > 0 Then
					Dim List = value.SplitDistinct
					If List?.Length > 0 AndAlso strSplit < List.Length Then
						value = List(strSplit)
					Else
						value = ""
					End If
				End If

				'长度
				Dim strLen = attrs("len").ToInteger
				Dim strEllipsis = attrs("ellipsis")
				If strLen > 0 Then value = value.Cut(strLen, strEllipsis)

				'保留头尾，中间省略
				Dim strShow = attrs("show").ToInteger
				If strShow > 0 Then value = value.ShortShow(strShow)

				'编码
				Dim strEncode = attrs("encode")
				If strEncode.NotEmpty Then
					Select Case strEncode.Trim.ToLower
							' JS 脚本转义，可以用于将数据转换成 JSON
						Case "js"
							value = value.EncodeJs

						Case "url"
							If value.NotEmpty Then value = Web.HttpUtility.UrlEncode(value, CHARSET)

						Case "html"
							value = value.EncodeHtml

						Case "md5"
							value = value.MD5(True, CHARSET)

						Case "sha1"
							value = value.SHA1(CHARSET)

						Case "base64"
							value = value.EncodeBase64
					End Select
				End If

				'解码
				Dim strDecode = attrs("decode")
				If strDecode.NotEmpty Then
					Select Case strDecode.ToLower
						Case "js"
							value = value.DecodeJs

						Case "url"
							If Not String.IsNullOrEmpty(value) Then value = System.Web.HttpUtility.UrlDecode(value, CHARSET)

						Case "html"
							value = value.DecodeHtml

						Case "base64"
							value = value.DecodeBase64
					End Select
				End If

				' 是否匹配 Cron 表达式
				Dim strCron = attrs("cron")
				If strCron.NotEmpty Then
					Dim dateValue = value.ToDateTime
					value = Misc.Cron.Expression.Timeup(strCron, dateValue, Nothing, False)
				End If

				' 是否匹配 Cron 表达式，日模式
				Dim strCronDay = attrs("cronday")
				If strCronDay.NotEmpty Then
					Dim dateValue = value.ToDateTime
					value = Misc.Cron.Expression.Timeup(strCronDay, dateValue, Nothing, True)
				End If

				' 是否与字符串匹配
				Dim strLike = attrs("like")
				If strLike.NotEmpty Then
					value = value.Like(strLike)
				End If

				' 计算，保留小数位数
				Dim strMath = attrs("math")
				If strMath.NotEmpty Then
					Dim len = strMath.ToInteger
					Dim val = value.Compute

					If len < 0 Then
						value = val
					Else
						value = val.ToFixed(len)
					End If
				End If

#Region "判断"
				' 判断
				Dim strValidate = attrs("validate")
				If strValidate.NotEmpty Then
					Select Case strValidate.ToLower
						Case "empty"
							value = value.IsEmpty

						Case "email"
							value = value.IsEmail

						Case "guid"
							value = value.IsGUID

						Case "ip"
							value = value.IsIP

						Case "ipv4"
							value = value.IsIPv4

						Case "ipv6"
							value = value.IsIPv6

						Case "phone"
							value = value.IsPhone

						Case "mobile", "mobilephone"
							value = value.IsMobilePhone

						Case "cid", "cardid"
							value = value.IsCardID

						Case "businessid", "business"
							value = value.IsBusinessID

						Case "passport"
							value = value.IsPassport

						Case "hkmo"
							value = value.IsHKMO

						Case "taiwan"
							value = value.IsTaiWan

						Case "uint"
							value = value.IsUInt

						Case "integer"
							value = value.IsInteger

						Case "number"
							value = value.IsNumber

						Case "letternumber"
							value = value.IsLetterNumber

						Case "username"
							value = value.IsUserName(value.Length, True)

						Case "password"
							value = value.IsPassword

						Case "url"
							value = value.IsUrl

						Case "md5", "hash"
							value = value.IsMD5Hash

						Case "chinese"
							value = value.IsChinese

						Case "ascii"
							value = value.IsAscii

						Case "car"
							value = value.IsCar

						Case "datetime"
							value = value.IsDateTime

						Case "date"
							value = value.IsDate

						Case "time"
							value = value.IsTime

						Case "json"
							value = value.IsJson

						Case "xml"
							value = value.IsXml

						Case Else
							' 如果值为时间，则进行时间判断
							If value.IsDateTime Then
								Dim dateValue = value.ToDateTime
								If dateValue = New Date Then
									value = False
								Else
									Select Case strValidate
										Case "monthbegin"
											value = dateValue.IsMonthBegin

										Case "monthend"
											value = dateValue.IsMonthEnd

										Case "monday", "mon"
											value = dateValue.IsMonday

										Case "tuesday", "tue"
											value = dateValue.IsTuesday

										Case "wednesday", "wed"
											value = dateValue.IsWednesday

										Case "thursday", "thu"
											value = dateValue.IsThursday

										Case "friday", "fri"
											value = dateValue.IsFriday

										Case "saturday", "sat"
											value = dateValue.IsSaturday

										Case "sunday", "sun"
											value = dateValue.IsSunday

										Case "weekend"
											value = dateValue.IsWeekend

										Case "holiday"
											value = dateValue.IsHoliday

										Case "beforeholiday", "bh"
											value = dateValue.IsBeforeHoliday

										Case "firstholiday", "fh"
											value = dateValue.IsFirstHoliday

										Case "lastholiday", "lh"
											value = dateValue.IsLastHoliday

										Case "afterholiday", "ah"
											value = dateValue.IsAfterHoliday

										Case "adjustday", "adj"
											value = dateValue.IsAdjustday

										Case "workday", "work"
											value = dateValue.IsWorkday

										Case "restday"
											value = dateValue.IsRestday

										Case "beforerestday", "br"
											value = dateValue.IsBeforeRestday

										Case "firstrestday", "fr"
											value = dateValue.IsFirstRestday

										Case "lastrestday", "lr"
											value = dateValue.IsLastRestday

										Case "afterrestday", "ar"
											value = dateValue.IsAfterRestday

										Case "weekendholiday", "wh"
											value = dateValue.IsWeekendHoliday

										Case "sundayholiday", "sh"
											value = dateValue.IsSundayHoliday

									End Select
								End If
							End If
					End Select
				End If
			End If

#End Region

			If value.NotEmpty AndAlso replaceFunc IsNot Nothing Then value = replaceFunc.Invoke(attrs, value)

			Return value
		End Function

		''' <summary>替换时间，值为参数，以当前时间为准，增加或者减少</summary>
		Private Shared Function ReplaceDate(value As String, dateNow As Date, attrs As NameValueDictionary, Optional isSub As Boolean = False)
			Dim Dt = dateNow
			Dim Keys = {"year", "month", "day", "hour", "minute", "second"}
			For Each key In Keys
				Dim strVal = attrs(If(isSub, "_" + key, key))
				If Not String.IsNullOrEmpty(strVal) Then
					Dim delay = value.ToInteger
					If isSub Then delay = 0 - delay

					Select Case key
						Case "year"
							Dt = Dt.AddYears(delay)

						Case "month"
							Dt = Dt.AddMonths(delay)

						Case "day"
							Dt = Dt.AddDays(delay)

						Case "hour"
							Dt = Dt.AddHours(delay)

						Case "minute"
							Dt = Dt.AddMinutes(delay)

						Case "second"
							Dt = Dt.AddSeconds(delay)
					End Select

					value = Dt.ToString(strVal)
				End If
			Next

			Return If(isSub, value, ReplaceDate(value, Dt, attrs, True))
		End Function

		'''' <summary>内链替换</summary>
		'''' <param name="template">源内容</param>
		'''' <param name="formatLink">格式，Markdown文档则使用默认格式：关键词用[key]表示；链接用[link]表示；为空时，自动分析，如果Link为网址则生成连接，否则直接输出Link</param>
		'''' <param name="count">替换数量</param>
		'''' <param name="isMarkdown">是否Markdown文档</param>
		'Public Shared Function ReplaceLink(links As NameValueDictionary, template As String, Optional formatLink As String = "", Optional count As Integer = 10, Optional isMarkdown As Boolean = False) As String
		'	' 数据无效或者链接不包含{link}，直接返回
		'	If links.IsEmpty OrElse template.IsEmpty Then Return template
		'	If formatLink.NotEmpty AndAlso Not formatLink.Contains("{link}", StringComparison.OrdinalIgnoreCase) Then Return template

		'	' 默认链接格式
		'	Dim LinkFormat As String
		'	If formatLink.IsEmpty Then
		'		LinkFormat = If(isMarkdown, "[{key}]({link})", "<a href=""{link}"" target=""_blank"">{key}</a>")
		'	Else
		'		LinkFormat = formatLink
		'	End If

		'	' 默认替换数量
		'	If count < 1 Then count = Integer.MaxValue

		'	' 返回结果
		'	Dim Ret = template

		'	'所有标签内与链接中不能添加链接
		'	'<[^>]*>
		'	'<a [^>]*>(.*?)</a>
		'	Dim Tags As New NameValueDictionary
		'	Dim Key = ":" & RandomHelper.Mix(6) & ":"
		'	Dim Idx = 0

		'	If Not isMarkdown Then
		'		Dim TagExp As String = "<a [*]</a>"
		'		Dim TagList As String() = template.Cut(TagExp, 0, True)
		'		If TagList.NotEmpty Then
		'			For Each Tag As String In TagList
		'				Idx += 1
		'				Dim Hash As String = Key & Idx
		'				If Not Tags.ContainsKey(Hash) Then
		'					Tags.AddFast(Hash, Tag)
		'					Ret = Ret.Replace(Tag, Hash)
		'				End If
		'			Next
		'		End If

		'		TagExp = "<[^>]*>"
		'		TagList = template.Cut(TagExp, 0, True)
		'		If TagList.NotEmpty Then
		'			For Each Tag As String In TagList
		'				Idx += 1
		'				Dim Hash As String = Key & Idx
		'				If Not Tags.ContainsKey(Hash) Then
		'					Tags.AddFast(Hash, Tag)
		'					Ret = Ret.Replace(Tag, Hash)
		'				End If
		'			Next
		'		End If
		'	End If

		'	'过滤掉所有链接
		'	For Each name As String In links.Keys
		'		Dim link = links(name)
		'		If formatLink.NotEmpty OrElse link.IsUrl Then link = LinkFormat.Replace("{key}", name).Replace("{link}", link)

		'		Idx += 1
		'		Dim Hash As String = Key & Idx
		'		Tags.AddFast(Hash, link)

		'		' 次数不限，直接替换
		'		If count = Integer.MaxValue Then
		'			Ret = Ret.Replace(name, Hash)
		'		Else
		'			' 次数有限，只替换其中一个
		'			' 总次数不能超过

		'			Dim b = Ret.IndexOf(name, StringComparison.OrdinalIgnoreCase)
		'			If b > -1 Then
		'				Dim e = b + name.Length
		'				If e <= Ret.Length Then
		'					'Ret = Ret.Substring(0, b) & Hash & Ret.Substring(e)
		'					Ret = String.Concat(Ret.AsSpan(0, b), Hash, Ret.AsSpan(e))

		'					count -= 1
		'					If count < 1 Then Exit For
		'				End If
		'			End If
		'		End If
		'	Next

		'	' 还原链接
		'	For Each Hash In Tags.Keys
		'		Ret = Ret.Replace(Hash, Tags(Hash))
		'	Next

		'	Return Ret
		'End Function

	End Class
End Namespace
