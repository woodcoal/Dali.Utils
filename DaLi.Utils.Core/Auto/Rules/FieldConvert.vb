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
' 	字段转换
'
' 	name: Auto.Rule.FieldConvert
' 	create: 2023-01-03
' 	memo: 字段转换
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>字段类型转换</summary>
	Public Class FieldConvert
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "字段类型转换"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>类型</summary>
		Public Property FieldType As FieldTypeEnum = FieldTypeEnum.TEXT

		''' <summary>参考值列表</summary>
		Public Property List As String

		''' <summary>数组分割符，为空则结果不是数组</summary>
		Public Property ArrayString As String

		''' <summary>数组项目是否可以重复</summary>
		Public Property ArrayRepeat As Boolean = False

		''' <summary>校验值的正则表达式</summary>
		Public RegularExpression As String = ""

		''' <summary>用于区域校验的最小值（数字/字符长度），为空不校验</summary>
		Public Min As Double? = Nothing

		''' <summary>用于区域校验的最大值（数字/字符长度），为空不校验</summary>
		Public Max As Double? = Nothing

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim rule As FieldConvert = Clone()

			rule.Source = AutoHelper.GetVarString(rule.Source, data)
			rule.List = AutoHelper.GetVarString(rule.List, data)

			Dim value = Nothing
			If rule.ArrayString.NotEmpty Then
				value = rule.Source.ToArrayValue(rule.FieldType, rule.ArrayString, rule.ArrayRepeat, Function(x) IsValidate(rule, x))
			Else
				value = rule.Source.ToValue(rule.FieldType, Function(x) IsValidate(rule, x), True)
			End If

			message.SetSuccess()
			Return value
		End Function

		''' <summary>校验结果是否在允许的范围内</summary>
		Private Shared Function IsValidate(rule As FieldConvert, value As Object) As Boolean
			If value Is Nothing Then Return True

			Dim R = True

			'  正则表达式校验，仅针对字符串结果有效，如：邮箱，网址，字符等
			If rule.RegularExpression.NotEmpty AndAlso rule.FieldType.IsString Then
				Try
					R = Text.RegularExpressions.Regex.IsMatch(value, rule.RegularExpression)
				Catch ex As Exception
				End Try
			End If

			' 比较最小值
			If R Then
				If rule.Min IsNot Nothing Then
					If rule.FieldType.IsString AndAlso rule.Min > 0 Then
						' 字符必须超过最小长度
						R = value.ToString.Length >= rule.Min
					ElseIf rule.Type.IsNumber Then
						R = System.Convert.ToDouble(value) >= rule.Min
					End If
				End If
			End If

			' 比较最大值
			If R Then
				If rule.Max IsNot Nothing Then
					If rule.FieldType.IsString AndAlso rule.Max > 0 Then
						' 字符必须超过最小长度
						R = value.ToString.Length <= rule.Max
					ElseIf rule.Type.IsNumber Then
						R = System.Convert.ToDouble(value) <= rule.Max
					End If
				End If
			End If

			' 是否为默认列表中的值
			If R AndAlso rule.List.NotEmpty Then
				R = rule.List.Contains(value.ToString, StringComparison.OrdinalIgnoreCase)
			End If

			Return R
		End Function

#End Region

	End Class
End Namespace

