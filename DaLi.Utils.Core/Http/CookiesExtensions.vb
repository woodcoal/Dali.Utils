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
' 	Cookies 扩展
'
' 	name: Http.CookiesExtensions
' 	create: 2020-11-17
' 	memo: Cookies 扩展
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Http

	''' <summary>Cookies 扩展</summary>
	Public Module CookiesExtensions

		''' <summary>转换成 String</summary>
		<Extension>
		Public Function ToCookiesString(this As System.Net.CookieCollection) As String
			Dim Ret = ""

			If this?.Count > 0 Then
				With New Text.StringBuilder
					For Each Cookie As System.Net.Cookie In this
						.AppendFormat("{0}; expires={1}; domain={2}; path={3}{4}{5},", Cookie.ToString(), Cookie.Expires.ToUniversalTime.ToString("r"), Cookie.Domain, Cookie.Path, If(Cookie.HttpOnly, "; HttpOnly", ""), If(Cookie.Secure, "; secure", ""))
					Next

					If .Length > 0 Then .Length -= 1

					Ret = .ToString
				End With
			End If

			Return Ret
		End Function

		''' <summary>将 String 转换成 CookieCollection</summary>
		<Extension>
		Public Function ToCookies(this As String, Optional domain As String = "") As System.Net.CookieCollection
			Dim Ret As New System.Net.CookieCollection

			If this.NotEmpty Then
				this = this.Replace(vbCr, ",").Replace(vbLf, ",")

				Dim CookieList As String() = this.SplitDistinct()
				If CookieList?.Length > 0 Then
					Dim Keys As New List(Of String)

					For I As Integer = 0 To CookieList.Length - 1
						If Not String.IsNullOrWhiteSpace(CookieList(I)) Then
							If CookieList(I).IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0 AndAlso I < CookieList.Length - 1 Then
								Keys.Add(CookieList(I) & "," & CookieList(I + 1))
								I += 1
							Else
								Keys.Add(CookieList(I))
							End If
						End If
					Next

					For I As Integer = 0 To Keys.Count - 1
						If Not String.IsNullOrEmpty(Keys(I)) Then
							CookieList = Keys(I).Split(";"c)

							If CookieList?.Length > 0 Then
								Dim Cookie As New System.Net.Cookie
								For J As Integer = 0 To CookieList.Length - 1
									Dim CookieItem As String = CookieList(J)
									If Not String.IsNullOrWhiteSpace(CookieItem) Then
										If CookieItem.Contains("="c) Then
											Dim Name = CookieItem.Substring(0, CookieItem.IndexOf("="c))
											Dim Value = CookieItem.Substring(CookieItem.IndexOf("="c) + 1)

											If J = 0 Then
												Cookie.Name = Name
												Cookie.Value = Value
											ElseIf Not String.IsNullOrEmpty(Name) Then
												Select Case Name.Trim.ToLower
													Case "path"
														Cookie.Path = Value
													Case "domain"
														If String.IsNullOrEmpty(Value) Then
															Cookie.Domain = domain
														Else
															Cookie.Domain = Value
														End If
													Case "expires"
														Cookie.Expires = Value.ToDateTime
													Case "httponly"
														Cookie.HttpOnly = True
													Case "secure"
														Cookie.Secure = True
												End Select
											End If
										End If
									End If

									Ret.Add(Cookie)
								Next
							End If
						End If
					Next
				End If
			End If

			Return Ret
		End Function

		''' <summary>更新多个 CookieCollection，并组合成新的 CookieCollection</summary>
		<Extension>
		Public Function Update(this As IEnumerable(Of System.Net.Cookie)) As System.Net.CookieCollection
			Dim Ret As New System.Net.CookieCollection

			If this?.Count > 0 Then
				For Each Cookies In this
					If Cookies IsNot Nothing Then Ret.Add(Cookies)
				Next
			End If

			Return Ret
		End Function

		''' <summary>更新多个 CookieCollection，并组合成新的 CookieCollection</summary>
		<Extension>
		Public Function Update(this As IEnumerable(Of System.Net.CookieCollection)) As System.Net.CookieCollection
			Dim Ret As New System.Net.CookieCollection

			If this?.Count > 0 Then
				For Each Cookies In this
					If Cookies IsNot Nothing Then Ret.Add(Cookies)
				Next
			End If

			Return Ret
		End Function

		''' <summary>更新 Cookies</summary>
		<Extension>
		Public Function Update(this As System.Net.CookieCollection, ParamArray cookies() As System.Net.CookieCollection) As System.Net.CookieCollection
			Dim Ret As New System.Net.CookieCollection

			If this?.Count > 0 Then Ret.Add(this)
			If cookies?.Length > 0 Then
				For Each Cook In cookies
					Ret.Add(Cook)
				Next
			End If

			Return Ret
		End Function

		''' <summary>更新 Cookies</summary>
		<Extension>
		Public Function Update(this As System.Net.CookieCollection, ParamArray cooks() As System.Net.Cookie) As System.Net.CookieCollection
			Dim Ret As New System.Net.CookieCollection

			If this?.Count > 0 Then Ret.Add(this)
			If cooks?.Length > 0 Then
				For Each Cook In cooks
					Ret.Add(Cook)
				Next
			End If

			Return Ret
		End Function

		''' <summary>更新 Cookies</summary>
		<Extension>
		Public Function Update(this As System.Net.CookieCollection, strCookies As String, Optional strDomain As String = "") As System.Net.CookieCollection
			Return Update({this, strCookies.ToCookies(strDomain)})
		End Function

	End Module

End Namespace