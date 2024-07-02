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
' 	控制台打印
'
' 	name: Auto.Rule.Console
' 	create: 2023-01-01
' 	memo: 控制台打印
' 	
' ------------------------------------------------------------

Namespace Auto.Rule

	''' <summary>控制台打印</summary>
	Public Class Console
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "控制台打印"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Content As String

		''' <summary>打印类型（info/succ/err/warning/nomarl）</summary>
		Public Property Mode As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "控制台打印内容不能为空"
			If Content.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "控制台打印内容不能为空"
			Dim source = AutoHelper.GetVarString(Content, data)
			If source.IsEmpty Then Return False

			Select Case Mode.EmptyValue("normal").ToLower.Substring(0, 1)
				Case "i"
					CON.Info(source)

				Case "e"
					CON.Err(source)

				Case "w"
					CON.Warn(source)

				Case "s"
					CON.Succ(source)

				Case Else
					CON.Echo(source)
			End Select

			' 忽略错误
			ErrorIgnore = True

			message.SetSuccess()
			Return True
		End Function

#End Region

	End Class
End Namespace
