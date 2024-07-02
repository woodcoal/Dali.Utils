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
' 	日志参数
'
' 	name: Setting.LogSetting
' 	create: 2023-02-14
' 	memo: 日志参数
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports Microsoft.AspNetCore.Mvc.Filters
Imports Microsoft.Extensions.Logging
Imports Serilog
Imports Serilog.Events
Imports Serilog.Sinks.SystemConsole.Themes

Namespace Setting

	''' <summary>日志参数</summary>
	Public Class LogSetting
		Inherits LocalSettingBase(Of LogSetting)

		''' <summary>系统内部操作日志是否记录到文件（0：Trace； 1：Debug； 2：Information； 3：Warning； 4：Error； 5：Critical； 6：None）</summary>
		<Range(0, 6)>
		Public Property File As LogLevel = LogLevel.Error

		''' <summary>日志文件存储路径</summary>
		<FieldChange(FieldTypeEnum.FOLDER)>
		Public Property FileFolder As String = ".log"

		''' <summary>文件日志保存最长时间，超过此时间的日志将被删除</summary>
		<Range(1, 999999)>
		Public Property FileDays As Integer = 30

		''' <summary>系统内部操作日志是否输出到控制台（0：Trace； 1：Debug； 2：Information； 3：Warning； 4：Error； 5：Critical； 6：None）</summary>
		<Range(0, 6)>
		Public Property Console As LogLevel = LogLevel.Error

		''' <summary>系统内部操作日志是否输出到服务器（0：Trace； 1：Debug； 2：Information； 3：Warning； 4：Error； 5：Critical； 6：None）</summary>
		<Range(0, 6)>
		Public Property Server As LogLevel = LogLevel.Error

		''' <summary>日志服务器</summary>
		<FieldType(FieldValidateEnum.URL)>
		Public Property Server_Url As String

		''' <summary>日志服务 Token</summary>
		Public Property Server_Token As String

		''' <summary>是否记录 api 访问信息，用于记录接口。（0：不记录，1：记录有所，2：仅仅记录访问成功，3：仅仅记录非成功访问，4：仅仅 4xx 错误，5：仅仅记录 5xx 错误）;如果需要记录 HTTP 请求信息，请注意级别：http 请求异常及 5xx，错误返回 Error / 黑名单：Critical / 4xx 错误，访问时长超过 10s 返回 Warning / 其他 Information</summary>
		Public Property Http As Integer = 0

#Region "Http 日志记录事件"

		''' <summary>创建日志对象</summary>
		Public Function CreateLogger() As Serilog.ILogger
			' 允许组件错误调试
			Debugging.SelfLog.Enable(System.Console.Error)

			' 初始化日志对象
			Dim cfg = New LoggerConfiguration().
				Enrich.WithProperty("App", New Dictionary(Of String, Object) From {{"ID", SYS.ID}, {"Name", SYS.Name}, {"Version", SYS.Version}}).   ' 添加应用标识
				Enrich.FromLogContext().                                                ' 记录相关上下文信息
				MinimumLevel.Verbose.                                                   ' 默认最小记录级别
				MinimumLevel.Override("Microsoft", LogEventLevel.Warning).              ' 对其他日志进行重写，目前框架只有微软自带的日志组件
				MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)    ' 对其他日志进行重写，目前框架只有微软自带的日志组件

			' 输出到服务器
			If Server <> LogLevel.None AndAlso Server_Url.IsUrl Then
				' 检查 Server_Url
				If NetHelper.Exists(Server_Url) Then
					If Server_Token.NotEmpty Then
						cfg.WriteTo.Seq(Server_Url, apiKey:=Server_Token, restrictedToMinimumLevel:=Server)
					Else
						cfg.WriteTo.Seq(Server_Url, restrictedToMinimumLevel:=Server)
					End If
				Else
					CON.Err($"日志服务器 {Server_Url} 无法正常访问，服务器端日志已暂停！")
				End If
			End If

			' 输出到控制台
			If Console <> LogLevel.None Then cfg.WriteTo.Console(theme:=SystemConsoleTheme.Colored, restrictedToMinimumLevel:=Console)

			' 输出到文件
			If File <> LogLevel.None Then
				Dim path = PathHelper.Root(FileFolder, True, True)
				If Not IO.Directory.Exists(path) Then IO.Directory.CreateDirectory(path)

				cfg.WriteTo.Logger(Sub(lg) lg.
									   Filter.ByIncludingOnly(Function(x) x.Level >= LogEventLevel.Error).
									   WriteTo.File(IO.Path.Combine(path, ".err"), rollingInterval:=RollingInterval.Day, restrictedToMinimumLevel:=File, retainedFileTimeLimit:=New TimeSpan(FileDays, 0, 0, 0), [shared]:=True))
				cfg.WriteTo.Logger(Sub(lg) lg.
									   Filter.ByIncludingOnly(Function(x) x.Level < LogEventLevel.Error).
									   WriteTo.File(IO.Path.Combine(path, ".log"), rollingInterval:=RollingInterval.Day, restrictedToMinimumLevel:=File, retainedFileTimeLimit:=New TimeSpan(FileDays, 0, 0, 0), [shared]:=True))
			End If

			Return cfg.CreateLogger()
		End Function

		' 日志操作事件列表
		Private Shared ReadOnly _Actions As New List(Of Action(Of ResultExecutedContext, KeyValueDictionary))

		' 日志操作，如需对 api 请求日志进一步处理，可以定义此函数
		Friend Shared Sub RequestLogProcess(content As ResultExecutedContext, result As KeyValueDictionary)
			_Actions.ForEach(Sub(act) act.Invoke(content, result))
		End Sub

		' 注册日志操作事件，如需对 api 请求日志进一步处理，可以定义此函数
		Public Shared Sub RegisterReuestlogProcess(process As Action(Of ResultExecutedContext, KeyValueDictionary))
			If process IsNot Nothing AndAlso Not _Actions.Contains(process) Then _Actions.Add(process)
		End Sub

#End Region

	End Class

End Namespace