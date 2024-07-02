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
' 	控制台简化操作
'
' 	name: CONS
' 	create: 2019-03-31
' 	memo: 控制台简化操作
' 	
' ------------------------------------------------------------

''' <summary>控制台简化操作</summary>
Public Class CON

#Region "其他输出"

	''' <summary>清空控制台</summary>
	Public Shared Sub Clear()
		Console.Clear()
		ColorDefault()
		Console.OutputEncoding = Text.Encoding.UTF8
	End Sub

	''' <summary>应用启动</summary>
	Public Shared Sub AppStart(Optional information As String = "")
		Title(information.EmptyValue("应用启动"))
	End Sub

	''' <summary>应用结束</summary>
	Public Shared Sub AppFinish(Optional information As String = "", Optional showPressKey As Boolean = True)
		Line()
		Echo("应用执行完成，累计操作时间：" & SYS_NOW.ShowDiff(SYS_START))
		Title(information.EmptyValue("应用结束"))

		Echo()
		If showPressKey Then Wait("按任意键退出系统...", True)
	End Sub

	''' <summary>等待</summary>
	Public Shared Sub Wait(Optional information As String = "", Optional anyKey As Boolean = False)
		Console.ForegroundColor = ConsoleColor.Gray
		Echo()
		Echo(information)
		Console.ResetColor()

		If anyKey Then
			Console.ReadKey()
		Else
			Console.Read()
		End If
	End Sub

	''' <summary>输出分隔符</summary>
	Public Shared Sub Line(Optional number As Integer = 0, Optional splitString As String = "-")
		splitString = splitString.NullValue("-")
		If number < 1 Then number = (Console.WindowWidth - 1) / splitString.UnicodeLength.Range(1)

		Console.ForegroundColor = ConsoleColor.DarkGray
		Console.WriteLine()
		Console.WriteLine(splitString.Duplicate(number))
		Console.WriteLine()
		Console.ForegroundColor = ConsoleColor.White
	End Sub

	''' <summary>输出标题</summary>
	Public Shared Sub Title(information As String, Optional titleColor As ConsoleColor = ConsoleColor.Yellow)
		If information.NotEmpty Then
			Dim Left = ((Console.WindowWidth - information.UnicodeLength) / 2) - 1
			Left = Left.Range(0)

			Dim Start = (Console.WindowWidth - 1).Range(0)

			Console.ForegroundColor = titleColor
			Console.WriteLine()
			Console.WriteLine(New String("#"c, Start))
			Console.WriteLine()
			Console.WriteLine(New String(" "c, Left) & information)
			Console.WriteLine()
			Console.WriteLine(New String("#"c, Start))
			Console.WriteLine()

			ColorDefault()
		End If
	End Sub

	''' <summary>输出带时间的内容</summary>
	Public Shared Sub Time(information As String, Optional foregroundColor As ConsoleColor = ConsoleColor.White)
		Console.BackgroundColor = ConsoleColor.DarkGray
		Console.ForegroundColor = ConsoleColor.Blue
		Console.Write(SYS_NOW_STR("HH:mm:ss"))

		Console.Write(vbTab)

		Console.BackgroundColor = ConsoleColor.Black
		Console.ForegroundColor = foregroundColor
		Console.Write(information)
		Console.Write(vbCrLf)

		ColorDefault()
	End Sub

	''' <summary>获取用户输入</summary>
	''' <param name="information">提示文本</param>
	''' <param name="isLine">是否含行输出，否则内容跟随其后。</param>
	Public Shared Function Input(Optional information As String = "", Optional isLine As Boolean = True) As String
		If information.NotEmpty Then Echo(information, isLine)
		Return Console.ReadLine
	End Function

#End Region

	'#Region "计时器"

	'	''' <summary>计时器列表</summary>
	'	Private ReadOnly _Stopwatchs As New Dictionary(Of String, Stopwatch)(StringComparer.OrdinalIgnoreCase)


	'	''' <summary>获取计时器</summary>
	'	Private ReadOnly Property GetStopwatch(name As String) As Stopwatch
	'		Get
	'			name = name.EmptyValue("app")

	'			SyncLock _Stopwatchs
	'				If Not _Stopwatchs.ContainsKey(name) Then
	'					_Stopwatchs.Add(name, New Stopwatch)
	'				End If
	'			End SyncLock

	'			Return _Stopwatchs(name)
	'		End Get
	'	End Property

	'	''' <summary>启动一个指定名称的计时器，如果之前已经启动则会重新开始，并返回此计数器对象</summary>
	'	Public Function TimeStart(Optional name As String = "") As Stopwatch
	'		Dim s = GetStopwatch(name)
	'		s.Start()
	'		Return s
	'	End Function

	'	''' <summary>停止一个计时器，并返回毫秒时间</summary>
	'	Public Function TimeStop(Optional name As String = "") As Long
	'		Dim s = GetStopwatch(name)
	'		s.Stop()
	'		Return s.ElapsedMilliseconds
	'	End Function

	'	''' <summary>停止一个计时器，并返回毫秒时间</summary>
	'	Public Function TimeRestart(Optional name As String = "") As Stopwatch
	'		Dim s = GetStopwatch(name)
	'		s.Stop()
	'		s.Reset()
	'		s.Start()
	'		Return s
	'	End Function

	'	''' <summary>输出时间，并返回此计数器对象</summary>
	'	Public Function TimeEcho(Optional name As String = "", Optional information As String = "", Optional mustStop As Boolean = False) As Stopwatch
	'		Dim s = GetStopwatch(name)
	'		s.Stop()
	'		Echo(information & s.Elapsed.Show)
	'		If Not mustStop Then s.Start()
	'		Return s
	'	End Function

	'#End Region

