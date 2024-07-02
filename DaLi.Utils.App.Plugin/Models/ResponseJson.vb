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
' 	输出 JSON 结果
'
' 	name: Model.ResponseJson
' 	create: 2023-02-14
' 	memo: 输出 JSON 结果
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc

Namespace Model

	''' <summary>输出 JSON 结果</summary>
	Public Class ResponseJson

		''' <summary>是否成功</summary>
		Public Property Data As Object

		''' <summary>结果代码，1000 以下与 statusCode 一致</summary>
		Public Property Code As Integer

		''' <summary>消息内容，当消息方式为 Page 跳转时，此内容为跳转地址</summary>
		Public Property Message As String

		''' <summary>请求队列ID</summary>
		Public Property TraceId As String

		''' <summary>当前请求的域名</summary>
		Public Property Host As String

		Public Sub New(Optional http As HttpContext = Nothing)
			Code = 200

			If http IsNot Nothing Then
				TraceId = http.TraceIdentifier
				Host = http.Request?.Host.ToString
			End If
		End Sub

		Public Sub New(data As Object, Optional message As String = "", Optional http As HttpContext = Nothing)
			Me.New(http)
			Me.Data = data
			Me.Message = message
		End Sub

		Public Sub New(code As Integer, message As String, Optional http As HttpContext = Nothing)
			Me.New(http)
			Me.Code = code
			Me.Message = message
		End Sub

		''' <summary>输出 JSON 内容</summary>
		''' <param name="statusCode">http StatusCode，不设置将自动分析 ErrorCode 值。成功为固定值 200</param>
		Public Function ActionResult(Optional statusCode As Integer? = Nothing) As IActionResult
			statusCode = If(statusCode, Code)
			Select Case statusCode
				Case 200
					statusCode = 200

				Case Is > 599, Is < 1
					statusCode = 400
			End Select

			Return New JsonResult(Me) With {
				.StatusCode = statusCode,
				.ContentType = "application/json"
			}
		End Function

		''' <summary>创建一个默认的输出</summary>
		Public Shared Function [Default](code As Integer, data As Object, Optional http As HttpContext = Nothing) As IActionResult
			Return New ResponseJson(http) With {.Code = code, .Data = data}.ActionResult
		End Function

		''' <summary>创建一个默认的输出</summary>
		Public Shared Function Err(code As Integer, message As String, Optional http As HttpContext = Nothing) As IActionResult
			Return New ResponseJson(code, message, http).ActionResult
		End Function

		''' <summary>创建一个默认的输出</summary>
		Public Shared Function Err(errMessage As ErrorMessage, Optional http As HttpContext = Nothing, Optional statusCode As Integer? = Nothing) As IActionResult
			If errMessage Is Nothing Then
				' 对象不存在
				statusCode = If(statusCode, 400)
				Return New ResponseJson(statusCode, Nothing, http).ActionResult(statusCode)

			ElseIf errMessage.HasException Then
				' 存在异常，500
				statusCode = If(statusCode, 500)
				Return New ResponseJson(statusCode, errMessage.Exception.Message, http).ActionResult(statusCode)

			ElseIf errMessage.Notification.NotEmpty Then
				' 存在通知错误提示
				statusCode = If(statusCode, 400)
				Return New ResponseJson(statusCode, errMessage.Notification, http).ActionResult(statusCode)

			Else
				' 最后验证消息
				statusCode = If(statusCode, 400)
				Dim Nvs = errMessage.Messages
				Dim errCode = If(errMessage.Code, -1)

				Return New ResponseJson(http) With {
					.Data = Nvs,
					.Code = errCode
				}.ActionResult(statusCode)
			End If
		End Function

	End Class
End Namespace
