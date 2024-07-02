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
' 	Dify 知识库文档段落操作
'
' 	name: Auto.Rule.DifySegmentAction
' 	create: 2024-05-20
' 	memo: Dify 知识库文档段落操作
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.AI.Dify.Model.Datasets
Imports DaLi.Utils.Auto

Namespace Auto.Rule

	''' <summary>Dify 知识库文档段落操作</summary>
	Public Class DifySegmentAction
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

		''' <summary>文档标识</summary>
		Public Property Document As String

		''' <summary>段落标识，空则为添加数据，非空则修改或者删除</summary>
		Public Property Segment As String

		''' <summary>操作模式：1.添加，2.修改，3.删除</summary>
		Public Property Mode As Integer

		'''''''''''''''''''''''''''''''''''''''''''''
		'	段落属性，删除模式无需填写
		'''''''''''''''''''''''''''''''''''''''''''''

		''' <summary>文本内容/问题内容，必填</summary>
		Public Property Segment_Content As String

		''' <summary>答案内容，非必填，如果知识库的模式为qa模式则传值</summary>
		Public Property Segment_Answer As String

		''' <summary>关键字，非必填</summary>
		Public Property Segment_Keywords As String

		''' <summary>启用，非必填</summary>
		Public Property Segment_Enabled As Boolean = True
#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "Dify 知识库标识必须设置"
			If Dataset.IsEmpty Then Return False

			message = "Dify 知识库文档标识必须设置"
			If Document.IsEmpty Then Return False

			message = "Dify 操作模式错误"
			If Mode > 3 OrElse Mode < 1 Then Return False

			message = "Dify 段落标识必须设置"
			If Mode <> 1 AndAlso Segment.IsEmpty Then Return False

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
			If docId.IsEmpty Then
				message.SetSuccess(False, "无效 Dify 知识库文档标识")
				Return Nothing
			End If

			' 非添加模式需要段落标识
			Dim segId = AutoHelper.GetVarString(Segment, data)?.ToGuid
			If Mode <> 1 AndAlso segId.IsEmpty Then
				message.SetSuccess(False, "无效 Dify 知识库文档段落标识")
				Return Nothing
			End If

			Dim seg As New SegmentBase With {
				.Content = AutoHelper.GetVarString(Segment_Content, data),
				.Answer = AutoHelper.GetVarString(Segment_Answer, data),
				.Keywords = AutoHelper.GetVarString(Segment_Keywords, data).SplitDistinct,
				.Enabled = Segment_Enabled
			}

			' API
			Dim ai As New AI.Dify.Datasets(url, apiKey)

			' 返回的结果
			Dim value As Object = Nothing

			' 返回的状态
			Dim ret As Object = Nothing

			Select Case Mode
				Case 1  ' 添加
					Dim res = ai.Segment_Create(id, docId, {seg})
					ret = res
					If res IsNot Nothing AndAlso res.Data IsNot Nothing AndAlso res.Data.Data.Any Then value = res.Data.Data(0)

				Case 2  ' 修改
					Dim res = ai.Segment_Update(id, docId, segId, seg)
					ret = res
					If res IsNot Nothing AndAlso res.Data IsNot Nothing Then value = res.Data.Data

				Case 3  ' 删除
					Dim res = ai.Segment_Delete(id, docId, segId)
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
