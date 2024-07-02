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
' 	日期验证
'
' 	name: Auto.Rule.ValidateDate
' 	create: 2023-01-03
' 	memo: 日期验证
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>日期验证</summary>
	Public Class ValidateDate
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "日期验证"
			End Get
		End Property

		''' <summary>原始日期数据，不设置为当前时间</summary>
		Public Property Source As String

		''' <summary>需要验证的时间参数</summary>
		Public Property Dates As DateNameEnum()

		''' <summary>是否所有日期都需要匹配</summary>
		Public Property ValidateALL As Boolean

		''' <summary>触发异常。True：匹配成功异常； False：匹配失败异常； Default：忽略，成功或失败都不触发</summary>
		Public Property ThrowValue As TristateEnum

		''' <summary>为匹配成功时输出的内容</summary>
		Public Property SuccValue As String

		''' <summary>为匹配失败时输出的内容</summary>
		Public Property FailValue As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "验证日期参数未设置"
			If Dates.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 用于比较的时间
			Dim dateNow = SYS_NOW_DATE
			If Source.NotEmpty Then dateNow = AutoHelper.GetVarString(Source, data).ToDateTime

			message.Message = "用于验证的日期无效"
			If Not dateNow.IsValidate Then Return False

			' 分析
			Dim Ret = False

			If ValidateALL Then
				' 全部需要满足，任意一条不满足退出
				For Each dateName In Dates
					Ret = IsValidate(dateNow, dateName)
					If Not Ret Then Exit For
				Next
			Else
				' 任意一条满足即可
				For Each dateName In Dates
					Ret = IsValidate(dateNow, dateName)
					If Ret Then Exit For
				Next
			End If

			Select Case ThrowValue
				Case TristateEnum.TRUE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("禁止与指定时间匹配").FormatTemplate(data, True)
					If Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

				Case TristateEnum.FALSE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("与指定时间不匹配").FormatTemplate(data, True)
					If Not Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

			End Select

			message.SetSuccess()

			If Ret Then
				Return If(SuccValue.NotEmpty, AutoHelper.GetVar(SuccValue, data), True)
			Else
				Return If(FailValue.NotEmpty, AutoHelper.GetVar(FailValue, data), False)
			End If
		End Function

		''' <summary>是否包含</summary>
		''' <param name="dateNow">检测的时间</param>
		''' <param name="dateName">日期名称</param>
		Private Shared Function IsValidate(dateNow As Date, dateName As DateNameEnum) As Boolean
			Select Case dateName
				Case DateNameEnum.ALL
					Return True

				Case DateNameEnum.MONDAY
					Return dateNow.IsMonday

				Case DateNameEnum.TUESDAY
					Return dateNow.IsTuesday

				Case DateNameEnum.WEDNESDAY
					Return dateNow.IsWednesday

				Case DateNameEnum.THURSDAY
					Return dateNow.IsThursday

				Case DateNameEnum.FRIDAY
					Return dateNow.IsFriday

				Case DateNameEnum.SATURDAY
					Return dateNow.IsSaturday

				Case DateNameEnum.SUNDAY
					Return dateNow.IsSunday

				Case DateNameEnum.MONTH_FIRST
					Return dateNow.IsMonthBegin

				Case DateNameEnum.MONTH_LAST
					Return dateNow.IsMonthEnd

				Case DateNameEnum.WORKDAY
					Return dateNow.IsWorkday

				Case DateNameEnum.ADJUSTDAY
					Return dateNow.IsAdjustday

				Case DateNameEnum.HOLIDAY
					Return dateNow.IsHoliday

				Case DateNameEnum.HOLIDAY_BEFORE
					Return dateNow.IsBeforeHoliday

				Case DateNameEnum.HOLIDAY_FIRST
					Return dateNow.IsFirstHoliday

				Case DateNameEnum.HOLIDAY_LAST
					Return dateNow.IsLastHoliday

				Case DateNameEnum.HOLIDAY_AFTER
					Return dateNow.IsAfterHoliday

				Case DateNameEnum.RESTDAY
					Return dateNow.IsRestday

				Case DateNameEnum.RESTDAY_BEFORE
					Return dateNow.IsBeforeRestday

				Case DateNameEnum.RESTDAY_FIRST
					Return dateNow.IsFirstRestday

				Case DateNameEnum.RESTDAY_LAST
					Return dateNow.IsLastRestday

				Case DateNameEnum.RESTDAY_AFTER
					Return dateNow.IsAfterRestday

				Case DateNameEnum.YESTERDAY
					Return dateNow.EqualsDay(SYS_NOW_DATE.AddDays(-1))

				Case DateNameEnum.TODAY
					Return dateNow.EqualsDay(SYS_NOW_DATE)

				Case DateNameEnum.TOMORROW
					Return dateNow.EqualsDay(SYS_NOW_DATE.AddDays(1))

				Case Else
					Return False
			End Select
		End Function

#End Region

	End Class
End Namespace