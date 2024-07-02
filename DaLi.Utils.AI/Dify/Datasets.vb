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
' 	Dify 知识库 API
'
' 	name: Dify.Datasets
' 	create: 2024-05-20
' 	memo: Dify 知识库 API
'
' ------------------------------------------------------------

Imports System.IO
Imports System.Net.Http
Imports DaLi.Utils.AI.Dify.Model
Imports DaLi.Utils.AI.Dify.Model.Datasets
Imports DaLi.Utils.Http
Imports DaLi.Utils.Http.Model

Namespace Dify

	''' <summary>Dify 知识库 API</summary>
	Public Class Datasets

		''' <summary>API 客户端</summary>
		Private ReadOnly _Api As ApiClient

		''' <summary>构造</summary>
		''' <param name="url">知识库服务器地址</param>
		''' <param name="key">知识库 ApiKey</param>
		Public Sub New(Optional url As String = "", Optional key As String = "")
			url = url.EmptyValue(AISettings.DIFY_URL)
			key = key.EmptyValue(AISettings.DIFY_KEY_DATASETS)

			If Not url.IsUrl Then Throw New Exception("Dify 知识库服务器地址错误")
			If key.IsEmpty OrElse key.Length < 10 Then Throw New Exception("Dify 知识库服务器 ApiKey 无效")

			_Api = New ApiClient With {
				.BaseURL = url,
				.Token = key
			}
		End Sub

		''' <summary>执行操作</summary>
		''' <typeparam name="T"></typeparam>
		''' <param name="method">请求方式</param>
		''' <param name="path">路径</param>
		''' <param name="data">提交数据</param>
		Private Function Execute(Of T)(method As HttpMethodEnum, path As String, Optional data As Object = Nothing, Optional fileContent As Stream = Nothing) As Result(Of T)
			Dim ret As String

			' 存在文件需要上传
			If data IsNot Nothing AndAlso fileContent IsNot Nothing Then
				Dim mContent = ApiClient.CreateFileContent(fileContent, "file")
				mContent.Add(New StringContent(Extension.ToJson(data, False, True, True)), "data")

				ret = _Api.Execute(method, path, mContent)
			Else
				ret = _Api.Execute(method, path, If(data IsNot Nothing, Extension.ToJson(data, False, True, True), Nothing))
			End If

			If _Api.StatusCode = Net.HttpStatusCode.OK Then Return New Result(Of T)(ret)
			If _Api.StatusCode = 0 Then Return New Result(Of T)("execute_error", 0, ret)
			Return If(ret.ToJsonObject(Of Result(Of T)), New Result(Of T)("system_error", _Api.StatusCode, ret))
		End Function

#Region "知识库"

		''' <summary>创建空知识库</summary>
		Public Function KB_Create(name As String) As Result(Of KB)
			If name.IsEmpty Then Return Nothing

			Dim path = "/v1/datasets"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of KB)(method, path, New NameValueDictionary From {{"name", name}})
		End Function

		''' <summary>知识库列表</summary>
		Public Function KB_Query(Optional page As Integer = 1, Optional size As Integer = 20) As Result(Of KBs)
			page = page.Range(1)
			size = size.Range(1, 100)

			Dim path = $"/v1/datasets?page={page}&limit={size}"
			Dim method = HttpMethodEnum.GET

			Return Execute(Of KBs)(method, path)
		End Function

#End Region

