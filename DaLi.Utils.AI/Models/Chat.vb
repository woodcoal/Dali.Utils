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
' 	聊天结构
'
' 	name: Model.Chat
' 	create: 2024-06-05
' 	memo: 聊天结构
'
' ------------------------------------------------------------

Imports System.Text.Json.Serialization

Namespace Model

	''' <summary>聊天消息参数</summary>
	Public Class ChatMessage

		''' <summary>消息的角色 system，user 或者 assistant</summary>
		<JsonPropertyName("role")>
		Public Property Role As String

		''' <summary>消息内容</summary>
		<JsonPropertyName("content")>
		Public Property Content As String

		''' <summary>（可选）base64 编码图像的列表（对于多模式模型，例如llava）</summary>
		<JsonPropertyName("images")>
		Public Property Images As IEnumerable(Of String)
	End Class

	'''' <summary>默认请求基类</summary>
	'Public Class Options
	'	''' <summary>（必填）型号名称</summary>
	'	Public Property Model As String

	'	''' <summary>聊天的消息，这个可以用来保留聊天记忆</summary>
	'	Public Property Messages As IEnumerable(Of Message)

	'	''' <summary>返回响应的格式。目前唯一接受的值是json</summary>
	'	Public Property Format As String

	'	''' <summary>模型文件文档中列出的其他模型参数，例如：temperature</summary>
	'	Public Property Options As IDictionary(Of String, Object)

	'	''' <summary>返回模式，空或者 true:流式返回 / false: 一次性全部返回</summary>
	'	Public Property Stream As Boolean
	'End Class

	''' <summary>聊天反馈结果</summary>
	Public Class ChatResult

		'''' <summary>队列编号，仅个别接口存在此值</summary>
		'Public Property ID As String

		''' <summary>生成内容</summary>
		Public Property Message As ChatMessage

		''' <summary>型号名称</summary>
		Public Property Model As String

		''' <summary>最后更新时间</summary>
		Public Property Last As Date

		''' <summary>是否成功返回，成功则 Message 为结果信息，否则 Message 为错误信息</summary>
		Public Property Success As Boolean

		''' <summary>花费 Token 数量及时间信息</summary>
		Public Property Tokens As TokensInfo

	End Class
End Namespace