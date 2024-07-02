' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	对象数组，仅一个时返回对象，否则返回数组
'
' 	name: Model.ObjectArray
' 	create: 2023-10-10
' 	memo: 对象数组，仅一个时返回对象，否则返回数组
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>对象数组，仅一个时返回对象，否则返回数组</summary>
	Public Class ObjectArray

		''' <summary>构造</summary>
		Public Shared Function NewObject(Of T)() As ObjectArray(Of T)
			Return New ObjectArray(Of T)()
		End Function

		''' <summary>构造</summary>
		Public Shared Function NewObject(Of T)(value As T) As ObjectArray(Of T)
			Return New ObjectArray(Of T)(value)
		End Function

		''' <summary>构造</summary>
		Public Shared Function NewObject(Of T)(ParamArray values As T()) As ObjectArray(Of T)
			Return New ObjectArray(Of T)(values)
		End Function

		''' <summary>构造</summary>
		Public Shared Function NewObject(Of T)(values As IEnumerable(Of T)) As ObjectArray(Of T)
			Return New ObjectArray(Of T)(values)
		End Function

	End Class

	''' <summary>对象数组，仅一个时返回对象，否则返回数组</summary>
	Public Class ObjectArray(Of T)

		''' <summary>内置对象</summary>
		Private _Instance As T()

		''' <summary>构造</summary>
		Public Sub New()
			_Instance = Array.Empty(Of T)()
		End Sub

		''' <summary>构造</summary>
		Public Sub New(value As T)
			_Instance = If(value Is Nothing, Array.Empty(Of T)(), {value})
		End Sub

		''' <summary>构造</summary>
		Public Sub New(ParamArray values As T())
			_Instance = If(values, Array.Empty(Of T)())
		End Sub

		''' <summary>构造</summary>
		Public Sub New(values As IEnumerable(Of T))
			_Instance = If(values Is Nothing, Array.Empty(Of T)(), values.ToArray)
		End Sub

		''' <summary>是否存在内容</summary>
		Public Function IsEmpty() As Boolean
			Return _Instance.Length < 1
		End Function

		''' <summary>是否存在多个对象，超过 1 个以上</summary>
		Public Function IsMuti() As Boolean
			Return _Instance.Length > 1
		End Function

		''' <summary>是否存在内容</summary>
		Public Function NotEmpty() As Boolean
			Return _Instance.Length > 0
		End Function

		''' <summary>元素长度</summary>
		Public ReadOnly Property Length As Integer
			Get
				Return _Instance.Length
			End Get
		End Property

		''' <summary>返回单个对象</summary>
		Public Function ToOne(Optional defaultValue As T = Nothing) As T
			Return If(First, defaultValue)
		End Function

		''' <summary>返回数组</summary>
		Public Function ToArray() As T()
			Return _Instance
		End Function

		''' <summary>转换成列表</summary>
		Public Function ToList() As List(Of T)
			Return _Instance.ToList
		End Function

		''' <summary>是否数组列表</summary>
		Public ReadOnly Property IsArray As Boolean
			Get
				Return _Instance.Length > 1
			End Get
		End Property

		''' <summary>获取设置指定项目，设置时如果索引超过指定长度则将在最后附加</summary>
		Default Public ReadOnly Property Item(index As Integer, Optional defaultValue As T = Nothing) As T
			Get
				If index > -1 AndAlso index < _Instance.Length Then
					Return _Instance.GetValue(index)
				Else
					Return defaultValue
				End If
			End Get
		End Property

		''' <summary>获取设置指定项目，设置时如果索引超过指定长度则将在最后附加</summary>
		Default Public Property Item(index As Integer) As T
			Get
				If index > -1 AndAlso index < _Instance.Length Then
					Return _Instance.GetValue(index)
				Else
					Return Nothing
				End If
			End Get
			Set(value As T)
				If index > -1 AndAlso index < _Instance.Length Then
					index = _Instance.Length

					ReDim Preserve _Instance(index)
				End If

				_Instance.SetValue(value, index)
			End Set
		End Property

		''' <summary>向数组最后附加对象</summary>
		Public Function Push(ParamArray values As T()) As T()
			If values?.Length > 0 Then
				Dim index = _Instance.Length
				ReDim Preserve _Instance(index + values.Length - 1)
				For I = 0 To values.Length - 1
					_Instance(I + 0) = values(I)
				Next
			End If
			Return _Instance
		End Function

		''' <summary>移除数组最后一项并返回移除项</summary>
		Public Function Pop() As T
			If _Instance.Length < 1 Then Return Nothing

			Dim ret = _Instance(_Instance.Length - 1)
			ReDim Preserve _Instance(_Instance.Length - 1)

			Return ret
		End Function

		''' <summary>向数组的开头添加一个或多个元素，并返回新数组</summary>
		Public Function Unshift(ParamArray values As T()) As T()
			If values?.Length > 0 Then
				Array.Reverse(_Instance)

				Dim index = _Instance.Length
				ReDim Preserve _Instance(index + values.Length - 1)
				For I = 0 To values.Length - 1
					_Instance(I + 0) = values(I)
				Next

				Array.Reverse(_Instance)
			End If

			Return _Instance
		End Function

		''' <summary>删除并返回数组的第一个元素</summary>
		Public Function Shift() As T
			If _Instance.Length < 1 Then Return Nothing

			Array.Reverse(_Instance)

			Dim ret = _Instance(_Instance.Length - 1)
			ReDim Preserve _Instance(_Instance.Length - 1)

			Array.Reverse(_Instance)

			Return ret
		End Function

		''' <summary>反转数组</summary>
		Public Function Reverse() As T()
			Array.Reverse(_Instance)
			Return _Instance
		End Function

		''' <summary>调整大小</summary>
		Public Function Resize(size As Integer) As T()
			Array.Resize(_Instance, size)
			Return _Instance
		End Function

		''' <summary>获取当前值</summary>
		Public ReadOnly Property Value As Object
			Get
				If _Instance.Length < 1 Then
					Return Nothing
				ElseIf _Instance.Length = 1 Then
					Return _Instance(0)
				Else
					Return _Instance
				End If
			End Get
		End Property

		''' <summary>第一个值</summary>
		Public ReadOnly Property First As T
			Get
				Return If(_Instance.Length > 0, _Instance(0), Nothing)
			End Get
		End Property

		''' <summary>最后一个值</summary>
		Public ReadOnly Property Last As T
			Get
				Return If(_Instance.Length > 0, _Instance(_Instance.Length - 1), Nothing)
			End Get
		End Property

		''' <summary>循环处理</summary>
		Public Sub ForEach(action As Action(Of T))
			If action Is Nothing Then Return

			For I = 0 To _Instance.Length - 1
				action.Invoke(_Instance(I))
			Next
		End Sub

		''' <summary>循环处理</summary>
		Public Sub ForEach(action As Action(Of T, Index))
			If action Is Nothing Then Return

			For I = 0 To _Instance.Length - 1
				action.Invoke(_Instance(I), I)
			Next
		End Sub

		''' <summary>循环处理</summary>
		Public Sub ForEach(action As Action(Of T, Index, T()))
			If action Is Nothing Then Return

			For I = 0 To _Instance.Length - 1
				action.Invoke(_Instance(I), I, _Instance)
			Next
		End Sub

		''' <summary>查找指定项</summary>
		Public Function Find(where As Func(Of T, Index, Boolean)) As T
			If where Is Nothing Then Return Nothing

			For I = 0 To _Instance.Length - 1
				Dim item = _Instance(I)
				If where.Invoke(item, I) Then Return item
			Next

			Return Nothing
		End Function

		''' <summary>查找指定项</summary>
		Public Function Find(where As Func(Of T, Boolean)) As T
			If where Is Nothing Then Return Nothing

			For I = 0 To _Instance.Length - 1
				Dim item = _Instance(I)
				If where.Invoke(item) Then Return item
			Next

			Return Nothing
		End Function

		''' <summary>是否存在</summary>
		Public Function Exist(where As Func(Of T, Index, Boolean)) As Boolean
			If where Is Nothing Then Return False

			For I = 0 To _Instance.Length - 1
				If where.Invoke(_Instance(I), I) Then Return True
			Next

			Return False
		End Function

		''' <summary>是否存在</summary>
		Public Function Exist(where As Func(Of T, Boolean)) As Boolean
			If where Is Nothing Then Return False

			For I = 0 To _Instance.Length - 1
				If where.Invoke(_Instance(I)) Then Return True
			Next

			Return False
		End Function

	End Class

End Namespace