#Region "颜色（默认/信息/成功/警告/错误）"

	Private Shared Sub ColorDefault()
		Console.ResetColor()
		Console.BackgroundColor = ConsoleColor.Black
		Console.ForegroundColor = ConsoleColor.White
	End Sub


	''' <summary>输出警告</summary>
	Private Shared Sub ColorWarn()
		Console.BackgroundColor = ConsoleColor.Black
		Console.ForegroundColor = ConsoleColor.Yellow
	End Sub

	''' <summary>输出错误</summary>
	Private Shared Sub ColorErr()
		Console.BackgroundColor = ConsoleColor.Black
		Console.ForegroundColor = ConsoleColor.Red
	End Sub

	''' <summary>输出错误</summary>
	Private Shared Sub ColorInfo()
		Console.BackgroundColor = ConsoleColor.Black
		Console.ForegroundColor = ConsoleColor.Blue
	End Sub

	''' <summary>输出错误</summary>
	Private Shared Sub ColorSucc()
		Console.BackgroundColor = ConsoleColor.Black
		Console.ForegroundColor = ConsoleColor.Green
	End Sub

	''' <summary>输出警告标题</summary>
	Private Shared Sub ColorWarnTitle()
		Console.BackgroundColor = ConsoleColor.Yellow
		Console.ForegroundColor = ConsoleColor.Red
	End Sub

	''' <summary>输出错误标题</summary>
	Private Shared Sub ColorErrTitle()
		Console.BackgroundColor = ConsoleColor.White
		Console.ForegroundColor = ConsoleColor.Red
	End Sub

	''' <summary>输出信息标题</summary>
	Private Shared Sub ColorInfoTitle()
		Console.BackgroundColor = ConsoleColor.Gray
		Console.ForegroundColor = ConsoleColor.Blue
	End Sub

	''' <summary>输出错误</summary>
	Private Shared Sub ColorSuccTitle()
		Console.BackgroundColor = ConsoleColor.White
		Console.ForegroundColor = ConsoleColor.Green
	End Sub

#End Region

#Region "输出"

	''' <summary>输出字符到控制台</summary>
	''' <param name="information">输出文本</param>
	''' <param name="isLine">是否含行输出，否则内容跟随其后。</param>
	Public Shared Sub Echo(Optional information As String = "", Optional isLine As Boolean = True)
		If String.IsNullOrWhiteSpace(information) Then
			Console.WriteLine()
		ElseIf isLine Then
			Console.WriteLine(information)
		Else
			Console.Write(information)
		End If

		ColorDefault()
	End Sub

	''' <summary>输出格式化字符到控制台</summary>
	Public Shared Sub Echo(informationListFormat As String, ParamArray informationValues() As Object)
		Echo(String.Format(informationListFormat, informationValues))
	End Sub

	''' <summary>输出一组字符到控制台，并用指定字符区分</summary>
	Public Shared Sub Echo(informationList As Object(), Optional splitString As String = vbTab)
		If informationList?.Length > 0 Then
			If informationList.Length > 1 Then
				Echo(String.Join(splitString, informationList))
			Else
				Echo(informationList(0))
			End If
		Else
			Echo()
		End If
	End Sub

#End Region

#Region "信息输出"

	''' <summary>信息标题</summary>
	Public Shared Sub InfoTitle(information As String)
		ColorInfoTitle()
		Echo(information)
	End Sub

	''' <summary>信息标题</summary>
	Public Shared Sub InfoTitle(ParamArray informations() As String)
		ColorInfoTitle()
		Echo(informations)
	End Sub

	''' <summary>输出字符到控制台</summary>
	Public Shared Sub Info(Optional information As String = "", Optional isLine As Boolean = True)
		ColorInfo()
		Echo(information, isLine)
	End Sub

	''' <summary>输出格式化字符到控制台</summary>
	Public Shared Sub Info(informationFormat As String, ParamArray informationValues() As Object)
		ColorInfo()
		Echo(informationFormat, informationValues)
	End Sub

	''' <summary>输出一组字符到控制台，并用指定字符区分</summary>
	Public Shared Sub Info(informationList As Object(), Optional splitString As String = vbTab)
		ColorInfo()
		Echo(informationList, splitString)
	End Sub

