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
' 	模块标识操作
'
' 	name: Provider.AppModuleProvider
' 	create: 2023-02-19
' 	memo: 模块标识操作
'
'	模型标识需要手动设置，不允许重复，通过 DbModuleAttribute 设置
'	自动编号从 100000 开始
'	如需要指定编号的雪花算法，则雪花算法的 modelId 支持 1-32，分别用 1xxx-32xxx 表示，如： 32xx 表示雪花算法使用 32 的 modelId ,其他编号则自动设置
'	搜索字段需要原始模型的字段已经设置了可以搜索，且字段为文本类型才可以使用
'	审计字段主要不要将主键或者超长复杂字段做为审计字段，否则将导致审计数据入库异常
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports FreeSql.Internal.Model
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Logging

Namespace Provider

	''' <summary>模块标识操作</summary>
	Public Class AppModuleProvider
		Inherits ProviderBase
		Implements IAppModuleProvider

		''' <summary>构造函数</summary>
		Public Sub New(db As IFreeSql, log As ILogger(Of AppModuleProvider))
			MyBase.New(db, log, E_MODULE_RELOAD)
		End Sub

		''' <summary>锁</summary>
		Private ReadOnly _Lock As New Object

		''' <summary>是否已经初始化</summary>
		Private _IsInit As Boolean = False

		''' <summary>内置数据，初始化默认基础项目</summary>
		Private _ModuleList As ImmutableDictionary(Of UInteger, (Name As String, Title As String, Search As String(), Audit As String())) = ImmutableDictionary.Create(Of UInteger, (Name As String, Title As String, Search As String(), Audit As String()))

		''' <summary>内置数据，初始化默认基础项目</summary>
		Public ReadOnly Property ModuleList As ImmutableDictionary(Of UInteger, (Name As String, Title As String, Search As String(), Audit As String())) Implements IAppModuleProvider.ModuleList
			Get
				If Not _IsInit Then Reload(False)
				Return _ModuleList
			End Get
		End Property

		''' <summary>重载数据，注意需要系统启动后立即执行</summary>
		Public Overrides Sub Reload() Implements IAppModuleProvider.Reload
			Reload(True)
		End Sub

		''' <summary>重载数据，注意需要系统启动后立即执行</summary>
		Public Overloads Sub Reload(mustReload As Boolean)
			If _IsInit AndAlso Not mustReload Then Return

			SyncLock _Lock
				' 获取初始数据
				Dim dics = Db.Select(Of ModuleEntity).ToDictionary(Function(x) x.ID, Function(x) (x.Name, x.Title, x.Search.SplitEx, x.Audit.SplitEx))

				' 是否需要初始化条件
				' 1. 目前数据库没有任何数据
				' 2. 调试模式，每次系统启动都重新初始化
				If dics.IsEmpty OrElse (SYS.Debug AndAlso Not _IsInit) Then
					If dics.IsEmpty Then dics = New Dictionary(Of UInteger, (Name As String, Title As String, String(), String()))

					' 基础数据不存在，需要按系统默认入库一次参数
					' 1xxx-32xxx 号模型使用带 moduleId 标识 雪花算法 ID，所以需要此方法的一定要存入低位ID
					' 否则直接使用全 ID 算法
					' 获取所有内置模块
					Dim entities = SYS.Plugins.
						GetTypes(Of IEntity).
						Select(Function(x) GetFields(x)).
						Where(Function(x) x IsNot Nothing).
						OrderBy(Function(x) x.ID).
						ToList

					' 检查是否存在重复项目
					Dim items = entities.
						Where(Function(x) x.ID < UInteger.MaxValue).
						GroupBy(Function(x) x.ID).Where(Function(x) x.Count > 1).
						ToList

					If items.NotEmpty Then
						' 存在重复项目，需要检查系统，当前无法继续执行
						Dim message = items.
							Select(Function(x) $"标识【{x.Key}】存在 {x.Count} 条重复模块 {x.Select(Function(y) y.Name).JoinString("、")}").
							JoinString("；" & vbCrLf)

						Throw New Exception($"{vbCrLf}存在重复项目，无法继续执行。异常模块：{vbCrLf}{message}")
					End If

					' 检查未入库的项目
					Dim Ids = dics.Keys.ToList
					entities = entities.
						Where(Function(x) Not Ids.Contains(x.ID)).
						OrderBy(Function(x) x.ID).
						ToList

					' 获取最大标识
					Dim max = Db.Select(Of ModuleEntity).OrderByDescending(Function(x) x.ID).ToOne(Function(x) x.ID)
					If max < 10000 Then max = 10000

					' 标注标识
					entities.ForEach(Sub(item)
										 max += 1

										 ' 为设置具体标识的项目 标识从 100000 开始计数，方便手动设置标识不被覆盖
										 If item.ID = UInteger.MaxValue Then
											 item.ID = max
										 End If
									 End Sub)

					' 入库
					Db.Insert(entities).ExecuteAffrows()

					' 重新加载
					dics = Db.Select(Of ModuleEntity).ToDictionary(Function(x) x.ID, Function(x) (x.Name, x.Title, x.Search.SplitEx, x.Audit.SplitEx))
				End If

				_ModuleList = dics.ToImmutableDictionary
				_IsInit = True
			End SyncLock
		End Sub

		''' <summary>获取模糊搜索字段</summary>
		''' <remarks>所有字段为字符串，且允许使用模糊搜索的属性将自动做为模糊搜索字段</remarks>
		Private Shared Function GetFields(type As Type) As ModuleEntity
			' 检查是否允许显示，未设置属性则自动添加默认属性，允许显示
			Dim attr = type.GetCustomAttribute(Of DbModuleAttribute)
			If attr Is Nothing Then attr = New DbModuleAttribute(True)
			If Not attr.Show Then Return Nothing

			Dim ID = attr.ID
			Dim Name = UpdateName(type)
			Dim Title = attr.Name.EmptyValue(Name)
			Dim Search = If(attr.Searchable, type.
				GetAllProperties.
				Select(Function(pro)
						   If pro.IsPublic AndAlso pro.CanRead AndAlso pro.CanWrite AndAlso pro.PropertyType.IsString Then
							   Dim attrMap = pro.GetCustomAttribute(Of NotMappedAttribute)
							   If attrMap Is Nothing Then
								   Dim attrQuery = pro.GetCustomAttribute(Of DbQueryAttribute)
								   If attrQuery?.QueryOperator = DynamicFilterOperator.Contains Then Return pro.Name
							   End If
						   End If

						   Return ""
					   End Function).
				Where(Function(x) x.NotEmpty).
				JoinString(","), "")

			Return New ModuleEntity With {.ID = ID, .Name = Name, .Title = Title, .Search = Search, .Update = SYS_NOW_DATE}
		End Function

		''' <summary>更新模型名称，移除多余名称，如：结尾的 Entity</summary>
		Public Shared Function UpdateName(name As String) As String
			If name.IsEmpty Then Return ""
			If name.EndsWith("Entity", StringComparison.OrdinalIgnoreCase) Then name = name.Substring(0, name.Length - 6)
			If name.Contains(".Entity.", StringComparison.OrdinalIgnoreCase) Then name = name.Replace(".Entity.", ".", StringComparison.OrdinalIgnoreCase)
			Return name
		End Function

		''' <summary>更新模型名称，移除多余名称，如：结尾的 Entity</summary>
		Public Shared Function UpdateName(type As Type) As String
			Return UpdateName(type.FullName)
		End Function

		''' <summary>验证对象是否有效，并返回有效对象</summary>
		Public Function Validate(moduleId As UInteger?, moduleValue As Long?) As Boolean Implements IAppModuleProvider.Validate
			If Not moduleId.HasValue OrElse moduleId < 1 OrElse Not moduleValue.HasValue OrElse moduleValue.Value = 0 Then Return False

			Dim moduleType = GetModuleType(moduleId.Value)
			If moduleType Is Nothing Then Return False

			Return Db.Select(moduleType).WhereID(moduleValue.Value).Any
		End Function

		''' <summary>递归从类型的模块属性中获取标识</summary>
		Public Function GetModuleId(type As Type) As UInteger Implements IAppModuleProvider.GetModuleId
			If type Is Nothing OrElse type = GetType(Object) Then Return 0

			Dim attr = type.GetCustomAttribute(Of DbModuleAttribute)
			If attr IsNot Nothing AndAlso attr.Show AndAlso attr.ID > 0 Then
				If attr.ID = UInteger.MaxValue Then
					' 从类型名称获取
					Return GetModuleId(type.FullName)
				Else
					' 从属性获取
					Return attr.ID
				End If
			Else
				Return GetModuleId(type.BaseType)
			End If
		End Function

		''' <summary>获取模块标识 标识 0 为 模块表本身，返回 0 则无效</summary>
		Public Function GetModuleId(moduleName As String) As UInteger Implements IAppModuleProvider.GetModuleId
			moduleName = UpdateName(moduleName)
			If moduleName.IsEmpty Then Return 0

			Return ModuleList.Where(Function(x) x.Value.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Key).FirstOrDefault
		End Function

		''' <summary>获取模块名称</summary>
		Public Function GetModuleName(moduleId As UInteger) As String Implements IAppModuleProvider.GetModuleName
			If moduleId < 1 Then
				Return Nothing
			Else
				Return If(ModuleList.ContainsKey(moduleId), ModuleList(moduleId).Name, Nothing)
			End If
		End Function

		''' <summary>获取模块搜索字段</summary>
		Public Function GetModuleSearch(moduleId As UInteger) As String() Implements IAppModuleProvider.GetModuleSearch
			If moduleId < 1 Then
				Return Nothing
			Else
				Return If(ModuleList.ContainsKey(moduleId), ModuleList(moduleId).Search, Nothing)
			End If
		End Function

		''' <summary>获取模块审计字段</summary>
		Public Function GetModuleAudit(moduleId As UInteger) As String() Implements IAppModuleProvider.GetModuleAudit
			If moduleId < 1 Then
				Return Nothing
			Else
				Return If(ModuleList.ContainsKey(moduleId), ModuleList(moduleId).Audit, Nothing)
			End If
		End Function

		Public Function GetModuleAudit(type As Type) As String() Implements IAppModuleProvider.GetModuleAudit
			Return GetModuleAudit(GetModuleId(type))
		End Function

		''' <summary>获取模块类型</summary>
		''' <param name="moduleId">有效的模块标识</param>
		Public Function GetModuleType(moduleId As UInteger) As Type Implements IAppModuleProvider.GetModuleType
			Dim name = GetModuleName(moduleId)
			If name.IsEmpty Then Return Nothing

			Return SYS.Plugins.GetTypes(Of IEntity).
				Where(Function(x) UpdateName(x.FullName).Equals(name, StringComparison.OrdinalIgnoreCase)).
				FirstOrDefault
		End Function

		''' <summary>是否包含系统内置模块</summary>
		Public Function ModuleNames(Optional incSystem As Boolean = False) As List(Of String) Implements IAppModuleProvider.ModuleNames
			Dim names = ModuleList.Values.Select(Function(x) x.Name).ToList

			If incSystem Then
				Return names
			Else
				Return names.Where(Function(x) Not x.Like({"dali.utils.*", "dali.framework.*"})).ToList
			End If
		End Function

		''' <summary>是否包含系统内置模块</summary>
		''' <param name="mode">Default 仅返回可模糊查询的数据，true 全部返回，false 仅返回系统数据</param>
		Public Function ModuleDatas(Optional mode As TristateEnum = TristateEnum.DEFAULT) As List(Of DataList) Implements IAppModuleProvider.ModuleDatas
			Dim list = ModuleList.Select(Function(x) New With {x.Key, x.Value.Title, x.Value.Name, x.Value.Search})

			Select Case mode
				Case TristateEnum.TRUE
					'list = ModuleList

				Case TristateEnum.FALSE
					list = list.Where(Function(x) Not x.Name.Like({"dali.utils.*", "dali.framework.*"}))

				Case Else
					list = list.Where(Function(x) x.Search.NotEmpty)

			End Select

			Return list.Select(Function(x) New DataList With {.Value = x.Key, .Text = x.Title, .Ext = x.Name}).ToList
		End Function

		''' <summary>获取资源基础信息</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="moduleValue">模块资源值</param>
		Public Function GetModuleInfo(moduleId As UInteger?, moduleValue As Long?) As String Implements IAppModuleProvider.GetModuleInfo
			Dim moduleInfo = Info(moduleId, moduleValue)
			If moduleInfo Is Nothing Then Return Nothing

			Dim keys = GetModuleSearch(moduleId)
			If keys?.NotEmpty Then Return moduleInfo(keys(0))?.ToString

			Return Nothing
		End Function

		''' <summary>获取资源基础信息</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="moduleValue">模块资源值</param>
		Public Function Info(moduleId As UInteger?, moduleValue As Long?) As IDictionary(Of String, Object) Implements IAppModuleProvider.Info
			If Not moduleId.HasValue OrElse moduleId < 1 OrElse Not moduleValue.HasValue OrElse moduleValue.Value = 0 Then Return Nothing

			Dim moduleType = GetModuleType(moduleId.Value)
			If moduleType Is Nothing Then Return Nothing

			Return Db.Select(moduleType).WhereID(moduleValue.Value).ToOne?.ToDictionary(False)
		End Function

		''' <summary>模糊搜索</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="keyword">关键词</param>
		Public Function Search(moduleId As UInteger, keyword As String, Optional count As Integer = 25) As List(Of DataList) Implements IAppModuleProvider.Search
			If moduleId < 1 OrElse keyword.IsEmpty Then Return Nothing

			' 类型是否存在
			Dim moduleType = GetModuleType(moduleId)
			If moduleType Is Nothing Then Return Nothing

			' 搜索字段是否存在
			Dim moduleSearch = GetModuleSearch(moduleId)
			If moduleSearch.IsEmpty Then Return Nothing

			' 所有有效键
			Dim pros = moduleType.GetOutputFields(False).Select(Function(x) x.Key.Name).ToList

			' 名称键
			Dim nameFields = moduleSearch.
						Where(Function(x) Not x.Equals("ID", StringComparison.OrdinalIgnoreCase) AndAlso pros.Contains(x, StringComparer.OrdinalIgnoreCase)).
						ToArray

			' 禁用键
			Dim disabledField = {"disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 值键
			Dim valueField = "ID"

			' 是否禁用反向，有可能字段是允许，所以要反向
			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

			' 返回数量
			count = count.Range(1, 100)

			' 条件
			Dim filter As New DynamicFilterInfo With {
				.Logic = DynamicFilterLogic.Or,
				.Filters = New List(Of DynamicFilterInfo)
			}

			Dim IdType = moduleType.GetSingleProperty("ID").PropertyType
			Dim Id = keyword.ToValue(IdType)
			If Id IsNot Nothing AndAlso Id <> 0 Then
				Dim filterID As New DynamicFilterInfo With {.Field = "ID", .[Operator] = DynamicFilterOperator.Eq, .Value = Id}
				filter.Filters.Add(filterID)
			End If

			For Each nameField In nameFields
				Dim filterName As New DynamicFilterInfo With {.Field = nameField, .[Operator] = DynamicFilterOperator.Contains, .Value = keyword}
				filter.Filters.Add(filterName)
			Next

			' 默认查询对象
			Dim query = Db.Select(moduleType).WhereDynamicFilter(filter)

			' 数量限制
			query.Take(count)


			Dim sql = query.ToSql

			' 所有结果字段 
			Dim retuenFields = nameFields.Union({disabledField, valueField}).Distinct.ToArray

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
												If text.NotEmpty AndAlso text.Contains(keyword, StringComparison.OrdinalIgnoreCase) Then
													data.Text = text
													Exit For
												End If
											Next

											' 如果获取不到值，则直接返回 标识
											If data.Text.IsEmpty Then
												If nameFields.NotEmpty Then
													data.Text = dic.Where(Function(x) x.Key.IsSame(nameFields(0))).Select(Function(x) x.Value).FirstOrDefault
												Else
													data.Text = data.Value
												End If
											End If

											Return data
										End Function).
										Distinct(Function(x) x.Text).ToList
		End Function

		''' <summary>获取字段中附件内容</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="moduleValue">标识</param>
		''' <param name="field">查询字段，确保字段为 byte()</param>
		''' <param name="contentType">附件类型，如: image/jpg application/octet-stream</param>
		''' <returns></returns>
		Public Function File(moduleId As UInteger, moduleValue As Long, field As String, Optional contentType As String = "") As IActionResult
			If moduleId < 1 OrElse moduleValue = 0 OrElse field.IsEmpty Then Return New BadRequestResult

			Dim type = GetModuleType(moduleId)
			If type Is Nothing Then Return New BadRequestResult

			Dim item = Db.Select(type).WhereEquals(moduleValue, Function(x) x.ID).ToOne()
			If item Is Nothing Then Return New BadRequestObjectResult("无此数据")

			Dim pro = item.GetType.GetSingleProperty(field)
			If pro Is Nothing OrElse pro.PropertyType <> GetType(Byte()) Then Return New BadRequestObjectResult("字段类型错误")

			Dim value As Byte() = pro.GetValue(item)
			If value Is Nothing OrElse value.Length < 1 Then Return New BadRequestObjectResult("无此数据")

			Return New FileContentResult(value, contentType.EmptyValue("application/octet-stream"))
		End Function
	End Class

End Namespace