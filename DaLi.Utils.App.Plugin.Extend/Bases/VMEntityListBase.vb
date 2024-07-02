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
' 	项目视图模型批量操作
'
' 	name: Base.VMEntityListBase
' 	create: 2023-10-08
' 	memo: 项目视图模型批量操作
'
' ------------------------------------------------------------

Imports FreeSql
Imports FreeSql.Internal.Model

Namespace Base
	''' <summary>项目视图模型批量操作</summary>
	Public MustInherit Class VMEntityListBase(Of T As {IEntity, Class})
		Inherits VMEntityBase(Of T)

#Region "数据对象"

		''' <summary>数据对象</summary>
		Private _Entities As IEnumerable(Of T)

		''' <summary>总记录数</summary>
		Private _Count As Long

		''' <summary>数据对象</summary>
		Public ReadOnly Property Entities As IEnumerable(Of T)
			Get
				Return If(_Entities, New List(Of T))
			End Get
		End Property

		''' <summary>总记录数</summary>
		Public ReadOnly Property Count As Long
			Get
				Return If(_Count < 1, 0, _Count)
			End Get
		End Property

		''' <summary>数据对象添加初始化操作等</summary>
		Public Sub SetEntities(value As IEnumerable(Of T), Optional count As Long = 0)
			_Entities = value
			_Count = count

			If _Count < 1 AndAlso _Entities.NotEmpty Then _Count = _Entities.Count
		End Sub

		Private Sub SetEntities(entity As T, ParamArray ids As Object())
			SetEntities(Nothing, 0)
			If entity Is Nothing OrElse ids.IsEmpty Then Return

			' 分析对象
			SetEntitiesIDs(ids)
			If Entities.IsEmpty Then Return

			' 附加参数
			For Each item In Entities
				Dim id = item.ID
				item.ClassAssign(entity, False)
				item.ID = id
			Next
		End Sub

		''' <summary>通过标识设置数据对象</summary>
		Public Sub SetEntitiesIDs(ParamArray ids As Object())
			If ids.IsEmpty Then
				SetEntities(Nothing, 0)
				Return
			End If

			Dim idList = ids.
				Select(Function(x) x?.ToString.ToValue(IdType)).
				Where(Function(x) x IsNot Nothing).
				ToList

			Dim query = ValidateQuery(EntityActionEnum.LIST)
			If query Is Nothing Then
				SetEntities(Nothing, 0)
				Return
			End If

			query.Where(Function(x) idList.Contains(x.ID))
			Dim Entities = If(ErrorMessage.IsPass, query.ToList, Nothing)

			SetEntities(Entities, 0)
		End Sub

		''' <summary>从查询获取数据对象</summary>
		Public Sub SetEntitiesQuery(queryVM As QueryBase(Of T))
			If queryVM Is Nothing Then SetEntities(Nothing, 0)

			Dim query = ValidateQuery(EntityActionEnum.LIST, queryVM)
			If query Is Nothing Then
				SetEntities(Nothing, 0)
				Return
			End If

			' 总记录数
			Dim Count = query.Count
			If Count < 1 Then
				SetEntities(Nothing, 0)
			Else
				SetEntities(query.ToList, Count)
			End If
		End Sub

		''' <summary>标识</summary>
		Public ReadOnly Property IDs As IEnumerable(Of Object)
			Get
				If Entities Is Nothing Then Return Nothing

				Dim pro = EntityType.GetSingleProperty("ID")
				If pro IsNot Nothing Then
					Return Entities.Select(Function(x) pro.GetValue(x)).ToList
				Else
					Return Entities.Select(Function(x) x.ID).ToList
				End If
			End Get
		End Property

#End Region

#Region "获取对象"

		''' <summary>获取单个项目</summary>
		Public Function List() As IEnumerable(Of T)
			Call ValidateList(EntityActionEnum.LIST, Entities)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 操作完成事件
			Return FinishList(EntityActionEnum.LIST, Entities)
		End Function

		''' <summary>获取单个项目，并将结果转换成指定类型</summary>
		Public Function List(Of Q As QueryBase(Of T))(returnSimple As Boolean) As List(Of IDictionary(Of String, Object))
			Dim entities = List()
			If entities Is Nothing Then Return Nothing

			' 结果处理
			Dim queryVM = Activator.CreateInstance(Of Q)()
			Return entities.Select(Function(x) queryVM.ConvertDictionary(x, returnSimple, Db)).ToList
		End Function

#End Region

#Region "批量添加"

		''' <summary>批量添加，不校验唯一字段</summary>
		Public Overridable Function Add() As List(Of T)
			Dim list = ValidateAdd(Entities)
			If Not ErrorMessage.IsPass Then Return Nothing

			Try
				' 入库
				Using repo = Db.GetRepository(Of T)
					list = repo.Insert(list)
				End Using

				' 编辑失败
				If list.IsEmpty Then
					ErrorMessage.Notification = "Error.Action"
					Return Nothing
				End If

				' 入库后处理
				For Each item In list
					' 保存字典
					DictionaryUpdate(item, DictionaryLoad(item, False))
				Next

				Return FinishAdd(list)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Nothing
			End Try
		End Function

		''' <summary>批量添加，不校验唯一字段</summary>
		Public Overridable Async Function AddAsync() As Task(Of List(Of T))
			Dim list = ValidateAdd(Entities)
			If Not ErrorMessage.IsPass Then Return Nothing

			Try
				' 入库
				Using repo = Db.GetRepository(Of T)
					list = Await repo.InsertAsync(list)
				End Using

				' 编辑失败
				If list.IsEmpty Then
					ErrorMessage.Notification = "Error.Action"
					Return Nothing
				End If

				' 入库后处理
				For Each item In list
					' 保存字典
					DictionaryUpdate(item, DictionaryLoad(item, False))
				Next

				Return FinishAdd(list)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Nothing
			End Try
		End Function

#End Region

#Region "批量编辑"

		''' <summary>批量编辑，编辑一组对象</summary>
		''' <param name="skipAudit">是否忽略数据审计，控制器审核属性有设置，则以设置为准，否则此参数为准</param>
		Public Overridable Function Edit(updateAll As Boolean, skipAudit As Boolean) As Integer
			Dim list = ValidateEdit(Entities, updateAll, skipAudit)
			If Not ErrorMessage.IsPass Then Return -1

			Try
				' 入库
				Dim count = Db.Update(Of T).SetSource(list).ExecuteAffrows()

				' 编辑失败
				If count < 1 Then
					ErrorMessage.Notification = "Error.Action"
					Return 0
				End If

				' 入库后保存字典
				For Each item In list
					DictionaryUpdate(item, DictionaryLoad(item, False))
				Next

				FinishEdit(list)

				Return count
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return -1
			End Try
		End Function

		''' <summary>批量编辑，将一组对象修改成相同的数据</summary>
		Public Overridable Function Edit(entity As T, ids As Object(), skipAudit As Boolean) As Integer
			SetEntities(entity, ids)
			Return Edit(False, skipAudit)
		End Function

		''' <summary>批量编辑，编辑一组对象</summary>
		''' <param name="skipAudit">是否忽略数据审计，控制器审核属性有设置，则以设置为准，否则此参数为准</param>
		Public Overridable Async Function EditAsync(updateAll As Boolean, skipAudit As Boolean) As Task(Of Integer)
			Dim list = ValidateEdit(Entities, updateAll, skipAudit)
			If Not ErrorMessage.IsPass Then Return -1

			Try
				' 入库
				Dim count = Await Db.Update(Of T).SetSource(list).ExecuteAffrowsAsync

				' 编辑失败
				If count < 1 Then
					ErrorMessage.Notification = "Error.Action"
					Return 0
				End If

				' 入库后保存字典
				For Each item In list
					DictionaryUpdate(item, DictionaryLoad(item, False))
				Next

				FinishEdit(list)

				Return count
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return -1
			End Try
		End Function

		''' <summary>批量编辑，将一组对象修改成相同的数据</summary>
		Public Overridable Async Function EditAsync(entity As T, ParamArray ids As Object()) As Task(Of Integer)
			SetEntities(entity, ids)
			Return Await EditAsync(False, False)
		End Function

#End Region

#Region "批量删除"

		''' <summary>批量删除</summary>
		Public Overridable Function Delete() As Integer
			Dim list = ValidateDelete(Entities)
			If Not ErrorMessage.IsPass Then Return -1

			Try
				Dim count = 0

				' 真实删除
				Dim items = list.Where(Function(x) x.Value).Select(Function(x) x.Key).ToArray
				If items.NotEmpty Then count += Db.Delete(Of T)(items).ExecuteAffrows

				' 删除标记
				items = list.Where(Function(x) Not x.Value).Select(Function(x) x.Key).ToArray
				If items.NotEmpty Then count += Db.Update(Of T).SetSource(items).ExecuteAffrows

				' 删除失败
				If count < 1 Then
					ErrorMessage.Notification = "Error.Action"
					Return 0
				End If

				' 删除后处理
				For Each item In list.Keys
					' 保存字典
					DictionaryRemove(item.ID)
				Next

				FinishDelete(list.Keys)

				Return count
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return -1
			End Try
		End Function

		''' <summary>批量删除</summary>
		Public Overridable Async Function DeleteAsync() As Task(Of Integer)
			Dim list = ValidateDelete(Entities)
			If Not ErrorMessage.IsPass Then Return -1

			Try
				Dim count = 0

				' 真实删除
				Dim items = list.Where(Function(x) x.Value).Select(Function(x) x.Key).ToArray
				If items.NotEmpty Then count += Await Db.Delete(Of T)(items).ExecuteAffrowsAsync

				' 删除标记
				items = list.Where(Function(x) Not x.Value).Select(Function(x) x.Key).ToArray
				If items.NotEmpty Then count += Await Db.Update(Of T).SetSource(items).ExecuteAffrowsAsync

				' 删除失败
				If count < 1 Then
					ErrorMessage.Notification = "Error.Action"
					Return 0
				End If

				' 删除后处理
				For Each item In list.Keys
					' 保存字典
					DictionaryRemove(item.ID)
				Next

				FinishDelete(list.Keys)

				Return count
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return -1
			End Try
		End Function

#End Region

#Region "查询数据"

		''' <summary>查询项目，并将结果转换成指定类型</summary>
		Public Function QueryList(queryVM As QueryBase(Of T)) As (Data As List(Of T), Count As Long, Pages As Integer)
			Dim queryInfo = ValidateQueryWithPage(EntityActionEnum.LIST, queryVM)
			If queryInfo.Count < 1 Then Return Nothing

			' 查询记录
			Dim entities = queryInfo.query.ToList

			Call ValidateList(EntityActionEnum.LIST, entities)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 操作完成事件
			entities = FinishList(EntityActionEnum.LIST, entities)
			If entities Is Nothing Then Return Nothing

			' 返回数据
			Return (entities, queryInfo.Count, queryInfo.Pages)
		End Function


		''' <summary>查询项目，并将结果转换成指定类型</summary>
		Public Function QueryList(queryVM As QueryBase(Of T), Optional returnSimple As Boolean = False) As (Data As List(Of IDictionary(Of String, Object)), Count As Long, Pages As Integer)
			Dim queryInfo = QueryList(queryVM)
			If queryInfo.Count < 1 Then Return Nothing

			' 转换数据
			Dim data = queryInfo.Data.Select(Function(x) queryVM.ConvertDictionary(x, returnSimple, Db)).ToList

			' 返回数据
			Return (data, queryInfo.Count, queryInfo.Pages)
		End Function

		''' <summary>获取树级列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
		''' <param name="parentId">上级</param>
		''' <param name="nameField">标题字段</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="extField">扩展字段</param>
		''' <param name="queryVM">查询条件</param>
		''' <param name="treeUpdate">二次处理结果</param>
		Public Function QueryDataTree(Of TParent As {IEntityParent, Class})(
																		   parentId As String,
																		   Optional nameField As String = Nothing,
																		   Optional disabledField As String = Nothing,
																		   Optional extField As String = Nothing,
																		   Optional queryVM As QueryBase(Of T) = Nothing,
																		   Optional treeUpdate As Func(Of TParent, DataTree, DataTree) = Nothing
																		) As List(Of DataTree)
			' 分析名称键
			Dim pros = GetType(TParent).
				GetAllProperties.
				Select(Function(x) x.Name).
				ToList

			' 名称键
			nameField = {nameField, "title", "name", "id"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 禁用键
			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 是否禁用反向，有可能字段是允许，所以要反向
			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

			' 扩展内容键
			extField = If(pros.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

			' 主键类型
			Dim IdType = GetType(TParent).GetSingleProperty("ID").PropertyType

			' 查询所有数据
			Dim queryAction As Func(Of String, List(Of DataTree)) = Function(id)
																		Dim Ret As New List(Of DataTree)

																		' 查询
																		Dim query = Db.Select(Of TParent)
																		queryVM?.QueryExecute(query)

																		' 上级非空则查询上级，否则查询空上级
																		If id.NotEmpty Then
																			Dim tureId = id.ToValue(IdType)
																			query.Where(Function(x) x.ParentId.Equals(tureId))
																		Else
																			If IdType.IsString Then
																				query.Where("ParentId Is Null Or ParentId = ''")

																			ElseIf IdType.IsNumber Then
																				query.Where("ParentId Is Null Or ParentId = 0")

																			Else
																				query.Where(Function(x) x.ParentId Is Nothing)
																			End If
																		End If

																		query.ToList.
																			ForEach(Sub(item)
																						Dim fields = {"ID", "ParentId", nameField, disabledField, extField}.Where(Function(x) x.NotEmpty).ToArray

																						' 数据结果对象
																						Dim dic = item.ToDictionary(False, fields)
																						Dim data As New DataList(dic, nameField, "ID", "ParentId", disabledField, extField)
																						If data.Value IsNot Nothing Then
																							' disabled 需要反转
																							If isRev Then data.Disabled = Not data.Disabled

																							Dim tree As New DataTree(data)

																							' 查询下级数据，标识为空则不再查询
																							tree.Children = queryAction(tree.Value)

																							' 二次处理值
																							tree = If(treeUpdate?.Invoke(item, tree), tree)

																							Ret.Add(tree)
																						End If
																					End Sub)

																		Return Ret
																	End Function

			' 执行查询
			Return queryAction(parentId)
		End Function

		''' <summary>获取列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
		''' <param name="nameField">标题字段</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="extField">扩展字段</param>
		''' <param name="queryVM">查询条件</param>
		''' <param name="listUpdate">二次处理结果</param>
		Public Function QueryDataList(
									 Optional nameField As String = Nothing,
									 Optional disabledField As String = Nothing,
									 Optional extField As String = Nothing,
									 Optional queryVM As QueryBase(Of T) = Nothing,
									 Optional listUpdate As Func(Of T, DataList, DataList) = Nothing
									) As List(Of DataList)
			' 分析名称键
			Dim pros = GetType(T).
				GetAllProperties.
				Select(Function(x) x.Name).
				ToList

			' 上级
			Dim parentField = If(pros.Contains("ParentId", StringComparer.OrdinalIgnoreCase), "ParentId", "")

			' 名称键
			nameField = {nameField, "title", "name", "id"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 禁用键
			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 描述键
			extField = If(pros.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

			' 是否禁用反向，有可能字段是允许，所以要反向
			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

			' 主键类型
			Dim IdType = GetType(T).GetSingleProperty("ID").PropertyType

			' 默认查询对象
			Dim query = ValidateQuery(EntityActionEnum.LIST, queryVM)
			If query Is Nothing Then Return Nothing

			' 最大返回记录数
			Dim pageMax = 10000

			' 更新查询条件，参数
			If queryVM IsNot Nothing Then
				pageMax = queryVM.Max.Range(10, queryVM.MaxCount)
				queryVM.QueryExecute(query)
			End If

			' 查询结果
			Return query.Take(pageMax).ToList?.Select(Function(item)
														  ' 要查询的字段，为空则过滤
														  Dim fields = {"ID", nameField, parentField, disabledField, extField}.Where(Function(x) x.NotEmpty).ToArray

														  Dim dic = item.ToDictionary(False, fields)
														  Dim data As New DataList(dic, nameField, "ID", "ParentId", disabledField, extField)

														  ' disabled 需要反转
														  If isRev Then data.Disabled = Not data.Disabled

														  ' 二次处理值
														  data = If(listUpdate?.Invoke(item, data), data)

														  Return data
													  End Function).ToList
		End Function

		''' <summary>获取列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
		''' <param name="name">查询的值</param>
		''' <param name="nameField">查询字段，多个字段用逗号间隔</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="nameUpdate">名称更新函数</param>
		Public Function QueryDataName(
									 name As String,
									 Optional nameField As String = Nothing,
									 Optional disabledField As String = "",
									 Optional valueField As String = "",
									 Optional count As Integer = 25,
									 Optional queryAction As Action(Of ISelect(Of T)) = Nothing,
									 Optional nameUpdate As Func(Of T, String, String) = Nothing
			) As List(Of DataList)
			If name.IsEmpty Then Return Nothing

			' 对于如果 name 为多个数组合并值，则表示查询显示值。一次查询所有值的第一个项目
			If name.Contains(","c) Then
				Dim ids = name.ToLongList
				If ids.NotEmpty Then
					Dim ret As New List(Of DataList)

					For Each id In ids
						Dim items = _QueryDataName(id, nameField, disabledField, valueField, count, queryAction, nameUpdate)
						If items IsNot Nothing Then ret.Add(items.FirstOrDefault)
					Next

					Return ret.Where(Function(x) x IsNot Nothing).ToList
				End If
			End If

			Return _QueryDataName(name, nameField, disabledField, valueField, count, queryAction, nameUpdate)
		End Function

		''' <summary>获取列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
		''' <param name="name">查询的值</param>
		''' <param name="nameField">查询字段，多个字段用逗号间隔</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="nameUpdate">名称更新函数</param>
		Private Function _QueryDataName(
									 name As String,
									 Optional nameField As String = Nothing,
									 Optional disabledField As String = "",
									 Optional valueField As String = "",
									 Optional count As Integer = 25,
									 Optional queryAction As Action(Of ISelect(Of T)) = Nothing,
									 Optional nameUpdate As Func(Of T, String, String) = Nothing
			) As List(Of DataList)
			' 分析名称键
			Dim pros = GetType(T).
				GetAllProperties.
				Select(Function(x) x.Name).
				ToList

			' 名称键
			nameField &= ",title,name"
			Dim nameFields = nameField.ToLower.SplitDistinct.Where(Function(x) x <> "id" AndAlso pros.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

			' 禁用键
			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 值键
			valueField = valueField.EmptyValue("ID")
			If Not pros.Contains(valueField, StringComparer.OrdinalIgnoreCase) Then valueField = "ID"

			' 是否禁用反向，有可能字段是允许，所以要反向
			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

			' 返回数量
			count = count.Range(1, 100)

			' 条件
			Dim filter As New DynamicFilterInfo With {
				.Logic = DynamicFilterLogic.Or,
				.Filters = New List(Of DynamicFilterInfo)
			}

			Dim IdType = GetType(T).GetSingleProperty("ID").PropertyType
			Dim Id = name.ToValue(IdType)
			If Id IsNot Nothing Then
				Dim filterID As New DynamicFilterInfo With {.Field = "ID", .[Operator] = DynamicFilterOperator.Eq, .Value = Id}
				filter.Filters.Add(filterID)
			End If

			For Each nameField In nameFields
				Dim filterName As New DynamicFilterInfo With {.Field = nameField, .[Operator] = DynamicFilterOperator.Contains, .Value = name}
				filter.Filters.Add(filterName)
			Next

			' 默认查询对象
			Dim query = ValidateQuery(EntityActionEnum.LIST)
			If query Is Nothing Then Return Nothing

			' 其他查询
			query.WhereDynamicFilter(filter)
			queryAction?.Invoke(query)

			' 数量限制
			query.Take(count)

			' 所有结果字段 
			Dim retuenFields = nameFields.Union({disabledField, valueField}).Distinct.ToArray

			' 是否 id
			Dim isId = name.ToLong.ToString = name

			' 查询结果
			Return query.ToList?.Select(Function(item)
											Dim dic = item.ToDictionary(False, retuenFields)
											Dim data As New DataList(dic, Nothing, valueField, Nothing, disabledField)

											' disabled 需要反转
											If isRev Then data.Disabled = Not data.Disabled

											' 从所有名称字段中分析值
											Dim text = ""
											For Each nameField In nameFields
												text = dic.Where(Function(x) x.Key.Equals(nameField, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString
												If text.NotEmpty AndAlso (isId OrElse text.Contains(name, StringComparison.OrdinalIgnoreCase)) Then
													data.Text = text
													Exit For
												End If
											Next

											' 如果获取不到值，则直接返回 标识
											If data.Text.IsEmpty Then data.Text = data.Value

											' 如果存在自定义名称函数，更新名称
											If nameUpdate IsNot Nothing Then
												text = nameUpdate.Invoke(item, data.Text)
												If text.NotEmpty Then data.Text = text
											End If

											Return data
										End Function).
										Distinct(Function(x) x.Text).ToList
		End Function

#End Region

	End Class
End Namespace