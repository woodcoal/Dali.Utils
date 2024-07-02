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
' 	数组扩展操作
'
' 	name: Extension.ArrayExtension
' 	create: 2020-10-23
' 	memo: 数组扩展操作
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>数组扩展操作</summary>
	Public Module ArrayExtension

		Private ReadOnly _DEFAULT_SEPARATOR As String() = {",", "，", ";", "；"}

#Region "1. 字符转数组"

		''' <summary>分解字符串为数组</summary>
		''' <param name="separator">分隔字符串，默认分隔符为逗号</param>
		''' <param name="splitOption">分隔选项</param>
		<Extension>
		Public Function SplitEx(this As String, Optional separator As String() = Nothing, Optional splitOption As SplitEnum = SplitEnum.NONE) As String()
			If String.IsNullOrWhiteSpace(this) Then Return Nothing

			' 清除多余数据(Html / 控制符)
			Dim isTrim = False

			If (splitOption And SplitEnum.CLEAR_HTML) = SplitEnum.CLEAR_HTML Then
				this = this.ClearHtml("all")
				isTrim = True
			ElseIf (splitOption And SplitEnum.CLEAR_TRIM) = SplitEnum.CLEAR_TRIM Then
				this = this.ClearSpace
				isTrim = True
			ElseIf (splitOption And SplitEnum.CLEAR_CONTROL) = SplitEnum.CLEAR_CONTROL Then
				this = this.ClearControl
				isTrim = True
			End If

			' 数据无效
			If isTrim AndAlso String.IsNullOrWhiteSpace(this) Then Return Nothing

			' 结果大小写处理
			If (splitOption And SplitEnum.RETRUN_UPPERCASE) = SplitEnum.RETRUN_UPPERCASE Then
				this = this.ToUpper
			ElseIf (splitOption And SplitEnum.RETRUN_LOWERCASE) = SplitEnum.RETRUN_LOWERCASE Then
				this = this.ToLower
			End If

			' 全角半角处理
			If (splitOption And SplitEnum.RETRUN_DBC) = SplitEnum.RETRUN_DBC Then
				this = this.ToDBC
			ElseIf (splitOption And SplitEnum.RETRUN_SBC) = SplitEnum.RETRUN_SBC Then
				this = this.ToSBC
			End If

			' 是否清除多余结果
			Dim SplitOpt = If((splitOption And SplitEnum.REMOVE_EMPTY_ENTRIES) = SplitEnum.REMOVE_EMPTY_ENTRIES, SplitEnum.REMOVE_EMPTY_ENTRIES, SplitEnum.NONE)

			If separator?.Length > 0 Then
				' 处理回车的兼容性，防止只有换行符号无法分割
				' 将回车转换成回车换行
				If separator.Any(Function(x) x = vbCrLf) Then
					this = this.Replace(vbCr, "{VB_Cr_Lf}").Replace(vbLf, "{VB_Cr_Lf}").Replace("{VB_Cr_Lf}{VB_Cr_Lf}", "{VB_Cr_Lf}").Replace("{VB_Cr_Lf}", vbCrLf)
				End If
			Else
				separator = _DEFAULT_SEPARATOR
			End If

			Dim Ret = this.Split(separator, SplitOpt)
			If Ret?.Length > 0 Then
				If isTrim Then
					For I = 0 To Ret.Length - 1
						Ret(I) = Ret(I).Trim
					Next
				End If

				If (splitOption And SplitEnum.REMOVE_SAME) = SplitEnum.REMOVE_SAME Then Return Ret.Distinct.ToArray
			End If

			Return Ret
		End Function

		''' <summary>分解字符串为数组</summary>
		''' <param name="separator">分隔字符串，默认分隔符为逗号</param>
		''' <param name="splitOption">分隔选项</param>
		<Extension>
		Public Function SplitEx(this As String, separator As String, Optional splitOption As SplitEnum = SplitEnum.NONE) As String()
			Return SplitEx(this, If(separator.IsNull, Nothing, {separator}), splitOption)
		End Function

		''' <summary>分解字符串为数组</summary>
		''' <param name="separator">分隔字符串，默认分隔符为逗号</param>
		<Extension>
		Public Function SplitDistinct(this As String, Optional separator As String() = Nothing) As String()
			Return SplitEx(this, separator, SplitEnum.REMOVE_SAME Or SplitEnum.REMOVE_EMPTY_ENTRIES)
		End Function

		''' <summary>分解字符串为数组，并过滤到重复的数据，无效数据</summary>
		''' <param name="separator">分隔字符串，默认分隔符为逗号</param>
		<Extension>
		Public Function SplitDistinct(this As String, separator As String) As String()
			Return SplitEx(this, separator, SplitEnum.REMOVE_SAME Or SplitEnum.REMOVE_EMPTY_ENTRIES)
		End Function

