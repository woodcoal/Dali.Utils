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
' 	Xml 扩展操作
'
' 	name: Extension.XmlExtension
' 	create: 2020-11-16
' 	memo: Xml 扩展操作
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace Extension

	''' <summary>Xml 扩展操作</summary>
	Public Module XmlExtension

#Region "XML 序列化"

		''' <summary>序列化对象为 Xml</summary>
		<Extension>
		Public Function ToXml(Of T)(this As T) As String
			Dim Value = ""

			If this IsNot Nothing Then
				Try
					Using mStringWriter As New IO.StringWriter
						Call New XmlSerializer(GetType(T)).Serialize(mStringWriter, this)
						Value = mStringWriter.ToString
					End Using
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		''' <summary>序列化对象为 Xml</summary>
		<Extension>
		Public Function ToXml(this As Object) As String
			Dim Value = ""

			If this IsNot Nothing Then
				Try
					Using mStringWriter As New IO.StringWriter
						Call New XmlSerializer(this.GetType).Serialize(mStringWriter, this)
						Value = mStringWriter.ToString

						With New System.Xml.XmlDocument
							.LoadXml(String.Format("<{0}/>", this.GetType.FullName))
							.InnerXml = Value
							Value = .DocumentElement.OuterXml
						End With
					End Using
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		''' <summary>反序列 Xml 化为对象</summary>
		''' <param name="withSpachCharts">是否需要保留回车等数据</param>
		<Extension>
		Public Function ToXmlObject(Of T)(this As String, Optional withSpachCharts As Boolean = True) As T
			Dim Value As T = Nothing

			If this.NotEmpty Then
				this = this.ClearLowAscii

				Try
					If withSpachCharts AndAlso this.StartsWith("<?xml", StringComparison.CurrentCultureIgnoreCase) Then
						' 默认 reader.Normalization = true 换行将丢失，使用 false 即可解决
						' 2016-12-16 修改
						Using mStringReader As New IO.StringReader(this)
							Dim mSerializer As New XmlSerializer(GetType(T))

							Using reader As New System.Xml.XmlTextReader(mStringReader)
								reader.Normalization = False
								Value = mSerializer.Deserialize(reader)
							End Using
						End Using
					Else
						'无法解决换行丢失的问题，重新更改代码
						Using mStringReader As New IO.StringReader(this)
							Dim mSerializer As New XmlSerializer(GetType(T))
							Value = CType(mSerializer.Deserialize(mStringReader), T)
						End Using
					End If
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		''' <summary>反序列 Xml 化为对象</summary>
		<Extension>
		Public Function XmlToItem(this As String, Optional typeName As String = "") As Object
			Dim Value As Object = Nothing

			If this.NotEmpty Then
				Try
					If typeName.IsEmpty Then
						With New System.Xml.XmlDocument
							.LoadXml(this)

							typeName = .DocumentElement.Name
							If typeName.ToLower.StartsWith("ArrayOf") Then
								typeName = String.Concat(typeName.AsSpan(7), "[]")
							End If

							typeName = "" & typeName
						End With
					End If

					If typeName.NotEmpty Then
						this = this.ClearLowAscii

						Using mStringReader As New IO.StringReader(this)
							Dim mSerializer As New XmlSerializer(Type.GetType(typeName, False, True))
							Value = mSerializer.Deserialize(mStringReader)
						End Using
					End If
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

#End Region

	End Module
End Namespace