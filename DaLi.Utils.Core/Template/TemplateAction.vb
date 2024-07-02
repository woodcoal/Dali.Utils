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
' 	操作事件
'
' 	name: TemplateAction
' 	create: 2022-12-14
' 	memo: 操作事件
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Text

Namespace Template

	''' <summary>操作事件</summary>
	Public Class TemplateAction

		''' <summary>默认编码</summary>
		Public Shared CHARSET As Encoding = Encoding.UTF8

		''' <summary>默认事件</summary>
		Private Shared _Default As TemplateAction

		''' <summary>默认事件</summary>
		Public Shared ReadOnly Property [Default] As TemplateAction
			Get
				If _Default Is Nothing Then
					_Default = New TemplateAction

					' 基础操作注册
					TemplateHelper.BaseActionRegister(_Default, CHARSET)
				End If

				Return _Default
			End Get
		End Property

		''' <summary>内置操作列表</summary>
		''' <remarks>Func: 传入 原始内容，参数列表，返回值</remarks>
		Private _Instance As ImmutableDictionary(Of String, Func(Of String, NameValueDictionary, String)) = ImmutableDictionary.Create(Of String, Func(Of String, NameValueDictionary, String))

		''' <summary>注册事件</summary>
		Public Function Register(command As String, execute As Func(Of String, NameValueDictionary, String), Optional replace As Boolean = False) As Boolean
			If command.IsEmpty OrElse execute Is Nothing Then Return False

			SyncLock _Instance
				command = command.Trim.ToLower

				If _Instance.ContainsKey(command) Then
					If replace Then
						_Instance = _Instance.SetItem(command, execute)
					Else
						Return False
					End If
				Else
					_Instance = _Instance.Add(command, execute)
				End If
			End SyncLock

			Return True
		End Function

		''' <summary>注销事件</summary>
		Public Function Unregister(command As String) As Boolean
			If command.IsEmpty Then Return False
			command = command.Trim.ToLower

			If Not _Instance.ContainsKey(command) Then Return False

			SyncLock _Instance
				_Instance = _Instance.Remove(command)
			End SyncLock

			Return True
		End Function

		''' <summary>执行操作</summary>
		''' <param name="source">原始内容</param>
		''' <param name="attributes">属性列表，所有名值列表，可以重复，按顺序批次操作</param>
		Public Function Execute(source As String, attributes As IDictionary(Of String, String)) As String
			If source.IsEmpty OrElse attributes.IsEmpty Then Return source

			Return Execute(source, attributes.Select(Function(x) (x.Key, x.Value)))
		End Function

		''' <summary>执行操作</summary>
		''' <param name="source">原始内容</param>
		''' <param name="attributes">属性列表，所有名值列表，可以重复，按顺序批次操作</param>
		Public Function Execute(source As String, attributes As IEnumerable(Of (Key As String, Value As String))) As String
			If attributes.IsEmpty Then Return source

			Try
				Dim max = attributes.Count - 1
				For I = 0 To max
					Dim key = attributes(I).Key.ToLower
					Dim value = attributes(I).Value

					' 带小数点的项目直接忽略
					If key.Contains("."c) Then Continue For

					' 检查是否存在对于的操作函数
					If Not _Instance.ContainsKey(key) Then Continue For

					' 组合操作数据
					Dim Nvs As New NameValueDictionary From {
						{key, value}
					}

					Dim prefix = $"{key}."
					Dim len = prefix.Length

					' 分析后续项目
					For J = I + 1 To max
						Dim attr = attributes(J)

						' 如果前缀与当前标签名一致，表示为当前项目的子数据，否则数据无效
						If attr.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
							Nvs.Add(attr.Key.Substring(len), attr.Value)
							I += 1
						Else
							Exit For
						End If
					Next

					' 执行操作
					source = _Instance(key).Invoke(source, Nvs)
				Next
			Catch ex As Exception
				source = Nothing
			End Try

			Return source
		End Function

		''' <summary>所有键</summary>
		Public Function Keys() As IEnumerable(Of String)
			Return _Instance.Keys
		End Function

	End Class

End Namespace