#Region "文档"

		''' <summary>通过文本创建文档</summary>
		''' <param name="id">知识库标识</param>
		Public Function Document_Create(id As Guid, doc As TextDocument) As Result(Of DocumentResult)
			If id.IsEmpty OrElse doc Is Nothing Then Return Nothing

			Dim path = $"/v1/datasets/{id}/document/create_by_text"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of DocumentResult)(method, path, doc)
		End Function

		''' <summary>通过文件创建文档</summary>
		''' <param name="id">知识库标识</param>
		Public Function Document_Create(id As Guid, doc As FileDocument, filePath As String) As Result(Of DocumentResult)
			If id.IsEmpty OrElse doc Is Nothing OrElse Not PathHelper.FileExist(filePath) Then Return Nothing

			Dim path = $"/v1/datasets/{id}/document/create_by_file"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of DocumentResult)(method, path, doc, New FileStream(filePath, FileMode.Open, FileAccess.Read))
		End Function

		''' <summary>通过文件创建文档</summary>
		''' <param name="id">知识库标识</param>
		Public Function Document_Create(id As Guid, doc As FileDocument, fileContent As Stream) As Result(Of DocumentResult)
			If id.IsEmpty OrElse doc Is Nothing OrElse fileContent Is Nothing Then Return Nothing

			Dim path = $"/v1/datasets/{id}/document/create_by_file"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of DocumentResult)(method, path, doc, fileContent)
		End Function

		''' <summary>文档删除</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Document_Delete(id As Guid, docId As Guid) As Result(Of Result)
			If id.IsEmpty OrElse docId.IsEmpty Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}"
			Dim method = HttpMethodEnum.DELETE

			Return Execute(Of Result)(method, path)
		End Function

		''' <summary>文档列表</summary>
		''' <param name="id">知识库标识</param>
		Public Function Document_Query(id As Guid, Optional page As Integer = 1, Optional size As Integer = 20, Optional keyword As String = "") As Result(Of Documents)
			page = page.Range(1)
			size = size.Range(1, 100)

			Dim path = $"/v1/datasets/{id}/documents?page={page}&limit={size}&keyword={keyword}"
			Dim method = HttpMethodEnum.GET

			Return Execute(Of Documents)(method, path)
		End Function

		''' <summary>通过文本更新文档</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Document_Update(id As Guid, docId As Guid, doc As TextDocument) As Result(Of DocumentResult)
			If id.IsEmpty OrElse docId.IsEmpty OrElse doc Is Nothing Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}/update_by_text"
			Dim method = HttpMethodEnum.POST

			doc.Indexing = Nothing

			Return Execute(Of DocumentResult)(method, path, doc)
		End Function

		''' <summary>通过文件更新文档</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Document_Update(id As Guid, docId As Guid, doc As FileDocument, filePath As String) As Result(Of DocumentResult)
			If id.IsEmpty OrElse docId.IsEmpty OrElse doc Is Nothing OrElse Not PathHelper.FileExist(filePath) Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}/update_by_file"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of DocumentResult)(method, path, doc, New FileStream(filePath, FileMode.Open, FileAccess.Read))
		End Function

		''' <summary>通过文件更新文档</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Document_Update(id As Guid, docId As Guid, doc As FileDocument, fileContent As Stream) As Result(Of DocumentResult)
			If id.IsEmpty OrElse docId.IsEmpty OrElse doc Is Nothing OrElse fileContent Is Nothing Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}/update_by_file"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of DocumentResult)(method, path, doc, fileContent)
		End Function

		''' <summary>文档状态</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="batch">批次号</param>
		Public Function Document_Status(id As Guid, batch As String) As Result(Of DocumentStatus)
			If id.IsEmpty OrElse batch.IsEmpty Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{batch}/indexing-status"
			Dim method = HttpMethodEnum.GET

			Return Execute(Of DocumentStatus)(method, path)
		End Function

#End Region

#Region "段落"

		''' <summary>增加分段</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Segment_Create(id As Guid, docId As Guid, segments As IEnumerable(Of SegmentBase)) As Result(Of Segments)
			If id.IsEmpty OrElse docId.IsEmpty OrElse segments Is Nothing Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}/segments"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of Segments)(method, path, New KeyValueDictionary From {{"segments", segments}})
		End Function

		''' <summary>分段列表</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		''' <param name="keyword">搜索关键词，可选</param>
		''' <param name="status">搜索状态，completed</param>
		Public Function Segment_Query(id As Guid, docId As Guid, Optional keyword As String = "", Optional status As String = "") As Result(Of Segments)
			If id.IsEmpty OrElse docId.IsEmpty Then Return Nothing

			Dim query As New List(Of String)
			If keyword.NotEmpty Then query.Add($"keyword={keyword}")
			If status.NotEmpty Then query.Add($"status={status}")

			Dim path = $"/v1/datasets/{id}/documents/{docId}/segments?{query.JoinString("&")}"
			Dim method = HttpMethodEnum.GET

			Return Execute(Of Segments)(method, path)
		End Function

		''' <summary>文档删除</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Segment_Delete(id As Guid, docId As Guid, segmentId As Guid) As Result(Of Result)
			If id.IsEmpty OrElse docId.IsEmpty OrElse segmentId.IsEmpty Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}/segments/{segmentId}"
			Dim method = HttpMethodEnum.DELETE

			Return Execute(Of Result)(method, path)
		End Function

		''' <summary>增加分段</summary>
		''' <param name="id">知识库标识</param>
		''' <param name="docId">文档标识</param>
		Public Function Segment_Update(id As Guid, docId As Guid, segmentId As Guid, segment As SegmentBase) As Result(Of SegmenItem)
			If id.IsEmpty OrElse docId.IsEmpty OrElse segmentId.IsEmpty OrElse segment Is Nothing Then Return Nothing

			Dim path = $"/v1/datasets/{id}/documents/{docId}/segments/{segmentId}"
			Dim method = HttpMethodEnum.POST

			Return Execute(Of SegmenItem)(method, path, New KeyValueDictionary From {{"segment", segment}})
		End Function

#End Region

	End Class
End Namespace