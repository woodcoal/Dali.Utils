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
' 	模型枚举相关扩展操作
'
' 	name: Extension.ModelExtension
' 	create: 2020-11-10
' 	memo: 模型枚举相关扩展操作
' 	
' 	2020-11-10:	create this file
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>模型枚举相关扩展操作</summary>
	Public Module ModelExtension

		''' <summary>频率描述</summary>
		''' <param name="this">频率</param>
		''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		<Extension>
		Public Function Description(this As TimeFrequencyEnum, Optional delayValue As Integer = 0) As String
			If this = TimeFrequencyEnum.NONE Or delayValue < 1 Then
				Return "不限"
			Else
				Return $"每{delayValue}{EnumExtension.Description(this)}"
			End If
		End Function

		''' <summary>字段类型是否数字</summary>
		<Extension>
		Public Function IsNumber(this As FieldTypeEnum) As Boolean
			Select Case this
				Case FieldTypeEnum.NUMBER, FieldTypeEnum.INTEGER, FieldTypeEnum.LONG, FieldTypeEnum.SINGLE, FieldTypeEnum.DOUBLE， FieldTypeEnum.BYTES
					Return True
				Case Else
					Return False
			End Select
		End Function

		''' <summary>字段类型是否为字符串</summary>
		<Extension>
		Public Function IsString(this As FieldTypeEnum) As Boolean
			Select Case this
				Case FieldTypeEnum.TEXT, FieldTypeEnum.ASCII, FieldTypeEnum.CHINESE, FieldTypeEnum.LOWER_CASE, FieldTypeEnum.UPPER_CASE, FieldTypeEnum.EMAIL, FieldTypeEnum.URL, FieldTypeEnum.FOLDER
					Return True
				Case Else
					Return False
			End Select
		End Function

		''' <summary>更新字典数据，标题包含小数点的需要进一步转换为下级字典</summary>
		<Extension>
		Public Function ToDeepDictionary(ByRef this As IDictionary(Of String, Object)) As Dictionary(Of String, Object)
			If this Is Nothing OrElse this.Count < 1 Then Return this

			Dim Ret As New Dictionary(Of String, Object)

			For Each Key In this.Keys
				Dim Value = this(Key)

				' 移除空格以及首尾多余的点
				If Not String.IsNullOrWhiteSpace(Key) Then
					Key = Key.Replace("."c, " "c)
					Key = Key.TrimFull
					Key = Key.Replace(" "c, "."c)
				Else
					Key = ""
				End If

				' 过滤掉多余点后，将不会出现 A.B 中 A B 为空的情况
				Dim P = Key.IndexOf("."c)
				If P > -1 Then
					Dim Bkey = Key.Substring(0, P).Trim
					Dim Ekey = Key.Substring(P + 1).Trim

					If Not Ret.ContainsKey(Bkey) Then Ret.Add(Bkey, New Dictionary(Of String, Object))
					Dim d = If(TryCast(Ret(Bkey), Dictionary(Of String, Object)), New Dictionary(Of String, Object))

					If Not d.ContainsKey(Ekey) Then d.Add(Ekey, Value)

					' 包含更多的点，则继续更新
					If Ekey.Contains("."c) Then ToDeepDictionary(d)

					'Ret(Bkey) = d
				Else
					If Not Ret.ContainsKey(Key) Then Ret.Add(Key, Value)
				End If
			Next

			this = Ret

			Return this
		End Function

		''' <summary>更新字典数据,将深层的字典数据转换成平级数据，键之间用小数点间隔</summary>
		<Extension>
		Public Function ToSingleDictionary(ByRef this As IDictionary(Of String, Object), Optional keepSource As Boolean = False) As Dictionary(Of String, Object)
			If this Is Nothing OrElse this.Count < 1 Then Return this

			Dim Ret As New KeyValueDictionary

			' 处理，如果数据处理过则返回 True
			Dim objConvert As Func(Of Object, String, Boolean) = Function(obj, prefix)
																	 If obj Is Nothing Then Return False

																	 ' 非列表数据不处理
																	 Dim objType = obj.GetType
																	 If Not objType.IsEnumerable Then Return False

																	 ' 前缀，无前缀则无需处理
																	 prefix = If(prefix.IsEmpty, "", prefix & ".")

																	 If objType.IsString Then
																		 ' 字符串，防止被枚举
																	 ElseIf objType.IsDictionary Then
																		 ' 是否字典对象（字典要早于列表）
																		 Dim dic = TryCast(obj, IDictionary)

																		 For Each key In dic.Keys
																			 Dim value = dic(key)
																			 Dim pre = prefix & key
																			 Dim flag = objConvert(value, pre)

																			 If keepSource OrElse Not flag Then Ret.Add(pre, value)
																		 Next

																		 Return True
																	 ElseIf objType.IsList OrElse objType.IsArray Then
																		 ' 是否列表对象
																		 Dim list = TryCast(obj, IEnumerable)

																		 Dim I = 0
																		 For Each value In list
																			 I += 1

																			 Dim pre = prefix & I
																			 Dim flag = objConvert(value, pre)

																			 If keepSource OrElse Not flag Then Ret.Add(pre, value)
																		 Next

																		 Return True
																	 End If

																	 Return False
																 End Function

			objConvert(this, "")

			this = Ret
			Return this
		End Function



	End Module
End Namespace