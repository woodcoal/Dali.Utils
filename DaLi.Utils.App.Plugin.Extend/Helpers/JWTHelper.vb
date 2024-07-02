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
' 	JWT 操作
'
' 	name: Helper.JWTHelper
' 	create: 2023-02-27
' 	memo: JWT 操作
'
' ------------------------------------------------------------

Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Microsoft.AspNetCore.Http
Imports Microsoft.IdentityModel.Tokens

Namespace Helper
	''' <summary>JWT 操作</summary>
	Public NotInheritable Class JWTHelper

		' Authorization: Bearer <token>
		' Cookies: token=

		Public Shared [Default] As Lazy(Of JWTHelper)

#Region "公共参数"

		''' <summary>JWT 设置参数</summary>
		Protected ReadOnly Setting As IJWTSetting

		''' <summary>默认储存 JWT Token 的字段，默认已包含 Authorization</summary>
		Public Shared TokenName As String

		''' <summary>JWT 管理器</summary>
		Protected ReadOnly Handler As JwtSecurityTokenHandler

		''' <summary>JWT 签名证书</summary>
		Private _SigningCredentials As SigningCredentials

		''' <summary>JWT 签名证书</summary>
		Protected ReadOnly Property SigningCredentials As SigningCredentials
			Get
				If _SigningCredentials Is Nothing Then
					Dim Key = New SymmetricSecurityKey(Text.Encoding.UTF8.GetBytes(Setting.SecurityKey))
					_SigningCredentials = New SigningCredentials(Key, "HS256")
				End If

				Return _SigningCredentials
			End Get
		End Property

		''' <summary>JWT 验证参数</summary>
		Private _ValidationParameters As TokenValidationParameters

		''' <summary>JWT 验证参数</summary>
		Protected ReadOnly Property ValidationParameters As TokenValidationParameters
			Get
				If _ValidationParameters Is Nothing Then
					_ValidationParameters = New TokenValidationParameters

					' 是否验证 Issuer
					If Setting.Issuer.IsEmpty Then
						_ValidationParameters.ValidateIssuer = False
					Else
						_ValidationParameters.ValidateIssuer = True
						_ValidationParameters.ValidIssuer = Setting.Issuer
					End If

					' 是否验证 Audience
					If Setting.Audience.IsEmpty Then
						_ValidationParameters.ValidateAudience = False
					Else
						_ValidationParameters.ValidateAudience = True
						_ValidationParameters.ValidAudience = Setting.Audience
					End If

					' 是否验证 SecurityKey
					_ValidationParameters.ValidateIssuerSigningKey = True
					_ValidationParameters.IssuerSigningKey = New SymmetricSecurityKey(Text.Encoding.UTF8.GetBytes(Setting.SecurityKey))

					' 是否验证失效时间,ClockSkew 必须加上，否则无法验证
					_ValidationParameters.RequireExpirationTime = True
					_ValidationParameters.ValidateLifetime = True
					_ValidationParameters.ClockSkew = TimeSpan.Zero
				End If

				Return _ValidationParameters
			End Get
		End Property

		''' <summary>初始化构造</summary>
		Public Sub New()
			TokenName = TokenName.EmptyValue("Authorization")
			Setting = SYS.GetSetting(Of IJWTSetting)
			Handler = New JwtSecurityTokenHandler
		End Sub

#End Region

#Region "创建 Token"

		''' <summary>生成 Token</summary>
		Public Function CreateToken(values As NameValueDictionary, Optional expired As Integer = 0) As String
			Dim claimsIdentity As New ClaimsIdentity
			values.ForEach(Sub(k, v) claimsIdentity.AddClaim(New Claim(k, v.EmptyValue)))

			If expired < 1 Then expired = Setting.Expires
			If expired < 1 Then expired = 1

			Return Handler.CreateEncodedJwt(New SecurityTokenDescriptor With {
				.Issuer = Setting.Issuer,
				.Audience = Setting.Audience,
				.Subject = claimsIdentity,
				.NotBefore = SYS_NOW_DATE.AddMinutes(-1),
				.Expires = SYS_NOW_DATE.AddMinutes(expired),
				.IssuedAt = SYS_NOW_DATE,
				.SigningCredentials = SigningCredentials
			})
		End Function

		''' <summary>生成 Token，仅针对 Cookies</summary>
		Public Function CreateToken(response As HttpResponse, values As NameValueDictionary, Optional expired As Integer = 0) As String

			If expired < 1 Then expired = Setting.Expires
			If expired < 1 Then expired = 1

			Dim strToken = CreateToken(values, expired)
			response.Cookies.Append(TokenName, strToken, New CookieOptions With {.Expires = DateTimeOffset.Now.AddMinutes(expired)})
			Return strToken
		End Function

#End Region

#Region "分析 Token"

		''' <summary>获取 Token 数据</summary>
		Public Function GetClaims(request As HttpRequest) As NameValueDictionary
			Return GetClaims(request.Token(TokenName))
		End Function

		''' <summary>获取 Token 数据</summary>
		Public Function GetClaims(strToken As String) As NameValueDictionary
			If strToken.NotEmpty Then
				Try
					Dim validate = Handler.ValidateToken(strToken, ValidationParameters, Nothing)
					If validate IsNot Nothing Then
						Dim claimsIdentity = TryCast(validate.Identity, ClaimsIdentity)
						If claimsIdentity?.Claims?.Count > 0 Then
							Return claimsIdentity.Claims.ToNameValueDictionary(Function(x) x.Type, Function(x) x.Value)
						End If
					End If
				Catch ex As Exception
				End Try
			End If

			Return Nothing
		End Function

#End Region

#Region "清除 Token"

		''' <summary>清除 Cookies 中 Token，如果存在用户标识还将清除用户所有缓存 token</summary>
		Public Sub ClearToken(response As HttpResponse)
			response.Cookies.Delete(TokenName)
		End Sub

#End Region

	End Class

End Namespace