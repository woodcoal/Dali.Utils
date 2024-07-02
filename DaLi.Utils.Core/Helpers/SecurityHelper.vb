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
' 	加密算法
'
' 	name: Helper.SecurityHelper
' 	create: 2020-11-15
' 	memo: 加密算法
' 	
' ------------------------------------------------------------

Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml

Namespace Helper.SecurityHelper

	''' <summary>Des 加密算法</summary>
	Public NotInheritable Class Des

		Private _IV As Byte()
		Private _Key As Byte()

		Private Property IV As Byte()
			Get
				If _IV Is Nothing Then _IV = New Byte() {50, &H69, 65, &H6, 77, &H66, 56, &H26}
				Return _IV
			End Get
			Set(value As Byte())
				_IV = value
			End Set
		End Property

		Private Property Key As Byte()
			Get
				If _Key Is Nothing Then _Key = New Byte() {&H77, 66, &H56, 26, &H50, 69, &H65, 6}
				Return _Key
			End Get
			Set(value As Byte())
				_Key = value
			End Set
		End Property

		Public WriteOnly Property KeyString As String
			Set(value As String)
				Key = Nothing

				If Not String.IsNullOrEmpty(value) Then
					value = HashHelper.MD5(value, False)
					Key = Text.Encoding.ASCII.GetBytes(value.Substring(0, 8))
					IV = Text.Encoding.ASCII.GetBytes(value.Substring(8, 8))
				End If
			End Set
		End Property

		'-------------------------------------------------------------------

		Public Function Encrypt(source As Byte(), Optional secKey As String = "") As Byte()
			Dim Value As Byte() = Nothing

			KeyString = secKey

			If source IsNot Nothing Then
				Try
					Using DES = Security.Cryptography.DES.Create
						Using mStream As New IO.MemoryStream
							Using cStream As New CryptoStream(mStream, DES.CreateEncryptor(Key, IV), CryptoStreamMode.Write)
								cStream.Write(source, 0, source.Length)
								cStream.FlushFinalBlock()

								Value = mStream.ToArray
							End Using
						End Using
					End Using
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		Public Function Encrypt(source As String, Optional secKey As String = "") As String
			If String.IsNullOrEmpty(source) Then Return ""

			Try
				Dim Content As Byte() = Text.Encoding.Unicode.GetBytes(source)
				Content = Encrypt(Content, secKey)
				Return Convert.ToBase64String(Content)
			Catch ex As Exception
				Return ""
			End Try
		End Function

		Public Function Encrypt(sourcePath As String, targetPath As String, secKey As String) As Boolean
			Dim Value As Boolean = False

			KeyString = secKey

			Try
				If IO.File.Exists(sourcePath) Then
					Dim Content As Byte() = IO.File.ReadAllBytes(sourcePath)

					If Content?.Length > 0 Then
						Using DES = Security.Cryptography.DES.Create
							Using fileStream As New IO.FileStream(targetPath, IO.FileMode.Create, IO.FileAccess.Write)
								Using cStream As New CryptoStream(fileStream, DES.CreateEncryptor(Key, IV), CryptoStreamMode.Write)
									cStream.Write(Content, 0, Content.Length)
									cStream.FlushFinalBlock()

									Value = True
								End Using
							End Using
						End Using
					End If
				End If
			Catch ex As Exception
			End Try

			Return Value
		End Function

		'-------------------------------------------------------------------

		Public Function Decrypt(source As Byte(), Optional secKey As String = "") As Byte()
			Dim Value As Byte() = Nothing

			KeyString = secKey

			If source?.Length > 0 Then
				Try
					Using DES = Security.Cryptography.DES.Create
						Using mStream As New IO.MemoryStream
							Using cStream As New CryptoStream(mStream, DES.CreateDecryptor(Key, IV), CryptoStreamMode.Write)
								cStream.Write(source, 0, source.Length)
								cStream.FlushFinalBlock()

								Value = mStream.ToArray
							End Using
						End Using
					End Using
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		Public Function Decrypt(source As String, Optional secKey As String = "") As String
			If String.IsNullOrEmpty(source) Then Return ""

			Try
				Dim Content As Byte() = Convert.FromBase64String(source)
				Content = Decrypt(Content, secKey)
				Return Text.Encoding.Unicode.GetString(Content)
			Catch ex As Exception
				Return ""
			End Try
		End Function

		Public Function Decrypt(sourcePath As String, targetPath As String, secKey As String) As Boolean
			Dim Value As Boolean = False

			KeyString = secKey

			Try
				If IO.File.Exists(sourcePath) Then
					Dim Content As Byte() = IO.File.ReadAllBytes(sourcePath)

					If Content?.Length > 0 Then
						Using DES = Security.Cryptography.DES.Create
							Using fileStream As New IO.FileStream(targetPath, IO.FileMode.Create, IO.FileAccess.Write)
								Using cStream As New CryptoStream(fileStream, DES.CreateDecryptor(Key, IV), CryptoStreamMode.Write)
									cStream.Write(Content, 0, Content.Length)
									cStream.FlushFinalBlock()

									Value = True
								End Using
							End Using
						End Using
					End If
				End If
			Catch ex As Exception
			End Try

			Return Value
		End Function

	End Class

	''' <summary>Rsa 签名算法</summary>
	Public NotInheritable Class Rsa

		''' <summary>默认密匙长度</summary>
		Public KeyLength As Integer = 1024

		''' <summary>默认加密填充方式</summary>
		Public EncryptionPadding As RSAEncryptionPadding = RSAEncryptionPadding.Pkcs1

		''' <summary>默认Hash算法</summary>
		Public HashAlgorithm As HashAlgorithmName = HashAlgorithmName.SHA1

		''' <summary>默认签名填充方式</summary>
		Public SignaturePadding As RSASignaturePadding = RSASignaturePadding.Pkcs1

		''' <summary>创建 RSA 对象</summary>
		Private Function CreateRsa() As System.Security.Cryptography.RSA
			Dim Rsa = Security.Cryptography.RSA.Create
			Rsa.KeySize = KeyLength

			Return Rsa
		End Function

#Region "加解密"

		''' <summary>使用公匙加密数据（RSA）</summary>
		Public Function Encrypt(source As Byte(), encryptPublicKey As String) As Byte()
			Dim Value As Byte() = Nothing

			Try
				If source?.Length > 0 AndAlso Not String.IsNullOrEmpty(encryptPublicKey) Then
					Using Rsa = CreateRsa()
						FromXml(Rsa, encryptPublicKey)
						Value = Rsa.Encrypt(source, EncryptionPadding)
					End Using
				End If
			Catch ex As Exception
			End Try

			Return Value
		End Function

		''' <summary>使用公匙加密数据（RSA）</summary>
		Public Function Encrypt(source As String, encryptPublicKey As String) As String
			Dim Value As String = ""

			If Not String.IsNullOrEmpty(source) AndAlso Not String.IsNullOrEmpty(encryptPublicKey) Then
				Dim byteValue = Text.Encoding.Unicode.GetBytes(source)
				byteValue = Encrypt(byteValue, encryptPublicKey)
				Value = Convert.ToBase64String(byteValue)
			End If

			Return Value
		End Function

		'-----------------------------------------------------------

		''' <summary>使用私匙解密数据（RSA）</summary>
		Public Function Decrypt(source As Byte(), decryptPrivateKey As String) As Byte()
			Dim Value As Byte() = Nothing

			If source?.Length > 0 AndAlso Not String.IsNullOrEmpty(decryptPrivateKey) Then
				Using Rsa = CreateRsa()
					FromXml(Rsa, decryptPrivateKey)
					Value = Rsa.Decrypt(source, EncryptionPadding)
				End Using
			End If

			Return Value
		End Function

		''' <summary>使用私匙解密数据（RSA）</summary>
		Public Function Decrypt(source As String, decryptPrivateKey As String) As String
			Dim Value As String = ""

			If Not String.IsNullOrEmpty(source) AndAlso Not String.IsNullOrEmpty(decryptPrivateKey) Then
				Dim byteValue = Convert.FromBase64String(source)
				byteValue = Decrypt(byteValue, decryptPrivateKey)
				Value = Text.Encoding.Unicode.GetString(byteValue)
			End If

			Return Value
		End Function

#End Region

#Region "私匙/公钥"

		'“BEGIN RSA PRIVATE KEY” => RSA.ImportRSAPrivateKey
		'“BEGIN PRIVATE KEY” => RSA.ImportPkcs8PrivateKey
		'“BEGIN ENCRYPTED PRIVATE KEY” => RSA.ImportEncryptedPkcs8PrivateKey
		'“BEGIN RSA PUBLIC KEY” => RSA.ImportRSAPublicKey
		'“BEGIN PUBLIC KEY” => RSA.ImportSubjectPublicKeyInfo


		''' <summary>Pkcs8 格式公钥转换</summary>
		''' <param name="publicKey">Pkcs8 格式公钥</param>
		Private Shared Function ConvertFromPublicKey(publicKey As String) As Byte()
			publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", String.Empty).
				Replace("-----END PUBLIC KEY-----", String.Empty).
				Replace("-----BEGIN CERTIFICATE-----", String.Empty).
				Replace("-----END CERTIFICATE-----", String.Empty)
			publicKey = Regex.Replace(publicKey, "\s+", String.Empty)

			Return Convert.FromBase64String(publicKey)
		End Function

		''' <summary>Pkcs8 格式私钥转换</summary>
		''' <param name="privateKey">Pkcs8 格式私钥</param>
		Private Shared Function ConvertFromPrivateKey(privateKey As String) As Byte()
			privateKey = privateKey.Replace("-----BEGIN PRIVATE KEY-----", String.Empty).
				Replace("-----END PRIVATE KEY-----", String.Empty).
				Replace("-----BEGIN CERTIFICATE-----", String.Empty).
				Replace("-----END CERTIFICATE-----", String.Empty)
			privateKey = Regex.Replace(privateKey, "\s+", String.Empty)

			Return Convert.FromBase64String(privateKey)
		End Function

		''' <summary>生成 RSA 私匙</summary>
		Public Function PrivateKey() As String
			Dim R = ""
			Using Rsa = CreateRsa()
				R = ToXml(Rsa, True)
			End Using
			Return R
		End Function

		''' <summary>生成 RSA 私匙</summary>
		Public Shared Function PrivateKey(rsa As System.Security.Cryptography.RSA) As String
			If rsa IsNot Nothing Then
				Return ToXml(rsa, True)
			Else
				Return ""
			End If
		End Function

		''' <summary>从私匙数据中获取 RSA公匙</summary>
		Public Function PublicKey(privateRSA As String) As String
			Dim R = ""
			Using Rsa = CreateRsa()
				FromXml(Rsa, privateRSA)
				R = ToXml(Rsa, False)
			End Using
			Return R
		End Function

		''' <summary>通过 RAS 获取公匙</summary>
		Public Shared Function PublicKey(rsa As System.Security.Cryptography.RSA, privateRSA As String) As String
			If rsa IsNot Nothing Then
				FromXml(rsa, privateRSA)
				Return ToXml(rsa, False)
			Else
				Return ""
			End If
		End Function

		''' <summary>是否有效的私钥，能获取到公钥才是有效的私钥</summary>
		Public Function IsKey(privateKey As String) As Boolean
			Dim R = False

			If Not String.IsNullOrWhiteSpace(privateKey) Then
				Try
					R = Not String.IsNullOrWhiteSpace(PublicKey(privateKey))
				Catch ex As Exception
				End Try
			End If

			Return R
		End Function