#End Region

#Region "成功输出"

	''' <summary>成功标题</summary>
	Public Shared Sub SuccTitle(information As String)
		ColorSuccTitle()
		Echo(information)
	End Sub

	''' <summary>警告标题</summary>
	Public Shared Sub SuccTitle(ParamArray informations() As String)
		ColorSuccTitle()
		Echo(informations)
	End Sub

	''' <summary>输出字符到控制台</summary>
	Public Shared Sub Succ(Optional information As String = "", Optional isLine As Boolean = True)
		ColorSucc()
		Echo(information, isLine)
	End Sub

	''' <summary>输出格式化字符到控制台</summary>
	Public Shared Sub Succ(informationFormat As String, ParamArray informationValues() As Object)
		ColorSucc()
		Echo(informationFormat, informationValues)
	End Sub

	''' <summary>输出一组字符到控制台，并用指定字符区分</summary>
	Public Shared Sub Succ(informationList As Object(), Optional splitString As String = vbTab)
		ColorSucc()
		Echo(informationList, splitString)
	End Sub

#End Region

#Region "错误输出"

	''' <summary>错误标题</summary>
	Public Shared Sub ErrTitle(information As String)
		ColorErrTitle()
		Echo(information)
	End Sub

	''' <summary>错误标题</summary>
	Public Shared Sub ErrTitle(ParamArray informations() As String)
		ColorErrTitle()
		Echo(informations)
	End Sub

	''' <summary>输出字符到控制台</summary>
	Public Shared Sub Err(ex As Exception, Optional information As String = "")
		ErrTitle(information.EmptyValue(ex?.Message))

		If ex IsNot Nothing Then
			Dim st As New StackTrace(ex, True)
			Dim frs = st?.GetFrames
			Dim sf = frs?(frs.Length - 1)

			With New Text.StringBuilder
				.AppendFormat("时间：{0}{1}", SYS_NOW_STR, vbCrLf)

				If sf IsNot Nothing Then
					.AppendFormat("类名：{0}{1}", sf.GetMethod().DeclaringType.FullName, vbCrLf)
					.AppendFormat("方法：{0}{1}", sf.GetMethod().Name, vbCrLf)
					.AppendFormat("行号：{0}{1}", sf.GetFileLineNumber, vbCrLf)
				End If

				.AppendFormat("异常：{0}{1}", ex.ToString, vbCrLf)


				Err(.ToString)
			End With
		Else
			Info("暂未发生异常")
		End If
	End Sub

	''' <summary>输出字符到控制台</summary>
	Public Shared Sub Err(Optional information As String = "", Optional isLine As Boolean = True)
		ColorErr()
		Echo(information, isLine)
	End Sub

	''' <summary>输出格式化字符到控制台</summary>
	Public Shared Sub Err(informationFormat As String, ParamArray informationValues() As Object)
		ColorErr()
		Echo(informationFormat, informationValues)
	End Sub

	''' <summary>输出一组字符到控制台，并用指定字符区分</summary>
	Public Shared Sub Err(informationList As Object(), Optional splitString As String = vbTab)
		ColorErr()
		Echo(informationList, splitString)
	End Sub

#End Region

#Region "警告输出"

	''' <summary>警告标题</summary>
	Public Shared Sub WarnTitle(information As String)
		ColorWarnTitle()
		Echo(information)
	End Sub

	''' <summary>警告标题</summary>
	Public Shared Sub WarnTitle(ParamArray informations() As String)
		ColorWarnTitle()
		Echo(informations)
	End Sub

	''' <summary>输出字符到控制台</summary>
	Public Shared Sub Warn(Optional information As String = "", Optional isLine As Boolean = True)
		ColorWarn()
		Echo(information, isLine)
	End Sub

	''' <summary>输出格式化字符到控制台</summary>
	Public Shared Sub Warn(informationFormat As String, ParamArray informationValues() As Object)
		ColorWarn()
		Echo(informationFormat, informationValues)
	End Sub

	''' <summary>输出一组字符到控制台，并用指定字符区分</summary>
	Public Shared Sub Warn(informationList As Object(), Optional splitString As String = vbTab)
		ColorWarn()
		Echo(informationList, splitString)
	End Sub

#End Region

End Class
