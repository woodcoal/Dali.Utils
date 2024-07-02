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
' 	文本内容替换
'
' 	name: Auto.Rule.TextReplace
' 	create: 2023-01-01
' 	memo: 文本内容替换
' 	
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>文本内容替换</summary>
	Public Class TextReplace
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "文本内容替换"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>替换部分</summary>
		Public Property Replaces As NameValueDictionary

		''' <summary>替换的 HTML 标签</summary>
		Public Property ClearTags As String()

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			message = "未设置有效的替换规则"
			If Replaces.IsEmpty AndAlso ClearTags.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

		''' <summary>克隆</summary>
		Public Overrides Function Clone() As Object
			Dim R As TextReplace = MemberwiseClone()
			R.Replaces = Replaces.Clone
			Return R
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim content = AutoHelper.GetVarString(Source, data)
			Dim reps = Replaces?.FormatAction(Function(x) AutoHelper.GetVarString(x, data))

			'-------------------
			' 内容替换
			'-------------------
			If reps.NotEmpty Then
				reps.ForEach(Sub(key, value) content = content.ReplaceRegex(key, value))
			End If

			'-------------------
			' 过滤 HTML 标签
			'-------------------
			If ClearTags.NotEmpty Then
				content = content.ClearHtml(ClearTags)
			End If

			' 输出
			message.SetSuccess()
			Return content
		End Function

#End Region

	End Class
End Namespace
