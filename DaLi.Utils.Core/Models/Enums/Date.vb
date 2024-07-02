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
' 	create: 2023-01-03
' 	memo: 日期枚举
' 	
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>日期枚举</summary>
	Public Enum DateNameEnum
		''' <summary>每天</summary>
		<Description("每天")>
		ALL = 0

		''' <summary>周一</summary>
		<Description("周一")>
		MONDAY = 1

		''' <summary>周二</summary>
		<Description("周二")>
		TUESDAY

		''' <summary>周三</summary>
		<Description("周三")>
		WEDNESDAY

		''' <summary>周四</summary>
		<Description("周四")>
		THURSDAY

		''' <summary>周五</summary>
		<Description("周五")>
		FRIDAY

		''' <summary>周六</summary>
		<Description("周六")>
		SATURDAY

		''' <summary>周日</summary>
		<Description("周日")>
		SUNDAY

		''' <summary>月初</summary>
		<Description("月初")>
		MONTH_FIRST

		''' <summary>月末</summary>
		<Description("月末")>
		MONTH_LAST

		' 10
		''' <summary>工作日</summary>
		<Description("工作日")>
		WORKDAY

		''' <summary>月初工作日</summary>
		<Description("月初工作日")>
		WORKDAY_MONTH_FIRST

		''' <summary>月末工作日</summary>
		<Description("月末工作日")>
		WORKDAY_MONTH_LAST

		''' <summary>周首个工作日</summary>
		<Description("周首个工作日")>
		WORKDAY_WEEK_FIRST

		''' <summary>周最后工作日</summary>
		<Description("周最后工作日")>
		WORKDAY_WEEK_LAST

		''' <summary>调休日</summary>
		<Description("调休日")>
		ADJUSTDAY

		''' <summary>节假日</summary>
		<Description("节假日")>
		HOLIDAY

		''' <summary>节假日前一日</summary>
		<Description("节假日前一日")>
		HOLIDAY_BEFORE

		''' <summary>节假日第一日</summary>
		<Description("节假日第一日")>
		HOLIDAY_FIRST

		''' <summary>节假日最后一日</summary>
		<Description("节假日最后一日")>
		HOLIDAY_LAST

		''' <summary>节假日后一日</summary>
		<Description("节假日后一日")>
		HOLIDAY_AFTER

		''' <summary>休息日</summary>
		<Description("休息日")>
		RESTDAY

		''' <summary>休息日前一日</summary>
		<Description("休息日前一日")>
		RESTDAY_BEFORE

		''' <summary>休息日第一日</summary>
		<Description("休息日第一日")>
		RESTDAY_FIRST

		''' <summary>休息日最后一日</summary>
		<Description("休息日最后一日")>
		RESTDAY_LAST

		''' <summary>休息日后一日</summary>
		<Description("休息日后一日")>
		RESTDAY_AFTER

		''' <summary>今天</summary>
		<Description("昨天")>
		YESTERDAY

		''' <summary>今天</summary>
		<Description("今天")>
		TODAY

		''' <summary>今天</summary>
		<Description("明天")>
		TOMORROW

	End Enum

End Namespace
