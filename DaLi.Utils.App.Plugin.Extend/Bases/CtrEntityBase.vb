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
' 	框架数据基本操作控制器基类
'
' 	name: Base.CtrEntityBase
' 	create: 2023-02-19
' 	memo: 框架数据基本操作控制器基类
'
' ------------------------------------------------------------

Imports FreeSql
Imports Microsoft.AspNetCore.Mvc

Namespace Base

	''' <summary>框架数据基本操作控制器基类</summary>
	Public MustInherit Class CtrEntityBase(Of T As {IEntity, Class}, VMItem As VMEntityItemBase(Of T), VMList As VMEntityListBase(Of T), Q As {QueryBase(Of T), New})
		Inherits CtrBase

#Region "事件"

		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Protected Overridable Sub ExecuteQuery(action As EntityActionEnum, query As ISelect(Of T), Optional queryVM As QueryBase(Of T) = Nothing)
			RaiseEvent OnQuery(action, query, queryVM)
		End Sub

		''' <summary>项目操作之前预处理</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Protected Overridable Sub ExecuteValidate(action As EntityActionEnum, entity As T, Optional source As T = Nothing)
			RaiseEvent OnValidate(action, entity, source)
		End Sub

		''' <summary>项目操作之后</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="data">单项操作时单个数值，多项时为数组</param>
		Protected Overridable Sub ExecuteFinish(action As EntityActionEnum, data As ObjectArray(Of T))
			RaiseEvent OnFinish(action, data)
		End Sub

		''' <summary>执行完成后返回结果</summary>
		''' <param name="action">操作</param>
		''' <param name="result">结果</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Protected Overridable Sub ExecuteReturn(action As EntityActionEnum, result As Object, isSuccess As Boolean?, Optional actionName As String = "")
			RaiseEvent OnReturn(action, result, isSuccess, actionName)
		End Sub

		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Public Event OnQuery(action As EntityActionEnum, query As ISelect(Of T), queryVM As QueryBase(Of T))

		''' <summary>项目操作之前预处理</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Public Event OnValidate(action As EntityActionEnum, entity As T, source As T)

		''' <summary>项目操作之后</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="data">单项操作时单个数值，多项时为数组</param>
		Public Event OnFinish(action As EntityActionEnum, data As ObjectArray(Of T))

		''' <summary>执行完成后返回结果</summary>
		''' <param name="action">操作</param>
		''' <param name="result">结果</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Public Event OnReturn(action As EntityActionEnum, result As Object, isSuccess As Boolean?, actionName As String)

#End Region

#Region "创建视图"

		''' <summary>创建项目视图</summary>
		Protected Overloads Function CreateVM(Of V As VMEntityBase(Of T))() As V
			Dim vm = Activator.CreateInstance(Of V)

			AddHandler vm.OnQuery, AddressOf ExecuteQuery
			AddHandler vm.OnValidate, AddressOf ExecuteValidate
			AddHandler vm.OnFinish, AddressOf ExecuteFinish

			Return vm
		End Function

		''' <summary>创建项目视图</summary>
		Protected Overloads Function CreateVMItem() As VMItem
			Dim vm = CreateVM(Of VMItem)()

			' 赋值基本数据
			UpdateVM(vm)

			Return vm
		End Function

		''' <summary>创建列表视图</summary>
		Protected Overloads Function CreateVMList() As VMList
			Dim vm = CreateVM(Of VMList)()

			' 赋值基本数据
			UpdateVM(vm)

			Return vm
		End Function

		''' <summary>创建项目视图</summary>
		Protected Overloads Function CreateVMItem(id As Object) As VMItem
			Dim vm = CreateVM(Of VMItem)()

			' 赋值基本数据
			UpdateVM(vm)

			' 查询对象
			vm.SetEntityID(id)

			Return vm
		End Function

		''' <summary>创建列表视图</summary>
		Protected Overloads Function CreateVMList(ParamArray ids As Object()) As VMList
			Dim vm = CreateVM(Of VMList)()

			' 赋值基本数据
			UpdateVM(vm)

			' 查询对象
			vm.SetEntitiesIDs(ids)

			Return vm
		End Function

		''' <summary>创建项目视图</summary>
		Protected Overloads Function CreateVMItem(entity As T) As VMItem
			Dim vm = CreateVM(Of VMItem)()

			' 赋值基本数据
			UpdateVM(vm)

			' 查询对象
			vm.SetEntity(entity)

			Return vm
		End Function

		''' <summary>创建列表视图</summary>
		Protected Overloads Function CreateVMList(entities As IEnumerable(Of T)) As VMList
			Dim vm = CreateVM(Of VMList)()

			' 赋值基本数据
			UpdateVM(vm)

			' 查询对象
			vm.SetEntities(entities)

			Return vm
		End Function

		''' <summary>创建项目视图</summary>
		Protected Overloads Function CreateVMItem(queryVM As Q) As VMItem
			Dim vm = CreateVM(Of VMItem)()

			' 赋值基本数据
			UpdateVM(vm)

			' 查询对象
			vm.SetEntityQuery(queryVM)

			Return vm
		End Function

		''' <summary>创建列表视图</summary>
		Protected Overloads Function CreateVMList(queryVM As Q) As VMList
			Dim vm = CreateVM(Of VMList)()

			' 赋值基本数据
			UpdateVM(vm)

			' 查询对象
			vm.SetEntitiesQuery(queryVM)

			Return vm
		End Function

