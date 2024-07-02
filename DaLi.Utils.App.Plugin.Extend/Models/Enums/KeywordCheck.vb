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
' 	name: Model
' 	create: 2023-02-19
' 	memo: 非法关键词检查类型
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model


	''' <summary>非法关键词检查类型</summary>
	Public Enum KeywordCheckEnum

		''' <summary>不检查</summary>
		<Description("不检查")>
		NONE = 0

		''' <summary>检查用户名</summary>
		<Description("用户名")>
		USER

		''' <summary>仅检查非法关键词</summary>
		<Description("仅检查")>
		CHECK

		''' <summary>替换非法关键词</summary>
		<Description("替换")>
		REPLACE

	End Enum
End Namespace
