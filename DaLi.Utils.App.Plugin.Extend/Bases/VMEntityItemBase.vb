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
' 	单个项目视图模型基础操作
'
' 	name: Base.VMEntityItemBase
' 	create: 2023-10-08
' 	memo: 单个项目视图模型基础操作（查、加、改、删）
'
' ------------------------------------------------------------

Imports FreeSql

Namespace Base
	''' <summary>单个项目视图模型基础操作（查、加、改、删）</summary>
	Public MustInherit Class VMEntityItemBase(Of T As {IEntity, Class})
		Inherits VMEntityBase(Of T)

#Region "数据对象"

		''' <summary>数据对象</summary>
		Private _Entity As T

		''' <summary>数据对象</summary>
		Public ReadOnly Property Entity As T
			Get
				Return _Entity
			End Get
		End Property

		''' <summary>数据对象添加初始化操作等</summary>
		Public Overridable Sub SetEntity(value As T)
			_Entity = value
		End Sub

		''' <summary>通过标识设置数据对象</summary>
		Public Sub SetEntityID(id As Object)
			Dim entity As T = Nothing

			If id IsNot Nothing Then
				Dim query = ValidateQuery(EntityActionEnum.ITEM)
				If ErrorMessage.IsPass Then
					' 如果 id 与 标识类型不一致，则先转换成字符串再换成正确类型
					If id.GetType <> IdType Then id = id.ToString.ToValue(IdType)

					entity = query.Where(Function(x) x.ID.Equals(id)).ToOne
				End If
			End If

			SetEntity(entity)
		End Sub

		''' <summary>从查询获取数据对象</summary>
		Public Sub SetEntityQuery(queryVM As QueryBase(Of T))
			If queryVM Is Nothing Then SetEntity(Nothing)

			Dim query = ValidateQuery(EntityActionEnum.ITEM, queryVM)

			If ErrorMessage.IsPass Then
				SetEntity(query.ToOne)
			Else
				SetEntity(Nothing)
			End If
		End Sub

		''' <summary>标识</summary>
		Public ReadOnly Property ID As Object
			Get
				If Entity Is Nothing Then Return Nothing

				If IdPro IsNot Nothing Then
					Return IdPro.GetValue(Entity)
				Else
					Return Entity.ID
				End If
			End Get
		End Property

#End Region

#Region "获取对象"

		''' <summary>获取单个项目</summary>
		Public Function Item() As T
			Call ValidateItem(EntityActionEnum.ITEM, Entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 操作完成事件
			Return FinishItem(EntityActionEnum.ITEM, Entity)
		End Function

		''' <summary>获取单个项目，并将结果转换成指定类型</summary>
		Public Function Item(Of Q As QueryBase(Of T))(returnSimple As Boolean) As IDictionary(Of String, Object)
			Dim entity = Item()
			If entity Is Nothing Then Return Nothing

			Dim queryVM = Activator.CreateInstance(Of Q)()
			Return queryVM.ConvertDictionary(entity, returnSimple, Db)
		End Function

#End Region

#Region "添加对象"

		''' <summary>添加对象</summary>
		Public Overridable Function Add() As T
			Try
				Dim entity = ValidateAdd(Me.Entity)
				If Not ErrorMessage.IsPass Then Return Nothing

				' 缓存字典，从客户端附加的来的字段数据
				Dim dicIds = DictionaryLoad(entity, True)

				' 入库
				Using repo = Db.GetRepository(Of T)
					Dim data = repo.Insert(entity)
					If data IsNot Nothing Then _Entity.ID = data.ID
				End Using

				' 保存字典
				DictionaryUpdate(entity, dicIds)

				' 操作完成事件
				Return FinishAdd(entity)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Nothing
			End Try
		End Function

		''' <summary>添加对象</summary>
		Public Overridable Async Function AddAsync() As Task(Of T)
			Try
				Dim entity = ValidateAdd(Me.Entity)
				If Not ErrorMessage.IsPass Then Return Nothing

				' 缓存字典，从客户端附加的来的字段数据
				Dim dicIds = DictionaryLoad(entity, True)

				Using repo = Db.GetRepository(Of T)
					Dim data = Await repo.InsertAsync(entity)
					If data IsNot Nothing Then _Entity.ID = data.ID
				End Using

				' 保存字典
				DictionaryUpdate(entity, dicIds)

				' 操作完成事件
				Return FinishAdd(entity)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Nothing
			End Try
		End Function

#End Region

#Region "编辑对象"

		''' <summary>编辑对象</summary>
		''' <param name="updateAll">是否更新所有字段</param>
		''' <param name="skipAudit">是否忽略数据审计，控制器审核属性有设置，则以设置为准，否则此参数为准</param>
		Public Overridable Function Edit(updateAll As Boolean, skipAudit As Boolean) As T
			Dim entity = ValidateEdit(Me.Entity, updateAll, skipAudit)
			If Not ErrorMessage.IsPass Then Return Nothing

			Try
				' 缓存字典，从客户端附加的来的字段数据
				Dim dicIds = DictionaryLoad(entity, True)

				' 入库
				Dim succ = Db.Update(Of T).SetSource(entity).ExecuteAffrows() > 0
				If Not succ Then Throw New Exception("更新数据异常")

				' 保存字典
				DictionaryUpdate(entity, dicIds)

				' 操作完成事件
				Return FinishEdit(entity)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Nothing
			End Try
		End Function

		''' <summary>编辑对象</summary>
		Public Overridable Async Function EditAsync(updateAll As Boolean, skipAudit As Boolean) As Task(Of T)
			Dim entity = ValidateEdit(Me.Entity, updateAll, skipAudit)
			If Not ErrorMessage.IsPass Then Return Nothing

			Try
				' 缓存字典，从客户端附加的来的字段数据
				Dim dicIds = DictionaryLoad(entity, True)

				' 入库
				Dim succ = Await Db.Update(Of T).SetSource(entity).ExecuteAffrowsAsync() > 0
				If Not succ Then Throw New Exception("更新数据异常")

				' 保存字典
				DictionaryUpdate(entity, dicIds)

				' 操作完成事件
				Return FinishEdit(entity)
			Catch ex As Exception
				ErrorMessage.Exception = ex
				Return Nothing
			End Try
		End Function

#End Region

#Region "删除对象"

		''' <summary>删除对象</summary>
		Public Overridable Function Delete() As Boolean
			' 存在错误需要退出
			Dim flag = ValidateDelete(Entity)
			If Not flag.HasValue Then Return False

			Dim succ As Boolean
			If flag.Value Then
				' 真实删除
				succ = Db.Delete(Of T)(ID).ExecuteAffrows = 1
			Else
				' 删除标记
				succ = Db.Update(Of T).SetSource(Entity).ExecuteAffrows = 1
			End If

			If succ Then
				' 保存字典
				DictionaryRemove(ID)

				' 操作完成事件
				succ = FinishDelete(Entity)
			Else
				ErrorMessage.Notification = "Error.Action"
			End If

			Return succ
		End Function

		''' <summary>删除对象</summary>
		Public Overridable Async Function DeleteAsync() As Task(Of Boolean)
			' 存在错误需要退出
			Dim flag = ValidateDelete(Entity)
			If Not flag.HasValue Then Return False

			Dim succ As Boolean
			If flag.Value Then
				' 真实删除
				succ = Await Db.Delete(Of T)(ID).ExecuteAffrowsAsync = 1
			Else
				' 删除标记
				succ = Await Db.Update(Of T).SetSource(Entity).ExecuteAffrowsAsync = 1
			End If

			If succ Then
				' 保存字典
				DictionaryRemove(ID)

				' 操作完成事件
				succ = FinishDelete(Entity)
			Else
				ErrorMessage.Notification = "Error.Action"
			End If

			Return succ
		End Function

#End Region

#Region "查询数据"

		''' <summary>查询单个项目，并将结果转换成指定类型</summary>
		Public Function QueryItem(Of Q As {QueryBase(Of T), New})(id As Object, Optional returnSimple As Boolean = False) As IDictionary(Of String, Object)
			If id Is Nothing Then Return Nothing

			Dim query = ValidateQuery(EntityActionEnum.ITEM, New Q)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 如果 id 与 标识类型不一致，则先转换成字符串再换成正确类型
			If id.GetType <> IdType Then id = id.ToString.ToValue(IdType)
			Dim entity = query.Where(Function(x) x.ID.Equals(id)).ToOne

			' 验证前的检查
			entity = ValidateItem(EntityActionEnum.ITEM, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			entity = FinishItem(EntityActionEnum.ITEM, entity)
			If entity Is Nothing Then Return Nothing

			' 结果处理
			Dim queryVM = Activator.CreateInstance(Of Q)()
			Return queryVM.ConvertDictionary(entity, returnSimple, Db)
		End Function

		''' <summary>查询单个项目，并将结果转换成指定类型</summary>
		Public Function QueryItem(queryVM As QueryBase(Of T), Optional returnSimple As Boolean = False) As IDictionary(Of String, Object)
			If queryVM Is Nothing Then
				ErrorMessage.Notification = "Error.Invalid"
				Return Nothing
			End If

			Dim query = ValidateQuery(EntityActionEnum.ITEM, queryVM)
			If Not ErrorMessage.IsPass Then Return Nothing

			Dim entity = query.ToOne

			' 验证前的检查
			Call ValidateItem(EntityActionEnum.ITEM, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			entity = FinishItem(EntityActionEnum.ITEM, entity)
			If entity Is Nothing Then Return Nothing

			' 结果处理
			Return queryVM.ConvertDictionary(entity, returnSimple, Db)
		End Function

#End Region

	End Class
End Namespace