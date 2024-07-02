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
' 	线程安全键值字典集合
'
' 	name: Model.SafeKeyValueDictionary
' 	create: 2023-01-15
' 	memo: 线程安全键值字典集合，忽略键名大小写
' 	
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Model

	''' <summary>线程安全键值字典集合，忽略键名大小写</summary>
	Public Class SafeKeyValueDictionary
		Inherits ConcurrentDictionary(Of String, Object)
		Implements ICloneable

#Region "初始化"

		Public Sub New()
			MyBase.New(StringComparer.OrdinalIgnoreCase)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			MyBase.New(collection, StringComparer.OrdinalIgnoreCase)
		End Sub

		''' <summary>克隆</summary>
		''' <remarks>注意：如果值为对象，则在克隆的时候可能不会深度克隆。</remarks>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return New SafeKeyValueDictionary(Me)
		End Function

#End Region

#Region "常用函数"

		''' <summary>添加，存在则忽略</summary>
		Public Sub Add(key As String, value As Object)
			If key.NotNull Then TryAdd(key, value)
		End Sub

		''' <summary>添加，存在则忽略</summary>
		Public Sub Add(kv As KeyValuePair(Of String, Object))
			Add(kv.Key, kv.Value)
		End Sub

		''' <summary>添加，存在则忽略</summary>
		Public Sub Add(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each kv In collection
					Add(kv)
				Next
			End SyncLock
		End Sub

		''' <summary>不存在添加，存在则更新</summary>
		Public Sub Update(key As String, value As Object)
			If key.NotNull Then AddOrUpdate(key, value, Function(x, y) value)
		End Sub

		''' <summary>不存在添加，存在则更新</summary>
		Public Sub Update(kv As KeyValuePair(Of String, Object))
			Update(kv.Key, kv.Value)
		End Sub

		''' <summary>不存在添加，存在则更新</summary>
		Public Sub Update(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each kv In collection
					Update(kv)
				Next
			End SyncLock
		End Sub

#End Region

		''' <summary>设置 / 获取项目，设置时如果不存在则新建，存在则更新</summary>
		Default Public Overloads Property Item(key As String) As Object
			Get
				If key.NotNull AndAlso ContainsKey(key) Then
					Return MyBase.Item(key)
				Else
					Return Nothing
				End If
			End Get
			Set(value As Object)
				If key.IsNull Then Return

				Update(key, value)
			End Set
		End Property

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Default Public Overloads ReadOnly Property Item(key As String, defaultValue As Object) As Object
			Get
				Return If(Item(key), defaultValue)
			End Get
		End Property

		'		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		'		Public Function GetValue(Of T As {Class, New})(key As String) As T
		'			Dim value = Item(key)
		'			If value IsNot Nothing Then
		'				Try
		'					Return value
		'				Catch ex As Exception
		'				End Try
		'			End If

		'			Return New T
		'		End Function

		'		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		'		Public Function GetValue(Of T)(key As String, Optional defaultValue As T = Nothing) As T
		'			Dim value = Item(key)
		'			If value IsNot Nothing Then
		'				Try
		'					Return value
		'				Catch ex As Exception
		'				End Try
		'			End If

		'			Return defaultValue
		'		End Function

		'		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		'		Public Function GetListValue(Of T)(key As String) As List(Of T)
		'			Dim ret As New List(Of T)

		'			Dim data = GetValue(Of IEnumerable(Of Object))(key)
		'			If data.NotEmpty Then
		'				For Each value In data
		'					Dim newVal = Extension.ChangeType(Of T)(value)
		'					If newVal IsNot Nothing Then ret.Add(newVal)
		'				Next
		'			End If

		'			Return ret
		'		End Function

		'		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		'		Public Function GetArrayValue(Of T)(key As String) As T()
		'			Return GetListValue(Of T)(key).ToArray()
		'		End Function
		'#End Region

		'#Region "序列化"

		'		''' <summary>Json 转换成对象</summary>
		'		Public Shared Function FromJson(source As String) As ConcurrentKeyValueDictionary
		'			Return If(source.ToJsonObject(Of ConcurrentKeyValueDictionary), New ConcurrentKeyValueDictionary)
		'		End Function

		'		''' <summary>Xml 转换成对象</summary>
		'		Public Shared Function FromXml(source As String) As ConcurrentKeyValueDictionary
		'			Return If(source.ToXmlObject(Of ConcurrentKeyValueDictionary), New ConcurrentKeyValueDictionary)
		'		End Function

		'		''' <summary>转换成XML</summary>
		'		Public Function ToXml() As String
		'			' 请勿使用 Extension 简写，如：Me.ToXml 这样会出现死循环
		'			Return XmlExtension.ToXml(Me)
		'		End Function

		'		''' <summary>转换成XML</summary>
		'		Public Function ToJson() As String
		'			' 请勿使用 Extension 简写，如：Me.ToJson 这样会出现死循环
		'			Return JsonExtension.ToJson(Me)
		'		End Function

		'		''' <summary>转换成 NameValueDictionary</summary>
		'		Public Function ToNameValueDictionary() As NameValueDictionary
		'			Dim ret As New NameValueDictionary

		'			For Each kv In Me
		'				ret(kv.Key) = kv.Value?.ToString
		'			Next

		'			Return ret
		'		End Function

		'#End Region

		'#Region "加解密"

		'		''' <summary>加密</summary>
		'		Public Function Encode(Optional key As String = "", Optional clutter As Integer = 0) As String
		'			Return ToJson.EncodeString(clutter, key)
		'		End Function

		'		''' <summary>解密</summary>
		'		Public Shared Function Decode(code As String, Optional key As String = "", Optional clutter As Integer = 0) As ConcurrentKeyValueDictionary
		'			Return FromJson(code.DecodeString(clutter, key))
		'		End Function

		'#End Region

		''' <summary>遍历项目</summary>
		Public Sub ForEach(action As Action(Of String, Object))
			If action IsNot Nothing AndAlso Count > 0 Then
				For Each KV In ToArray()
					action.Invoke(KV.Key, KV.Value)
				Next
			End If
		End Sub

		''' <summary>替换值中标签数据，使用 JSON 序列化深度修改，注意返回结果的结构可能会发生变化！</summary>
		Public Function FormatTemplate(replaceDatas As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As SafeKeyValueDictionary
			Dim Ret As New SafeKeyValueDictionary

			For Each Kv In ToArray()
				If Kv.Value IsNot Nothing AndAlso Kv.Value.GetType.IsString Then
					Ret.TryAdd(Kv.Key, Kv.Value.ToString.FormatTemplate(replaceDatas, clearTag))
				Else
					Ret.TryAdd(Kv.Key, Kv.Value)
				End If
			Next

			Return Ret
		End Function
	End Class

End Namespace