#End Region

#Region "2. 字符转数值数组"

		''' <summary>字符串转换成数据列表</summary>
		''' <param name="includeZero">是否包含零</param>
		<Extension>
		Public Function ToIntegerList(this As String, Optional includeZero As Boolean = False) As IEnumerable(Of Integer)
			' 默认不设置分隔符，默认为：DEFAULT_SEPARATOR
			Dim Arrs = this.SplitEx(String.Empty, SplitEnum.REMOVE_SAME Or SplitEnum.REMOVE_EMPTY_ENTRIES Or SplitEnum.RETRUN_DBC)
			Return Arrs?.Select(Function(x) x.ToInteger(False)).Where(Function(x) includeZero OrElse x <> 0).Distinct
		End Function

		''' <summary>字符串转换成数据列表</summary>
		''' <param name="includeZero">是否包含零</param>
		<Extension>
		Public Function ToLongList(this As String, Optional includeZero As Boolean = False) As IEnumerable(Of Long)
			Dim Arrs = this.SplitEx(String.Empty, SplitEnum.REMOVE_SAME Or SplitEnum.REMOVE_EMPTY_ENTRIES Or SplitEnum.RETRUN_DBC)
			Return Arrs?.Select(Function(x) x.ToLong(False)).Where(Function(x) includeZero OrElse x <> 0).Distinct
		End Function

		''' <summary>Ids列表换成字符串</summary>
		''' <param name="excludeZero">是否排除零</param>
		<Extension>
		Public Function ToNumberString(this As IEnumerable(Of String), Optional excludeZero As Boolean = False, Optional separator As String = ",") As String
			If this?.Count > 0 Then
				Return String.Join(separator, this.Select(Function(x) x.ToLong(x)).Where(Function(x) Not excludeZero OrElse x <> 0).Distinct)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>Ids列表换成字符串</summary>
		''' <param name="excludeZero">是否排除零</param>
		<Extension>
		Public Function ToNumberString(this As IEnumerable(Of Integer), Optional excludeZero As Boolean = False, Optional separator As String = ",") As String
			If this?.Count > 0 Then
				Return String.Join(separator, this.Where(Function(x) Not excludeZero OrElse x <> 0).Distinct)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>Ids列表换成字符串</summary>
		''' <param name="excludeZero">是否包含零</param>
		<Extension>
		Public Function ToNumberString(this As IEnumerable(Of Long), Optional excludeZero As Boolean = False, Optional separator As String = ",") As String
			If this?.Count > 0 Then
				Return String.Join(separator, this.Where(Function(x) Not excludeZero OrElse x <> 0).Distinct)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>Ids列表换成字符串</summary>
		''' <param name="excludeZero">是否排除零</param>
		<Extension>
		Public Function ToNumberString(this As String(), Optional excludeZero As Boolean = False, Optional separator As String = ",") As String
			If this?.Length > 0 Then
				Return String.Join(separator, this.Select(Function(x) x.ToLong(x)).Where(Function(x) Not excludeZero OrElse x <> 0).Distinct)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>Ids列表换成字符串</summary>
		''' <param name="excludeZero">是否排除零</param>
		<Extension>
		Public Function ToNumberString(this As Integer(), Optional excludeZero As Boolean = False, Optional separator As String = ",") As String
			If this?.Length > 0 Then
				Return String.Join(separator, this.Where(Function(x) Not excludeZero OrElse x <> 0).Distinct)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>Ids列表换成字符串</summary>
		''' <param name="excludeZero">是否包含零</param>
		<Extension>
		Public Function ToNumberString(this As Long(), Optional excludeZero As Boolean = False, Optional separator As String = ",") As String
			If this?.Length > 0 Then
				Return String.Join(separator, this.Where(Function(x) Not excludeZero OrElse x <> 0).Distinct)
			Else
				Return String.Empty
			End If
		End Function

#End Region

#Region "3. 数组转换成字符"


		''' <summary>数组转换成字符串</summary>
		''' <param name="this">数组</param>
		''' <param name="separator">分隔字符</param>
		''' <param name="maxLength">返回最大长度</param>
		<Extension>
		Public Function JoinString(this As String(), Optional separator As String = ",", Optional maxLength As Integer = 0) As String
			If this?.Length > 0 Then
				Return String.Join(separator, this).Cut(separator, maxLength)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>数组转换成字符串</summary>
		''' <param name="this">数组</param>
		''' <param name="separator">分隔字符</param>
		<Extension>
		Public Function JoinString(Of T)(this As T(), Optional separator As String = ",", Optional maxLength As Integer = 0) As String
			If this?.Length > 0 Then
				Return String.Join(separator, this).Cut(separator, maxLength)
			Else
				Return String.Empty
			End If
		End Function

		''' <summary>数组转换成字符串</summary>
		''' <param name="this">数组</param>
		''' <param name="separator">分隔字符</param>
		<Extension>
		Public Function JoinString(Of T)(this As IEnumerable(Of T), Optional separator As String = ",", Optional maxLength As Integer = 0) As String
			If this.NotEmpty Then
				Return String.Join(separator, this).Cut(separator, maxLength)
			Else
				Return ""
			End If
		End Function

