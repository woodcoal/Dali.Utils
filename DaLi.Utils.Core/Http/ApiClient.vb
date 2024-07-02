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
' 	WebApi 操作客户端
'
' 	name: Http.ApiClient
' 	create: 2024-05-19
' 	memo: WebApi 操作客户端，基于系统 httpClient 重写
' 	
' ------------------------------------------------------------

Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Reflection
Imports DaLi.Utils.Helper.SecurityHelper
Imports DaLi.Utils.Http.Model

Namespace Http

	''' <summary>WebApi 操作客户端</summary>
	Public Class ApiClient
		Implements IDisposable

#Region "参数"

		''' <summary>HttpClientHandler</summary>
		Private ReadOnly _Handler As HttpClientHandler

		''' <summary>HttpClient</summary>
		Private ReadOnly _Client As Net.Http.HttpClient

		''' <summary>基础地址，如：http(s)://Host/api/v2/</summary>
		Public Property BaseURL As String
			Get
				Return _Client.BaseAddress?.OriginalString
			End Get
			Set(value As String)
				If Not value.IsUrl Then Return

				_Client.BaseAddress = New Uri(value)
			End Set
		End Property

		''' <summary>JWT Authorization Token</summary> 
		Public Token As String

		''' <summary>获取或设置一个值，该值指示请求是否应跟随重定向响应。</summary>
		Public Property AllowAutoRedirect As Boolean
			Get
				Return _Handler.AllowAutoRedirect
			End Get
			Set(value As Boolean)
				_Handler.AllowAutoRedirect = value
			End Set
		End Property

		''' <summary>获取或设置请求的代理信息。</summary>
		Public Property Proxy As IWebProxy
			Get
				Return _Handler.Proxy
			End Get
			Set(value As IWebProxy)
				_Handler.Proxy = If(value, SYS_PROXY)
			End Set
		End Property

		''' <summary>是否使用代理服务器，True 则 Proxy 参数有效，未设置 Proxy 则使用系统全局的设置，全局也未设置则使用 IE 的代理； False 不使用代理。</summary>
		Public Property UseProxy As Boolean
			Get
				Return _Handler.UseProxy
			End Get
			Set(value As Boolean)
				_Handler.UseProxy = value
			End Set
		End Property

		''' <summary>当前应用标识</summary> 
		Public AppID As String

		''' <summary>客户端标识</summary> 
		Public ClientID As Long

		''' <summary>客户端密匙</summary> 
		Public ClientKey As Guid

		''' <summary>用于签名的数据</summary> 
		Public ClientData As IDictionary(Of String, Object)

		''' <summary>客户端版本</summary> 
		Public ClientVersion As Single?

		''' <summary>客户端加密算法</summary> 
		Public ClientValidate As ClientSignEnum

		''' <summary>超时时长，单位：毫秒</summary>
		Public WriteOnly Property Timeout As Integer
			Set(value As Integer)
				If value > 0 AndAlso _Client.Timeout.TotalMilliseconds <> value Then _Client.Timeout = TimeSpan.FromMilliseconds(value)
			End Set
		End Property

		''' <summary>获取或设置 Referer HTTP 标头的值。</summary>
		Public Property Referer As String

		''' <summary>获取或设置 User-agent HTTP 标头的值。</summary>
		Public Property UserAgent As String

		'----------------------------------------------------------------

		''' <summary>获取或设置Cooikes字符串</summary> 
		Public ReadOnly Property Cookies As CookieContainer
			Get
				If _Handler.CookieContainer Is Nothing Then _Handler.CookieContainer = New CookieContainer
				Return _Handler.CookieContainer
			End Get
		End Property

		'''' <summary>获取请求的头部数据</summary> 
		'Private _RequestHeaders As HttpRequestHeaders

		'''' <summary>获取请求的头部数据</summary> 
		'Public ReadOnly Property RequestHeaders As HttpRequestHeaders
		'	Get
		'		If _RequestHeaders Is Nothing Then _RequestHeaders = New HttpRequestHeaders
		'		Return _RequestHeaders
		'	End Get
		'End Property

		'''' <summary>请求的头部数据</summary> 
		Public ReadOnly RequestHeaders As New NameValueDictionary

		''' <summary>设置请求头部数据</summary>
		Public Sub SetHeader(key As String, value As String)
			RequestHeaders.Update(key, value)
			'If _Request.Headers.Contains(key) Then _Request.Headers.Remove(key)
			'_Request.Headers.TryAddWithoutValidation(key, value)
		End Sub

		'------------------------------------------------------------------

		''' <summary>获取系统状态</summary>
		Private _StatusCode As HttpStatusCode

		''' <summary>获取系统状态</summary>
		Public ReadOnly Property StatusCode As HttpStatusCode
			Get
				Return _StatusCode
			End Get
		End Property

		''' <summary>数据流</summary>
		Private _HttpContent As HttpContent

		''' <summary>数据结果</summary>
		Public ReadOnly Property HttpContent As HttpContent
			Get
				Return _HttpContent
			End Get
		End Property

		''' <summary>文本结果</summary>
		Private _StringContent As String

		''' <summary>文本结果</summary>
		Public ReadOnly Property StringContent As String
			Get
				Return _StringContent
			End Get
		End Property

		''' <summary>获取反馈的头部数据</summary> 
		Private _ResponseHeaders As HttpResponseHeaders

		''' <summary>获取或设置Cooikes字符串</summary> 
		Public ReadOnly Property ResponseHeaders As HttpResponseHeaders
			Get
				Return _ResponseHeaders
			End Get
		End Property

