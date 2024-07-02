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
' 	Json 相关操作
'
' 	name: Json
' 	create: 2024-01-25
' 	memo: Json 相关操作
'
' ------------------------------------------------------------

Imports System.Text.Json

Namespace Misc.Json

	''' <summary>Object 数据的自定义转换</summary>
	Public Class JsonObjectConverter
		Inherits JsonConverterBase

		'Public Sub New(JsonElementParse As Func(Of JsonElement, Object))
		'End Sub

		''' <summary>写入</summary>
		Public Overrides Sub Write(writer As Utf8JsonWriter, value As Object, options As JsonSerializerOptions)
			If value Is Nothing Then
				writer.WriteStringValue("null")
				Return
			End If

			Dim type = value.GetType()
			If type.IsNullableNumber AndAlso Double.IsNaN(value) Then
				writer.WriteStringValue("NaN")
				Return
			End If

			JsonSerializer.Serialize(writer, value, type, options)
		End Sub

		''' <summary>读取</summary>
		Protected Overrides Function Read(value As JsonElement) As Object
			Return value.JsonElementParse
		End Function

	End Class

	''' <summary>小写的命名策略</summary>
	Public Class JsonLowerCaseNamingPolicy
		Inherits JsonNamingPolicy

		Public Overrides Function ConvertName(name As String) As String
			Return name.ToLower
		End Function
	End Class

	''' <summary>大写的命名策略</summary>
	Public Class JsonUpperCaseNamingPolicy
		Inherits JsonNamingPolicy

		Public Overrides Function ConvertName(name As String) As String
			Return name.ToUpper
		End Function
	End Class
End Namespace