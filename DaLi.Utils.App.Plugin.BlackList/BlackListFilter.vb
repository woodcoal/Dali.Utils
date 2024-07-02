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
' 	黑名单过滤器
'
' 	name: Filter.BlackListFilter
' 	create: 2023-02-14
' 	memo: 黑名单过滤器
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Extension
Imports DaLi.Utils.App.Plugin.Setting
Imports Microsoft.AspNetCore.Mvc.Filters
Imports Microsoft.Extensions.Logging

Namespace Filter

	''' <summary>框架基础过滤器</summary>
	<FilterInfo(Integer.MinValue + 1, True)>
	Public Class BlackListFilter
		Inherits ActionFilterAttribute

		Private Const BLACK_IP = "[BLACK#IP]"

		''' <summary>日志组件</summary>
		Protected ReadOnly LOG As ILogger(Of BlackListFilter)

		Public Sub New(log As ILogger(Of BlackListFilter))
			Me.LOG = log
		End Sub

		Public Overrides Sub OnActionExecuting(context As ActionExecutingContext)
			' 检查是否黑名单
			Dim result = SYS.GetSetting(Of BlackListSetting).Check(context.HttpContext, LOG)
			If result IsNot Nothing Then
				' 存在黑名单
				context.Result = result
				context.HttpContext.ContextItem(BLACK_IP, True)
			Else
				MyBase.OnActionExecuting(context)
			End If
		End Sub

		''' <summary>结果返回，记录日志</summary>
		Public Overrides Sub OnResultExecuted(context As ResultExecutedContext)
			' 是否黑名单
			Dim IsBlack = context.HttpContext.ContextItem(Of Boolean)(BLACK_IP)

			' 非白名单继续执行，否则直接返回
			If Not IsBlack Then MyBase.OnResultExecuted(context)
		End Sub

	End Class
End Namespace
