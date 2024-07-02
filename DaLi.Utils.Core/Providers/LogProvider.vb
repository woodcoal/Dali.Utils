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
' 	日志操作
'
' 	name: Provider.LogProvider
' 	create: 2019-03-31
' 	memo: 日志操作，日志记录到文件
' 	
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Provider

	''' <summary>日志操作</summary>
	Public Class LogProvider
		Implements IDisposable

		''' <summary>日志存储选项</summary>
		Public Class LogOption

			''' <summary>日志保存目录</summary>
			Private _Folder As String

			''' <summary>日志保存目录</summary>
			Public Property Folder As String
				Get
					Return _Folder
				End Get
				Set(value As String)
					If value.IsEmpty Then
						_Folder = ""
					Else
						_Folder = PathHelper.Root(value, True, True)
					End If
				End Set
			End Property

			''' <summary>文件保存格式</summary>
			Private _FileFormat As String

			''' <summary>文件保存格式</summary>
			Public Property FileFormat As String
				Get
					If _FileFormat.IsEmpty Then _FileFormat = "[YYYY]-[MM]-[DD].log"
					Return _FileFormat
				End Get
				Set(value As String)
					_FileFormat = value
				End Set
			End Property

			''' <summary>自动保存间隔时间（秒）</summary>
			Private _SaveInterval As Integer

			''' <summary>自动保存间隔时间（秒）</summary>
			Public Property SaveInterval As Integer
				Get
					Return _SaveInterval
				End Get
				Set(value As Integer)
					_SaveInterval = value.Range(1, 3600)
				End Set
			End Property

			''' <summary>保存文件时按类型分目录保存（True：分目录；False：保存到同一文件）</summary>
			Public SaveByLevel As Boolean = False

			''' <summary>最后操作时间</summary>
			Public Last As Date

			''' <summary>是否工作中</summary>
			Public IsBusy As Boolean = False

			''' <summary>是否启用文件保存</summary>
			Public Enabled As Boolean = True
		End Class

		'Trace = 0			DEBUG		包含最详细消息的日志。 这些消息可能包含敏感应用程序数据。 这些消息默认情况下处于禁用状态，并且绝不应在生产环境中启用。
		'Debug = 1			DEBUG		在开发过程中用于交互式调查的日志。 这些日志应主要包含对调试有用的信息，并且没有长期价值。
		'Information = 2	INFO		跟踪应用程序的常规流的日志。 这些日志应具有长期价值。
		'Warning = 3		ERROR		突出显示应用程序流中的异常或意外事件（不会导致应用程序执行停止）的日志。
		'[Error] = 4		ERROR		当前执行流因故障而停止时突出显示的日志。 这些日志指示当前活动中的故障，而不是应用程序范围内的故障。
		'Critical = 5		ERROR		描述不可恢复的应用程序/系统崩溃或需要立即引起注意的灾难性故障的日志。
		'None = 6						不用于写入日志消息。 指定日志记录类别不应写入任何消息。

		''' <summary>日志保存类型</summary>
		Public Level As LogLevelEnum = LogLevelEnum.ERROR

		''' <summary>日志记录事件</summary>
		Public Event Record(level As LogLevelEnum, message As String)

		''' <summary>选项</summary>
		Public Shared ReadOnly Property Options As New LogOption

		''' <summary>日志记录</summary>
		Protected Shared ReadOnly Instance As New ConcurrentQueue(Of (LogLevelEnum, String, Date))

		''' <summary>构造，如果保存目录为空在则不进行日志记录，如果需要记录到外部如数据库，请处理 Record 事件</summary>
		''' <param name="level">保存类型</param>
		Public Sub New(Optional level As LogLevelEnum = LogLevelEnum.ERROR)
			Me.Level = level
		End Sub

