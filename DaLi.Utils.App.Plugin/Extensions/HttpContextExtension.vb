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
' 	HttpContext 扩展
'
' 	name: Extension.HttpContextExtension
' 	create: 2023-02-14
' 	memo: HttpContext 扩展
'
' ------------------------------------------------------------

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Primitives

Namespace Extension

	''' <summary>HttpContext 扩展</summary>
	Public Module HttpContextExtension
		Private Const CONTENT_DATA = "_Context_Data_"
		Private Const CONTENT_BODY = "_Context_Body_"

#Region "获取请求的值"

		''' <summary>获取所有请求数据</summary>
		<Extension>
		Public Function RequestData(this As HttpContext) As KeyValueDictionary
			Return If(this?.Request.RequestData, New KeyValueDictionary)
		End Function

		''' <summary>获取所有请求数据</summary>
		<Extension>
		Public Function RequestData(this As HttpRequest) As KeyValueDictionary
			If this IsNot Nothing Then
				' 从请求缓存数据中获取
				Dim result = this.HttpContext.ContextItem(Of KeyValueDictionary)(CONTENT_DATA)
				If result IsNot Nothing Then Return result

				' 不存在，分析
				result = New KeyValueDictionary

				Dim Forms = this.FormEx
				If Forms.NotEmpty Then
					For Each Q In Forms
						result.Add(Q.Key, Q.Value.ToString)
					Next
				Else
					result.AddRange(this.Json)
				End If

				If this.Query.NotEmpty Then
					For Each Q In this.Query
						result.Add(Q.Key, Q.Value.ToString)
					Next
				End If

				this.HttpContext.ContextItem(CONTENT_DATA, result)
				Return result
			End If

			Return New KeyValueDictionary
		End Function

		''' <summary>获取头部值</summary>
		<Extension>
		Public Function Header(this As HttpRequest, name As String) As String
			If this Is Nothing OrElse name.IsEmpty Then Return Nothing

			Return this.Headers.
				Where(Function(x) x.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).
				Select(Function(x) x.Value).
				FirstOrDefault.ToString
		End Function

		''' <summary>获取 Cookies 值</summary>
		<Extension>
		Public Function Cookie(this As HttpRequest, name As String) As String
			If this Is Nothing OrElse name.IsEmpty Then Return Nothing

			Return this.Cookies.
				Where(Function(x) x.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).
				Select(Function(x) x.Value).
				FirstOrDefault
		End Function

		''' <summary>获取 HttpContext 值</summary>
		<Extension>
		Public Function ContextItem(Of T)(this As HttpContext, name As Object) As T
			If this Is Nothing OrElse name Is Nothing OrElse Not this.Items.ContainsKey(name) Then Return Nothing
			Return ChangeType(Of T)(this.Items(name))
		End Function

		''' <summary>设置 HttpContext 值</summary>
		<Extension>
		Public Sub ContextItem(this As HttpContext, name As Object, value As Object)
			If this Is Nothing OrElse name Is Nothing Then Return

			If this.Items.ContainsKey(name) Then
				this.Items(name) = value
			Else
				this.Items.Add(name, value)
			End If
		End Sub

		''' <summary>HttpRequest 请求的内容</summary>
		<Extension>
		Public Function Content(this As HttpRequest) As String
			If this Is Nothing OrElse this.Body Is Nothing OrElse Not this.Body.CanRead Then Return Nothing

			Dim Value = this.HttpContext.ContextItem(Of String)(CONTENT_BODY)
			If Value.NotNull Then Return Value

			Try
				'this.EnableBuffering
				this.Body.Position = 0
				Value = New StreamReader(this.Body).ReadToEndAsync.Result.EmptyValue
				this.Body.Position = 0

				this.HttpContext.ContextItem(CONTENT_BODY, Value)
			Catch ex As Exception
			End Try

			Return Value
		End Function

		''' <summary>HttpRequest Json 请求数据，仅 POST PUT PATCH 才有效</summary>
		<Extension>
		Public Function Json(this As HttpRequest) As IDictionary(Of String, Object)
			If this IsNot Nothing AndAlso this.HasJsonContentType Then
				Try
					Return this.Content.ToJsonDictionary
				Catch ex As Exception
				End Try
			End If

			Return Nothing
		End Function

		''' <summary>HttpRequest Json 请求数据，仅 POST PUT PATCH 才有效</summary>
		<Extension>
		Public Function Json(this As HttpRequest, key As String) As String
			If key.NotEmpty Then
				Return this.Json?.Item(key)
			End If

			Return ""
		End Function

		''' <summary>HttpRequest.Form 异常修正</summary>
		<Extension>
		Public Function FormEx(this As HttpRequest) As IDictionary(Of String, StringValues)

			If this IsNot Nothing AndAlso
				this.HasFormContentType AndAlso
				this.Form.NotEmpty AndAlso
				this.Form.Files.IsEmpty Then
				Return this.Form.ToDictionary(Function(x) x.Key, Function(x) x.Value)
			Else
				Return New Dictionary(Of String, StringValues)
			End If
		End Function

		''' <summary>HttpRequest.Form 异常修正</summary>
		<Extension>
		Public Function FormEx(this As HttpRequest, key As String) As StringValues
			If key.NotEmpty Then
				Dim Form = this.FormEx
				If Form.NotEmpty AndAlso Form.ContainsKey(key) Then Return Form(key)
			End If

			Return ""
		End Function

		''' <summary>获取请求数据的值，POST => JSON => QUERY => COOKIES => HEADERS</summary>
		<Extension>
		Public Function RequestValue(this As HttpContext, name As String) As String
			Return this?.Request.RequestValue(name)
		End Function

		''' <summary>获取请求数据的值，POST => JSON => QUERY => COOKIES => HEADERS</summary>
		<Extension>
		Public Function RequestValue(this As HttpRequest, name As String) As String
			If this Is Nothing OrElse name.IsEmpty Then Return ""

			Dim value As String = this.RequestData(name)
			If value.IsEmpty Then
				value = this.Cookie(name)
				If value.IsEmpty Then
					value = this.Header(name)
				End If
			End If

			Return value
		End Function

