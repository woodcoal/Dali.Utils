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
' 	文本水印算法
'
' 	name: TextWatermark
' 	create: 2024-01-05
' 	memo: 文本水印算法，将水印文本转换成四进制，使用等宽不可见字符插入原始文本中来实现
'
' ------------------------------------------------------------

Imports System.Text

Namespace Misc

	''' <summary>文本水印算法</summary>
	Public Class TextWatermark

		''' <summary>默认水印编码</summary>
		Private ReadOnly _Encoding As Encoding

		''' <summary>水印密码</summary>
		Private ReadOnly _Password As Byte()

		''' <summary>隐藏字符，前 4 位为 4 进制字符，最后一位为水印分隔符</summary>
		Private ReadOnly _KEYS As Char() = {"", ChrW(&H200B), ChrW(&H200C), ChrW(&H200D), Chr(127)}

		''' <summary>构造</summary>
		Public Sub New(Optional password As String = "", Optional encoding As Encoding = Nothing)
			password = password.EmptyValue([GetType].Assembly.Name).MD5.Substring(9, 8)
			_Password = Encoding.ASCII.GetBytes(password)

			_Encoding = If(encoding, Encoding.Unicode)
		End Sub

		''' <summary>Byte 转换为四进制</summary>
		Private Shared Function ByteToBase4(source As Byte) As Byte()
			Dim ret As New List(Of Byte)

			Dim v As Integer = source
			While v > 0
				ret.Add(v Mod 4)
				v \= 4
			End While

			ret.AddRange({0, 0, 0, 0})

			Return ret.Take(4).Reverse.ToArray
		End Function

		''' <summary>四进制转换为 Byte</summary>
		Private Shared Function Base4ToByte(source As Byte()) As Byte
			If source.IsEmpty OrElse source.Length > 4 Then Return 0

			Dim ret = 0
			For Each v In source
				If v > 3 Then v = 0
				ret = (ret * 4) + v
			Next

			Return ret
		End Function

		''' <summary>水印加密</summary>
		''' <param name="watermark">水印内容</param>
		Private Function EncodeWatermark(watermark As String) As List(Of Byte)
			If watermark.IsEmpty Then Return Nothing

			Dim ret As New List(Of Byte)

			Dim bs = _Encoding.GetBytes(watermark)
			Dim idx = 0

			For Each b In bs
				' 每隔密码段为分隔符，以便随机取值时不会破坏字符
				If idx = 0 Then ret.Add(4)

				b = b Xor _Password(idx)
				ret.AddRange(ByteToBase4(b))

				idx += 1
				If idx >= _Password.Length Then idx = 0
			Next

			' 返回
			Return ret
		End Function

		''' <summary>水印解密</summary>
		''' <param name="source">水印机密后的内容</param>
		Private Function DecodeWatermark(source As List(Of Byte)) As String
			If source.IsEmpty Then Return ""

			'' 移除多余的分隔符
			'source = source.Where(Function(x) x < 4).ToList

			Dim bs As New List(Of Byte)
			Dim idx = 0
			Dim p = 0
			While idx < source.Count
				' 检查是否分隔符
				If source(idx) = 4 Then
					idx += 1
					p = 0
					Continue While
				End If

				Dim b = Base4ToByte(source.Skip(idx).Take(4).ToArray)
				b = b Xor _Password(p)
				bs.Add(b)

				idx += 4
				p += 1
				If p >= _Password.Length Then p = 0
			End While


			Dim watermark = _Encoding.GetString(bs.ToArray)
			Return watermark
		End Function

		''' <summary>生成水印</summary>
		Public Function Encode(source As String, watermark As String, Optional ByRef message As String = "") As String
			If source.IsEmpty OrElse watermark.IsEmpty Then
				message = "原始文本与水印不存在"
				Return ""
			End If

			Dim watermarkEncode = EncodeWatermark(watermark)
			Dim watermarkLength = watermarkEncode.Count
			If source.Length < watermarkLength Then
				message = "原始文本长度不能小于水印长度的 4 倍"
				Return ""
			End If

			With New StringBuilder
				Dim idx = 0

				For I = 0 To source.Length - 2
					.Append(source(I))

					Dim m = watermarkEncode(idx)
					If m > 0 Then .Append(_KEYS(m))

					idx += 1
					If idx >= watermarkLength Then idx = 0
				Next

				'尾部插入结束分隔符
				.Append(_KEYS(4))
				.Append(source(source.Length - 1))

				Return .ToString
			End With
		End Function

		''' <summary>获取水印</summary>
		Public Function Decode(source As String, Optional ByRef message As String = "") As String
			If source.IsEmpty Then
				message = "原始文本不存在"
				Return ""
			End If

			Dim bs As New List(Of Byte)

			Dim idx = 0
			While idx < source.Length
				Dim b = Array.IndexOf(_KEYS, source(idx))
				If b > 0 Then
					bs.Add(b)
					idx += 2
				Else
					bs.Add(0)
					idx += 1
				End If
			End While


			' 首位标识
			Dim first = bs.IndexOf(4)
			Dim last = bs.LastIndexOf(4)
			If first < 0 OrElse last < first Then Return ""

			bs = bs.Skip(first + 1).Take(last - first - 1).ToList

			Return DecodeWatermark(bs)
		End Function
	End Class

End Namespace