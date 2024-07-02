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
' 	备注
'
' 	name: Auto.Rule.Memo
' 	create: 2023-01-01
' 	memo: 备注，用于分割项目，注释项目，无其他作用
' 	
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>备注</summary>
	Public Class Memo
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "备注"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Content As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "备注内容不能为空"
			If Content.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 忽略错误
			ErrorIgnore = True

			message.SetSuccess()
			Return Nothing
		End Function

#End Region

	End Class
End Namespace