#End Region

#Region "请求操作"

		Public Sub New()
			'_Request = New HttpRequestMessage
			' 忽略证书安全并使用 Cookies
			_Handler = New HttpClientHandler With {
				.ServerCertificateCustomValidationCallback = Function() True,
				.UseCookies = True
			}

			_Client = New Net.Http.HttpClient(_Handler)
		End Sub

		''' <summary>更新客户端签名</summary>
		Private Sub UpdateClientSign(request As HttpRequestMessage)
			If ClientID.IsEmpty OrElse ClientKey.IsEmpty Then Return

			' 加入客户端信息
			Dim signDatas As New KeyValueDictionary

			Dim signPath = request.RequestUri.AbsolutePath
			Dim signQuery = NameValueDictionary.FromQueryString(request.RequestUri.Query)
			If signQuery.NotEmpty Then signQuery.ForEach(Sub(k, v) signDatas.Add(k, v))

			signDatas.UpdateRange(ClientData)

			Dim ticks = SYS_NOW_DATE.JsTicks
			Dim code = Guid.NewGuid.ToString
			Dim version = If(ClientVersion, Assembly.GetEntryAssembly.Version)

			SetHeader("x-client-id", ClientID.ToString)
			SetHeader("x-client-app", AppID)
			SetHeader("x-client-key", Sign.Client(ClientValidate, ClientID, ClientKey, AppID, request.Method.ToString, signPath, signDatas, ticks, code))
			SetHeader("x-client-ver", version.ToString)
			SetHeader("x-client-date", ticks.ToString)
			SetHeader("x-client-code", code)
		End Sub

		''' <summary>创建请求消息</summary>
		Private Function CreateRequest(method As HttpMethodEnum, path As String, Optional content As HttpContent = Nothing) As HttpRequestMessage
			' 请求地址
			Dim url = NetHelper.AbsoluteUrl(BaseURL, path)

			' 请求方式
			Dim m = HttpMethod.Get
			Select Case method
				Case HttpMethodEnum.GET
					m = HttpMethod.Get

				Case HttpMethodEnum.POST
					m = HttpMethod.Post

				Case HttpMethodEnum.PUT
					m = HttpMethod.Put

				Case HttpMethodEnum.PATCH
					m = HttpMethod.Patch

				Case HttpMethodEnum.DELETE
					m = HttpMethod.Delete

				Case HttpMethodEnum.OPTIONS
					m = HttpMethod.Options

				Case HttpMethodEnum.HEAD
					m = HttpMethod.Head

				Case HttpMethodEnum.TRACE
					m = HttpMethod.Trace

				Case Else
					m = HttpMethod.Get
			End Select

			Dim request As New HttpRequestMessage(m, url)

			If Referer.NotEmpty Then request.Headers.Referrer = New Uri(Referer)
			If UserAgent.NotEmpty Then request.Headers.UserAgent.ParseAdd(UserAgent)
			If RequestHeaders.NotEmpty Then RequestHeaders.ForEach(Sub(key, value)
																	   If request.Headers.Contains(key) Then request.Headers.Remove(key)
																	   request.Headers.TryAddWithoutValidation(key, value)
																   End Sub)


			' JWT 认证
			If Token.IsEmpty Then
				request.Headers.Authorization = Nothing
			Else
				request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", Token)
			End If

			request.Content = content

			' 更新签名信息
			UpdateClientSign(request)

			Return request
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">类型</param>
		''' <param name="path">地址</param>
		''' <param name="content">提交的内容</param>
		Public Function Execute(method As HttpMethodEnum, path As String, Optional content As HttpContent = Nothing) As String
			_StringContent = ""
			_HttpContent = Nothing
			_StatusCode = 0
			_ResponseHeaders = Nothing

			Dim DoExecute = Sub()
								Using request = CreateRequest(method, path, content)
									Try
										Using response = _Client.Send(request)
											_StatusCode = response.StatusCode
											_HttpContent = response.Content
											_ResponseHeaders = response.Headers

											If response.IsSuccessStatusCode Then response.EnsureSuccessStatusCode()

											Using Stream = response.Content.ReadAsStream
												Using reader = New StreamReader(Stream)
													_StringContent = reader.ReadToEnd()
												End Using
											End Using
										End Using
									Catch ex As Exception
										_StringContent = ex.Message
									End Try
								End Using
							End Sub

			If content Is Nothing Then
				DoExecute()
			Else
				Using content
					DoExecute()
				End Using
			End If

			Return _StringContent
		End Function

		''' <summary>异步执行请求</summary>
		''' <param name="method">类型</param>
		''' <param name="path">地址</param>
		''' <param name="content">提交的内容</param>
		Public Async Function ExecuteAsync(method As HttpMethodEnum, path As String, Optional content As HttpContent = Nothing, Optional callback As Action(Of String) = Nothing) As Task(Of String)
			_StringContent = ""
			_HttpContent = Nothing
			_StatusCode = 0
			_ResponseHeaders = Nothing

			Dim DoExecute = Async Function()
								Using request = CreateRequest(method, path, content)
									Try
										Using response = Await _Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
											_StatusCode = response.StatusCode
											_HttpContent = response.Content
											_ResponseHeaders = response.Headers

											If response.IsSuccessStatusCode Then response.EnsureSuccessStatusCode()

											Using stream = Await response.Content.ReadAsStreamAsync()
												Using reader = New StreamReader(stream)
													While Not reader.EndOfStream
														Dim str = Await reader.ReadLineAsync()
														If Not String.IsNullOrEmpty(str) Then
															callback?.Invoke(str)

															_StringContent &= str
														End If
													End While
												End Using
											End Using
										End Using
									Catch ex As Exception
										_StringContent = ex.Message
									End Try
								End Using
							End Function

			If content Is Nothing Then
				Await DoExecute()
			Else
				Using content
					Await DoExecute()
				End Using
			End If

			Return _StringContent
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="data">提交参数</param>
		''' <param name="isJson">是否 JSON 格式请求</param>
		Public Function Execute(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True) As String
			Dim content As HttpContent = Nothing

			If data IsNot Nothing Then
				If isJson Then
					content = New StringContent(data.ToJson(False, False, True), Text.Encoding.UTF8, "application/json")
				ElseIf {HttpMethodEnum.GET, HttpMethodEnum.DELETE}.Contains(method) Then
					Dim query As New NameValueDictionary
					For Each key In data.Keys
						query.Add(key, data(key))
					Next
					path = NetHelper.NameValue2QueryString(query, path)
				Else
					content = New FormUrlEncodedContent(data)
				End If
			End If

			Return Execute(method, path, content)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="data">提交参数</param>
		''' <param name="isJson">是否 JSON 格式请求</param>
		Public Async Function ExecuteAsync(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True, Optional callback As Action(Of String) = Nothing) As Task(Of String)
			Dim content As HttpContent = Nothing

			If data IsNot Nothing Then
				If isJson Then
					content = New StringContent(data.ToJson(False, False, True), Text.Encoding.UTF8, "application/json")
				ElseIf {HttpMethodEnum.GET, HttpMethodEnum.DELETE}.Contains(method) Then
					Dim query As New NameValueDictionary
					For Each key In data.Keys
						query.Add(key, data(key))
					Next
					path = NetHelper.NameValue2QueryString(query, path)
				Else
					content = New FormUrlEncodedContent(data)
				End If
			End If

			Return Await ExecuteAsync(method, path, content, callback)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="json">提交 JSON 数据</param> 
		Public Function Execute(method As HttpMethodEnum, path As String, json As String) As String
			Dim content = If(json.IsEmpty, Nothing, New StringContent(json, Text.Encoding.UTF8, "application/json"))
			Return Execute(method, path, content)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="json">提交 JSON 数据</param>
		Public Async Function ExecuteAsync(method As HttpMethodEnum, path As String, json As String, Optional callback As Action(Of String) = Nothing) As Task(Of String)
			Dim content = If(json.IsEmpty, Nothing, New StringContent(json, Text.Encoding.UTF8, "application/json"))
			Return Await ExecuteAsync(method, path, content, callback)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="data">提交参数</param>
		''' <param name="isJson">是否 JSON 格式请求</param>
		Public Function Execute(Of T)(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True) As T
			Return Execute(method, path, data, isJson).ToJsonObject(Of T)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="data">提交参数</param>
		''' <param name="isJson">是否 JSON 格式请求</param>
		Public Async Function ExecuteAsync(Of T)(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True, Optional callback As Action(Of String) = Nothing) As Task(Of T)
			Return (Await ExecuteAsync(method, path, data, isJson)).ToJsonObject(Of T)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="json">提交 JSON 数据</param> 
		Public Function Execute(Of T)(method As HttpMethodEnum, path As String, Optional json As String = "") As T
			Return Execute(method, path, json).ToJsonObject(Of T)
		End Function

		''' <summary>执行请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="json">提交 JSON 数据</param>
		Public Async Function ExecuteAsync(Of T)(method As HttpMethodEnum, path As String, Optional json As String = "", Optional callback As Action(Of String) = Nothing) As Task(Of T)
			Return (Await ExecuteAsync(method, path, json, callback)).ToJsonObject(Of T)
		End Function

		''' <summary>执行内置 API 请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="data">提交参数</param>
		''' <param name="isJson">是否 JSON 格式请求</param>
		Public Function ExecuteApi(Of T)(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True) As ApiResult(Of T)
			Return Execute(method, path, data, isJson).ToJsonObject(Of ApiResult(Of T))
		End Function

		''' <summary>执行内置 API 请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="data">提交参数</param>
		''' <param name="isJson">是否 JSON 格式请求</param>
		Public Async Function ExecuteApiAsync(Of T)(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True, Optional callback As Action(Of String) = Nothing) As Task(Of ApiResult(Of T))
			Return (Await ExecuteAsync(method, path, data, isJson)).ToJsonObject(Of ApiResult(Of T))
		End Function

		''' <summary>执行内置 API 请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="json">提交 JSON 数据</param> 
		Public Function ExecuteApi(Of T)(method As HttpMethodEnum, path As String, Optional json As String = "") As ApiResult(Of T)
			Return Execute(method, path, json).ToJsonObject(Of ApiResult(Of T))
		End Function

		''' <summary>执行内置 API 请求</summary>
		''' <param name="method">请求方式</param>
		''' <param name="path">请求路径</param>
		''' <param name="json">提交 JSON 数据</param>
		Public Async Function ExecuteApiAsync(Of T)(method As HttpMethodEnum, path As String, Optional json As String = "", Optional callback As Action(Of String) = Nothing) As Task(Of ApiResult(Of T))
			Return (Await ExecuteAsync(method, path, json, callback)).ToJsonObject(Of ApiResult(Of T))
		End Function

		''' <summary>创建上传对象数据</summary>
		Public Shared Function CreateFileContent(filePath As String, Optional fieldName As String = "") As MultipartFormDataContent
			If Not PathHelper.FileExist(filePath) Then Return Nothing

			Dim content = New FileStream(filePath, FileMode.Open, FileAccess.Read)
			Dim fileName = IO.Path.GetFileName(filePath)

			Return CreateFileContent(content, fieldName, fileName)
		End Function


		''' <summary>创建上传对象数据</summary>
		''' <param name="fileContent">文件数据流</param>
		''' <param name="fieldName">字段名称</param>
		''' <param name="fileName">文件名</param>
		''' <returns></returns>
		Public Shared Function CreateFileContent(fileContent As Stream, Optional fieldName As String = "", Optional fileName As String = "") As MultipartFormDataContent
			If fileContent Is Nothing Then Return Nothing

			Dim ret As New MultipartFormDataContent()
			Dim content As New StreamContent(fileContent)

			If fieldName.IsEmpty Then
				ret.Add(content)
			Else
				If fileName.IsEmpty Then
					If fileContent.GetType = GetType(FileStream) Then
						With TryCast(fileContent, FileStream)
							fileName = IO.Path.GetFileName(.Name)
						End With
					End If
				End If

				If fileName.IsEmpty Then
					ret.Add(content, fieldName)
				Else
					ret.Add(content, fieldName, fileName)
				End If
			End If

			Return ret
		End Function

#End Region

#Region "注销"

		Public Sub Dispose() Implements IDisposable.Dispose
			_Client?.Dispose()
			_Handler?.Dispose()
			GC.SuppressFinalize(Me)
		End Sub
#End Region

	End Class

End Namespace
