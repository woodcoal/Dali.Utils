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
' 	黑名单设置
'
' 	name: Setting.BlackListSetting
' 	create: 2023-02-14
' 	memo: 黑名单设置
'
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Extension
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Logging

Namespace Setting

	''' <summary>全局数据库通用参数</summary>
	Public Class BlackListSetting
		Inherits LocalSettingBase(Of BlackListSetting)

		''' <summary>更改配置文件此配置不更新，如果需要实时更新黑名单数据，建议增加数据库功能调用</summary>
		Protected Overrides ReadOnly Property AutoUpdate As Boolean
			Get
				Return True
			End Get
		End Property

		''' <summary>是否允许空白 UserAgent 访问</summary>
		<Description("是否允许空白 UserAgent 访问")>
		Public Property UserAgentEmpty As Boolean = False

		''' <summary>黑名单浏览器头</summary>
		<Description("黑名单浏览器头，JSON 文本数组")>
		Public Property UserAgents As String()

		''' <summary>黑名单IP</summary>
		<Description("黑名单IP，JSON 文本数组")>
		Public Property IPs As String()

		''' <summary>黑名单返回状态码</summary>
		<Description("黑名单IP返回状态码，200-600 整数")>
		<Range(200, 600)>
		Public Property RetrunCode As Integer = 444

		''' <summary>黑名单返回内容</summary>
		<Description("黑名单IP返回内容")>
		Public Property RetrunContent As String

		''' <summary>日志是否记录黑名单数据</summary>
		<Description("日志是否记录黑名单数据")>
		Public Property Record As Boolean = False

		''' <summary>是否记录黑名单请求信息</summary>
		Public Function Check(context As HttpContext, Optional log As ILogger = Nothing) As IActionResult
			Dim hasBlack = False

			' 检查是否允许空白 UserAgent
			If Not UserAgentEmpty Then
				Dim UserAgent = context.UserAgent
				If UserAgent.IsEmpty Then
					hasBlack = True
					If Record Then log?.LogWarning("黑名单请求：{id}，Path: {path}，空 UserAgent 头", context.TraceIdentifier, context.Url)
				End If
			End If

			' 检查 IP
			If Not hasBlack AndAlso IPs.NotEmpty Then
				Dim IPList = context.IPs
				If IPList.NotEmpty Then
					For Each item In IPList
						If item.Like(IPs) Then
							hasBlack = True
							If Record Then log?.LogWarning("黑名单请求：{id}，Path: {path}，IP：{ips} / {ip}", context.TraceIdentifier, context.Url, IPList.JoinString, item)
							Exit For
						End If
					Next
				End If
			End If

			' 检查浏览器头
			If Not hasBlack AndAlso UserAgents.NotEmpty Then
				Dim UserAgent = context.UserAgent
				hasBlack = UserAgent.Like(UserAgents)

				If hasBlack AndAlso Record Then log?.LogWarning("黑名单请求：{id}，Path: {path}，UserAgent：{UserAgent}", context.TraceIdentifier, context.Url, UserAgent)
			End If

			' 存在黑名单
			If hasBlack Then
				' 存在非法IP
				Select Case RetrunCode
					Case 301
						Return New RedirectResult(RetrunContent, True)

					Case 302
						Return New RedirectResult(RetrunContent, False)

					Case 307
						Return New RedirectResult(RetrunContent, False, True)

					Case 308
						Return New RedirectResult(RetrunContent, True, True)

					Case Else
						Return New ContentResult With {
							.StatusCode = RetrunCode,
							.Content = RetrunContent,
							.ContentType = "text/html"
						}
				End Select
			End If

			Return Nothing
		End Function

	End Class

End Namespace

