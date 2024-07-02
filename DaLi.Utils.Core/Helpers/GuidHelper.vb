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
' 	条件化生成GUID
'
' 	name: Helper.GuidHelper
' 	create: 2021-03-12
' 	memo: 条件化生成GUID，根据数据库要求生成不同规格的GUID
' 	
' ------------------------------------------------------------

Imports System.Security.Cryptography

Namespace Helper

	''' <summary>条件化生成GUID</summary>
	''' <remarks>
	''' 从不同数据库GUID的处理方式来看，显然没有一个通用的有序GUID生成算法，我们针对不同应用程序分别对待。经过一番实验之后， 我确定了大部分为下面3中情况：
	''' 生成的GUID 按照字符串顺序排列
	''' 生成的GUID 按照二进制的顺序排列
	''' 生成的GUID 像SQL Server, 按照末尾部分排列
	''' https://www.cnblogs.com/CameronWu/p/guids-as-fast-primary-keys-under-multiple-database.html
	''' https://raw.githubusercontent.com/jhtodd/SequentialGuid/
	''' 
	''' 本系统做了简单改进，仅随机生成10位字节，6位时间码改成5位，那么只能记录34年左右的数据，那么用当前时间减去 2020-1-1 开始计算。
	''' 时间码第6位改成顺序码，防止重复，只要1ms内不产生256个数据则将不会重复
	''' 
	''' Database	|	GUID Column			|	SequentialGuidType Value 
	''' ------------------------------------------------------------------
	''' SQL Server	|	uniqueidentifier	| 	SequentialAtEnd
	''' MySQL		|	char(36)			|	SequentialAsString 
	''' Oracle		|	raw(16)				|	SequentialAsBinary 
	''' PostgreSQL	|	uuid				|	SequentialAsString 
	''' SQLite		|	varies				|	varies 
	''' 
	''' SQLite 没有 GUID 类型字段
	''' </remarks>
	Public NotInheritable Class GuidHelper

		''' <summary>记录次序</summary>
		Private Shared _Index As Byte

		''' <summary>最后的时间戳</summary>
		Private Shared _Last As Long

		''' <summary>生成GUID</summary>
		Public Shared Function Create(Optional sequentialOption As GuidEnum = GuidEnum.END_SEQUENTIAL) As Guid
			If sequentialOption = GuidEnum.NONE Then Return Guid.NewGuid

			Dim randomBytes As Byte() = New Byte(9) {}

			' 创建随机数
			Using _RandomGenerator = RandomNumberGenerator.Create
				_RandomGenerator.GetBytes(randomBytes)
			End Using


			' 637134336000000000 = New Date(2020, 1, 1).Ticks
			' 63713433600000 = New Date(2020, 1, 1).Ticks / 10000

			Dim timestamp As Long = (Date.UtcNow.Ticks / 10000L) - 63713433600000
			Dim timestampBytes As Byte() = New Byte(5) {}

			If _Last <> timestamp Then
				_Last = timestamp
				_Index = 0
			Else
				If _Index > 254 Then
					_Index = 0
				Else
					Try
						_Index += 1
					Catch ex As Exception
						'Log.Err("Err:" & Index)
						_Index += 1
					End Try
				End If
			End If

			Buffer.SetByte(timestampBytes, 0, _Index)
			Buffer.BlockCopy(BitConverter.GetBytes(timestamp), 0, timestampBytes, 1, 5)

			' 为什么字符串顺序和二进制顺序有区别？
			' 因为在 little - endian system 环境中.NET处理GUID和string的方式并不是你想象的和其他运行.net的环境中的那样。
			If BitConverter.IsLittleEndian Then
				System.Array.Reverse(timestampBytes)
			End If

			Dim guidBytes As Byte() = New Byte(15) {}

			Select Case sequentialOption
				Case GuidEnum.STRING_SEQUENTIAL, GuidEnum.BINARY_SEQUENTIAL
					Buffer.BlockCopy(timestampBytes, 0, guidBytes, 0, 6)
					Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10)

					If sequentialOption = GuidEnum.STRING_SEQUENTIAL AndAlso BitConverter.IsLittleEndian Then
						System.Array.Reverse(guidBytes, 0, 4)
						System.Array.Reverse(guidBytes, 4, 2)
					End If

				Case GuidEnum.END_SEQUENTIAL
					Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10)
					Buffer.BlockCopy(timestampBytes, 0, guidBytes, 10, 6)
			End Select

			Return New System.Guid(guidBytes)
		End Function

	End Class
End Namespace
