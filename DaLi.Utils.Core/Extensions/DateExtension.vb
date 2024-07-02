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
' 	日期扩展操作
'
' 	name: Extension.DateExtension
' 	create: 2020-10-23
' 	memo: 日期扩展操作
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>日期扩展操作</summary>
	Public Module DateExtension

		''' <summary>1970-1-1 的时间戳数值</summary>
		Public Const TICKS_19700101 = 621355968000000000

#Region "1. 格式转换"

		''' <summary>将当前时间转换成整数，仅转换1970-2050 年，精确到秒</summary>
		<Extension>
		Public Function ToInteger(this As Date) As Integer
			Dim R = 0

			If this >= New Date(1970, 1, 1) And this < New Date(2050, 1, 1) Then
				R = ((this.Ticks - TICKS_19700101) / 10000000) + Integer.MinValue
			End If

			Return R
		End Function

		''' <summary>获取 Unix 时间戳，注意 2038 年问题，最小单位为秒</summary>
		<Extension>
		Public Function UnixTicks(this As Date) As Integer
			this = this.Range(New Date(1970, 1, 1), New Date(2038, 1, 1))
			Return (this.ToUniversalTime.Ticks - TICKS_19700101) / 10000000
		End Function

		''' <summary>扩展获取 Unix 时间戳，最小单位为秒，UINT 可以支持到 2106 年</summary>
		<Extension>
		Public Function UnixTicks2(this As Date) As UInteger
			this = this.Range(New Date(1970, 1, 1), New Date(2106, 1, 1))
			Return (this.ToUniversalTime.Ticks - TICKS_19700101) / 10000000
		End Function

		''' <summary>获取 Js 时间戳，最小单位为毫秒</summary>
		<Extension>
		Public Function JsTicks(this As Date) As Long
			If this >= New Date(1970, 1, 1) Then
				Return (this.ToUniversalTime.Ticks - TICKS_19700101) / 10000
			Else
				Return 0
			End If
		End Function

		''' <summary>将无符号整数转成时间，使用 UnixTicks 方式，最小单位：秒</summary>
		<Extension>
		Public Function ToDate(this As UInteger) As Date
			Return New Date((this * 10000000) + TICKS_19700101)
		End Function

		''' <summary>将当前时间转换成整数，仅转换1970-2050 年</summary>
		''' <param name="isUnix">当前数值是否 Unix Ticks</param>
		<Extension>
		Public Function ToDate(this As Integer, Optional isUnix As Boolean = True) As Date
			Dim Ticks As Long = this

			If isUnix Then
				If Ticks < 0 Then Ticks = 0
			Else
				Ticks -= Integer.MinValue
			End If

			Ticks = (Ticks * 10000000) + TICKS_19700101
			Return New Date(Ticks)
		End Function

		''' <summary>将当前时间转换成整数，仅转换1970-2050 年</summary>
		''' <param name="isJs">当前数值是否 Js Ticks</param>
		<Extension>
		Public Function ToDate(this As Long, Optional isJs As Boolean = True) As Date
			Dim Ticks As Long = this

			If isJs Then
				If Ticks < 0 Then Ticks = 0
				Ticks = (Ticks * 10000) + TICKS_19700101
			End If

			Return New Date(Ticks)
		End Function

#End Region