#End Region

#Region "自定义操作"

		''' <summary>通用操作</summary>
		''' <param name="execute">试图创建后的操作</param>
		''' <param name="action">操作</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Protected Function ExecuteByItem(execute As Func(Of VMItem, Object), Optional action As EntityActionEnum = EntityActionEnum.NONE, Optional actionName As String = "") As IActionResult
			Try
				Dim vm = CreateVMItem()

				Dim ret = execute?.Invoke(vm)
				Dim isActionResult = ret IsNot Nothing AndAlso GetType(IActionResult).IsAssignableFrom(ret.GetType)

				Dim Ok = ErrorMessage.IsPass
				ExecuteReturn(action, If(isActionResult, vm.Entity, ret), Ok, actionName)

				If isActionResult Then
					Return ret
				Else
					Return If(Ok, Succ(ret), Err)
				End If
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Err
			End Try
		End Function

		''' <summary>通用操作</summary>
		''' <param name="execute">试图创建后的操作</param>
		''' <param name="action">操作</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Protected Function ExecuteByList(execute As Func(Of VMList, Object), Optional action As EntityActionEnum = EntityActionEnum.NONE, Optional actionName As String = "") As IActionResult
			Try
				Dim vm = CreateVMList()
				Dim ret = execute?.Invoke(vm)
				Dim Ok = ErrorMessage.IsPass

				ExecuteReturn(action, ret, Ok, actionName)

				Return If(Ok, Succ(ret), Err)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Err
			End Try
		End Function

		''' <summary>通过标识操作</summary>
		''' <param name="execute">试图创建后的操作</param>
		''' <param name="action">操作</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Protected Function ExecuteById(id As String, execute As Func(Of T, VMItem, Object), Optional action As EntityActionEnum = EntityActionEnum.NONE, Optional actionName As String = "") As IActionResult
			If id.IsEmpty Then Return Err

			Try
				Dim vm = CreateVMItem(id)
				If vm.Entity Is Nothing Then Return Err("No.NotFound")

				Dim ret = execute?.Invoke(vm.Entity, vm)
				Dim isActionResult = ret IsNot Nothing AndAlso GetType(IActionResult).IsAssignableFrom(ret.GetType)

				Dim Ok = ErrorMessage.IsPass
				ExecuteReturn(action, If(isActionResult, vm.Entity, ret), Ok, actionName)

				If isActionResult Then
					Return ret
				Else
					Return If(Ok, Succ(ret), Err)
				End If
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Err
			End Try
		End Function

		''' <summary>通过标识操作</summary>
		''' <param name="execute">试图创建后的操作</param>
		''' <param name="action">操作</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Protected Function ExecuteByIds(ids As String(), execute As Func(Of IEnumerable(Of T), VMList, Object), Optional action As EntityActionEnum = EntityActionEnum.NONE, Optional actionName As String = "") As IActionResult
			If ids.IsEmpty Then Return Err

			Try
				Dim vm = CreateVMList(ids)
				If vm.Entities Is Nothing Then Return Err("No.NotFound")

				Dim ret = execute?.Invoke(vm.Entities, vm)
				Dim Ok = ErrorMessage.IsPass

				ExecuteReturn(action, ret, Ok, actionName)
				Return If(Ok, Succ(ret), Err)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Err
			End Try
		End Function

		''' <summary>通过对象操作</summary>
		''' <param name="execute">试图创建后的操作</param>
		''' <param name="action">操作</param>
		''' <param name="actionName">批量操作或者其他操作</param>
		Protected Function ExecuteByEntity(entity As T, execute As Func(Of T, VMItem, Object), Optional action As EntityActionEnum = EntityActionEnum.NONE, Optional actionName As String = "") As IActionResult
			If entity Is Nothing Then Return Err

			Try
				Dim vm = CreateVMItem(entity)
				Dim ret = execute?.Invoke(vm.Entity, vm)
				Dim isActionResult = ret IsNot Nothing AndAlso GetType(IActionResult).IsAssignableFrom(ret.GetType)

				Dim Ok = ErrorMessage.IsPass
				ExecuteReturn(action, If(isActionResult, vm.Entity, ret), Ok, actionName)

				If isActionResult Then
					Return ret
				Else
					Return If(Ok, Succ(ret), Err)
				End If
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Err
			End Try
		End Function

		''' <summary>通过查询操作</summary>
		''' <param name="execute">试图创建后的操作</param>
		Protected Function ExecuteByQuery(queryVM As Q, execute As Func(Of IEnumerable(Of T), VMList, Object), Optional action As EntityActionEnum = EntityActionEnum.NONE, Optional actionName As String = "") As IActionResult
			If queryVM Is Nothing Then Return Err

			Try
				Dim vm = CreateVMList(queryVM)
				If vm.Entities Is Nothing Then Return Err("No.NotFound")

				Dim ret = execute?.Invoke(vm.Entities, vm)
				Dim Ok = ErrorMessage.IsPass

				ExecuteReturn(action, ret, Ok, actionName)

				Return If(Ok, Succ(ret), Err)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Err
			End Try
		End Function

