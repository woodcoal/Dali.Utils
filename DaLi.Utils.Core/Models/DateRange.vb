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
' 	时间区域
'
' 	name: Model.DateRange
' 	create: 2021-05-10
' 	memo: 时间区域
'
' ------------------------------------------------------------

Imports System.Text.Json.Serialization

Namespace Model

	''' <summary>时间区域</summary>
	Public Class DateRange

		''' <summary>开始时间对象</summary>
		<JsonIgnore>
		Public Start As Date?

		''' <summary>结束时间对象</summary>
		<JsonIgnore>
		Public [End] As Date?

		''' <summary>开始时间字符串</summary>
		Public Property DateStart As String
			Get
				Return Start?.ToString("yyyy-MM-dd HH:mm:ss")
			End Get
			Set(value As String)
				Start = If(value.IsEmpty, Nothing, value.ToDateTime.Date)
				If Start IsNot Nothing AndAlso Start = New Date Then Start = Nothing
			End Set
		End Property

		''' <summary>结束时间字符串</summary>
		Public Property DateEnd As String
			Get
				Return [End]?.ToString("yyyy-MM-dd HH:mm:ss")
			End Get
			Set(value As String)
				[End] = If(value.IsEmpty, Nothing, value.ToDateTime.Date.AddDays(1).AddMilliseconds(-1))
				If [End] IsNot Nothing AndAlso [End] = New Date Then [End] = Nothing
			End Set
		End Property

	End Class
End Namespace
