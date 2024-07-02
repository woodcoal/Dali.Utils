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
' 	字段值转换
'
' 	name: Attribute.FieldChangeAttribute
' 	create: 2023-02-14
' 	memo: 字段值转换，将字段内容转换成对应格式，无法转换时产生错误
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Attribute

	''' <summary>字段值转换</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
	Public NotInheritable Class FieldChangeAttribute
		Inherits ValidationAttribute

		''' <summary>检查模式</summary>
		Public ReadOnly Property Type As FieldTypeEnum

		Public Sub New(Optional type As FieldTypeEnum = FieldTypeEnum.TEXT)
			Me.Type = type
		End Sub

		''' <summary>校验方式</summary>
		Protected Overrides Function IsValid(value As Object, validationContext As ValidationContext) As ValidationResult
			If value Is Nothing Then Return New ValidationResult(ErrorMessage.EmptyValue("字段值不能为空"), {validationContext?.MemberName})

			' 替换
			Dim val = value.ToString.ToValue(Type)
			If val Is Nothing Then Return New ValidationResult(ErrorMessage.EmptyValue($"转换类型失败，无法转换成类型：{Type.Description}"), {validationContext?.MemberName})

			If val <> value Then
				' 发生了替换，回写进去
				Dim name = validationContext.MemberName
				Dim field = validationContext.ObjectType.GetSingleProperty(name)
				If field IsNot Nothing Then field.SetValue(validationContext.ObjectInstance, val)
			End If

			Return ValidationResult.Success
		End Function

	End Class

End Namespace