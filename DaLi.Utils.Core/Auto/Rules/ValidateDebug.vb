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
' 	验证当前是否调试环境
'
' 	name: Auto.Rule.ValidateDebug
' 	create: 2024-06-20
' 	memo: 验证当前是否调试环境
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>验证当前是否调试环境</summary>
	Public Class ValidateDebug
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "验证当前是否调试环境"
			End Get
		End Property

		''' <summary>为匹配成功时输出的内容</summary>
		Public Property SuccValue As String

		''' <summary>为匹配失败时输出的内容</summary>
		Public Property FailValue As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.SetSuccess()
			Dim Ret = AutoHelper.IsDebug(data)

			If Ret Then
				Return If(SuccValue.NotEmpty, AutoHelper.GetVar(SuccValue, data), True)
			Else
				Return If(FailValue.NotEmpty, AutoHelper.GetVar(FailValue, data), False)
			End If
		End Function

#End Region

	End Class
End Namespace