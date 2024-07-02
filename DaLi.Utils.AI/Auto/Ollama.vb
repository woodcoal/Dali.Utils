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
' 	Ollama 文本分析
'
' 	name: Auto.Rule.OllamaText
' 	create: 2024-05-19
' 	memo: Ollama 文本分析
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>Ollama 文本分析</summary>
	Public Class Ollama
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "Ollama 文本分析"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>服务器地址,不设置则使用系统全局参数</summary>
		Public Property Url As String

		''' <summary>模型</summary>
		Public Property Model As String

		''' <summary>系统提示词</summary>
		Public Property System As String

		''' <summary>模型参数</summary>
		Public Property Options As KeyValueDictionary

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "要分析的原始内容未设置"
			If Source.IsEmpty Then Return False

			'message = "Ollama 服务器地址未设置"
			'If Url.NotEmpty AndAlso Not Url.IsUrl Then Return False

			'message = "大语言模型为设置"
			'If Model.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim url = AutoHelper.GetVarString(Me.Url, data)
			Dim model = AutoHelper.GetVarString(Me.Model, data)
			Dim content = AutoHelper.GetVarString(Source, data)
			Dim system = AutoHelper.GetVarString(Me.System, data)

			' Options 的值为数值，非字符
			Dim Opts = Options?.
					Select(Function(x)
							   Dim value = AutoHelper.GetVar(x.Value, data)
							   If value IsNot Nothing Then
								   Dim type = value.GetType
								   If type.IsString Then
									   value = value.ToString.ToNumber
								   ElseIf Not type.IsNumber Then
									   value = Nothing
								   End If
							   End If

							   Return (x.Key, value)
						   End Function).
						   Where(Function(x) x.value IsNot Nothing AndAlso x.value >= 0).
					ToDictionary(Function(x) x.Key, Function(x) x.value)

			Dim ai As New AI.Ollama.Chat(url, model, Opts) With {.System = system}
			Dim res = ai.Process(content)
			message.SetSuccess(res.Success, res.Message.Content)
			Return If(res.Success, res.Message.Content, "")
		End Function

#End Region

	End Class
End Namespace
