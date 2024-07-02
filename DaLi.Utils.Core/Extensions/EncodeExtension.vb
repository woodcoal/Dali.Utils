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
' 	将字符串编码
'
' 	name: Extension.EncodeExtension
' 	create: 2019-03-23
' 	memo: 将字符串编码
' 	
' ------------------------------------------------------------

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Extension

	''' <summary>将字符串编码</summary>
	Public Module EncodeExtension

#Region "其他操作"

		''' <summary>回车文本编码，仅编码回车、Tab</summary>
		<Extension>
		Public Function EncodeLine(this As String) As String
			If this.IsEmpty Then Return this

			this = this.Replace(vbCrLf, "[BR]")
			this = this.Replace(vbTab, "[TAB]")
			this = this.Replace(vbCr, "[CR]")
			this = this.Replace(vbLf, "[LF]")
			Return this
		End Function

		''' <summary>回车文本解码</summary>
		<Extension>
		Public Function DecodeLine(this As String) As String
			If this.IsEmpty Then Return this

			this = this.Replace("[BR]", vbCrLf)
			this = this.Replace("[TAB]", vbTab)
			this = this.Replace("[CR]", vbCr)
			this = this.Replace("[LF]", vbLf)
			Return this
		End Function

		''' <summary>HTML 编码</summary>
		<Extension>
		Public Function EncodeHtml(this As String) As String
			Return Web.HttpUtility.HtmlEncode(this)
		End Function

		''' <summary>HTML 解码</summary>
		<Extension>
		Public Function DecodeHtml(this As String) As String
			Return Web.HttpUtility.HtmlDecode(this)
		End Function

		''' <summary>URL 编码</summary>
		''' <param name="this">要处理的字符串</param>
		''' <param name="encoding">编码</param>
		<Extension>
		Public Function EncodeUrl(this As String, Optional encoding As Text.Encoding = Nothing) As String
			If encoding Is Nothing Then
				Return Web.HttpUtility.UrlEncode(this)
			Else
				Return Web.HttpUtility.UrlEncode(this, encoding)
			End If
		End Function

		''' <summary>URL 解码</summary>
		''' <param name="this">要解码的字符串</param>
		''' <param name="encoding">编码</param>
		<Extension>
		Public Function DecodeUrl(this As String, Optional encoding As Text.Encoding = Nothing) As String
			If encoding Is Nothing Then
				Return Web.HttpUtility.UrlDecode(this)
			Else
				Return Web.HttpUtility.UrlDecode(this, encoding)
			End If
		End Function

		''' <summary>Unicode 解码，用于 Js 中汉字编码 \uxxxx 类似格式字符</summary> 
		<Extension>
		Public Function DecodeJsUnicode(this As String) As String
			If this.NotEmpty AndAlso this.Contains("\u", StringComparison.OrdinalIgnoreCase) Then
				Return Regex.Unescape(this)
			Else
				Return this
			End If
		End Function

		''' <summary>脚本转义</summary>
		<Extension>
		Public Function EncodeJs(this As String) As String
			If Not this.IsEmpty Then
				' 先转义斜线，防止其他符号转义后重复转义斜线
				this = this.Replace("\", "\\").Replace("'", "\'").Replace("""", "\""").Replace("&", "\&").Replace(vbCrLf, "\n").Replace(vbCr, "\r").Replace(vbLf, "\n").Replace(vbTab, "\t").Replace(vbBack, "\b").Replace(vbFormFeed, "\f")

				'\\	反斜杠
				'\'	单引号
				'\"	双引号
				'\&	和号
				'\n	换行符
				'\r	回车符
				'\t	制表符
				'\b	退格符
				'\f	换页符
			End If

			Return this
		End Function

		''' <summary>脚本反转义</summary>
		<Extension>
		Public Function DecodeJs(this As String) As String
			If Not this.IsEmpty Then
				this = this.Replace("\f", vbFormFeed).Replace("\b", vbBack).Replace("\t", vbTab).Replace("\n", vbLf).Replace("\r", vbCr).Replace("\&", "&").Replace("\""", """").Replace("\'", "'").Replace("\\", "\")

				'\\	反斜杠
				'\'	单引号
				'\"	双引号
				'\&	和号
				'\n	换行符
				'\r	回车符
				'\t	制表符
				'\b	退格符
				'\f	换页符
			End If

			Return this
		End Function

		''' <summary>异或加密/解密</summary>
		<Extension>
		Public Function XorCoder(this As String, key As String) As String
			If this.IsEmpty OrElse key.IsEmpty Then Return this

			Dim source = Encoding.Unicode.GetBytes(this)
			Dim codes = Encoding.Unicode.GetBytes(key)

			Dim bs As New List(Of Byte)

			Dim idx = 0
			For Each s In source
				bs.Add(s Xor codes(idx))

				idx += 1
				If idx >= codes.Length Then idx = 0
			Next

			Return Encoding.Unicode.GetString(bs.ToArray())
		End Function

#End Region

#Region "编码为 Byte()"

		''' <summary>使用 UTF-8 编码将 String 转换成 Byte()</summary>
		<Extension>
		Public Function ToBytes(this As String) As Byte()
			Return ToBytes(this, Text.Encoding.UTF8)
		End Function

		''' <summary>通过指定的编码方式返回字符数组</summary>
		<Extension>
		Public Function ToBytes(this As String, encoding As Text.Encoding) As Byte()
			If this.IsNull Then Return Nothing

			encoding = If(encoding, Text.Encoding.UTF8)
			Return encoding.GetBytes(this)
		End Function

		''' <summary>使用 UTF-8 编码将 String 转换成 Byte()，同时增加一定数量的干扰内容</summary>
		''' <param name="clutter">干扰字符数量，结果将是干扰量的 N + 1 倍</param>
		''' <memo>
		''' 算法：
		''' 在实际的数据中加入一定量的干扰，以干扰系数为 2 为例
		''' 实际数组应为： ABCDEF
		''' 干扰后则为： A** *B* **C D** *E* **F
		''' 在每个感染字符串中加入 N 个干扰字符，实际字符的位置为 I Mod (N + 1)
		''' </memo>
		<Extension>
		Public Function ToBytes(this As String, clutter As Integer) As Byte()
			Return EncodeBytes(ToBytes(this), clutter)
		End Function

		''' <summary>通过 Des 加密方式返回字符数组</summary>
		''' <param name="this"></param>
		''' <param name="key">Des 加密密匙(8字节)</param>
		<Extension>
		Public Function ToBytes(this As String, key As String) As Byte()
			If String.IsNullOrEmpty(this) Then Return Nothing
			Return New SecurityHelper.Des().Encrypt(ToBytes(this), key)
		End Function

#End Region

#Region "解码为 String"

		''' <summary>使用 UTF-8 编码将 Byte() 还原成 String </summary>
		''' <remarks>2009-04-14</remarks>
		<Extension>
		Public Function ToString(this As Byte()) As String
			Return ToString(this, Text.Encoding.UTF8)
		End Function

		''' <summary>通过指定的编码方式返回字符串</summary>
		<Extension>
		Public Function ToString(this As Byte(), encoding As Text.Encoding) As String
			If this?.Length > 0 Then
				Try
					encoding = If(encoding, Text.Encoding.UTF8)
					Return encoding.GetString(this)
				Catch ex As Exception
				End Try
			End If

			Return ""
		End Function

		''' <summary>去掉干扰内容，然后使用 UTF-8 编码将 Byte() 还原成 String</summary>
		''' <param name="clutter">干扰字符数量，结果将是干扰量的 1 / (N + 1)</param>
		''' <returns></returns>
		''' <remarks>2009-04-14</remarks>
		''' <memo>
		''' 算法：
		''' 在实际的数据中过滤掉干扰，以干扰系数为 2 为例
		''' 干扰数组应为： A** *B* **C D** *E* **F
		''' 实际过滤后为： ABCDEF
		''' 在每个感染字符串中过滤 N 个干扰字符，实际字符的位置为 I Mod (N + 1)
		''' </memo>
		<Extension>
		Public Function ToString(this As Byte(), clutter As Integer) As String
			Return ToString(DecodeBytes(this, clutter))
		End Function

		''' <summary>通过 Des 解密方式返回字符串</summary>
		''' <param name="this"></param>
		''' <param name="Key">解密密匙</param>
		<Extension>
		Public Function ToString(this As Byte(), key As String) As String
			If this?.Length > 0 Then
				Try
					Return ToString(New SecurityHelper.Des().Decrypt(this, key))
				Catch ex As Exception

				End Try
			End If

			Return ""
		End Function

#End Region

#Region "String"

		''' <summary>字符串 Base64 编码 </summary>
		<Extension>
		Public Function EncodeBase64(this As String) As String
			If this.IsNull Then
				Return ""
			Else
				Return ToBase64(this.ToBytes)
			End If
		End Function

		''' <summary>字符串 Base64 解码</summary>
		<Extension>
		Public Function DecodeBase64(this As String) As String
			If this.IsEmpty Then
				Return ""
			Else
				Return ToString(FromBase64(this))
			End If
		End Function

		''' <summary>字符串编码</summary>
		<Extension>
		Public Function EncodeString(this As String, clutter As Integer) As String
			If this.IsNull Then
				Return ""
			Else
				Return ToBase64(this.ToBytes(clutter))
			End If
		End Function

		''' <summary>字符串解码</summary>
		<Extension>
		Public Function DncodeString(this As String, clutter As Integer) As String
			If this.IsEmpty Then
				Return ""
			Else
				Return FromBase64(this).ToString(clutter)
			End If
		End Function

		''' <summary>字符串编码(干扰并DES加密)</summary>
		''' <param name="clutter">干扰字符数量，结果将是干扰量的 1 / (N + 1)</param>
		<Extension>
		Public Function EncodeString(this As String, clutter As Integer, key As String) As String
			Dim bytes = this.ToBytes(clutter)
			bytes = New SecurityHelper.Des().Encrypt(bytes, key)
			Return ToBase64(bytes)
		End Function

		''' <summary>字符串解码(DES解密去干扰)</summary>
		''' <param name="clutter">干扰字符数量，结果将是干扰量的 1 / (N + 1)</param>
		<Extension>
		Public Function DecodeString(this As String, clutter As Integer, key As String) As String
			Dim bytes = FromBase64(this)
			bytes = New SecurityHelper.Des().Decrypt(bytes, key)
			Return bytes.ToString(clutter)
		End Function

		''' <summary>字符串 DES 加密</summary>
		<Extension>
		Public Function EncryptDES(this As String, key As String) As String
			Return New SecurityHelper.Des().Encrypt(this, key)
		End Function

		''' <summary>字符串 DES 解密</summary>
		<Extension>
		Public Function DecryptDES(this As String, key As String) As String
			Return New SecurityHelper.Des().Decrypt(this, key)
		End Function

#End Region

#Region "相关操作"

		''' <summary>
		''' 使用 UTF-8 编码将 String 转换成 Byte()，同时增加一定数量的干扰内容
		''' </summary>
		''' <param name="clutter">干扰字符数量，结果将是干扰量的 N + 1 倍</param>
		''' <remarks>2009-04-14</remarks>
		''' <memo>
		''' 算法：
		''' 在实际的数据中加入一定量的干扰，以干扰系数为 2 为例
		''' 实际数组应为： ABCDEF
		''' 干扰后则为： A** *B* **C D** *E* **F
		''' 在每个感染字符串中加入 N 个干扰字符，实际字符的位置为 I Mod (N + 1)
		''' </memo>
		Private Function EncodeBytes(this As Byte(), clutter As Integer) As Byte()
			Dim Result As Byte() = this

			If this?.Length > 0 AndAlso clutter > 0 Then
				Try
					clutter += 1

					ReDim Result((this.Length * clutter) - 1)

					' 使用RNGCryptoServiceProvider 做种，可以在一秒内产生的随机数重复率非常的低，对于以往使用时间做种的方法是个升级
					Using RNG = Security.Cryptography.RandomNumberGenerator.Create
						RNG.GetBytes(Result)
					End Using

					For I As Integer = 0 To this.Length - 1
						Result((I * clutter) + (I Mod clutter)) = this(I)
					Next
				Catch ex As Exception
				End Try
			End If

			Return Result
		End Function

		''' <summary>去掉干扰内容，然后使用 UTF-8 编码将 Byte() 还原成 String</summary>
		''' <param name="clutter">干扰字符数量，结果将是干扰量的 1 / (N + 1)</param>
		''' <remarks>2009-04-14</remarks>
		''' <memo>
		''' 算法：
		''' 在实际的数据中过滤掉干扰，以干扰系数为 2 为例
		''' 干扰数组应为： A** *B* **C D** *E* **F
		''' 实际过滤后为： ABCDEF
		''' 在每个感染字符串中过滤 N 个干扰字符，实际字符的位置为 I Mod (N + 1)
		''' </memo>
		Private Function DecodeBytes(this As Byte(), clutter As Integer) As Byte()
			Dim Result As Byte() = this

			If this?.Length > 0 AndAlso clutter > 0 Then
				Try
					clutter += 1
					If this.Length Mod clutter = 0 Then
						ReDim Result((this.Length / clutter) - 1)

						For I As Integer = 0 To Result.Length - 1
							Result(I) = this((I * clutter) + (I Mod clutter))
						Next
					End If
				Catch ex As Exception
				End Try
			End If

			Return Result
		End Function

		''' <summary>Base64 编码</summary>
		Private Function ToBase64(this As Byte()) As String
			Try
				If this?.Length > 0 Then Return Convert.ToBase64String(this)
			Catch
			End Try

			Return ""
		End Function

		''' <summary>Base64 解码</summary>
		Private Function FromBase64(this As String) As Byte()
			Try
				If Not this.IsEmpty Then
					Return Convert.FromBase64String(this)
				End If
			Catch ex As Exception
			End Try

			Return Nothing
		End Function

#End Region

#Region "其他操作"

		''' <summary>将数据流转换成 Base64 编码</summary>
		<Extension>
		Public Function ToBase64(this As Stream) As String
			Dim ret = ""
			If this Is Nothing OrElse Not this.CanSeek Then Return ret

			' 确保 stream 的位置在开始处
			this.Seek(0, SeekOrigin.Begin)

			' 读取到 bytes 中
			Using ms As New MemoryStream
				this.CopyTo(ms)

				ret = Convert.ToBase64String(ms.ToArray)
			End Using

			Return ret
		End Function

		''' <summary>将 Base64 编码转换成数据流</summary>
		<Extension>
		Public Function ToStream(this As String) As Stream
			If this.IsEmpty Then Return Nothing

			' 将 Base64 字符串转换为 byte 数组
			Dim bytes = Convert.FromBase64String(this)
			If bytes.IsEmpty Then Return Nothing

			Dim ret = New MemoryStream(bytes)
			ret.Seek(0, SeekOrigin.Begin)

			Return ret
		End Function

#End Region

	End Module

End Namespace