#End Region

#Region "4. 判断"

		''' <summary>列表是否为空</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function IsEmpty(Of T)(this As IEnumerable(Of T)) As Boolean
			Return this Is Nothing OrElse Not this.Any
		End Function

		''' <summary>数组是否为空</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function IsEmpty(Of T)(this As T()) As Boolean
			Return this Is Nothing OrElse this.Length < 1
		End Function

		''' <summary>数组是否为空</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function IsEmpty(this As Array) As Boolean
			Return this Is Nothing OrElse this.Length < 1
		End Function

		''' <summary>列表存在数据</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function NotEmpty(Of T)(this As IEnumerable(Of T)) As Boolean
			Return this IsNot Nothing AndAlso this.Any
		End Function

		''' <summary>列表存在数据</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function NotEmpty(Of T)(this As T()) As Boolean
			Return this IsNot Nothing AndAlso this.Length > 0
		End Function

		''' <summary>数组存在数据</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function NotEmpty(this As Array) As Boolean
			Return this IsNot Nothing AndAlso this.Length > 0
		End Function

		''' <summary>列表是否为空</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function EmptyValue(Of T)(this As IEnumerable(Of T), defaultValues As IEnumerable(Of T)) As IEnumerable(Of T)
			If this.IsEmpty Then
				Return defaultValues
			Else
				Return this
			End If
		End Function

		''' <summary>数组是否为空</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function EmptyValue(Of T)(this As T(), defaultValues As T()) As T()
			If this.IsEmpty Then
				Return defaultValues
			Else
				Return this
			End If
		End Function

		''' <summary>数组是否为空</summary>
		''' <param name="this">数组</param>
		<Extension>
		Public Function EmptyValue(this As Array, defaultValues As Array) As Array
			If this.IsEmpty Then
				Return defaultValues
			Else
				Return this
			End If
		End Function

#End Region


#Region "待整理，防止使用了，但是需要移除的项目"


		''' <summary>将字符串分隔成相同长度的文本数组</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="splitLength">截取的长度</param>
		<Extension>
		<Obsolete("待整理")>
		Public Function Split(this As String, splitLength As Integer) As String()
			If splitLength > 0 AndAlso this.NotNull Then
				Dim R As New List(Of String)

				' 实际字符长度
				Dim sLen = 0
				Dim sLast = 0

				Dim Max = this.Length - 1

				For I = 0 To Max
					' 为汉字或全脚符号长度加2否则加1
					If AscW(this(I)) > 127 Then
						sLen += 2
					Else
						sLen += 1
					End If

					If sLen >= splitLength Or I >= Max Then
						If sLen > splitLength Then I -= 1

						Dim s = this.Substring(sLast, I - sLast + 1)
						If Not String.IsNullOrEmpty(s) Then R.Add(s)

						sLast = I + 1
						sLen = 0
					End If
				Next

				Return R.ToArray
			End If

			Return Nothing
		End Function



		'''' <summary>集合是否为空</summary>
		'''' <param name="this">数组</param>
		'<Extension>
		'Public Function EmptyValue(Of T)(this As List(Of T), defaultValues As List(Of T)) As List(Of T)
		'	If this.IsEmpty Then
		'		Return defaultValues
		'	Else
		'		Return this
		'	End If
		'End Function

		'''' <summary>集合存在数据</summary>
		'''' <param name="this">数组</param>
		'<Extension>
		'Public Function NotEmpty(Of T)(this As List(Of T)) As Boolean
		'	Return this IsNot Nothing AndAlso this.Count > 0
		'End Function

		'''' <summary>集合是否为空</summary>
		'''' <param name="this">数组</param>
		'<Extension>
		'Public Function IsEmpty(Of T)(this As List(Of T)) As Boolean
		'	Return this Is Nothing OrElse this.Count < 1
		'End Function

#End Region

	End Module
End Namespace