#End Region

#Region "从请求的值中获取数据"

		''' <summary>当前访问地址</summary>
		<Extension>
		Public Function Url(this As HttpRequest) As String
			Dim Value = ""

			If this IsNot Nothing Then
				' 页面参数
				Dim sb As New Text.StringBuilder
				sb.Append(this.Scheme).Append("://").Append(this.Host)
				sb.Append(this.PathBase).Append(this.Path)
				sb.Append(this.QueryString)

				Value = sb.ToString
			End If

			Return Value
		End Function

		''' <summary>当前访问地址</summary>
		<Extension>
		Public Function Url(this As HttpContext) As String
			Return this?.Request.Url
		End Function

		''' <summary>获取 UserAgent</summary>
		<Extension>
		Public Function UserAgent(this As HttpRequest) As String
			Return this.Header("User-Agent")
		End Function

		''' <summary>获取 UserAgent</summary>
		<Extension>
		Public Function UserAgent(this As HttpContext) As String
			Return this?.Request.UserAgent
		End Function

		''' <summary>获取IP</summary>
		<Extension>
		Public Function IP(this As HttpRequest, Optional onlyIPv4 As Boolean = False) As String
			If this IsNot Nothing Then
				Dim isIP = Function(str As String) As Boolean
							   If onlyIPv4 Then
								   Return str.IsIPv4
							   Else
								   Return str.IsIP
							   End If
						   End Function

				Dim Value = this.HttpContext?.ContextItem(Of String)("IP")
				If isIP(Value) Then Return Value

				' 无缓存，获取 IP
				Dim isEmpty = Function(str As String) str.IsEmpty OrElse str.IsSame("unknown")

				Value = this.Header("X-Real-IP")
				If isEmpty(Value) Then
					Value = this.Header("X-Forwarded-For")
					If Not isEmpty(Value) Then Value = Value.Split(","c).LastOrDefault.ToString

					If isEmpty(Value) Then Value = this.HttpContext?.Connection.RemoteIpAddress.ToString
				End If

				If isIP(Value) Then
					this.HttpContext?.ContextItem("IP", Value)
					Return Value
				End If
			End If

			Return ""
		End Function

		''' <summary>获取IP</summary>
		<Extension>
		Public Function IP(this As HttpContext) As String
			Return this?.Request.IP
		End Function

		''' <summary>获取所有相关 IP</summary>
		''' <param name="removeLocalIP">是否移除本地 IP</param>
		<Extension>
		Public Function IPs(this As HttpRequest, Optional removeLocalIP As Boolean = True) As String()
			Dim data = this.Header("X-Real-IP") & "," & this.Header("X-Forwarded-For") & "," & this.HttpContext?.Connection.RemoteIpAddress.ToString

			Return data.Split(","c, StringSplitOptions.RemoveEmptyEntries).
				Select(Function(x) x.Trim).
				Distinct.
				Where(Function(x) x.IsIP AndAlso (Not removeLocalIP OrElse NetHelper.IsPublicIPv4(x))).
				ToArray
		End Function

		''' <summary>获取所有相关IP</summary>
		<Extension>
		Public Function IPs(this As HttpContext) As String()
			Return this?.Request.IPs
		End Function

		''' <summary>获取 Token 参数，顺序获取：Post / Get / Header / Cookies</summary>
		''' <param name="tokenFieldName">token 字段名称，按顺序获取</param>
		<Extension>
		Public Function Token(this As HttpRequest, ParamArray tokenFieldName() As String) As String
			' 查询字段
			If tokenFieldName.IsEmpty Then
				tokenFieldName = {"Authorization", "Token"}
			Else
				tokenFieldName = tokenFieldName.Union({"Authorization", "Token"}).Distinct(Function(x) x.ToLower).ToArray
			End If

			For Each field In tokenFieldName
				Dim value = this.RequestValue(field)
				If value.NotEmpty Then Return If(value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase), value.Substring(7).Trim, value.Trim)
			Next

			Return ""
		End Function

		''' <summary>移除数据中加密信息</summary>
		<Extension>
		Public Function HiddenPassword(Of T)(this As IDictionary(Of String, T)) As NameValueDictionary
			If this.IsEmpty Then Return Nothing

			Dim ret As New NameValueDictionary
			For Each item In this
				Dim value = item.Value?.ToString

				If item.Key.Like("authorization") Then
					ret.Add(item.Key, value.ShortShow(50))

				ElseIf item.Key.Like("key") Then
					ret.Add(item.Key, value.ShortShow(16))

				ElseIf item.Key.Like("*token*") Then
					ret.Add(item.Key, value.ShortShow(30))

				ElseIf item.Key.Like("*password*") Then
					ret.Add(item.Key, value.ShortShow(6))

				ElseIf TypeOf item.Value Is StringValues Then
					ret.Add(item.Key, value)

				Else
					ret.Add(item.Key, Utils.Extension.ToObjectString(item.Value))
				End If
			Next

			Return ret
		End Function
#End Region

	End Module

End Namespace