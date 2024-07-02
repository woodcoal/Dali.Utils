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
' 	OneApi 基础操作
'
' 	name: OneApi.Base
' 	create: 2024-06-05
' 	memo: OneApi 基础操作
'
' ------------------------------------------------------------

Imports System.Net
Imports DaLi.Utils.AI.OneApi.Model
Imports DaLi.Utils.Http
Imports DaLi.Utils.Http.Model

Namespace OneApi
	Public MustInherit Class Base

		''' <summary>API 客户端</summary>
		Private ReadOnly _Api As ApiClient

		Public Sub New(Optional url As String = "", Optional key As String = "")
			If url.IsEmpty Then url = AISettings.ONEAPI_URL
			If Not url.IsUrl Then Throw New Exception("OneApi 服务器地址错误")

			If key.IsEmpty Then key = AISettings.ONEAPI_KEY
			If key.IsEmpty Then Throw New Exception("OneApi Api-Key 未设置")

			_Api = New ApiClient With {
				.BaseURL = url,
				.Token = key,
				.Timeout = 600000 ' 超时：10分钟
			}
		End Sub

		''' <summary>执行 API 操作</summary>
		Protected Function Execute(Of T)(path As String, method As HttpMethodEnum, Optional params As IDictionary(Of String, Object) = Nothing, Optional ByRef status As (Code As HttpStatusCode, Message As String) = Nothing) As T
			Dim data = _Api.Execute(method, path, params, True)
			status = (_Api.StatusCode, data)

			If _Api.StatusCode = Net.HttpStatusCode.OK Then Return data.ToJsonObject(Of T)
		End Function

		''' <summary>流式执行 API 操作</summary>
		Protected Async Function ExecuteAsync(Of T As ResponseBase)(path As String, method As HttpMethodEnum, Optional params As IDictionary(Of String, Object) = Nothing, Optional callback As Action(Of HttpStatusCode, String, T) = Nothing) As Task
			Await _Api.ExecuteAsync(method,
								path,
								params,
								True,
								Sub(str)
									' 处理返回值 data: {...}
									If str.StartsWith("data:") Then str = str.Substring(5).Trim

									' 处理完成
									If str = "[DONE]" Then Return

									Dim data As T = Nothing
									If _Api.StatusCode = Net.HttpStatusCode.OK Then data = str.ToJsonObject(Of T)

									callback?.Invoke(_Api.StatusCode, str, data)

								End Sub)
		End Function

	End Class
End Namespace