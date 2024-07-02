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
' 	枚举相关扩展操作
'
' 	name: Extension.EnumExtension
' 	create: 2020-11-14
' 	memo: 枚举相关扩展操作
' 	
' ------------------------------------------------------------

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>枚举相关扩展操作</summary>
	Public Module EnumExtension

		''' <summary>获取指定类型的属性</summary>
		<Extension>
		Public Function Attribute(Of T As System.Attribute)(this As System.Enum) As T
			Dim enumType = this.GetType
			Dim enumName = System.Enum.GetName(enumType, this)
			If enumName.IsEmpty Then Return Nothing

			Dim enumField = enumType.GetField(enumName)
			If enumField Is Nothing Then Return Nothing

			Return enumField.GetCustomAttributes(GetType(T), False).Select(Function(x) TryCast(x, T)).FirstOrDefault
		End Function

#Region "枚举对象操作"

		''' <summary>是否带标志枚举</summary>
		<Extension>
		Public Function IsFlagEnum(this As Type) As Boolean
			If this?.IsEnum Then
				Return this.GetCustomAttribute(Of FlagsAttribute) IsNot Nothing
			Else
				Return Nothing
			End If
		End Function

		''' <summary>枚举所有项目</summary>
		''' <param name="this">枚举类型，其他类型则返回无效内容</param>
		<Extension>
		Public Function EnumValues(this As Type) As List(Of System.Enum)
			If this?.IsEnum Then
				Dim Arr = System.Enum.GetValues(this)
				If Arr.Length > 0 Then
					Dim Ret As New List(Of System.Enum)

					If Arr.Length > 0 Then
						For Each A In Arr
							Ret.Add(System.Enum.Parse(this, A))
						Next

						Return Ret
					End If
				End If
			End If

			Return Nothing
		End Function

		''' <summary>枚举所有项目，并返回枚举与描述字典</summary>
		''' <param name="this">枚举类型，其他类型则返回无效内容</param>
		''' <param name="isName">字典描述为名称还是描述</param>
		<Extension>
		Public Function EnumDictionary(this As Type, Optional isName As Boolean = True) As Dictionary(Of System.Enum, String)
			Dim Enums = this.EnumValues
			If Enums?.Count > 0 Then
				Return Enums.ToDictionary(Function(x) x, Function(x) If(isName, x.Name, x.Description))
			Else
				Return Nothing
			End If
		End Function

		''' <summary>通过名称获取枚举值</summary>
		''' <param name="this">枚举类型，其他类型则返回无效内容</param>
		''' <param name="name">枚举项目名称</param>
		''' <param name="isName">通过 名称(True)、描述(False) 获取值</param>
		<Extension>
		Public Function EnumValue(this As Type, name As String, Optional isName As Boolean = True) As System.Enum
			If this?.IsEnum Then
				If isName Then
					Dim Ret As System.Enum = Nothing
					If System.Enum.TryParse(this, name, True, Ret) Then Return Ret
				Else
					Return this.EnumDictionary(False)?.Where(Function(x) x.Value.IsSame(name)).Select(Function(x) x.Key).FirstOrDefault
				End If
			End If

			Return Nothing
		End Function

		''' <summary>通过名称获取枚举值</summary>
		''' <param name="this">枚举类型，其他类型则返回无效内容</param>
		''' <param name="names">枚举项目名称列表</param>
		''' <param name="isName">通过 名称(True)、描述(False) 获取值</param>
		<Extension>
		Public Function EnumFlagValue(this As Type, names As String(), Optional isName As Boolean = True) As System.Enum
			If this?.IsFlagEnum AndAlso names?.Length > 0 Then
				If isName Then
					Return System.Enum.Parse(this, String.Join(","c, names))
				Else
					Dim Value = this.EnumDictionary(False)?.Where(Function(x) names.Contains(x.Value, StringComparer.OrdinalIgnoreCase)).Sum(Function(x) Convert.ToInt32(x.Key))
					Return System.Enum.Parse(this, Value)
				End If
			End If

			Return Nothing
		End Function

		''' <summary>获取枚举值列表</summary>
		''' <param name="this">枚举类型，其他类型则返回无效内容</param>
		<Extension>
		Public Function FlagValue(this As [Enum]) As List(Of [Enum])
			Dim Ret = this?.GetType.EnumValues?.Where(Function(x) Convert.ToInt32(x) <> 0 AndAlso this.HasFlag(x)).ToList
			If Ret.NotEmpty Then
				Return Ret
			Else
				Return New List(Of [Enum]) From {Nothing}
			End If
		End Function

#End Region

#Region "普通枚举项目操作"

		''' <summary>枚举分类</summary>
		''' <param name="this">枚举</param>
		<Extension>
		Public Function Category(this As System.Enum) As String
			Return this.Attribute(Of CategoryAttribute)?.Category
		End Function

		''' <summary>枚举描述</summary>
		''' <param name="this">枚举</param>
		<Extension>
		Public Function Description(this As System.Enum) As String
			Dim type = this.GetType
			If type.GetCustomAttribute(Of FlagsAttribute) IsNot Nothing Then
				' Flags 枚举
				Dim flagValue = Convert.ToInt32(this)
				If flagValue > 0 Then
					With New List(Of String)
						For Each key In this.GetType.EnumValues
							Dim enumValue = Convert.ToInt32(key)
							If enumValue > 0 AndAlso (enumValue And flagValue) = enumValue Then
								.Add(key.Attribute(Of DescriptionAttribute)?.Description)
							End If
						Next

						Return String.Join(","c, .ToArray)
					End With
				Else
					Return this.Attribute(Of DescriptionAttribute)?.Description
				End If
			Else
				' 普通枚举
				Return this.Attribute(Of DescriptionAttribute)?.Description
			End If
		End Function

		''' <summary>枚举名称</summary>
		''' <param name="this">枚举</param>
		<Extension>
		Public Function Name(this As System.Enum) As String
			Return this?.ToString
		End Function

#End Region

#Region "标志枚举项目操作"

		'''' <summary>Flag枚举中是否包含此项</summary>
		'''' <param name="this">枚举</param>
		'<Extension>
		'Public Function HasFlag(this As System.Enum, target As System.Enum) As Boolean
		'	Dim K = Convert.ToInt32(this)
		'	Dim V = Convert.ToInt32(target)

		'	Return (K And V) = V
		'End Function

		''' <summary>Flag枚举中是否添加项目</summary>
		''' <param name="this">枚举</param>
		<Extension>
		Public Function AddFlag(this As System.Enum, target As System.Enum) As System.Enum
			Return System.Enum.Parse(this.GetType, Convert.ToInt32(this) + Convert.ToInt32(target))
		End Function

		''' <summary>Flag枚举中是否添加项目</summary>
		''' <param name="this">枚举</param>
		<Extension>
		Public Function AddFlag(this As System.Enum()) As System.Enum
			If this.NotEmpty Then
				Return System.Enum.Parse(this(0).GetType, this.Select(Function(x) Convert.ToInt32(x)).Sum)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>Flag枚举中是否移除项目</summary>
		''' <param name="this">枚举</param>
		<Extension>
		Public Function RemoveFlag(this As System.Enum, target As System.Enum) As System.Enum
			If this.HasFlag(target) Then
				Return System.Enum.Parse(this.GetType, Convert.ToInt32(this) - Convert.ToInt32(target))
			Else
				Return this
			End If
		End Function

#End Region


	End Module
End Namespace