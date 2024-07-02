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
' 	JWT 参数接口
'
' 	name: Interface.IJWTSetting
' 	create: 2023-02-27
' 	memo: JWT 参数接口
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace [Interface]
	''' <summary>JWT 参数接口</summary>
	Public Interface IJWTSetting
		Inherits ILocalSetting

		''' <summary>JWT Token 密匙</summary>
		Property SecurityKey As String

		''' <summary>JWT 颁布机构</summary>
		Property Issuer As String

		''' <summary>JWT 使用者</summary>
		Property Audience As String

		''' <summary>JWT 密匙超时时长，单位：分钟</summary>
		<Range(1, 999999)>
		Property Expires As Integer

		''' <summary>JWT 刷新密匙超时时长，必须大于 JWT 密钥时长，单位：分钟</summary>
		<Range(1, 999999)>
		Property RefreshExpires As Integer

	End Interface
End Namespace