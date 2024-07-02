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
' 	文本摘要分析
'
' 	name: Auto.Rule.TextSummary
' 	create: 2023-01-05
' 	memo: 文本摘要分析
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>文本摘要分析</summary>
	Public Class TextSummary
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "文本摘要分析"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>摘要长度（25-1000）</summary>
		Public Property Length As Integer

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
			Dim content = AutoHelper.GetVarString(Source, data)
			Dim num = Length.Range(25, 1000)

			' 输出
			message.SetSuccess()
			Return Segment.Default.Summary(content, num)
		End Function

#End Region

	End Class
End Namespace
