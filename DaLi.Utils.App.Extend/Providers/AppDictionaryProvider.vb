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
' 	字典数据操作
'
' 	name: Provider.AppDictionaryProvider
' 	create: 2023-02-19
' 	memo: 字典数据操作
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports Microsoft.Extensions.Logging

Namespace Provider

	''' <summary>字典数据操作</summary>
	Public Class AppDictionaryProvider
		Inherits ProviderBase
		Implements IAppDictionaryProvider

		Public Sub New(db As IFreeSql, log As ILogger(Of AppDictionaryProvider))
			MyBase.New(db, log, E_DICTIONARY_RELOAD)
		End Sub

#Region "缓存基础数据"

		''' <summary>数据集</summary>
		Private _Instance As ImmutableList(Of DataList) = ImmutableList.Create(Of DataList)

		''' <summary>重载数据集</summary>
		Public Overrides Sub Reload() Implements IAppDictionaryProvider.Reload
			SyncLock _Instance
				_Instance = Db.Select(Of DictionaryEntity).
					ToList(Function(x) New DataList With {
						.Value = x.ID,
						.Text = x.Value,
						.Ext = New With {x.Muti, x.Key, x.Required},
						.Parent = If(x.ParentId, 0),
						.Disabled = Not x.Enabled
					}).
					ToImmutableList
			End SyncLock

			Log.LogDebug("加载 {name} 数据 {count} 条", "字典", _Instance.Count)
		End Sub

		''' <summary>获取指定级别及其子集数据</summary>
		''' <param name="id">编号</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Public Function List(Optional id As Long = 0， Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False, Optional action As Action(Of DataTree) = Nothing) As List(Of DataTree) Implements IAppDictionaryProvider.List
			Dim datas As IEnumerable(Of DataList)

			' 如果查询顶级项目，则不能包含顶级本身
			If id = 0 Then includeSelf = False

			' Ext: {Muti, Key}	
			If includeSelf Then
				datas = _Instance.Where(Function(x) x.Value = id AndAlso (includeValue OrElse x.Ext.Muti <> 0))
			Else
				datas = _Instance.Where(Function(x) x.Parent = id AndAlso (includeValue OrElse x.Ext.Muti <> 0))
			End If

			Return datas.
				Select(Function(x)
						   Dim item As New DataTree(x)
						   item.Children = List(item.Value, includeValue, False, action)

						   action?.Invoke(item)
						   Return item
					   End Function).
				ToList
		End Function

		''' <summary>获取指定上级数据</summary>
		''' <param name="key">上级标识</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Public Function List(key As String, Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False, Optional action As Action(Of DataTree) = Nothing) As List(Of DataTree) Implements IAppDictionaryProvider.List
			If key.IsEmpty Then Return List(0, includeValue, includeSelf)

			' 获取标识项目
			Dim id As Long? = Item(key)?.Value
			If Not id.HasValue Then Return Nothing

			Return List(id.Value, includeValue, includeSelf, action)
		End Function

		''' <summary>获取指定级别及其子集数据的编号</summary>
		''' <param name="id">编号</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Public Function IDs(Optional id As Long = 0， Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False) As List(Of Long) Implements IAppDictionaryProvider.IDs
			Return List(id, includeValue, includeSelf)?.Select(Function(x) x.Value).Cast(Of Long).ToList
		End Function

		''' <summary>获取指定级别及其子集数据的编号</summary>
		''' <param name="key">上级标识</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Public Function IDs(key As String, Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False) As List(Of Long) Implements IAppDictionaryProvider.IDs
			Return List(key, includeValue, includeSelf)?.Select(Function(x) x.Value).Cast(Of Long).ToList
		End Function

		''' <summary>通过编号获取项目</summary>
		''' <param name="id">编号</param>
		Public Function Item(id As Long) As DataList Implements IAppDictionaryProvider.Item
			If id.IsEmpty Then Return Nothing

			' 获取项目
			Return _Instance.Where(Function(x) x.Value = id).FirstOrDefault
		End Function

		''' <summary>通过标识获取项目</summary>	
		''' <param name="key">标识</param>
		Public Function Item(key As String) As DataList Implements IAppDictionaryProvider.Item
			If key.IsEmpty Then Return Nothing

			' 获取项目
			Return _Instance.Where(Function(x) key.Equals(x.Ext.key?.ToString, StringComparison.OrdinalIgnoreCase)).FirstOrDefault
		End Function

#End Region

#Region "字典数据"

		''' <summary>字典初始化，不存在则创建</summary>
		''' <param name="parentId">上级编号，上级不存在则不处理</param>
		''' <param name="value">添加的值，必填</param>
		''' <param name="Key">自定义标识，非必须</param>
		''' <param name="muti">节点类型（-1. 分组，0.值对象，1. 单选，>1 多选数量）/ 分组下仅能存放单/多选；单/多选下仅能存放值；值下不能存放任何东西</param>
		''' <param name="afterInsert">项目添加成功后的操作，如添加子项目等</param>
		Public Function DictionaryInsert(parentId As Long, id As Long, value As String, Optional muti As Integer = -1, Optional key As String = "", Optional afterInsert As Action(Of DictionaryEntity) = Nothing, Optional isSystem As Boolean = True) As DictionaryEntity Implements IAppDictionaryProvider.DictionaryInsert
			Dim dic = DictionaryItem(id)
			If dic IsNot Nothing Then Return dic

			Dim parent As DictionaryEntity = Nothing
			If parentId > 0 Then
				parent = Db.Select(Of DictionaryEntity).WhereID(parentId).ToOne
				If parent Is Nothing Then Return Nothing
			End If

			Return DictionaryInsert(parent, value, id, muti, key, afterInsert, isSystem)
		End Function

		''' <summary>字典初始化，不存在则创建</summary>
		''' <param name="parentKey">上级别名，上级不存在则设置未 String.Empty</param>
		''' <param name="value">添加的值，必填</param>
		''' <param name="Key">自定义别名，非必须</param>
		''' <param name="muti">节点类型（-1. 分组，0.值对象，1. 单选，>1 多选数量）/ 分组下仅能存放单/多选；单/多选下仅能存放值；值下不能存放任何东西</param>
		''' <param name="afterInsert">项目添加成功后的操作，如添加子项目等</param>
		Public Function DictionaryInsert(parentKey As String, value As String, Optional muti As Integer = -1, Optional key As String = "", Optional afterInsert As Action(Of DictionaryEntity) = Nothing, Optional isSystem As Boolean = True) As DictionaryEntity Implements IAppDictionaryProvider.DictionaryInsert
			Dim parent As DictionaryEntity = Nothing

			If parentKey.NotEmpty Then
				parent = Db.Select(Of DictionaryEntity).Where(Function(x) x.Key.Equals(parentKey, StringComparison.OrdinalIgnoreCase)).ToOne
				If parent Is Nothing OrElse parent.Muti = 0 Then Return Nothing
			End If

			Return DictionaryInsert(parent, value, 0, muti, key, afterInsert, isSystem)
		End Function

		''' <summary>字典初始化，不存在则创建</summary>
		''' <param name="parent">上级，不存在则为顶级</param>
		''' <param name="value">添加的值，必填</param>
		''' <param name="Key">自定义标识，非必须</param>
		''' <param name="muti">节点类型（-1. 分组，0.值对象，1. 单选，>1 多选数量）/ 分组下仅能存放单/多选；单/多选下仅能存放值；值下不能存放任何东西</param>
		''' <param name="afterInsert">项目添加成功后的操作，如添加子项目等</param>
		Private Function DictionaryInsert(parent As DictionaryEntity, value As String, Optional id As Long = 0, Optional muti As Integer = -1, Optional key As String = "", Optional afterInsert As Action(Of DictionaryEntity) = Nothing, Optional isSystem As Boolean = True) As DictionaryEntity
			If parent IsNot Nothing Then
				If parent.Muti = 0 Then Return Nothing

				' 值只能放在多选下
				If muti = 0 AndAlso parent.Muti < 1 Then Return Nothing

				' 分组/单/多选只能放在分组下
				If muti <> 0 AndAlso parent.Muti > -1 Then Return Nothing
			Else
				' 顶级不能放值
				If muti = 0 Then Return Nothing
			End If

			Dim dic As DictionaryEntity = Nothing

			Using repo = Db.GetRepository(Of DictionaryEntity)
				' 检查 Key 是否存在
				If key.NotEmpty Then
					Dim exist = repo.Select.Where(Function(x) x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Any
					If exist Then key &= Guid.NewGuid.ToString
					If key.Length > 50 Then key = key.Substring(0, 50)
				End If


				' 不存在，添加
				dic = New DictionaryEntity With {
						.ParentId = parent?.ID,
						.Value = value,
						.Key = key,
						.Muti = muti,
						.IsSystem = isSystem,
						.UpdateTime = SYS_NOW_DATE,
						.Enabled = True
					}
				If id > 0 Then dic.ID = id

				dic = repo.Insert(dic)

				afterInsert?.Invoke(dic)
			End Using

			' 远程通知
			SYS.Events.Publish(E_DICTIONARY_RELOAD)

			Return dic
		End Function

		''' <summary>是否存在指定标识的项目</summary>
		Public Function DictionaryExist(id As Long, Optional notExistAction As Action = Nothing) As Boolean Implements IAppDictionaryProvider.DictionaryExist
			If id < 1 Then
				Return False
			Else
				Dim inc = Db.Select(Of DictionaryEntity).WhereID(id).Any

				If Not inc AndAlso notExistAction IsNot Nothing Then notExistAction.Invoke()

				Return inc
			End If
		End Function

		''' <summary>是否存在指定别名的项目</summary>
		''' <param name="key">别名</param>
		''' <param name="notExistAction">不存在如何操作</param>
		Public Function DictionaryExist(key As String, Optional notExistAction As Action = Nothing) As Boolean Implements IAppDictionaryProvider.DictionaryExist
			If key.IsEmpty Then
				Return False
			Else
				Dim inc = Db.Select(Of DictionaryEntity).Where(Function(x) x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Any

				If Not inc AndAlso notExistAction IsNot Nothing Then notExistAction.Invoke()

				Return inc
			End If
		End Function

		''' <summary>是否存在指定标识的项目</summary>
		Public Function DictionaryExist(parentId As Long, value As String, Optional notExistAction As Action = Nothing) As Boolean
			If parentId < 0 OrElse value.IsEmpty Then
				Return False
			Else
				Dim inc = Db.Select(Of DictionaryEntity).Where(Function(x) x.ParentId = parentId AndAlso x.Value.Equals(value, StringComparison.OrdinalIgnoreCase)).Any

				If Not inc AndAlso notExistAction IsNot Nothing Then notExistAction.Invoke()

				Return inc
			End If
		End Function

		''' <summary>移除指定标识，仅允许移除非系统项目的值项目</summary>
		Public Function DictionaryRemove(id As Long) As Boolean Implements IAppDictionaryProvider.DictionaryRemove
			If id < 1 Then Return False

			Dim ret = Db.Delete(Of DictionaryEntity)(id).
						Where(Function(x) Not x.IsSystem AndAlso x.Muti = 0).
						ExecuteAffrows > 0

			' 远程通知
			If ret Then SYS.Events.Publish(E_DICTIONARY_RELOAD)

			Return ret
		End Function

		''' <summary>通过标识获取字典</summary>
		Default Public ReadOnly Property DictionaryItem(parentId As Long, dictionaryValue As String) As DictionaryEntity Implements IAppDictionaryProvider.DictionaryItem
			Get
				If parentId >= 0 AndAlso dictionaryValue.NotEmpty Then
					Return Db.Select(Of DictionaryEntity).Where(Function(x) x.ParentId = parentId AndAlso x.Value.Equals(dictionaryValue, StringComparison.OrdinalIgnoreCase)).ToOne
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>通过标识获取字典</summary>
		Default Public ReadOnly Property DictionaryItem(id As Long) As DictionaryEntity Implements IAppDictionaryProvider.DictionaryItem
			Get
				If id < 0 Then Return Nothing

				Return Db.Select(Of DictionaryEntity).WhereID(id).ToOne
			End Get
		End Property

		''' <summary>通过别名获取字典</summary>
		Default Public ReadOnly Property DictionaryItem(key As String) As DictionaryEntity Implements IAppDictionaryProvider.DictionaryItem
			Get
				If key.IsEmpty Then Return Nothing

				Return Db.Select(Of DictionaryEntity).Where(Function(x) x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).ToOne
			End Get
		End Property

		''' <summary>批量插入值，值只能放在单/多选下</summary>
		''' <param name="parentId">父级节点标识</param>
		''' <param name="values">值字典</param>
		''' <param name="isSystem">是否系统标签</param>
		Public Function DictionaryInsertValues(parentId As Long, values As IEnumerable(Of String), Optional isSystem As Boolean = False) As Integer Implements IAppDictionaryProvider.DictionaryInsertValues
			Dim result = -1

			' 检查上级状态，如果上级不存在，或者不允许添加数据则直接退出
			If parentId < 0 OrElse values.IsEmpty Then Return result

			Dim parent = Db.Select(Of DictionaryEntity).WhereEquals(parentId, Function(x) x.ID).ToOne
			If parent Is Nothing OrElse parent.Muti < 1 Then Return result

			' 当前值列表
			Dim incValues = values.Distinct.ToList

			' 检查重复的数据
			Dim exValues = Db.Select(Of DictionaryEntity).
							WhereEquals(parentId, Function(x) x.ParentId).
							WhereContain(incValues, Function(x) x.ID).
							ToList(Function(x) x.Value)
			exValues = If(exValues, New List(Of String))

			Dim dics = incValues.Where(Function(x) Not exValues.Contains(x, StringComparer.OrdinalIgnoreCase)) _
					.Select(Function(x) New DictionaryEntity With {
						.Value = x,
						.ParentId = parentId,
						.Muti = 0,
						.IsSystem = isSystem,
						.UpdateTime = SYS_NOW_DATE,
						.Enabled = True
					})

			result = Db.Insert(dics).ExecuteAffrows

			' 远程通知
			SYS.Events.Publish(E_DICTIONARY_RELOAD)

			Return result
		End Function

		''' <summary>批量插入值，值只能放在单/多选下</summary>
		''' <param name="parentId">父级节点标识</param>
		''' <param name="values">值字典</param>
		''' <param name="isSystem">是否系统标签</param>
		Public Function DictionaryInsertValues(parentId As Long, values As IDictionary(Of Long, String), Optional isSystem As Boolean = False) As Integer Implements IAppDictionaryProvider.DictionaryInsertValues
			Dim result = -1

			' 检查上级状态，如果上级不存在，或者不允许添加数据则直接退出
			If parentId < 0 OrElse values.IsEmpty Then Return result

			Dim parent = Db.Select(Of DictionaryEntity).WhereEquals(parentId, Function(x) x.ID).ToOne
			If parent Is Nothing OrElse parent.Muti < 1 Then Return result

			' 列表中的标识
			Dim ids = values.Where(Function(x) x.Key <> 0).Select(Function(x) x.Key).ToList

			' 检查重复的数据
			Dim exIds = Db.Select(Of DictionaryEntity).
							WhereContain(ids, Function(x) x.ID).
							ToList(Function(x) x.ID)
			exIds = If(exIds, New List(Of Long))

			' 当前值列表
			Dim dics = values.Where(Function(x) Not exIds.Contains(x.Key)).
				Select(Function(x) New DictionaryEntity With {
					.ID = x.Key,
					.Value = x.Value,
					.ParentId = parentId,
					.Muti = 0,
					.IsSystem = isSystem,
					.UpdateTime = SYS_NOW_DATE,
					.Enabled = True
				})

			result = Db.Insert(dics).ExecuteAffrows

			' 远程通知
			If result > 0 Then SYS.Events.Publish(E_DICTIONARY_RELOAD)

			Return result
		End Function

		''' <summary>更新值</summary>
		''' <param name="id">标识</param>
		''' <param name="value">新值，如果与旧值一致，则不处理</param>
		Public Sub DictionaryUpdateValue(id As Long, value As String) Implements IAppDictionaryProvider.DictionaryUpdateValue
			If id < 1 OrElse value.IsEmpty Then Exit Sub

			Dim item = Db.Select(Of DictionaryEntity).WhereID(id).ToOne

			' 不存在，且存在上级则新建
			If item IsNot Nothing AndAlso item.Value <> value Then
				Dim changed = Db.Update(Of DictionaryEntity)(item.ID).Set(Function(x) x.Value, value).ExecuteAffrows() > 0

				' 远程通知
				If changed Then SYS.Events.Publish(E_DICTIONARY_RELOAD)
			End If
		End Sub

		''' <summary>切换项目启用状态</summary>
		Public Sub DictionarySwitchEnable(id As Long, value As Boolean) Implements IAppDictionaryProvider.DictionarySwitchEnable
			If id.IsEmpty Then Exit Sub

			Using repo = Db.GetRepository(Of DictionaryEntity)
				Dim item = repo.Where(Function(x) x.ID = id).ToOne
				If item IsNot Nothing AndAlso item.Enabled <> value Then
					item.Enabled = value
					item.UpdateTime = SYS_NOW_DATE
					repo.Update(item)

					' 远程通知
					SYS.Events.Publish(E_DICTIONARY_RELOAD)
				End If
			End Using
		End Sub

		''' <summary>切换系统状态</summary>
		Public Sub DictionarySwitchSystem(id As Long, value As Boolean) Implements IAppDictionaryProvider.DictionarySwitchSystem
			If id.IsEmpty Then Exit Sub

			Using repo = Db.GetRepository(Of DictionaryEntity)
				Dim item = repo.Where(Function(x) x.ID = id).ToOne
				If item IsNot Nothing AndAlso item.IsSystem <> value Then
					item.IsSystem = value
					item.UpdateTime = SYS_NOW_DATE
					repo.Update(item)
				End If
			End Using
		End Sub

		''' <summary>获取值列表</summary>
		''' <param name="parentId">父级节点标识</param>
		Public Function DictionaryValues(parentId As Long) As IDictionary(Of Long, String) Implements IAppDictionaryProvider.DictionaryValues
			Return _Instance.Where(Function(x) x.Parent = parentId).ToDictionary(Of Long, String)(Function(x) x.Value, Function(x) x.Value)
		End Function

		''' <summary>获取值列表</summary>
		''' <param name="parentKey">父级节点别名</param>
		Public Function DictionaryValues(parentKey As String) As IDictionary(Of Long, String) Implements IAppDictionaryProvider.DictionaryValues
			' 获取标识项目
			Dim id As Long? = Item(parentKey)?.Value
			If Not id.HasValue Then Return Nothing

			Return DictionaryValues(id.Value)
		End Function

		'''' <summary>获取值列表</summary>
		'''' <param name="parentId">父级节点标识</param>
		'Public Function DictionaryValues(parentId As Long) As IDictionary(Of Long, String) Implements IAppDictionaryProvider.DictionaryValues
		'	Dim parent = DictionaryItem(parentId)
		'	If parent IsNot Nothing Then
		'		Return Db.Select(Of DictionaryEntity).
		'			WhereEquals(parent.ID, Function(x) x.ParentId).
		'			ToList(Function(x) (x.ID, x.Value))?.
		'			ToDictionary(Function(x) x.ID, Function(x) x.Value)
		'	Else
		'		Return Nothing
		'	End If
		'End Function

		'''' <summary>获取值列表</summary>
		'''' <param name="parentKey">父级节点别名</param>
		'Public Function DictionaryValues(parentKey As String) As IDictionary(Of Long, String) Implements IAppDictionaryProvider.DictionaryValues
		'	Dim parent = DictionaryItem(parentKey)
		'	If parent IsNot Nothing Then
		'		Return Db.Select(Of DictionaryEntity).
		'			WhereEquals(parent.ID, Function(x) x.ParentId).
		'			ToList(Function(x) (x.ID, x.Value))?.
		'			ToDictionary(Function(x) x.ID, Function(x) x.Value)
		'	Else
		'		Return Nothing
		'	End If
		'End Function

		'''' <summary>字典数据验证，是否选中了必填选项，是否超过指定数量</summary>
		'''' <param name="ids">字典标识列表</param>
		'''' <param name="enEmpty">是否允许空数据</param>
		'''' <returns>存在异常则返回错误信息，否则为空</returns>
		'Public Function DictionaryValidate(ids As IEnumerable(Of Long), enEmpty As Boolean) As String
		'	If ids.IsEmpty Then Return If(enEmpty, "", "Error.Dictionary.Empty") ' 数据不能为空

		'	' 检查值是否在字典中
		'	Dim dics = _Instance.Where(Function(x) Not x.Disabled AndAlso x.Ext.Muti = 0 AndAlso ids.Contains(x.Value)).ToList
		'	If dics.IsEmpty Then Return "Error.Dictionary.Include" ' 存在数据无效

		'	' 获取所有上级
		'	Dim dicGroups = dics.GroupBy(Function(x) x.Parent).ToDictionary(Function(x) _Instance.Where(Function(y) y.Value = x.Key).FirstOrDefault, Function(x) x.ToList)
		'	If dicGroups.IsEmpty Then Return "Error.Dictionary.Include" ' 存在数据无效

		'	' 检查是否存在禁用上级
		'	If dicGroups.Keys.Any(Function(x) x.Disabled) Then Return "Error.Dictionary.Disabled" ' 存在禁用数据
		'End Function

