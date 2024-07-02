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
' 	代理规则执行
'
' 	name: Auto.RuleProxy
' 	create: 2023-01-06
' 	memo: 代理规则执行，本机无法执行时，通过服务器代理执行规则
'
' ------------------------------------------------------------

Imports System.Text.Json.Serialization
Imports DaLi.Utils.Http

Namespace Auto
	''' <summary>代理规则执行</summary>
	Public Class RuleProxy
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "代理规则执行"
			End Get
		End Property

		''' <summary>原始规则</summary>
		Public Property Rule As String

		''' <summary>获取 API 客户端的方法</summary>
		<JsonIgnore>
		Public Shared GetApiClient As Func(Of ApiClient)

		Public Sub New()
			Enabled = True
		End Sub

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "未设置有效的设置 API 客户端的函数"
			If GetApiClient Is Nothing Then Return False

			message = "原始规则无效"
			If Rule.IsEmpty Then Return False

			Dim dic = Rule.ToJsonDictionary
			If dic.IsEmpty OrElse Not dic.ContainsKey("type") Then Return Nothing

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "无效 API 客户端"
			Dim client = GetApiClient()
			If client Is Nothing Then Return Nothing

			' {"success":true,"data":{"success":true,"time":"2023-01-16T11:49:03.2730039","rule":{"type":"TableData","name":"\u8868\u683C\u6570\u636E\u5F55\u5165","output":"db","errorIgnore":false,"enabled":true},"result":false,"children":[]},"traceId":"0HMNNEHM7OB94:00000002","host":"localhost:10000"}
			Dim ret = client.ExecuteApi(Of AutoMessage)(Http.Model.HttpMethodEnum.POST, "auto/rule", New With {Rule, data}.ToJson(False, False, False))
			If ret IsNot Nothing Then
				'{"success":true,"time":"2023-01-16T11:49:03.2730039","rule":{"type":"TableData","name":"\u8868\u683C\u6570\u636E\u5F55\u5165","output":"db","errorIgnore":false,"enabled":true},"result":false,"children":[]}

				message.Copy(ret.Data)

				'Dim ruleResult = JsonExtension.ToJson(ret.Data("result")).ToJsonDictionary
				'If ruleResult.IsEmpty Then Throw New AutoException(ExceptionEnum.NO_RESULT)

				'' 原始规则数据
				'Dim ruleContent = Rule.ToJsonNameValues

				'' 替换消息
				'Dim ruleMessage = JsonExtension.ToJson(ret.Data("message")).ToJsonObject(Of AutoMessage)
				'If ruleMessage Is Nothing Then
				'	message.Copy(ruleMessage)
				'Else
				'	'message.RuleType = ruleContent("type")
				'End If

				' 分析输出参数
				' Output = ruleContent("output").EmptyValue(ruleContent("type"))
				Output = message.Output

				' 返回首条结果
				Return JsonExtension.ToJson(ret.Data.Result).ToJsonCollection.Value
			Else
				message.Message = $"代理规则执行异常 {ret?.ErrorMessage}"
				Return Nothing
			End If
		End Function

#End Region
	End Class
End Namespace