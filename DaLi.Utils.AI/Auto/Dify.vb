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
' 	Dify 文本分析
'
' 	name: Auto.Rule.DifyText
' 	create: 2024-06-07
' 	memo: Dify 文本分析
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>Dify 文本分析</summary>
	Public Class Dify
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "Dify 文本分析"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>服务器地址,不设置则使用系统全局参数</summary>
		Public Property Url As String

		''' <summary>ApiKey,不设置则使用系统全局参数</summary>
		Public Property ApiKey As String

		''' <summary>对话参数</summary>
		Public Property Options As String

		''' <summary>操作方式：1.文本补全；2.对话；3.Agent；4.Workflow</summary>
		Public Property Mode As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "要分析的原始内容未设置"
			If Mode <> 4 AndAlso Source.IsEmpty Then Return False

			message = "无效操作"
			If Mode < 1 OrElse Mode > 4 Then Return False

			'message = "Dify 服务器地址未设置"
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
			Dim apiKey = AutoHelper.GetVarString(Me.ApiKey, data)
			Dim content = AutoHelper.GetVarString(Source, data)

			' Options 的值为数值，非字符
			Dim Opts As IDictionary(Of String, Object) = Nothing
			If Options.NotEmpty AndAlso Mode <> 4 Then Opts = TryCast(AutoHelper.GetVarObject(Options, data), IDictionary(Of String, Object))

			Dim succ = False
			Dim result = Nothing

			Select Case Mode
				Case 1
					' 文本补全
					Dim ai As New AI.Dify.Text(url, apiKey)
					Dim res = ai.Process(content, Opts)

					succ = res.Success
					result = res.Content

				Case 2
					' 对话
					Dim ai As New AI.Dify.Chat(url, apiKey)
					Dim res = ai.Process(content, Opts)

					succ = res.Success
					result = res.Message.Content
				Case 3
					' Agent
					Dim ai As New AI.Dify.Agent(url, apiKey)
					Dim res = ai.Process(content, Opts)

					succ = res.Success
					result = res.Message.Content

				Case 4
					' Workflow
					Dim KVs = New KeyValueDictionary(content.ToJsonDictionary)
					KVs.AddRange(Options.ToJsonDictionary)

					For Each key In kvs.Keys
						KVs(key) = AutoHelper.GetVar(KVs(key), data)
					Next

					Dim ai As New AI.Dify.Workflow(url, apiKey)
					Dim res = ai.Process(KVs)

					succ = res.Err.IsEmpty
					result = If(succ, res.Outputs, res.Err)
			End Select

			message.SetSuccess(succ, If(succ, "", result))
			Return If(succ, result, "")
		End Function

#End Region

	End Class
End Namespace
