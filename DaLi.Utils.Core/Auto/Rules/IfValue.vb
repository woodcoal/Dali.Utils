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
' 	判断是否存在值
'
' 	name: Auto.Rule.IfValue
' 	create: 2023-01-12
' 	memo: 判断是否存在值
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>判断是否存在值</summary>
	Public Class IfValue
		Inherits ValidateValue

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "判断是否存在值"
			End Get
		End Property

		''' <summary>存在生效还是不存在生效（True 不存在值是执行子规则，False 存在值时执行子规则）</summary>
		Public Property No As Boolean

		''' <summary>判断后执行的规则</summary>
		Public Property Rules As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "判断后执行的规则未设置"
			If Rules.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 检查规则是否有效
			message.Message = "判断执行规则未设置或者异常"
			Dim ruleList = AutoHelper.RuleList(Rules, True)
			If ruleList.IsEmpty Then Return Nothing

			message.SetSuccess()
			message.Message = If(No, "指定值不存在", "不能存在指定值")

			Dim ret = IsValidate(data, message)
			If ret = No Then Return If(FailValue.NotEmpty, AutoHelper.GetVar(FailValue, data), ret)

			data = AutoHelper.FlowExecute(ruleList, False, False, data, message)
			Return If(SuccValue.NotEmpty, AutoHelper.GetVarObject(SuccValue, data), data)
		End Function

#End Region

	End Class
End Namespace