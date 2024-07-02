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
' 	默认配置
'
' 	name: AISettings
' 	create: 2024-06-05
' 	memo: 默认配置
'
' ------------------------------------------------------------

''' <summary>默认配置</summary>
Public Class AISettings

#Region "OLLAMA"

	''' <summary>Ollama 默认服务器地址 http://***:11434</summary>
	Public Shared Property OLLAMA_URL As String

	''' <summary>Ollama 默认模型名称</summary>
	Public Shared Property OLLAMA_MODEL As String

#End Region

#Region "DIFY"

	''' <summary>Dify 默认服务器地址 http://ai.hndl.vip/</summary>
	Public Shared Property DIFY_URL As String

	''' <summary>Dify 助手鉴权 ApiKey</summary>
	Public Shared Property DIFY_KEY_AGENT As String

	''' <summary>Dify 文本补全鉴权 ApiKey</summary>
	Public Shared Property DIFY_KEY_TEXT As String

	''' <summary>Dify 对话鉴权 ApiKey</summary>
	Public Shared Property DIFY_KEY_CHAT As String

	''' <summary>Dify 流程鉴权 ApiKey</summary>
	Public Shared Property DIFY_KEY_WORKFLOW As String

	''' <summary>Dify 知识库鉴权 ApiKey (dataset-******)</summary>
	Public Shared Property DIFY_KEY_DATASETS As String

#End Region

#Region "ONEAPI"

	''' <summary>ONEAPI 默认服务器地址 http://***/</summary>
	Public Shared Property ONEAPI_URL As String

	''' <summary>ONEAPI 默认模型名称</summary>
	Public Shared Property ONEAPI_MODEL As String

	''' <summary>ONEAPI ApiKey</summary>
	Public Shared Property ONEAPI_KEY As String
#End Region

End Class
