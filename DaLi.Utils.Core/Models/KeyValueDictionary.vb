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
' 	键值字典集合
'
' 	name: Model.KeyValueDictionary
' 	create: 2022-04-08
' 	memo: 键值字典集合，忽略键名大小写
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>键值字典集合，忽略键名大小写</summary>
	Public Class KeyValueDictionary
		Inherits Dictionary(Of String, Object)
		Implements ICloneable

		''' <summary>线程锁定对象</summary>
		Private ReadOnly _Lock As New Object

#Region "初始化"

		Public Sub New()
			MyBase.New(StringComparer.OrdinalIgnoreCase)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(collection)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, Object))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(dictionary)
		End Sub

#End Region

#Region "常用函数"

		''' <summary>添加键值，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Overloads Sub AddFast(key As String, value As Object)
			If key.IsNull Then Return

			SyncLock _Lock
				TryAdd(key, value)
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			If collection.IsEmpty Then Return

			SyncLock collection
				SyncLock _Lock
					For Each c In collection
						If c.Key.NotNull Then TryAdd(c.Key, c.Value)
					Next
				End SyncLock
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(dictionary As IDictionary(Of String, Object))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				SyncLock _Lock
					For Each Key In dictionary.Keys
						If Key.NotNull Then TryAdd(Key, dictionary(Key))
					Next
				End SyncLock
			End SyncLock
		End Sub

		''' <summary>添加键值，如果不存在则添加，存在则不处理</summary>
		Public Overloads Function Add(key As String, value As Object) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If Not ContainsKey(key) Then
						MyBase.Add(key, value)
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>添加一组数据，如果不存在则添加，存在则不处理</summary>
		Public Sub AddRange(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			If collection.IsEmpty Then Return

			SyncLock collection
				SyncLock _Lock
					For Each c In collection
						If c.Key.NotNull AndAlso Not ContainsKey(c.Key) Then MyBase.Add(c.Key, c.Value)
					Next
				End SyncLock
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果不存在则添加，存在则不处理</summary>
		Public Sub AddRange(dictionary As IDictionary(Of String, Object))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				SyncLock _Lock
					For Each Key In dictionary.Keys
						If Key.NotNull AndAlso Not ContainsKey(Key) Then MyBase.Add(Key, dictionary(Key))
					Next
				End SyncLock
			End SyncLock
		End Sub

		''' <summary>更新数据，如果不存在则添加，存在则替换</summary>
		Public Sub Update(key As String, value As Object)
			Item(key) = value
		End Sub

		''' <summary>更新一组数据，如果不存在则添加，存在则替换</summary>
		Public Sub UpdateRange(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Item(c.Key) = c.Value
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，如果不存在则添加，存在则替换</summary>
		Public Sub UpdateRange(dictionary As IDictionary(Of String, Object))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Item(c.Key) = c.Value
				Next
			End SyncLock
		End Sub

		''' <summary>移除项目</summary>
		Public Overloads Sub Clear()
			If Count > 0 Then
				SyncLock _Lock
					MyBase.Clear()
				End SyncLock
			End If
		End Sub

		''' <summary>移除项目</summary>
		Public Overloads Function Remove(key As String) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If ContainsKey(key) Then
						R = MyBase.Remove(key)
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>移除项目</summary>
		Public Overloads Sub Remove(ParamArray keys() As String)
			If keys.IsEmpty Then Return

			SyncLock _Lock
				For Each Key In keys
					If Key.NotNull AndAlso ContainsKey(Key) Then MyBase.Remove(Key)
				Next
			End SyncLock
		End Sub

		''' <summary>仅保留指定键的值</summary>
		Public Sub Keep(ParamArray keys() As String)
			If keys.IsEmpty Then Return

			' 获取需要移除的键
			keys = Me.Keys.Where(Function(x) Not keys.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

			Remove(keys)
		End Sub

		''' <summary>设置 / 获取项目，设置时如果不存在则新建，存在则更新</summary>
		Default Public Overloads Property Item(key As String) As Object
			Get
				Dim R = Nothing

				SyncLock _Lock
					If key.NotNull AndAlso ContainsKey(key) Then
						R = MyBase.Item(key)
					End If
				End SyncLock

				Return R
			End Get
			Set(value As Object)
				If key.NotNull Then
					SyncLock _Lock
						If ContainsKey(key) Then
							MyBase.Item(key) = value
						Else
							MyBase.Add(key, value)
						End If
					End SyncLock
				End If
			End Set
		End Property

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Default Public Overloads ReadOnly Property Item(key As String, defaultValue As Object) As Object
			Get
				Return If(Item(key), defaultValue)
			End Get
		End Property

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(key As String) As String
			Return GetValue(key, "")
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(key As String, baseType As Type) As Object
			Dim value = Item(key)
			If value IsNot Nothing Then value = JsonExtension.ToJson(value).ToJsonObject(baseType)

			Return value
		End Function

		'''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		'Public Function GetValue(Of T As {Class, New})(key As String) As T
		'	Dim value = Item(key)
		'	If value IsNot Nothing Then
		'		Try
		'			Return value
		'		Catch ex As Exception
		'		End Try
		'	End If

		'	Return New T
		'End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(Of T)(key As String, Optional defaultValue As T = Nothing) As T
			Dim value = Item(key)
			'If value IsNot Nothing Then value = JsonExtension.ToJson(value).ToJsonObject(Of T)
			If value IsNot Nothing Then value = TypeExtension.ToObjectString(value).ToValue(Of T)

			Return If(value, defaultValue)
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(key As String) As List(Of String)
			Return GetListValue(Of String)(key)
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(key As String, baseType As Type) As List(Of Object)
			'Dim data = GetValue(Of IEnumerable(Of Object))(key)
			Dim data = ChangeType(Of IEnumerable(Of Object))(Item(key))
			If data Is Nothing Then Return Nothing

			Return data.Select(Function(x) ToValue(x, baseType)).Where(Function(x) x IsNot Nothing).ToList
			'Return data.Select(Function(x) JsonExtension.ToJson(x).ToJsonObject(baseType)).Where(Function(x) x IsNot Nothing).ToList
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(Of T)(key As String) As List(Of T)
			Dim data = ChangeType(Of IEnumerable(Of Object))(Item(key))
			If data Is Nothing Then Return Nothing

			Return data.Select(Function(x) ToValue(Of T)(x)).Where(Function(x) x IsNot Nothing).ToList
			'Return data.Select(Function(x) JsonExtension.ToJson(x).ToJsonObject(Of T)).Where(Function(x) x IsNot Nothing).ToList
		End Function

#End Region

#Region "序列化"

		''' <summary>Json 转换成对象</summary>
		Public Shared Function FromJson(source As String) As KeyValueDictionary
			If String.IsNullOrWhiteSpace(source) Then
				Return New KeyValueDictionary
			Else
				Return New KeyValueDictionary(source.ToJsonDictionary)
			End If
		End Function

		''' <summary>Xml 转换成对象</summary>
		Public Shared Function FromXml(source As String) As KeyValueDictionary
			Return If(source.ToXmlObject(Of KeyValueDictionary), New KeyValueDictionary)
		End Function

		''' <summary>转换成XML</summary>
		Public Function ToXml() As String
			' 请勿使用 Extension 简写，如：Me.ToXml 这样会出现死循环
			Return XmlExtension.ToXml(Me)
		End Function

		''' <summary>转换成XML</summary>
		Public Function ToJson() As String
			' 请勿使用 Extension 简写，如：Me.ToJson 这样会出现死循环
			Return JsonExtension.ToJson(Of KeyValueDictionary)(Me)
		End Function

		''' <summary>转换成 NameValueDictionary</summary>
		Public Function ToNameValueDictionary() As NameValueDictionary
			Dim ret As New NameValueDictionary

			For Each kv In Me
				ret(kv.Key) = kv.Value?.ToString
			Next

			Return ret
		End Function

#End Region

#Region "加解密"

		''' <summary>加密</summary>
		Public Function Encode(Optional key As String = "", Optional clutter As Integer = 0) As String
			Return ToJson.EncodeString(clutter, key)
		End Function

		''' <summary>解密</summary>
		Public Shared Function Decode(code As String, Optional key As String = "", Optional clutter As Integer = 0) As KeyValueDictionary
			Return FromJson(code.DecodeString(clutter, key))
		End Function

#End Region

		''' <summary>克隆</summary>
		''' <remarks>注意：如果值为对象，则在克隆的时候可能不会深度克隆。</remarks>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return New KeyValueDictionary(Me)
		End Function

		''' <summary>遍历项目</summary>
		Public Sub ForEach(action As Action(Of String, Object))
			If action IsNot Nothing AndAlso Count > 0 Then
				For Each KV In New KeyValueDictionary(Me)
					action.Invoke(KV.Key, KV.Value)
				Next
			End If
		End Sub

		''' <summary>替换值中标签数据，使用 JSON 序列化深度修改，注意返回结果的结构可能会发生变化！</summary>
		Public Function FormatTemplate(replaceDatas As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As KeyValueDictionary
			Dim Ret As New KeyValueDictionary

			For Each Kv In Me
				If Kv.Value IsNot Nothing AndAlso Kv.Value.GetType.IsString Then
					Ret.Add(Kv.Key, Kv.Value.ToString.FormatTemplate(replaceDatas, clearTag))
				Else
					Ret.Add(Kv.Key, Kv.Value)
				End If
			Next

			Return Ret
		End Function

		''' <summary>替换值中标签数据</summary>
		Public Function FormatAction(act As Func(Of Object, Object)) As KeyValueDictionary
			Dim Ret = New KeyValueDictionary(Me)
			If Ret.Count < 1 OrElse act Is Nothing Then Return Ret

			For Each key In Ret.Keys
				Ret(key) = act(Ret(key))
			Next

			Return Ret
		End Function
	End Class

End Namespace