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
' 	测试控制器
'
' 	name: TestController
' 	create: 2023-05-31
' 	memo: 用于调式环境参数调试
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Mvc

Namespace Controller

	''' <summary>测试控制器</summary>
	<NoLog>
	<Env(EnvRunEnum.DEBUG)>
	<Route("_test")>
	Public Class TestController
		Inherits ApiControllerBase

#Region "字段模糊查询"

		''' <summary>测试数据</summary>
		Private Shared ReadOnly _Names As New NameValueDictionary

		''' <summary>测试数据</summary>
		Private Shared ReadOnly Property Names As NameValueDictionary
			Get
				' 创建测试数组
				If _Names.IsEmpty Then
					For I = 1 To 1000
						_Names.Add(I, RandomHelper.Chars(6))
					Next
				End If

				Return _Names
			End Get
		End Property

		''' <summary>自动完成参数测试</summary>
		<HttpPost("names")>
		Public Function TestNames() As IActionResult
			Dim name = HttpContext.RequestData.GetValue("name")
			If name.IsEmpty Then Return Succ()

			Dim isValue = HttpContext.RequestData.GetValue("value", False)


			' 是否 ID
			If isValue Then Return Succ(Names.Where(Function(x) x.Key = name).FirstOrDefault)

			' 查询
			Dim ret As New List(Of KeyValueDictionary)
			Dim Index = 0
			Names.ForEach(Sub(key, value)
							  If key = name OrElse value.StartsWith(name, StringComparison.OrdinalIgnoreCase) Then
								  Index += 1
								  Dim disabled = Index Mod 5 = 3
								  ret.Add(New KeyValueDictionary From {{"Text", value}, {"Value", key}, {"Disabled", disabled}})
							  End If
						  End Sub)

			Return Succ(ret)
		End Function

#End Region

#Region "树形数据"

		''' <summary>测试数据</summary>
		Private Shared ReadOnly _Trees As New List(Of KeyValueDictionary)

		''' <summary>测试数据</summary>
		Private Shared ReadOnly Property Trees As List(Of KeyValueDictionary)
			Get

				' 创建测试数组
				SyncLock _Trees
					If _Trees.IsEmpty Then
						Dim Idx = 0

						Dim PMax = Random.Shared.Next(3, 8)
						Dim IMax = Random.Shared.Next(4, 9)
						Dim CMax = Random.Shared.Next(5, 10)

						For P = 0 To PMax
							Idx += 1

							Dim parent = New KeyValueDictionary From {{"Value", Idx}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 1}}
							_Trees.Add(parent)

							For I = 1 To IMax
								Idx += 1

								Dim item = New KeyValueDictionary From {{"Value", Idx}, {"Parent", parent("Value")}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 7}}
								_Trees.Add(item)

								For C = 1 To CMax
									Idx += 1

									Dim child = New KeyValueDictionary From {{"Value", Idx}, {"Parent", item("Value")}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 14}}
									_Trees.Add(child)
								Next
							Next
						Next
					End If
				End SyncLock

				Return _Trees
			End Get
		End Property

		''' <summary>自动完成参数测试</summary>
		<HttpGet("tree")>
		Public Function TestTree() As IActionResult
			Return Succ(Trees)
		End Function

#End Region

#Region "字典数据"

		''' <summary>测试数据</summary>
		Private Shared ReadOnly _Dict As New List(Of KeyValueDictionary)

		''' <summary>测试数据</summary>
		Private Shared ReadOnly Property Dict As List(Of KeyValueDictionary)
			Get

				' 创建测试数组
				SyncLock _Dict
					If _Dict.IsEmpty Then
						Dim Idx = 0

						Dim PMax = Random.Shared.Next(3, 8)
						Dim IMax = Random.Shared.Next(4, 9)
						Dim CMax = Random.Shared.Next(5, 10)
						Dim ZMax = Random.Shared.Next(6, 11)

						Dim GetCount = Function()
										   Dim Count = Random.Shared.Next(-1, 3)
										   If Count < 1 Then Count = -1
										   If Count > 2 Then Count = Random.Shared.Next(2, 10)

										   Return Count
									   End Function

						For P = 0 To PMax
							Idx += 1

							Dim parent = New KeyValueDictionary From {{"Value", Idx}, {"Count", -1}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 1}}
							_Dict.Add(parent)

							For I = 1 To IMax
								Idx += 1

								' 节点类型，组？单选？多选？
								Dim Count = GetCount()
								If Count > 7 Then Count = 1 ' 强制将大于 7 的改为单选，以便增加测试数据几率

								' 单选，多选的下级为值
								Dim IsValue = Count > 0

								Dim item = New KeyValueDictionary From {{"Value", Idx}, {"Parent", parent("Value")}, {"Count", Count}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 7}}
								_Dict.Add(item)

								For C = 1 To CMax
									Idx += 1

									' 下级节点类型
									Dim CCount = If(IsValue, 0, Random.Shared.Next(1, 10))
									If CCount > 5 Then CCount = 1 ' 强制将大于 5 的改为单选，以便增加测试数据几率

									Dim child = New KeyValueDictionary From {{"Value", Idx}, {"Parent", item("Value")}, {"Count", CCount}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 14}}
									_Dict.Add(child)

									' 下级为多选
									If CCount > 0 Then
										For Z = 1 To ZMax
											Idx += 1

											Dim zson = New KeyValueDictionary From {{"Value", Idx}, {"Parent", child("Value")}, {"Count", 0}, {"Text", RandomHelper.Chars(6)}, {"Disabled", Idx Mod 15 = 13}}
											_Dict.Add(zson)
										Next
									End If
								Next
							Next
						Next
					End If
				End SyncLock

				Return _Dict
			End Get
		End Property

		''' <summary>自动完成参数测试</summary>
		<HttpGet("dict")>
		Public Function TestDict() As IActionResult
			Return Succ(Dict)
		End Function

#End Region

	End Class
End Namespace
