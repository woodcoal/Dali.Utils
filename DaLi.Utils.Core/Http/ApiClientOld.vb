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
' 	create: 2020-11-17
' 	memo: WebApi 操作客户端
' 	
' ------------------------------------------------------------

Imports System.Net
Imports System.Reflection
Imports DaLi.Utils.Helper.SecurityHelper
Imports DaLi.Utils.Http.Model

Namespace Http

	''' <summary>WebApi 操作客户端</summary>
	<Obsolete("旧版 ApiClient，已弃用，请使用新版 ApiClient")>
	Public Class ApiClientOld

		''' <summary>请求数据</summary>
		Public ReadOnly Request As New Request

#Region "参数"

		''' <summary>基础地址，如：http(s)://Host/api/v2/</summary>
		Public BaseURL As String

		''' <summary>交表单编码方式</summary>
		Public Encoding As Text.Encoding

		''' <summary>JWT Authorization Token</summary> 
		Public Token As String

		''' <summary>当前应用标识</summary> 
		Public AppID As String

		''' <summary>客户端标识</summary> 
		Public ClientID As Long

		''' <summary>客户端密匙</summary> 
		Public ClientKey As Guid

		''' <summary>客户端版本</summary> 
		Public ClientVersion As Single?

		''' <summary>客户端加密算法</summary> 
		Public ClientValidate As ClientSignEnum

		''' <summary>超时时长，单位：毫秒</summary>
		Public Property Timeout As Integer
			Get
				Return Request.Timeout
			End Get
			Set(value As Integer)
				Request.Timeout = value
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

		'----------------------------------------------------------------

		''' <summary>获取或设置Cookie</summary> 
		Public Cookies As CookieCollection

		''' <summary>获取或设置Cooikes字符串</summary> 
		Public Property CookieString As String
			Get
				Return Cookies.ToCookiesString
			End Get
			Set(value As String)
				Cookies.Update(value)
			End Set
		End Property

		'------------------------------------------------------------------

		''' <summary>设置请求头部数据</summary>
		Public Sub SetHeader(key As HttpRequestHeader, value As String)
			Request.Header(key) = value
		End Sub

		''' <summary>设置请求头部数据</summary>
		Public Sub SetHeader(key As String, value As String)
			Request.Header(key) = value
		End Sub

		'------------------------------------------------------------------

		''' <summary>获取系统状态</summary>
		Public StatusCode As HttpStatusCode

#End Region