#End Region

#Region "字典关系数据"

		''' <summary>更新资源的字典关系，返回字典标识</summary>
		''' <param name="value">数据对象</param>
		''' <param name="dicIds">字典编号</param>
		Public Sub ModuleUpdate(Of T As {IEntity, Class})(value As T, Optional dicIds As IEnumerable(Of Long) = Nothing) Implements IAppDictionaryProvider.ModuleUpdate
			If value Is Nothing Then Return

			ModuleUpdate(ExtendHelper.GetModuleId(Of T), value.ID, dicIds)
		End Sub

		''' <summary>更新资源的字典关系，返回字典标识</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValue">指定模块数据编号</param>
		''' <param name="dicIds">字典数据</param>
		Public Sub ModuleUpdate(moduleId As Integer, moduleValue As Long, Optional dicIds As IEnumerable(Of Long) = Nothing) Implements IAppDictionaryProvider.ModuleUpdate
			If moduleId < 1 OrElse moduleValue = 0 Then Return

			' 空数组，移除
			If dicIds.IsEmpty Then
				ModuleRemove(moduleId, moduleValue)
				Return
			End If

			dicIds = dicIds.Where(Function(x) x > 0).Distinct.ToArray
			If dicIds.IsEmpty Then Return

			' 找出已经存在的数据
			' 仅返回值对象
			dicIds = Db.Select(Of DictionaryEntity).
					WhereContain(dicIds, Function(x) x.ID).
					ToList(Function(x) x.ID)
			If dicIds.IsEmpty Then Return

			' 返回结果
			Dim ret As New List(Of String)

			' 已经存在的数据
			Dim incIds = Db.Select(Of DictionaryDataEntity).
					Where(Function(x) x.ModuleId = moduleId AndAlso x.ModuleValue = moduleValue).
					ToList(Function(x) x.DictionaryId)

			' 删除无效关系
			If incIds.NotEmpty Then
				Dim ids = incIds.Except(dicIds).ToList
				If ids.NotEmpty Then
					Db.Delete(Of DictionaryDataEntity).
						Where(Function(x) x.ModuleId = moduleId AndAlso x.ModuleValue = moduleValue).
						Where(Function(x) ids.Contains(x.DictionaryId)).
						ExecuteAffrows()
				End If

				' 需要补充的项目
				dicIds = dicIds.Except(incIds).ToList
			End If

			' 添加关系
			If dicIds.NotEmpty Then
				Dim datas = dicIds.Select(Function(x) New DictionaryDataEntity With {
										 .ModuleId = moduleId,
										 .ModuleValue = moduleValue,
										 .DictionaryId = x
										}).ToList

				Db.Insert(datas).ExecuteAffrows()
			End If
		End Sub

		'----------------------------------------------------------------

		''' <summary>移除指定模块数据字典数据</summary>
		Public Function ModuleRemove(Of T As {IEntity, Class})(value As T) As Boolean Implements IAppDictionaryProvider.ModuleRemove
			If value?.ID <> 0 Then
				Return ModuleRemove(ExtendHelper.GetModuleId(Of T), value.ID)
			Else
				Return False
			End If
		End Function

		''' <summary>移除指定模块数据字典数据</summary>
		''' <param name="moduleType">模块类型</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Public Function ModuleRemove(moduleType As Type, ParamArray moduleValues() As Long) As Boolean Implements IAppDictionaryProvider.ModuleRemove
			Return ModuleRemove(ExtendHelper.GetModuleId(moduleType), moduleValues)
		End Function

		''' <summary>移除指定模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Public Function ModuleRemove(moduleId As Integer, ParamArray moduleValues() As Long) As Boolean Implements IAppDictionaryProvider.ModuleRemove
			If moduleId < 1 OrElse moduleValues.IsEmpty Then Return False

			Dim del = Db.Delete(Of DictionaryDataEntity).Where(Function(x) x.ModuleId = moduleId)

			If moduleValues.Length > 1 Then
				del.Where(Function(x) moduleValues.Contains(x.ModuleValue))
			Else
				del.Where(Function(x) x.ModuleValue = moduleValues(0))
			End If

			Return del.ExecuteAffrows > 0
		End Function

		'----------------------------------------------------------------

		''' <summary>字典数据</summary>
		''' <param name="value">数据对象</param>
		Public Function ModuleDictionayIds(Of T As {IEntity, Class})(value As T) As IEnumerable(Of Long) Implements IAppDictionaryProvider.ModuleDictionayIds
			If value?.ID <> 0 Then
				Return ModuleDictionayIds(ExtendHelper.GetModuleId(Of T), value.ID)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>获取指定模块数据字典数据标识</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Public Function ModuleDictionayIds(moduleId As Integer, ParamArray moduleValues() As Long) As IEnumerable(Of Long) Implements IAppDictionaryProvider.ModuleDictionayIds
			If moduleId < 1 OrElse moduleValues.IsEmpty Then Return Nothing

			Dim query = Db.Select(Of DictionaryDataEntity).WhereEquals(moduleId, Function(x) x.ModuleId)

			If moduleValues.Length > 1 Then
				query.WhereContain(moduleValues, Function(x) x.ModuleValue)
			Else
				query.WhereEquals(moduleValues(0), Function(x) x.ModuleValue)
			End If

			Return query.Distinct.ToList(Function(x) x.DictionaryId)
		End Function

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="value">模块数据</param>
		Public Function ModuleDictionaies(Of T As {IEntity, Class})(value As T) As IEnumerable(Of DictionaryEntity) Implements IAppDictionaryProvider.ModuleDictionaies
			If value?.ID <> 0 Then
				Return ModuleDictionaies(ExtendHelper.GetModuleId(Of T), value.ID)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValue">指定模块数据编号</param>
		Public Function ModuleDictionaies(moduleId As Integer, moduleValue As Long) As IEnumerable(Of DictionaryEntity) Implements IAppDictionaryProvider.ModuleDictionaies
			If moduleId < 1 OrElse moduleValue = 0 Then Return Nothing

			Return Db.Select(Of DictionaryDataEntity).
				Include(Function(x) x.Dictionary).
				WhereEquals(moduleId, Function(x) x.ModuleId).
				WhereEquals(moduleValue, Function(x) x.ModuleValue).
				ToList

			'Return Db.Select(Of AppDictionaryData, AppDictionary).Where(Function(x, y) x.DictionaryId = y.ID AndAlso x.ModuleId = moduleId AndAlso x.ModuleValue = moduleValue).ToList(Function(x, y) y)
		End Function

		'----------------------------------------------------------------

		''' <summary>获取指定字典标识模块数据标识</summary>
		''' <param name="dicIds">指定字典标识模</param>
		Public Function ModuleValues(Of T As {IEntity, Class})(ParamArray dicIds() As Long) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.ModuleValues
			Return ModuleValues(ExtendHelper.GetModuleId(Of T), dicIds)
		End Function

		''' <summary>获取指定字典标识模块数据标识</summary>
		''' <param name="moduleType">模块类型</param>
		''' <param name="dicIds">指定字典标识模</param>
		Public Function ModuleValues(moduleType As Type, ParamArray dicIds() As Long) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.ModuleValues
			Return ModuleValues(ExtendHelper.GetModuleId(moduleType), dicIds)
		End Function

		''' <summary>获取指定字典标识模块数据标识</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="dicIds">指定字典标识模</param>
		Public Function ModuleValues(moduleId As Integer, ParamArray dicIds() As Long) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.ModuleValues
			If moduleId < 1 OrElse dicIds.IsEmpty Then Return Nothing

			Dim query = Db.Select(Of DictionaryDataEntity).WhereEquals(moduleId, Function(x) x.ModuleId.Value)

			If dicIds.Length > 1 Then
				query.WhereContain(dicIds, Function(x) x.DictionaryId)
			Else
				query.WhereEquals(dicIds(0), Function(x) x.DictionaryId)
			End If

			Return query.Distinct.ToList
		End Function

		'----------------------------------------------------------------

		''' <summary>获取模块数据字典数据</summary>
		''' <param name="value">模块数据</param>
		Public Function DataList(Of T As {IEntity, Class})(Optional value As T = Nothing) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.DataList
			If value?.ID <> 0 Then
				Return DataList(ExtendHelper.GetModuleId(Of T), value.ID)
			Else
				Return DataList(ExtendHelper.GetModuleId(Of T))
			End If
		End Function

		''' <summary>获取模块数据字典数据</summary>
		''' <param name="moduleType">模块类型</param>
		Public Function DataList(moduleType As Type) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.DataList
			Return DataList(ExtendHelper.GetModuleId(moduleType))
		End Function

		''' <summary>获取模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		Public Function DataList(moduleId As Integer) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.DataList
			If moduleId < 1 Then Return Nothing

			Return Db.Select(Of DictionaryDataEntity).WhereEquals(moduleId, Function(x) x.ModuleId).ToList
		End Function

		'----------------------------------------------------------------

		''' <summary>字典数据</summary>
		''' <param name="values">数据对象</param>
		Public Function DataList(Of T As {IEntity, Class})(ParamArray values() As T) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.DataList
			If values.IsEmpty Then Return Nothing

			Dim ids = values.Select(Function(x) x.ID).Distinct.ToArray
			If ids.IsEmpty Then Return Nothing

			Return DataList(GetType(T), ids)
		End Function

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="moduleType">模块类型</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Public Function DataList(moduleType As Type, ParamArray moduleValues() As Long) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.DataList
			Return DataList(ExtendHelper.GetModuleId(moduleType), moduleValues)
		End Function

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Public Function DataList(moduleId As Integer, ParamArray moduleValues() As Long) As IEnumerable(Of DictionaryDataEntity) Implements IAppDictionaryProvider.DataList
			If moduleId < 1 OrElse moduleValues.IsEmpty Then Return Nothing

			Dim query = Db.Select(Of DictionaryDataEntity).WhereEquals(moduleId, Function(x) x.ModuleId)

			If moduleValues.Length > 1 Then
				query.WhereContain(moduleValues, Function(x) x.ModuleValue)
			Else
				query.WhereEquals(moduleValues(0), Function(x) x.ModuleValue)
			End If

			Return query.ToList
		End Function

