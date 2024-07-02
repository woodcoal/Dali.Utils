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
' 	身份证相关操作
'
' 	name: Helper.CardIDHelper
' 	create: 2019-03-23
' 	memo: 身份证相关操作
' 	
' ------------------------------------------------------------

Imports System.Text.RegularExpressions

Namespace Helper

	''' <summary>身份证相关操作</summary>
	Public NotInheritable Class CardIDHelper

		''' <summary>验证身份证号码</summary>
		''' <param name="Id">身份证号码</param>
		''' <returns>验证成功为True，否则为False</returns>
		Public Shared Function Validate(id As String) As Boolean
			If id.NotEmpty AndAlso Regex.IsMatch(id, "^((\d{17}(\d|x))|(\d{15})$)") Then
				' 校验 Code ，仅18位有效
				Dim Code = id.Right(1)

				' 转换成长整数后转字符串
				id = id.Left(17).ToLong

				If id.Length = 17 Then
					' 验证18位身份证号 GB11643-1999标准
					Dim Address As String = "x11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91x"
					If Address.Contains(id.Remove(2)) Then
						'验证生日
						Dim Birth = Date.Parse(id.Substring(6, 8).Insert(6, "-").Insert(4, "-"))
						If Birth > New Date(1900, 1, 1) AndAlso Birth < SYS_NOW_DATE Then
							'校验码验证
							Return ValidateCode(id) = Code
						End If
					End If

				ElseIf id.Length = 15 Then
					' 验证15位身份证号
					Dim cID As Long = 0
					'数字验证
					If Long.TryParse(id, cID) AndAlso cID > Math.Pow(10, 14) Then
						'省份验证
						Dim Address As String = "x11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91x"
						If Address.Contains(id.Remove(2)) Then
							'验证生日
							Dim Birth = Date.Parse("19" & id.Substring(6, 6).Insert(4, "-").Insert(2, "-"))
							If Birth > New Date(1900, 1, 1) AndAlso Birth < SYS_NOW_DATE Then
								'校验码验证
								Return True
							End If
						End If
					End If
				End If
			End If

			Return False
		End Function

		''' <summary>身份证 15 位升 18 位</summary>
		Public Shared Function Update(id As String) As String
			If id?.Length = 15 Then
				id = id.Left(6) & "19" & id.Right(9)
				Return id & ValidateCode(id)
			Else
				Return ""
			End If
		End Function

		''' <summary>从身份证获取用户所在省份、生日、性别信息</summary> 
		''' <param name="ID">身份证字符串</param> 
		''' <returns>如：福建,1978-06-30,男</returns> 
		Public Shared Function Information(id As String) As (Validate As Boolean, Province As String, Birth As Date, Gender As GenderEnum?)
			Dim IsValidate = Validate(id)
			Dim Province = ""
			Dim Birth As Date
			Dim Gender = Nothing

			If IsValidate Then
				'地区
				Dim Provinces = {Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, "北京", "天津", "河北", "山西", "内蒙古", Nothing, Nothing, Nothing, Nothing, Nothing, "辽宁", "吉林", "黑龙江", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, "上海", "江苏", "浙江", "安微", "福建", "江西", "山东", Nothing, Nothing, Nothing, "河南", "湖北", "湖南", "广东", "广西", "海南", Nothing, Nothing, Nothing, "重庆", "四川", "贵州", "云南", "西藏", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, "陕西", "甘肃", "青海", "宁夏", "新疆", Nothing, Nothing, Nothing, Nothing, Nothing, "台湾", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, "香港", "澳门", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, "国外"}

				id = id.Replace("x", "a", StringComparison.OrdinalIgnoreCase)

				Province = Provinces(Integer.Parse(id.Substring(0, 2)))

				If id.Length = 15 Then
					Birth = Date.Parse(id.Substring(6, 6).Insert(4, "-").Insert(2, "-"))
				Else
					Birth = Date.Parse(id.Substring(6, 8).Insert(6, "-").Insert(4, "-"))
				End If

				If id.Length = 18 Then
					Gender = If(Integer.Parse(id.Substring(16, 1)) Mod 2 = 1, GenderEnum.MALE, GenderEnum.FEMALE)
				Else
					Gender = If(Integer.Parse(id.Substring(14, 1)) Mod 2 = 1, GenderEnum.MALE, GenderEnum.FEMALE)
				End If
			End If

			Return (IsValidate, Province, Birth, Gender)
		End Function

		'''<summary>生成校验码</summary> 
		''' <param name="ID">身份证前17位</param>
		Private Shared Function ValidateCode(id As String) As String
			If id?.Length > 16 Then
				id = id.Substring(0, 17).ToLong
				If id.Length = 17 Then
					Dim VarifyCode = {"1", "0", "x", "9", "8", "7", "6", "5", "4", "3", "2"}
					Dim CodeValue = {7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2}

					Dim Sum As Integer = 0
					For I As Integer = 0 To 16
						Sum += CodeValue(I) * Integer.Parse(id(I))
					Next

					Return VarifyCode(Sum Mod 11)
				End If
			End If

			Return ""
		End Function

	End Class
End Namespace

