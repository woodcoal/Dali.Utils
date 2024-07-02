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
' 	公共参数枚举
'
' 	name: Model
' 	create: 2019-03-14
' 	memo: 日志记录类型
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>日志记录类型</summary>
	Public Enum LogLevelEnum
		''' <summary>包含最详细消息的日志。 这些消息可能包含敏感应用程序数据。 这些消息默认情况下处于禁用状态，并且绝不应在生产环境中启用。</summary>
		<Description("记录")>
		TRACE = 0

		''' <summary>在开发过程中用于交互式调查的日志。 这些日志应主要包含对调试有用的信息，并且没有长期价值。</summary>
		<Description("调试")>
		DEBUG = 1

		''' <summary>跟踪应用程序的常规流的日志。 这些日志应具有长期价值。</summary>
		<Description("信息")>
		INFORMATION = 2

		''' <summary>突出显示应用程序流中的异常或意外事件（不会导致应用程序执行停止）的日志。</summary>
		<Description("警告")>
		WARNING = 3

		''' <summary>当前执行流因故障而停止时突出显示的日志。 这些日志指示当前活动中的故障，而不是应用程序范围内的故障。</summary>
		<Description("错误")>
		[ERROR] = 4

		''' <summary>描述不可恢复的应用程序/系统崩溃或需要立即引起注意的灾难性故障的日志。</summary>
		<Description("崩溃")>
		CRITICAL = 5

		''' <summary>不用于写入日志消息。 指定日志记录类别不应写入任何消息。</summary>
		<Description("禁用")>
		NONE = 6

	End Enum


End Namespace