#End Region

#Region "更新资源"

		''' <summary>更新项目字典数据</summary>
		Public Sub EntityUpdate(Of T As {IEntity, Class})(entity As T) Implements IAppDictionaryProvider.EntityUpdate
			If entity Is Nothing Then Return

			Dim pro = ExtendHelper.GetDictionarProperty(entity)
			If pro Is Nothing OrElse Not pro.CanWrite Then Return

			pro.SetValue(entity, ModuleDictionayIds(entity))
		End Sub

		''' <summary>更新项目字典数据</summary>
		Public Sub EntitiesUpdate(Of T As {IEntity, Class})(entities As IEnumerable(Of T)) Implements IAppDictionaryProvider.EntitiesUpdate
			If entities.IsEmpty Then Return

			Dim pro = ExtendHelper.GetDictionarProperty(Of T)()
			If pro Is Nothing OrElse Not pro.CanWrite Then Return

			Dim dics = DataList(entities.ToArray)
			If dics.NotEmpty Then
				For Each entityItem In entities
					Dim dicVals = dics.Where(Function(x) x.ModuleValue = entityItem.ID).Select(Function(x) x.DictionaryId).ToList
					pro.SetValue(entityItem, dicVals)
				Next
			End If
		End Sub

#End Region

	End Class

End Namespace

