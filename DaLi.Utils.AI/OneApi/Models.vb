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
' 	OneApi 模型
'
' 	name: OneApi
' 	create: 2024-05-18
' 	memo: OneApi 模型
' 	
' ------------------------------------------------------------

Imports System.Text.Json.Serialization
Imports DaLi.Utils.AI.Model

Namespace OneApi.Model

	''' <summary>反馈结果基类</summary>
	Public Class ResponseBase
		''' <summary>数据类型</summary>
		<JsonPropertyName("object")>
		Public Property Type As String

	End Class

	''' <summary>OneApi 模型</summary>
	Public Class Model
		Inherits KeyValueDictionary

		''' <summary>模型名称</summary>
		Public Property Name As String
			Get
				Return Item("id")
			End Get
			Set(value As String)
				Item("id") = value
			End Set
		End Property

		''' <summary>创建时间</summary>
		Public Property Created As Date
			Get
				Return GetValue("created", 0).ToDate(True)
			End Get
			Set(value As Date)
				Item("created") = value.UnixTicks
			End Set
		End Property

		''' <summary>组织</summary>
		Public Property Owned As String
			Get
				Return Item("owned_by")
			End Get
			Set(value As String)
				Item("owned_by") = value
			End Set
		End Property
	End Class

	''' <summary>型号列表</summary>
	Public Class Models
		Inherits ResponseBase

		''' <summary>模型列表</summary>
		Public Property Data As Model()
	End Class

	''' <summary>聊天反馈结果</summary>
	Public Class ChatResponse
		Inherits ResponseBase

		''' <summary>模型为输入提示生成的完成选项</summary>
		Public Class Choice
			''' <summary>完成原因。模型停止生成标记的原因。这可能是stop因为模型到达了自然停止点或提供的停止序列， length或者达到了请求中指定的最大标记数，或者content_filter由于内容过滤器中的标志而省略了内容。</summary>
			<JsonPropertyName("finish_reason")>
			Public Property Reason As String

			''' <summary>index</summary>
			Public Property Index As Integer

			''' <summary>对数概率</summary>
			Public Property Logprobs As Object

			''' <summary>文本内容</summary>
			Public Property Text As String

			''' <summary>消息内容</summary>
			Public Property Message As ChatMessage

			''' <summary>块消息内容</summary>
			Public Property Delta As ChatMessage

		End Class

		''' <summary>完成请求的使用情况统计。</summary>
		Public Class TokenInfo
			''' <summary>生成的完成中的令牌数。</summary>
			<JsonPropertyName("completion_tokens")>
			Public Property Completion As Integer

			''' <summary>生成的完成中耗时：秒</summary>
			<JsonPropertyName("completion_time")>
			Public Property CompletionTime As Single

			''' <summary>提示的令牌数。</summary>
			<JsonPropertyName("prompt_tokens")>
			Public Property Prompt As Integer

			''' <summary>提示的耗时：秒</summary>
			<JsonPropertyName("prompt_time")>
			Public Property PromptTime As Single

			''' <summary>总令牌数。</summary>
			<JsonPropertyName("total_tokens")>
			Public Property Total As Integer

			''' <summary>总耗时：秒</summary>
			<JsonPropertyName("total_time")>
			Public Property TotalTime As Single

			''' <summary>队列时长：秒</summary>
			<JsonPropertyName("queue_time")>
			Public Property QueueTime As Single
		End Class

		''' <summary>完成的唯一标识符。</summary>
		Public Property ID As String

		''' <summary>用于完成的模型。</summary>
		Public Property Model As String

		''' <summary>完成创建时的 Unix 时间戳（以秒为单位）</summary>
		Public Property Created As Integer

		''' <summary>系统指纹。该指纹代表模型运行的后端配置。可以与请求参数结合使用，seed以了解何时进行了可能影响确定性的后端更改。</summary>
		<JsonPropertyName("system_fingerprint")>
		Public Property Fingerprint As String

		''' <summary>输出结果。</summary>
		Public Property Choices As Choice()

		''' <summary>完成请求的使用情况统计。</summary>
		<JsonPropertyName("usage")>
		Public Property Usage As TokenInfo

	End Class

End Namespace