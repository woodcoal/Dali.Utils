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
' 	Http 操作处理
'
' 	name: Http.Client
' 	create: 2020-11-17
' 	memo: Http 操作处理
' 	
' ------------------------------------------------------------

Imports System.Net

Namespace Http

	''' <summary>Http 操作处理</summary>
	Public Class Client

		''' <summary>请求数据</summary>
		Public ReadOnly Request As Request

		''' <summary>反馈数据</summary>
		Public ReadOnly Response As Response

		''' <summary>执行错误</summary>
		Private _Exception As WebException

		''' <summary>执行错误</summary>
		Public ReadOnly Property Exception As WebException
			Get
				Return _Exception
			End Get
		End Property

		''' <summary>当前操作状态</summary>
		Private _Status As Model.HttpStatusEnum

		''' <summary>状态改变</summary>
		Public Event StatusChange(status As Model.HttpStatusEnum, request As HttpWebRequest, response As HttpWebResponse)

		''' <summary>构造</summary>
		Public Sub New(Optional req As Request = Nothing)
			Request = If(req, New Request)
			Response = New Response
			_Status = Model.HttpStatusEnum.UNKNOWN
		End Sub

		''' <summary>创建请求</summary>
		Private Function Create() As HttpWebRequest
			Dim Req = HttpWebRequest.CreateHttp(Request.Url)

#Region "网络优化，有待验证"

			' 默认并发连接数，系统默认 2
			ServicePointManager.DefaultConnectionLimit = 512

			Req.ServicePoint.Expect100Continue = False

			'是否使用 Nagle 不使用 提高效率  
			Req.ServicePoint.UseNagleAlgorithm = False

			'最大连接数  
			Req.ServicePoint.ConnectionLimit = 65500

			''数据是否缓冲 false 提高效率；此参数不能设置，否则出现异常
			'oRequest.AllowWriteStreamBuffering = False
#End Region

			'-------------
			' HTTPS 处理
			'-------------
			If Request.Url.Like("https://*") Then
				ServicePointManager.ServerCertificateValidationCallback = Function() True
				Req.ProtocolVersion = HttpVersion.Version11
			End If

			'-------------
			' 基本初始化
			'-------------
			Req.Accept = Request.Accept
			Req.Referer = Request.Referer
			Req.UserAgent = Request.UserAgent

			' 自动跳转
			Req.AllowAutoRedirect = Request.AllowAutoRedirect
			If Request.MaximumAutomaticRedirections > 0 Then Req.MaximumAutomaticRedirections = Request.MaximumAutomaticRedirections

			' 数据压缩
			Req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate")
			Req.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate

			'超时
			If Request.Timeout > 0 Then Req.Timeout = Request.Timeout

			' 设置代理服务器，
			' 为空的时候采用 IE 的设置， Nothing 为 系统设置， Not Nothing 则为自行设置的内容
			If Request.UseProxy Then
				Dim Proxy = If(Request.Proxy, SYS_PROXY)
				If Proxy IsNot Nothing Then
					Req.Proxy = Proxy
				Else
					Req.Proxy = WebRequest.GetSystemWebProxy
				End If
			Else
				Req.Proxy = Nothing
			End If

			'-----------------
			' 请求头
			'-----------------
			If Request.Headers.Count > 0 Then
				Req.Headers.Add(Request.Headers)
			End If

			'-----------------
			' Cookies 处理
			'-----------------
			If Request.Cookies IsNot Nothing Then
				Req.CookieContainer = New CookieContainer

				Dim Host As String = ""
				If Req.RequestUri IsNot Nothing Then
					Host = Req.RequestUri.Host.ToLower
				End If

				For Each Cookie As Cookie In Request.Cookies
					Try
						If Cookie.Value.Contains(","c) Then Cookie.Value = Cookie.Value.Replace(",", "%2C")
						If Host.NotEmpty Then Cookie.Domain = Host
						Req.CookieContainer.Add(Cookie)
					Catch ex As Exception
					End Try
				Next
			End If

			'-----------------
			' 数据提交
			'-----------------

			' 是否存在附件, 强制上传模式为附件
			If Request.Content.Count > 0 Then
				If Request.Method <> Model.HttpMethodEnum.POST OrElse Request.Method <> Model.HttpMethodEnum.PUT OrElse Request.Method <> Model.HttpMethodEnum.PATCH Then Request.Method = Model.HttpMethodEnum.POST
				If Request.HasAttachments Then Request.ContentType = Model.HttpPostEnum.MULTIPART
			End If

			' 请求方式
			Req.Method = Request.Method.Name.ToUpper

			' 初始化请求类型
			Request.ContentType = Request.ContentType.EmptyValue(Request.Header(HttpRequestHeader.ContentType))

			' 发送数据
			If Request.Content.Count > 0 Then
				Select Case Request.Method
					Case Model.HttpMethodEnum.POST, Model.HttpMethodEnum.PUT, Model.HttpMethodEnum.PATCH
						Using Memory = Req.GetRequestStream
							Using Writer As New IO.StreamWriter(Memory)

								Select Case Request.PostType
									Case Model.HttpPostEnum.RAW
										' 上传原始数据
										Req.ContentType = Request.ContentType.EmptyValue("text/plain")
										Writer.Write(Request.RawContent)

									Case Model.HttpPostEnum.JSON
										' 上传 JSON 请求
										' 对于包含 . 的键名，将转换到子键。如： a.b=xxx   =>   {a:{b:xx}}

										Req.ContentType = "application/json"
										Writer.Write(Request.JsonContent)

									Case Model.HttpPostEnum.MULTIPART
										' 上传附件

										Dim boundary As String = RandomHelper.Hash
										Req.ContentType = "multipart/form-data; boundary=" & boundary

										For Each Content In Request.Content
											Dim fileName = ""
											Dim fileType = ""

											Dim ext = Content.Value.Ext
											If ext IsNot Nothing Then
												If ext.GetType.IsString Then
													fileName = ext
												Else
													Dim dic = JsonExtension.ToJson(ext).ToJsonNameValues
													If dic.NotEmpty Then
														fileName = dic("filename").EmptyValue(dic("name"))
														fileType = dic("fileType").EmptyValue(dic("type")).EmptyValue(dic("mime"))
													End If
												End If
											End If

											Select Case Content.Value.ValueType
												Case Model.HttpFieldTypeEnum.CONTENT
													' 内容 bytes
													Dim Data = TryCast(Content.Value.Value, Byte())
													If Data?.Length > 0 Then
														fileName = fileName.EmptyValue(Content.Key)

														Writer.Write("--" & boundary & vbCrLf)
														Writer.Write("Content-Disposition: form-data; name=""{0}""; filename=""{1}""{2}", Content.Key, fileName, vbCrLf)
														Writer.Write("Content-Type: {0}{1}{1}", fileType, vbCrLf)
														Writer.Flush()
														Memory.Write(Data, 0, Data.Length)
														Writer.Write(vbCrLf)
													End If

												Case Model.HttpFieldTypeEnum.PATH
													' 附件
													Dim Path = Content.Value.Value.ToString
													If PathHelper.FileExist(Path) Then
														Dim Data = IO.File.ReadAllBytes(Path)
														If Data?.Length > 0 Then
															fileName = fileName.EmptyValue(IO.Path.GetFileName(Path))
															fileType = fileType.EmptyValue(NetHelper.Ext2Mime(IO.Path.GetExtension(Path)))

															Writer.Write("--" & boundary & vbCrLf)
															Writer.Write("Content-Disposition: form-data; name=""{0}""; filename=""{1}""{2}", Content.Key, fileName, vbCrLf)
															Writer.Write("Content-Type: {0}{1}{1}", fileType, vbCrLf)
															Writer.Flush()
															Memory.Write(Data, 0, Data.Length)
															Writer.Write(vbCrLf)
														End If
													End If

												Case Else
													' 普通字段
													fileName = fileName.EmptyValue(Content.Key)

													Writer.Write("--" & boundary & vbCrLf)
													Writer.Write("Content-Disposition: form-data; name=""{0}""; filename=""{1}""{2}", Content.Key, fileName, vbCrLf)
													Writer.Write("Content-Type: {0}{1}{1}", fileType, vbCrLf)
													Writer.Write(Content.Value.Value.ToString & vbCrLf)
											End Select
										Next

										Writer.Write("--" & boundary & "--" & vbCrLf)

									Case Else
										' 表单上传
										Req.ContentType = "application/x-www-form-urlencoded"
										Writer.Write(Request.QueryContent)

								End Select

							End Using
						End Using

					Case Else
						If Request.PostType = Model.HttpPostEnum.JSON Then
							Req.ContentType = "application/json"
						Else
							Req.ContentType = Request.ContentType.EmptyValue("text/plain")
						End If
				End Select
			End If

			Return Req
		End Function

		''' <summary>发出一次新的请求,并返回获得的回应</summary> 
		''' <returns>相应的HttpWebResponse</returns> 
		Public Function Execute() As HttpWebResponse
			_Exception = Nothing

			' 防止重复请求，上次操作未完成不再操作
			If _Status <> Model.HttpStatusEnum.UNKNOWN AndAlso _Status <> Model.HttpStatusEnum.FINISH Then Return Nothing

			StatusUpdate(Model.HttpStatusEnum.PREPARE, Nothing, Nothing)
			Dim Req = Create()
			StatusUpdate(Model.HttpStatusEnum.REQUEST, Req, Nothing)

			Dim Res As HttpWebResponse
			Try
				Res = Req.GetResponse()
				StatusUpdate(Model.HttpStatusEnum.RESPONSE, Req, Res)
			Catch ex As WebException
				Res = ex.Response
				StatusUpdate(Model.HttpStatusEnum.FAILURE, Req, Res)
				'CON.Err(ex, "获取网页数据")

				_Exception = ex
			End Try

			ResponseUpdate(Req, Res)

			StatusUpdate(Model.HttpStatusEnum.FINISH, Req, Res)

			Return Res
		End Function

		''' <summary>异步请求，通过 callBack 获取反馈的 HttpWebResponse</summary> 
		Public Sub ExecuteAsync(callBack As Action(Of HttpWebResponse), Optional cancelAction As Func(Of Boolean) = Nothing)
			' 防止重复请求，上次操作未完成不再操作
			If _Status <> Model.HttpStatusEnum.UNKNOWN AndAlso _Status <> Model.HttpStatusEnum.FINISH Then Exit Sub

			StatusUpdate(Model.HttpStatusEnum.PREPARE, Nothing, Nothing)
			Dim Req = Create()
			StatusUpdate(Model.HttpStatusEnum.REQUEST, Req, Nothing)

			Req.BeginGetResponse(Sub(x)
									 Dim Res As HttpWebResponse
									 Try
										 Res = Req.EndGetResponse(x)
										 StatusUpdate(Model.HttpStatusEnum.RESPONSE, Req, Res)
									 Catch ex As WebException
										 Res = ex.Response
										 StatusUpdate(Model.HttpStatusEnum.FAILURE, Req, Res)
									 End Try

									 ResponseUpdate(Req, Res)

									 StatusUpdate(Model.HttpStatusEnum.FINISH, Req, Res)

									 callBack?.Invoke(Res)
								 End Sub, Nothing)

			' 是否取消操作
			If cancelAction?.Invoke Then Req.Abort()
		End Sub

		''' <summary>更新 Response</summary> 
		Private Sub ResponseUpdate(req As HttpWebRequest, res As HttpWebResponse)
			Response.Reset()

			If res Is Nothing Then Exit Sub

			Response.Url = req.RequestUri.OriginalString
			Response.CharacterSet = res.CharacterSet
			Response.ContentEncoding = res.ContentEncoding
			Response.ContentLength = res.ContentLength
			Response.ContentType = res.ContentType
			Response.StatusCode = res.StatusCode
			Response.StatusDescription = res.StatusDescription

			' Header
			If res.Headers?.Count > 0 Then
				Response.Headers = New NameValueDictionary
				Response.Headers.AddRangeFast(res.Headers)
			End If

			' COOKIES
			Try
				Response.Cookies = Request.Cookies.Update(res.Headers.Item(HttpResponseHeader.SetCookie))
			Catch ex As Exception
				Response.Cookies = Request.Cookies.Update(res.Cookies)
			End Try

			' 内容
			Response.Content = res.GetResponseStream
		End Sub

		''' <summary>更新操作状态</summary> 
		Private Sub StatusUpdate(status As Model.HttpStatusEnum, request As HttpWebRequest, response As HttpWebResponse)
			_Status = status
			RaiseEvent StatusChange(status, request, response)
		End Sub

	End Class

End Namespace
