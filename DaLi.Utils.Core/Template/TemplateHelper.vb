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
' 	模板相关操作
'
' 	name: TemplateHelper
' 	create: 2022-12-14
' 	memo: 模板相关操作
'
' ------------------------------------------------------------

Imports System.Text
Imports System.Text.RegularExpressions

Namespace Template
	''' <summary>模板相关操作</summary>
	Public NotInheritable Class TemplateHelper

		''' <summary>分析属性</summary>
		Public Shared Function GetAttributes(source As String) As List(Of (Key As String, Value As String))
			If source.IsEmpty Then Return Nothing

			Dim ret As New List(Of (Key As String, Value As String))

			' Attributes 属性分析
			Dim MatchCollection = Regex.Matches(source, "([a-zA-Z0-9\.\-_]+)=(['""\s]?)(.+?)\2")
			For Each Match As Match In MatchCollection
				ret.Add((Match.Groups(1).Value.ToLower, Match.Groups(3).Value.DecodeHtml))
			Next

			Return ret
		End Function

		''' <summary>基础操作注册</summary>
		''' <remarks>
		''' if / like / split / math / template 中替换内容使用 [*] 代表原始数据，支持时间操作：[yyyy]年 [MM]月 [dd]日 [HH]时 [mm]分 [ss]秒 等
		''' </remarks>
		Public Shared Sub BaseActionRegister(action As TemplateAction, Optional charset As Encoding = Nothing)
			'---------------------
			' 基础操作
			'---------------------

			' 清理 Html
			action.Register("clear", Function(value, attrs)
										 Dim strClear = attrs("clear").ToLower

										 Select Case strClear
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

										 Return value
									 End Function)

			' 替换 replace / replace.to
			' 为防止标签替换产生异常，正在表达式中 {} 由 <> 代替
			action.Register("replace", Function(value, attrs)
										   Dim strReplace = attrs("replace")
										   Dim strTo = attrs("to")

										   If strReplace.StartsWith("(") AndAlso strReplace.EndsWith(")") Then strReplace = strReplace.Replace("<", "{").Replace(">", "}")
										   Return value.ReplaceRegex(strReplace, strTo)
									   End Function)

			' 截取 cut / cut.inc
			' 不包含本身
			action.Register("cut", Function(value, attrs)
									   Dim strCut = attrs("cut")
									   If strCut.IsNumber Then
										   Return value.Cut(strCut.ToInteger)
									   Else
										   If attrs("inc").ToBoolean Then
											   ' 包含本身
											   Return value.Cut(strCut, 0, False)
										   Else
											   ' 不包含本身
											   Return value.Cut(strCut, 1, False)
										   End If
									   End If
								   End Function)

			' 截取左侧字符串
			action.Register("left", Function(value, attrs) value.Left(attrs("left").ToInteger))

			' 截取中间字符串
			action.Register("mid", Function(value, attrs)
									   Dim ns = (attrs("mid") & ",0").Split(","c)
									   Dim na = ns(0).ToInteger
									   Dim nb = ns(1).ToInteger
									   If na >= 0 Then
										   If nb > 0 Then
											   value = value.Substring(na, nb)
										   Else
											   value = value.Substring(na)
										   End If
									   End If

									   Return value
								   End Function)

			' 截取右侧字符串
			action.Register("right", Function(value, attrs) value.Right(attrs("right").ToInteger))

			' 格式化，可以加随机码干扰，对于数字会自动加上这个值，对于文字则直接附加此数据
			action.Register("format", Function(value, attrs)
										  Dim strFormat = attrs("format")

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
												  value = value.Length

											  Case Else
												  If value.IsDateTime Then
													  value = value.GetDateTime(strFormat)

												  ElseIf value.IsNumber Then
													  value = value.ToNumber.ToString(strFormat)

												  Else
													  value = String.Format(strFormat, value)
												  End If
										  End Select

										  Return value
									  End Function)

			' 模板，用 [*] 代替原值组合成新字符串
			action.Register("template", Function(value, attrs)
											Dim strTemp = attrs("template")
											If Not strTemp.Contains("[*]") Then strTemp = "[*]" & strTemp

											Return strTemp.Replace("[*]", value).GetDateTime("[", "]")
										End Function)

			' 保留头尾，中间省略
			action.Register("show", Function(value, attrs) value.ShortShow(attrs("show").ToInteger))

			'---------------------
			' 编码解码
			'---------------------

			' 编码
			action.Register("encode", Function(value, attrs)
										  Select Case attrs("encode").ToLower
											  Case "js"
												  ' JS 脚本转义，可以用于将数据转换成 JSON
												  value = value.EncodeJs
											  Case "url"
												  value = value.EncodeUrl(charset)

											  Case "html"
												  value = value.EncodeHtml
											  Case "md5"
												  value = value.MD5(True, charset)

											  Case "sha1"
												  value = value.SHA1(charset)

											  Case "base64"
												  value = value.EncodeBase64
										  End Select

										  Return value
									  End Function)

			' 解码
			action.Register("decode", Function(value, attrs)
										  Dim strDecode = attrs("decode").ToLower

										  Select Case strDecode
											  Case "js"
												  value = value.DecodeJs

											  Case "url"
												  value = value.DecodeUrl(charset)

											  Case "html"
												  value = value.DecodeHtml

											  Case "base64"
												  value = value.DecodeBase64
										  End Select

										  Return value
									  End Function)

			' Xor 异或加密
			action.Register("xor", Function(value, attrs) value.XorCoder(attrs("xor")))

			'---------------------
			' 计算
			'---------------------

			' 计算，保留小数位数	math / math.fixed
			action.Register("math", Function(value, attrs)
										Dim exp = attrs("math")
										If Not exp.Contains("[*]") Then exp = "[*]" & exp
										value = exp.Replace("[*]", value).GetDateTime("[", "]")

										Dim fixed = attrs("fixed").ToInteger
										Dim val = value.Compute

										If fixed < 0 Then
											value = val
										Else
											value = val.ToFixed(fixed)
										End If

										Return value
									End Function)

			' 相加计算，如果为日期则使用时间计算
			action.Register("add", Function(value, attrs)
									   ' 日期方式，默认计算日期，多个计算则用逗号间隔
									   ' 1m,2y ... 为避免冲突，秒使用字母 i 代替
									   If value.IsDateTime Then
										   Return DateMath(value, attrs("add"))

									   ElseIf value.IsNumber Then
										   ' 数值计算
										   Return value.ToNumber + attrs("add").ToNumber

									   Else
										   Return value
									   End If
								   End Function)

			' 与当前日期计算，与 add 操作取值相反
			action.Register("date", Function(value, attrs) DateMath(attrs("date"), value))

			'---------------------
			' 比较判断
			'---------------------
			' 判断(like,is,if 共用 true false，请勿重复判断)
			' is / is.true / is.false
			action.Register("is", Function(value, attrs)
									  ' 判断
									  Dim strIs = attrs("is").ToLower
									  Dim flag As Boolean? = Nothing

									  Select Case strIs
										  Case "empty"
											  flag = value.IsEmpty

										  Case "email"
											  flag = value.IsEmail

										  Case "guid"
											  flag = value.IsGUID

										  Case "ip"
											  flag = value.IsIP

										  Case "ipv4"
											  flag = value.IsIPv4

										  Case "ipv6"
											  flag = value.IsIPv6

										  Case "phone"
											  flag = value.IsPhone

										  Case "mobile", "mobilephone"
											  flag = value.IsMobilePhone

										  Case "cid", "cardid"
											  flag = value.IsCardID

										  Case "businessid", "business"
											  flag = value.IsBusinessID

										  Case "passport"
											  flag = value.IsPassport

										  Case "hkmo"
											  flag = value.IsHKMO

										  Case "taiwan"
											  flag = value.IsTaiWan

										  Case "uint"
											  flag = value.IsUInt

										  Case "integer"
											  flag = value.IsInteger

										  Case "number"
											  flag = value.IsNumber

										  Case "letternumber"
											  flag = value.IsLetterNumber

										  Case "username"
											  flag = value.IsUserName(value.Length, True)

										  Case "password"
											  flag = value.IsPassword

										  Case "url"
											  flag = value.IsUrl

										  Case "md5", "hash"
											  flag = value.IsMD5Hash

										  Case "chinese"
											  flag = value.IsChinese

										  Case "ascii"
											  flag = value.IsAscii

										  Case "car"
											  flag = value.IsCar

										  Case "datetime"
											  flag = value.IsDateTime

										  Case "date"
											  flag = value.IsDate

										  Case "time"
											  flag = value.IsTime

										  Case "json"
											  flag = value.IsJson

										  Case "xml"
											  flag = value.IsXml

										  Case Else
											  ' 如果值为时间，则进行时间判断
											  If value.IsDateTime Then
												  Dim dateValue = value.ToDateTime
												  If dateValue > New Date Then
													  Select Case strIs
														  Case "monthbegin"
															  flag = dateValue.IsMonthBegin

														  Case "monthend"
															  flag = dateValue.IsMonthEnd

														  Case "monday", "mon"
															  flag = dateValue.IsMonday

														  Case "tuesday", "tue"
															  flag = dateValue.IsTuesday

														  Case "wednesday", "wed"
															  flag = dateValue.IsWednesday

														  Case "thursday", "thu"
															  flag = dateValue.IsThursday

														  Case "friday", "fri"
															  flag = dateValue.IsFriday

														  Case "saturday", "sat"
															  flag = dateValue.IsSaturday

														  Case "sunday", "sun"
															  flag = dateValue.IsSunday

														  Case "weekend"
															  flag = dateValue.IsWeekend

														  Case "holiday"
															  flag = dateValue.IsHoliday

														  Case "beforeholiday", "bh"
															  flag = dateValue.IsBeforeHoliday

														  Case "firstholiday", "fh"
															  flag = dateValue.IsFirstHoliday

														  Case "lastholiday", "lh"
															  flag = dateValue.IsLastHoliday

														  Case "afterholiday", "ah"
															  flag = dateValue.IsAfterHoliday

														  Case "adjustday", "adj"
															  flag = dateValue.IsAdjustday

														  Case "workday", "work"
															  flag = dateValue.IsWorkday

														  Case "restday"
															  flag = dateValue.IsRestday

														  Case "beforerestday", "br"
															  flag = dateValue.IsBeforeRestday

														  Case "firstrestday", "fr"
															  flag = dateValue.IsFirstRestday

														  Case "lastrestday", "lr"
															  flag = dateValue.IsLastRestday

														  Case "afterrestday", "ar"
															  flag = dateValue.IsAfterRestday

														  Case "weekendholiday", "wh"
															  flag = dateValue.IsWeekendHoliday

														  Case "sundayholiday", "sh"
															  flag = dateValue.IsSundayHoliday

														  Case "cron"
															  Dim strExp = attrs("exp")
															  flag = Misc.Cron.Expression.Timeup(strExp, dateValue, Nothing, False)

														  Case "cronday"
															  Dim strExp = attrs("exp")
															  flag = Misc.Cron.Expression.Timeup(strExp, dateValue, Nothing, True)

														  Case Else
															  If strIs.StartsWith("cron:") Then
																  ' 是否匹配 Cron 表达式
																  Dim strCron = strIs.Substring(5)
																  If strCron.NotEmpty Then flag = Misc.Cron.Expression.Timeup(strCron, dateValue, Nothing, False)

															  ElseIf strIs.StartsWith("cronday:") Then
																  ' 是否匹配 Cron 表达式，日模式
																  Dim strCron = strIs.Substring(8)
																  If strCron.NotEmpty Then flag = Misc.Cron.Expression.Timeup(strCron, dateValue, Nothing, True)
															  End If
													  End Select
												  End If
											  End If
									  End Select

									  If flag.HasValue Then
										  If flag.Value Then
											  Return attrs("true").EmptyValue(True).Replace("[*]", value).GetDateTime("[", "]")
										  Else
											  Return attrs("false").EmptyValue(False).Replace("[*]", value).GetDateTime("[", "]")
										  End If
									  Else
										  Return value
									  End If
								  End Function)

			' 是否与字符串匹配		like / like.true / like.false
			action.Register("like", Function(value, attrs)
										Dim flag = value.Like(attrs("like"))
										If flag Then
											Return attrs("true").EmptyValue(True).Replace("[*]", value).GetDateTime("[", "]")
										Else
											Return attrs("false").EmptyValue(False).Replace("[*]", value).GetDateTime("[", "]")
										End If
									End Function)

			'---------------------
			' 数组
			'---------------------
			' 分解数组，并重新组合或者返回指定索引值，注意：重复值将被过滤
			' split 分隔符号；join 重新组合的字符串，如果为数组则直接返回指定索引值
			' split / split.join / split.template
			action.Register("split", Function(value, attrs)
										 Dim arr = value.SplitDistinct(attrs("split"))
										 If arr.IsEmpty Then Return ""

										 Dim strJoin = attrs("join")
										 Dim strTemp = attrs("template")
										 If Not strTemp.Contains("[*]") Then strTemp = "[*]" & strTemp

										 ' 数值，直接返回指定索引
										 If strJoin.IsInteger Then
											 Dim index = strJoin.ToInteger
											 If arr.Length > index AndAlso index > -1 Then
												 Return strTemp.Replace("[*]", arr(index)).GetDateTime("[", "]")
											 Else
												 Return ""
											 End If
										 End If

										 ' 重新组合
										 For I As Integer = 0 To arr.Length - 1
											 arr(I) = arr(I).TrimFull
											 arr(I) = strTemp.Replace("[*]", arr(I)).GetDateTime("[", "]")
										 Next

										 Return arr.JoinString(strJoin)
									 End Function)


			'---------------------
			' 从字典中获取值
			'---------------------
			' k1:v1|k2:v2|……|kn:Vn
			action.Register("from", Function(value, attrs)
										Dim arr = attrs("from").DecodeHtml.SplitDistinct("|")
										If arr.IsEmpty Then Return ""

										Dim Nvs As New NameValueDictionary
										For Each line In arr
											If line.IsEmpty Then Continue For

											Dim kv = $"{line}:".Split(":"c)
											If kv(0).IsEmpty Then Continue For

											If kv(1).IsEmpty Then kv(1) = kv(0)
											Nvs(kv(0)) = kv(1).DecodeLine
										Next

										Return Nvs(value)
									End Function)

		End Sub

		''' <summary>日期计算</summary>
		''' <param name="value">日期字符串，无效则使用当前时间</param>
		''' <param name="exps">计算表达式，多个逗号间隔</param>
		Private Shared Function DateMath(value As String, exps As String) As String
			If value.IsEmpty OrElse exps.IsEmpty Then Return value

			Dim dateValue As Date
			If value.Equals("now", StringComparison.OrdinalIgnoreCase) OrElse value.Equals("true", StringComparison.OrdinalIgnoreCase) Then
				dateValue = SYS_NOW_DATE
			Else
				dateValue = value.ToDateTime
			End If
			If Not dateValue.IsValidate Then Return ""

			For Each exp In exps.ToLower.Split
				If exp.NotEmpty Then
					Dim v = exp.ToInteger(True)

					Select Case exp.Right(1)
						Case "s"
							dateValue = dateValue.AddSeconds(v)
						Case "i"
							dateValue = dateValue.AddMinutes(v)
						Case "h"
							dateValue = dateValue.AddHours(v)
						Case "m"
							dateValue = dateValue.AddMonths(v)
						Case "y"
							dateValue = dateValue.AddYears(v)
						Case "w"
							dateValue = dateValue.AddDays(v * 7)
						Case Else
							dateValue = dateValue.AddDays(v)
					End Select
				End If
			Next

			Return dateValue.ToString("yyyy-MM-dd HH:mm:ss")
		End Function

	End Class
End Namespace