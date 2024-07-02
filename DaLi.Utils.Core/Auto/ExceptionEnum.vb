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
' 	错误类型枚举
'
' 	name: Auto.ExceptionEnum
' 	create: 2023-01-06
' 	memo: 错误类型枚举
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Auto
	''' <summary>错误类型枚举</summary>
	Public Enum ExceptionEnum
		''' <summary>通用错误</summary>
		<Description("通用错误")>
		NORMAL

		''' <summary>执行错误</summary>
		<Description("执行错误")>
		EXECUTE_ERROR

		''' <summary>内部错误</summary>
		<Description("内部错误")>
		INNER_EXCEPTION

		''' <summary>无结果</summary>
		<Description("无结果")>
		NO_RESULT

		''' <summary>循环停止</summary>
		<Description("循环停止")>
		LOOP_STOP

		''' <summary>循环中断</summary>
		<Description("循环中断")>
		LOOP_BREAK

		''' <summary>值验证失败</summary>
		<Description("验证失败")>
		VALUE_VALIDATE

	End Enum
End Namespace