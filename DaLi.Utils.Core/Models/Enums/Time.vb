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
' 	memo: 时间周期频率
' 	
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>时间周期</summary>
	Public Enum TimeIntervalEnum
		''' <summary>关闭</summary>
		<Description("无")>
		DISABLE

		''' <summary>周期任务，每隔指定（秒）执行的任务，准点执行，错过验证时间则跳过此次，如每隔10秒执行任务，如：每隔10秒执行任务，第20秒没有执行检测，则跳过此次</summary>
		<Description("周期，错过不运行")>
		INTERVAL

		''' <summary>周期延时任务，超过指定（秒）执行的任务，只要当前时间与最后一次任务时间的差大于间隔时间则执行任务</summary>
		<Description("周期，错过也运行")>
		INTERVAL_OVER

		''' <summary>指定时分秒，具体到秒，时间准点才执行，超过不执行</summary>
		<Description("指定时分秒，错过不运行")>
		FIX

		''' <summary>指定时分秒，具体到秒，时间准点才执行，准时或者超时未执行则继续执行</summary>
		<Description("指定时分秒，错过也运行")>
		FIX_OVER

		''' <summary>指定小时与分钟，按分钟准点执行，超过不执行</summary>
		<Description("指定时分，错过不运行")>
		FIX_MINUTE

		''' <summary>指定小时与分钟，按分钟准点执行，准时或者超时未执行则继续执行</summary>
		<Description("指定时分，错过也运行")>
		FIX_MINUTE_OVER

		''' <summary>指定小时，按小时准点执行，超过不执行</summary>
		<Description("指定小时，错过不运行")>
		FIX_HOUR

		''' <summary>指定小时，按小时准点执行，准时或者超时未执行则继续执行</summary>
		<Description("指定小时，错过也运行")>
		FIX_HOUR_OVER
	End Enum

	''' <summary>频率</summary>
	Public Enum TimeFrequencyEnum
		''' <summary>不限</summary>
		<Description("不限")>
		NONE

		''' <summary>每秒一条</summary>
		<Description("秒")>
		SECOND

		''' <summary>每分钟一条</summary>
		<Description("分钟")>
		MINUTE

		''' <summary>每小时一条</summary>
		<Description("小时")>
		HOUR

		''' <summary>每日一条</summary>
		<Description("日")>
		DAY

		''' <summary>每周一条</summary>
		<Description("周")>
		WEEK

		''' <summary>每月一条</summary>
		<Description("月")>
		MONTH

		''' <summary>每年一条</summary>
		<Description("年")>
		YEAR

	End Enum

End Namespace
