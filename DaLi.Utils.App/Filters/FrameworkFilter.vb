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
' 	框架基础过滤器
'
' 	name: Filter.FrameworkFilter
' 	create: 2023-02-14
' 	memo: 框架基础过滤器（黑名单，日志操作）
' 	
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.Controllers
Imports Microsoft.AspNetCore.Mvc.Filters
Imports Microsoft.Extensions.Logging
Imports Serilog
Imports Serilog.Events

Namespace Filter

	''' <summary>框架基础过滤器</summary>
	<FilterInfo(Integer.MinValue, True)>
	Public Class FrameworkFilter
		Inherits ActionFilterAttribute

		Private Const START_TIME = "[START#TIME]"
		Private Const LOG_ENABLED = "[LOG#ENABLED]"
		Private Const LOG_RESULT = "[LOG#RESULT]"

		''' <summary>日志组件</summary>
		Protected ReadOnly LOG As ILogger(Of FrameworkFilter)

		Public Sub New(log As ILogger(Of FrameworkFilter))
			Me.LOG = log
		End Sub

		Public Overrides Sub OnActionExecuting(context As ActionExecutingContext)
			' 附加启动时间
			context.HttpContext.ContextItem(START_TIME, SYS_NOW)

			' 检查运行环境
			Dim envAttr = ControllerHelper.GetAttribute(Of EnvAttribute)(context.ActionDescriptor)
			Dim envPass = envAttr Is Nothing OrElse envAttr.IsRuntime(SYS.Debug)

			' 未通过环境检测
			If Not envPass Then
				Dim message = If(SYS.Debug, "调试模式下不能执行正式环境 API"， "正式环境不能执行调试 API")
				context.Result = ResponseJson.Err(400, message, context.HttpContext)
				Exit Sub
			End If

			MyBase.OnActionExecuting(context)
		End Sub

		''' <summary>结果返回，记录日志</summary>
		Public Overrides Sub OnResultExecuted(context As ResultExecutedContext)
			' 输出
			Dim http = context.HttpContext
			Dim res = http.Response

			' 是否记录日志
			Dim enLog = False

			' 系统参数设置是否允许记录日志
			Select Case SYS.GetSetting(Of LogSetting).Http
				Case 1
					' 1 全部类型都记录
					enLog = True

				Case 2
					' 2 仅记录成功事件
					enLog = res.StatusCode < 300

				Case 3
					' 3 仅记录失败事件
					enLog = res.StatusCode >= 300

				Case 4
					' 4 仅记录 4xx 事件
					enLog = res.StatusCode >= 400 AndAlso res.StatusCode < 500

				Case 5
					' 5 仅记录 5xx 事件
					enLog = res.StatusCode >= 500 AndAlso res.StatusCode < 600

			End Select

			' 继续检查控制器是否明确禁止记录
			If enLog Then
				Dim actionDescriptor = TryCast(context.ActionDescriptor, ControllerActionDescriptor)
				If actionDescriptor IsNot Nothing Then
					Dim hasNolog = actionDescriptor.MethodInfo.IsDefined(GetType(NoLogAttribute), False)
					If Not hasNolog Then hasNolog = actionDescriptor.ControllerTypeInfo.IsDefined(GetType(NoLogAttribute), False)

					enLog = Not hasNolog
				End If
			End If

			' 标记日志状态
			context.HttpContext.ContextItem(LOG_ENABLED, enLog)

			' 记录日志信息
			If enLog Then
				Dim Result As New KeyValueDictionary

				' 请求结果 
				If context.Result IsNot Nothing Then
					Dim dic = context.Result.ToJson.ToJsonDictionary
					If dic IsNot Nothing Then
						' 如果存在 value 则只返回 value
						If dic.ContainsKey("value") Then dic = dic("value")

						Result.Add("Result", dic)
					End If
				End If

				' 请求参数
				Result.Add("Params", http.RequestData.ToJson.ToJsonDictionary)

				' IP
				Result.Add("IP", http.IP)

				' Request
				Result.Add("Request", http.Request.Headers.HiddenPassword.ToJson.ToJsonDictionary)

				' Response
				Result.Add("Response", http.Response.Headers.HiddenPassword.ToJson.ToJsonDictionary)

				' 控制器信息
				Dim action = TryCast(context.ActionDescriptor, ControllerActionDescriptor)
				If action IsNot Nothing Then Result.Add("Controller", New Dictionary(Of String, String) From {{"Controller", action.ControllerName}, {"Action", action.ActionName}})

				' 日志进一步处理
				LogSetting.RequestLogProcess(context, Result)

				' 记录日志信息
				http.ContextItem(LOG_RESULT, Result)
			End If

			MyBase.OnResultExecuted(context)
		End Sub

#Region "生成 Serilog 日志数据"

		''' <summary>分析请求返回日志级别</summary>
		''' <param name="http">http 上下文</param>
		''' <param name="time">请求时长</param>
		''' <param name="ex">异常数据</param>
		Friend Shared Function SerilogLevel(http As HttpContext, time As Double, ex As Exception) As LogEventLevel
			' 是否记录，将记录调整至最低，以便高于此值的不再记录
			Dim enLog = http.ContextItem(Of Boolean)(LOG_ENABLED)
			If Not enLog Then Return LogEventLevel.Verbose

			' 出现异常，返回 Error 级别
			If ex IsNot Nothing OrElse http.Response.StatusCode > 499 Then Return LogEventLevel.Error

			' 超过 5 秒响应，返回 Warning 级别
			If time > 5000 OrElse http.Response.StatusCode > 399 Then Return LogEventLevel.Warning

			' 默认返回 Information 级别
			Return LogEventLevel.Information
		End Function

		''' <summary>获取日志内容</summary>
		''' <param name="content">Serilog 内容</param>
		''' <param name="http">http 上下文</param>
		Public Shared Sub SerilogContent(content As IDiagnosticContext, http As HttpContext)
			' 是否记录
			Dim enLog = http.ContextItem(Of Boolean)(LOG_ENABLED)
			If Not enLog Then Return

			' 日志信息
			Dim Messages = http.ContextItem(Of Dictionary(Of String, Object))(LOG_RESULT)
			If Messages.NotEmpty Then
				For Each item In Messages
					content.Set(item.Key, item.Value)
				Next
			End If
		End Sub

		'''' <summary>移除数据中加密信息</summary>
		'Public Shared Function UpdateHeaders(Of T)(data As IDictionary(Of String, T)) As Dictionary(Of String, String)
		'	If data.IsEmpty Then Return Nothing

		'	Dim ret As New Dictionary(Of String, String)
		'	For Each item In data
		'		If item.Key.Like("authorization") Then
		'			ret.Add(item.Key, item.Value.ToString.ShortShow(50))

		'		ElseIf item.Key.Like("*token*") Then
		'			ret.Add(item.Key, item.Value.ToString.ShortShow(30))

		'		ElseIf item.Key.Like("*password*") Then
		'			ret.Add(item.Key, item.Value.ToString.ShortShow(6))

		'		Else
		'			ret.Add(item.Key, item.Value.ToString)
		'		End If
		'	Next

		'	Return ret
		'End Function

#End Region

	End Class
End Namespace
