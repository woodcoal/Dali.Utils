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
' 	Dify 模型
'
' 	name: Dify.Model
' 	create: 2024-05-20
' 	memo: Dify 模型
'
' ------------------------------------------------------------

Imports System.Text.Json.Serialization
Imports DaLi.Utils.AI.Dify.Model.Chat

Namespace Dify.Model

	''' <summary>返回的结果</summary>
	Public Class Result
		''' <summary>结果，成功返回 success</summary>
		Public Property Result As String
	End Class

	Namespace Datasets

		''' <summary>返回的结果</summary>
		Public Class Result(Of T)
			Public Sub New()
			End Sub

			Public Sub New(data As T)
				Me.Data = data
			End Sub

			Public Sub New(data As String)
				Me.Data = data.ToJsonObject(Of T)
			End Sub

			Public Sub New(code As String, status As Integer, message As String)
				Me.Code = code
				Me.Status = status
				Me.Message = message
			End Sub

			''' <summary>返回的错误代码</summary>
			Public Property Code As String

			''' <summary>返回的错误状态</summary>
			Public Property Status As Integer

			''' <summary>返回的错误信息</summary>
			Public Property Message As String

			''' <summary>正确是返回的对象</summary>
			Public Property Data As T
		End Class

		''' <summary>知识库</summary>
		Public Class KB

			''' <summary>知识库标识</summary>
			Public Property Id As String

			''' <summary>知识库名称</summary>
			Public Property Name As String

			''' <summary>知识库描述</summary>
			Public Property Description As String

			''' <summary>provider:vendor</summary>
			Public Property Provider As String

			''' <summary>可见权限：only_me</summary>
			Public Property Permission As String

			''' <summary>数据源类型</summary>
			<JsonPropertyName("data_source_type")>
			Public Property Type As String

			''' <summary>索引方式</summary>
			<JsonPropertyName("indexing_technique")>
			Public Property Indexing As String

			''' <summary>应用数量</summary>
			<JsonPropertyName("app_count")>
			Public Property Apps As Integer

			''' <summary>文档数量</summary>
			<JsonPropertyName("document_count")>
			Public Property Documents As Integer

			''' <summary>字数</summary>
			<JsonPropertyName("word_count")>
			Public Property Words As Integer

			''' <summary>创建人</summary>
			<JsonPropertyName("created_by")>
			Public Property CreatedBy As String

			''' <summary>创建时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

			''' <summary>更新人</summary>
			<JsonPropertyName("updated_by")>
			Public Property UpdatedBy As String

			''' <summary>更新时间</summary>
			<JsonPropertyName("updated_at")>
			Public Property Updated As Long

			''' <summary>Embedding 模型</summary>
			<JsonPropertyName("embedding_model")>
			Public Property EmbeddingModel As Object

			''' <summary>Embedding 模型供应商</summary>
			<JsonPropertyName("embedding_model_provider")>
			Public Property EmbeddingModelProvider As Object

			''' <summary>Embedding 模型状态</summary>
			<JsonPropertyName("embedding_available")>
			Public Property EmbeddingAvailable As Object

			''' <summary>retrieval_model_dict</summary>
			<JsonPropertyName("retrieval_model_dict")>
			Public Property RetrievalModel As KeyValueDictionary

			''' <summary>tags</summary>
			Public Property Tags As Integer()
		End Class

		''' <summary>知识库列表</summary>
		Public Class KBs

			''' <summary>当前页码</summary>
			Public Property Page As Integer

			''' <summary>总记录数</summary>
			Public Property Total As Integer

			''' <summary>每页记录数</summary>
			Public Property Limit As Integer

			''' <summary>还有下一页</summary>
			<JsonPropertyName("has_more")>
			Public Property More As Boolean

			''' <summary>记录</summary>
			Public Property Data As KB()
		End Class

		''' <summary>文档</summary>
		Public Class Document

			''' <summary>数据源信息</summary>
			Public Class Info_
				''' <summary>上传文件标识</summary>
				<JsonPropertyName("upload_file_id")>
				Public Property FieldID As String
			End Class

			''' <summary>文档</summary>
			Public Property ID As String

			''' <summary>序号</summary>
			Public Property Position As Integer

			''' <summary>数据源类型</summary>
			<JsonPropertyName("data_source_type")>
			Public Property Type As String

			''' <summary>数据源信息</summary>
			<JsonPropertyName("data_source_info")>
			Public Property Info As Info_

			''' <summary>知识库处理规则标识</summary>
			<JsonPropertyName("dataset_process_rule_id")>
			Public Property RuleId As String

			''' <summary>名称</summary>
			Public Property Name As String

			''' <summary>来源</summary>
			<JsonPropertyName("created_from")>
			Public Property CreatedFrom As String

			''' <summary>创建人</summary>
			<JsonPropertyName("created_by")>
			Public Property CreatedBy As String

			''' <summary>创建时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

			''' <summary>Tokens</summary>
			Public Property Tokens As Integer

			''' <summary>索引状态</summary>
			<JsonPropertyName("indexing_status")>
			Public Property IndexingStatus As String

			''' <summary>错误信息</summary>
			<JsonPropertyName("error")>
			Public Property Err As String

			''' <summary>启用</summary>
			Public Property Enabled As Boolean

			''' <summary>禁用人</summary>
			<JsonPropertyName("disabled_by")>
			Public Property DisabledBy As String

			''' <summary>禁用时间</summary>
			<JsonPropertyName("disabled_at")>
			Public Property Disabled As Long?

			''' <summary>是否存档</summary>
			Public Property Archived As Boolean

			''' <summary>显示状态</summary>
			<JsonPropertyName("display_status")>
			Public Property DisplayStatus As String

			''' <summary>字数</summary>
			<JsonPropertyName("word_count")>
			Public Property Words As Integer

			''' <summary>命中数</summary>
			<JsonPropertyName("hit_count")>
			Public Property Hits As Integer

			''' <summary>文档模型：qa_model/text_model 问答模型</summary>
			<JsonPropertyName("doc_form")>
			Public Property Form As String

		End Class

		''' <summary>文档列表</summary>
		Public Class Documents

			''' <summary>当前页码</summary>
			Public Property Page As Integer

			''' <summary>总记录数</summary>
			Public Property Total As Integer

			''' <summary>每页记录数</summary>
			Public Property Limit As Integer

			''' <summary>还有下一页</summary>
			<JsonPropertyName("has_more")>
			Public Property More As Boolean

			''' <summary>记录</summary>
			Public Property Data As Document()
		End Class

		''' <summary>文档上传公用属性</summary>
		Public Class DocumentOption
			''' <summary>索引方式，取值：high_quality 高质量 / economy 经济</summary>
			<JsonPropertyName("indexing_technique")>
			Public Property Indexing As String = "high_quality"

			''' <summary>处理规则</summary>
			<JsonPropertyName("process_rule")>
			Public Property ProcessRule As ProcessRule

			''' <summary>文档模型：qa_model/text_model 问答模型</summary>
			<JsonPropertyName("doc_form")>
			Public Property Form As String

			''' <summary>文档模型语言</summary>
			<JsonPropertyName("doc_language")>
			Public Property Language As String = "Chinese"
		End Class

		''' <summary>文本文档</summary>
		Public Class TextDocument
			Inherits DocumentOption

			''' <summary>文档名称</summary>
			Public Property Name As String

			''' <summary>文档内容</summary>
			Public Property Text As String

		End Class

		''' <summary>文件文档</summary>
		Public Class FileDocument
			Inherits DocumentOption

			''' <summary>源文档 ID （选填）</summary>
			''' <remarks>
			''' 用于重新上传文档或修改文档清洗、分段配置，缺失的信息从源文档复制；
			''' 源文档不可为归档的文档；
			''' 当传入 original_document_id 时，代表文档进行更新操作，process_rule 为可填项目，不填默认使用源文档的分段方式；
			''' 未传入 original_document_id 时，代表文档进行新增操作，process_rule 为必填
			''' </remarks>
			<JsonPropertyName("original_document_id ")>
			Public Property ID As String
		End Class

		''' <summary>文档处理结果</summary>
		Public Class DocumentResult
			''' <summary>批次</summary>
			Public Property Batch As String

			''' <summary>文档信息</summary>
			Public Property Document As Document
		End Class

		''' <summary>处理规则</summary>
		Public Class ProcessRule

			''' <summary>处理规则</summary>
			Public Class Rules_
				''' <summary>分段规则</summary>
				Public Class Segmentation_

					''' <summary>自定义分段标识符， 目前仅允许设置一个分隔符。默认为 \n</summary>
					Public Property Separator As String

					''' <summary>分段最大长度 (token)，默认：1000</summary>
					<JsonPropertyName("max_tokens")>
					Public Property MaxTokens As Integer = 1000

					''' <summary>设置分段之间的重叠长度可以保留分段之间的语义关系，提升召回效果。建议设置为最大分段长度的10%-25%</summary>
					<JsonPropertyName("chunk_overlap")>
					Public Property ChunkOverlap As Integer = 100
				End Class

				''' <summary>预处理规则</summary>
				Public Class PreProcessingRules_

					''' <summary>预处理规则的唯一标识符(remove_extra_spaces:替换连续空格、换行符、制表符；remove_urls_emails:删除 URL、电子邮件地址)</summary>
					Public Property ID As String

					''' <summary>是否选中该规则，不传入文档 ID 时代表默认值</summary>
					Public Property Enabled As Boolean
				End Class

				''' <summary>预处理规则</summary>
				<JsonPropertyName("pre_processing_rules")>
				Public Property PreProcessingRules As PreProcessingRules_()

				''' <summary>分段规则</summary>
				Public Property Segmentation As Segmentation_
			End Class

			''' <summary>清洗、分段模式。automatic 自动 / custom 自定义</summary>
			Public Property Mode As String = "automatic"

			''' <summary>自定义规则（自动模式下，该字段为空）</summary>
			Public Property Rules As Rules_
		End Class

		''' <summary>索引状态</summary>
		Public Class IndexingStatus
			''' <summary>标识</summary>
			Public Property Id As String

			''' <summary>索引状态</summary>
			<JsonPropertyName("indexing_status")>
			Public Property IndexingStatus As String

			''' <summary>处理开始</summary>
			<JsonPropertyName("processing_started_at")>
			Public Property ProcessingStarted As Long

			''' <summary>处理完成</summary>
			<JsonPropertyName("parsing_completed_at")>
			Public Property ParsingCompleted As Long

			''' <summary>清除完成</summary>
			<JsonPropertyName("cleaning_completed_at")>
			Public Property CleaningCompleted As Long

			''' <summary>划分完成</summary>
			<JsonPropertyName("splitting_completed_at")>
			Public Property SplittingCompleted As Long

			''' <summary>完成时间</summary>
			<JsonPropertyName("completed_at")>
			Public Property Completed As Long?

			''' <summary>暂停时间</summary>
			<JsonPropertyName("paused_at")>
			Public Property Paused As Long?

			''' <summary>错误信息</summary>
			<JsonPropertyName("error")>
			Public Property Err As String

			''' <summary>停止时间</summary>
			<JsonPropertyName("stopped_at")>
			Public Property Stopped As Long?

			''' <summary>完成段落数</summary>
			<JsonPropertyName("completed_segments")>
			Public Property CompletedSegments As Integer

			''' <summary>总段落数</summary>
			<JsonPropertyName("total_segments")>
			Public Property TotalSegments As Integer
		End Class

		''' <summary>文档索引状态</summary>
		Public Class DocumentStatus
			Public Property Data As IndexingStatus()
		End Class

		''' <summary>段落基类</summary>
		Public Class SegmentBase

			''' <summary>文本内容/问题内容，必填</summary>
			Public Property Content As String

			''' <summary>答案内容，非必填，如果知识库的模式为qa模式则传值</summary>
			Public Property Answer As String

			''' <summary>关键字，非必填</summary>
			Public Property Keywords As String()

			''' <summary>启用，非必填</summary>
			Public Property Enabled As Boolean = True
		End Class

		''' <summary>段落</summary>
		Public Class Segment
			Inherits SegmentBase

			''' <summary>段落标识</summary>
			Public Property ID As String

			''' <summary>序号</summary>
			Public Property Position As Integer

			''' <summary>文档标识</summary>
			<JsonPropertyName("document_id")>
			Public Property Document As String

			''' <summary>字数</summary>
			<JsonPropertyName("word_count")>
			Public Property Words As Integer

			''' <summary>Tokens</summary>
			Public Property Tokens As Integer

			''' <summary>索引节点标识</summary>
			<JsonPropertyName("index_node_id")>
			Public Property NodeId As String

			''' <summary>索引节点 Hash</summary>
			<JsonPropertyName("index_node_hash")>
			Public Property NodeHash As String

			''' <summary>命中数</summary>
			<JsonPropertyName("hit_count")>
			Public Property Hits As Integer

			''' <summary>禁用人</summary>
			<JsonPropertyName("disabled_by")>
			Public Property DisabledBy As String

			''' <summary>禁用时间</summary>
			<JsonPropertyName("disabled_at")>
			Public Property Disabled As Long?

			''' <summary>状态</summary>
			Public Property Status As String

			''' <summary>创建人</summary>
			<JsonPropertyName("created_by")>
			Public Property CreatedBy As String

			''' <summary>创建时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

			''' <summary>索引时间</summary>
			<JsonPropertyName("indexing_at")>
			Public Property Indexing As Long

			''' <summary>完成时间</summary>
			<JsonPropertyName("completed_at")>
			Public Property Completed As Long

			''' <summary>错误信息</summary>
			<JsonPropertyName("error")>
			Public Property Err As String

			''' <summary>停止时间</summary>
			<JsonPropertyName("stopped_at")>
			Public Property Stopped As Long?

		End Class

		''' <summary>段落列表</summary>
		Public Class Segments

			''' <summary>段落列表</summary>
			Public Property Data As Segment()

			''' <summary>总记录数</summary>
			Public Property Total As Integer

			''' <summary>文档模型：qa_model/text_model 问答模型</summary>
			<JsonPropertyName("doc_form")>
			Public Property Form As String
		End Class

		''' <summary>段落项目</summary>
		Public Class SegmenItem

			''' <summary>段落列表</summary>
			Public Property Data As Segment

			''' <summary>文档模型：qa_model/text_model 问答模型</summary>
			<JsonPropertyName("doc_form")>
			Public Property Form As String
		End Class
	End Namespace

	Namespace Chat

		''' <summary>文件</summary>
		Public Class File
			''' <summary>支持类型：图片 image（目前仅支持图片格式）</summary>
			Public Property Type As String = "image"

			''' <summary>传递方式。remote_url: 图片地址。local_file: 上传文件。</summary>
			<JsonPropertyName("transfer_method")>
			Public Property Method As String = "remote_url"

			''' <summary>图片地址。（仅当传递方式为 remote_url 时）</summary>
			Public Property Url As String

			''' <summary>上传文件 ID。（仅当传递方式为 local_file 时）。</summary>
			<JsonPropertyName("upload_file_id")>
			Public Property FileId As String

		End Class

		''' <summary>聊天请求</summary>
		Public Class Request

			''' <summary>用户输入/提问内容</summary>
			Public Property Query As String

			''' <summary>允许传入 App 定义的各变量值。 inputs 参数包含了多组键值对（Key/Value pairs），每组的键对应一个特定变量，每组的值则是该变量的具体值。 默认 {}</summary>
			Public Property Inputs As New KeyValueDictionary

			''' <summary>输出模式，是边响应边输出还是操作完成后整体输出。流式模式：streaming；阻塞模式：blocking</summary>
			<JsonPropertyName("response_mode")>
			Public Property Mode As String = "streaming"

			''' <summary>用户标识，用于定义终端用户的身份，方便检索、统计。 由开发者定义规则，需保证用户标识在应用内唯一。</summary>
			Public Property User As String

			''' <summary>会话 ID（选填），需要基于之前的聊天记录继续对话，必须传之前消息的 conversation_id。</summary>
			<JsonPropertyName("conversation_id")>
			Public Property Conversation As String

			''' <summary>上传的文件</summary>
			Public Property Files As IEnumerable(Of File)

			''' <summary>（选填）自动生成标题，默认 true。 若设置为 false，则可通过调用会话重命名接口并设置 auto_generate 为 true 实现异步生成标题。</summary>
			<JsonPropertyName("auto_generate_name")>
			Public Property AutoTitle As Boolean = True
		End Class

		''' <summary>返回完整的 App 结果</summary>
		Public Class Response

			''' <summary>消息唯一 ID</summary>
			<JsonPropertyName("message_id")>
			Public Property MessageId As String

			''' <summary>会话 ID（选填），需要基于之前的聊天记录继续对话，必须传之前消息的 conversation_id。</summary>
			<JsonPropertyName("conversation_id")>
			Public Property ConversationId As String

			''' <summary>App 模式，固定为 chat</summary>
			Public Property Mode As String

			''' <summary>完整回复内容</summary>
			Public Property Answer As String

			''' <summary>元数据</summary>
			Public Property Metadata As Metadata

			''' <summary>创建时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

		End Class

		''' <summary>元数据</summary>
		Public Class Metadata

			''' <summary>上传的文件</summary>
			Public Property Usage As KeyValueDictionary

			''' <summary>引用和归属分段列表</summary>
			<JsonPropertyName("retriever_resources")>
			Public Property Retriever As IEnumerable(Of KeyValueDictionary)

		End Class
	End Namespace

	Namespace Workflow

		''' <summary>流程数据</summary>
		Public Class WorkflowItem
			Inherits WorkflowFinish
		End Class

		''' <summary>返回完整的 App 结果</summary>
		Public Class Response

			''' <summary>workflow 执行 ID</summary>
			<JsonPropertyName("workflow_run_id")>
			Public Property WorkflowId As String

			''' <summary>任务 ID，用于请求跟踪和下方的停止响应接口</summary>
			<JsonPropertyName("task_id")>
			Public Property TaskId As String

			''' <summary>详细内容</summary>
			Public Property Data As WorkflowItem

		End Class

		''' <summary>事件基类</summary>
		Public MustInherit Class EventBase
			''' <summary>workflow ID</summary>
			Public Property ID As String

		End Class

		''' <summary>流程开始</summary>
		Public Class WorkflowStart
			Inherits EventBase

			''' <summary>关联 Workflow ID</summary>
			<JsonPropertyName("workflow_id")>
			Public Property WorkflowId As String

			''' <summary>自增序号，App 内自增，从 1 开始</summary>
			<JsonPropertyName("sequence_number")>
			Public Property Sequence As Integer


			''' <summary>开始时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long
		End Class

		''' <summary>流程结束</summary>
		Public Class WorkflowFinish
			Inherits EventBase

			''' <summary>关联 Workflow ID</summary>
			<JsonPropertyName("workflow_id")>
			Public Property WorkflowId As String

			''' <summary>执行状态, running / succeeded / failed / stopped</summary>
			Public Property Status As String

			''' <summary>输出内容</summary>
			Public Property Outputs As KeyValueDictionary

			''' <summary>错误原因</summary>
			<JsonPropertyName("error")>
			Public Property Err As String

			''' <summary>耗时(s)</summary>
			<JsonPropertyName("elapsed_time")>
			Public Property Elapsed As Single

			''' <summary>总使用 tokens</summary>
			<JsonPropertyName("total_tokens")>
			Public Property Total As Integer

			''' <summary>总步数</summary>
			<JsonPropertyName("total_steps")>
			Public Property Steps As Integer

			''' <summary>开始时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

			''' <summary>结束时间</summary>
			<JsonPropertyName("finished_at")>
			Public Property Finished As Long
		End Class

		''' <summary>节点开始</summary>
		Public Class NodeStart
			Inherits EventBase

			''' <summary>节点 ID</summary>
			<JsonPropertyName("node_id")>
			Public Property NodeId As String

			''' <summary>节点类型</summary>
			<JsonPropertyName("node_type")>
			Public Property NodeType As String

			''' <summary>节点名称</summary>
			Public Property Title As String

			''' <summary>执行序号，用于展示 Tracing Node 顺序</summary>
			Public Property Index As Integer

			''' <summary>前置节点 ID，用于画布展示执行路径</summary>
			<JsonPropertyName("predecessor_node_id")>
			Public Property BeforeId As String

			''' <summary>节点中所有使用到的前置节点变量内容</summary>
			Public Property Inputs As IEnumerable(Of Object)

			''' <summary>开始时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long
		End Class

		''' <summary>节点结束</summary>
		Public Class NodeFinish
			Inherits EventBase

			Public Class NodeMeta
				''' <summary>总使用 tokens</summary>
				<JsonPropertyName("total_tokens")>
				Public Property Tokens As Integer

				''' <summary>总费用</summary>
				<JsonPropertyName("total_price")>
				Public Property Price As Decimal

				''' <summary>货币</summary>
				<JsonPropertyName("currency")>
				Public Property Currency As String
			End Class

			''' <summary>节点 ID</summary>
			<JsonPropertyName("node_id")>
			Public Property NodeId As String

			''' <summary>节点类型</summary>
			<JsonPropertyName("node_type")>
			Public Property NodeType As String

			''' <summary>执行序号，用于展示 Tracing Node 顺序</summary>
			Public Property Index As Integer

			''' <summary>前置节点 ID，用于画布展示执行路径</summary>
			<JsonPropertyName("predecessor_node_id")>
			Public Property BeforeId As String

			''' <summary>节点中所有使用到的前置节点变量内容</summary>
			Public Property Inputs As IEnumerable(Of Object)

			''' <summary>节点过程数据</summary>
			<JsonPropertyName("process_data")>
			Public Property ProcessData As Object

			''' <summary>输出内容</summary>
			Public Property Outputs As Object

			''' <summary>执行状态, running / succeeded / failed / stopped</summary>
			Public Property Status As String

			''' <summary>错误原因</summary>
			<JsonPropertyName("error")>
			Public Property Err As String

			''' <summary>耗时(s)</summary>
			<JsonPropertyName("elapsed_time")>
			Public Property Elapsed As Single

			''' <summary>开始时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

			''' <summary>元数据</summary>
			<JsonPropertyName("execution_metadata")>
			Public Property Total As NodeMeta

		End Class

	End Namespace

	Namespace Events
		''' <summary>事件基类</summary>
		Public Class Base

			''' <summary>事件类型 </summary>
			''' <remarks>
			''' message：LLM 返回文本块事件；
			''' agent_message：Agent 模式下返回文本块事件；
			''' agent_thought：Agent 模式下有关 Agent 思考步骤的相关内容，涉及到工具调用（仅Agent模式下使用）；
			''' message_file：文件事件，表示有新文件需要展示；
			''' message_end：消息结束事件，收到此事件则代表流式返回结束；
			''' message_replace：消息内容替换事件。 开启内容审查和审查输出内容时，若命中了审查条件，则会通过此事件替换消息内容为预设回复；
			''' error：流式输出过程中出现的异常会以 stream event 形式输出，收到异常事件后即结束；
			''' ping：每 10s 一次的 ping 事件，保持连接存活。
			''' </remarks>
			<JsonPropertyName("event")>
			Public Property EventType As String

			''' <summary>任务 ID，用于请求跟踪和下方的停止响应接口</summary>
			<JsonPropertyName("task_id")>
			Public Property TaskId As String

			''' <summary>消息唯一 ID</summary>
			<JsonPropertyName("message_id")>
			Public Property MessageId As String

			''' <summary>会话 ID</summary>
			<JsonPropertyName("conversation_id")>
			Public Property ConversationId As String

		End Class

		''' <summary>事件基类</summary>
		Public Class MessageEvent
			Inherits Base

			''' <summary>LLM 返回文本块事件</summary>
			Public Property Answer As String

			''' <summary>创建时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long
		End Class

		''' <summary>Agent模式下返回文本块事件</summary>
		Public Class AgentMessageEvent
			Inherits MessageEvent
		End Class

		''' <summary>Agent 模式下有关 Agent 思考步骤的相关内容，涉及到工具调用</summary>
		Public Class AgentThoughtEvent
			Inherits Base

			''' <summary>agent_thought ID，每一轮 Agent 迭代都会有一个唯一的id</summary>
			Public Property ID As String

			''' <summary>agent_thought 在消息中的位置，如第一轮迭代 position 为 1</summary>
			Public Property Position As Integer

			''' <summary>agent 的思考内容</summary>
			Public Property Thought As String

			''' <summary>工具调用的返回结果</summary>
			Public Property Observation As String

			''' <summary>使用的工具列表，以 ; 分割多个工具</summary>
			Public Property Tools As String

			''' <summary>工具的输入，JSON格式的字符串(object)。如：{"dalle3": {"prompt": "a cute cat"}}</summary>
			<JsonPropertyName("tool_input")>
			Public Property ToolInput As String

			''' <summary>创建时间</summary>
			<JsonPropertyName("created_at")>
			Public Property Created As Long

			''' <summary>使用的工具列表，以 ; 分割多个工具</summary>
			<JsonPropertyName("message_files")>
			Public Property Files As String()

		End Class

		''' <summary>文件事件，表示有新文件需要展示</summary>
		Public Class FileEvent
			Inherits Base

			''' <summary>文件唯一ID</summary>
			Public Property ID As String

			''' <summary>文件类型，目前仅为 image</summary>
			Public Property Type As String

			''' <summary>文件归属，user或assistant，该接口返回仅为 assistant</summary>
			<JsonPropertyName("belongs_to")>
			Public Property Belongs As String

			''' <summary>文件访问地址</summary>
			Public Property Url As String
		End Class

		''' <summary>消息结束事件，收到此事件则代表流式返回结束</summary>
		Public Class EndEvent
			Inherits Base

			''' <summary>元数据</summary>
			Public Property Metadata As Metadata
		End Class

		''' <summary>消息内容替换事件。 开启内容审查和审查输出内容时，若命中了审查条件，则会通过此事件替换消息内容为预设回复。</summary>
		Public Class ReplaceEvent
			Inherits MessageEvent
		End Class

		''' <summary>流式输出过程中出现的异常会以 stream event 形式输出，收到异常事件后即结束。</summary>
		Public Class ErrorEvent
			Inherits Base

			''' <summary>HTTP 状态码</summary>
			Public Property Status As Integer

			''' <summary>错误码</summary>
			Public Property Code As String

			''' <summary>错误消息</summary>
			Public Property Message As String
		End Class

		''' <summary>流程事件</summary>
		''' <remarks>
		''' ping：每 10s 一次的 ping 事件，保持连接存活。
		''' 
		''' workflow_started workflow 开始执行
		''' node_started node 开始执行
		''' node_finished node 执行结束，成功失败同一事件中不同状态
		''' workflow_finished workflow 执行结束，成功失败同一事件中不同状态
		''' </remarks>
		Public Class WorkflowEvent(Of T)

			<JsonPropertyName("event")>
			Public Property EventType As String

			''' <summary>任务 ID，用于请求跟踪和下方的停止响应接口</summary>
			<JsonPropertyName("task_id")>
			Public Property TaskId As String

			''' <summary>workflow 执行 ID</summary>
			<JsonPropertyName("workflow_run_id")>
			Public Property WorkflowId As String

			''' <summary>事件内容</summary>
			Public Property Data As T
		End Class


	End Namespace
End Namespace
