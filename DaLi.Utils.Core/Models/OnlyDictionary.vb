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
' 	只读字典数据
'
' 	name: Model.OnlyDictionary
' 	create: 2022-01-18
' 	memo: 只读字典数据，键和值的不可变未排序集合；对于键为字符，数字,GUID自动使用排序对象，否则按手动构造设置参数
'
' ------------------------------------------------------------

Imports System.Collections.Immutable

Namespace Model

	''' <summary>只读字典数据，键和值的不可变未排序集合</summary>
	Public Class OnlyDictionary(Of TKey, TValue)

		''' <summary>是否键排序表集合，True：键排序集合；False：键不排序集合</summary>
		Protected ReadOnly SortDictionay As Boolean

		''' <summary>线程锁</summary>
		Private ReadOnly _Lock As New Object

		''' <summary>构造；对于键为字符，数字,GUID自动使用排序对象，否则按手动构造设置参数</summary>
		''' <param name="sortDictionay">是否键排序表集合，True：键排序集合；False：键不排序集合</param>
		Public Sub New(Optional sortDictionay As Boolean? = Nothing)
			If sortDictionay Is Nothing Then
				Dim t = GetType(TKey)
				If t.IsString OrElse t.IsNumber OrElse t.IsGuid OrElse t.IsEnum Then
					sortDictionay = True
				End If
			End If

			Me.SortDictionay = sortDictionay

			' 赋值空值，会自动识别集合类型处理
			Instance = Nothing
		End Sub

		''' <summary>基础数据</summary>
		Private _Instance As ImmutableDictionary(Of TKey, TValue)

		''' <summary>基础数据</summary>
		Public Property Instance As ImmutableDictionary(Of TKey, TValue)
			Get
				Return _Instance
			End Get
			Protected Set(value As ImmutableDictionary(Of TKey, TValue))
				If value IsNot Nothing Then
					_Instance = value
				Else
					_Instance = If(SortDictionay, ImmutableSortedDictionary.Create(Of TKey, TValue), ImmutableDictionary.Create(Of TKey, TValue))
				End If
			End Set
		End Property

		''' <summary>获取项目 / 设置项目（存在则替换）</summary>
		Default Public Property Item(key As TKey) As TValue
			Get
				If key IsNot Nothing AndAlso Instance.ContainsKey(key) Then
					Return Instance(key)
				Else
					Return Nothing
				End If
			End Get
			Set(value As TValue)
				If key Is Nothing Then Return

				SyncLock _Lock
					If Instance.ContainsKey(key) Then
						Instance = Instance.SetItem(key, value)
					Else
						Instance = Instance.Add(key, value)
					End If
				End SyncLock
			End Set
		End Property

		''' <summary>添加数据，存在则忽略</summary>
		Public Sub Add(key As TKey, value As TValue)
			If key Is Nothing OrElse Instance.ContainsKey(key) Then Return

			SyncLock _Lock
				Instance = Instance.Add(key, value)
			End SyncLock
		End Sub

		''' <summary>添加数据，存在则忽略</summary>
		Public Sub AddRange(pairs As IEnumerable(Of KeyValuePair(Of TKey, TValue)))
			If pairs.NotEmpty Then
				SyncLock _Lock
					Instance = Instance.AddRange(pairs)
				End SyncLock
			End If
		End Sub

		''' <summary>更新数据，存在则替换，不存在忽略</summary>
		Public Sub Update(key As TKey, value As TValue)
			If key Is Nothing OrElse Not Instance.ContainsKey(key) Then Return

			SyncLock _Lock
				Instance = Instance.SetItem(key, value)
			End SyncLock
		End Sub

		''' <summary>更新数据</summary>
		Public Sub Update(pairs As IEnumerable(Of KeyValuePair(Of TKey, TValue)))
			If pairs.NotEmpty Then
				SyncLock _Lock
					Instance = Instance.SetItems(pairs)
				End SyncLock
			End If
		End Sub

		''' <summary>移除指定数据</summary>
		Public Sub Remove(key As TKey)
			If key Is Nothing OrElse Not Instance.ContainsKey(key) Then Return
			SyncLock _Lock
				Instance = Instance.Remove(key)
			End SyncLock
		End Sub

		''' <summary>移除指定数据</summary>
		Public Sub RemoveRange(keys As IEnumerable(Of TKey))
			If keys.NotEmpty Then
				SyncLock _Lock
					Instance.RemoveRange(keys)
				End SyncLock
			End If
		End Sub

		''' <summary>清除所有数据</summary>
		Public Sub Clear()
			SyncLock _Lock
				Instance = Instance.Clear
			End SyncLock
		End Sub

		''' <summary>数量</summary>
		Public ReadOnly Property Count As Integer
			Get
				Return Instance.Count
			End Get
		End Property

		''' <summary>键列表</summary>
		Public ReadOnly Property Keys As IEnumerable(Of TKey)
			Get
				Return Instance.Keys
			End Get
		End Property

		''' <summary>值列表</summary>
		Public ReadOnly Property Values As IEnumerable(Of TValue)
			Get
				Return Instance.Values
			End Get
		End Property

		''' <summary>是否存在</summary>
		Public Function Contains(pair As KeyValuePair(Of TKey, TValue)) As Boolean
			Return Instance.Contains(pair)
		End Function

		''' <summary>是否存在</summary>
		Public Function ContainsKey(key As TKey) As Boolean
			Return Instance.ContainsKey(key)
		End Function

	End Class

End Namespace