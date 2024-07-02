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
' 	Http 请求
'
' 	name: Http.Request
' 	create: 2020-11-17
' 	memo: Http 请求
' 	
' ------------------------------------------------------------

Imports System.Net
Imports DaLi.Utils.Http.Model

Namespace Http

	''' <summary>Http 请求</summary>
	Public Class Request

		Private Const KEY_RAW = "_RAW_"

#Region "参数"

		''' <summary>获取或设置 Accept HTTP 标头的值。</summary>
		Public Accept As String = ""

		''' <summary>获取或设置一个值，该值指示请求是否应跟随重定向响应。</summary>
		Public AllowAutoRedirect As Boolean = True

		''' <summary>获取或设置表单提交类型。</summary>
		Public PostType As HttpPostEnum = HttpPostEnum.DEFAULT

		''' <summary>设置表单编码</summary>
		Public PostEncoding As Text.Encoding = UTF8

		''' <summary>获取或设置 Content-type HTTP 标头的值。</summary>
		Public ContentType As String = ""

		''' <summary>获取或设置请求将跟随的重定向的最大数目。系统默认：50</summary>
		Public MaximumAutomaticRedirections As Integer

		''' <summary>获取或设置请求的方法。</summary>
		Public Method As HttpMethodEnum = HttpMethodEnum.GET

		''' <summary>获取或设置请求的代理信息。</summary>
		Public Proxy As IWebProxy = Nothing

		''' <summary>是否使用代理服务器，True 则 Proxy 参数有效，未设置 Proxy 则使用系统全局的设置，全局也未设置则使用 IE 的代理； False 不使用代理。</summary>
		Public UseProxy As Boolean = False

		''' <summary>获取或设置 Referer HTTP 标头的值。</summary>
		Public Referer As String = ""

		''' <summary>获取或设置 GetResponse() 和 GetRequestStream() 方法的超时值（以毫秒为单位）。</summary>
		Public Timeout As Integer = 30000

		''' <summary>获取或设置 User-agent HTTP 标头的值。</summary>
		Public UserAgent As String = ""

		''' <summary>提交、上传的数据</summary>
		''' <remarks>Value - 内容，ValueType - 类型，Ext - 附加信息，如文件名</remarks>
		Public ReadOnly Content As New Dictionary(Of String, (Value As Object, ValueType As HttpFieldTypeEnum, Ext As Object))

		''' <summary>访问网址</summary>
		Public Url As String

		''' <summary>请求头部数据</summary>
		Public ReadOnly Headers As New WebHeaderCollection

		''' <summary>请求头部数据</summary>
		Public Cookies As New CookieCollection

#End Region

