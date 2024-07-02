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
' 	Redis Hash 表操作
'
' 	name: Json
' 	create: 2024-06-22
' 	memo: Redis Hash 表操作
'
' ------------------------------------------------------------

Imports System.Diagnostics.CodeAnalysis
Imports FreeRedis

''' <summary>Redis Hash 表</summary>
Public Class RedisHash(Of T)
	Implements IDictionary(Of String, T)

	''' <summary>数据名称</summary>
	Public ReadOnly Name As String

	''' <summary>忽略大小写</summary>
	Public ReadOnly IgnoreCase As String

	''' <summary>Redis 客户端</summary>
	Protected ReadOnly Client As RedisClient

	''' <summary>缓存数据，以便加速读取</summary>
	Private ReadOnly _Cache As New LRU(Of String, Object)(100)

	''' <summary>构造</summary>
	Public Sub New(client As RedisClient, name As String, Optional ignoreCase As Boolean = True)
		If client Is Nothing Then Throw New Exception("Redis 客户端无效")
		If name.IsEmpty Then Throw New Exception("数据名称为设置")

		Me.Name = name
		Me.Client = client
		Me.IgnoreCase = ignoreCase

		' 检查当前类型是否 HASH，如果不是则删除
		Dim type = client.TypeName(name)
		If type <> "hash" Then client.Del(name)
	End Sub

	''' <summary>获取所有数据</summary>
	Public Function ToDictionary() As Dictionary(Of String, T)
		Return GetAll()
	End Function

	''' <summary>获取所有原始数据</summary>
	Public Function ToNameValueDictionary() As Dictionary(Of String, String)
		Dim action = Function() Client.HGetAll(Name)
		Return ExecuteResult(action)
	End Function

#Region "增、改、删"

	''' <summary>获取或者设置字段与值</summary>
	Default Public Property Item(field As String) As T Implements IDictionary(Of String, T).Item
		Get
			Return [Get](field)
		End Get
		Set(value As T)
			[Set](field, value)
		End Set
	End Property

	''' <summary>添加字段值</summary>
	''' <param name="field">字段</param>
	''' <param name="value">值</param>
	Public Sub Add(field As String, value As T) Implements IDictionary(Of String, T).Add
		SetNx(field, value)
	End Sub

	''' <summary>添加字段值队</summary>
	''' <param name="item">字段值队</param>
	Public Sub Add(item As KeyValuePair(Of String, T)) Implements ICollection(Of KeyValuePair(Of String, T)).Add
		Add(item.Key, item.Value)
	End Sub

	''' <summary>获取值</summary>
	Public Function TryGetValue(field As String, <MaybeNullWhen(False)> ByRef value As T) As Boolean Implements IDictionary(Of String, T).TryGetValue
		Dim message = ""
		value = [Get](field, message)

		Return message.IsEmpty
	End Function

	''' <summary>清空所有数据</summary>
	Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, T)).Clear
		Client.Del(Name)
	End Sub

	''' <summary>删除指定字段</summary>
	''' <param name="field">字段</param>
	Public Function Remove(field As String) As Boolean Implements IDictionary(Of String, T).Remove
		Return Del(field, Nothing)
	End Function

	''' <summary>删除指定字段值队</summary>
	''' <param name="item">字段值队</param>
	Public Function Remove(item As KeyValuePair(Of String, T)) As Boolean Implements ICollection(Of KeyValuePair(Of String, T)).Remove
		' 获取指定的字段的值
		Dim value = [Get](item.Key)
		If value Is Nothing OrElse Not value.Equals(item.Value) Then Return False

		Return Remove(item.Key)
	End Function

#End Region

