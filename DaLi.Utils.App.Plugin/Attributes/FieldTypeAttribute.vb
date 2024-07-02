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
' 	字段类型验证
'
' 	name: Attribute.FieldTypeAttribute
' 	create: 2023-02-14
' 	memo: 字段类型验证
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Attribute

	''' <summary>字段类型验证</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
	Public NotInheritable Class FieldTypeAttribute
		Inherits ValidationAttribute

		''' <summary>检查模式</summary>
		Public ReadOnly Property Type As FieldValidateEnum

		''' <summary>是否允许空值</summary>
		Public ReadOnly Property EnNull As Boolean

		Public Sub New(Optional type As FieldValidateEnum = FieldValidateEnum.ASCII, Optional enNull As Boolean = True)
			Me.Type = type
			Me.EnNull = enNull
		End Sub

		''' <summary>校验方式</summary>
		Protected Overrides Function IsValid(value As Object, validationContext As ValidationContext) As ValidationResult
			If String.IsNullOrEmpty(value) Then
				If EnNull Then
					Return ValidationResult.Success
				Else
					Return New ValidationResult(ErrorMessage.EmptyValue("字段值不能为空"), {validationContext?.MemberName})
				End If
			End If

			' 值类型
			'Dim valueType = value.GetType
			'If Not (valueType.IsString OrElse valueType.IsNumber OrElse valueType.IsDate) Then Return tip("非字符字段无法验证")

			' 默认错误
			Dim str = value.ToString

			Select Case Type
				Case FieldValidateEnum.GUID
					If str.IsGUID Then Return ValidationResult.Success

				Case FieldValidateEnum.ASCII
					If str.IsAscii Then Return ValidationResult.Success

				Case FieldValidateEnum.XML
					If str.IsXml Then Return ValidationResult.Success

				Case FieldValidateEnum.JSON
					If str.IsJson Then Return ValidationResult.Success

				Case FieldValidateEnum.CHINESE
					If str.IsChinese Then Return ValidationResult.Success

				Case FieldValidateEnum.UPPER_CASE
					If str.ToUpper = str Then Return ValidationResult.Success

				Case FieldValidateEnum.LOWER_CASE
					If str.ToLower = str Then Return ValidationResult.Success

				Case FieldValidateEnum.DATETIME
					If str.IsDateTime Then Return ValidationResult.Success

				Case FieldValidateEnum.TRISTATE
					If {"true", "false", "default", "UseDefault"}.Contains(str, StringComparer.OrdinalIgnoreCase) Then Return ValidationResult.Success

				Case FieldValidateEnum.BOOLEAN
					If {"true", "false"}.Contains(str, StringComparer.OrdinalIgnoreCase) Then Return ValidationResult.Success

				Case FieldValidateEnum.EMAIL
					If str.IsEmail Then Return ValidationResult.Success

				Case FieldValidateEnum.URL
					If str.IsUrl Then Return ValidationResult.Success

				Case FieldValidateEnum.IP
					If str.IsIP Then Return ValidationResult.Success

				Case FieldValidateEnum.IPv4
					If str.IsIPv4 Then Return ValidationResult.Success

				Case FieldValidateEnum.IPv6
					If str.IsIPv6 Then Return ValidationResult.Success

				Case FieldValidateEnum.MOBILEPHONE
					' 如果 str 为 0 且忽略 nullstr 则也返回正确，因为此时字段可能为数字字段
					If str = "0" AndAlso EnNull Then
						Return ValidationResult.Success
					ElseIf str.IsMobilePhone Then
						Return ValidationResult.Success
					End If

				Case FieldValidateEnum.PHONE
					' 如果 str 为 0 且忽略 nullstr 则也返回正确，因为此时字段可能为数字字段
					If str = "0" AndAlso EnNull Then
						Return ValidationResult.Success
					ElseIf str.IsPhone Then
						Return ValidationResult.Success
					End If

				Case FieldValidateEnum.PATH
					If str.IsPath Then Return ValidationResult.Success

				Case FieldValidateEnum.CARDID
					If str.IsCardID Then Return ValidationResult.Success

				Case FieldValidateEnum.LETTERNUMBER
					If str.IsLetterNumber Then Return ValidationResult.Success

				Case FieldValidateEnum.USERNAME
					If str.IsUserName(1000, False) Then Return ValidationResult.Success

				Case FieldValidateEnum.USERNAME_ENDOT
					If str.IsUserName(1000, True) Then Return ValidationResult.Success

				Case FieldValidateEnum.PASSWORD
					If str.IsPassword Then Return ValidationResult.Success

				Case FieldValidateEnum.PASSWORD_NUMBERLETTER
					If str.IsPasswordNumberLetter Then Return ValidationResult.Success

				Case FieldValidateEnum.PASSWORD_COMPLEX
					If str.IsPasswordComplex Then Return ValidationResult.Success

				Case FieldValidateEnum.MD5HASH
					If str.IsMD5Hash Then Return ValidationResult.Success

				Case FieldValidateEnum.CAR
					If str.IsCar Then Return ValidationResult.Success

				Case Else
					' 数值类处理
					Dim val = str.ToNumber(False)

					Select Case Type
						Case FieldValidateEnum.UINTEGER
							If val > UInteger.MinValue AndAlso val <= UInteger.MaxValue Then Return ValidationResult.Success

						Case FieldValidateEnum.INTEGER
							If val > Integer.MinValue AndAlso val <= Integer.MaxValue Then Return ValidationResult.Success

						Case FieldValidateEnum.ULONG
							If val > ULong.MinValue AndAlso val <= ULong.MaxValue Then Return ValidationResult.Success

						Case FieldValidateEnum.LONG
							If val > Long.MinValue AndAlso val <= Long.MaxValue Then Return ValidationResult.Success

						Case FieldValidateEnum.SINGLE
							If val > Single.MinValue AndAlso val <= Single.MaxValue Then Return ValidationResult.Success

						Case FieldValidateEnum.DOUBLE
							If val > Double.MinValue AndAlso val <= Double.MaxValue Then Return ValidationResult.Success

						Case FieldValidateEnum.BYTE
							If val > Byte.MinValue AndAlso val <= Byte.MaxValue Then Return ValidationResult.Success

						Case Else   ' NUMBER
							Return ValidationResult.Success
					End Select
			End Select

			Return New ValidationResult(ErrorMessage.EmptyValue($"验证类型失败，无效类型：{Type.Description}"), {validationContext?.MemberName})
		End Function

	End Class

End Namespace