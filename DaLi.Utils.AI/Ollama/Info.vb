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
' 	Ollama 信息相关 API
'
' 	name: Ollama.Info
' 	create: 2024-06-05
' 	memo: Ollama 信息相关 API
'
' ------------------------------------------------------------

Imports DaLi.Utils.AI.Ollama.Model
Imports DaLi.Utils.Http.Model

Namespace Ollama
	''' <summary>Ollama 信息相关 API</summary>
	Public Class Info
		Inherits Base

		Public Sub New(Optional url As String = "")
			MyBase.New(url)
		End Sub

		''' <summary>模型格式转换</summary>
		Private Shared Function ModelConvert(tag As Model.Model) As AI.Model.Model
			If tag Is Nothing Then Return Nothing

			Dim model As New AI.Model.Model
			model.AddRangeFast(tag.ToDictionary(False))
			model.Name = tag.Name
			model.Created = tag.Modified

			Return model
		End Function

		''' <summary>列出可用的模型</summary>
		Public Function Models() As IEnumerable(Of AI.Model.Model)
			Dim tags = Execute(Of Models)("/api/tags", HttpMethodEnum.GET)
			If tags Is Nothing OrElse tags.Models Is Nothing Then Return Nothing

			Return tags.Models.
				Select(Function(x) ModelConvert(x)).
				Where(Function(x) x IsNot Nothing).
				ToList
		End Function

		''' <summary>获取模型详细信息</summary>
		Public Function Model(name As String) As AI.Model.Model
			If name.IsEmpty Then Return Nothing

			Dim data As New KeyValueDictionary From {{"name", name}}
			Dim tag = Execute(Of Model.Model)("/api/show", HttpMethodEnum.POST, data)

			Dim m = ModelConvert(tag)
			If m IsNot Nothing Then m.Name = name

			Return m
		End Function

	End Class
End Namespace