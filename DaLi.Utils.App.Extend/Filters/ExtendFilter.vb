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
' 	框架授权过滤器
'
' 	name: Filter.ExtendFilter
' 	create: 2023-02-24
' 	memo: 框架授权过滤器(控制器请求赋值)
' 	
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Mvc.Filters

Namespace Filter

	''' <summary>框架授权过滤器</summary>
	<FilterInfo(Integer.MinValue + 100, True)>
	Public Class ExtendFilter
		Inherits ActionFilterAttribute

		''' <summary>公共请求上下文对象</summary>
		Protected ReadOnly ControllerContext As IAppContext

		Public Sub New(controllerContext As IAppContext)
			Me.ControllerContext = controllerContext
		End Sub

		Public Overrides Sub OnActionExecuting(context As ActionExecutingContext)
			'-----------------------
			' 赋值请求对象
			'-----------------------
			Dim ctrl = TryCast(context.Controller, CtrBase)

			' 将上下文赋值到 Http 请求中
			If ctrl IsNot Nothing Then
				ctrl.HttpContext.ContextItem(VAR_CONTROLLER_CONTEXT, ControllerContext)
				ctrl.AppContext = ControllerContext
			Else
				context.HttpContext.ContextItem(VAR_CONTROLLER_CONTEXT, ControllerContext)
			End If

			MyBase.OnActionExecuting(context)
		End Sub

		Public Overrides Sub OnResultExecuted(context As ResultExecutedContext)
			' 如果验证 ErrorMessage 存在异常，将此异常数据反馈到 ResultExecutedContext 中，以便日志记录
			If context.Exception Is Nothing Then
				Dim ctrl = TryCast(context.Controller, CtrBase)

				' 将上下文赋值到 Http 请求中
				If ctrl IsNot Nothing Then
					If ctrl.ErrorMessage.Exception IsNot Nothing Then
						context.Exception = ctrl.ErrorMessage.Exception
						context.ExceptionHandled = True
					End If
				End If
			End If

			MyBase.OnResultExecuted(context)
		End Sub
	End Class
End Namespace