#End Region

#Region "加签/验签"

		''' <summary>通过私匙签名数据（RSA）</summary>
		Public Function Sign(source As String, signPriveteKey As String) As String
			Dim R = ""

			If Not String.IsNullOrEmpty(source) AndAlso Not String.IsNullOrEmpty(signPriveteKey) Then
				Using Rsa = CreateRsa()
					FromXml(Rsa, signPriveteKey)

					Dim byteValue = Text.Encoding.Unicode.GetBytes(source)
					byteValue = Rsa.SignData(byteValue, HashAlgorithm, SignaturePadding)
					R = Convert.ToBase64String(byteValue)
				End Using
			End If

			Return R
		End Function

		''' <summary>通过私匙签名数据（RSA）</summary>
		Public Function Sign(source As Byte(), signPriveteKey As String) As Byte()
			Dim R As Byte() = Nothing

			If source?.Length > 0 AndAlso Not String.IsNullOrEmpty(signPriveteKey) Then
				Using Rsa = CreateRsa()
					FromXml(Rsa, signPriveteKey)
					R = Rsa.SignData(source, HashAlgorithm, SignaturePadding)
				End Using
			End If

			Return R
		End Function

		''' <summary>通过私匙签名数据（RSA）</summary>
		Public Function Sign(source As IO.Stream, signPriveteKey As String) As Byte()
			Dim R As Byte() = Nothing

			If source IsNot Nothing AndAlso Not String.IsNullOrEmpty(signPriveteKey) Then
				Using Rsa = CreateRsa()
					FromXml(Rsa, signPriveteKey)
					R = Rsa.SignData(source, HashAlgorithm, SignaturePadding)
				End Using
			End If

			Return R
		End Function

		'--------------------------------------------------------------------------

		''' <summary>通过公匙验证签名数据（RSA）</summary>
		''' <param name="source">原始数据</param>
		''' <param name="signature">已经签名的数据</param>
		Public Function Verify(source As String, signature As String, verifyPublicKey As String) As Boolean
			If Not String.IsNullOrEmpty(source) AndAlso Not String.IsNullOrEmpty(signature) AndAlso Not String.IsNullOrEmpty(verifyPublicKey) Then
				Return Verify(Text.Encoding.Unicode.GetBytes(source), Convert.FromBase64String(signature), verifyPublicKey)
			Else
				Return False
			End If
		End Function

		''' <summary>通过公匙验证签名数据（RSA）</summary>
		''' <param name="source">原始数据</param>
		''' <param name="signature">已经签名的数据</param>
		Public Function Verify(source As Byte(), signature As Byte(), verifyPublicKey As String) As Boolean
			Dim R = False

			If source?.Length > 0 AndAlso signature?.Length > 0 AndAlso Not String.IsNullOrEmpty(verifyPublicKey) Then
				Using Rsa = CreateRsa()
					FromXml(Rsa, verifyPublicKey)
					R = Rsa.VerifyData(source, signature, HashAlgorithm, SignaturePadding)
				End Using
			End If

			Return R
		End Function

		''' <summary>通过公匙验证签名数据（RSA）</summary>
		''' <param name="source">原始数据</param>
		''' <param name="Signature">已经签名的数据</param>
		Public Function Verify(source As IO.Stream, signature As Byte(), verifyPublicKey As String) As Boolean
			Dim R = False

			If source IsNot Nothing AndAlso signature?.Length > 0 AndAlso Not String.IsNullOrEmpty(verifyPublicKey) Then
				Using Rsa = CreateRsa()
					FromXml(Rsa, verifyPublicKey)
					R = Rsa.VerifyData(source, signature, HashAlgorithm, SignaturePadding)
				End Using
			End If

			Return R
		End Function

		'--------------------------------------------------------------------------

		''' <summary>使用私钥基于 SHA-256 算法生成签名。</summary>
		''' <returns>经 Base64 编码的签名。</returns>
		Public Function SignWithSHA256(source As String, signPriveteKey As String) As String
			If source.IsEmpty OrElse signPriveteKey.IsEmpty Then Return ""

			Dim Ret = ""
			Dim byteValue = Text.Encoding.UTF8.GetBytes(source)
			Dim keyData = ConvertFromPrivateKey(signPriveteKey)

			Using Rsa = CreateRsa()
				Rsa.ImportPkcs8PrivateKey(keyData, 0)

				byteValue = Rsa.SignData(byteValue, HashAlgorithmName.SHA256, SignaturePadding)
				Ret = Convert.ToBase64String(byteValue)
			End Using

			Return Ret
		End Function

		''' <summary>使用公钥基于 SHA-256 算法验证签名。</summary>
		''' <param name="verifyPublicKey">PKCS#8 公钥（PEM 格式）。</param>
		''' <param name="source">待验证的文本数据。</param>
		''' <param name="signature">经 Base64 编码的待验证的签名。</param>
		''' <returns>验证结果。</returns>
		Public Function VerifyWithSHA256(source As String, signature As String, verifyPublicKey As String) As Boolean
			If source.IsEmpty OrElse signature.IsEmpty OrElse verifyPublicKey.IsEmpty Then Return False

			Dim Ret = False
			Dim byteValue = Text.Encoding.UTF8.GetBytes(source)
			Dim byteSignature = Convert.FromBase64String(signature)
			Dim keyData = ConvertFromPrivateKey(verifyPublicKey)

			Using Rsa = CreateRsa()
				Rsa.ImportSubjectPublicKeyInfo(keyData, 0)

				Ret = Rsa.VerifyData(byteValue, byteSignature, HashAlgorithmName.SHA256, SignaturePadding)
			End Using

			Return Ret
		End Function

#End Region

#Region "导入导出XML格式参数"

		''' <summary>从XML中获取密匙参数</summary>
		Private Shared Sub FromXml(ByRef rsa As System.Security.Cryptography.RSA, xml As String)
			Dim csp = New RSAParameters()

			Using reader = XmlReader.Create(New StringReader(xml))
				While reader.Read()
					If reader.NodeType <> XmlNodeType.Element Then Continue While
					Dim elName = reader.Name
					If elName = "RSAKeyValue" Then Continue While

					Do
						reader.Read()
					Loop While reader.NodeType <> XmlNodeType.Text AndAlso reader.NodeType <> XmlNodeType.EndElement

					If reader.NodeType = XmlNodeType.EndElement Then Continue While
					Dim value = reader.Value

					Select Case elName
						Case "Modulus"
							csp.Modulus = Convert.FromBase64String(value)
						Case "Exponent"
							csp.Exponent = Convert.FromBase64String(value)
						Case "P"
							csp.P = Convert.FromBase64String(value)
						Case "Q"
							csp.Q = Convert.FromBase64String(value)
						Case "DP"
							csp.DP = Convert.FromBase64String(value)
						Case "DQ"
							csp.DQ = Convert.FromBase64String(value)
						Case "InverseQ"
							csp.InverseQ = Convert.FromBase64String(value)
						Case "D"
							csp.D = Convert.FromBase64String(value)
					End Select
				End While
			End Using

			rsa.ImportParameters(csp)
		End Sub

		''' <summary>生成密匙</summary>
		Private Shared Function ToXml(ByRef rsa As System.Security.Cryptography.RSA, includePrivateParameters As Boolean) As String
			Dim csp = rsa.ExportParameters(includePrivateParameters)

			Dim sb As New Text.StringBuilder
			sb.Append("<RSAKeyValue>")
			sb.Append("<Modulus>").Append(Convert.ToBase64String(csp.Modulus)).Append("</Modulus>")
			sb.Append("<Exponent>").Append(Convert.ToBase64String(csp.Exponent)).Append("</Exponent>")

			If includePrivateParameters Then
				sb.Append("<P>").Append(Convert.ToBase64String(csp.P)).Append("</P>")
				sb.Append("<Q>").Append(Convert.ToBase64String(csp.Q)).Append("</Q>")
				sb.Append("<DP>").Append(Convert.ToBase64String(csp.DP)).Append("</DP>")
				sb.Append("<DQ>").Append(Convert.ToBase64String(csp.DQ)).Append("</DQ>")
				sb.Append("<InverseQ>").Append(Convert.ToBase64String(csp.InverseQ)).Append("</InverseQ>")
				sb.Append("<D>").Append(Convert.ToBase64String(csp.D)).Append("</D>")
			End If

			sb.Append("</RSAKeyValue>")
			Return sb.ToString
		End Function

#End Region

	End Class

	''' <summary>AES_GCM 加密算法</summary>
	Public NotInheritable Class AES_GCM

		''' <summary>AES GCM 加密</summary>
		''' <param name="source">需要加密的内容</param>
		''' <param name="key">密钥，32 位字符</param>
		''' <param name="nonce">始化向量，随机 12 位字符</param>
		''' <param name="additionalData">附加数据作</param>
		''' <param name="includeTags">加密后的结果是否包含 authentication tag</param>
		Public Shared Function Encrypt(source As String, key As String, nonce As String, Optional additionalData As String = "", Optional includeTags As Boolean = True) As String
			If source.IsEmpty OrElse key.IsEmpty OrElse nonce.IsEmpty Then Return ""

			Dim byteSource = Encoding.UTF8.GetBytes(source)
			Dim byteKey = Encoding.UTF8.GetBytes(key)
			Dim byteNonce = Encoding.UTF8.GetBytes(nonce)
			Dim byteAdditionalData = If(additionalData.IsEmpty, Nothing, Encoding.UTF8.GetBytes(additionalData))

			Dim byteReturn(byteSource.Length - 1) As Byte
			Dim byteTags(15) As Byte

			Using aesGcm As New AesGcm(byteKey)
				aesGcm.Encrypt(byteNonce, byteSource, byteReturn, byteTags, byteAdditionalData)
			End Using

			' 不包含 Tags 直接返回
			If Not includeTags Then Return Convert.ToBase64String(byteReturn)

			ReDim Preserve byteReturn(byteReturn.Length + 15)
			Buffer.BlockCopy(byteTags, 0, byteReturn, byteReturn.Length, byteTags.Length)
			Return Convert.ToBase64String(byteReturn)
		End Function

		''' <summary>AES GCM 解密</summary>
		''' <param name="source">需要解密的内容</param>
		''' <param name="key">密钥，32 位字符</param>
		''' <param name="nonce">始化向量，随机 12 位字符</param>
		''' <param name="additionalData">附加数据作</param>
		Public Shared Function Decrypt(source As String, key As String, nonce As String, Optional additionalData As String = "") As String
			If source.IsEmpty OrElse key.IsEmpty OrElse nonce.IsEmpty Then Return ""

			Dim byteSource = Convert.FromBase64String(source)
			Dim byteKey = Encoding.UTF8.GetBytes(key)
			Dim byteNonce = Encoding.UTF8.GetBytes(nonce)
			Dim byteAdditionalData = If(additionalData.IsEmpty, Nothing, Encoding.UTF8.GetBytes(additionalData))

			Dim byteTags(15) As Byte
			Buffer.BlockCopy(byteSource, byteSource.Length - 16, byteTags, 0, byteTags.Length)
			ReDim Preserve byteSource(byteSource.Length - 17)
			Dim byteReturn(byteSource.Length - 1) As Byte

			Using aesGcm As New AesGcm(byteKey)
				aesGcm.Decrypt(byteNonce, byteSource, byteTags, byteReturn, byteAdditionalData)
			End Using

			Return Encoding.UTF8.GetString(byteReturn)
		End Function

	End Class

	''' <summary>签名算法</summary>
	Public NotInheritable Class Sign

		''' <summary>根据加密不同计算密匙签名</summary>
		''' <param name="mode">签名方式</param>
		''' <param name="account">账号 / 客户端标识</param>
		''' <param name="key">密匙 / 客户端密钥</param>
		''' <param name="appId">使用接口的应用标识</param>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="parameters">请求参数</param>
		''' <param name="ticks">请求时间戳(UNIX)</param>
		''' <param name="rnd">请求随机参数</param>
		Public Shared Function Client(mode As ClientSignEnum,
									  account As String,
									  key As Guid,
									  Optional appId As String = "",
									  Optional method As String = "",
									  Optional path As String = "",
									  Optional parameters As IDictionary(Of String, Object) = Nothing,
									  Optional ticks As Long = 0,
									  Optional rnd As String = "") As String
			Dim Sign = ""

			If key <> Guid.Empty Then
				Select Case mode
					Case ClientSignEnum.NONE
						' NONE: 不验证，仅仅分析账号

						Sign = account

					Case ClientSignEnum.PASSWORD
						' HASH: MD5（账号+密码）

						Sign = MakeSign(account, key, appId)

					Case ClientSignEnum.COMMAND
						' COMMAND: MD5(账号+密码+请求方式+地址+参数排序)

						method = method.EmptyValue.ToUpper
						path = path.EmptyValue.ToLower

						Sign = MakeSign(account, key, appId, method, path, ParameterSortString(parameters))

					Case ClientSignEnum.SIGN
						' SIGN: MD5(账号+密码+5分钟时间迭代次数+请求方式+地址+参数排序)

						method = method.EmptyValue.ToUpper
						path = path.EmptyValue.ToLower

						Dim time = Math.Floor(SYS_NOW_DATE.UnixTicks / 300)

						Sign = MakeSign(account, key, time, appId, method, path, ParameterSortString(parameters))

					Case ClientSignEnum.SIGN_RND
						' SIGN_RND: MD5(账号+密码+10分钟时间迭代次数+随机码+请求方式+地址+参数排序)

						method = method.EmptyValue.ToUpper
						path = path.EmptyValue.ToLower

						Dim time = Math.Floor(SYS_NOW_DATE.UnixTicks / 600)
						rnd = rnd.EmptyValue(Guid.NewGuid.ToString)

						Sign = MakeSign(account, key, time, rnd, appId, method, path, ParameterSortString(parameters))

					Case ClientSignEnum.SIGN_TIME
						' SIGN_TIME: MD5(账号+密码+时间戳+请求方式+地址+参数排序)

						method = method.EmptyValue.ToUpper
						path = path.EmptyValue.ToLower

						Dim time = If(ticks < 1, SYS_NOW_DATE.JsTicks, ticks)

						Sign = MakeSign(account, key, time, appId, method, path, ParameterSortString(parameters))

					Case ClientSignEnum.SIGN_TIME_RND
						' SIGN_TIME_RND: MD5(账号+密码+时间戳+随机码+请求方式+地址+参数排序)

						method = method.EmptyValue.ToUpper
						path = path.EmptyValue.ToLower

						Dim time = If(ticks < 1, SYS_NOW_DATE.JsTicks, ticks)
						rnd = rnd.EmptyValue(Guid.NewGuid.ToString)


						Sign = MakeSign(account, key, time, rnd, appId, method, path, ParameterSortString(parameters))
				End Select
			End If

			Return Sign
		End Function

		''' <summary>获取参数值</summary>
		Public Shared Function ParameterSortString(parameters As IDictionary(Of String, Object)) As String
			Return parameters?.
				ToJson.ToJsonNameValues?.   ' 转单层对象
				Where(Function(x) x.Value.NotEmpty).
				Select(Function(x) $"{x.Key}={TypeExtension.ToObjectString(x.Value)}".ToLower). ' 转字符串
				OrderBy(Function(x) x, StringComparer.OrdinalIgnoreCase). ' 排序
				JoinString("&")
		End Function

		''' <summary>生成签名</summary>
		Private Shared Function MakeSign(ParamArray datas As Object()) As String
			'CON.Warn(datas)
			Return String.Join(Chr(10), datas).MD5
		End Function

	End Class

End Namespace