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
' 	Dify Agent 信息
'
' 	name: Dify.Agent
' 	create: 2024-06-07
' 	memo: Dify Agent 信息
'
' ------------------------------------------------------------

Imports System.Net
Imports System.Reflection
Imports DaLi.Utils.AI.Dify.Model.Events
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.Http.Model

Namespace Dify
	''' <summary>Dify Agent 信息</summary>
	Public Class Agent
		Inherits Base

		''' <summary>使用人</summary>
		Private _User As String

		''' <summary>消息队列号，用于保持对话进程</summary>
		Private _TaskId As String

		''' <summary>最后一次返回消息的标识</summary>
		Private _MessageId As String

		''' <summary>构造</summary>
		''' <param name="url">服务器地址</param>
		''' <param name="key">ApiKey</param>
		''' <param name="user">用户标识，用比喻标记使用者</param>
		Public Sub New(Optional url As String = "", Optional key As String = "", Optional user As String = "")
			MyBase.New(url, key)

			_User = user.EmptyValue(Assembly.GetEntryAssembly.Title)
		End Sub

		''' <summary>开始新的消息队列</summary>
		Public Sub NewAgent(Optional user As String = "")
			_TaskId = ""
			_User = user.EmptyValue(Assembly.GetEntryAssembly.Title)
		End Sub

		''' <summary>结果转换</summary>
		Private Function ResultConver(res As Model.Chat.Response, status As (Code As HttpStatusCode, Message As String), timer As Long) As ChatResult
			_MessageId = ""
			Dim ret As New ChatResult With {
				.Success = False,
				.Message = New ChatMessage With {.Content = status.Message.EmptyValue(status.Code.ToString), .Role = "error"},
				.Last = SYS_NOW_DATE
			}

			If res IsNot Nothing Then
				_TaskId = res.ConversationId
				_MessageId = res.MessageId

				ret.Success = True
				ret.Message = New ChatMessage With {.Role = "assistant", .Content = res.Answer}

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

		''' <summary>对话</summary>
		''' <param name="prompt">生成响应的提示</param>
		''' <param name="params">聊天助手中其他附加参数</param>
		''' <returns>同步方式，一次性返回所有结果, Dify Agent 不支持阻塞模式，本操作使用异步任务模拟完成</returns>
		Public Function Process(prompt As String, Optional params As KeyValueDictionary = Nothing) As ChatResult
			If prompt.IsEmpty Then Return Nothing

			Dim ret As ChatResult = Nothing

			Task.Run(Async Function()
						 ret = Await ProcessAsync(prompt, params)
					 End Function).Wait()

			Return ret
		End Function

		''' <summary>对话</summary>
		''' <param name="prompt">生成响应的提示</param>
		''' <returns>异步方式，流式返回结果</returns>
		Public Async Function ProcessAsync(prompt As String,
										   Optional params As KeyValueDictionary = Nothing,
										   Optional callback As Action(Of ProcessStatusEnum, String) = Nothing) As Task(Of ChatResult)
			If prompt.IsEmpty Then Return Nothing

			Dim options As New KeyValueDictionary From {{"query", prompt}, {"inputs", params}, {"user", _User}, {"conversation_id", _TaskId}, {"response_mode", "streaming"}}
			Dim status As (Code As HttpStatusCode, Message As String) = Nothing

			Dim data = ""
			Dim last As New Model.Chat.Response

			' 开始处理
			callback?.Invoke(ProcessStatusEnum.BEGIN, "")

			Dim s As New Stopwatch
			s.Start()

			Dim tokens = 0

			Await ExecuteAsync("/v1/chat-messages",
							   HttpMethodEnum.POST,
							   options,
							   Sub(code, message, eventType)
								   status.Code = code
								   status.Message = message

								   ' 分析信息，暂时仅处理 agent_message / agent_thought / message_end / error 三类事件，其他后续再研究
								   Select Case eventType
									   Case "agent_message"
										   Dim evt = message.ToJsonObject(Of AgentMessageEvent)
										   If evt IsNot Nothing Then
											   last.MessageId = evt.MessageId
											   last.ConversationId = evt.ConversationId
											   last.Created = evt.Created
											   last.Answer &= evt.Answer

											   callback?.Invoke(ProcessStatusEnum.PROCESS, evt.Answer)
										   End If

									   Case "agent_thought"
										   ' 工具调用信息，暂不做处理
										   Dim evt = message.ToJsonObject(Of AgentThoughtEvent)
										   If evt IsNot Nothing Then

											   '	''' <summary>agent_thought ID，每一轮 Agent 迭代都会有一个唯一的id</summary>
											   'Public Property ID As String

											   '''' <summary>agent_thought 在消息中的位置，如第一轮迭代 position 为 1</summary>
											   'Public Property Position As Integer

											   '''' <summary>agent 的思考内容</summary>
											   'Public Property Thought As String

											   '''' <summary>工具调用的返回结果</summary>
											   'Public Property Observation As String

											   '''' <summary>使用的工具列表，以 ; 分割多个工具</summary>
											   'Public Property Tools As String

											   '''' <summary>工具的输入，JSON格式的字符串(object)。如：{"dalle3": {"prompt": "a cute cat"}}</summary>
											   '<JsonPropertyName("tool_input")>
											   'Public Property ToolInput As String

											   '''' <summary>创建时间</summary>
											   '<JsonPropertyName("created_at")>
											   'Public Property Created As Long

											   '''' <summary>使用的工具列表，以 ; 分割多个工具</summary>
											   '<JsonPropertyName("message_files")>
											   'Public Property Files As String()



											   'last.MessageId = evt.MessageId
											   'last.ConversationId = evt.ConversationId
											   'last.Created = evt.Created
											   'last.Answer &= evt.Answer

											   'callback?.Invoke(ProcessStatusEnum.PROCESS, evt.Answer)
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