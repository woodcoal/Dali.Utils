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
' 	JWT 参数
'
' 	name: Setting.JWT
' 	create: 2021-01-15
' 	memo: JWT 参数
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Setting

	''' <summary>JWT 参数</summary>
	Public Class JWTSetting
		Inherits LocalSettingBase(Of JWTSetting)
		Implements IJWTSetting

		''' <summary>JWT Token 密匙</summary>
		<Required>
		Public Property SecurityKey As String = [GetType].Assembly.FullName Implements IJWTSetting.SecurityKey

		''' <summary>JWT 颁布机构</summary>
		Public Property Issuer As String = [GetType].Assembly.Company Implements IJWTSetting.Issuer

		''' <summary>JWT 使用者</summary>
		Public Property Audience As String = [GetType].Assembly.Name Implements IJWTSetting.Audience

		''' <summary>JWT 密匙超时时长，单位：分钟；默认 1 小时</summary>
		<Range(1, 999999)>
		Public Property Expires As Integer = 60 Implements IJWTSetting.Expires

		''' <summary>JWT 刷新密匙超时时长，必须大于 JWT 密钥时长，单位：分钟；默认 30 天</summary>
		<Range(1, 999999)>
		Public Property RefreshExpires As Integer = 43200 Implements IJWTSetting.RefreshExpires

		Protected Overrides Sub Initialize(provider As ISettingProvider)
			SecurityKey = SecurityKey.EmptyValue([GetType].Assembly.FullName).MD5
			Issuer = Issuer.EmptyValue([GetType].Assembly.Company)
			Audience = Issuer.EmptyValue([GetType].Assembly.Name)
		End Sub
	End Class

End Namespace

