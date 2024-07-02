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
' 	文本转数组
'
' 	name: Auto.Rule.TextArray
' 	create: 2023-01-16
' 	memo: 文本转数组
' 	
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>文本转数组</summary>
	Public Class TextArray
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "文本转数组"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>字符分隔符</summary>
		Public Property Separator As String

		''' <summary>是否过滤重复内容</summary>
		Public Property Distinct As Boolean

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim source = AutoHelper.GetVarString(Me.Source, data)
			Dim separator = AutoHelper.GetVarString(Me.Separator, data)

			message.SetSuccess()
			If source.IsEmpty Then Return Nothing

			If Distinct Then
				Return source.SplitDistinct(separator)
			Else
				Return source.SplitEx(separator)
			End If
		End Function

#End Region

	End Class
End Namespace