#Region "2. 输出格式化"

		''' <summary>计算当前时间与过去时间的时间差（当前时间-过去时间)</summary>
		''' <param name="this">过去时间</param>
		''' <param name="target">当前时间</param>
		''' <param name="isEN">是否返回英文字符串</param>
		<Extension>
		Public Function ShowDiff(this As DateTimeOffset, Optional target As DateTimeOffset = Nothing, Optional isEN As Boolean = False) As String
			If target = New DateTimeOffset Then target = SYS_NOW

			Dim Ticks = Math.Abs(target.Subtract(this).Ticks)
			Return New TimeSpan(Ticks).Show(isEN)
		End Function

		''' <summary>计算当前时间与过去时间的时间差（当前时间-过去时间)</summary>
		''' <param name="this">过去时间</param>
		''' <param name="target">当前时间</param>
		''' <param name="isEN">是否返回英文字符串</param>
		<Extension>
		Public Function ShowDiff(this As Date, Optional target As Date = Nothing, Optional isEN As Boolean = False) As String
			If target = New Date Then target = SYS_NOW_DATE

			Dim Ticks = Math.Abs(target.Subtract(this).Ticks)
			Return New TimeSpan(Ticks).Show(isEN)
		End Function

		''' <summary>将秒换算成对应的时长</summary>
		<Extension>
		Public Function ToTimeSpan(this As Long) As String
			Return New TimeSpan(this).ToString("c")
		End Function

		''' <summary>将 TimeSpan 格式化</summary>
		''' <param name="isEN">是否返回英文字符串</param>
		<Extension>
		Public Function Show(this As TimeSpan, Optional isEN As Boolean = False) As String
			If this.Ticks < 1 Then Return "-"

			With New Text.StringBuilder
				If this.TotalSeconds > 30 Then
					If this.Days > 0 Then
						.Append(this.Days)
						.Append(If(isEN, "day ", "天 "))
					End If

					If this.Hours > 0 Then
						.Append(this.Hours)
						.Append(If(isEN, "hour ", "小时 "))
					End If

					If this.Minutes > 0 Then
						.Append(this.Minutes)
						.Append(If(isEN, "min ", "分钟 "))
					End If

					If this.Seconds > 0 Then
						.Append(this.Seconds)
						.Append(If(isEN, "sec", "秒"))
					End If
				ElseIf this.TotalSeconds > 1 Then
					.Append(this.TotalSeconds.ToString("0.00"))
					.Append(If(isEN, "sec", "秒"))
				Else
					.Append(this.TotalMilliseconds)
					.Append(If(isEN, "ms ", "毫秒"))
				End If

				Return .ToString
			End With
		End Function

#End Region

#Region "3. 计算操作"

		''' <summary>获取指定时间的所在频率内的有效时间起始。如:12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</summary>
		''' <param name="This">要操作的时间</param>
		''' <param name="frequencyOption">频率</param>
		''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		''' <returns>DateStart: 开始时间；DateEnd：结束时间</returns>
		<Extension>
		Public Function Frequency(this As Date?, frequencyOption As TimeFrequencyEnum, delayValue As Integer) As (DateStart As Date, DateEnd As Date)
			Dim dateQuery As Date = If(this Is Nothing, SYS_NOW_DATE, this)
			Return dateQuery.Frequency(frequencyOption, delayValue)
		End Function

		''' <summary>获取指定时间的所在频率内的有效时间起始。如:12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</summary>
		''' <param name="This">要操作的时间，基准时间以 2000-1-1 为准</param>
		''' <param name="frequencyOption">频率</param>
		''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		''' <returns>DateStart: 开始时间；DateEnd：结束时间</returns>
		<Extension>
		Public Function Frequency(this As Date, frequencyOption As TimeFrequencyEnum, delayValue As Integer) As (DateStart As Date, DateEnd As Date)
			Dim DateStart As Date
			Dim DateEnd As Date
			If Not this.IsValidate Then this = New Date(2000, 1, 1)

			If delayValue < 1 Then delayValue = 1

			Select Case frequencyOption
				Case TimeFrequencyEnum.SECOND
					DateStart = this.Date.AddHours(this.Hour).AddMinutes(this.Minute).AddSeconds(this.Second + 1)
					DateEnd = this.Date.AddHours(this.Hour).AddMinutes(this.Minute).AddSeconds(this.Second)

					DateStart = DateStart.AddSeconds(0 - delayValue)
					DateEnd = DateEnd.AddSeconds(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.MINUTE
					DateStart = this.Date.AddHours(this.Hour).AddMinutes(this.Minute + 1)
					DateEnd = this.Date.AddHours(this.Hour).AddMinutes(this.Minute)

					DateStart = DateStart.AddMinutes(0 - delayValue)
					DateEnd = DateEnd.AddMinutes(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.HOUR
					DateStart = this.Date.AddHours(this.Hour + 1)
					DateEnd = this.Date.AddHours(this.Hour)

					DateStart = DateStart.AddHours(0 - delayValue)
					DateEnd = DateEnd.AddHours(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.DAY
					DateStart = this.Date.AddDays(1)
					DateEnd = this.Date

					DateStart = DateStart.AddDays(0 - delayValue)
					DateEnd = DateEnd.AddDays(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.WEEK
					Dim w As Integer = this.DayOfWeek
					If w = 0 Then w = 7
					DateStart = this.Date.AddDays(8 - w)
					DateEnd = this.Date.AddDays(1 - w)

					DateStart = DateStart.AddDays(0 - (delayValue * 7))
					DateEnd = DateEnd.AddDays(delayValue * 7).AddMilliseconds(-1)

				Case TimeFrequencyEnum.MONTH
					DateStart = New Date(this.Year, this.Month, 1).AddMonths(1)
					DateEnd = New Date(this.Year, this.Month, 1)

					DateStart = DateStart.AddMonths(0 - delayValue)
					DateEnd = DateEnd.AddMonths(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.YEAR
					DateStart = New Date(this.Year, 1, 1).AddYears(1)
					DateEnd = New Date(this.Year, 1, 1)

					DateStart = DateStart.AddYears(0 - delayValue)
					DateEnd = DateEnd.AddYears(delayValue).AddMilliseconds(-1)

				Case Else
					DateStart = this
					DateEnd = this
			End Select

			Return (DateStart, DateEnd)
		End Function

		''' <summary>获取以指定时间为基准，获取需要查询时间所在频率内的有效时间起始。如：2020年1月1日10点为基准，每3天为周期，那么 2020年1月4日12点所在区域为 2020年1月3日10点至2020年1月5日9点末</summary>
		''' <param name="This">要操作的时间</param>
		''' <param name="frequencyOption">频率</param>
		''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		''' <param name="baseDate">基准日期，用于计算间隔的初始日期，未设置则为 1年1月1日，根据周期将自动精确到指定单位</param>
		''' <returns>DateStart: 开始时间；DateEnd：结束时间</returns>
		<Extension>
		Public Function Frequency(this As Date, frequencyOption As TimeFrequencyEnum, delayValue As Integer, baseDate As Date) As (DateStart As Date, DateEnd As Date)
			Dim DateStart As Date
			Dim DateEnd As Date

			If delayValue < 1 Then delayValue = 1

			Select Case frequencyOption
				Case TimeFrequencyEnum.SECOND
					Dim seconds = this.Subtract(baseDate).TotalSeconds
					seconds = Math.Floor(seconds / delayValue) * delayValue

					DateStart = baseDate.AddSeconds(seconds)
					DateEnd = DateStart.AddSeconds(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.MINUTE
					baseDate = baseDate.Date.AddHours(baseDate.Hour).AddMinutes(baseDate.Minute)
					Dim minutes = this.Subtract(baseDate).TotalMinutes
					minutes = Math.Floor(minutes / delayValue) * delayValue

					DateStart = baseDate.AddMinutes(minutes)
					DateEnd = DateStart.AddMinutes(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.HOUR
					baseDate = baseDate.Date.AddHours(baseDate.Hour)
					Dim hours = this.Subtract(baseDate).TotalHours
					hours = Math.Floor(hours / delayValue) * delayValue

					DateStart = baseDate.AddHours(hours)
					DateEnd = DateStart.AddHours(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.DAY
					baseDate = baseDate.Date
					Dim days = this.Subtract(baseDate).TotalDays
					days = Math.Floor(days / delayValue) * delayValue

					DateStart = baseDate.AddDays(days)
					DateEnd = DateStart.AddDays(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.WEEK
					' 分析指定日期的周一
					baseDate = baseDate.Frequency(TimeFrequencyEnum.WEEK, 1).DateStart

					Dim days = this.Subtract(baseDate).TotalDays
					days = Math.Floor(days / delayValue / 7) * delayValue * 7

					DateStart = baseDate.AddDays(days)
					DateEnd = DateStart.AddDays(delayValue * 7).AddMilliseconds(-1)

					'Dim w As Integer = this.DayOfWeek
					'If w = 0 Then w = 7
					'DateStart = this.Date.AddDays(8 - w)
					'DateEnd = this.Date.AddDays(1 - w)

					'DateStart = DateStart.AddDays(0 - (delayValue * 7))
					'DateEnd = DateEnd.AddDays(delayValue * 7).AddMilliseconds(-1)

				Case TimeFrequencyEnum.MONTH
					baseDate = New Date(baseDate.Year, baseDate.Month, 1)
					Dim months = (this.Year * 12) + this.Month - (baseDate.Year * 12) - baseDate.Month
					months = Math.Floor(months / delayValue) * delayValue

					DateStart = baseDate.AddMonths(months)
					DateEnd = DateStart.AddMonths(delayValue).AddMilliseconds(-1)

				Case TimeFrequencyEnum.YEAR
					Dim years = this.Year - baseDate.Year
					years = Math.Floor(years / delayValue) * delayValue

					DateStart = baseDate.AddYears(years)
					DateEnd = DateStart.AddYears(delayValue).AddMilliseconds(-1)

				Case Else
					DateStart = this
					DateEnd = this
			End Select

			Return (DateStart, DateEnd)
		End Function

#End Region

#Region "4. 基本日期判断"

		''' <summary>当前日期是否有效，大于 2000 年才有效</summary>
		<Extension>
		Public Function IsValidate(this As Date) As Boolean
			Return this >= New Date(2000, 1, 1)
		End Function

		''' <summary>当前日期是否有效，大于 2000 年才有效</summary>
		<Extension>
		Public Function IsValidate(this As Date?) As Boolean
			Return this IsNot Nothing AndAlso this >= New Date(2000, 1, 1)
		End Function

		''' <summary>指定日期是不是在日期组中</summary>
		''' <param name="this">要操作的时间</param>
		''' <param name="days">日期组</param>
		<Extension>
		Public Function IsInDays(this As Date, days As IEnumerable(Of Date)) As Boolean
			If days Is Nothing Then Return False
			Return days?.Any(Function(x) x.Date = this.Date)
		End Function

		''' <summary>指定日期是不是在日期组中</summary>
		''' <param name="this">要操作的时间</param>
		''' <param name="days">日期组</param>
		<Extension>
		Public Function IsInDays(this As Date, ParamArray days As Date()) As Boolean
			If days Is Nothing Then Return False
			Return days.Any(Function(x) x.Date = this.Date)
		End Function

		''' <summary>指定日期是不是在日期组中</summary>
		''' <param name="This">要操作的时间</param>
		''' <param name="Days">日期字符串组，逗号间隔</param>
		<Extension>
		Public Function IsInDays(this As Date, days As String) As Boolean
			Return this.IsInDays(days.ToDateList)
		End Function

		''' <summary>是否一个月的第一天</summary>
		<Extension>
		Public Function IsMonthBegin(this As Date) As Boolean
			Return this.Day = 1
		End Function

		''' <summary>是否一个月的第最后一天</summary>
		<Extension>
		Public Function IsMonthEnd(this As Date) As Boolean
			Return this.Day = New Date(this.Year, this.Month, 1).AddMonths(1).AddDays(-1).Day
		End Function

		''' <summary>是否周一</summary>
		<Extension>
		Public Function IsMonday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Monday
		End Function

		''' <summary>是否周二</summary>
		<Extension>
		Public Function IsTuesday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Tuesday
		End Function

		''' <summary>是否周三</summary>
		<Extension>
		Public Function IsWednesday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Wednesday
		End Function

		''' <summary>是否周四</summary>
		<Extension>
		Public Function IsThursday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Thursday
		End Function

		''' <summary>是否周五</summary>
		<Extension>
		Public Function IsFriday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Friday
		End Function

		''' <summary>是否周六</summary>
		<Extension>
		Public Function IsSaturday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Saturday
		End Function

		''' <summary>是否周日</summary>
		<Extension>
		Public Function IsSunday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Sunday
		End Function

		''' <summary>是不是周末</summary>
		<Extension>
		Public Function IsWeekend(this As Date) As Boolean
			Select Case this.DayOfWeek
				Case DayOfWeek.Saturday, DayOfWeek.Sunday
					Return True
				Case Else
					Return False
			End Select
		End Function

		''' <summary>是不是国家法定假期</summary>
		<Extension>
		Public Function IsHoliday(this As Date) As Boolean
			Return this.IsInDays(DATE_HOLIDAY)
		End Function

		''' <summary>是不是法定假期前一天</summary>
		''' <remarks>今天工作，明天假日</remarks>
		<Extension>
		Public Function IsBeforeHoliday(this As Date) As Boolean
			Return Not IsHoliday(this) AndAlso IsHoliday(this.AddDays(1))
		End Function

		''' <summary>是不是法定假期第一天</summary>
		''' <remarks>昨天工作，今天假日</remarks>
		<Extension>
		Public Function IsFirstHoliday(this As Date) As Boolean
			Return Not IsHoliday(this.AddDays(-1)) AndAlso IsHoliday(this)
		End Function

		''' <summary>是不是法定假期最后一天</summary>
		''' <remarks>今天假日，明天工作</remarks>
		<Extension>
		Public Function IsLastHoliday(this As Date) As Boolean
			Return IsHoliday(this) AndAlso Not IsHoliday(this.AddDays(1))
		End Function

		''' <summary>是不是法定假期后一天</summary>
		''' <remarks>昨天假日，今天工作</remarks>
		<Extension>
		Public Function IsAfterHoliday(this As Date) As Boolean
			Return IsHoliday(this.AddDays(-1)) AndAlso Not IsHoliday(this)
		End Function

		''' <summary>是不是法定调整工作日</summary>
		<Extension>
		Public Function IsAdjustday(this As Date) As Boolean
			Return this.IsInDays(DATE_ADJUST)
		End Function

		''' <summary>是否工作日</summary>
		<Extension>
		Public Function IsWorkday(this As Date) As Boolean
			Return Not IsRestday(this)
		End Function

		''' <summary>是否月第一个工作日</summary>
		<Extension>
		Public Function IsMonthWorkFirst(this As Date) As Boolean
			Return this.MonthWorkFirst.EqualsDay(this)
		End Function

		''' <summary>是否月最后一个工作日</summary>
		<Extension>
		Public Function IsMonthWorkEnd(this As Date) As Boolean
			Return this.MonthWorkEnd.EqualsDay(this)
		End Function

		''' <summary>是否周第一个工作日</summary>
		<Extension>
		Public Function IsWeekWorkFirst(this As Date) As Boolean
			Return this.WeekWorkFirst.EqualsDay(this)
		End Function

		''' <summary>是否周最后一个工作日</summary>
		<Extension>
		Public Function IsWeekWorkEnd(this As Date) As Boolean
			Return this.WeekWorkEnd.EqualsDay(this)
		End Function

		''' <summary>是不是休息日，含周末和国家法定假期</summary>
		<Extension>
		Public Function IsRestday(this As Date) As Boolean
			' 假期直接为休息日
			' 周末且不是调休日
			Return IsHoliday(this) OrElse (IsWeekend(this) And Not IsAdjustday(this))
		End Function

		''' <summary>是不是休息日前一天</summary>
		''' <remarks>今天工作，明天假日</remarks>
		<Extension>
		Public Function IsBeforeRestday(this As Date) As Boolean
			Return Not IsRestday(this) AndAlso IsRestday(this.AddDays(1))
		End Function

		''' <summary>是不是休息日第一天</summary>
		''' <remarks>昨天工作，今天假日</remarks>
		<Extension>
		Public Function IsFirstRestday(d As Date) As Boolean
			Return Not IsRestday(d.AddDays(-1)) AndAlso IsRestday(d)
		End Function

		''' <summary>是不是休息日最后一天</summary>
		''' <remarks>今天假日，明天工作</remarks>
		<Extension>
		Public Function IsLastRestday(this As Date) As Boolean
			Return IsRestday(this) AndAlso Not IsRestday(this.AddDays(1))
		End Function

		''' <summary>是不是休息日后一天</summary>
		''' <remarks>昨天假日，今天工作</remarks>
		<Extension>
		Public Function IsAfterRestday(this As Date) As Boolean
			Return IsRestday(this.AddDays(-1)) AndAlso Not IsRestday(this)
		End Function

		''' <summary>是不是周末和国家法定假期</summary>
		<Extension>
		Public Function IsWeekendHoliday(this As Date) As Boolean
			Return IsWeekend(this) Or IsHoliday(this)
		End Function

		''' <summary>是不是周末和国家法定假期</summary>
		<Extension>
		Public Function IsSundayHoliday(this As Date) As Boolean
			Return IsSunday(this) Or IsHoliday(this)
		End Function

		''' <summary>计算两个时间是否相等，比较到秒，及年月日时分秒都相等</summary>
		<Extension>
		Public Function EqualsSecond(this As Date, target As Date) As Boolean
			Return this.Second = target.Second AndAlso EqualsMinute(this, target)
		End Function

		''' <summary>计算两个时间是否相等，比较到秒，及年月日时分秒都相等</summary>
		<Extension>
		Public Function EqualsMinute(this As Date, target As Date) As Boolean
			Return this.Minute = target.Minute AndAlso EqualsHour(this, target)
		End Function

		''' <summary>计算两个时间是否相等，比较到秒，及年月日时分秒都相等</summary>
		<Extension>
		Public Function EqualsHour(this As Date, target As Date) As Boolean
			Return this.Hour = target.Hour AndAlso EqualsDay(this, target)
		End Function

		''' <summary>计算两个时间是否相等，比较到秒，及年月日时分秒都相等</summary>
		<Extension>
		Public Function EqualsDay(this As Date, target As Date) As Boolean
			Return this.Date = target.Date
		End Function

		''' <summary>是不是工作时段；开始结束相等，一天都工作，开始大于结束，则凌晨到开始，开始到结束休息，结束到凌晨工作</summary>
		''' <param name="this">当前时间</param>
		''' <param name="timeStart">工作开始时间（时分）</param>
		''' <param name="timeStop">工作结束时间（时分）</param>
		''' <remarks>2016-09-25 增加</remarks>
		<Extension>
		Public Function IsTimeWork(this As Date, timeStart As Date, timeStop As Date) As Boolean
			Return this.IsTimeWork(timeStart.Hour, timeStart.Minute, timeStop.Hour, timeStop.Minute)
		End Function

		''' <summary>是不是工作时段；开始结束相等，一天都工作，开始大于结束，则凌晨到开始，开始到结束休息，结束到凌晨工作</summary>
		''' <param name="this">当前时间</param>
		''' <param name="hourStart">开始小时</param>
		''' <param name="minuteStart">开始分钟</param>
		''' <param name="hourStop">结束小时</param>
		''' <param name="minuteStop">结束分钟</param>
		<Extension>
		Public Function IsTimeWork(this As Date, hourStart As Integer, minuteStart As Integer, hourStop As Integer, minuteStop As Integer) As Boolean
			' 如果开始和结束时间都相等表示全天都工作
			' 如果开始时间比结束时间晚则表示工作到第二天了
			Dim ds = this.Date.AddHours(hourStart).AddMinutes(minuteStart)
			Dim de = this.Date.AddHours(hourStop).AddMinutes(minuteStop)

			If ds < de Then
				Return this <= de And this >= ds
			ElseIf ds > de Then
				Return this <= de Or this >= ds
			Else
				Return True
			End If
		End Function

#End Region

#Region "5. 日期获取"

		''' <summary>计算指定一天的凌晨零点</summary>
		<Extension>
		Public Function DayFirst(this As Date) As Date
			Return this.Date
		End Function

		''' <summary>计算指定一天的最后时刻</summary>
		<Extension>
		Public Function DayEnd(this As Date) As Date
			Return this.Date.AddDays(1).AddMilliseconds(-1)
		End Function

		''' <summary>计算指定日期所在的周一</summary>
		<Extension>
		Public Function WeekFirst(this As Date) As Date
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7

			Return this.Date.AddDays(1 - w)
		End Function

		''' <summary>计算指定日期所在的周日</summary>
		<Extension>
		Public Function WeekEnd(this As Date) As Date
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7

			Return this.Date.AddDays(8 - w).AddMilliseconds(-1)
		End Function

		''' <summary>计算指定日期的月初时刻</summary>
		<Extension>
		Public Function MonthFirst(this As Date) As Date
			Return New Date(this.Year, this.Month, 1)
		End Function

		''' <summary>计算指定日期的月末时刻</summary>
		<Extension>
		Public Function MonthEnd(this As Date) As Date
			Dim value = this.AddMonths(1)
			Return New Date(value.Year, value.Month, 1).AddMilliseconds(-1)
		End Function

		''' <summary>计算指定日期的年初时刻</summary>
		<Extension>
		Public Function YearFirst(this As Date) As Date
			Return New Date(this.Year, 1, 1)
		End Function

		''' <summary>计算指定年末时刻</summary>
		<Extension>
		Public Function YearEnd(this As Date) As Date
			Return New Date(this.Year + 1, 1, 1).AddMilliseconds(-1)
		End Function

		'------------------------------------------------------------------------------------------

		''' <summary>一周第一个工作日</summary>
		<Extension>
		Public Function WeekWorkFirst(this As Date) As Date
			Dim ret As Date = Nothing

			' 获取周一
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7
			Dim day = this.Date.AddDays(1 - w)

			For i = 1 To 7
				If day.IsWorkday Then
					ret = day
					Exit For
				End If

				day = day.AddDays(1)
			Next


			Return ret
		End Function

		''' <summary>计算指定日期所在的周日</summary>
		<Extension>
		Public Function WeekWorkEnd(this As Date) As Date
			Dim ret As Date = Nothing

			' 获取周日
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7
			Dim day = this.Date.AddDays(7 - w)

			For i = 1 To 7
				If day.IsWorkday Then
					ret = day
					Exit For
				End If

				day = day.AddDays(-1)
			Next

			Return ret
		End Function

		''' <summary>计算指定日期的月初时刻</summary>
		<Extension>
		Public Function MonthWorkFirst(this As Date) As Date
			Dim ret As Date = Nothing

			' 计算周期
			Dim day = New Date(this.Year, this.Month, 1)
			Dim len = day.AddMonths(1).AddSeconds(-1).Day

			For i = 1 To len
				If day.IsWorkday Then
					ret = day
					Exit For
				End If

				day = day.AddDays(1)
			Next

			Return ret
		End Function

		''' <summary>计算指定日期的月末时刻</summary>
		<Extension>
		Public Function MonthWorkEnd(this As Date) As Date
			Dim ret As Date = Nothing

			' 计算周期
			Dim day = New Date(this.Year, this.Month, 1).AddMonths(1).AddSeconds(-1).Date
			Dim len = day.Day

			For i = 1 To len
				If day.IsWorkday Then
					ret = day
					Exit For
				End If

				day = day.AddDays(-1)
			Next

			Return ret
		End Function

		''' <summary>计算指定日期的年初时刻</summary>
		<Extension>
		Public Function YearWorkFirst(this As Date) As Date
			Dim ret As Date = Nothing

			' 计算周期
			Dim day = New Date(this.Year, 1, 1)
			Dim len As Integer = day.AddYears(1).Subtract(day).TotalDays

			For i = 1 To len
				If day.IsWorkday Then
					ret = day
					Exit For
				End If

				day = day.AddDays(1)
			Next

			Return ret
		End Function

		''' <summary>计算指定年末时刻</summary>
		<Extension>
		Public Function YearWorkEnd(this As Date) As Date
			Dim ret As Date = Nothing

			' 计算周期
			Dim day = New Date(this.Year + 1, 1, 1).AddSeconds(-1).Date
			Dim len As Integer = day.Subtract(New Date(this.Year, 1, 1)).TotalDays

			For i = 1 To len
				If day.IsWorkday Then
					ret = day
					Exit For
				End If

				day = day.AddDays(-1)
			Next

			Return ret
		End Function

#End Region

	End Module
End Namespace