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
' 	Http 操作客户端
'
' 	name: Http.HttpClient
' 	create: 2020-11-17
' 	memo: Http 操作客户端
' 	
' ------------------------------------------------------------

Imports System.IO
Imports System.Net
Imports System.Text
Imports DaLi.Utils.Http.Model

Namespace Http

	''' <summary>Http 操作客户端</summary>
	Public Class HttpClient
		Inherits Client
		Implements IDisposable

		Private _HttpResponse As HttpWebResponse

#Region "参数"

		''' <summary>获取或设置一个值，该值指示请求是否应跟随重定向响应。</summary>
		Public Property AllowAutoRedirect As Boolean
			Get
				Return Request.AllowAutoRedirect
			End Get
			Set(value As Boolean)
				Request.AllowAutoRedirect = value
			End Set
		End Property

		''' <summary>获取或设置 Content-type HTTP 标头的值。</summary>
		Public Property PostType As HttpPostEnum
			Get
				Return Request.PostType
			End Get
			Set(value As HttpPostEnum)
				Request.PostType = value
			End Set
		End Property

		''' <summary>设置表单编码</summary>
		Public Property PostEncoding As Encoding
			Get
				Return Request.PostEncoding
			End Get
			Set(value As Encoding)
				Request.PostEncoding = value
			End Set
		End Property

		''' <summary>
		''' 设置 / 获取响应的内容类型。
		''' 设置为 Request 头
		''' 获取为 Response 头
		''' </summary>
		Public Property ContentType As String
			Get
				Return Response.ContentType
			End Get
			Set(value As String)
				Request.ContentType = value
			End Set
		End Property

		''' <summary>获取或设置请求的方法。</summary>
		Public Property Method As HttpMethodEnum
			Get
				Return Request.Method
			End Get
			Set(value As HttpMethodEnum)
				Request.Method = value
			End Set
		End Property

		''' <summary>获取或设置 Referer HTTP 标头的值。</summary>
		Public Property Referer As String
			Get
				Return Request.Referer
			End Get
			Set(value As String)
				Request.Referer = value
			End Set
		End Property

		''' <summary>获取或设置 User-agent HTTP 标头的值。</summary>
		Public Property UserAgent As String
			Get
				Return Request.UserAgent
			End Get
			Set(value As String)
				Request.UserAgent = value
			End Set
		End Property

		''' <summary>访问网址</summary>
		Public Property Url As String
			Get
				Return Request.Url
			End Get
			Set(value As String)
				Request.Url = value
			End Set
		End Property

		''' <summary>超时时长，单位：毫秒</summary>
		Public Property Timeout As Integer
			Get
				Return Request.Timeout
			End Get
			Set(value As Integer)
				Request.Timeout = value
			End Set
		End Property

		''' <summary>请求头部数据</summary>
		Public ReadOnly Property RequestHeaders As WebHeaderCollection
			Get
				Return Request.Headers
			End Get
		End Property

		'----------------------------------------------------------------

		''' <summary>设置字符方式请求名称，如：GET POST</summary>
		Public Sub SetMethod(method As String)
			Select Case method.EmptyValue("GET").ToUpper
				Case "GET"
					Me.Method = HttpMethodEnum.GET

				Case "POST"
					Me.Method = HttpMethodEnum.POST

				Case "PUT"
					Me.Method = HttpMethodEnum.PUT

				Case "PATCH"
					Me.Method = HttpMethodEnum.PATCH

				Case "DELETE"
					Me.Method = HttpMethodEnum.DELETE

				Case "OPTIONS"
					Me.Method = HttpMethodEnum.OPTIONS

				Case "HEAD"
					Me.Method = HttpMethodEnum.HEAD

				Case "TRACE"
					Me.Method = HttpMethodEnum.TRACE

				Case Else
					Me.Method = HttpMethodEnum.GET
			End Select
		End Sub

		'----------------------------------------------------------------

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookies(strCookies As String, Optional domain As String = "")
			Request.SetCookies(strCookies, domain)
		End Sub

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookies(lstCookies As CookieCollection)
			Request.SetCookies(lstCookies)
		End Sub

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookie(name As String, value As String)
			Request.SetCookie(name, value)
		End Sub

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookie(cookie As Cookie)
			Request.SetCookie(cookie)
		End Sub

		'------------------------------------------------------------------

		''' <summary>清空字段</summary>
		Public Sub ClearContent()
			Request.ClearContent()
		End Sub

		''' <summary>移除字段</summary>
		Public Sub RemoveContent(name As String)
			Request.RemoveContent(name)
		End Sub

		''' <summary>获取字段</summary>
		Public Function GetContent(name As String) As (Value As Object, ValueType As HttpFieldTypeEnum, Ext As Object)
			Return Request.GetContent(name)
		End Function

		''' <summary>设置字段数据</summary>
		Public Sub SetContent(name As String, value As Object, Optional type As HttpFieldTypeEnum = HttpFieldTypeEnum.DEFAULT, Optional ext As Object = Nothing)
			Request.SetContent(name, value, type, ext)
		End Sub

		''' <summary>设置提交字段数据</summary>
		Public Sub SetPostContent(name As String, value As String)
			Request.SetPostContent(name, value)
		End Sub

		''' <summary>设置提交上传文件数据</summary>
		Public Sub SetFileContent(name As String, path As String, Optional fileName As String = "", Optional fileType As String = "")
			Request.SetFileContent(name, path, fileName, fileType)
		End Sub

		''' <summary>设置提交 bytes 数据</summary>
		Public Sub SetDataContent(name As String, data As Byte(), Optional fileName As String = "", Optional fileType As String = "")
			Request.SetDataContent(name, data, fileName, fileType)
		End Sub

		''' <summary>设置提交原始字符串（json）</summary>
		Public Sub SetRawContent(data As String)
			Request.SetRawContent(data)
		End Sub

		'------------------------------------------------------------------

		''' <summary>设置请求头部数据</summary>
		Public Sub SetHeader(key As HttpRequestHeader, value As String)
			Request.Header(key) = value
		End Sub

		''' <summary>设置请求头部数据</summary>
		Public Sub SetHeader(key As String, value As String)
			Request.Header(key) = value
		End Sub


