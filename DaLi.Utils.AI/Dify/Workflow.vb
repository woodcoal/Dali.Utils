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
' 	Dify 流程
'
' 	name: Dify.Workflow
' 	create: 2024-06-07
' 	memo: Dify 流程
'
' ------------------------------------------------------------

Imports System.Net
Imports System.Reflection
Imports DaLi.Utils.AI.Dify.Model.Events
Imports DaLi.Utils.AI.Dify.Model.Workflow
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.Http.Model

Namespace Dify
	''' <summary>Dify 流程</summary>
	Public Class Workflow
		Inherits Base

		''' <summary>使用人</summary>
		Private _User As String

		''' <summary>构造</summary>
		''' <param name="url">服务器地址</param>
		''' <param name="key">ApiKey</param>
		''' <param name="user">用户标识，用比喻标记使用者</param>
		Public Sub New(Optional url As String = "", Optional key As String = "", Optional user As String = "")
			MyBase.New(url, key.EmptyValue(AISettings.DIFY_KEY_WORKFLOW))

			_User = user.EmptyValue(Assembly.GetEntryAssembly.Title)
		End Sub

		''' <summary>开始新的消息队列</summary>
		Public Sub NewWorkflow(Optional user As String = "")
			_User = user.EmptyValue(Assembly.GetEntryAssembly.Title)
		End Sub

		''' <summary>补全</summary>
		''' <param name="params">聊天助手中其他附加参数</param>
		''' <param name="images">图片地址</param>
		''' <returns>同步方式，一次性返回所有结果</returns>
		Public Function Process(Optional params As KeyValueDictionary = Nothing, Optional images As String() = Nothing) As WorkflowItem
			Dim options As New KeyValueDictionary From {{"inputs", If(params, New KeyValueDictionary)}, {"user", _User}, {"response_mode", "blocking"}}
			UpdateOptions(options, images)

			Dim status As (Code As HttpStatusCode, Message As String) = Nothing
			Dim res = Execute(Of Response)("/v1/workflows/run", HttpMethodEnum.POST, options, status)

			If status.Code = HttpStatusCode.OK Then
				Return res.Data
			Else
				Return New WorkflowItem With {.Status = "error", .Err = status.Message, .Outputs = New KeyValueDictionary From {{"code", status.Code}, {"message", status.Message}}}
			End If
		End Function

		''' <summary>补全</summary>
		''' <param name="Images">base64 编码图像的列表（对于多模式模型，例如llava）</param>
		''' <returns>异步方式，流式返回结果</returns>
		Public Async Function ProcessAsync(Optional params As KeyValueDictionary = Nothing,
										   Optional images As String() = Nothing,
										   Optional callback As Action(Of ProcessStatusEnum, String, EventBase) = Nothing) As Task(Of WorkflowItem)
			Dim options As New KeyValueDictionary From {{"inputs", If(params, New KeyValueDictionary)}, {"user", _User}, {"response_mode", "streaming"}}
			UpdateOptions(options, images)

			Dim status As (Code As HttpStatusCode, Message As String) = Nothing
			Dim ret As WorkflowItem = Nothing

			' 开始处理
			callback?.Invoke(ProcessStatusEnum.BEGIN, Nothing, Nothing)

			Await ExecuteAsync("/v1/workflows/run",
							   HttpMethodEnum.POST,
							   options,
							   Sub(code, message, eventType)
								   status.Code = code
								   status.Message = message

								   ' 分析信息，暂时仅处理 workflow_started / node_started / node_finished / workflow_finished / error 五类事件，其他后续再研究
								   Select Case eventType
									   Case "workflow_started"
										   Dim evt = message.ToJsonObject(Of WorkflowEvent(Of WorkflowStart))
										   If evt IsNot Nothing Then callback?.Invoke(ProcessStatusEnum.PROCESS, eventType, evt.Data)

									   Case "node_started"
										   Dim evt = message.ToJsonObject(Of WorkflowEvent(Of NodeStart))
										   If evt IsNot Nothing Then callback?.Invoke(ProcessStatusEnum.PROCESS, eventType, evt.Data)

									   Case "node_finished"
										   Dim evt = message.ToJsonObject(Of WorkflowEvent(Of NodeFinish))
										   If evt IsNot Nothing Then callback?.Invoke(ProcessStatusEnum.PROCESS, eventType, evt.Data)

									   Case "workflow_finished"
										   Dim evt = message.ToJsonObject(Of WorkflowEvent(Of WorkflowItem))
										   If evt IsNot Nothing Then
											   callback?.Invoke(ProcessStatusEnum.PROCESS, eventType, evt.Data)
											   ret = evt.Data
										   End If

									   Case "error"
										   Dim evt = message.ToJsonObject(Of ErrorEvent)
										   If evt IsNot Nothing Then
											   status.Code = evt.Status
											   status.Message = evt.Message
										   End If
								   End Select
							   End Sub)

			If status.Code = HttpStatusCode.OK Then
				callback?.Invoke(ProcessStatusEnum.FINISH, Nothing, ret)
			Else
				ret = New WorkflowItem With {.Status = "error", .Err = status.Message, .Outputs = New KeyValueDictionary From {{"code", status.Code}, {"message", status.Message}}}
				callback?.Invoke(ProcessStatusEnum.FAIL, Nothing, ret)
			End If

			Return ret
		End Function

	End Class
End Namespace