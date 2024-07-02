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
' 	表达式计算
'
' 	name: Auto.Rule.Math
' 	create: 2023-01-03
' 	memo: 表达式计算
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>表达式计算</summary>
	Public Class Math
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "表达式计算"
			End Get
		End Property

		''' <summary>表达式</summary>
		Public Property Expression As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "表达式未设置"
			If Expression.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim exp = AutoHelper.GetVarString(Expression, data)

			Try
				Dim ret = New Data.DataTable().Compute(exp, Nothing)

				message.SetSuccess()
				Return ret
			Catch ex As Exception
				message.Message = ex.Message
				Throw
			End Try
		End Function

#End Region

	End Class
End Namespace