#Region "记录"

		''' <summary>是否允许记录日志</summary>
		Public Function IsEnabled(logLevel As LogLevelEnum) As Boolean
			Return Level <= logLevel AndAlso logLevel <> LogLevelEnum.NONE
		End Function

		''' <summary>记录日志</summary>
		Public Sub Log(level As LogLevelEnum, exception As Exception, message As String, ParamArray args As Object())
			If Not IsEnabled(level) Then Return

			If exception IsNot Nothing Then
				With New Text.StringBuilder
					' 如果存在异常，则通过异常获取 StackFrame
					Dim st As New StackTrace(exception, True)
					Dim frs = st?.GetFrames
					Dim sf = frs?(frs.Length - 1)

					If sf IsNot Nothing Then
						.AppendFormat("类名：{0}{1}", sf.GetMethod().DeclaringType.FullName, vbCrLf)
						.AppendFormat("方法：{0}{1}", sf.GetMethod().Name, vbCrLf)
						.AppendFormat("行号：{0}{1}", sf.GetFileLineNumber, vbCrLf)
					End If

					.AppendFormat("异常：{0}", exception.ToString)

					message &= vbCrLf & .ToString
				End With
			End If

			Log(level, message, args)
		End Sub

		''' <summary>日志记录</summary>
		Public Sub Log(level As LogLevelEnum, message As String, ParamArray args As Object())
			If Not IsEnabled(level) Then Return

			Try
				message = String.Format(message, args)
			Catch
			End Try

			' 允许文件保存
			If Options.Enabled Then
				Instance.Enqueue((level, message, SYS_NOW_DATE))
				Call SaveAsync()
			End If

			' 触发记录事件
			RaiseEvent Record(level, message)
		End Sub

		''' <summary>详细消息的日志</summary>
		''' <param name="message">包含最详细消息的日志。 这些消息可能包含敏感应用程序数据。 这些消息默认情况下处于禁用状态，并且绝不应在生产环境中启用。</param>
		Public Sub Trace(message As String, ParamArray args As Object())
			Log(LogLevelEnum.TRACE, message, args)
		End Sub

		''' <summary>在开发过程中用于交互式调查的日志</summary>
		''' <param name="message">在开发过程中用于交互式调查的日志。 这些日志应主要包含对调试有用的信息，并且没有长期价值。</param>
		Public Sub Debug(message As String, ParamArray args As Object())
			Log(LogLevelEnum.DEBUG, message, args)
		End Sub

		''' <summary>跟踪应用程序的常规流的日志</summary>
		''' <param name="message">跟踪应用程序的常规流的日志。 这些日志应具有长期价值。</param>
		Public Sub Information(message As String, ParamArray args As Object())
			Log(LogLevelEnum.INFORMATION, message, args)
		End Sub

		''' <summary>突出显示应用程序流中的异常或意外事件</summary>
		''' <param name="message">突出显示应用程序流中的异常或意外事件（不会导致应用程序执行停止）的日志。</param>
		Public Sub Warning(message As String, ParamArray args As Object())
			Log(LogLevelEnum.WARNING, message, args)
		End Sub

		''' <summary>当前执行流因故障而停止时突出显示的日志</summary>
		''' <param name="message">当前执行流因故障而停止时突出显示的日志。 这些日志指示当前活动中的故障，而不是应用程序范围内的故障。</param>
		Public Sub [Error](message As String, ParamArray args As Object())
			Log(LogLevelEnum.ERROR, message, args)
		End Sub

		''' <summary>当前执行流因故障而停止时突出显示的日志</summary>
		''' <param name="message">当前执行流因故障而停止时突出显示的日志。 这些日志指示当前活动中的故障，而不是应用程序范围内的故障。</param>
		Public Sub [Error](exception As Exception, message As String, ParamArray args As Object())
			Log(LogLevelEnum.ERROR, exception, message, args)
		End Sub

		''' <summary>系统崩溃或需要立即引起注意的灾难性故障的日志</summary>
		''' <param name="message">描述不可恢复的应用程序/系统崩溃或需要立即引起注意的灾难性故障的日志。</param>
		Public Sub Critical(message As String, ParamArray args As Object())
			Log(LogLevelEnum.CRITICAL, message, args)
		End Sub

		''' <summary>系统崩溃或需要立即引起注意的灾难性故障的日志</summary>
		''' <param name="message">描述不可恢复的应用程序/系统崩溃或需要立即引起注意的灾难性故障的日志。</param>
		Public Sub Critical(exception As Exception, message As String, ParamArray args As Object())
			Log(LogLevelEnum.CRITICAL, exception, message, args)
		End Sub

#End Region

#Region "保存"

		''' <summary>保存</summary>
		''' <param name="saveNow">是否立即保存</param>
		Private Shared Async Function SaveAsync(Optional saveNow As Boolean = False) As Task
			' 文件夹未设置或者没有数据不保存
			If Not Options.Enabled OrElse Options.IsBusy OrElse Options.Folder.IsEmpty OrElse Instance.IsEmpty Then Return

			' 还未到保存时间且非立即保存则直接退出
			If Not saveNow AndAlso Options.Last.AddSeconds(Options.SaveInterval) > Date.Now Then Return

			Await Task.Run(Sub()
							   Options.IsBusy = True
							   Options.Last = Date.Now

							   If Options.SaveByLevel Then
								   ' 按类型分类保存
								   Dim strDebug As New Text.StringBuilder
								   Dim strInfo As New Text.StringBuilder
								   Dim strError As New Text.StringBuilder

								   Dim log As (level As LogLevelEnum, message As String, update As Date) = Nothing
								   While Instance?.Count > 0 AndAlso Instance.TryDequeue(log)
									   'Trace = 0		DEBUG
									   'Debug = 1		DEBUG
									   'Information = 2	INFO
									   'Warning = 3		ERROR
									   '[Error] = 4		ERROR
									   'Critical = 5	ERROR

									   Select Case log.level
										   Case LogLevelEnum.TRACE, LogLevelEnum.DEBUG
											   strDebug.Append($"[{log.level.Description}]{vbTab}")
											   strDebug.AppendLine($"{log.update:yyyy-MM-dd HH:mm:ss}{vbTab}{log.message}{vbCrLf}")

										   Case LogLevelEnum.WARNING, LogLevelEnum.ERROR, LogLevelEnum.CRITICAL
											   strError.Append($"[{log.level.Description}]{vbTab}")
											   strError.AppendLine($"{log.update:yyyy-MM-dd HH:mm:ss}{vbTab}{log.message}{vbCrLf}")

										   Case Else
											   strInfo.AppendLine($"{log.update:yyyy-MM-dd HH:mm:ss}{vbTab}{log.message}{vbCrLf}")

									   End Select

								   End While

								   Save(strInfo, "Info")
								   Save(strDebug, "Debug")
								   Save(strError, "Error")
							   Else
								   ' 保存到单一文件
								   Dim strLog As New Text.StringBuilder

								   Dim log As (level As LogLevelEnum, message As String, update As Date) = Nothing
								   While Instance?.Count > 0 AndAlso Instance.TryDequeue(log)
									   strLog.Append($"[{log.level.Description}]{vbTab}")
									   strLog.AppendLine($"{log.update:yyyy-MM-dd HH:mm:ss}{vbTab}{log.message}{vbCrLf}")
								   End While

								   Save(strLog, "")
							   End If

							   Options.Last = Date.Now
							   Options.IsBusy = False

							   ' 创建一个周期任务，以便检查未保存的数据
							   If Not saveNow Then
								   Threading.Thread.Sleep(3000)
								   SetTimeout(Async Sub() Await SaveAsync(), Options.SaveInterval)
							   End If
						   End Sub)
		End Function

		''' <summary>保存</summary>
		Private Shared Sub Save(strBuilder As Text.StringBuilder, level As String)
			If strBuilder?.Length > 0 Then
				Dim fileName = Options.FileFormat.GetDateTime(SYS_NOW_DATE)
				fileName = IO.Path.Combine(Options.Folder, level, fileName)
				fileName = PathHelper.Root(fileName, True)

				Try
					IO.File.AppendAllText(fileName, strBuilder.ToString)
				Catch ex As Exception
					' 保存文件出现异常，记录操异常事件
					' Err(ex, str.ToString)
				End Try
			End If
		End Sub

#End Region

#Region "注销"

		''' <summary>关闭</summary>
		Public Shared Sub Close()
			Call SaveAsync(True).Wait()
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Close()
			GC.SuppressFinalize(Me)
		End Sub

#End Region

	End Class

End Namespace
