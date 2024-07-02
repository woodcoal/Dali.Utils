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
' 	文本关键词分析
'
' 	name: Auto.Rule.TextKeywords
' 	create: 2023-01-05
' 	memo: 文本关键词分析
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>文本关键词分析</summary>
	Public Class TextKeywords
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "文本关键词分析"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>关键词数量（1-25）</summary>
		Public Property Count As Integer

		''' <summary>词性，多个用逗号间隔</summary>
		Public Property POS As String()

		''' <summary>关联长度，大于 0：TextRank Span 长度；等于 0：TF-IDF 算法；小于 0：综合两者算法，绝对值为 Span 长度</summary>
		Public Property Span As Integer

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
			Dim num = Count.Range(1, 25)

			' POS
			Dim pos = Me.POS
			If pos.IsEmpty Then pos = Nothing

			' Span
			Dim span = Me.Span.Range(-100, 100)

			' 输出
			message.SetSuccess()
			Return Segment.Default.Keywords(content, num, pos, Span)
		End Function

#End Region

	End Class
End Namespace