#Region "其他属性"

	''' <summary>是否包含指定的字段</summary>
	Public Function ContainsKey(field As String) As Boolean Implements IDictionary(Of String, T).ContainsKey
		Return Exists(field)
	End Function

	''' <summary>是否包含指定的字段值队</summary>
	''' <param name="item">字段值队</param>
	Public Function Contains(item As KeyValuePair(Of String, T)) As Boolean Implements ICollection(Of KeyValuePair(Of String, T)).Contains
		' 获取指定的字段的值
		Dim value = [Get](item.Key)
		Return value Is Nothing AndAlso Not value.Equals(item.Value)
	End Function

	''' <summary>获取所有的字段</summary>
	Public ReadOnly Property Keys As ICollection(Of String) Implements IDictionary(Of String, T).Keys
		Get
			Return Keys_()
		End Get
	End Property

	''' <summary>获取所有的值</summary>
	Public ReadOnly Property Values As ICollection(Of T) Implements IDictionary(Of String, T).Values
		Get
			Return Vals()
		End Get
	End Property

	''' <summary>获取项目数量</summary>
	Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, T)).Count
		Get
			Return Len()
		End Get
	End Property

	''' <summary>是否只读</summary>
	Public ReadOnly Property IsReadOnly As Boolean = False Implements ICollection(Of KeyValuePair(Of String, T)).IsReadOnly

#End Region

#Region "枚举"

	''' <summary>返回循环访问的枚举数</summary>
	Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, T)) Implements IEnumerable(Of KeyValuePair(Of String, T)).GetEnumerator
		Return ToDictionary()?.GetEnumerator
	End Function

	Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
		Return ToDictionary()?.GetEnumerator
	End Function

	''' <summary>从指定的数组索引开始，将 ICollection 中的元素复制到一个数组中。</summary>
	''' <param name="array">一维数组，用作从 ICollection 复制的元素的目标位置。 该数组的索引必须从零开始。</param>
	''' <param name="arrayIndex">array 中从零开始的索引， 从此处开始复制。</param>
	Public Sub CopyTo(array() As KeyValuePair(Of String, T), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, T)).CopyTo
		ToDictionary()?.ToArray.CopyTo(array, arrayIndex)
	End Sub

#End Region

#Region "基础操作"

	''' <summary>检查获取操作的字段名称</summary>
	Private Function GetField(field As String, Optional ByRef errorMessage As String = "") As String
		errorMessage = "字段未设置"
		If field.IsEmpty Then Return ""

		errorMessage = ""
		Return If(IgnoreCase, field.ToLower, field)
	End Function

	''' <summary>检查获取操作的字段名称</summary>
	Private Function GetFields(fields As String(), Optional ByRef errorMessage As String = "") As String()
		errorMessage = "字段未设置"
		If fields.IsEmpty Then Return Nothing

		errorMessage = ""
		Return If(IgnoreCase, fields.Select(Function(x) x.ToLower).ToArray, fields)
	End Function

#End Region

#Region "原始操作"

	''' <summary>HDel 删除指定字段</summary>
	''' <param name="field">字段</param>
	Public Function Del(field As String, Optional ByRef errorMessage As String = "") As Boolean
		field = GetField(field, errorMessage)
		If field.IsEmpty Then Return False

		Return ExecuteResult(Function()
								 Dim flag = Client.HDel(Name, field) > 0

								 ' 删除缓存字段
								 If flag Then _Cache.Remove(field)

								 Return flag
							 End Function, errorMessage)
	End Function

	''' <summary>HDel 删除一组指定字段</summary>
	Public Function Del(fields As String(), Optional ByRef errorMessage As String = "") As Long
		fields = GetFields(fields, errorMessage)
		If fields.IsEmpty Then Return False

		Return ExecuteResult(Function()
								 Dim count = Client.HDel(Name, fields)

								 ' 删除缓存字段
								 If count > 0 Then fields.ToList.ForEach(Sub(x) _Cache.Remove(x))

								 Return count
							 End Function, errorMessage)
	End Function

	''' <summary>HExists 指定的字段是否存在</summary>
	''' <param name="field">字段</param>
	Public Function Exists(field As String, Optional ByRef errorMessage As String = "") As Boolean
		field = GetField(field, errorMessage)
		If field.IsEmpty Then Return False

		Return ExecuteResult(Function() Client.HExists(Name, field), errorMessage)
	End Function

	''' <summary>HGet 获取值</summary>
	''' <param name="field">字段</param>
	Public Function [Get](field As String, Optional ByRef errorMessage As String = "") As T
		field = GetField(field, errorMessage)
		If field.IsEmpty Then Return Nothing

		Dim action = Function() Client.HGet(Name, field).ToValue(Of T)
		Dim message = ""
		Dim value = _Cache.Get(field, Function() ExecuteResult(action, message))

		errorMessage = message
		Return value
	End Function

	''' <summary>HGetAll 获取所有数据</summary>
	Public Function GetAll(Optional ByRef errorMessage As String = "") As Dictionary(Of String, T)
		Return ExecuteResult(Function() Client.HGetAll(Name).ToDictionary(Function(x) x.Key, Function(x) x.Value.ToValue(Of T)), errorMessage)
	End Function

	'''' <summary>HINCRBY 字段的整数值加上增量</summary>
	'''' <param name="field">字段</param>
	'''' <param name="value">值</param>
	'''' <returns>返回修改后的数值</returns>
	'Public Function NumberAdd(field As String, value As Long, Optional ByRef errorMessage As String = "") As Long
	'	field = GetField(field, errorMessage)
	'	If field.IsEmpty Then Return False

	'	Dim action = Function()
	'					 value = Client.HIncrBy(Name, field, value) > 0

	'					 ' 添加缓存
	'					 _Cache.Put(field, value)

	'					 Return value
	'				 End Function
	'	Return ExecuteResult(action, errorMessage)
	'End Function

	'''' <summary>HINCRBYFLOAT 字段的整数值加上增量</summary>
	'''' <param name="field">字段</param>
	'''' <param name="value">值</param>
	'''' <returns>返回修改后的数值</returns>
	'Public Function FloatAdd(field As String, value As Decimal, Optional ByRef errorMessage As String = "") As Decimal
	'	field = GetField(field, errorMessage)
	'	If field.IsEmpty Then Return False

	'	Dim action = Function()
	'					 value = Client.HIncrByFloat(Name, field, value) > 0

	'					 ' 添加缓存
	'					 _Cache.Put(field, value)

	'					 Return value
	'				 End Function
	'	Return ExecuteResult(action, errorMessage)
	'End Function

	''' <summary>HKEYS 获取所有字段</summary>
	Public Function Keys_(Optional ByRef errorMessage As String = "") As String()
		Return ExecuteResult(Function() Client.HKeys(Name), errorMessage)
	End Function

	''' <summary>HLEN 获取字段的数量</summary>
	Public Function Len(Optional ByRef errorMessage As String = "") As Long
		Return ExecuteResult(Function() Client.HLen(Name), errorMessage)
	End Function

	''' <summary>HMGet 获取所有给定字段的值</summary>
	Public Function MGet(fields As String(), Optional ByRef errorMessage As String = "") As T()
		fields = GetFields(fields, errorMessage)
		If fields.IsEmpty Then Return Nothing

		Return ExecuteResult(Function()
								 Dim data = Client.HMGet(Name, fields)?.Select(Function(x) x.ToValue(Of T)).ToArray

								 ' 删除缓存字段
								 If data.NotEmpty Then
									 If data.Length = fields.Length Then
										 For I = 0 To fields.Length - 1
											 Dim field = fields(I)
											 Dim value = data(I)
											 fields.ToList.ForEach(Sub(x) _Cache.Put(field, value))
										 Next
									 End If
								 End If

								 Return data
							 End Function, errorMessage)
	End Function

	''' <summary>HMSet 同时设置多个值</summary>
	''' <param name="data">字段与值字典数据</param>
	Public Function MSet(data As IDictionary(Of String, T), Optional ByRef errorMessage As String = "") As Boolean
		errorMessage = "无任何数据需要设置"
		If data.IsEmpty Then Return Nothing

		Dim dic As New Dictionary(Of String, T)
		For Each kv In data
			Dim key = GetField(kv.Key)
			If key.IsEmpty Then Continue For

			If Not dic.ContainsKey(key) Then dic.Add(key, kv.Value)
		Next
		If dic.IsEmpty Then Return Nothing

		ExecuteResult(Function()
						  Client.HMSet(Name, dic.ToDictionary(Function(x) x.Key, Function(x) x.Value?.ToObjectString))

						  ' 添加缓存
						  For Each kv In dic
							  _Cache.Put(kv.Key, kv.Value)
						  Next

						  Return Nothing
					  End Function, errorMessage)

		Return errorMessage.IsEmpty
	End Function

	''' <summary>HSet 设置值，存在则修改，不存在则添加</summary>
	''' <param name="field">字段</param>
	''' <param name="value">值</param>
	Public Function [Set](field As String, value As T, Optional ByRef errorMessage As String = "") As Boolean
		field = GetField(field, errorMessage)
		If field.IsEmpty Then Return False

		Dim action = Function()
						 Dim flag = Client.HSet(Name, field, value?.ToObjectString) > 0

						 ' 添加缓存
						 If flag Then _Cache.Put(field, value)

						 Return flag
					 End Function
		Return ExecuteResult(action, errorMessage)
	End Function

	''' <summary>HSETNX 只有在字段 field 不存在时，设置哈希表字段的值。</summary>
	''' <param name="field">字段</param>
	''' <param name="value">值</param>
	Public Function SetNx(field As String, value As T, Optional ByRef errorMessage As String = "") As Boolean
		field = GetField(field, errorMessage)
		If field.IsEmpty Then Return False

		Dim action = Function()
						 Dim flag = Client.HSetNx(Name, field, value?.ToObjectString)

						 ' 添加缓存
						 If flag Then _Cache.Put(field, value)

						 Return flag
					 End Function
		Return ExecuteResult(action, errorMessage)
	End Function

	''' <summary>HVALS 获取所有值。</summary>
	Public Function Vals(Optional ByRef errorMessage As String = "") As T()
		Return ExecuteResult(Function() Client.HVals(Name)?.Select(Function(x) x.ToValue(Of T)), errorMessage)
	End Function

	''' <summary>HSCAN 迭代哈希表中的键值对</summary>
	Public Function Scan(pattern As String, Optional ByRef errorMessage As String = "") As Dictionary(Of String, T)
		Dim dic As New Dictionary(Of String, T)

		ExecuteResult(Function()
						  Dim idx = 0
						  While True
							  Dim result = Client.HScan(idx, pattern, 10, "")
							  If result.items.IsEmpty Then Exit While

							  For Each kv In result.items
								  If Not dic.ContainsKey(kv.Key) Then dic.Add(kv.Key, kv.Value.ToValue(Of T))
							  Next

							  If result.cursor < 1 Then Exit While
						  End While

						  Return Nothing
					  End Function, errorMessage)

		Return If(errorMessage.NotEmpty, Nothing, dic)
	End Function

#End Region

End Class
