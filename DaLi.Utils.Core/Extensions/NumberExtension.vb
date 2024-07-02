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
' 	数字扩展操作
'
' 	name: Extension.NumberExtension
' 	create: 2020-10-23
' 	memo: 数字扩展操作
' 	
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Extension

	''' <summary>数字扩展操作</summary>
	Public Module NumberExtension

		' 默认进制字符串
		' 2, 8, 10, 16, 26, 32, 34, 36, 52, 62, 64
		' 32：base 32 去掉 ailo
		' 34: 去掉IO，类似车牌
		Public ReadOnly LETTERS As ImmutableDictionary(Of Integer, String) = New Dictionary(Of Integer, String) From {
			{2, "01"},
			{8, "01234567"},
			{10, "0123456789"},
			{16, "0123456789ABCDEF"},
			{26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"},
			{32, "0123456789bcdefghjkmnpqrstuvwxyz"},
			{34, "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ"},
			{36, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"},
			{52, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"},
			{62, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"},
			{64, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"}
		}.ToImmutableDictionary

#Region "1. 常用操作"

		''' <summary>是否空值</summary>
		''' <param name="this">要操作的 Integer</param>
		<Extension>
		Public Function IsEmpty(this As Integer?) As Boolean
			Return Not this.HasValue OrElse this = 0
		End Function

		''' <summary>是否空值</summary>
		''' <param name="this">要操作的 Integer</param>
		<Extension>
		Public Function IsEmpty(this As Integer) As Boolean
			Return this = 0
		End Function

		''' <summary>是否空值</summary>
		''' <param name="this">要操作的 Long</param>
		<Extension>
		Public Function IsEmpty(this As Long?) As Boolean
			Return Not this.HasValue OrElse this.Value = 0
		End Function

		''' <summary>是否空值</summary>
		''' <param name="this">要操作的 Long</param>
		<Extension>
		Public Function IsEmpty(this As Long) As Boolean
			Return this = 0
		End Function

		''' <summary>不为空值</summary>
		''' <param name="this">要操作的 Integer</param>
		<Extension>
		Public Function NotEmpty(this As Integer?) As Boolean
			Return this.HasValue AndAlso this <> 0
		End Function

		''' <summary>不为空值</summary>
		''' <param name="this">要操作的 Integer</param>
		<Extension>
		Public Function NotEmpty(this As Integer) As Boolean
			Return this <> 0
		End Function

		''' <summary>不为空值</summary>
		''' <param name="this">要操作的 Long</param>
		<Extension>
		Public Function NotEmpty(this As Long?) As Boolean
			Return this.HasValue AndAlso this <> 0
		End Function

		''' <summary>不为空值</summary>
		''' <param name="this">要操作的 Long</param>
		<Extension>
		Public Function NotEmpty(this As Long) As Boolean
			Return this <> 0
		End Function

		''' <summary>保留小数点位数</summary>
		''' <param name="this">原始字符</param>
		''' <param name="fixLength">小数点位数</param>
		''' <param name="fillZero">整数的小数位是否补零</param>
		<Extension>
		Public Function ToFixed(this As Double, fixLength As Integer, Optional fillZero As Boolean = False) As String
			Dim fmt = "0." & If(fillZero, New String("0"c, fixLength), New String("#"c, fixLength))
			Return this.ToString(fmt)
		End Function

		''' <summary>保留小数点位数</summary>
		''' <param name="this">原始字符</param>
		''' <param name="fixLength">小数点位数</param>
		''' <param name="fillZero">整数的小数位是否补零</param>
		<Extension>
		Public Function ToFixed(this As Single, fixLength As Integer, Optional fillZero As Boolean = False) As String
			Dim fmt = "0." & If(fillZero, New String("0"c, fixLength), New String("#"c, fixLength))
			Return this.ToString(fmt)
		End Function

		''' <summary>格式化数据</summary>
		''' <param name="this"></param>
		''' <param name="use1024">是否使用 1024 作为单位，否则使用 1000</param>
		<Extension>
		Public Function ToSize(this As Double, Optional use1024 As Boolean = True) As String
			Dim NowSize(1) As Double
			Dim unit = If(use1024, 1024, 1000)

			NowSize(0) = this / unit
			If NowSize(0) > 0.9 Then
				NowSize(1) = NowSize(0)
				NowSize(0) = NowSize(0) / unit
				If NowSize(0) > 0.9 Then
					NowSize(1) = NowSize(0)
					NowSize(0) = NowSize(0) / unit
					If NowSize(0) > 0.9 Then
						NowSize(1) = NowSize(0)
						NowSize(0) = NowSize(0) / unit
						If NowSize(0) > 0.9 Then
							Return NowSize(0).ToString("0.00") & "T"
						Else
							Return NowSize(1).ToString("0.00") & "G"
						End If
					Else
						Return NowSize(1).ToString("0.00") & "M"
					End If
				Else
					Return NowSize(1).ToString("0.00") & "K"
				End If
			Else
				Return this.ToString("0.00")
			End If
		End Function

		''' <summary>格式化数据</summary>
		<Extension>
		Public Function ToSize(this As Integer, Optional use1024 As Boolean = True) As String
			Return Convert.ToDouble(this).ToSize(use1024)
		End Function

		''' <summary>格式化数据</summary>
		<Extension>
		Public Function ToSize(this As Long, Optional use1024 As Boolean = True) As String
			Return Convert.ToDouble(this).ToSize(use1024)
		End Function

		''' <summary>格式化数据</summary>
		<Extension>
		Public Function ToSize(this As Single, Optional use1024 As Boolean = True) As String
			Return Convert.ToDouble(this).ToSize(use1024)
		End Function

		''' <summary>字符串表达式计算</summary>
		''' <param name="this">表达式</param>
		<Extension>
		Public Function Compute(this As String) As Double
			If this.NotEmpty Then
				Try
					Return New Data.DataTable().Compute(this, Nothing)
				Catch ex As Exception
				End Try
			End If

			Return 0
		End Function

#End Region

#Region "2. 区间大小"

		''' <summary>区间大小</summary>
		<Extension>
		Public Function Range(this As Integer, Optional min As Integer? = Nothing, Optional max As Integer? = Nothing) As Integer
			If min IsNot Nothing AndAlso this < min Then this = min
			If max IsNot Nothing AndAlso this > max Then this = max
			Return this
		End Function

		''' <summary>区间大小</summary>
		<Extension>
		Public Function Range(this As Long, Optional min As Integer? = Nothing, Optional max As Integer? = Nothing) As Long
			If min IsNot Nothing AndAlso this < min Then this = min
			If max IsNot Nothing AndAlso this > max Then this = max
			Return this
		End Function

		''' <summary>区间大小</summary>
		<Extension>
		Public Function Range(this As Single, Optional min As Integer? = Nothing, Optional max As Integer? = Nothing) As Single
			If min IsNot Nothing AndAlso this < min Then this = min
			If max IsNot Nothing AndAlso this > max Then this = max
			Return this
		End Function

		''' <summary>区间大小</summary>
		<Extension>
		Public Function Range(this As Double, Optional min As Integer? = Nothing, Optional max As Integer? = Nothing) As Double
			If min IsNot Nothing AndAlso this < min Then this = min
			If max IsNot Nothing AndAlso this > max Then this = max
			Return this
		End Function

		''' <summary>区间大小</summary>
		<Extension>
		Public Function Range(this As Date, Optional min As Date? = Nothing, Optional max As Date? = Nothing) As Date
			If min IsNot Nothing AndAlso this < min Then this = min
			If max IsNot Nothing AndAlso this > max Then this = max
			Return this
		End Function