#End Region

#Region "获取项目"

		''' <summary>获取项目</summary>
		<HttpGet("{id}")>
		<ResponseCache(Duration:=5)>
		Public Overridable Function Item(id As String) As IActionResult
			Return Item(id, Nothing)
		End Function

		''' <summary>获取项目</summary>
		''' <param name="successAction">获取成功后的操作</param>
		Protected Function Item(id As String, successAction As Action(Of T)) As IActionResult
			If id.IsEmpty Then Return Err

			Dim vm = CreateVMItem()
			Dim Ret = vm.QueryItem(Of Q)(id)
			Dim ok = Ret IsNot Nothing
			If ok Then
				successAction?.Invoke(Ret)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.ITEM, Ret, ok)

			Return If(ok, Succ(Ret), Err)
		End Function

		''' <summary>获取项目</summary>
		''' <param name="successAction">获取成功后的操作</param>
		Protected Function Item(queryVM As Q, successAction As Action(Of T)) As IActionResult
			If queryVM Is Nothing Then Return Err

			Dim vm = CreateVMItem()
			Dim Ret = vm.QueryItem(queryVM)
			Dim ok = Ret IsNot Nothing
			If ok Then
				successAction?.Invoke(Ret)
				ok = ErrorMessage.IsPass
			End If

			Return If(ok, Succ(Ret), Err)
		End Function

#End Region

#Region "添加项目"

		''' <summary>添加项目</summary>
		<HttpPost()>
		Public Overridable Function Add(entity As T) As IActionResult
			Return Add(entity, Nothing)
		End Function

		''' <summary>添加项目</summary>
		''' <param name="successAction">添加成功后的操作</param>
		Protected Function Add(entity As T, successAction As Action(Of T)) As IActionResult
			If entity Is Nothing Then Return Err

			Dim vm = CreateVMItem(entity)
			vm.Add()

			Dim ok = ErrorMessage.IsPass
			If ok Then
				successAction?.Invoke(vm.Entity)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.ADD, vm.Entity, ok)

			Return If(ok, Succ(vm.Entity?.ID), Err)
		End Function

