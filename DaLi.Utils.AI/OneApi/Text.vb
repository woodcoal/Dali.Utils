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
' 	OneApi 文本补全 API
'
' 	name: OneApi.Text
' 	create: 2024-06-05
' 	memo: OneApi 文本补全 API
'
' ------------------------------------------------------------

Imports System.Net
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.AI.OneApi.Model
Imports DaLi.Utils.Http.Model

Namespace OneApi

	''' <summary>OneApi 文本补全 API</summary>
	<Obsolete("此接口存在问题，与作者设计有关")>
	Public Class Text
		Inherits Base

		''' <summary>API 地址</summary>
		Public ReadOnly Url As String

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
		Public Sub New(Optional url As String = "", Optional key As String = "", Optional model As String = "", Optional params As IDictionary(Of String, Object) = Nothing)
			MyBase.New(url, key)

			model = model.EmptyValue(AISettings.ONEAPI_MODEL)
			If model.IsEmpty Then Throw New Exception("OneApi 模型未设置")

			Me.Model = model
			Me.Params = params
		End Sub

		''' <summary>结果转换</summary>
		Private Function ResultConver(res As ChatResponse, status As (Code As HttpStatusCode, Message As String)) As TextResult
			Dim ret As New TextResult With {
				.Model = Model,
				.Success = False,
				.Content = status.Message.EmptyValue(status.Code.ToString),
				.Last = SYS_NOW_DATE
			}

			If res IsNot Nothing Then
				ret.Success = True
				ret.Content = res.Choices(0).Message.Content
				ret.Tokens = New TokensInfo With {
					.Input = res.Usage.Prompt,
					.Output = res.Usage.Completion,
					.Total = res.Usage.Total,
					.TimeLoad = res.Usage.QueueTime,
					.TimePrompt = res.Usage.PromptTime,
					.TimeEval = res.Usage.CompletionTime,
					.TimeTotal = res.Usage.TotalTime
				}
			End If

			Return ret
		End Function

		''' <summary>聊天</summary>
		''' <param name="prompt">生成响应的提示</param>
		''' <param name="Images">base64 编码图像的列表</param>
		''' <returns>同步方式，一次性返回所有结果</returns>
		Public Function Process(prompt As String, Optional images As String() = Nothing) As TextResult
			If Model.IsEmpty OrElse (prompt.IsEmpty AndAlso images.IsEmpty) Then Return Nothing

			Dim options As New KeyValueDictionary(Params) From {{"prompt", prompt}, {"model", Model}, {"stream", False}}
			Dim status As (Code As HttpStatusCode, Message As String) = Nothing

			Dim s As New Stopwatch
			s.Start()

			Dim res = Execute(Of ChatResponse)("/v1/completions", HttpMethodEnum.POST, options, status)

			s.Stop()

			If res IsNot Nothing AndAlso res.Usage IsNot Nothing AndAlso res.Usage.TotalTime < 1 Then res.Usage.TotalTime = s.ElapsedMilliseconds

			Return ResultConver(res, status)
		End Function

		''' <summary>聊天</summary>
		''' <param name="prompt">生成响应的提示</param>
		''' <param name="Images">base64 编码图像的列表（对于多模式模型，例如llava）</param>
		''' <returns>异步方式，流式返回结果</returns>
		Public Async Function ProcessAsync(prompt As String, Optional images As String() = Nothing, Optional callback As Action(Of ProcessStatusEnum, String) = Nothing) As Task(Of TextResult)
			If Model.IsEmpty OrElse (prompt.IsEmpty AndAlso images.IsEmpty) Then Return Nothing

			Dim options As New KeyValueDictionary(Params) From {{"prompt", prompt}, {"model", Model}, {"stream", True}}
			Dim status As (Code As HttpStatusCode, Message As String) = Nothing

			Dim data = ""
			Dim last As ChatResponse = Nothing

			' 开始处理
			callback?.Invoke(ProcessStatusEnum.BEGIN, "")

			Dim s As New Stopwatch
			s.Start()

			Dim tokens = 0

			Await ExecuteAsync(Of ChatResponse)("/v1/completions",
													 HttpMethodEnum.POST,
													 options,
													 Sub(code, message, res)
														 status.Code = code
														 status.Message = message

														 last = res

														 If res Is Nothing OrElse res.Choices.IsEmpty Then Return

														 callback?.Invoke(ProcessStatusEnum.PROCESS, res.Choices(0).Delta.Content)
														 data &= res.Choices(0).Delta.Content

														 tokens += 1
													 End Sub)

			s.Stop()

			' 最后将所有文本结果合并到输出中
			If last IsNot Nothing AndAlso last.Choices.NotEmpty Then last.Choices(0).Message = New ChatMessage With {.Role = "assistant", .Content = data}

			If status.Code <> HttpStatusCode.OK Then
				callback?.Invoke(ProcessStatusEnum.FAIL, status.Message.EmptyValue(status.Code.ToString))
			Else
				callback?.Invoke(ProcessStatusEnum.FINISH, "")
			End If

			' 由于使用 One-APi 接口，所以数据不一定准确，需要自己计算
			If last IsNot Nothing Then
				If last.Usage Is Nothing Then last.Usage = New ChatResponse.TokenInfo
				If last.Usage.TotalTime < 1 Then last.Usage.TotalTime = s.ElapsedMilliseconds
				If last.Usage.Prompt < 1 Then last.Usage.Prompt = prompt.Length
				If last.Usage.Completion < 1 Then last.Usage.Completion = tokens
				If last.Usage.Total < 1 Then last.Usage.Total = last.Usage.Completion + last.Usage.Prompt
			End If

			Return ResultConver(last, status)
		End Function

	End Class
End Namespace