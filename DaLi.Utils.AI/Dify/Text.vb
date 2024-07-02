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
' 	Dify 文本补全
'
' 	name: Dify.Text
' 	create: 2024-06-07
' 	memo: Dify 文本补全
'
' ------------------------------------------------------------

Imports System.Net
Imports System.Reflection
Imports DaLi.Utils.AI.Dify.Model.Events
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.Http.Model

Namespace Dify
	''' <summary>Dify 文本补全</summary>
	Public Class Text
		Inherits Base

		''' <summary>使用人</summary>
		Private _User As String

		''' <summary>最后一次返回消息的标识</summary>
		Private _MessageId As String

		''' <summary>构造</summary>
		''' <param name="url">服务器地址</param>
		''' <param name="key">ApiKey</param>
		''' <param name="user">用户标识，用比喻标记使用者</param>
		Public Sub New(Optional url As String = "", Optional key As String = "", Optional user As String = "")
			MyBase.New(url, key.EmptyValue(AISettings.DIFY_KEY_TEXT))

			_User = user.EmptyValue(Assembly.GetEntryAssembly.Title)
		End Sub

		''' <summary>开始新的消息队列</summary>
		Public Sub NewText(Optional user As String = "")
			_User = user.EmptyValue(Assembly.GetEntryAssembly.Title)
		End Sub

		''' <summary>结果转换</summary>
		Private Function ResultConver(res As Model.Chat.Response, status As (Code As HttpStatusCode, Message As String), timer As Long) As TextResult
			_MessageId = ""
			Dim ret As New TextResult With {
				.Success = False,
				.Content = status.Message.EmptyValue(status.Code.ToString),
				.Last = SYS_NOW_DATE
			}

			If res IsNot Nothing Then
				_MessageId = res.MessageId

				ret.Success = True
				ret.Content = res.Answer

				If res.Metadata IsNot Nothing AndAlso res.Metadata.Usage IsNot Nothing Then
					ret.Tokens = New TokensInfo With {
						.Input = res.Metadata.Usage.GetValue("prompt_tokens", 0),
						.Output = res.Metadata.Usage.GetValue("completion_tokens", 0),
						.Total = res.Metadata.Usage.GetValue("total_tokens", 0),
						.TimeTotal = timer
					}
				End If
			End If

			Return ret
		End Function

		''' <summary>补全</summary>
		''' <param name="prompt">生成响应的提示，替换 params 中 query 参数</param>
		''' <param name="params">聊天助手中其他附加参数</param>
		''' <param name="images">图片地址</param>
		''' <returns>同步方式，一次性返回所有结果</returns>
		Public Function Process(prompt As String, Optional params As KeyValueDictionary = Nothing, Optional images As String() = Nothing) As TextResult
			If prompt.IsEmpty Then Return Nothing

			params = If(params, New KeyValueDictionary)
			params("query") = prompt

			Dim options As New KeyValueDictionary(params) From {{"inputs", params}, {"user", _User}, {"response_mode", "blocking"}}
			UpdateOptions(options, images)

			Dim status As (Code As HttpStatusCode, Message As String) = Nothing

			Dim s As New Stopwatch
			s.Start()

			Dim res = Execute(Of Model.Chat.Response)("/v1/completion-messages", HttpMethodEnum.POST, options, status)

			s.Stop()

			Return ResultConver(res, status, s.ElapsedMilliseconds)
		End Function

		''' <summary>补全</summary>
		''' <param name="prompt">生成响应的提示，替换 params 中 query 参数</param>
		''' <param name="Images">base64 编码图像的列表（对于多模式模型，例如llava）</param>
		''' <returns>异步方式，流式返回结果</returns>
		Public Async Function ProcessAsync(prompt As String,
										   Optional params As KeyValueDictionary = Nothing,
										   Optional images As String() = Nothing,
										   Optional callback As Action(Of ProcessStatusEnum, String) = Nothing) As Task(Of TextResult)
			If prompt.IsEmpty Then Return Nothing

			params = If(params, New KeyValueDictionary)
			params("query") = prompt

			Dim options As New KeyValueDictionary(params) From {{"inputs", params}, {"user", _User}, {"response_mode", "streaming"}}
			UpdateOptions(options, images)

			Dim status As (Code As HttpStatusCode, Message As String) = Nothing

			Dim data = ""
			Dim last As New Model.Chat.Response

			' 开始处理
			callback?.Invoke(ProcessStatusEnum.BEGIN, "")

			Dim s As New Stopwatch
			s.Start()

			Dim tokens = 0

			Await ExecuteAsync("/v1/completion-messages",
							   HttpMethodEnum.POST,
							   options,
							   Sub(code, message, eventType)
								   status.Code = code
								   status.Message = message

								   CON.Echo({eventType, message})

								   ' 分析信息，暂时仅处理 message / message_end / error 三类事件，其他后续再研究
								   Select Case eventType
									   Case "message"
										   Dim evt = message.ToJsonObject(Of MessageEvent)
										   If evt IsNot Nothing Then
											   last.MessageId = evt.MessageId
											   last.ConversationId = evt.ConversationId
											   last.Created = evt.Created
											   last.Answer &= evt.Answer

											   callback?.Invoke(ProcessStatusEnum.PROCESS, evt.Answer)
										   End If

									   Case "message_end"
										   Dim evt = message.ToJsonObject(Of EndEvent)
										   If evt IsNot Nothing Then
											   last.Metadata = evt.Metadata
										   End If

									   Case "error"
										   Dim evt = message.ToJsonObject(Of ErrorEvent)
										   If evt IsNot Nothing Then
											   status.Code = evt.Status
											   status.Message = evt.Message
										   End If
								   End Select
							   End Sub)

			s.Stop()

			If status.Code <> HttpStatusCode.OK Then
				callback?.Invoke(ProcessStatusEnum.FAIL, status.Message.EmptyValue(status.Code.ToString))
			Else
				callback?.Invoke(ProcessStatusEnum.FINISH, "")
			End If

			Return ResultConver(last, status, s.ElapsedMilliseconds)
		End Function

		''' <summary>消息反馈（点赞）</summary>
		''' <param name="messageId">消息 ID</param>
		''' <param name="action">操作：true 赞，false 反对，default 取消</param>
		''' <remarks>消息终端用户反馈、点赞，方便应用开发者优化输出预期。</remarks>
		Public Function Feedback(action As TristateEnum, Optional messageId As String = "", Optional ByRef errorMessage As String = "") As Boolean
			messageId = messageId.EmptyValue(_MessageId)

			If messageId.IsEmpty Then
				errorMessage = "无效消息 ID"
				Return Nothing
			End If

			Dim path = $"/v1/messages/{messageId}/feedbacks"
			Dim method = HttpMethodEnum.POST

			Dim data As New KeyValueDictionary From {{"user", _User}}
			If action = TristateEnum.TRUE Then data("rating") = "like"
			If action = TristateEnum.FALSE Then data("rating") = "dislike"
			If action = TristateEnum.DEFAULT Then data("rating") = Nothing

			Dim status As (Code As HttpStatusCode, Message As String) = Nothing
			Dim ret = Execute(Of Model.Result)(path, method, data, status)

			If status.Code = HttpStatusCode.OK Then Return ret.Result.ToBoolean

			errorMessage = status.Message
			Return False
		End Function

		''' <summary>获取下一轮建议</summary>
		''' <param name="messageId">消息 ID，不设置则使用当前最后一次消息标识</param>
		''' <param name="errorMessage">返回的异常信息</param>
		Public Function Suggest(Optional messageId As String = "", Optional ByRef errorMessage As String = "") As IEnumerable(Of String)
			messageId = messageId.EmptyValue(_MessageId)

			If messageId.IsEmpty Then
				errorMessage = "无效消息 ID"
				Return Nothing
			End If

			Dim path = $"/v1/messages/{messageId}/suggested?user={_User}"
			Dim method = HttpMethodEnum.GET

			Dim status As (Code As HttpStatusCode, Message As String) = Nothing
			Dim ret = Execute(Of KeyValueDictionary)(path, method, Nothing, status)

			If status.Code = HttpStatusCode.OK Then Return ret.GetValue(Of IEnumerable(Of String))("data")

			errorMessage = status.Message
			Return Nothing
		End Function
	End Class
End Namespace