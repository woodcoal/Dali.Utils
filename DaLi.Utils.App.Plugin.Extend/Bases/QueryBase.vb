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
' 	查询基类
'
' 	name: Base.QueryBase
' 	create: 2023-02-17
' 	memo: 查询基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Text.Json.Serialization
Imports FreeSql
Imports FreeSql.Internal.Model

Namespace Base

	''' <summary>搜索基类</summary>
	Public Class QueryBase(Of T As {IEntity, Class})
		Inherits KeyValueDictionary

		''' <summary>分页，最大每页记录数</summary>
		<JsonIgnore>
		Public Overridable ReadOnly Property MaxLimit As Integer = 100

		''' <summary>分页，最大页数</summary>
		<JsonIgnore>
		Public Overridable ReadOnly Property MaxPages As Integer = 1000

		''' <summary>非分页，最多查询数据量</summary>
		<JsonIgnore>
		Public Overridable ReadOnly Property MaxCount As Integer = 10000

		''' <summary>强制返回最多数量，不分页</summary>
		<JsonIgnore>
		Public Property Max As Integer
			Get
				Return GetValue("Max", 0)
			End Get
			Set(value As Integer)
				Item("Max") = value.Range(0, MaxCount)
			End Set
		End Property

		''' <summary>当前页</summary>
		<JsonIgnore>
		Public Property Page As Integer
			Get
				Return GetValue("Page", 1)
			End Get
			Set(value As Integer)
				Item("Page") = value.Range(1, MaxPages)
			End Set
		End Property

		''' <summary>每页数</summary>
		<JsonIgnore>
		Public Property Limit As Integer
			Get
				Return GetValue("Limit", MaxLimit)
			End Get
			Set(value As Integer)
				Item("Limit") = value.Range(1, MaxLimit)
			End Set
		End Property

		''' <summary>字典数据</summary>
		Private _DictionaryProvider As IAppDictionaryProvider

		''' <summary>对象类型</summary>
		Protected ReadOnly EntityType As Type

		''' <summary>排序字段</summary>
		Public ReadOnly DefaultSort As New KeyValueDictionary

		'''' <summary>默认/禁止搜索字段名称</summary>
		Protected ReadOnly DisabledFields As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

		''' <summary>动态查询，注意不要与其他查询冲突，动态查询在默认查询后，排序前，方便查询二次加工</summary>
		<JsonIgnore>
		Public Property DynamicFilter As DynamicFilterInfo

		'''' <summary>查询执行前事件，最先执行</summary>
		Public Event OnExecute(query As ISelect(Of T))

		Public Sub New()
			EntityType = GetType(T)

			' 禁止字段使用的名称，系统已经占用
			DisabledFields.Add("Dictionary")
			DisabledFields.Add("ParentId")
			DisabledFields.Add("Parent")
			DisabledFields.Add("Sort")
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			Me.New()
			AddRangeFast(collection)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, Object))
			Me.New()
			AddRangeFast(dictionary)
		End Sub

		''' <summary>设置默认排序</summary>
		Public Sub SetSort(fieldName As String, isAsc As Boolean)
			If fieldName.IsEmpty Then Exit Sub
			DefaultSort(fieldName) = isAsc
		End Sub

		''' <summary>设置默认排序</summary>
		Public Sub SetSort(fieldExp As Expression(Of Func(Of T, Object)), isAsc As Boolean)
			' 继续检测
			If fieldExp IsNot Nothing Then
				Dim e = TryCast(fieldExp.Body, UnaryExpression)
				If e IsNot Nothing Then
					Dim p = TryCast(e.Operand, MemberExpression)
					Dim name = p?.Member.Name

					SetSort(name, isAsc)
				End If
			End If
		End Sub

		''' <summary>查询过程</summary>
		Public Overridable Sub QueryExecute(query As ISelect(Of T))
			' 查询前事件，优先操作
			RaiseEvent OnExecute(query)

			' 动态查询对象
			Dim Filters As New List(Of DynamicFilterInfo)

			'-----------------------------
			' 从类型中分析需要搜索的字段
			'-----------------------------
			Dim fields = EntityType.GetAllProperties.
						Select(Function(x)
								   Dim attr = x.GetCustomAttribute(Of DbQueryAttribute)(True)
								   If attr?.QueryEnabled Then
									   Return New With {.fieldType = x.PropertyType, .fieldName = x.Name, .queryName = attr.QueryName.EmptyValue(x.Name), .fieldOperator = attr.QueryOperator}
								   Else
									   Return Nothing
								   End If
							   End Function).
						Where(Function(x) x IsNot Nothing AndAlso ContainsKey(x.queryName) AndAlso Not DisabledFields.Contains(x.queryName) AndAlso Item(x.queryName) IsNot Nothing).
						ToList

			' 需要查询搜索
			fields?.ForEach(Sub(field)
								' 字符串操作
								Dim queryOperator As DynamicFilterOperator? = field.fieldOperator
								Dim queryValue As Object = Nothing

								If field.fieldType.IsString Then
									'-----------------
									' 字符串操作
									'-----------------
									Select Case field.fieldOperator
										Case DynamicFilterOperator.Any
											Dim value = GetListValue(field.queryName)
											If value.NotEmpty Then queryValue = value

										Case DynamicFilterOperator.Eq,
											 DynamicFilterOperator.Equal,
											 DynamicFilterOperator.Equals,
											 DynamicFilterOperator.StartsWith,
											 DynamicFilterOperator.EndsWith,
											 DynamicFilterOperator.Contains,
											 DynamicFilterOperator.NotEqual,
											 DynamicFilterOperator.NotStartsWith,
											 DynamicFilterOperator.NotEndsWith,
											 DynamicFilterOperator.NotContains
											Dim value = GetValue(field.queryName)
											If value.NotEmpty Then queryValue = value

										Case Else
											queryOperator = Nothing

									End Select

								ElseIf field.fieldType.IsNullableNumber Then
									'-----------------
									' 数值操作
									'-----------------
									Select Case field.fieldOperator
										Case DynamicFilterOperator.Range
											' 数值
											Dim queryType = Item(field.queryName).GetType
											Dim Min = queryType.GetSingleProperty("Min")?.GetValue(queryValue)
											Dim Max = queryType.GetSingleProperty("Max")?.GetValue(queryValue)

											If Min Is Nothing OrElse Max Is Nothing Then
												Dim value = GetListValue(Of Decimal)(field.queryName)
												If value.NotEmpty AndAlso value.Count > 1 Then
													Min = value(0)
													Max = value(1)
												End If
											End If

											If Min IsNot Nothing AndAlso Max IsNot Nothing Then
												If Min > Max Then Swap(Min, Max)
												queryValue = $"{Min}, {Max}"
											End If

										Case DynamicFilterOperator.Any
											Dim value = GetListValue(Of Decimal)(field.queryName)
											If value.NotEmpty Then queryValue = value.JoinString(",")

										Case DynamicFilterOperator.Eq,
											 DynamicFilterOperator.Equal,
											 DynamicFilterOperator.Equals,
											 DynamicFilterOperator.NotEqual,
											 DynamicFilterOperator.LessThan,
											 DynamicFilterOperator.LessThanOrEqual,
											 DynamicFilterOperator.GreaterThan,
											 DynamicFilterOperator.GreaterThanOrEqual
											queryValue = Item(field.queryName)

										Case Else
											queryOperator = Nothing

									End Select

								ElseIf field.fieldType.IsNullableDate Then
									'-----------------
									' 时间操作
									'-----------------
									Select Case field.fieldOperator
										Case DynamicFilterOperator.DateRange, DynamicFilterOperator.Range
											' 数值
											Dim value = GetValue(Of List(Of Date))(field.queryName)

											If value.NotEmpty AndAlso value.Count > 1 Then
												Dim dateStart = If(value(0).IsValidate, value(0), New Date)
												Dim dateEnd = If(value(1).IsValidate, value(1), New Date)

												If dateStart > dateEnd Then Swap(dateStart, dateEnd)

												' 强制使用日期区间
												field.fieldOperator = DynamicFilterOperator.DateRange
												queryValue = {dateStart, dateEnd} ' $"{value.Start:yyyy-MM-dd HH:mm}, {value.End:yyyy-MM-dd HH:mm}"
											End If

										Case DynamicFilterOperator.Eq,
											 DynamicFilterOperator.Equal,
											 DynamicFilterOperator.Equals,
											 DynamicFilterOperator.NotEqual,
											 DynamicFilterOperator.LessThan,
											 DynamicFilterOperator.LessThanOrEqual,
											 DynamicFilterOperator.GreaterThan,
											 DynamicFilterOperator.GreaterThanOrEqual
											queryValue = Item(field.queryName)

										Case Else
											queryOperator = Nothing

									End Select

								ElseIf field.fieldType.IsNullableEnum Then
									'-----------------
									' 枚举操作
									'-----------------
									Select Case field.fieldOperator
										Case DynamicFilterOperator.Eq,
											 DynamicFilterOperator.Equal,
											 DynamicFilterOperator.Equals,
											 DynamicFilterOperator.NotEqual
											queryValue = GetValue(Of Integer)(field.queryName)

										Case Else
											queryOperator = Nothing
									End Select

								ElseIf field.fieldType.IsNullableBoolean Then
									'-----------------
									' 是否操作
									'-----------------
									Select Case field.fieldOperator
										Case DynamicFilterOperator.Eq,
											 DynamicFilterOperator.Equal,
											 DynamicFilterOperator.Equals,
											 DynamicFilterOperator.NotEqual
											queryValue = GetValue(field.queryName).ToBoolean

										Case Else
											queryOperator = Nothing
									End Select
								End If

								If queryOperator IsNot Nothing AndAlso queryValue IsNot Nothing Then
									Filters.Add(New DynamicFilterInfo With {
											.Field = field.fieldName,
											.[Operator] = queryOperator,
											.Value = queryValue
										})
								End If
							End Sub)

			'-----------------------------
			' 字典数据查询，字典组优先
			'-----------------------------
			' 尝试获取数组，失败则获取文本，都失败则不处理
			Dim dics = GetListValue(Of Long)("Dictionary")?.ToArray
			If dics.IsEmpty Then
				Dim dic = GetValue(Of Long)("Dictionary")
				If dic.NotEmpty Then dics = {dic}
			End If

			If dics.NotEmpty Then
				If _DictionaryProvider Is Nothing Then _DictionaryProvider = SYS.GetService(Of IAppDictionaryProvider)

				' 存在属性，获取属性标识
				Dim Ids = _DictionaryProvider.ModuleValues(EntityType, dics)?.Select(Function(x) x.ModuleValue.Value).ToList

				' 获取到Id，则匹配，否则返回空，无需继续操作
				If Ids.IsEmpty Then
					query.Where("1=0")
					Exit Sub
				Else
					query.WhereContain(Ids, Function(x) x.ID)
				End If
			End If

			'-----------------------------
			' 上级数据查询
			'-----------------------------
			If EntityType.IsComeFrom(GetType(IEntityParent)) Then
				' 上级类型
				Dim proType = GetType(T).GetSingleProperty("ParentId").PropertyType

				' 检查是否存在上级参数
				Dim parentName = MyBase.Where(Function(x) {"ParentIds", "ParentId", "Parent"}.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).Select(Function(x) x.Key).FirstOrDefault

				' 获取上级
				Dim pids = GetListValue(parentName, proType)
				If pids.IsEmpty Then
					' 非数组或不存在，尝试获取单个上级
					Dim pid = GetValue(parentName, proType)

					' 存在单个上级数据
					If pid IsNot Nothing Then
						Filters.Add(New DynamicFilterInfo With {
									.Field = "ParentId",
									.[Operator] = DynamicFilterOperator.Equals,
									.Value = pid
								})
					End If
				Else
					Filters.Add(New DynamicFilterInfo With {
								.Field = "ParentId",
								.[Operator] = DynamicFilterOperator.Any,
								.Value = pids
								})
				End If
			End If

			'-----------------------------
			' 扩展内容查询
			'-----------------------------
			If EntityType.IsComeFrom(GetType(IEntityExtend)) Then
				Dim ext = GetValue("Extension")
				If ext.NotEmpty Then
					Filters.Add(New DynamicFilterInfo With {
								.Field = "Extension",
								.[Operator] = DynamicFilterOperator.Contains,
								.Value = ext
								})
				End If
			End If

			' 动态查询
			If Filters.NotEmpty Then
				Dim dy As New DynamicFilterInfo

				If Filters.Count > 1 Then
					dy.Filters = Filters
					dy.Logic = DynamicFilterLogic.And
				Else
					dy = Filters(0)
				End If

				query.WhereDynamicFilter(dy)
			End If

			' 动态查询
			If DynamicFilter IsNot Nothing Then query.WhereDynamicFilter(DynamicFilter)

			'-----------------------------
			' 排序，先添加的字段先排序
			' 支持文本 sort:"ID desc"
			'-----------------------------
			Dim sort As IDictionary(Of String, Object) = Nothing

			Dim sortValue = Item("Sort")
			If sortValue IsNot Nothing Then
				If sortValue.GetType.IsString Then
					' 如果排序为文本字段：xx desc 或者 xx asc，多条逗号间隔，未设置，默认升序
					sort = sortValue.ToString.
							Split(","c).
							Select(Function(x) x.TrimFull.ToLower).
							Distinct.
							Where(Function(x) x.NotEmpty).
							Select(Function(x) $"{x} asc".Split(" "c)).
							Distinct(Function(x) x(0)).
							ToDictionary(Of String, Object)(Function(x) x(0), Function(x) x(1).NotEmpty AndAlso x(1).StartsWith("d"))
				Else
					sort = ChangeType(Of IDictionary(Of String, Object))(sortValue)
				End If
			End If

			If sort.IsEmpty Then sort = DefaultSort.Clone

			' 是否存在有效排序
			Dim hasSort = False

			If sort.NotEmpty Then
				For Each s In sort
					' 检查是否存在此字段
					' 映射为 NotMapped 的字段不能排序
					If s.Key.NotEmpty AndAlso s.Value IsNot Nothing AndAlso s.Value.GetType.IsBoolean Then
						Dim pro = EntityType.GetSingleProperty(s.Key)
						If pro IsNot Nothing AndAlso pro.GetCustomAttribute(Of NotMappedAttribute) Is Nothing Then
							hasSort = True
							query.OrderByPropertyName(s.Key, s.Value)
						End If
					End If
				Next
			End If

			If Not hasSort Then query.OrderByDescending(Function(x) x.ID)

			' 调试模式输出 SQL 语句
			If SYS.Debug Then Debug.WriteLine("正在执行 SQL 语句：" & query.ToSql)
		End Sub

		''' <summary>数据结果输出处理</summary>
		Public Overridable Function ConvertDictionary(item As T, returnSimple As Boolean, db As IFreeSql) As IDictionary(Of String, Object)
			Return item.ToDictionary(returnSimple)
		End Function

	End Class

End Namespace