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
' 	操作指令异常
'
' 	name: Auto.AutoException
' 	create: 2023-01-06
' 	memo: 操作指令异常
'
' ------------------------------------------------------------

Imports System.Runtime.Serialization

Namespace Auto
	''' <summary>操作指令异常</summary>
	<Serializable>
	Public Class AutoException
		Inherits Exception

		''' <summary>错误类型</summary>
		Public ReadOnly Property AutoType As ExceptionEnum

		''' <summary>异常消息</summary>
		Public ReadOnly Property AutoMessage As AutoMessage

		Protected Sub New()
			MyBase.New()
		End Sub

		Public Sub New(type As ExceptionEnum, message As AutoMessage)
			MyBase.New($"操作指令异常：{type.Description}")
			AutoType = type
			AutoMessage = message
		End Sub

		Public Sub New(type As ExceptionEnum, message As AutoMessage, description As String)
			MyBase.New(description)
			AutoType = type
			AutoMessage = message
		End Sub

		Public Sub New(type As ExceptionEnum, message As AutoMessage, description As String, innerException As Exception)
			MyBase.New(description, innerException)
			AutoType = type
			AutoMessage = message
		End Sub

		Protected Sub New(info As SerializationInfo, context As StreamingContext)
			MyBase.New(info, context)
		End Sub
	End Class
End Namespace
