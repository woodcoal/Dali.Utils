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
' 	字典数据操作接口
'
' 	name: Interface.IAppDictionaryProvider
' 	create: 2023-02-19
' 	memo: 字典数据操作接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>字典数据操作接口</summary>
	Public Interface IAppDictionaryProvider

		''' <summary>重载数据</summary>
		Sub Reload()

		''' <summary>获取指定级别及其子集数据</summary>
		''' <param name="id">编号</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Function List(Optional id As Long = 0， Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False, Optional action As Action(Of DataTree) = Nothing) As List(Of DataTree)

		''' <summary>获取指定级别及其子集数据</summary>
		''' <param name="key">上级标识</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Function List(key As String, Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False, Optional action As Action(Of DataTree) = Nothing) As List(Of DataTree)

		''' <summary>获取指定级别及其子集数据的编号</summary>
		''' <param name="id">编号</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Function IDs(Optional id As Long = 0， Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False) As List(Of Long)

		''' <summary>获取指定级别及其子集数据的编号</summary>
		''' <param name="key">上级标识</param>
		''' <param name="includeValue">是否包含最终值，否则只返回组</param>
		''' <param name="includeSelf">是否包含本身</param>
		Function IDs(key As String, Optional includeValue As Boolean = True, Optional includeSelf As Boolean = False) As List(Of Long)

		''' <summary>通过编号获取项目</summary>
		''' <param name="id">编号</param>
		Function Item(id As Long) As DataList

		''' <summary>通过标识获取项目</summary>
		''' <param name="key">标识</param>
		Function Item(key As String) As DataList

#Region "字典数据"

		''' <summary>字典初始化，不存在则创建</summary>
		''' <param name="parentId">上级编号，上级不存在则设置为顶级</param>
		''' <param name="value">添加的值，必填</param>
		''' <param name="Key">自定义别名，非必须</param>
		''' <param name="muti">节点类型（-1. 分组，0.值对象，1. 单选，>1 多选数量）/ 分组下仅能存放单/多选；单/多选下仅能存放值；值下不能存放任何东西</param>
		''' <param name="afterInsert">项目添加成功后的操作，如添加子项目等</param>
		Function DictionaryInsert(parentId As Long, id As Long, value As String, Optional muti As Integer = -1, Optional key As String = "", Optional afterInsert As Action(Of DictionaryEntity) = Nothing, Optional isSystem As Boolean = True) As DictionaryEntity

		''' <summary>字典初始化，不存在则创建</summary>
		''' <param name="parentKey">上级别名，上级不存在则设置未 String.Empty</param>
		''' <param name="value">添加的值，必填</param>
		''' <param name="Key">自定义别名，非必须</param>
		''' <param name="muti">节点类型（-1. 分组，0.值对象，1. 单选，>1 多选数量）/ 分组下仅能存放单/多选；单/多选下仅能存放值；值下不能存放任何东西</param>
		''' <param name="afterInsert">项目添加成功后的操作，如添加子项目等</param>
		Function DictionaryInsert(parentKey As String, value As String, Optional muti As Integer = -1, Optional key As String = "", Optional afterInsert As Action(Of DictionaryEntity) = Nothing, Optional isSystem As Boolean = True) As DictionaryEntity

		''' <summary>是否存在指定标识的项目</summary>
		Function DictionaryExist(id As Long, Optional notExistAction As Action = Nothing) As Boolean

		''' <summary>是否存在指定别名的项目</summary>
		''' <param name="key">别名</param>
		''' <param name="notExistAction">不存在如何操作</param>
		Function DictionaryExist(key As String, Optional notExistAction As Action = Nothing) As Boolean

		'''' <summary>是否存在指定标识的项目</summary>
		'Function DictionaryExist(parentId As Long, value As String, Optional notExistAction As Action = Nothing) As Boolean

		''' <summary>移除指定标识，仅允许移除非系统项目</summary>
		Function DictionaryRemove(id As Long) As Boolean

		''' <summary>通过标识获取字典</summary>
		Default ReadOnly Property DictionaryItem(parentId As Long, dictionaryValue As String) As DictionaryEntity

		''' <summary>通过标识获取字典</summary>
		Default ReadOnly Property DictionaryItem(id As Long) As DictionaryEntity

		''' <summary>通过别名获取字典</summary>
		Default ReadOnly Property DictionaryItem(key As String) As DictionaryEntity

		''' <summary>批量插入值，值只能放在单/多选下</summary>
		''' <param name="parentId">父级节点标识</param>
		''' <param name="values">值字典</param>
		''' <param name="isSystem">是否系统标签</param>
		Function DictionaryInsertValues(parentId As Long, values As IEnumerable(Of String), Optional isSystem As Boolean = False) As Integer

		''' <summary>批量插入值，值只能放在单/多选下</summary>
		''' <param name="parentId">父级节点标识</param>
		''' <param name="values">值字典</param>
		''' <param name="isSystem">是否系统标签</param>
		Function DictionaryInsertValues(parentId As Long, values As IDictionary(Of Long, String), Optional isSystem As Boolean = False) As Integer

		''' <summary>更新值</summary>
		''' <param name="id">标识</param>
		''' <param name="value">新值，如果与旧值一致，则不处理</param>
		Sub DictionaryUpdateValue(id As Long, value As String)

		''' <summary>切换项目启用状态</summary>
		Sub DictionarySwitchEnable(id As Long, value As Boolean)

		''' <summary>切换系统状态</summary>
		Sub DictionarySwitchSystem(id As Long, value As Boolean)

		''' <summary>获取值列表</summary>
		''' <param name="parentId">父级节点标识</param>
		Function DictionaryValues(parentId As Long) As IDictionary(Of Long, String)

		''' <summary>获取值列表</summary>
		Function DictionaryValues(parentKey As String) As IDictionary(Of Long, String)