#Region "请求操作"

		Public Function Execute(method As HttpMethodEnum, path As String) As String
			Dim Url = NetHelper.AbsoluteUrl(BaseURL, path)

			If Url.IsUrl Then
				Request.Url = Url
				Request.Cookies = Cookies
				Request.Method = method

				If Token.NotEmpty Then Request.Header(Net.HttpRequestHeader.Authorization) = "Bearer " & Token

				' 加入客户端信息
				If ClientID.NotEmpty AndAlso ClientKey.NotEmpty Then
					Dim signDatas As New KeyValueDictionary

					Dim U As New Uri(Url)
					Dim signPath = U.AbsolutePath
					Dim signQuery = NameValueDictionary.FromQueryString(U.Query)
					If signQuery.NotEmpty Then signQuery.ForEach(Sub(k, v) signDatas.Add(k, v))

					signDatas.UpdateRange(Request.PostContent)

					Dim rawContent = Request.RawContent.ToJsonDictionary
					signDatas.UpdateRange(rawContent)

					Dim ticks = SYS_NOW_DATE.JsTicks
					Dim code = Guid.NewGuid.ToString
					Dim version = If(ClientVersion, Assembly.GetEntryAssembly.Version)

					SetHeader("x-client-id", ClientID.ToString)
					SetHeader("x-client-app", AppID)
					SetHeader("x-client-key", Sign.Client(ClientValidate, ClientID, ClientKey, AppID, method.Name, signPath, signDatas, ticks, code))
					SetHeader("x-client-ver", version.ToString)
					SetHeader("x-client-date", ticks.ToString)
					SetHeader("x-client-code", code)
				End If

				Dim Ret = ""

				Try
					Dim Client = New Client(Request)
					Using Client.Execute()
						With Client.Response
							StatusCode = .StatusCode
							Cookies = .Cookies
							Ret = .StringContent(Encoding)
						End With
					End Using

					Return Ret
				Catch ex As Exception
				End Try
			End If

			Return ""
		End Function

		Public Function Execute(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True) As String
			Request.PostType = If(isJson, HttpPostEnum.JSON, HttpPostEnum.DEFAULT)
			Request.Content.Clear()

			If data?.Count > 0 Then
				If isJson Then
					Request.SetRawContent(data.ToJson)
				Else
					For Each kv In data
						Request.SetPostContent(kv.Key, kv.Value?.ToString)
					Next
				End If
			End If

			Return Execute(method, path)
		End Function

		Public Function Execute(method As HttpMethodEnum, path As String, json As String) As String
			Request.PostType = HttpPostEnum.RAW
			Request.ContentType = "application/json"
			Request.Content.Clear()
			Request.SetRawContent(json)

			Return Execute(method, path)
		End Function

		Public Function Execute(Of T)(method As HttpMethodEnum, path As String) As ApiResult(Of T)
			Return Execute(method, path).ToJsonObject(Of ApiResult(Of T))
		End Function

		Public Function Execute(Of T)(method As HttpMethodEnum, path As String, data As IDictionary, Optional isJson As Boolean = True) As ApiResult(Of T)
			Return Execute(method, path, data, isJson).ToJsonObject(Of ApiResult(Of T))
		End Function

		Public Function Execute(Of T)(method As HttpMethodEnum, path As String, json As String) As ApiResult(Of T)
			Return Execute(method, path, json).ToJsonObject(Of ApiResult(Of T))
		End Function


		'------------------------------------------------------------------------

		''' <summary>GET请求</summary> 
		Public Function [GET](path As String, Optional queryList As NameValueDictionary = Nothing) As String
			Return Execute(HttpMethodEnum.GET, NetHelper.NameValue2QueryString(queryList, path))
		End Function

		''' <summary>GET请求</summary> 
		Public Function [GET](Of T)(path As String, Optional queryList As NameValueDictionary = Nothing) As ApiResult(Of T)
			Return Execute(Of T)(HttpMethodEnum.GET, NetHelper.NameValue2QueryString(queryList, path))
		End Function

		''' <summary>POST请求</summary> 
		Public Function POST(path As String, data As IDictionary, Optional isJson As Boolean = True) As String
			Return Execute(HttpMethodEnum.POST, path, data, isJson)
		End Function

		''' <summary>POST请求</summary> 
		Public Function POST(Of T)(path As String, data As IDictionary, Optional isJson As Boolean = True) As ApiResult(Of T)
			Return Execute(Of T)(HttpMethodEnum.POST, path, data, isJson)
		End Function

		''' <summary>POST请求</summary> 
		Public Function POST(path As String, json As String) As String
			Return Execute(HttpMethodEnum.POST, path, json)
		End Function

		''' <summary>POST请求</summary> 
		Public Function POST(Of T)(path As String, json As String) As ApiResult(Of T)
			Return Execute(Of T)(HttpMethodEnum.POST, path, json)
		End Function

		''' <summary>PUT请求</summary> 
		Public Function PUT(path As String, data As IDictionary, Optional isJson As Boolean = True) As String
			Return Execute(HttpMethodEnum.PUT, path, data, isJson)
		End Function

		''' <summary>PUT请求</summary> 
		Public Function PUT(Of T)(path As String, data As IDictionary, Optional isJson As Boolean = True) As ApiResult(Of T)
			Return Execute(Of T)(HttpMethodEnum.PUT, path, data, isJson)
		End Function

		''' <summary>PUT请求</summary> 
		Public Function PUT(path As String, json As String) As String
			Return Execute(HttpMethodEnum.PUT, path, json)
		End Function

		''' <summary>PUT请求</summary> 
		Public Function PUT(Of T)(path As String, json As String) As ApiResult(Of T)
			Return Execute(Of T)(HttpMethodEnum.PUT, path, json)
		End Function

		''' <summary>DELETE请求</summary> 
		Public Function DELETE(path As String, Optional queryList As NameValueDictionary = Nothing) As String
			Return Execute(HttpMethodEnum.DELETE, NetHelper.NameValue2QueryString(queryList, path))
		End Function

		''' <summary>DELETE请求</summary> 
		Public Function DELETE(Of T)(path As String, Optional queryList As NameValueDictionary = Nothing) As ApiResult(Of T)
			Return Execute(Of T)(HttpMethodEnum.DELETE, NetHelper.NameValue2QueryString(queryList, path))
		End Function

		''' <summary>重置参数</summary> 
		Public Sub Reset()
			Request.Reset()
			Cookies = Nothing
		End Sub

#End Region

	End Class

End Namespace
