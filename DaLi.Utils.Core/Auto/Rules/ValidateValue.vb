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
' 	验证值是否存在内容
'
' 	name: Auto.Rule.ValidateValue
' 	create: 2023-01-12
' 	memo: 验证值是否存在内容，字典、列表、文本长度大于 1，时间大于 2000年，数字大于 0，布尔为 True
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>验证值是否存在内容</summary>
	Public Class ValidateValue
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "验证值是否存在内容"
			End Get
		End Property

		''' <summary>原始日期数据，不设置为当前时间</summary>
		Public Property Source As String

		''' <summary>触发异常。True：匹配成功异常； False：匹配失败异常； Default：忽略，成功或失败都不触发</summary>
		Public Property ThrowValue As TristateEnum

		''' <summary>为匹配成功时输出的内容</summary>
		Public Property SuccValue As String

		''' <summary>为匹配失败时输出的内容</summary>
		Public Property FailValue As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "验证的值未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Function IsValidate(data As IDictionary(Of String, Object), message As AutoMessage) As Boolean
			Dim Ret = False

			message.SetSuccess()
			Dim value = AutoHelper.GetVar(Source, data)
			If value IsNot Nothing Then
				Dim type = value.GetType

				If type.IsList(Of Object) Then
					Ret = TryCast(value, IEnumerable(Of Object))?.Count > 0

				ElseIf type.IsDictionary(Of String, Object) Then
					Ret = TryCast(value, IDictionary(Of String, Object))?.Count > 0

				ElseIf type.IsString Then
					Ret = TryCast(value, String)?.Length > 0

				ElseIf type.IsDate Then
					Ret = value.ToString.ToDateTime.IsValidate

				ElseIf type.IsBoolean Then
					Ret = value.ToString.ToBoolean

				ElseIf type.IsNumber Then
					Ret = value.ToString.ToNumber > 0

				Else
					Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, "无效数据格式")
				End If
			End If

			Select Case ThrowValue
				Case TristateEnum.TRUE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("值不能存在内容").FormatTemplate(data, True)
					If Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

				Case TristateEnum.FALSE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("值必须存在内容").FormatTemplate(data, True)
					If Not Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)
			End Select

			Return Ret
		End Function

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim Ret = IsValidate(data, message)

			If Ret Then
				Return If(SuccValue.NotEmpty, AutoHelper.GetVar(SuccValue, data), True)
			Else
				Return If(FailValue.NotEmpty, AutoHelper.GetVar(FailValue, data), False)
			End If
		End Function

#End Region

	End Class
End Namespace