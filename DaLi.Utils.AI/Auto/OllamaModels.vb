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
' 	Ollama 模型列表
'
' 	name: Auto.Rule.OllamaModels
' 	create: 2024-05-19
' 	memo: Ollama 模型列表
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>Ollama 模型列表</summary>
	Public Class OllamaModels
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "Ollama 模型列表"
			End Get
		End Property

		''' <summary>服务器地址,不设置则使用系统全局参数</summary>
		Public Property Url As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			'message = "Ollama 服务器地址未设置"
			'If Url.NotEmpty AndAlso Not Url.IsUrl Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim url = AutoHelper.GetVarString(Me.Url, data)

			Dim ai As New AI.Ollama.Info(url)
			Dim res = ai.Models

			If res.NotEmpty Then
				message.SetSuccess()
				Return res.Select(Function(x) x.Name).ToList
			Else
				message.SetSuccess(False, "无任何大语言模型数据")
				Return Nothing
			End If
		End Function

#End Region

	End Class
End Namespace
