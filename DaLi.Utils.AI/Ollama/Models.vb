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
' 	Ollama 模型
'
' 	name: Ollama
' 	create: 2024-05-18
' 	memo: Ollama 模型
' 	
' ------------------------------------------------------------

Imports System.Text.Json.Serialization

Namespace Ollama.Model

	''' <summary>反馈结果基类</summary>
	Public Class ResponseBase
		''' <summary>返回状态，空则表示成功，存在文本表示错误信息</summary>
		Public Property Status As String

		''' <summary>型号名称</summary>
		Public Property Model As String

		''' <summary>创建时间</summary>
		<JsonPropertyName("created_at")>
		Public Property Created As Date

		''' <summary>是否已经完成</summary>
		Public Property Done As Boolean

		''' <summary>完成状态</summary>
		<JsonPropertyName("done_reason")>
		Public Property DoneReason As String

		''' <summary>生成响应所花费的时间，单位：纳秒</summary>
		<JsonPropertyName("total_duration")>
		Public Property Total As Long

		''' <summary>加载模型所花费的时间，单位：纳秒</summary>
		<JsonPropertyName("load_duration")>
		Public Property Load As Long

		''' <summary>提示中的标记数量</summary>
		<JsonPropertyName("prompt_eval_count")>
		Public Property PromptEvalCount As Integer

		''' <summary>评估提示所花费的时间，单位：纳秒</summary>
		<JsonPropertyName("prompt_eval_duration")>
		Public Property PromptEvalDuration As Long

		''' <summary>响应中的令牌数量</summary>
		<JsonPropertyName("eval_count")>
		Public Property EvalCount As Integer

		''' <summary>生成响应所花费的时间，单位：纳秒</summary>
		<JsonPropertyName("eval_duration")>
		Public Property EvalDuration As Long
	End Class

	''' <summary>Ollama 模型</summary>
	Public Class Model
		''' <summary>模型名称</summary>
		Public Property Name As String

		''' <summary>更新时间</summary>
		<JsonPropertyName("modified_at")>
		Public Property Modified As Date

		''' <summary>到期时间</summary>
		<JsonPropertyName("expires_at")>
		Public Property Expires As Date

		''' <summary>模型大小</summary>
		Public Property Size As Long

		''' <summary>指纹</summary>
		Public Property Digest As String

		''' <summary>详细信息</summary>
		Public Property Details As KeyValueDictionary

		''' <summary>模型文件</summary>
		Public Property Modelfile As String

		''' <summary>参数</summary>
		Public Property Parameters As String

		''' <summary>模板</summary>
		Public Property Template As String

		''' <summary>版权</summary>
		Public Property License As String
	End Class

	''' <summary>型号列表</summary>
	Public Class Models
		Public Property Models As Model()
	End Class

	''' <summary>文本补全反馈结果</summary>
	Public Class GenerateResponse
		Inherits ResponseBase

		''' <summary>生成内容</summary>
		Public Property Response As String

		''' <summary>此响应中使用的对话编码，可以在下一个请求中发送以保留对话记忆</summary>
		Public Property Context As Integer()

	End Class

	''' <summary>聊天反馈结果</summary>
	Public Class ChatResponse
		Inherits ResponseBase

		''' <summary>生成内容</summary>
		Public Property Message As AI.Model.ChatMessage

	End Class

End Namespace