#End Region

#Region "修改项目"

		''' <summary>修改项目</summary>
		<HttpPut()>
		Public Overridable Function Edit(entity As T) As IActionResult
			Return Edit(entity, Nothing)
		End Function

		''' <summary>修改项目</summary>
		''' <param name="successAction">修改成功后的操作</param>
		''' <param name="skipAudit">是否忽略数据审计，控制器审核属性有设置，则以设置为准，否则此参数为准</param>
		Protected Function Edit(entity As T, successAction As Action(Of T), Optional skipAudit As Boolean = False) As IActionResult
			If entity Is Nothing OrElse entity.ID.IsEmpty Then Return Err

			Dim vm = CreateVMItem(entity)
			vm.Edit(False, skipAudit)

			Dim ok = ErrorMessage.IsPass
			If ok Then
				successAction?.Invoke(vm.Entity)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.EDIT, vm.Entity, ok)

			Return If(ok, Succ(vm.Entity?.ID), Err)
		End Function

#End Region

#Region "删除项目"

		''' <summary>删除项目</summary>
		<HttpDelete("{id}")>
		Public Overridable Function Delete(id As String) As IActionResult
			Return Delete(id, Nothing)
		End Function

		''' <summary>删除项目</summary>
		''' <param name="successAction">删除成功后的操作</param>
		Protected Function Delete(id As String, successAction As Action(Of T)) As IActionResult
			If id.IsEmpty OrElse id = "0" Then Return Err

			Dim vm = CreateVMItem(id)
			vm.Delete()

			Dim ok = ErrorMessage.IsPass
			If ok Then
				successAction?.Invoke(vm.Entity)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.DELETE, vm.Entity, ok)

			Return If(ok, Succ(id), Err)
		End Function

		''' <summary>删除项目</summary>
		''' <param name="successAction">删除成功后的操作</param>
		Protected Function Delete(queryVM As Q, successAction As Action(Of T)) As IActionResult
			If queryVM Is Nothing Then Return Err

			Dim vm = CreateVMItem(queryVM)
			vm.Delete()

			Dim ok = ErrorMessage.IsPass
			If ok Then
				successAction?.Invoke(vm.Entity)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.DELETE, vm.Entity, ok)

			Return If(ok, Succ(vm.Entity?.ID), Err)
		End Function

#End Region

#Region "批量操作"

		''' <summary>批量删除项目</summary>
		''' <param name="successAction">删除成功后的操作</param>
		Protected Function BatchDelete(ids() As String, successAction As Action(Of Integer)) As IActionResult
			If ids.IsEmpty Then Return Err

			Dim vm = CreateVMList(ids)
			Dim ret = vm.Delete()
			Dim ok = ret > 0
			If ok Then
				successAction?.Invoke(ret)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.DELETE, ret, ok, "批量删除")

			Return If(ok, Succ(ret), Err)
		End Function

		''' <summary>批量修改项目</summary>
		''' <param name="successAction">删除成功后的操作</param>
		Protected Function BatchEdit(entity As T, successAction As Action(Of Integer), skipAudit As Boolean) As IActionResult
			If entity Is Nothing Then Return Err

			' 从请求中获取 扩展数据
			Dim ids = AppContext.Fields.GetListValue("ext")
			If ids.IsEmpty Then
				ErrorMessage.Notification = "Error.NoData"
				Return Err
			End If

			' 移除 ext
			AppContext.Fields.Remove("ext")

			Dim vm = CreateVMList()
			Dim ret = vm.Edit(entity, ids.ToArray, skipAudit)
			Dim ok = ret > 0
			If ok Then
				successAction?.Invoke(ret)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.EDIT, ret, ok, "批量编辑")

			Return If(ok, Succ(ret), Err)
		End Function

		''' <summary>批量添加项目，不检查字段是否重复</summary>
		''' <param name="successAction">删除成功后的操作</param>
		Protected Function BatchAdd(entities As IEnumerable(Of T), successAction As Action(Of IEnumerable(Of T))) As IActionResult
			If entities Is Nothing Then Return Err

			Dim vm = CreateVMList(entities)
			Dim ret = vm.Add()
			Dim ok = ret?.Count > 0
			If ok Then
				successAction?.Invoke(ret)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.EDIT, ret, ok, "批量编辑")

			Return If(ok, Succ(ret), Err)
		End Function

#End Region

#Region "查询项目"

		''' <summary>查询项目</summary>
		<HttpPost("query")>
		Public Overridable Function Query(queryVM As Q) As IActionResult
			Return Query(queryVM, Nothing)
		End Function

		''' <summary>查询项目</summary>
		''' <param name="successAction">成功后的操作</param>
		Protected Function Query(queryVM As Q, successAction As Action(Of (Data As IEnumerable(Of Object), Count As Integer, Pages As Integer))) As IActionResult
			If queryVM Is Nothing Then Return Err

			Dim vm = CreateVMList()
			Dim Ret = vm.QueryList(queryVM, True)
			Dim ok = ErrorMessage.IsPass
			If ok Then
				successAction?.Invoke(Ret)
				ok = ErrorMessage.IsPass
			End If

			ExecuteReturn(EntityActionEnum.LIST, Ret, ok)

			Return If(ok, Succ(New With {Ret.Data, Ret.Count, Ret.Pages}), Err)
		End Function

		''' <summary>返回树级数据，用于下拉列表框</summary>
		''' <param name="parentId">上级</param>
		''' <param name="nameField">标题字段</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="extField">扩展字段</param>
		''' <param name="queryVM">查询条件</param>
		''' <param name="treeUpdate">二次处理结果</param>
		Protected Overridable Function QueryDataTree(Of TParent As {IEntityParent, Class})(
																						  Optional parentId As String = Nothing,
																						  Optional nameField As String = Nothing,
																						  Optional disabledField As String = Nothing,
																						  Optional extField As String = Nothing，
																						  Optional queryVM As Q = Nothing,
																						  Optional treeUpdate As Func(Of TParent, DataTree, DataTree) = Nothing
																						 ) As IActionResult
			If {"{id}", "{parentId}", "null", "undefined"}.Contains(parentId, StringComparer.OrdinalIgnoreCase) Then parentId = ""

			Dim vm = CreateVMList()
			Dim ret = vm.QueryDataTree(parentId, nameField, disabledField, extField, queryVM, treeUpdate)

			ExecuteReturn(EntityActionEnum.LIST, ret, Nothing, "树形列表")

			Return Succ(ret)
		End Function

		''' <summary>返回指定数据列表，用于下拉列表框</summary>
		''' <param name="nameField">标题字段</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="extField">扩展字段</param>
		''' <param name="queryVM">查询条件</param>
		''' <param name="listUpdate">二次处理结果</param>
		Protected Overridable Function QueryDataList(
													Optional queryVM As Q = Nothing,
													Optional nameField As String = Nothing,
													Optional disabledField As String = Nothing,
													Optional extField As String = Nothing,
													Optional listUpdate As Func(Of T, DataList, DataList) = Nothing
												) As IActionResult
			Dim vm = CreateVMList()
			Dim ret = vm.QueryDataList(nameField, disabledField, extField, queryVM, listUpdate)

			ExecuteReturn(EntityActionEnum.LIST, ret, Nothing, "单层列表")

			Return Succ(ret)
		End Function

		''' <summary>名称模糊搜索</summary>
		''' <param name="name">查询的值</param>
		''' <param name="nameField">查询字段</param>
		''' <param name="disabledField">是否禁用字段</param>
		''' <param name="nameUpdate">名称更新函数</param>
		Protected Overridable Function QueryDataName(
													name As String,
													Optional nameField As String = Nothing,
													Optional disabledField As String = Nothing,
													Optional valueField As String = "",
													Optional count As Integer = 25,
													Optional queryAction As Action(Of ISelect(Of T)) = Nothing,
													Optional nameUpdate As Func(Of T, String, String) = Nothing
												) As IActionResult
			Dim vm = CreateVMList()
			Dim ret = vm.QueryDataName(name, nameField, disabledField, valueField, count, queryAction, nameUpdate)

			ExecuteReturn(EntityActionEnum.LIST, ret, Nothing, "模糊搜索")

			Return Succ(ret)
		End Function

#End Region

	End Class
End Namespace
