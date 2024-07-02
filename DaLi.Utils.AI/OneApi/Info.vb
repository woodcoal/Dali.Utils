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
' 	OneApi 信息相关 API
'
' 	name: OneApi.Info
' 	create: 2024-06-05
' 	memo: OneApi 信息相关 API
'
' ------------------------------------------------------------

Imports DaLi.Utils.AI.OneApi.Model
Imports DaLi.Utils.Http.Model

Namespace OneApi
	''' <summary>OneApi 信息相关 API</summary>
	Public Class Info
		Inherits Base

		Public Sub New(Optional url As String = "", Optional key As String = "")
			MyBase.New(url, key)
		End Sub

		''' <summary>模型格式转换</summary>
		Private Shared Function ModelConvert(tag As Model.Model) As AI.Model.Model
			If tag Is Nothing Then Return Nothing

			Dim model As New AI.Model.Model
			model.AddRangeFast(tag)
			model.Name = tag.Name
			model.Created = tag.Created

			Return model
		End Function

		''' <summary>列出可用的模型</summary>
		Public Function Models() As IEnumerable(Of AI.Model.Model)
			Dim tags = Execute(Of Models)("/v1/models", HttpMethodEnum.GET)
			If tags Is Nothing OrElse tags.Data Is Nothing Then Return Nothing

			Return tags.Data.
				Select(Function(x) ModelConvert(x)).
				Where(Function(x) x IsNot Nothing).
				ToList
		End Function

		''' <summary>获取模型详细信息</summary>
		Public Function Model(name As String) As AI.Model.Model
			If name.IsEmpty Then Return Nothing

			Dim tag = Execute(Of Model.Model)($"/v1/models/{name}", HttpMethodEnum.GET)

			Dim m = ModelConvert(tag)
			If m IsNot Nothing Then m.Name = name

			Return m
		End Function

	End Class
End Namespace