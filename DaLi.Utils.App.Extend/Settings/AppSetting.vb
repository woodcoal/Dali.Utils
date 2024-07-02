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
' 	全局数据库通用参数
'
' 	name: Setting.App
' 	create: 2023-02-18
' 	memo: 全局数据库通用参数
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Setting

	''' <summary>全局数据库通用参数</summary>
	Public Class AppSetting
		Inherits DbSettingBase(Of AppSetting)

		''' <summary>法定假期日期表</summary>
		<Description("法定假期日期表，JSON 日期数组（2000年到今后10年内数据）")>
		Public Property Holidays As Date()

		''' <summary>法定调休日期表</summary>
		<Description("法定调休日期表，JSON 日期数组（2000年到今后10年内数据）")>
		Public Property Adjustdays As Date()

		Protected Overrides Sub Initialize(provider As ISettingProvider)
			Dim dateRange = Function(dates As Date()) As Date()
								If dates.IsEmpty Then Return Nothing

								Dim max = Date.Now.Year + 10
								Return dates.
									Where(Function(x) x.Year >= 2000 AndAlso x.Year <= max).
									Select(Function(x) x.Date).
									Distinct.
									ToArray
							End Function

			DATE_HOLIDAY = dateRange(Holidays)
			DATE_ADJUST = dateRange(Adjustdays)
		End Sub
	End Class

End Namespace

