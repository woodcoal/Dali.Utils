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
' 	Json 扩展操作
'
' 	name: Extension.JsonExtension
' 	create: 2021-03-11
' 	memo: Json 扩展操作
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization

Namespace Extension

	''' <summary>Json 扩展操作</summary>
	Public Module JsonExtension

#Region "Json 序列化"

		''' <summary>序列化 JSON 对象</summary>
		<Extension>
		Public Function ToJson(this As Object, options As JsonSerializerOptions, Optional type As Type = Nothing) As String
			If this IsNot Nothing Then
				options = If(options, New JsonSerializerOptions With {
					.PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
					.WriteIndented = True,
					.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
				})

				type = If(type, this.GetType)
				Return JsonSerializer.Serialize(this, type, options)
			End If

			Return ""
		End Function

		''' <summary>序列化 JSON 对象</summary>
		<Extension>
		Public Function ToJson(this As Object, Optional indented As Boolean = True, Optional camelCase As Boolean = True, Optional skipNull As Boolean = False) As String
			If this IsNot Nothing Then
				Dim options = New JsonSerializerOptions With {
					.PropertyNamingPolicy = If(camelCase, JsonNamingPolicy.CamelCase, Nothing),
					.DictionaryKeyPolicy = If(camelCase, JsonNamingPolicy.CamelCase, Nothing),
					.WriteIndented = indented,
					.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					.DefaultIgnoreCondition = If(skipNull, JsonIgnoreCondition.WhenWritingNull, JsonIgnoreCondition.Never)
				}

				Return JsonSerializer.Serialize(this, this.GetType, options)
			End If

			Return ""
		End Function

		''' <summary>序列化 JSON 对象</summary>
		<Extension>
		Public Function ToJson(Of T)(this As T, Optional indented As Boolean = True, Optional camelCase As Boolean = True, Optional skipNull As Boolean = False) As String
			If this IsNot Nothing Then
				Dim options = New JsonSerializerOptions With {
					.PropertyNamingPolicy = If(camelCase, JsonNamingPolicy.CamelCase, Nothing),
					.DictionaryKeyPolicy = If(camelCase, JsonNamingPolicy.CamelCase, Nothing),
					.WriteIndented = indented,
					.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					.DefaultIgnoreCondition = If(skipNull, JsonIgnoreCondition.WhenWritingNull, JsonIgnoreCondition.Never)
				}

				Return JsonSerializer.Serialize(this, options)
			End If

			Return ""
		End Function

		''' <summary>序列化 JSON 对象，名称不使用引号</summary>
		<Extension>
		Public Function ToJsonNoQuote(this As Object, Optional indented As Boolean = True, Optional camelCase As Boolean = False, Optional skipNull As Boolean = False) As String
			Dim Json = ToJson(this, indented, camelCase, skipNull)
			If Json.NotEmpty Then
				Try
					Dim pattern As String = """(\w+)""(\s*:\s*)"
					Dim replacement As String = "$1$2"
					Dim rge = New Text.RegularExpressions.Regex(pattern)
					Json = rge.Replace(Json, replacement)
				Catch ex As Exception
				End Try
			End If

			Return Json
		End Function

#End Region

#Region "Json 反序列化"

		''' <summary>反序列化 JSON 对象</summary>
		<Extension>
		Public Function ToJsonObject(this As String, type As Type, Optional options As JsonSerializerOptions = Nothing) As Object
			Try
				If this.NotEmpty AndAlso type IsNot Nothing Then
					If options Is Nothing Then
						options = New JsonSerializerOptions With {
							.PropertyNameCaseInsensitive = True,
							.ReadCommentHandling = JsonCommentHandling.Skip,
							.AllowTrailingCommas = True,
							.NumberHandling = JsonNumberHandling.AllowReadingFromString
						}

						' todo: Utils.Core.CSharp 如果需要记得启用
						'options.Converters.Add(New Misc.JsonConverter(Function(x) x.JsonElementParse))
						options.Converters.Add(New Misc.Json.JsonObjectConverter)
					End If

					Return JsonSerializer.Deserialize(this, type, options)
				End If
			Catch ex As Exception
			End Try

			Return Nothing
		End Function

		''' <summary>反序列化 JSON 对象</summary>
		<Extension>
		Public Function ToJsonObject(Of T)(this As String, Optional options As JsonSerializerOptions = Nothing) As T
			Try
				If this.NotEmpty Then
					options = If(options, New JsonSerializerOptions With {
						.PropertyNameCaseInsensitive = True,
						.ReadCommentHandling = JsonCommentHandling.Skip,
						.AllowTrailingCommas = True
					})

					' todo: Utils.Core.CSharp 如果需要记得启用
					'options.Converters.Add(New Misc.JsonConverter(Function(x) x.JsonElementParse))
					options.Converters.Add(New Misc.Json.JsonObjectConverter)

					Return JsonSerializer.Deserialize(Of T)(this, options)
				End If
			Catch ex As Exception
			End Try

			Return Nothing
		End Function

		''' <summary>反序列化 JSON 对象</summary>
		''' <param name="this">字符串</param>
		''' <param name="act">转换后对每个值进行的二次操作，如替换等</param>
		<Extension>
		Public Function ToJsonObject(this As String, Optional act As Func(Of Object, Type, Object) = Nothing) As Object
			Return ObjectAction(this.ToJsonCollection.Value, act)
		End Function

		''' <summary>对字典或者集合进行二次操作</summary>
		''' <param name="this">字典或者集合</param>
		''' <param name="act">转换后对每个值进行的二次操作，如替换等</param>
		Public Function ObjectAction(this As Object, Optional act As Func(Of Object, Type, Object) = Nothing) As Object
			If this Is Nothing Then Return Nothing
			If act Is Nothing Then Return this

			Dim type = this.GetType
			If type.IsDictionary Then
				Dim dic = TryCast(this, IDictionary)
				For Each k In dic.Keys
					dic(k) = ObjectAction(dic(k), act)
				Next
				Return dic
			ElseIf type.IsEnumerable Then
				Dim list = New List(Of Object)
				For Each item In TryCast(this, IEnumerable)
					list.Add(ObjectAction(item, act))
				Next
				Return list
			Else
				Return act.Invoke(this, type)
			End If
		End Function

		'----------------------------------------------------------------------------

		''' <summary>解析Josn为数据集合，根据JSON内容可能为字典</summary>
		''' <param name="this">Json 字符串</param>
		''' <param name="removeNothing">是否移除无效内容节点</param>
		''' <param name="replaceTags">用于替换值的标签数据，可以将 JSON 值中的标签替换成指定的内容</param>
		<Extension>
		Public Function ToJsonDictionary(this As String, Optional removeNothing As Boolean = False, Optional replaceTags As IDictionary(Of String, Object) = Nothing) As IDictionary(Of String, Object)
			Dim result = this.ToJsonCollection(removeNothing, replaceTags)
			If result.IsList Then
				Return Nothing
			Else
				Return result.Value
			End If
		End Function

		''' <summary>解析Josn为数据集合，根据JSON内容可能为列表</summary>
		''' <param name="this">Json 字符串</param>
		''' <param name="removeNothing">是否移除无效内容节点</param>
		''' <param name="replaceTags">用于替换值的标签数据，可以将 JSON 值中的标签替换成指定的内容</param>
		<Extension>
		Public Function ToJsonList(this As String, Optional removeNothing As Boolean = False, Optional replaceTags As IDictionary(Of String, Object) = Nothing) As List(Of Object)
			Dim result = this.ToJsonCollection(removeNothing, replaceTags)
			If result.IsList Then
				Return result.Value
			Else
				Return Nothing
			End If
		End Function

		''' <summary>解析Josn为数据集合，根据JSON内容可能为字典或者列表</summary>
		''' <param name="this">Json 字符串</param>
		''' <param name="removeNothing">是否移除无效内容节点</param>
		''' <param name="replaceTags">用于替换值的标签数据，可以将 JSON 值中的标签替换成指定的内容</param>
		<Extension>
		Public Function ToJsonCollection(this As String, Optional removeNothing As Boolean = False, Optional replaceTags As IDictionary(Of String, Object) = Nothing) As (Value As Object, IsList As Boolean)
			Try
				Dim options As New JsonDocumentOptions With {
					.AllowTrailingCommas = True,
					.CommentHandling = JsonCommentHandling.Skip,
					.MaxDepth = 64
				}

				Dim Ret As Object
				Using Doc = JsonDocument.Parse(this, options)
					Ret = JsonElementParse(Doc.RootElement, removeNothing, replaceTags)
				End Using

				Return (Ret, Not Ret.GetType.IsDictionary)
			Catch ex As Exception
			End Try

			Return Nothing
		End Function

		''' <summary>解析节点</summary>
		''' <param name="this">Json 节点</param>
		''' <param name="removeNothing">是否移除无效内容节点</param>
		''' <param name="replaceTags">用于替换值的标签数据，可以将 JSON 值中的标签替换成指定的内容</param>
		''' <remarks>
		''' JsonValueKind 类型：
		''' 0:Undefined		1:Object		2:Array
		''' 3:String		4:Number		5:True
		''' 6:False			7:Null
		''' </remarks>
		<Extension>
		Public Function JsonElementParse(this As JsonElement, Optional removeNothing As Boolean = False, Optional replaceTags As IDictionary(Of String, Object) = Nothing) As Object
			Select Case this.ValueKind
				'Case JsonValueKind.Undefined, JsonValueKind.Null

				Case JsonValueKind.Object
					Dim Ret = this.EnumerateObject.Select(Function(x) (x.Name, JsonElementParse(x.Value, removeNothing, replaceTags))).Where(Function(x) x.Item2 IsNot Nothing OrElse Not removeNothing).ToDictionary(Function(x) x.Name, Function(x) x.Item2)

					If removeNothing AndAlso Ret.IsEmpty Then
						Return Nothing
					Else
						Return Ret
					End If

				Case JsonValueKind.Array
					Dim Ret = this.EnumerateArray.Select(Function(x) JsonElementParse(x, removeNothing, replaceTags)).Where(Function(x) x IsNot Nothing OrElse Not removeNothing).ToList

					If removeNothing AndAlso Ret.IsEmpty Then
						Return Nothing
					Else
						Return Ret
					End If

				Case JsonValueKind.String
					Dim Value = this.GetString

					' 进一步处理字符串
					If Value.NotEmpty Then
						Dim ret = Nothing

						If Value.IsGUID AndAlso this.TryGetGuid(ret) Then Return ret

						' 尝试直接转换成日期
						If this.TryGetDateTime(ret) Then Return ret
						'If Regex.IsMatch(Value, "^\d{4}[\-/][0-1]?[0-9][\-/][0-3]?[0-9][\sT][0-2]?[0-9]:[0-5]?[0-9]:[0-5]?[0-9]$") AndAlso this.TryGetDateTime(ret) Then Return ret
					End If

					Return If(replaceTags.IsEmpty, Value, Value.FormatTemplate(replaceTags))

				Case JsonValueKind.Number
					Dim num = Nothing
					If this.TryGetByte(num) Then Return num
					If this.TryGetInt16(num) Then Return num
					If this.TryGetInt32(num) Then Return num
					If this.TryGetInt64(num) Then Return num
					If this.TryGetSingle(num) Then Return num
					If this.TryGetDouble(num) Then Return num
					'If this.TryGetDecimal(num) Then Return num
					Return this.GetDecimal

				Case JsonValueKind.True
					Return True

				Case JsonValueKind.False
					Return False

			End Select

			Return Nothing
		End Function

		''' <summary>解析Josn为数据集合，根据JSON内容可能为字典或者列表</summary>
		''' <param name="this">Json 字符串</param>
		<Extension>
		Public Function ToJsonNameValues(this As String) As NameValueDictionary
			Try
				Dim options As New JsonDocumentOptions With {
					.AllowTrailingCommas = True,
					.CommentHandling = JsonCommentHandling.Skip
				}

				Dim Ret As NameValueDictionary = Nothing
				Using Doc = JsonDocument.Parse(this, options)
					'  仅支持对象节点是 Json 序列化，数组和其他类型无法转换
					JsonElementParseCollection(Ret, Doc.RootElement, "")
				End Using

				Return Ret
			Catch ex As Exception
			End Try

			Return Nothing
		End Function

		''' <summary>解析对象节点</summary>
		''' <param name="ele">Json 节点</param>
		Private Sub JsonElementParseCollection(ByRef this As NameValueDictionary, ele As JsonElement, prefix As String)
			this = If(this, New NameValueDictionary)
			prefix = prefix.EmptyValue

			If ele.ValueKind = JsonValueKind.Object Then
				' 上下两级中的节点名称用冒号间隔
				If prefix.NotEmpty Then prefix &= "."

				' 分析节点
				For Each e In ele.EnumerateObject
					Dim Key = prefix & e.Name

					Select Case e.Value.ValueKind
						Case JsonValueKind.Object, JsonValueKind.Array
							' 需要递归子节点内容
							JsonElementParseCollection(this, e.Value, Key)

						Case Else
							' 直接分析节点字符
							this.Add(Key, e.Value.ToString)
					End Select
				Next

			ElseIf ele.ValueKind = JsonValueKind.Array Then
				' 分析节点
				Dim Idx = 0
				For Each e In ele.EnumerateArray
					Dim Key = prefix & "." & Idx

					Select Case e.ValueKind
						Case JsonValueKind.Object, JsonValueKind.Array
							' 需要递归子节点内容
							JsonElementParseCollection(this, e, Key)

						Case Else
							' 直接分析节点字符
							this.Add(Key, e.ToString)
					End Select

					Idx += 1
				Next
			End If
		End Sub

#End Region

	End Module
End Namespace
