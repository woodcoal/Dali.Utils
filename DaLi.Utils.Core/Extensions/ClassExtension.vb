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
' 	类相关扩展操作
'
' 	name: Extension.ClassExtension
' 	create: 2022-06-28
' 	memo: 类相关扩展操作
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension
	Public Module ClassExtension

		''' <summary>类相互转换，适合 Entity 转 Dto</summary>
		''' <param name="this">要转换的原始类型</param>
		''' <param name="simple">是否仅转换概要（True：仅转换概要，False：不限制）注意是否输的属性出来自源数据</param>
		<Extension>
		Public Function ClassConvert(Of TEntity As Class, TDto As {Class, New})(this As TEntity, Optional simple As Boolean = False) As TDto
			Dim proEnts = GetType(TEntity).GetOutputFields(True)
			If proEnts.IsEmpty Then Return Nothing

			Dim proDTOs = GetType(TDto).GetAllProperties
			If proDTOs.IsEmpty Then Return Nothing

			' 分析处理
			Dim ret As New TDto

			For Each pro In proDTOs
				' 非公开可写属性不转换
				If Not pro.IsPublic OrElse Not pro.CanWrite Then Continue For

				' 非公开可读属性不转换
				Dim ent = proEnts.Where(Function(x) x.Key.Name.IsSame(pro.Name) AndAlso x.Key.PropertyType = pro.PropertyType).FirstOrDefault
				If ent.Key Is Nothing Then Continue For

				' 获取转换内容
				Dim value = ent.Key.GetValue(this)
				If value Is Nothing Then Continue For

				' 简要模式简化内容
				If simple And ent.Value > 0 AndAlso pro.PropertyType.IsString Then value = value.ToString.ShortShow(ent.Value)

				' 赋值
				pro.SetValue(ret, value)
			Next

			Return ret

		End Function

		''' <summary>类相互转换，适合 Entity 转 Dto</summary>
		''' <param name="this">要转换的原始类型</param>
		''' <param name="simple">是否仅转换概要（True：仅转换概要，False：不限制）注意是否输的属性出来自源数据</param>
		<Extension>
		Public Function ClassConvert(Of TEntity As Class)(this As TEntity, dtoType As Type, Optional simple As Boolean = False) As Object
			If dtoType Is Nothing Then Return Nothing

			Dim proEnts = GetType(TEntity).GetOutputFields(True)
			If proEnts.IsEmpty Then Return Nothing

			Dim proDTOs = dtoType.GetAllProperties
			If proDTOs.IsEmpty Then Return Nothing

			' 分析处理
			Dim ret = Activator.CreateInstance(dtoType)

			For Each pro In proDTOs
				' 非公开可写属性不转换
				If Not pro.IsPublic OrElse Not pro.CanWrite Then Continue For

				' 非公开可读属性不转换
				Dim ent = proEnts.Where(Function(x) x.Key.Name.IsSame(pro.Name) AndAlso x.Key.PropertyType = pro.PropertyType).FirstOrDefault
				If ent.Key Is Nothing Then Continue For

				' 获取转换内容
				Dim value = ent.Key.GetValue(this)
				If value Is Nothing Then Continue For

				' 简要模式简化内容
				If simple And ent.Value > 0 AndAlso pro.PropertyType.IsString Then value = value.ToString.ShortShow(ent.Value)

				' 赋值
				pro.SetValue(ret, value)
			Next

			Return ret
		End Function

		'''' <summary>转换成对象</summary>
		'''' <param name="this">要输出的对象</param>
		'''' <param name="returnSimple">返回详情还是概要</param>
		'''' <param name="extensions">扩展写入数据</param>
		'<Extension>
		'Public Function ToDictionary(Of T As Class)(this As T, returnSimple As Boolean, Optional extensions As IDictionary(Of String, Object) = Nothing) As IDictionary(Of String, Object)
		'	Dim ret As New KeyValueDictionary(extensions)
		'	If this Is Nothing OrElse Not this.GetType.IsExtendClass Then Return ret

		'	Dim fields = this.GetType.GetOutputFields
		'	If fields.IsEmpty Then Return ret

		'	' 字段分析
		'	fields.
		'		Where(Function(x) Not returnSimple OrElse x.Value >= 0).
		'		ToList.
		'		ForEach(Sub(pro)
		'					Dim value = pro.Key.GetValue(this)
		'					If value IsNot Nothing Then
		'						' 是否需要进一步获取数据
		'						' 1. 来自列表的数据，枚举直接返回
		'						' 2. 来自自定义类的数据需要进一步分析
		'						' 3. 其他设置长度的对象需要分析返回长度
		'						' 4. 最后直接返回
		'						Dim proType = pro.Key.PropertyType

		'						If proType.IsDictionary Then
		'							' 是否字典对象（字典要早于列表）
		'							Dim dic = TryCast(value, IDictionary)
		'							Dim dicValue As New Dictionary(Of Object, Object)

		'							For Each key In dic.Keys
		'								dicValue.TryAdd(key, ToDictionary(dic(key), returnSimple))
		'							Next

		'							value = dicValue

		'						ElseIf proType.IsList Then
		'							' 是否列表对象
		'							Dim list = TryCast(value, IEnumerable(Of Object))
		'							Dim listValue As New List(Of Object)

		'							For Each item In list
		'								listValue.Add(ToDictionary(item, returnSimple))
		'							Next

		'							value = listValue

		'						ElseIf proType.IsExtendClass Then
		'							' 进一步分析
		'							value = ToDictionary(value, returnSimple)

		'							' 字符串截取长度
		'						ElseIf pro.Value > 0 AndAlso returnSimple Then
		'							value = value.ToString.ShortShow(pro.Value, " --- ")
		'						End If
		'					End If

		'					ret.TryAdd(pro.Key.Name, value)
		'				End Sub)

		'	Return ret
		'End Function

		''' <summary>转换成对象</summary>
		''' <param name="this">要输出的对象</param>
		''' <param name="returnSimple">返回详情还是概要</param>
		''' <param name="extensions">扩展写入数据</param>
		<Extension>
		Public Function ToDictionary(Of T As Class)(this As T, returnSimple As Boolean, Optional extensions As IDictionary(Of String, Object) = Nothing) As IDictionary(Of String, Object)
			Dim ret As New KeyValueDictionary(extensions)
			If this Is Nothing OrElse Not this.GetType.IsExtendClass Then Return ret

			Dim dic = TryCast(ToDictionary(this, returnSimple, 0), IDictionary(Of String, Object))
			If dic.NotEmpty Then
				For Each item In dic
					ret.TryAdd(item.Key, item.Value)
				Next
			End If

			Return ret
		End Function

		''' <summary>转换成对象</summary>
		''' <param name="this">要输出的对象</param>
		''' <param name="len">要截取的长度,0 不限，大于 0 实际长度，小于 0 文本最大长度 500</param>
		<Extension>
		Public Function ToDictionary(Of T)(this As T, returnSimple As Boolean, len As Integer) As Object
			If this Is Nothing Then Return Nothing

			Dim ret As Object = this

			Dim type = this.GetType
			' 值类型数据，且设置长度则需要截取
			If type.IsString Then
				If len <> 0 AndAlso returnSimple Then
					len = len.Range(1, 500)
					ret = this.ToString.ShortShow(len, " …… ")
				End If

			ElseIf type.IsDictionary Then
				' 是否字典对象（字典要早于列表）
				Dim dic = TryCast(this, IDictionary)
				Dim dicValue As New Dictionary(Of Object, Object)

				For Each key In dic.Keys
					dicValue.TryAdd(key, ToDictionary(dic(key), returnSimple, len))
				Next

				ret = dicValue

			ElseIf type.IsList Then
				' 是否列表对象
				Dim list = TryCast(this, IEnumerable(Of Object))
				Dim listValue As New List(Of Object)

				For Each item In list
					listValue.Add(ToDictionary(item, returnSimple, len))
				Next

				ret = listValue

			ElseIf type.IsExtendClass Then
				' 进一步分析
				Dim fields = type.GetOutputFields(returnSimple)
				If fields.IsEmpty Then Return Nothing

				' 字段分析
				Dim dic As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
				fields.Where(Function(x) Not returnSimple OrElse x.Value >= -1).
					ToList?.
					ForEach(Sub(pro)
								Dim value = pro.Key.GetValue(this)
								value = ToDictionary(value, returnSimple, pro.Value)
								'If value IsNot Nothing Then dic.TryAdd(pro.Key.Name, value)

								' 不论是否存在有效值，只要字段存在则必须返回内容，否则会导致数据结构不一致
								dic.TryAdd(pro.Key.Name, value)
							End Sub)
				ret = dic
			End If

			Return ret
		End Function

		''' <summary>项目值转换成列表</summary>
		<Extension>
		Public Function ToDictionary(Of T As Class)(this As T, returnSimple As Boolean, returnFields As IEnumerable(Of String)) As IDictionary(Of String, Object)
			Dim ret = this.ToDictionary(returnSimple)

			If ret.NotEmpty AndAlso returnFields.NotEmpty Then
				Return ret.Where(Function(x) returnFields.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(Function(x) x.Key, Function(x) x.Value)
			Else
				Return ret
			End If
		End Function

		''' <summary>项目值转换成列表</summary>
		<Extension>
		Public Function ToDictionaries(Of T As Class)(this As IEnumerable(Of T), returnSimple As Boolean, returnFields() As String) As List(Of IDictionary(Of String, Object))
			Return this?.
				Select(Function(x) x.ToDictionary(returnSimple, returnFields)).
				Where(Function(x) x.NotEmpty).
				ToList
		End Function

		''' <summary>将目标类中的字段替换当前类中的字段，以当前类中公用可写属性为标准，使用目标类中的数据进行替换，不存在则不处理</summary>
		''' <param name="this">原类型</param>
		''' <param name="target">目标数据</param>
		''' <param name="replaceNothing">如果目标中对应的值为空是否替换</param>
		''' <returns>替换后的类</returns>
		<Extension>
		Public Function ClassAssign(Of T As Class)(this As T, target As Object, replaceNothing As Boolean) As T
			If target Is Nothing Then Return this
			If this Is Nothing Then Return target

			Dim proSource = this.GetType.GetAllProperties
			If proSource.IsEmpty Then Return this

			Dim proTarget = target.GetType.GetAllProperties
			If proTarget.IsEmpty Then Return this

			proSource.
				Where(Function(x) x.IsPublic AndAlso x.CanWrite).
				ToList.
				ForEach(Sub(x)
							Dim pro = proTarget.Where(Function(y) y.CanRead AndAlso x.Name.IsSame(y.Name)).FirstOrDefault
							If pro IsNot Nothing Then
								Dim value = pro.GetValue(target)
								If replaceNothing OrElse value IsNot Nothing Then x.SetValue(this, value)
							End If
						End Sub)

			Return this
		End Function

		''' <summary>将目标类中的字段替换当前类中的字段，使用目标类中的数据进行替换，存在则替换，不存在添加</summary>
		''' <returns>替换后的类</returns>
		Public Function ClassAssign(Of T As Class)(ParamArray objs As T()) As IDictionary(Of String, Object)
			If objs.IsEmpty Then Return Nothing

			Dim ret As New KeyValueDictionary
			objs.ToDictionaries(False, Nothing).ForEach(Sub(x) ret.UpdateRange(x))

			Return ret
		End Function
	End Module
End Namespace