#End Region

#Region "3. 进制转换"

		''' <summary>进制数转换</summary>
		''' <param name="This">要转换的数字</param>
		''' <param name="fromBase">2、8、10、16</param>
		''' <param name="toBase">2、8、10、16</param>
		<Extension>
		Public Function NumberChange(this As String, fromBase As Integer, toBase As Integer) As String
			Try
				Return Convert.ToString(Convert.ToInt64(this, fromBase), toBase)
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>二进制转十进制</summary> 
		<Extension>
		Public Function FromBin(this As String) As Long
			Try
				Return Convert.ToInt64(this, 2)
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>八进制转十进制</summary> 
		<Extension>
		Public Function FromOct(this As String) As Long
			Try
				Return Convert.ToInt64(this, 8)
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>十六进制转十进制</summary> 
		<Extension>
		Public Function FromDec(this As String) As Long
			Try
				Return Convert.ToInt64(this, 16)
			Catch ex As Exception
				Return 0
			End Try
		End Function

		''' <summary>十进制转二进制</summary> 
		<Extension>
		Public Function ToBin(this As Integer) As String
			Return Convert.ToString(this, 2)
		End Function

		''' <summary>十进制转二进制</summary> 
		<Extension>
		Public Function ToBin(this As Long) As String
			Return Convert.ToString(this, 2)
		End Function

		''' <summary>十进制转八进制</summary> 
		<Extension>
		Public Function ToOct(this As Integer) As String
			Return Convert.ToString(this, 8)
		End Function

		''' <summary>十进制转八进制</summary> 
		<Extension>
		Public Function ToOct(this As Long) As String
			Return Convert.ToString(this, 8)
		End Function

		''' <summary>十进制转十六进制</summary> 
		<Extension>
		Public Function ToHex(this As Integer) As String
			Return Convert.ToString(this, 16)
		End Function

		''' <summary>十进制转十六进制</summary> 
		<Extension>
		Public Function ToHex(this As Long) As String
			Return Convert.ToString(this, 16)
		End Function

		''' <summary>二进制转十六进制</summary> 
		<Extension>
		Public Function Bin2Hex(this As String) As String
			If this.IsEmpty OrElse Not Regex.IsMatch(this, "^[01]+$") Then Return ""

			' 四位一组，数据长度
			Dim sLen = Math.Ceiling(this.Length / 4) * 4

			' 补零数量
			Dim zLen = sLen - this.Length
			If zLen > 0 Then this = "0".Duplicate(zLen) & this

			With New Text.StringBuilder
				For I = 0 To sLen Step 4
					Select Case this.Substring(I, 4)
						Case "0000"
							.Append("0"c)
						Case "0001"
							.Append("1"c)
						Case "0010"
							.Append("2"c)
						Case "0011"
							.Append("3"c)
						Case "0100"
							.Append("4"c)
						Case "0101"
							.Append("5"c)
						Case "0110"
							.Append("6"c)
						Case "0111"
							.Append("7"c)
						Case "1000"
							.Append("8"c)
						Case "1001"
							.Append("9"c)
						Case "1010"
							.Append("A"c)
						Case "1011"
							.Append("B"c)
						Case "1100"
							.Append("C"c)
						Case "1101"
							.Append("D"c)
						Case "1110"
							.Append("E"c)
						Case Else
							.Append("F"c)
					End Select
				Next

				Return .ToString
			End With
		End Function

		''' <summary>十六进制转二进制</summary> 
		<Extension>
		Public Function Hex2Bin(this As String) As String
			If this.IsEmpty OrElse Not Regex.IsMatch(this, "^[0-9a-fA-F]+$") Then Return ""

			With New Text.StringBuilder
				For Each S In this
					Select Case this
						Case "0"c
							.Append("0000")
						Case "1"c
							.Append("0001")
						Case "2"c
							.Append("0010")
						Case "3"c
							.Append("0011")
						Case "4"c
							.Append("0100")
						Case "5"c
							.Append("0101")
						Case "6"c
							.Append("0110")
						Case "7"c
							.Append("0111")
						Case "8"c
							.Append("1000")
						Case "9"c
							.Append("1001")
						Case "A"c, "a"c
							.Append("1010")
						Case "B"c, "b"c
							.Append("1011")
						Case "C"c, "c"c
							.Append("1100")
						Case "D", "d"c
							.Append("1101")
						Case "E"c, "e"c
							.Append("1110")
						Case "F"c, "f"c
							.Append("1110")
					End Select
				Next

				Return .ToString
			End With
		End Function

		''' <summary>进制转换</summary> 
		''' <param name="this">原始十进制数</param>
		''' <param name="radix">转换进制</param>
		''' <returns>返回位值数组</returns>
		<Extension>
		Public Function Encode(this As Decimal, radix As Integer) As IEnumerable(Of Integer)
			If this < 0 Then Return Nothing
			If this = 0 Then Return {0}

			' 递归除
			Dim ret As New List(Of Integer)

			While True
				' 余数
				Dim v = this Mod radix
				ret.Add(v)

				this -= v
				If this < 1 Then Exit While

				' 整除
				this /= radix
			End While

			' 倒序
			ret.Reverse()

			Return ret
		End Function

		''' <summary>进制转换</summary> 
		''' <param name="this">原始位值数组</param>
		''' <param name="radix">转换进制</param>
		''' <returns>十进制数</returns>
		<Extension>
		Public Function Decode(this As IEnumerable(Of Integer), radix As Integer) As Decimal
			If this.IsEmpty Then Return -1

			' 递归乘
			Dim ret As Decimal = 0

			' 基数
			Dim base As Decimal = 1

			For I = this.Count - 1 To 0 Step -1
				Dim idx = this(I)
				ret += idx * base

				If I > 0 Then base *= radix
			Next

			Return ret
		End Function

		''' <summary>数值压缩转字符</summary> 
		''' <param name="this">原始十进制数</param>
		''' <param name="letters">转换字符列表</param>
		<Extension>
		Public Function Encode(this As Decimal, letters As String) As String
			If this < 0 OrElse letters.IsEmpty Then Return ""

			Dim nums = this.Encode(letters.Length)
			If nums.IsEmpty Then Return ""

			Dim chars = nums.Select(Function(x) Convert.ToChar(letters(x))).ToArray
			Return New String(chars)
		End Function

		''' <summary>字符解压成数值</summary> 
		''' <param name="this">原始位值数组</param>
		''' <param name="letters">转换字符列表</param>
		<Extension>
		Public Function Decode(this As String, letters As String) As Decimal
			If this.IsEmpty OrElse letters.IsEmpty Then Return -1

			Dim nums = this.ToArray.Select(Function(x) letters.IndexOf(x))
			Return Decode(nums, letters.Length)
		End Function

		''' <summary>数值压缩转字符</summary> 
		''' <param name="this">原始十进制数</param>
		''' <param name="radix">转换类型</param>
		<Extension>
		Public Function Encode(this As Decimal, radix As RadixEnum) As String
			Return Encode(this, LETTERS(Convert.ToInt32(radix)))
		End Function

		''' <summary>字符解压成数值</summary> 
		''' <param name="this">原始位值数组</param>
		''' <param name="radix">转换类型</param>
		<Extension>
		Public Function Decode(this As String, radix As RadixEnum) As Decimal
			Return Decode(this, LETTERS(Convert.ToInt32(radix)))
		End Function

		''' <summary>数值压缩转字符</summary> 
		''' <param name="this">原始十进制数</param>
		''' <param name="radix">转换类型</param>
		<Extension>
		Public Function Encode(this As Long, radix As RadixEnum) As String
			Return Encode(this, LETTERS(Convert.ToInt32(radix)))
		End Function

		''' <summary>数值压缩转字符</summary> 
		''' <param name="this">原始十进制数</param>
		''' <param name="radix">转换类型</param>
		<Extension>
		Public Function Encode(this As Integer, radix As RadixEnum) As String
			Return Encode(this, LETTERS(Convert.ToInt32(radix)))
		End Function
#End Region

	End Module
End Namespace

