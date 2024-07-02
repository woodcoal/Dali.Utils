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
' 	自定义单项参数
'
' 	name: Auto.Rule.Parameter
' 	create: 2023-01-03
' 	memo: 自定义单项参数，尝试 JSON 反序列化，序列化不成功则返回原值
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>自定义单项参数</summary>
	Public Class ParameterVar
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "自定义单项参数"
			End Get
		End Property

		''' <summary>参数列表</summary>
		Public Property Vars As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "参数未设置"
			If Vars.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "原始参数无效"
			Dim ret = AutoHelper.GetVarObject(Vars, data)
			If ret Is Nothing Then Return Nothing

			message.SetSuccess()
			Return ret
		End Function

#End Region

	End Class
End Namespace