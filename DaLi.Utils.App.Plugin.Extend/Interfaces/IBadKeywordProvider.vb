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
' 	应用上下文接口
'
' 	name: Interface.IBadKeywordProvider
' 	create: 2023-02-19
' 	memo: 应用上下文接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>应用上下文接口</summary>
	Public Interface IBadKeywordProvider

		''' <summary>是否包含无效用户名</summary>
		Function BadUser(source As String) As Boolean

		''' <summary>是否包含无效关键词</summary>
		Function Contains(source As String) As Boolean

		''' <summary>替换关键词</summary>
		Function Replace(source As String) As String

	End Interface

End Namespace