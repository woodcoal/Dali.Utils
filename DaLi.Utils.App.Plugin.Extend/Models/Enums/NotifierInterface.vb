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
' 	create: 2024-03-05
' 	memo: 通知接口
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model


	''' <summary>非法关键词检查类型</summary>
	Public Enum NotifierInterfaceEnum

		''' <summary>webHook</summary>
		<Description("webHook")>
		WEBHOOK

		''' <summary>邮箱</summary>
		<Description("邮箱")>
		EMAIL

		''' <summary>短信</summary>
		<Description("短信")>
		SMS

	End Enum
End Namespace
