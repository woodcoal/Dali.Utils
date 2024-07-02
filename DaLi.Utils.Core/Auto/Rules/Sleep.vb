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
' 	延时
'
' 	name: Auto.Rule.Sleep
' 	create: 2023-01-05
' 	memo: 延时
' 	
' ------------------------------------------------------------

Namespace Auto.Rule

	''' <summary>延时</summary>
	Public Class Sleep
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "延时"
			End Get
		End Property

		''' <summary>延时时长，单位：秒</summary>
		Public Property Length As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "延时时长不能小于 0 "
			If Length < 0 Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			If Length > 0 Then Threading.Thread.Sleep(Length * 1000)

			' 忽略错误
			ErrorIgnore = True

			message.SetSuccess()
			Return True
		End Function

#End Region

	End Class
End Namespace