#End Region

#Region "字典关系数据"

		''' <summary>更新资源的字典关系，返回字典标识</summary>
		''' <param name="value">数据对象</param>
		''' <param name="dicIds">字典编号</param>
		Sub ModuleUpdate(Of T As {IEntity, Class})(value As T, Optional dicIds As IEnumerable(Of Long) = Nothing)

		''' <summary>更新资源的字典关系，返回字典标识</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValue">指定模块数据编号</param>
		''' <param name="dicIds">字典数据</param>
		Sub ModuleUpdate(moduleId As Integer, moduleValue As Long, Optional dicIds As IEnumerable(Of Long) = Nothing)

		'----------------------------------------------------------------

		''' <summary>移除指定模块数据字典数据</summary>
		Function ModuleRemove(Of T As {IEntity, Class})(value As T) As Boolean

		''' <summary>移除指定模块数据字典数据</summary>
		''' <param name="moduleType">模块类型</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Function ModuleRemove(moduleType As Type, ParamArray moduleValues() As Long) As Boolean

		''' <summary>移除指定模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Function ModuleRemove(moduleId As Integer, ParamArray moduleValues() As Long) As Boolean

		'----------------------------------------------------------------

		''' <summary>字典数据</summary>
		''' <param name="value">数据对象</param>
		Function ModuleDictionayIds(Of T As {IEntity, Class})(value As T) As IEnumerable(Of Long)

		''' <summary>获取指定模块数据字典数据标识</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Function ModuleDictionayIds(moduleId As Integer, ParamArray moduleValues() As Long) As IEnumerable(Of Long)

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="value">模块数据</param>
		Function ModuleDictionaies(Of T As {IEntity, Class})(value As T) As IEnumerable(Of DictionaryEntity)

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValue">指定模块数据编号</param>
		Function ModuleDictionaies(moduleId As Integer, moduleValue As Long) As IEnumerable(Of DictionaryEntity)

		'----------------------------------------------------------------

		''' <summary>获取指定字典标识模块数据标识</summary>
		''' <param name="dicIds">指定字典标识模</param>
		Function ModuleValues(Of T As {IEntity, Class})(ParamArray dicIds() As Long) As IEnumerable(Of DictionaryDataEntity)

		''' <summary>获取指定字典标识模块数据标识</summary>
		''' <param name="moduleType">模块类型</param>
		''' <param name="dicIds">指定字典标识模</param>
		Function ModuleValues(moduleType As Type, ParamArray dicIds() As Long) As IEnumerable(Of DictionaryDataEntity)

		''' <summary>获取指定字典标识模块数据标识</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="dicIds">指定字典标识模</param>
		Function ModuleValues(moduleId As Integer, ParamArray dicIds() As Long) As IEnumerable(Of DictionaryDataEntity)

		'----------------------------------------------------------------

		''' <summary>获取模块数据字典数据</summary>
		''' <param name="value">模块数据</param>
		Function DataList(Of T As {IEntity, Class})(Optional value As T = Nothing) As IEnumerable(Of DictionaryDataEntity)

		''' <summary>获取模块数据字典数据</summary>
		''' <param name="moduleType">模块类型</param>
		Function DataList(moduleType As Type) As IEnumerable(Of DictionaryDataEntity)

		''' <summary>获取模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		Function DataList(moduleId As Integer) As IEnumerable(Of DictionaryDataEntity)

		'----------------------------------------------------------------

		''' <summary>字典数据</summary>
		''' <param name="values">数据对象</param>
		Function DataList(Of T As {IEntity, Class})(ParamArray values() As T) As IEnumerable(Of DictionaryDataEntity)

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="moduleType">模块类型</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Function DataList(moduleType As Type, ParamArray moduleValues() As Long) As IEnumerable(Of DictionaryDataEntity)

		''' <summary>获取指定模块数据字典数据</summary>
		''' <param name="moduleId">模块标识</param>
		''' <param name="moduleValues">指定模块数据编号</param>
		Function DataList(moduleId As Integer, ParamArray moduleValues() As Long) As IEnumerable(Of DictionaryDataEntity)

#End Region

		''' <summary>更新项目字典数据</summary>
		Sub EntityUpdate(Of T As {IEntity, Class})(entity As T)

		''' <summary>更新项目字典数据</summary>
		Sub EntitiesUpdate(Of T As {IEntity, Class})(entities As IEnumerable(Of T))

	End Interface

End Namespace