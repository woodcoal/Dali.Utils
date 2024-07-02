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
' 	Http 代理服务器
'
' 	name: Http.HttpProxy
' 	create: 2022-10-11
' 	memo: Http 代理服务器
'
' ------------------------------------------------------------
Imports System.Net
Imports System.Threading
Imports DaLi.Utils.Http.Model

Namespace Http

	''' <summary>Http 代理服务器</summary>
	Public Class HttpProxy

		''' <summary>用于验证的网址</summary>
		Public ValidateURL As String

		''' <summary>用于验证的网址页面中包含的内容</summary>
		Public ValidateCONTENT As String

		''' <summary>用于验证的请求方式</summary>
		Public ValidateMETHOD As HttpMethodEnum = HttpMethodEnum.GET

		''' <summary>用于验证的表单提交方式</summary>
		Public ValidatePOSTTYPE As HttpPostEnum = HttpPostEnum.DEFAULT

		''' <summary>用于验证的表单提交内容</summary>
		Public ValidatePOSTCONTENT As IDictionary(Of String, String)

		''' <summary>验证超时时间(单位：秒)</summary>
		Public ValidateTIMEOUT As Integer

		''' <summary>批量验证时线程数量</summary>
		Public ValidateTHREAD As Integer

		''' <summary>批量验证时，验证次数</summary>
		Public ValidateTIMES As Integer

		''' <summary>创建代理服务器</summary>
		''' <param name="address">代理服务器地址</param>
		''' <param name="port">代理服务器端口</param>
		''' <param name="account">账号</param>
		''' <param name="password">密码</param>
		''' <returns>返回代理响应速度，小于 1 则无效</returns>
		Public Shared Function CreateProxy(address As String, port As Integer, Optional account As String = "", Optional password As String = "") As IWebProxy
			Dim proxy As New WebProxy(address, port)
			If account.NotEmpty Then proxy.Credentials = New NetworkCredential(account, password)
			Return proxy
		End Function

		''' <summary>代理服务器验证</summary>
		''' <param name="address">代理服务器地址</param>
		''' <param name="port">代理服务器端口</param>
		''' <param name="account">账号</param>
		''' <param name="password">密码</param>
		''' <returns>返回代理响应速度，小于 1 则无效</returns>
		Public Function Validate(address As String, port As Integer, Optional account As String = "", Optional password As String = "") As Long
			If address.IsEmpty Then Return -1
			If port < 1 OrElse port > 65535 Then Return -2

			Dim url = ValidateURL
			If Not url.IsUrl Then url = "http://www.baidu.com"

			Dim content = ValidateCONTENT
			If content.IsEmpty Then content = "*<html*"

			' 状态
			Dim succ = False

			Dim s As New Stopwatch

			Dim http As New HttpClient With {
				.Url = url,
				.Method = ValidateMETHOD,
				.PostType = ValidatePOSTTYPE,
				.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36",
				.Referer = url
			}

			If ValidatePOSTCONTENT.NotEmpty Then
				For Each Kv In ValidatePOSTCONTENT
					http.SetPostContent(Kv.Key, Kv.Value)
				Next
			End If

			' 超时 10 秒
			http.Request.Timeout = ValidateTIMEOUT.Range(1, 60) * 1000
			http.Request.UseProxy = True
			http.Request.Proxy = CreateProxy(address, port, account, password)

			s.Start()

			Try
				http.Execute()
				Dim html = http.GetHtml
				If html.NotEmpty AndAlso http.StatusCode = HttpStatusCode.OK Then
					succ = html.Like(content)
				End If
			Catch ex As Exception
			End Try

			s.Stop()

			Return If(succ, s.ElapsedMilliseconds, 0)
		End Function

#Region "代理分析"

		''' <summary>分析代理</summary>
		''' <param name="address">代理服务器地址</param>
		''' <param name="port">代理服务器端口</param>
		''' <param name="account">账号</param>
		''' <param name="password">密码</param>
		''' <param name="times">验证次数</param>
		Public Function Analyse(address As String, port As Integer, Optional account As String = "", Optional password As String = "", Optional times As Integer? = Nothing) As (Level As Integer, Speed As Long)
			Dim level = 0
			Dim speed As Long = 0
			Dim count = If(times, ValidateTIMES).Range(1, 100)
			For I = 1 To count
				If I > 1 Then Thread.Sleep(1000)

				Dim s = Validate(address, port, account, password)

				' 非网络问题直接返回
				If s < 0 Then Exit For

				If s > 1 Then
					level += 1
					speed += s
				End If
			Next

			' 平均速度
			If level > 0 Then
				speed /= level
				Return (level, speed)
			Else
				Return (0, -1)
			End If
		End Function

		''' <summary>分析代理</summary>
		''' <param name="proxies">匿名代理服务器地址</param>
		''' <param name="successAction">验证成功操作，参数：代理服务器地址，端口，验证成功次数，平均速度(ms)</param>
		''' <param name="failAction">验证失败操作，参数：代理服务器地址，端口，验证成功次数，平均速度(ms)</param>
		Public Function Analyse(proxies As IDictionary(Of String, Integer), Optional successAction As Action(Of String, Integer, Integer, Long) = Nothing, Optional failAction As Action(Of String, Integer) = Nothing) As List(Of (Address As String, Port As Integer, Level As Integer, Speed As Long))
			If proxies.IsEmpty Then Return Nothing

			'----------
			' 分析
			'----------
			Dim no = -1
			Dim tasks As New List(Of Task)
			Dim ret As New List(Of (String, Integer, Integer, Long))
			For I = 1 To ValidateTHREAD.Range(1, 50)
				tasks.Add(Task.Run(Sub()
									   While True
										   Dim idx = Interlocked.Increment(no)
										   If idx >= proxies.Count Then Exit While

										   Dim ip = proxies.Keys(idx)
										   Dim port = proxies.Values(idx)

										   Dim res = Analyse(ip, port)
										   If res.Level > 0 Then
											   ret.Add((ip, port, res.Level, res.Speed))
											   successAction?.Invoke(ip, port, res.Level, res.Speed)
										   Else
											   failAction?.Invoke(ip, port)
										   End If
									   End While
								   End Sub))
			Next

			Task.WaitAll(tasks.ToArray)

			Return ret
		End Function

		''' <summary>分析并检测代理服务器数据</summary>
		''' <param name="proxyContent">代理服务器内容</param>
		''' <param name="successAction">验证成功操作，参数：代理服务器地址，端口，验证成功次数，平均速度(ms)</param>
		''' <param name="failAction">验证失败操作，参数：代理服务器地址，端口，验证成功次数，平均速度(ms)</param>
		Public Function Analyse(proxyContent As String, Optional successAction As Action(Of String, Integer, Integer, Long) = Nothing, Optional failAction As Action(Of String, Integer) = Nothing) As List(Of (Address As String, Port As Integer, Level As Integer, Speed As Long))
			If proxyContent.IsEmpty Then Return Nothing

			'-------------
			' 整理地址列表
			'-------------
			Dim list = proxyContent.SplitDistinct(vbCrLf)
			If list.IsEmpty Then Return Nothing

			Dim proxies = New Dictionary(Of String, Integer)
			For Each item In list
				If item.NotEmpty Then
					item = item.Replace(vbTab, " ").TrimFull
					If item.Contains(" "c) Then
						Dim arr = item.Split(" "c, StringSplitOptions.RemoveEmptyEntries)
						If arr?.Length > 1 Then
							Dim IP = arr(0)
							Dim Port = arr(1).ToInteger

							If IP.NotEmpty AndAlso Port > 1 AndAlso Port < 65536 AndAlso Not proxies.ContainsKey(arr(0)) Then proxies.Add(arr(0), arr(1))
						End If
					End If
				End If
			Next

			If proxies.IsEmpty Then Return Nothing

			'----------
			' 分析
			'----------
			Return Analyse(proxies, successAction, failAction)
		End Function
#End Region

#Region "自动代理"

		''' <summary>默认代理列表</summary>
		Private ReadOnly _ProxyList As New Dictionary(Of (Address As String, Port As Integer), IWebProxy)

		''' <summary>当前使用代理服务器索引</summary>
		Private _ProxyIndex As Integer = -1

		''' <summary>将本机信息加入代理，以便循环时可以使用本机数据，获取代理服务器时，本机数据为 Nothing</summary>
		Public Sub SetProxyLocal()
			Dim key = ("", 0)

			If Not _ProxyList.ContainsKey(key) Then
				SyncLock _ProxyList
					_ProxyList.Add(key, Nothing)
				End SyncLock
			End If
		End Sub

		''' <summary>移除本机代理</summary>
		Public Sub RemoveProxyLocal()
			Dim key = ("", 0)

			If _ProxyList.ContainsKey(key) Then
				SyncLock _ProxyList
					_ProxyList.Remove(key)
				End SyncLock
			End If
		End Sub

		''' <summary>添加代理服务器数据并是否同时验证</summary>
		Public Sub SetProxy(address As String, port As Integer, Optional mustVaildate As Boolean = False)
			If address.IsEmpty OrElse port < 1 OrElse port > 65535 Then Return

			Dim key = (address, port)
			If _ProxyList.ContainsKey(key) Then Return

			If mustVaildate AndAlso Validate(address, port) < 1 Then Return

			SyncLock _ProxyList
				_ProxyList.Add(key, Nothing)
			End SyncLock
		End Sub

		''' <summary>移除代理服务器数据</summary>
		Public Sub RemoveProxy(address As String, port As Integer)
			Dim key = (address, port)

			If _ProxyList.ContainsKey(key) Then
				SyncLock _ProxyList
					_ProxyList.Remove(key)
				End SyncLock
			End If
		End Sub

		''' <summary>添加代理服务器数据并是否同时验证</summary>
		Public Sub SetProxies(data As IDictionary(Of String, Integer), Optional mustVaildate As Boolean = False)
			If data.IsEmpty Then Return

			For Each item In data
				SetProxy(item.Key, item.Value, mustVaildate)
			Next
		End Sub

		''' <summary>获取代理，如果不存在有效的则返回空值</summary>
		''' <param name="beforeValidate">是否在加载前验证有效性</param>
		Public Function GetProxy(Optional beforeValidate As Boolean = False) As IWebProxy
			If _ProxyList.IsEmpty Then Return Nothing

			Dim index = Interlocked.Increment(_ProxyIndex)
			If index >= _ProxyList.Count Then
				index = 0
				Interlocked.Exchange(_ProxyIndex, -1)
			End If

			Dim ip = _ProxyList.Keys(index)

			' 如果增加本机代理信息，直接返回
			If ip.Address.IsEmpty Then Return Nothing

			If beforeValidate AndAlso Validate(ip.Address, ip.Port) < 1 Then Return Nothing

			Dim proxy = _ProxyList.Values(index)
			If proxy Is Nothing Then
				proxy = CreateProxy(ip.Address, ip.Port)

				SyncLock _ProxyList
					_ProxyList(ip) = proxy
				End SyncLock
			End If

			Return proxy
		End Function

		''' <summary>获取所有代理数据，除本地代理</summary>
		Public Function GetProxies() As Dictionary(Of String, Integer)
			Return _ProxyList.Where(Function(x) x.Key.Address.NotEmpty).ToDictionary(Function(x) x.Key.Address, Function(x) x.Key.Port)
		End Function

		''' <summary>是否存在代理</summary>
		Public Function HasProxy() As Boolean
			Return _ProxyList.Any
		End Function

#End Region

	End Class

End Namespace

