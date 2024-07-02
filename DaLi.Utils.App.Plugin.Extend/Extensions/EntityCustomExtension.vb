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
' 	自定义属性相关操作
'
' 	name: Helper.EntityCustomExtension
' 	create: 2024-06-29
' 	memo: 自定义属性相关操作
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.ComponentModel.DataAnnotations
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports FreeSql.DataAnnotations

Namespace Extension
	''' <summary>自定义属性相关操作</summary>
	Public Module EntityCustomExtension

		''' <summary>字段属性缓存</summary>
		Private _FieldsCache As ImmutableDictionary(Of String, ImmutableList(Of (Source As PropertyInfo, Target As PropertyInfo, Action As String, Provider As String, EmptyOnly As Boolean))) = ImmutableDictionary.Create(Of String, ImmutableList(Of (Source As PropertyInfo, Target As PropertyInfo, Action As String, Provider As String, EmptyOnly As Boolean)))

		''' <summary>获取自定义属性列表</summary>
		<Extension>
		Public Function GetEntityCustomAttributes(this As Type) As ImmutableList(Of (Source As PropertyInfo, Target As PropertyInfo, Action As String, Provider As String, EmptyOnly As Boolean))
			Dim typeName = this.FullName
			If _FieldsCache.ContainsKey(typeName) Then Return _FieldsCache.Item(typeName)

			'' 添加，编辑模式下检查关键词
			Dim pros = this.GetAllProperties
			Dim names = pros.Select(Function(x) x.Name).ToList
			Dim list As New List(Of (Source As PropertyInfo, Target As PropertyInfo, Action As String, Provider As String, EmptyOnly As Boolean))

			' 获取所有需要处理的字段属性
			For Each pro In pros
				Dim attrs = pro.GetCustomAttributes(Of EntityCustomAttribute)
				If attrs.IsEmpty Then Continue For

				Dim name = pro.Name
				For Each attr In attrs
					' 未设置操作且未设置操作源，跳过
					If attr.Action.IsEmpty AndAlso attr.Provider.IsEmpty Then Continue For

					' 检查来源字段，无效则设置空值
					Dim source As PropertyInfo = Nothing
					If attr.Source.NotEmpty Then source = pros.Where(Function(x) attr.Source.Equals(x.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault

					Dim action = attr.Action.EmptyValue.ToLower
					Dim provider = attr.Provider.EmptyValue.ToLower

					list.Add((source, pro, action, provider, attr.EmptyOnly))
				Next
			Next

			Dim data = list.ToImmutableList
			_FieldsCache = _FieldsCache.Add(typeName, data)
			Return data
		End Function

		''' <summary>获取自定义属性列表</summary>
		''' <param name="this">类型</param>
		''' <param name="provider">操作源</param>
		''' <param name="enEmptySource">是否允许来源属性为空</param>
		<Extension>
		Public Function GetEntityCustomAttributes(this As Type, provider As String, Optional enEmptySource As Boolean = False) As List(Of (Source As PropertyInfo, Target As PropertyInfo, Action As String, EmptyOnly As Boolean))
			Return this.GetEntityCustomAttributes?.
				Where(Function(x) x.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) AndAlso (x.Source IsNot Nothing OrElse enEmptySource)).
				Select(Function(x) (x.Source, x.Target, x.Action, x.emptyonly)).
				ToList
		End Function

		''' <summary>获取文本字段允许最大的文本长度</summary>
		''' <param name="this">属性</param>
		''' <returns>
		''' 大于 0 表示允许的最大文本长度；
		''' 等于 0 表示不限制；
		''' 小于 0 表示不允许
		''' </returns>
		<Extension>
		Public Function GetStringLength(this As PropertyInfo) As Integer
			If Not this.PropertyType.IsString Then Return -1

			Dim maxLength = this.GetCustomAttribute(Of MaxLengthAttribute)
			If maxLength IsNot Nothing Then Return maxLength.Length

			Dim stringLength = this.GetCustomAttribute(Of StringLengthAttribute)
			If stringLength IsNot Nothing Then Return stringLength.MaximumLength

			Dim dbColumn = this.GetCustomAttribute(Of DbColumnAttribute)
			If dbColumn IsNot Nothing Then Return If(dbColumn.StringLength < 1, 0, dbColumn.StringLength)

			Dim column = this.GetCustomAttribute(Of ColumnAttribute)
			If column IsNot Nothing Then Return If(column.StringLength < 1, 0, column.StringLength)

			Return Integer.MinValue
		End Function

	End Module
End Namespace
