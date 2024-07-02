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
' 	判断操作
'
' 	name: Auto.Rule.IfValidate
' 	create: 2023-01-04
' 	memo: 判断操作
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>判断操作</summary>
	Public Class IfValidate
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "判断操作"
			End Get
		End Property

		''' <summary>原始数据变量名</summary>
		Public Property Var As String

		''' <summary>判断（True：为真执行，False：为否执行）</summary>
		Public Property Value As Boolean

		''' <summary>判断后执行的规则</summary>
		Public Property Rules As String

		''' <summary>为匹配成功时输出的内容</summary>
		Public Property SuccValue As String

		''' <summary>为匹配失败时输出的内容</summary>
		Public Property FailValue As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始数据变量名未设置"
			If Var.IsEmpty Then Return False

			message = "判断后执行的规则未设置"
			If Rules.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 执行操作
			Output = Nothing

			message.Message = "变量数据不存在"
			If data.IsEmpty Then Return Nothing

			' 检查规则是否有效
			message.Message = "判断执行规则未设置或者异常"
			Dim ruleList = AutoHelper.RuleList(Rules, True)
			If ruleList.IsEmpty Then Return Nothing

			message.Message = "无此判断变量值，变量值必须为：真假、字符串或者数字"
			Dim value = AutoHelper.GetVarString(Var, data)
			If value.IsEmpty Then Return Nothing

			' 验证结果, 未匹配退出
			message.SetSuccess()
			message.Message = "结果不匹配"
			If value.ToBoolean <> Me.Value Then Return If(AutoHelper.GetVar(FailValue, data), False)

			data = AutoHelper.FlowExecute(ruleList, False, False, data, message)
			Return If(SuccValue.NotEmpty, AutoHelper.GetVarObject(SuccValue, data), data)
		End Function

#End Region

	End Class
End Namespace