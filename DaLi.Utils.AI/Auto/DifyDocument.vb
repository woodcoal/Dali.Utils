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
' 	Dify 知识库文档列表
'
' 	name: Auto.Rule.DifyDocument
' 	create: 2024-05-20
' 	memo: Dify 知识库文档列表
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>Dify 知识库文档列表</summary>
	Public Class DifyDocument
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "Dify 知识库文档列表"
			End Get
		End Property

		''' <summary>服务器地址,不设置则使用系统全局参数</summary>
		Public Property Url As String

		''' <summary>ApiKey,不设置则使用系统全局参数</summary>
		Public Property ApiKey As String

		''' <summary>知识库标识</summary>
		Public Property Dataset As String

		''' <summary>文档名关键词</summary>
		Public Property Keyword As String

		''' <summary>当前页码</summary>
		Public Property Page As Integer

		''' <summary>每页数量</summary>
		Public Property Size As Integer = 100

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "Dify 知识库标识必须设置"
			If Dataset.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim url = AutoHelper.GetVarString(Me.Url, data)
			If url.NotEmpty AndAlso Not url.IsUrl Then
				message.SetSuccess(False, "无效 Dify 服务器地址")
				Return Nothing
			End If

			Dim apiKey = AutoHelper.GetVarString(Me.ApiKey, data)

			Dim id = AutoHelper.GetVarString(Dataset, data)?.ToGuid
			If id.IsEmpty Then
				message.SetSuccess(False, "无效 Dify 知识库标识")
				Return Nothing
			End If

			Dim Keyword = AutoHelper.GetVarString(Me.Keyword, data)

			Dim ai As New AI.Dify.Datasets(url, apiKey)
			Dim res = ai.Document_Query(id, Page, Size, Keyword)

			If res IsNot Nothing AndAlso res.Data IsNot Nothing AndAlso res.Data.Data.NotEmpty Then
				message.SetSuccess()
				Return res.Data.Data.Select(Function(x) New With {x.Id, x.Name}).ToList
			Else
				message.SetSuccess(False, $"{res.Status}. {res.Message}")
				Return Nothing
			End If
		End Function

#End Region

	End Class
End Namespace
