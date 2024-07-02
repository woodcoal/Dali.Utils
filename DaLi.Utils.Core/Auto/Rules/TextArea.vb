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
' 	区域文本截取
'
' 	name: Auto.Rule.TextArea
' 	create: 2023-01-01
' 	memo: 区域文本截取
' 	
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>区域文本截取</summary>
	Public Class TextArea
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "截取文本区域"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>开始区域</summary>
		Public Property AreaBegin As String

		''' <summary>结束区域</summary>
		Public Property AreaEnd As String

		''' <summary>是否包含起始区域</summary>
		Public Property IncBegin As Boolean

		''' <summary>是否包含结束区域</summary>
		Public Property IncEnd As Boolean

		''' <summary>如果需要返回多段内容，则此处为多段内容的间隔符号</summary>
		Public Property MutiLine As String

		''' <summary>清除单元格中的标签</summary>
		Public Property ClearTags As String()

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			message = "未设置有效的规则内容"
			If AreaBegin.IsEmpty AndAlso AreaEnd.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 克隆防止修改原始数据
			Dim rule As TextArea = Clone()

			rule.Source = AutoHelper.GetVarString(rule.Source, data)
			rule.AreaBegin = AutoHelper.GetVarString(rule.AreaBegin, data)
			rule.AreaEnd = AutoHelper.GetVarString(rule.AreaEnd, data)

			' -----------------
			' 区域切割
			' -----------------
			Dim Rets As New List(Of String)
			Dim isMuti = rule.MutiLine.NotEmpty
			Dim cutRet = rule.Source.Cut(rule.AreaBegin, rule.AreaEnd, isMuti, False)
			If cutRet Is Nothing Then
				message.Message = "未分析到任何有效内容"
				Return Nothing
			Else
				If isMuti Then
					Rets.AddRange(cutRet)
				ElseIf Not String.IsNullOrEmpty(cutRet) Then
					Rets.Add(cutRet)
				End If
			End If

			Dim value = Rets.
				Select(Function(x)
						   If rule.IncBegin Then x = rule.AreaBegin & x
						   If rule.IncEnd Then x &= rule.AreaEnd

						   Return x
					   End Function).
				JoinString(rule.MutiLine)

			' 清理 Html
			If rule.ClearTags.NotEmpty Then value = value.ClearHtml(rule.ClearTags)

			message.SetSuccess()
			Return value
		End Function

#End Region

	End Class
End Namespace
