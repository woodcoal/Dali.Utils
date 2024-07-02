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
' 	枚举
'
' 	name: Enums
' 	create: 2024-06-05
' 	memo: 枚举
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>进程状态</summary>
	Public Enum ProcessStatusEnum
		''' <summary>还未处理</summary>
		NONE

		''' <summary>开始处理</summary>
		BEGIN

		''' <summary>处理中</summary>
		PROCESS

		''' <summary>处理完成</summary>
		FINISH

		''' <summary>处理失败</summary>
		FAIL
	End Enum

	''' <summary>AI 类型</summary>
	Public Enum AITypeEnum

		''' <summary>Ollama</summary>
		OLLAMA

		''' <summary>OneApi</summary>
		ONEAPI

		''' <summary>Dify</summary>
		DIFY
	End Enum

End Namespace