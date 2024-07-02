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
' 	序列化数据为 JSON
'
' 	name: Auto.Rule.JsonString
' 	create: 2023-01-03
' 	memo: 序列化数据为 JSON
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>序列化数据为 JSON</summary>
	Public Class JsonString
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "序列化数据为 JSON"
			End Get
		End Property

		''' <summary>原始数据变量名</summary>
		Public Property Source As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始数据变量名未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "变量数据不存在"
			If data.IsEmpty Then Return Nothing

			message.Message = "无效变量值，变量值必须存在"
			Dim value = AutoHelper.GetVar(Source, data)
			If value Is Nothing Then Return Nothing

			message.SetSuccess()
			Return JsonExtension.ToJson(value)
		End Function

#End Region

	End Class
End Namespace