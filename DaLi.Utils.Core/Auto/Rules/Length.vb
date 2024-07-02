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
' 	数据内容长度
'
' 	name: Auto.Rule.Length
' 	create: 2023-01-12
' 	memo: 数据内容长度，列表、字典返回数量，文本返回字符长度，其他无效
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>数据内容长度</summary>
	Public Class Length
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "数据内容长度"
			End Get
		End Property

		''' <summary>来源数据</summary>
		Public Property Source As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "来源数据未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim value = AutoHelper.GetVar(Source, data)
			If value Is Nothing Then Return -1

			Dim type = value.GetType

			message.SetSuccess()
			If type.IsList(Of Object) Then
				Return TryCast(value, IEnumerable(Of Object))?.Count

			ElseIf type.IsDictionary(Of String, Object) Then
				Return TryCast(value, IDictionary(Of String, Object))?.Count

			ElseIf type.IsString Then
				Return TryCast(value, String)?.Length

			Else
				Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, ErrorMessage.EmptyValue("无效数据格式，获取数据长度仅支持：列表，字典与文本数据"))

			End If
		End Function

#End Region

	End Class
End Namespace