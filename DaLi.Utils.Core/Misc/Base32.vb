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
' 	base32 算法
'
' 	name: Base32
' 	create: 2022-08-05
' 	memo: base32 算法
'
' ------------------------------------------------------------

Imports System.Threading

Namespace Misc
	''' <summary>base32 算法</summary>
	Public Class Base32

		Public Shared Function ToBytes(input As String) As Byte()
			If String.IsNullOrEmpty(input) Then Throw New ArgumentNullException(NameOf(input))

			input = input.TrimEnd("="c) 'remove padding characters

			Dim byteCount As Integer = input.Length * 5 / 8 'this must be TRUNCATED
			Dim returnArray = New Byte(byteCount - 1) {}

			Dim curByte As Byte = 0
			Dim bitsRemaining As Byte = 8
			Dim mask As Integer
			Dim arrayIndex = 0

			For Each c In input
				Dim cValue = CharToValue(c)

				If bitsRemaining > 5 Then
					mask = cValue << (bitsRemaining - 5)
					curByte = curByte Or mask
					bitsRemaining -= 5
				Else
					mask = cValue >> (5 - bitsRemaining)
					curByte = curByte Or mask

					returnArray(arrayIndex) = curByte
					Interlocked.Increment(arrayIndex)

					curByte = (cValue << (3 + bitsRemaining)) And 255
					bitsRemaining += 3
				End If
			Next

			'if we didn't end with a full byte
			If arrayIndex <> byteCount Then
				returnArray(arrayIndex) = curByte
			End If

			Return returnArray
		End Function

		Public Overloads Shared Function ToString(input As Byte()) As String
			If input Is Nothing OrElse input.Length = 0 Then Throw New ArgumentNullException(NameOf(input))

			Dim charCount = CInt(Math.Ceiling(input.Length / 5)) * 8
			Dim returnArray = New Char(charCount - 1) {}

			Dim nextChar As Byte = 0
			Dim bitsRemaining As Byte = 5
			Dim arrayIndex = 0

			For Each b In input
				nextChar = nextChar Or (b >> (8 - bitsRemaining))
				returnArray(arrayIndex) = ValueToChar(nextChar)
				Interlocked.Increment(arrayIndex)

				If bitsRemaining < 4 Then
					nextChar = (b >> (3 - bitsRemaining)) And 31
					returnArray(arrayIndex) = ValueToChar(nextChar)
					Interlocked.Increment(arrayIndex)

					bitsRemaining += 5
				End If

				bitsRemaining -= 3
				nextChar = (b << bitsRemaining) And 31
			Next

			'if we didn't end with a full char
			If arrayIndex <> charCount Then
				returnArray(arrayIndex) = ValueToChar(nextChar)
				Interlocked.Increment(arrayIndex)

				While arrayIndex <> charCount
					returnArray(arrayIndex) = "="c
					Interlocked.Increment(arrayIndex)
				End While
			End If

			Return New String(returnArray)
		End Function

		Private Shared Function CharToValue(c As Char) As Integer
			Dim value As Integer = AscW(c)

			If value < 91 AndAlso value > 64 Then
				'65-90 == uppercase letters
				Return value - 65

			ElseIf value < 56 AndAlso value > 49 Then
				'50-55 == numbers 2-7
				Return value - 24

			ElseIf value < 123 AndAlso value > 96 Then
				'97-122 == lowercase letters
				Return value - 97

			Else
				Throw New ArgumentException("Character is not a Base32 character.", NameOf(c))
			End If
		End Function

		Private Shared Function ValueToChar(b As Byte) As Char
			If b < 26 Then
				Return ChrW(b + 65)
			ElseIf b < 32 Then
				Return ChrW(b + 24)
			Else
				Throw New ArgumentException("Byte is not a value Base32 value.", NameOf(b))
			End If
		End Function

	End Class
End Namespace