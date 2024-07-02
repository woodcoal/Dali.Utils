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
' 	LRU 缓存
'
' 	name: LRU
' 	create: 2020-08-14
' 	memo: 基于线程安全字典对象的 LRU 缓存
' 	
' ------------------------------------------------------------

''' <summary>LRU 缓存，注意 key 不能为元组数据</summary>
Public Class LRU(Of K, V)

	Public ReadOnly Instance As Concurrent.ConcurrentDictionary(Of K, Node)
	Private _First As K
	Private _Last As K

	Public Size As Integer

	Public Sub New(Optional size As Integer = 100)
		Instance = New Concurrent.ConcurrentDictionary(Of K, Node)
		Me.Size = size
	End Sub

	''' <summary>添加节点</summary>
	Public Sub Put(key As K, value As V)
		If Size < 1 OrElse key Is Nothing Then Exit Sub

		If Instance.ContainsKey(key) Then
			Dim Node As Node = Nothing
			If Instance.TryGetValue(key, Node) Then
				Node.UpdateValue(value)
				Update(Node)
			End If
		Else
			Dim Node As New Node(key, value)

			' 第一次添加数据，将赋值默认首尾参数 
			If Instance.IsEmpty Then
				_First = key
				_Last = key
			End If

			If Instance.TryAdd(key, Node) Then Update(Node)
		End If
	End Sub

	''' <summary>获取节点</summary>
	''' <param name="defValue">当值不存在时的默认值</param>
	Public Function [Get](key As K, Optional defValue As V = Nothing) As V
		If Size < 1 OrElse key Is Nothing Then Return defValue

		If Instance.ContainsKey(key) Then
			Dim Node As Node = Nothing

			If Instance.TryGetValue(key, Node) Then
				If Node IsNot Nothing Then
					Update(Node)
					Return Node.Value
				End If
			End If
		End If

		Return defValue
	End Function

	''' <summary>获取节点</summary>
	''' <param name="pubAction">当值不存在时，通过此函数重新生成值并更新到缓存</param>
	Public Function [Get](key As K, pubAction As Func(Of K, V)) As V
		Dim value = [Get](key)

		If value Is Nothing AndAlso pubAction IsNot Nothing Then
			value = pubAction.Invoke(key)
			If value IsNot Nothing Then Put(key, value)
		End If

		Return value
	End Function

	Public Sub Clear()
		SyncLock Instance
			Instance.Clear()
			_First = Nothing
			_Last = Nothing
		End SyncLock
	End Sub

	Public Sub Remove(key As K)
		' 找到此节点，移除相关关系
		' 将要删除的项目移动到顶部
		Dim tmp = Instance(key)
		If tmp IsNot Nothing Then Update(tmp)

		' 重置队首
		_First = tmp.Nxt

		' 删除项目
		Instance.TryRemove(key, Nothing)
	End Sub

	Public ReadOnly Property Count As Integer
		Get
			Return Instance.Count
		End Get
	End Property

	''' <summary>所有节点数据字典列表</summary>
	Public ReadOnly Property Dictionary As Dictionary(Of K, V)
		Get
			Return Instance.ToDictionary(Function(x) x.Value.Key, Function(x) x.Value.Value)
		End Get
	End Property

	''' <summary>所有节点数据列表</summary>
	Public ReadOnly Property List As List(Of Node)
		Get
			Return Instance.Select(Function(x) x.Value).ToList
		End Get
	End Property

	''' <summary>更新当前节点位置，移动到队首</summary>
	Private Sub Update(node As Node)
		If node Is Nothing Then Exit Sub

		' 只有一个数据的时候，无需调整
		If Instance.Count < 2 Then Exit Sub

		' 检查是否队首节点，队首则不处理 
		If node.Key.Equals(_First) Then Exit Sub

		SyncLock Instance
			' 原位置前后对接
			If node.Prv IsNot Nothing Then
				Dim tmp = Instance(node.Prv)
				If tmp IsNot Nothing Then tmp.Nxt = node.Nxt
			End If
			If node.Nxt IsNot Nothing Then
				Dim tmp = Instance(node.Nxt)
				If tmp IsNot Nothing Then tmp.Prv = node.Prv
			End If

			' 检查是否队末节点，队末则需要更新为新数据
			If node.Key.Equals(_Last) Then
				If node.Prv IsNot Nothing Then _Last = node.Prv

				'If Node.Prv IsNot Nothing Then
				'	Dim tmp = Instance(Node.Prv)
				'	If tmp IsNot Nothing Then
				'		Last = tmp.Key
				'		tmp.Nxt = Nothing
				'	End If
				'End If
			End If

			' 移到队首，原来队首的前置为当前节点
			Dim tmpFirst = Instance(_First)
			tmpFirst.Prv = node.Key
			node.Nxt = _First
			node.Prv = Nothing

			' 队首更新为当前节点
			_First = node.Key

			' 检查对末数据
			If Size > 1 AndAlso Instance.Count > Size Then
				' 移除最后一个
				Dim tmp As Node = Nothing
				If Instance.TryRemove(_Last, tmp) Then
					If tmp IsNot Nothing Then
						_Last = tmp.Prv
					End If
				End If

				' 将最后项目的下一节点置空
				tmp = Instance(_Last)
				If tmp IsNot Nothing Then tmp.Nxt = Nothing
			End If
		End SyncLock
	End Sub

	''' <summary>双向链接节点</summary>
	Public Class Node

		Public Prv As K
		Public Nxt As K

		Public ReadOnly Key As K
		Private _Value As V

		Public CountGet As Integer
		Public CountSet As Integer

		Public Sub New(key As K, value As V, Optional prv As K = Nothing, Optional nxt As K = Nothing)
			CountGet = 0
			CountSet = 0

			Me.Key = key
			UpdateValue(value)
			Me.Prv = prv
			Me.Nxt = nxt
		End Sub

		''' <summary>获取访问次数</summary>
		Public ReadOnly Property Count As Integer
			Get
				Return CountGet + CountSet
			End Get
		End Property

		Public ReadOnly Property Value As V
			Get
				Threading.Interlocked.Increment(CountGet)
				Return _Value
			End Get
		End Property

		Public Sub UpdateValue(value As V)
			Threading.Interlocked.Increment(CountSet)
			_Value = value
		End Sub

	End Class
End Class
