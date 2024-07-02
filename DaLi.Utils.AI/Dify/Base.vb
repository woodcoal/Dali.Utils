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
' 	Dify 基础操作
'
' 	name: Dify.Base
' 	create: 2024-06-06
' 	memo: Dify 基础操作
'
' ------------------------------------------------------------

Imports System.Net
Imports DaLi.Utils.Http
Imports DaLi.Utils.Http.Model

Namespace Dify
	''' <summary>Dify 基础操作</summary>
	Public MustInherit Class Base

		''' <summary>API 客户端</summary>
		Private ReadOnly _Api As ApiClient

		''' <summary>构造</summary>
		''' <param name="url">知识库服务器地址</param>
		''' <param name="key">知识库 ApiKey</param>
		Public Sub New(Optional url As String = "", Optional key As String = "")
			url = url.EmptyValue(AISettings.DIFY_URL)
			key = key.EmptyValue(AISettings.DIFY_KEY_AGENT)

			If Not url.IsUrl Then Throw New Exception("Dify 服务器地址错误")
			If key.IsEmpty OrElse key.Length < 10 Then Throw New Exception("Dify Api-Key 无效")

			_Api = New ApiClient With {
				.BaseURL = url,
				.Token = key,
				.Timeout = 600000
			}
		End Sub

		''' <summary>执行 API 操作</summary>
		Protected Function Execute(Of T)(path As String, method As HttpMethodEnum, Optional params As IDictionary(Of String, Object) = Nothing, Optional ByRef status As (Code As HttpStatusCode, Message As String) = Nothing) As T
			Dim data = _Api.Execute(method, path, params, True)
			status = (_Api.StatusCode, data)

			If _Api.StatusCode = Net.HttpStatusCode.OK Then Return data.ToJsonObject(Of T)
		End Function

		''' <summary>流式执行 API 操作</summary>
		''' <param name="callback">参数1：http 状态码；参数2：http 文本结果；参数3：事件类型</param>
		Protected Async Function ExecuteAsync(path As String, method As HttpMethodEnum, Optional params As IDictionary(Of String, Object) = Nothing, Optional callback As Action(Of HttpStatusCode, String, String) = Nothing) As Task
			Await _Api.ExecuteAsync(method,
								path,
								params,
								True,
								Sub(str)
									' data: {"event": "message", "message_id": "5ad4cb98-e6290", "conversation_id": "45701982-555f2", "answer": " I", "created_at": 1679586595}

									' 处理返回值 data: {...}
									If str.StartsWith("data:") Then str = str.Substring(5).Trim

									Dim eventType = str.ToJsonObject(Of Model.Events.Base)?.EventType
									callback?.Invoke(_Api.StatusCode, str, eventType)
								End Sub)
		End Function

		''' <summary>更新参数中的文件数据</summary>
		Protected Shared Sub UpdateOptions(Options As KeyValueDictionary, Optional images As String() = Nothing)
			If Options.IsEmpty Then Return

			If images.NotEmpty Then
				Dim files = images.
					Where(Function(x) x.IsUrl).
					Select(Function(x) New KeyValueDictionary From {{"type", "image"}, {"transfer_method", "remote_url"}, {"url", x}}).
					ToList
				Options.Add("files", files)
			End If
		End Sub
	End Class
End Namespace