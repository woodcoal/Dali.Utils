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
' 	验证字段处理
'
' 	name: Helper.ValidationAttributeHelper
' 	create: 2023-02-18
' 	memo: 验证字段处理
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports System.Reflection

Namespace Helper
	''' <summary>验证字段处理</summary>
	Public NotInheritable Class ValidationAttributeHelper

		''' <summary>获取所有验证字段，并将值转成字典数据</summary>
		Public Shared Function AttributeValues(pro As PropertyInfo) As NameValueDictionary
			If pro Is Nothing Then Return Nothing

			' 分析属性的基础类型
			Dim type = pro.PropertyType
			If type.IsEnum Then type = GetType(System.Enum)

			Dim data As New NameValueDictionary From {
				{"Type", type.FullName}
			}

			' StringLengthAttribute
			Dim SL = pro.GetCustomAttribute(Of StringLengthAttribute)
			If SL IsNot Nothing Then
				data.Add("StringLength", "")
				data.Add("StringLength.ErrorMessage", SL.ErrorMessage)
				data.Add("StringLength.MinimumLength", SL.MinimumLength)
				data.Add("StringLength.MaximumLength", SL.MaximumLength)
			End If

			' RequiredAttribute
			Dim Required = pro.GetCustomAttribute(Of RequiredAttribute)
			If Required IsNot Nothing Then
				data.Add("Required", "")
				data.Add("Required.ErrorMessage", Required.ErrorMessage)
				data.Add("Required.AllowEmptyStrings", Required.AllowEmptyStrings)
			End If

			' RegularExpressionAttribute
			Dim RE = pro.GetCustomAttribute(Of RegularExpressionAttribute)
			If RE IsNot Nothing Then
				data.Add("RegularExpression", "")
				data.Add("RegularExpression.ErrorMessage", RE.ErrorMessage)
				data.Add("RegularExpression.Pattern", RE.Pattern)
			End If

			' RangeAttribute
			Dim Range = pro.GetCustomAttribute(Of RangeAttribute)
			If Range IsNot Nothing Then
				data.Add("Range", "")
				data.Add("Range.ErrorMessage", Range.ErrorMessage)
				data.Add("Range.Minimum", Range.Minimum)
				data.Add("Range.Maximum", Range.Maximum)
				data.Add("Range.OperandType", Range.OperandType.FullName)
			End If

			' MinLengthAttribute
			Dim MinLen = pro.GetCustomAttribute(Of MinLengthAttribute)
			If MinLen IsNot Nothing Then
				data.Add("MinLength", "")
				data.Add("MinLength.ErrorMessage", MinLen.ErrorMessage)
				data.Add("MinLength.Length", MinLen.Length)
			End If

			' MaxLengthAttribute
			Dim MaxLen = pro.GetCustomAttribute(Of MaxLengthAttribute)
			If MaxLen IsNot Nothing Then
				data.Add("MaxLength", "")
				data.Add("MaxLength.ErrorMessage", MaxLen.ErrorMessage)
				data.Add("MaxLength.Length", MaxLen.Length)
			End If

			' FieldTypeAttribute
			Dim FT = pro.GetCustomAttribute(Of FieldTypeAttribute)
			If FT IsNot Nothing Then
				data.Add("FieldType", "")
				data.Add("FieldType.ErrorMessage", FT.ErrorMessage)
				data.Add("FieldType.Type", FT.Type)
			End If

			' FieldChangeAttribute
			Dim FC = pro.GetCustomAttribute(Of FieldChangeAttribute)
			If FC IsNot Nothing Then
				data.Add("FieldChange", "")
				data.Add("FieldChange.ErrorMessage", FC.ErrorMessage)
				data.Add("FieldChange.Type", FC.Type)
			End If

			' BadKeywordAttribute
			Dim BK = pro.GetCustomAttribute(Of BadKeywordAttribute)
			If BK IsNot Nothing Then
				data.Add("BadKeyword", "")
				data.Add("BadKeyword.ErrorMessage", BK.ErrorMessage)
				data.Add("BadKeyword.Mode", BK.Mode)
			End If

			' FieldEncodeAttribute
			Dim FEC = pro.GetCustomAttribute(Of FieldEncodeAttribute)
			If FEC IsNot Nothing Then
				data.Add("FieldEncode", "")
				data.Add("FieldEncode.Key", FEC.Key)
			End If

			Return data
		End Function

		''' <summary>获取所有验证字段，并将值转成字典数据</summary>
		Public Shared Function AttributeList(data As NameValueDictionary) As List(Of System.Attribute)
			If data.IsEmpty Then Return Nothing

			Dim attrs As New List(Of System.Attribute)

			' StringLengthAttribute
			If data.ContainsKey("StringLength") Then
				attrs.Add(New StringLengthAttribute(data("StringLength.MaximumLength").ToInteger) With {
					.MinimumLength = data("StringLength.MinimumLength").ToInteger,
					.ErrorMessage = data("StringLength.ErrorMessage")
				})
			End If

			' RequiredAttribute
			If data.ContainsKey("Required") Then
				attrs.Add(New RequiredAttribute With {
					.AllowEmptyStrings = data("Required.AllowEmptyStrings").ToBoolean,
					.ErrorMessage = data("Required.ErrorMessage")
				})
			End If

			' RegularExpressionAttribute
			If data.ContainsKey("RegularExpression") Then
				attrs.Add(New RegularExpressionAttribute(data("RegularExpression.Pattern")) With {
					.ErrorMessage = data("RegularExpression.ErrorMessage")
				})
			End If

			' RangeAttribute
			If data.ContainsKey("Range") Then
				' 区间检测，检测值需要先转换成区间内置类型值才能验证
				Dim attrType = Type.GetType(data("Range.OperandType"), False, True)
				Dim min = data("Range.Minimum")
				Dim max = data("Range.Maximum")

				attrs.Add(New RangeAttribute(attrType, min, max) With {
					.ErrorMessage = data("Range.ErrorMessage")
				})
			End If

			' MinLengthAttribute
			If data.ContainsKey("MinLength") Then
				attrs.Add(New MinLengthAttribute(data("MinLength.Length").ToInteger) With {
					.ErrorMessage = data("MinLength.ErrorMessage")
				})
			End If

			' MaxLengthAttribute
			If data.ContainsKey("MaxLength") Then
				attrs.Add(New MaxLengthAttribute(data("MaxLength.Length").ToInteger) With {
					.ErrorMessage = data("MaxLength.ErrorMessage")
				})
			End If

			' FieldTypeAttribute
			If data.ContainsKey("FieldType") Then
				attrs.Add(New FieldTypeAttribute(data("FieldType.Type").ToInteger) With {
					.ErrorMessage = data("FieldType.ErrorMessage")
				})
			End If

			' FieldChangeAttribute
			If data.ContainsKey("FieldChange") Then
				attrs.Add(New FieldTypeAttribute(data("FieldChange.Type").ToInteger) With {
					.ErrorMessage = data("FieldChange.ErrorMessage")
				})
			End If

			' BadKeywordAttribute
			If data.ContainsKey("BadKeyword") Then
				attrs.Add(New FieldTypeAttribute(data("BadKeyword.Mode").ToInteger) With {
					.ErrorMessage = data("BadKeyword.ErrorMessage")
				})
			End If

			' FieldEncodedAttribute
			If data.ContainsKey("FieldEncode") Then
				attrs.Add(New FieldEncodeAttribute(data("FieldEncode.Key")))
			End If

			Return attrs
		End Function

		''' <summary>获取所有验证字段，并将值转成字典数据，验证失败返回错误提示</summary>
		Public Shared Function AttributeValidate(data As NameValueDictionary, value As Object) As String
			If data.IsEmpty Then Return ""

			' StringLengthAttribute
			If data.ContainsKey("StringLength") Then
				Try
					Dim attr As New StringLengthAttribute(data("StringLength.MaximumLength").ToInteger) With {
						.MinimumLength = data("StringLength.MinimumLength").ToInteger
					}

					If Not attr.IsValid(value) Then Return data("StringLength.ErrorMessage").EmptyValue("长度不符合条件")
				Catch ex As Exception
				End Try
			End If

			' RequiredAttribute
			If data.ContainsKey("Required") Then
				Try
					Dim attr As New RequiredAttribute() With {
						.AllowEmptyStrings = data("StringLength.AllowEmptyStrings").ToBoolean
					}

					If Not attr.IsValid(value) Then Return data("Required.ErrorMessage").EmptyValue("不能为空")
				Catch ex As Exception
				End Try
			End If

			' RegularExpressionAttribute
			If data.ContainsKey("RegularExpression") Then
				Try
					Dim attr As New RegularExpressionAttribute(data("RegularExpression.Pattern"))
					If Not attr.IsValid(value) Then Return data("RegularExpression.ErrorMessage").EmptyValue("格式错误")
				Catch ex As Exception
				End Try
			End If

			' RangeAttribute
			If data.ContainsKey("Range") Then
				Try
					' 区间检测，检测值需要先转换成区间内置类型值才能验证
					Dim attrType = Type.GetType(data("Range.OperandType"))
					Dim attrValue = ChangeType(value, attrType)
					If attrValue Is Nothing Then Return "无效格式"

					Dim attr As New RangeAttribute(attrType, data("Range.Minimum"), data("Range.Maximum"))
					If Not attr.IsValid(attrValue) Then Return data("Range.ErrorMessage").EmptyValue("不在指定范围内")
				Catch ex As Exception
				End Try
			End If


			' MinLengthAttribute
			If data.ContainsKey("MinLength") Then
				Try
					Dim attr As New MinLengthAttribute(data("MinLength.Length").ToInteger)
					If Not attr.IsValid(value) Then Return data("MinLength.ErrorMessage").EmptyValue("少于指定范围")
				Catch ex As Exception
				End Try
			End If

			' MaxLengthAttribute
			If data.ContainsKey("MaxLength") Then
				Try
					Dim attr As New MaxLengthAttribute(data("MaxLength.Length").ToInteger)
					If Not attr.IsValid(value) Then Return data("MaxLength.ErrorMessage").EmptyValue("大于指定范围")
				Catch ex As Exception
				End Try
			End If

			' FieldTypeAttribute
			If data.ContainsKey("FieldType") Then
				Try
					Dim attr As New FieldTypeAttribute(data("FieldType.Type").ToInteger)
					If Not attr.IsValid(value) Then Return data("FieldType.ErrorMessage").EmptyValue("类型错误")
				Catch ex As Exception
				End Try
			End If

			' FieldChangeAttribute
			If data.ContainsKey("FieldChange") Then
				Try
					Dim attr As New FieldChangeAttribute(data("FieldChange.Type").ToInteger)
					If Not attr.IsValid(value) Then Return data("FieldChange.ErrorMessage").EmptyValue("类型转换失败")
				Catch ex As Exception
				End Try
			End If

			' BadKeywordAttribute
			If data.ContainsKey("BadKeyword") Then
				Try
					Dim attr As New BadKeywordAttribute(data("BadKeyword.Mode").ToInteger)
					If Not attr.IsValid(value) Then Return data("BadKeyword.ErrorMessage").EmptyValue("包含禁用关键词")
				Catch ex As Exception
				End Try
			End If

			Return ""
		End Function

		''' <summary>获取所有验证描述</summary>
		Public Shared Function AttributeDescription(pro As PropertyInfo) As String
			If pro Is Nothing Then Return ""

			' 默认描述
			Dim Desc = pro.GetCustomAttribute(Of DescriptionAttribute)?.Description.ShortShow(300)

			' 属性描述
			Dim Ret As New List(Of String) From {
				$"数据类型：{pro.PropertyType.FullName}"
			}

			' StringLengthAttribute
			Dim SL = pro.GetCustomAttribute(Of StringLengthAttribute)
			If SL IsNot Nothing Then Ret.Add($"字符长度：{SL.MinimumLength} ~ {SL.MaximumLength}")

			' RequiredAttribute
			Dim Required = pro.GetCustomAttribute(Of RequiredAttribute)
			If Required IsNot Nothing Then
				Ret.Add("必填")
				If Required.AllowEmptyStrings Then Ret.Add("允许空字符串")
			End If

			' RegularExpressionAttribute
			Dim RE = pro.GetCustomAttribute(Of RegularExpressionAttribute)
			If RE IsNot Nothing Then Ret.Add($"正则表达式：{RE.Pattern}")

			' RangeAttribute
			Dim Range = pro.GetCustomAttribute(Of RangeAttribute)
			If Range IsNot Nothing Then
				Ret.Add($"区间类型：{Range.OperandType.FullName}")
				Ret.Add($"区间范围：{Range.Minimum} ~ {Range.Maximum}")
			End If

			' MinLengthAttribute
			Dim MinLen = pro.GetCustomAttribute(Of MinLengthAttribute)
			If MinLen IsNot Nothing Then Ret.Add($"最小长度：{MinLen.Length}")

			' MaxLengthAttribute
			Dim MaxLen = pro.GetCustomAttribute(Of MaxLengthAttribute)
			If MaxLen IsNot Nothing Then Ret.Add($"最大长度：{MaxLen.Length}")

			' FieldTypeAttribute
			Dim FT = pro.GetCustomAttribute(Of FieldTypeAttribute)
			If FT IsNot Nothing Then Ret.Add($"值格式：{FT.Type.Description}")

			' FieldChangeAttribute
			Dim FC = pro.GetCustomAttribute(Of FieldChangeAttribute)
			If FC IsNot Nothing Then Ret.Add($"值转换类型：{FC.Type.Description}")

			' BadKeywordAttribute
			Dim BK = pro.GetCustomAttribute(Of BadKeywordAttribute)
			If BK IsNot Nothing Then Ret.Add($"禁用关键词：{BK.Mode.Description}")

			' FieldEncodeAttribute
			Dim FEC = pro.GetCustomAttribute(Of FieldEncodeAttribute)
			If FEC IsNot Nothing Then Ret.Add($"字段加密密钥：{FEC.Key.Mask }")

			Dim attrs = Ret.JoinString("；")
			If Desc.NotEmpty AndAlso attrs.NotEmpty Then
				Return $"{Desc}（{attrs}）"
			Else
				Return Desc & attrs
			End If
		End Function


	End Class
End Namespace