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
' 	产生随机字符串
'
' 	name: Helper.RandomHelper
' 	create: 2019-03-13
' 	memo: 产生随机字符串
' 	
' ------------------------------------------------------------

Namespace Helper

	''' <summary>产生随机字符串</summary> 
	Public NotInheritable Class RandomHelper

		Private Const LOWER_CHARS As String = "abcdefghijklmnopqrstuvwxyz"
		Private Const UPPER_CHARS As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
		Private Const NUMBER_CHARS As String = "0123456789"

		''' <summary>获取指定长度随机的数字字符串</summary> 
		''' <param name="length">待获取随机字符串长度</param> 
		Public Shared Function Number(length As Integer) As String
			Return Make(NUMBER_CHARS, length)
		End Function

		''' <summary>获取指定长度随机的字母字符串（包含大小写字母）</summary> 
		''' <param name="length">待获取随机字符串长度</param> 
		Public Shared Function Chars(length As Integer) As String
			Return Make(LOWER_CHARS & UPPER_CHARS, length)
		End Function


		''' <summary>获取指定长度随机的字母＋数字混和字符串（包含大小写字母）</summary> 
		Public Shared Function Mix(length As Integer) As String
			Return Make(LOWER_CHARS & UPPER_CHARS & NUMBER_CHARS, length)
		End Function

		''' <summary>从指定字符串中抽取指定长度的随机字符串</summary> 
		''' <param name="source">源字符串</param> 
		''' <param name="length">待获取随机字符串长度</param>
		Public Shared Function Make(source As String, length As Integer) As String
			Dim Value = ""

			If source?.Length > 0 Then
				length = length.Range(1, 1024)

				' 使用RNGCryptoServiceProvider 做种，可以在一秒内产生的随机数重复率非常的低，对于以往使用时间做种的方法是个升级
				Dim RndBytes = New Byte(3) {}

				Using RNG = Security.Cryptography.RandomNumberGenerator.Create
					RNG.GetBytes(RndBytes)
				End Using

				Dim Seed = BitConverter.ToInt32(RndBytes, 0)
				With New System.Random(Seed)
					For I = 0 To length - 1
						Value &= source.Substring(.Next(0, source.Length - 1), 1)
					Next
				End With
			End If

			Return Value
		End Function

		''' <summary>GUID</summary>
		Public Shared Function Guid() As String
			Return System.Guid.NewGuid.ToString
		End Function

		''' <summary>16位字符串</summary>
		Public Shared Function Hash() As String
			Return Guid.MD5(False)
		End Function

		''' <summary>随机产生常用汉字</summary>
		''' <param name="length">要产生汉字的个数</param>
		''' <returns>常用汉字</returns>
		''' <remarks>
		''' 随机中文汉字验证码的基本原理:汉字区位码表区位码、国标码与机内码的转换关系
		''' 1）区位码先转换成十六进制数表示
		''' 2）（区位码的十六进制表示）＋2020H＝国标码；
		''' 3）国标码＋8080H＝机内码
		''' 举例：以汉字“大”为例，“大”字的区内码为20831、区号为20，位号为832、将区位号2083转换为十六进制表示为1453H3、1453H＋2020H＝3473H，得到国标码3473H4、3473H＋8080H＝B4F3H，得到机内码为B4F3H
		''' 常用汉字在16-55区,其中55区有几个空的,故要将其去除.
		''' </remarks>
		Public Shared Function ChineseWords(length As Integer) As String
			Dim Value As String = ""


			length = length.Range(1, 1024)

			' 使用RNGCryptoServiceProvider 做种，可以在一秒内产生的随机数重复率非常的低，对于以往使用时间做种的方法是个升级
			Dim RndBytes = New Byte(3) {}
			Using RNG = Security.Cryptography.RandomNumberGenerator.Create
				RNG.GetBytes(RndBytes)
			End Using
			Dim _RND = New System.Random(BitConverter.ToInt32(RndBytes, 0))

			For I As Integer = 1 To length
				'获取区码(常用汉字的区码范围[fan wei]为16-55)
				Dim regionCode As Integer = _RND.Next(16, 56)

				'获取位码(位码范围[fan wei]为1-94 由于55区的90,91,92,93,94为空,故将其排除)
				Dim positionCode As Integer

				If regionCode > 54 Then
					'55区排除 90,91,92,93,94
					positionCode = _RND.Next(1, 90)
				Else
					positionCode = _RND.Next(1, 95)
				End If

				' 转换区位码为机内码
				regionCode += 160
				positionCode += 160

				' 转换为汉字
				Dim bytes() As Byte = {CByte(regionCode), CByte(positionCode)}

				Value &= GB2312.GetString(bytes)
			Next

			Return Value
		End Function

	End Class

End Namespace
