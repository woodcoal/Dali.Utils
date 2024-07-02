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
' 	Ollama 文本补全 API
'
' 	name: Ollama.Text
' 	create: 2024-06-05
' 	memo: Ollama 文本补全 API
'
' ------------------------------------------------------------

Imports System.Net
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.AI.Ollama.Model
Imports DaLi.Utils.Http.Model

Namespace Ollama

	''' <summary>Ollama 文本补全 API</summary>
	Public Class Text
		Inherits Base

		''' <summary>当前模型</summary>
		Public ReadOnly Model As String

		''' <summary>模型参数</summary>
		Public ReadOnly Params As IDictionary(Of String, Object)

		''' <summary>系统角色，具体的提示内容</summary>
		Public Property System As String

		''' <summary>构造</summary>
		''' <param name="url">Api 地址</param>
		''' <param name="model">模型</param>
		''' <param name="params">模型参数</param>
		Public Sub New(Optional url As String = "", Optional model As String = "", Optional params As IDictionary(Of String, Object) = Nothing)
			MyBase.New(url)

			model = model.EmptyValue(AISettings.OLLAMA_MODEL)
			If model.IsEmpty Then Throw New Exception("Ollama 模型未设置")

			Me.Model = model
			Me.Params = params
		End Sub

		''' <summary>结果转换</summary>
		Private Function ResultConver(res As GenerateResponse, status As (Code As HttpStatusCode, Message As String)) As TextResult
			Dim ret As New TextResult With {
				.Model = Model,
				.Success = False,
				.Content = status.Message.EmptyValue(status.Code.ToString),
				.Last = SYS_NOW_DATE
			}

			If res IsNot Nothing Then
				ret.Success = True
				ret.Content = res.Response
				ret.Tokens = New TokensInfo With {
					.Input = res.PromptEvalCount,
					.Output = res.EvalCount,
					.Total = res.PromptEvalCount + res.EvalCount,
					.TimeLoad = res.Load / 1000000,
					.TimePrompt = res.PromptEvalDuration / 1000000,
					.TimeEval = res.EvalDuration / 1000000,
					.TimeTotal = res.Total / 1000000
				}
			End If

			Return ret
		End Function

		''' <summary>生成补全</summary>
		''' <param name="prompt">生成响应的提示</param>
		''' <param name="Images">base64 编码图像的列表（对于多模式模型，例如llava）</param>
		''' <returns>同步方式，一次性返回所有结果</returns>
		Public Function Process(prompt As String, Optional images As String() = Nothing) As TextResult
			If Model.IsEmpty OrElse (prompt.IsEmpty AndAlso images.IsEmpty) Then Return Nothing

			Dim options As New KeyValueDictionary From {{"prompt", prompt}, {"images", images}, {"model", Model}, {"options", Params}, {"System", System}, {"stream", False}}
			Dim status As (Code As HttpStatusCode, Message As String) = Nothing
			Dim res = Execute(Of GenerateResponse)("/api/generate", HttpMethodEnum.POST, options, status)

			Return ResultConver(res, status)
		End Function

		''' <summary>生成补全</summary>
		''' <param name="prompt">生成响应的提示</param>
		''' <param name="Images">base64 编码图像的列表（对于多模式模型，例如llava）</param>
		''' <returns>异步方式，流式返回结果</returns>
		Public Async Function ProcessAsync(prompt As String, Optional images As String() = Nothing, Optional callback As Action(Of ProcessStatusEnum, String) = Nothing) As Task(Of TextResult)
			If Model.IsEmpty OrElse (prompt.IsEmpty AndAlso images.IsEmpty) Then Return Nothing

			Dim options As New KeyValueDictionary From {{"prompt", prompt}, {"images", images}, {"model", Model}, {"options", Params}, {"System", System}, {"stream", True}}
			Dim status As (Code As HttpStatusCode, Message As String) = Nothing

			Dim data = ""
			Dim last As GenerateResponse = Nothing

			' 开始处理
			callback?.Invoke(ProcessStatusEnum.BEGIN, "")

			Await ExecuteAsync(Of GenerateResponse)("/api/generate",
													 HttpMethodEnum.POST,
													 options,
													 Sub(code, message, res)
														 status.Code = code
														 status.Message = message

														 If res Is Nothing Then Return

														 callback?.Invoke(ProcessStatusEnum.PROCESS, res.Response)
														 data &= res.Response
														 If res.Done Then last = res
													 End Sub)
			' 最后将所有文本结果合并到输出中
			If last IsNot Nothing Then last.Response = data

			If status.Code <> HttpStatusCode.OK Then
				callback?.Invoke(ProcessStatusEnum.FAIL, status.Message.EmptyValue(status.Code.ToString))
			Else
				callback?.Invoke(ProcessStatusEnum.FINISH, "")
			End If

			Return ResultConver(last, status)
		End Function

	End Class
End Namespace
