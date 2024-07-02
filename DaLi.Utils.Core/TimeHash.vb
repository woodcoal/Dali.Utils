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
' 	时间一次性密码
'
' 	name: TimeHash
' 	create: 2021-07-31
' 	memo: 时间一次性密码
' 	
' ------------------------------------------------------------

Imports System.Security.Cryptography

''' <summary>时间一次性密码</summary>
''' <remarks>时间次数与密匙合并后使用MD5</remarks>
Public NotInheritable Class TimeHash

	Public Shared Function GoogleAuthenticator(secret As String, Optional isBase32 As Boolean = True) As String
		Return HashCodeByTime(secret, 30, 6, isBase32)
	End Function

	''' <summary>默认算法，间隔5分钟，返回32个字符</summary>
	''' <param name="secret">密匙</param>
	Public Shared Function HashCode(secret As String) As String
		Return HashCodeByTime(secret, 300, True)
	End Function

	''' <summary>按照时间生成哈希编码（Google Authenticator）</summary>
	''' <param name="secret">秘钥</param>
	''' <param name="duration">时间迭代间隔，谷歌默认 30 秒</param>
	''' <param name="digits">返回数值长度</param>
	''' <param name="isBase32">密钥是否 Base32 编码，对于移动端默认使用了 Base32 编码</param>
	Public Shared Function HashCodeByTime(secret As String, Optional duration As Integer = 30, Optional digits As Integer = 6, Optional isBase32 As Boolean = True) As String
		If String.IsNullOrEmpty(secret) OrElse duration < 1 Then
			Return ""
		Else
			Return HashCodeByCount(secret, TimeCounter(SYS_NOW_DATE, duration), digits， isBase32)
		End If
	End Function

	''' <summary>按照时间生成哈希编码</summary>
	''' <param name="secret">秘钥</param>
	''' <param name="duration">时间迭代间隔，谷歌默认 30 秒</param>
	''' <param name="mode">返回模式，16位(False)或者32位(True,默认)</param>
	Public Shared Function HashCodeByTime(secret As String, Optional duration As Integer = 30, Optional mode As Boolean = True) As String
		If String.IsNullOrEmpty(secret) OrElse duration < 5 Then
			Return ""
		Else
			Return HashCodeByCount(secret, TimeCounter(SYS_NOW_DATE, duration), mode)
		End If
	End Function

	''' <summary>按时间迭代生成密匙，迭代当前及上一次，下一次数据组合</summary>
	''' <param name="secret">秘钥</param>
	''' <param name="duration">时间迭代间隔，谷歌默认 30 秒</param>
	''' <param name="mode">返回模式，16位(False)或者32位(True,默认)</param>
	Public Shared Function HashCodeByTimeAll(secret As String, Optional duration As Integer = 30, Optional mode As Boolean = True) As String()
		If String.IsNullOrEmpty(secret) OrElse duration < 5 Then
			Return Nothing
		Else
			Dim Count = TimeCounter(SYS_NOW_DATE, duration)

			Return {HashCodeByCount(secret, Count - 1, mode), HashCodeByCount(secret, Count, mode), HashCodeByCount(secret, Count + 1, mode)}
		End If
	End Function

	''' <summary>按照次数生成哈希编码</summary>
	''' <param name="secret">秘钥</param>
	''' <param name="iterationNumber">迭代次数</param>
	''' <param name="mode">返回模式，16位(False)或者32位(True,默认)</param>
	Public Shared Function HashCodeByCount(secret As String, iterationNumber As Long, Optional mode As Boolean = True) As String
		If String.IsNullOrEmpty(secret) OrElse iterationNumber < 1 Then
			Return ""
		Else
			Return $"[{iterationNumber}]{secret}".MD5(mode)
		End If
	End Function

	''' <summary>按照次数生成哈希编码（Google Authenticator）</summary>
	''' <param name="secret">秘钥</param>
	''' <param name="iterationNumber">迭代次数</param>
	''' <param name="digits">返回数值长度</param>
	''' <param name="isBase32">密钥是否 Base32 编码，对于移动端默认使用了 Base32 编码</param>
	Public Shared Function HashCodeByCount(secret As String, iterationNumber As Long, Optional digits As Integer = 6, Optional isBase32 As Boolean = True) As String
		Dim counter = BitConverter.GetBytes(iterationNumber)
		If BitConverter.IsLittleEndian Then Array.Reverse(counter)

		Dim key = If(isBase32, Misc.Base32.ToBytes(secret), Text.Encoding.UTF8.GetBytes(secret))
		Dim password As Integer

		Using hmac As New HMACSHA1(key)
			Dim hash = hmac.ComputeHash(counter)
			Dim offset = hash(hash.Length - 1) And &HF

			' Convert the 4 bytes into an integer, ignoring the sign.
			Dim binary = (hash(offset) And &H7F) << 24 Or (hash(offset + 1) And &HFF) << 16 Or (hash(offset + 2) And &HFF) << 8 Or (hash(offset + 3) And &HFF)

			password = binary Mod CInt(Math.Pow(10, digits)) ' 6 digits
		End Using

		Return password.ToString(New String("0"c, digits))
	End Function

	''' <summary>迭代次数（最少 5 秒迭一次）</summary>
	Private Shared ReadOnly Property TimeCounter(Optional timeNow As Date = Nothing, Optional duration As Integer = 30) As Long
		Get
			Return (SYS_NOW_DATE.ToUniversalTime - New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / duration
		End Get
	End Property
End Class
