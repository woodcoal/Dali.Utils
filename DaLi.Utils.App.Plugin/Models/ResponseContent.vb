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
' 	输出文本结果
'
' 	name: Model.ResponseContent
' 	create: 2023-02-14
' 	memo: 输出文本结果
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Mvc

Namespace Model

	''' <summary>输出文本结果</summary>
	Public Class ResponseContent

		''' <summary>文本内容</summary>
		Public Property Content As String

		''' <summary>状态码</summary>
		Public Property StatusCode As Integer

		''' <summary>输出类型</summary>
		Public Property ContentType As String

		Public Sub New()
			Me.New("")
		End Sub

		Public Sub New(content As String)
			Me.New(content, "text/html; charset=utf-8")
		End Sub

		Public Sub New(content As String, contentType As String)
			Me.New(content, contentType, 200)
		End Sub

		Public Sub New(content As String, contentType As String, statusCode As Integer)
			Me.Content = content
			Me.StatusCode = statusCode
			Me.ContentType = contentType
		End Sub

		Public Function ActionResult() As IActionResult
			Return New ContentResult With {
				.StatusCode = StatusCode,
				.ContentType = ContentType,
				.Content = Content
			}
		End Function

	End Class
End Namespace
