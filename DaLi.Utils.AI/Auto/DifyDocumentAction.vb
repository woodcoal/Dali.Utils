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
' 	Dify 知识库文档操作
'
' 	name: Auto.Rule.DifyDocumentAction
' 	create: 2024-05-20
' 	memo: Dify 知识库文档操作
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.AI.Dify.Model.Datasets
Imports DaLi.Utils.AI.Dify.Model.Datasets.ProcessRule.Rules_
Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>Dify 知识库文档操作</summary>
	Public Class DifyDocumentAction
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "Dify 知识库文档操作"
			End Get
		End Property

		''' <summary>服务器地址,不设置则使用系统全局参数</summary>
		Public Property Url As String

		''' <summary>ApiKey,不设置则使用系统全局参数</summary>
		Public Property ApiKey As String

		''' <summary>知识库标识</summary>
		Public Property Dataset As String

		''' <summary>文档标识，空则为添加数据，非空则修改或者删除</summary>
		Public Property Document As String

		''' <summary>操作模式：1.添加，2.修改，3.删除</summary>
		Public Property Mode As Integer

		'''''''''''''''''''''''''''''''''''''''''''''
		'	文档属性，删除模式无需填写
		'''''''''''''''''''''''''''''''''''''''''''''

		''' <summary>文档名称</summary>
		Public Property Document_Name As String

		''' <summary>文档内容</summary>
		Public Property Document_Text As String

		''' <summary>索引模式：高性能与经济</summary>
		Public Property Document_Indexing As Boolean

		''' <summary>是否问答模式模型</summary>
		Public Property Document_QA As Boolean

		''' <summary>分段最大文本 Tokens 数，0 则使用默认分段操作</summary>
		Public Property Document_Tokens As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "Dify 知识库标识必须设置"
			If Dataset.IsEmpty Then Return False

			message = "Dify 操作模式错误"
			If Mode > 3 OrElse Mode < 1 Then Return False

			message = "Dify 文档标识必须设置"
			If Mode <> 1 AndAlso Document.IsEmpty Then Return False

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

			Dim docId = AutoHelper.GetVarString(Document, data)?.ToGuid

			' 非添加模式需要文档标识
			If Mode <> 1 AndAlso docId.IsEmpty Then
				message.SetSuccess(False, "无效 Dify 知识库文档标识")
				Return Nothing
			End If

			Dim doc As New TextDocument With {
				.Name = AutoHelper.GetVarString(Document_Name, data),
				.Text = AutoHelper.GetVarString(Document_Text, data),
				.Indexing = If(Document_Indexing, "high_quality", "economy"),
				.Form = If(Document_QA, "qa_model", "text_model")
			}

			' 分段最大 Token 数
			Document_Tokens = Document_Tokens.Range(0, 1024)
			If Document_Tokens < 1 Then
				doc.ProcessRule = New ProcessRule With {.Mode = "automatic"}
			Else
				doc.ProcessRule = New ProcessRule With {
					.Mode = "custom",
					.Rules = New ProcessRule.Rules_ With {
						.PreProcessingRules = {
							New PreProcessingRules_ With {.ID = "remove_extra_spaces", .Enabled = True},
							New PreProcessingRules_ With {.ID = "remove_urls_emails", .Enabled = False}
						},
						.Segmentation = New Segmentation_ With {
							.MaxTokens = Document_Tokens.Range(64),
							.ChunkOverlap = .MaxTokens * 0.15
						}
					}
				}
			End If

			' API
			Dim ai As New AI.Dify.Datasets(url, apiKey)

			' 返回的结果
			Dim value As Object = Nothing

			' 返回的状态
			Dim ret As Object = Nothing

			Select Case Mode
				Case 1  ' 添加
					Dim res = ai.Document_Create(id, doc)
					ret = res
					If res IsNot Nothing AndAlso res.Data IsNot Nothing Then value = res.Data.Document

				Case 2  ' 修改
					Dim res = ai.Document_Update(id, docId, doc)
					ret = res
					If res IsNot Nothing AndAlso res.Data IsNot Nothing Then value = res.Data.Document

				Case 3  ' 删除
					Dim res = ai.Document_Delete(id, docId)
					ret = res
					If res IsNot Nothing AndAlso res.Data IsNot Nothing Then
						If res.Data.Result.ToBoolean Then value = True
					End If
			End Select

			If value IsNot Nothing Then
				message.SetSuccess()
				Return value
			ElseIf ret IsNot Nothing Then
				message.SetSuccess(False, $"{ret.Status}. {ret.Message}")
			Else
				message.SetSuccess(False, $"系统异常，无异常提示信息返回")
			End If

			Return Nothing
		End Function

#End Region

	End Class
End Namespace