#Region "属性"

		''' <summary>是否存在需要上传的附件</summary>
		Public ReadOnly Property HasAttachments As Boolean
			Get
				Return Content.Any(Function(x) x.Value.ValueType <> HttpFieldTypeEnum.Default)
			End Get
		End Property

		''' <summary>获取原始内容</summary>
		Public Property RawContent As String
			Get
				Return Content.Where(Function(x) x.Key = KEY_RAW).Select(Function(x) x.Value.Value).FirstOrDefault
			End Get
			Set(value As String)
				If Content.ContainsKey(KEY_RAW) Then
					Content(KEY_RAW) = (value, HttpFieldTypeEnum.DEFAULT, Nothing)
				Else
					Content.Add(KEY_RAW, (value, HttpFieldTypeEnum.DEFAULT, Nothing))
				End If
			End Set
		End Property

		''' <summary>表单内容</summary>
		Public ReadOnly Property PostContent As Dictionary(Of String, Object)
			Get
				Return Content.Where(Function(x) x.Key <> KEY_RAW AndAlso x.Value.ValueType = HttpFieldTypeEnum.DEFAULT).ToDictionary(Function(x) x.Key, Function(x) x.Value.Value)
			End Get
		End Property

		''' <summary>获取序列化JSON后的内容</summary>
		Public ReadOnly Property JsonContent As String
			Get
				' 先从表单中获取，没有则返回 RAW
				Dim data = Content.Where(Function(x) x.Key <> KEY_RAW).ToDictionary(Function(x) x.Key, Function(x) x.Value.Value)
				If data.IsEmpty Then
					Return RawContent
				Else
					Return data.ToDeepDictionary.ToJson
				End If
			End Get
		End Property

		''' <summary>获取序列化 QueryString 后的内容</summary>
		Public ReadOnly Property QueryContent As String
			Get
				Dim Dic = New NameValueDictionary
				Dic.AddRangeFast(Content.Where(Function(x) x.Key <> KEY_RAW).ToDictionary(Function(x) x.Key, Function(x) x.Value.Value.ToString))
				Return Dic.ToQueryString(PostEncoding)
			End Get
		End Property

		''' <summary>设置头部数据</summary>
		Public Property Header(key As HttpRequestHeader) As String
			Get
				Return Headers(key)
			End Get
			Set(value As String)
				Headers(key) = value
			End Set
		End Property

		''' <summary>设置头部数据</summary>
		Public Property Header(key As String) As String
			Get
				Return Headers(key)
			End Get
			Set(value As String)
				Headers(key) = value
			End Set
		End Property

		''' <summary>获取提交的原始内容</summary>
		Public ReadOnly Property Data As String
			Get

				Select Case PostType
					Case HttpPostEnum.RAW
						' 上传原始数据
						Return RawContent

					Case Model.HttpPostEnum.JSON
						' 上传 JSON 请求
						' 对于包含 . 的键名，将转换到子键。如： a.b=xxx   =>   {a:{b:xx}}
						Return JsonContent

					Case Model.HttpPostEnum.MULTIPART
						' 上传附件，暂不支持转文本
						Return Nothing

					Case Else
						' 表单上传
						Return QueryContent
				End Select
			End Get
		End Property

		'''' <summary>设置提交字段数据</summary>
		'Public WriteOnly Property PostContent(key As String) As String
		'	Set(value As String)
		'		If key.NotEmpty AndAlso Not Content.ContainsKey(key) Then
		'			Content.Add(key, (value, HttpFieldType.Default))
		'		End If
		'	End Set
		'End Property

		'''' <summary>设置提交上传文件数据</summary>
		'Public WriteOnly Property FileContent(key As String) As String
		'	Set(path As String)
		'		If key.NotEmpty AndAlso Not Content.ContainsKey(key) AndAlso PathHelper.FileExist(path) Then
		'			Content.Add(key, (path, HttpFieldType.Path))
		'		End If
		'	End Set
		'End Property

		'''' <summary>设置提交 bytes 数据</summary>
		'Public WriteOnly Property DataContent(key As String) As Byte()
		'	Set(data As Byte())
		'		If key.NotEmpty AndAlso Not Content.ContainsKey(key) AndAlso data?.Length > 0 Then
		'			Content.Add(key, (data, HttpFieldType.Content))
		'		End If
		'	End Set
		'End Property

#End Region

#Region "事件"

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookies(strCookies As String, Optional domain As String = "")
			Cookies.Add(strCookies.ToCookies(domain))
		End Sub

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookies(lstCookies As CookieCollection)
			If lstCookies?.Count > 0 Then Cookies.Add(lstCookies)
		End Sub

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookie(name As String, value As String)
			If name.NotEmpty Then Cookies.Add(New Cookie(name, value))
		End Sub

		''' <summary>添加 Cookes 字符串</summary>
		Public Sub SetCookie(cookie As Cookie)
			If cookie IsNot Nothing Then Cookies.Add(cookie)
		End Sub

		'------------------------------------------------------------------

		''' <summary>清空字段</summary>
		Public Sub ClearContent()
			Content.Clear()
		End Sub

		''' <summary>移除字段</summary>
		Public Sub RemoveContent(name As String)
			If Content.ContainsKey(name) Then Content.Remove(name)
		End Sub

		''' <summary>获取字段</summary>
		Public Function GetContent(name As String) As (Value As Object, ValueType As HttpFieldTypeEnum, Ext As Object)
			If Content.ContainsKey(name) Then
				Return Content(name)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>设置字段数据</summary>
		Public Sub SetContent(name As String, value As Object, Optional type As HttpFieldTypeEnum = HttpFieldTypeEnum.DEFAULT, Optional ext As Object = Nothing)
			If Not Content.ContainsKey(name) Then Content.Add(name, (value, type, ext))
		End Sub

		''' <summary>设置提交字段数据</summary>
		Public Sub SetPostContent(name As String, value As String)
			SetContent(name, value)
		End Sub

		''' <summary>设置提交上传文件数据</summary>
		Public Sub SetFileContent(name As String, path As String, Optional fileName As String = "", Optional fileType As String = "")
			If PathHelper.FileExist(path) Then
				Dim ext = If(fileType.IsEmpty, fileName, New NameValueDictionary From {{"fileName", fileName}, {"fileType", fileType}})
				SetContent(name, path, HttpFieldTypeEnum.PATH, ext)
			End If
		End Sub

		''' <summary>设置提交 bytes 数据</summary>
		Public Sub SetDataContent(name As String, data As Byte(), Optional fileName As String = "", Optional fileType As String = "")
			If data?.Length > 0 Then
				Dim ext = If(fileType.IsEmpty, fileName, New NameValueDictionary From {{"fileName", fileName}, {"fileType", fileType}})
				SetContent(name, data, HttpFieldTypeEnum.CONTENT, ext)
			End If
		End Sub

		''' <summary>设置提交的原始内容</summary>
		Public Sub SetRawContent(value As String)
			RawContent = value
		End Sub

#End Region

#Region "相关操作"

		''' <summary>重新初始化参数</summary>
		Public Sub Reset()
			Url = ""
			Accept = ""
			AllowAutoRedirect = True
			ContentType = HttpPostEnum.Default
			MaximumAutomaticRedirections = 50
			Method = HttpMethodEnum.GET
			Proxy = Nothing
			UseProxy = False
			Referer = ""
			Timeout = 30000
			UserAgent = ""
			Content.Clear()
			Headers.Clear()
			Cookies = New CookieCollection
		End Sub

#End Region

	End Class
End Namespace