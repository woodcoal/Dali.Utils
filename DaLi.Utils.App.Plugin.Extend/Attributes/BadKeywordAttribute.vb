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
' 	非法关键词属性
'
' 	name: Attribute.BadKeywordAttribute
' 	create: 2023-02-27
' 	memo: 非法关键词属性
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Attribute

	''' <summary>非法关键词属性</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
	Public NotInheritable Class BadKeywordAttribute
		Inherits ValidationAttribute

		''' <summary>检查模式</summary>
		Public ReadOnly Property Mode As KeywordCheckEnum

		Public Sub New(Optional mode As KeywordCheckEnum = KeywordCheckEnum.REPLACE)
			Me.Mode = mode
		End Sub

		''' <summary>校验方式</summary>
		Protected Overrides Function IsValid(value As Object, validationContext As ValidationContext) As ValidationResult
			If value IsNot Nothing AndAlso value.GetType.IsString Then
				Dim Pro = SYS.GetService(Of IBadKeywordProvider)
				If Pro IsNot Nothing Then
					Dim source As String = value
					Dim message = ErrorMessage.EmptyValue("Error.BadKeyword")

					Select Case Mode
						Case KeywordCheckEnum.USER
							' 用户名
							If Pro.BadUser(source) Then
								Return New ValidationResult(message, {validationContext.MemberName})
							End If

						Case KeywordCheckEnum.CHECK
							' 查找
							If Pro.Contains(source) Then
								Return New ValidationResult(message, {validationContext.MemberName})
							End If

						Case KeywordCheckEnum.REPLACE
							' 替换
							source = Pro.Replace(source)
							If source <> value Then
								' 发生了替换，回写进去
								Dim name = validationContext.MemberName
								Dim field = validationContext.ObjectType.GetSingleProperty(name)
								If field IsNot Nothing Then field.SetValue(validationContext.ObjectInstance, source)
							End If

					End Select
				End If
			End If

			Return ValidationResult.Success
		End Function

	End Class

End Namespace