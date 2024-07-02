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
' 	列表转字典数据
'
' 	name: Auto.Rule.List2Dictionary
' 	create: 2023-01-05
' 	memo: 列表转字典数据
' 	
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>列表转字典数据</summary>
	Public Class List2Dictionary
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "列表转字典数据"
			End Get
		End Property

		''' <summary>原始内容,如果存在对应字段则使用直接使用变量值，否则尝试反序列为列表</summary>
		Public Property Source As String

		''' <summary>转换失败是否返回空值</summary>
		Public Property ReturnNothing As Boolean

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容不能为空"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "原始内容不能为空"
			Dim value = AutoHelper.GetVar(Source, data)
			If value Is Nothing Then Return Nothing

			message.Message = "转换后数据无效，无法转换"
			Dim ret = AutoHelper.List2Dictionary(value, ReturnNothing)
			If ret Is Nothing Then Return Nothing

			message.SetSuccess()
			Return ret
		End Function

#End Region

	End Class
End Namespace
