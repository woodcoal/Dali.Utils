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
' 	类型相关扩展操作
'
' 	name: Extension.PropertyExtension
' 	create: 2021-01-21
' 	memo: 类型相关扩展操作
' 	
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>类型相关扩展操作</summary>
	Public Module PropertyExtension

#Region "获取属性列表"

		Private _PropertyCache As ImmutableDictionary(Of String, PropertyInfo()) = ImmutableDictionary.Create(Of String, PropertyInfo())

		''' <summary>获取所有属性列表</summary>
		<Extension>
		Public Function GetAllProperties(this As Type) As PropertyInfo()
			If this Is Nothing Then Return Nothing

			Dim name = this.FullName

			SyncLock _PropertyCache
				If _PropertyCache.ContainsKey(name) Then
					Return _PropertyCache(name)
				Else
					Dim pros = this.GetProperties(BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
					_PropertyCache = _PropertyCache.Add(name, pros)
					Return pros
				End If
			End SyncLock
		End Function

		''' <summary>获取指定属性</summary>
		<Extension>
		Public Function GetSingleProperty(this As Type, name As String) As PropertyInfo
			If this Is Nothing OrElse name.IsEmpty Then Return Nothing

			Dim pros = this.GetAllProperties
			If pros.IsEmpty Then Return Nothing

			Return pros.Where(Function(x) x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault
		End Function

		''' <summary>获取指定属性</summary>
		<Extension>
		Public Function GetSingleProperty(this As Type, where As Func(Of PropertyInfo, Boolean)) As PropertyInfo
			If this Is Nothing OrElse where IsNot Nothing Then Return Nothing

			Dim pros = this.GetAllProperties
			If pros.IsEmpty Then Return Nothing

			Return pros.Where(where).FirstOrDefault
		End Function

#End Region

#Region "获取公共属性列表"

		''' <summary>缓存字段</summary>
		Private _OutputFieldCache As ImmutableDictionary(Of String, ImmutableDictionary(Of PropertyInfo, Integer)) = ImmutableDictionary.Create(Of String, ImmutableDictionary(Of PropertyInfo, Integer))

		''' <summary>锁定对象，以便线程安全</summary>
		Private ReadOnly _LockObject As New Object

		''' <summary>获取类型所有输出属性</summary>
		''' <returns>
		'''  Pro：属性
		'''  Type：False 不显示，Default 未设置，True 仅详情模式显示
		'''  Length：-1 未设置长度，0 全部展示，>1 实际长度显示
		''' </returns>
		Private Function GetOutputProperties(this As Type) As IEnumerable(Of (Pro As PropertyInfo, Type As TristateEnum, Length As Integer))
			Dim pros = this?.GetAllProperties()
			Return pros?.
				Where(Function(pro) pro.CanRead AndAlso pro.IsPublic).
				Select(Function(pro) As (Pro As PropertyInfo, Type As TristateEnum, Length As Integer)
						   Dim attr = pro.GetCustomAttribute(Of OutputAttribute)(True)

						   ' 未设置属性
						   If attr Is Nothing Then
							   ' 对于返回结果为函数，强制不显示
							   Dim proType = pro.PropertyType
							   If proType.Name.StartsWith("Func") Or proType.Name.StartsWith("Action") Then Return (pro, TristateEnum.FALSE, -1)

							   ' 默认输出
							   Return (pro, TristateEnum.DEFAULT, -1)
						   End If

						   'Dim length = attr.Length  ' 字段内容默认显示文本长度

						   'Select Case attr.Status
						   ' Case TristateEnum.FALSE
						   '  ' 此字段不显示, 长度 -2
						   '  length = -2

						   ' Case TristateEnum.TRUE
						   '  ' 强制显示全部内容，长度 0
						   '  length = 0

						   ' Case TristateEnum.DEFAULT
						   '  ' 默认属性值
						   'End Select

						   Return (pro, attr.Status, attr.Length)
					   End Function)
		End Function

		''' <summary>获取所有属性列表</summary>
		''' <param name="this">类型</param>
		''' <param name="returnSimple">是否概要模式，否则返回详情模式</param>
		<Extension>
		Public Function GetOutputFields(this As Type, returnSimple As Boolean) As ImmutableDictionary(Of PropertyInfo, Integer)
			Dim outputFields = ImmutableDictionary.Create(Of PropertyInfo, Integer)
			If this Is Nothing Then Return outputFields

			Dim cacheName = this.FullName & If(returnSimple, "_", "")

			SyncLock _LockObject
				If _OutputFieldCache.ContainsKey(cacheName) Then
					outputFields = _OutputFieldCache(cacheName)
				Else
					' 1. 获取默认属性的参数
					Dim pros = GetOutputProperties(this).
						Where(Function(x) If(returnSimple, x.Type = TristateEnum.DEFAULT, x.Type <> TristateEnum.FALSE)).
						ToDictionary(Function(x) x.Pro, Function(x) x.Length)
					If pros.NotEmpty Then

						' 2. 检查是否存在未设置参数的属性
						If pros.Any(Function(x) x.Value = -1) Then
							' 尝试从接口获取属性
							Dim intrfaces = this.GetInterfaces
							If intrfaces.NotEmpty Then
								' 获取接口中自定义 Output 字段属性数据，未设置的也将排除
								Dim inPros = intrfaces.
									Reverse.
									Select(Function(x) GetOutputProperties(x)?.
										Where(Function(y) If(returnSimple, y.Type = TristateEnum.DEFAULT, y.Type <> TristateEnum.FALSE) AndAlso y.Length > -1).
										Distinct(Function(y) y.Pro.Name).
										ToDictionary(Function(y) y.Pro.Name, Function(y) y.Length)).
									Where(Function(x) x.NotEmpty).
									ToList

								' 存在数据则从接口中分析结果
								If inPros.NotEmpty Then
									'Dim hasReplace = False

									For Each pro In pros.Keys
										If pros(pro) = -1 Then
											For Each inPro In inPros
												If inPro.ContainsKey(pro.Name) Then
													'hasReplace = True
													pros(pro) = inPro(pro.Name)
													Exit For
												End If
											Next
										End If
									Next

									' 存在替换数据，则移除替换后需要隐藏的字段
									'If hasReplace Then pros = pros.
									'	Where(Function(x) x.Value > -2).
									'	ToDictionary(Function(x) x.Key, Function(x) If(x.Value < 0, 0, x.Value))
								End If
							End If
						End If

						' 移除相同名称的字段，移除掉基类属性
						Dim Names = pros.
							Select(Function(x) x.Key.Name.ToLower).
							GroupBy(Function(x) x).
							Where(Function(x) x.Count > 1).
							Select(Function(x) x.Key).
							ToList

						' 存在同名字段
						If Names?.Count > 0 Then
							Names.ForEach(Sub(item)
											  ' 获取所有相关类
											  Dim keys = pros.Where(Function(x) x.Key.Name.ToLower = item).Select(Function(x) x.Key).ToList
											  keys.ForEach(Sub(key)
															   Dim type = key.DeclaringType

															   ' 检查是否基类
															   If keys.Any(Function(x) x <> key AndAlso type.IsAssignableFrom(x.DeclaringType)) Then
																   pros.Remove(key)
															   End If
														   End Sub)
										  End Sub)
						End If

						outputFields = pros.ToImmutableDictionary
					End If

					_OutputFieldCache = _OutputFieldCache.Add(cacheName, outputFields)
				End If
			End SyncLock

			Return outputFields
		End Function

		'''' <summary>获取所有属性列表</summary>
		'<Extension>
		'Public Function GetOutputFields(this As Type) As ImmutableDictionary(Of PropertyInfo, Integer)
		'	If this Is Nothing Then Return Nothing

		'	Dim name = this.FullName
		'	Dim outputFields As ImmutableDictionary(Of PropertyInfo, Integer) = Nothing

		'	SyncLock _LockObject
		'		If _OutputFieldCache.ContainsKey(name) Then
		'			outputFields = _OutputFieldCache(name)
		'		Else
		'			Dim fields As New Dictionary(Of PropertyInfo, Integer)

		'			Dim pros = this.GetAllProperties
		'			If pros.NotEmpty Then
		'				For Each pro In pros
		'					' 属性可读，为公共参数，且不带任何参数
		'					If pro.CanRead AndAlso pro.IsPublic Then
		'						Dim status As TristateEnum = TristateEnum.DEFAULT
		'						Dim length = 0   ' 字段内容默认显示文本长度

		'						' 对于值为函数的字段不处理
		'						Dim proType = pro.PropertyType
		'						If proType.Name.StartsWith("Func`") Or proType.Name.StartsWith("Action`") Then status = TristateEnum.FALSE

		'						Dim attr = pro.GetCustomAttribute(Of OutputAttribute)(True)
		'						If attr IsNot Nothing Then
		'							status = attr.Status
		'							length = attr.Length
		'						End If

		'						Select Case status
		'							Case TristateEnum.FALSE
		'							' 此字段不显示

		'							Case TristateEnum.TRUE
		'								' 仅详情显示
		'								fields.Add(pro, -1)

		'							Case TristateEnum.DEFAULT
		'								' 全显示
		'								' 此时要验证输出字符长度， 最多显示 500 字符
		'								If length > 0 AndAlso (pro.PropertyType.IsString OrElse pro.PropertyType.IsNullableNumber OrElse pro.PropertyType.IsNullableGuid) Then
		'									fields.Add(pro, length.Range(0, 500))
		'								Else
		'									fields.Add(pro, 0)
		'								End If
		'						End Select
		'					End If
		'				Next
		'			End If

		'			' 移除相同名称的字段，移除掉基类属性
		'			Dim Names = fields.
		'				Select(Function(x) x.Key.Name.ToLower).
		'				GroupBy(Function(x) x).
		'				Where(Function(x) x.Count > 1).
		'				Select(Function(x) x.Key).
		'				ToList

		'			' 存在同名字段
		'			If Names?.Count > 0 Then
		'				Names.ForEach(Sub(item)
		'								  ' 获取所有相关类
		'								  Dim keys = fields.Where(Function(x) x.Key.Name.ToLower = item).Select(Function(x) x.Key).ToList
		'								  keys.ForEach(Sub(key)
		'												   Dim type = key.DeclaringType

		'												   ' 检查是否基类
		'												   If keys.Any(Function(x) x <> key AndAlso type.IsAssignableFrom(x.DeclaringType)) Then
		'													   fields.Remove(key)
		'												   End If
		'											   End Sub)
		'							  End Sub)
		'			End If

		'			outputFields = fields.ToImmutableDictionary

		'			_OutputFieldCache = _OutputFieldCache.Add(name, outputFields)
		'		End If
		'	End SyncLock

		'	Return outputFields
		'End Function

#End Region

		''' <summary>是否公共属性，且无参数</summary>
		<Extension>
		Public Function IsPublic(this As PropertyInfo) As Boolean
			If this Is Nothing Then Return False

			Return this.GetMethod.IsPublic AndAlso this.GetIndexParameters.IsEmpty
		End Function

	End Module
End Namespace
