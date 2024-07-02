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
' 	内容验证
'
' 	name: Auto.Rule.ValidateInclude
' 	create: 2023-01-03
' 	memo: 内容验证
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>内容验证</summary>
	Public Class ValidateInclude
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "内容验证"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>操作，默认包含(等于、不等于、包含、不包含、大于、小于、大于等于、小于等于)</summary>
		Public Property Action As String

		''' <summary>验证时，包含内容</summary>
		Public Property Values As String()

		''' <summary>全部匹配</summary>
		Public Property ValidateAll As Boolean

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
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			message = "验证值必须设置"
			If Values.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim Ret = IsValidate(data, message)

			If Ret Then
				Return If(SuccValue.NotEmpty, AutoHelper.GetVar(SuccValue, data), True)
			Else
				Return If(FailValue.NotEmpty, AutoHelper.GetVar(FailValue, data), False)
			End If
		End Function

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Function IsValidate(data As IDictionary(Of String, Object), message As AutoMessage) As Boolean
			Dim Ret = IsValidate(Source, Action, Values, ValidateAll, data)

			Select Case ThrowValue
				Case TristateEnum.TRUE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("禁止包含指定内容").FormatTemplate(data, True)
					If Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

				Case TristateEnum.FALSE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("未包含指定内容").FormatTemplate(data, True)
					If Not Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

			End Select

			message.SetSuccess()
			Return Ret
		End Function

		''' <summary>是否包含</summary>
		''' <param name="content">要检测的字符串</param>
		''' <param name="data">需要替换的标签数据</param>
		Public Shared Function IsValidate(content As String, action As String, values As String(), Optional incALL As Boolean = False, Optional data As IDictionary(Of String, Object) = Nothing) As Boolean
			' 无内容返回有效
			If values.IsEmpty Then Return True

			' 操作
			action = action.EmptyValue("include").ToLower

			' 空内容无法校验，返回无效
			content = AutoHelper.GetVarString(content, data)
			If content.IsEmpty Then Return False

			' 比较的值
			If data IsNot Nothing Then
				values = values.Distinct.Select(Function(x) AutoHelper.GetVarString(x, data)).Where(Function(x) x.NotEmpty).ToArray

				' 去重后无内容返回有效
				If values.IsEmpty Then Return True
			End If

			Dim Ret = False

			' 开始进行校验
			If incALL Then
				'-------------------
				' 每条记录都需要满足
				'-------------------
				For Each s As String In values
					Ret = IsValidate(content, action, s)
					If Not Ret Then Exit For
				Next
			Else
				'-------------------
				' 满足其中一条记录即可
				'-------------------
				For Each s As String In values
					Ret = IsValidate(content, action, s)
					If Ret Then Exit For
				Next
			End If

			Return Ret
		End Function

		''' <summary>比较</summary>
		''' <param name="content">要检测的字符串</param>
		''' <param name="action">操作：等于、不等于、包含、不包含、大于、小于、大于等于、小于等于</param>
		''' <param name="value">用于比较的值</param>
		Private Shared Function IsValidate(content As String, action As String, value As String) As Boolean
			Select Case action
				Case "=", "＝", "等于", "equal"
					Return content = value

				Case "!", "<>", "!=", "≠", "不等于", "notequal"
					Return content <> value

				Case "包含", "in", "inc", "include"
					Return content.Like(value)

				Case "不包含", "not", "notin", "ex", "exclude"
					Return Not content.Like(value)

				Case ">", "＞", "大于", "more", "large"
					Return content.ToNumber > value.ToNumber

				Case ">=", "≥", "大于等于", "morethan", "largethan"
					Return content.ToNumber >= value.ToNumber

				Case "<", "＜", "小于", "less"
					Return content.ToNumber < value.ToNumber

				Case "<=", "≤", "小于等于", "lessthan"
					Return content.ToNumber <= value.ToNumber
			End Select

			Return False
		End Function

#End Region

	End Class
End Namespace