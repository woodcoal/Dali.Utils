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
' 	Api 公用操作
'
' 	name: Controller.AppController
' 	create: 2023-02-14
' 	memo: Api 公用操作
' 	
' ------------------------------------------------------------

Imports System.ComponentModel
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc

Namespace Controller

	''' <summary>API 控制器基类</summary>
	<NoLog>
	Partial Public Class AppController
		Inherits ApiControllerBase

		''' <summary>服务器握手测试，字符串结果</summary>
		<Description("握手测试(TEXT)")>
		<HttpGet("ping")>
		Public Function Ping(data As String) As String
			Return $"pong! Hello {data} @ {HttpContext.IP} # {SYS_NOW_STR}!"
		End Function

		''' <summary>服务器握手测试，JSON 结果</summary>
		<Description("握手测试(JSON)")>
		<HttpPost("ping")>
		Public Function Hello(<FromForm> data As String) As IActionResult
			Return Succ(New With {data, HttpContext.IP, .TIME = SYS_NOW})
		End Function

		''' <summary>系统相关信息</summary>
		<HttpGet("sys")>
		<ResponseCache(Duration:=1800, Location:=ResponseCacheLocation.Client)>
		Public Function System() As IActionResult
			' 加载相关参数
			Dim ClientItem As New With {HttpContext.IP}

			' 返回数据
			Return Succ(New With {.Client = ClientItem, .Sys = SYS.Information})
		End Function

	End Class

End Namespace