#End Region

#Region "结果"

		''' <summary>获取头部数据</summary>
		Public Function GetHeader(key As String) As String
			Return Response.Header(key)
		End Function

		'------------------------------------------------------------------

		''' <summary>获取 Cookies</summary>
		Public Function GetCookies() As CookieCollection
			Return Response.Cookies
		End Function

		'------------------------------------------------------------------

		''' <summary>获取数据流内容。注意：如果数据流已经被读取则将丢失</summary>
		Public Function GetStream() As Stream
			Return Response.Content
		End Function

		''' <summary>字节类型内容</summary>
		Public Function GetBytes() As Byte()
			Return Response.BytesContent
		End Function

		''' <summary>网页类型内容（自动分析编码）</summary>
		Public Function GetHtml(codeName As String) As String
			Dim encoding As Encoding = Nothing

			If codeName.NotEmpty Then
				Try
					EncodingRegister()
					encoding = Encoding.GetEncoding(codeName)
				Catch ex As Exception
				End Try
			End If

			Return GetHtml(encoding)
		End Function

		''' <summary>网页类型内容（自动分析编码）</summary>
		Public Function GetHtml(codepage As Integer) As String
			Dim encoding As Encoding = Nothing

			If codepage > 0 Then
				Try
					EncodingRegister()
					encoding = Encoding.GetEncoding(codepage)
				Catch ex As Exception
				End Try
			End If

			Return GetHtml(encoding)
		End Function

		''' <summary>网页类型内容（自动分析编码）</summary>
		Public Function GetHtml(Optional encoding As Text.Encoding = Nothing) As String
			Return Response.HtmlContent(encoding)
		End Function

		''' <summary>字符串类型内容</summary>
		Public Function GetString(Optional encoding As Text.Encoding = Nothing) As String
			Return Response.StringContent(encoding)
		End Function

		''' <summary>字符串类型内容</summary>
		Public Function GetJson(Of T)(Optional encoding As Text.Encoding = Nothing) As T
			Return GetString(encoding).ToJsonObject(Of T)
		End Function

		''' <summary>字符串类型内容</summary>
		Public Function GetJson(Optional encoding As Text.Encoding = Nothing) As Object
			Return GetString(encoding).ToJsonCollection.Value
		End Function

		''' <summary>Json 类型内容</summary>
		Public Function ToJsonObject(type As Type, Optional encoding As Text.Encoding = Nothing) As Object
			Return Response.JsonContent(type, encoding)
		End Function

		''' <summary>保存附件</summary>
		''' <param name="fileFloder">文件保存的路径</param>
		''' <param name="fileExists">文件存在的操作</param>
		''' <param name="fileName">文件名</param>
		Public Function SaveFile(fileFloder As String, Optional fileExists As ExistsActionEnum = ExistsActionEnum.CANCEL, Optional fileName As String = "") As (Path As String, Flag As Boolean)
			Return Response.FileContent(fileFloder, fileExists, fileName)
		End Function

		'------------------------------------------------------------------

		''' <summary>获取当前跳转地址。</summary>
		Public ReadOnly Property Location As String
			Get
				Return Response.Location
			End Get
		End Property

		''' <summary>获取响应的字符集。</summary>
		Public ReadOnly Property CharacterSet As String
			Get
				Return Response.CharacterSet
			End Get
		End Property

		''' <summary>获取用于对响应体进行编码的方法。</summary>
		Public ReadOnly Property ContentEncoding As String
			Get
				Return Response.ContentEncoding
			End Get
		End Property

		''' <summary>获取请求返回的内容的长度。</summary>
		Public ReadOnly Property ContentLength As Long
			Get
				Return Response.ContentLength
			End Get
		End Property

		''' <summary>获取来自服务器的与此响应关联的标头。</summary>
		Public ReadOnly Property ResponseHeaders As NameValueDictionary
			Get
				Return Response.Headers
			End Get
		End Property

		''' <summary>获取响应的状态。</summary>
		Public ReadOnly Property StatusCode As HttpStatusCode
			Get
				Return Response.StatusCode
			End Get
		End Property

		''' <summary>获取与响应一起返回的状态说明。</summary>
		Public ReadOnly Property StatusDescription As String
			Get
				Return Response.StatusDescription
			End Get
		End Property

		''' <summary>获取的内容。</summary>
		Public ReadOnly Property Content As Stream
			Get
				Return Response.Content
			End Get
		End Property

#End Region

#Region "操作"

		Public Sub Reset()
			Request.Reset()
			Response.Reset()
			_HttpResponse?.Dispose()
		End Sub

		''' <summary>执行操作</summary>
		''' <param name="relativePath">相对网址或者需要直接访问的网址</param>
		Public Overloads Function Execute(Optional relativePath As String = "") As HttpClient
			Url = NetHelper.AbsoluteUrl(Url, relativePath)

			_HttpResponse?.Dispose()
			_HttpResponse = MyBase.Execute

			Return Me
		End Function

#End Region

		Public Sub Dispose() Implements IDisposable.Dispose
			_HttpResponse?.Dispose()
			GC.SuppressFinalize(Me)
		End Sub

	End Class

End Namespace
