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
' 	哈希相关操作
'
' 	name: Helper.HashHelper
' 	create: 2020-11-15
' 	memo: 哈希相关操作
' 	
' ------------------------------------------------------------

Imports System.Security

Namespace Helper

	''' <summary>哈希相关操作</summary>
	Public NotInheritable Class HashHelper

		''' <summary>Hash计算</summary>
		Public Shared Function ComputeHash(content As Byte(), Optional hashMode As HashModeEnum = HashModeEnum.MD5) As String
			If content?.Length > 0 Then
				Dim HashAlgorithm As Cryptography.HashAlgorithm

				Select Case hashMode
					Case HashModeEnum.SHA1
						HashAlgorithm = Cryptography.SHA1.Create
					Case HashModeEnum.SHA256
						HashAlgorithm = Cryptography.SHA256.Create
					Case HashModeEnum.SHA384
						HashAlgorithm = Cryptography.SHA384.Create
					Case HashModeEnum.SHA512
						HashAlgorithm = Cryptography.SHA512.Create
					Case Else
						HashAlgorithm = Cryptography.MD5.Create
				End Select

				Dim HashBytes As Byte()
				Using HashAlgorithm
					HashBytes = HashAlgorithm.ComputeHash(content)
				End Using
				Return BitConverter.ToString(HashBytes, 0).Replace("-", String.Empty).ToLower()
			End If

			Return ""
		End Function

		''' <summary>Hash计算</summary>
		Public Shared Function ComputeHash(content As String, Optional hashMode As HashModeEnum = HashModeEnum.MD5, Optional encoding As Text.Encoding = Nothing) As String
			Dim Value As String = ""

			If Not String.IsNullOrEmpty(content) Then
				If encoding Is Nothing Then encoding = Text.Encoding.UTF8

				Value = ComputeHash(encoding.GetBytes(content), hashMode)
			End If

			Return Value
		End Function

		''' <summary>SHA1</summary>
		''' <remarks>2009-04-14</remarks>
		Public Shared Function SHA1(source As String, Optional encoding As Text.Encoding = Nothing) As String
			Return ComputeHash(source, HashModeEnum.SHA1, encoding)
		End Function

		''' <summary>MD5</summary>
		''' <param name="retMode">返回模式，16位(False)或者32位(True,默认)</param>
		Public Shared Function MD5(source As String, Optional retMode As Boolean = True, Optional encoding As Text.Encoding = Nothing) As String
			Dim Ret = ComputeHash(source, HashModeEnum.MD5, encoding)
			Ret = Ret.EmptyValue(New String("0"c, 32))

			Return If(retMode, Ret, Ret.Substring(8, 16))
		End Function

		''' <summary>用默认的编码方式返回一组字符串的MD5</summary>
		Public Shared Function MD5(ParamArray sourceList As String()) As String
			Return MD5(String.Join(vbCrLf, sourceList))
		End Function

		''' <summary>Hash 验证</summary>
		Public Shared Function HashVerify(content As Byte(), hashString As String, Optional hashMode As HashModeEnum = HashModeEnum.MD5) As Boolean
			Return hashString = ComputeHash(content, hashMode)
		End Function

		''' <summary>Hash 验证</summary>
		Public Shared Function HashVerify(content As String, hashString As String, Optional hashMode As HashModeEnum = HashModeEnum.MD5, Optional encoding As Text.Encoding = Nothing) As Boolean
			Return hashString = ComputeHash(content, hashMode, encoding)
		End Function

		''' <summary>文件的HASH</summary>
		Public Shared Function File(filePath As String, Optional hashMode As HashModeEnum = HashModeEnum.MD5) As String
			Dim hash = ""

			If PathHelper.FileExist(filePath) Then
				Using stream = IO.File.OpenRead(filePath)
					Dim HashAlgorithm As Cryptography.HashAlgorithm

					Select Case hashMode
						Case HashModeEnum.SHA1
							HashAlgorithm = Cryptography.SHA1.Create
						Case HashModeEnum.SHA256
							HashAlgorithm = Cryptography.SHA256.Create
						Case HashModeEnum.SHA384
							HashAlgorithm = Cryptography.SHA384.Create
						Case HashModeEnum.SHA512
							HashAlgorithm = Cryptography.SHA512.Create
						Case Else
							HashAlgorithm = Cryptography.MD5.Create
					End Select

					Dim HashBytes As Byte()
					Using HashAlgorithm
						HashBytes = HashAlgorithm.ComputeHash(stream)
					End Using

					hash = BitConverter.ToString(HashBytes, 0).Replace("-", String.Empty).ToLower()
				End Using
			End If

			Return hash
		End Function

	End Class

End Namespace