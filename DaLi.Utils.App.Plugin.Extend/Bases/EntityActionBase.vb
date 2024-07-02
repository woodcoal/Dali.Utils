' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	Dali.App Is licensed under GPLv3
'
' ------------------------------------------------------------
'
' 	对实体数据进行二次处理
'
' 	name: Base.EntityActionBase
' 	create: 2024-06-28
' 	memo: 对实体数据进行二次处理。
' 		  只处理属性上存在 EntityCustomAttribute 属性的字段，如： EntityCustom(Provider = "xx", Action = "?", Source = "?")；
' 		  EntityCustomAttribute 的 Provider 必须设置。
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Reflection
Imports FreeSql

Namespace Base

	''' <summary>对实体数据进行 AI 处理</summary>
	Public MustInherit Class EntityActionBase
		Implements IEntityAction

		''' <summary>默认操作源名称(EntityCustom 中 Provider 的值)</summary>
		Protected MustOverride ReadOnly Property ProviderName As String

		''' <summary>默认允许的操作，为空则表示不设置才生效(EntityCustom 中 Action 是否必须在此范围)</summary>
		Protected MustOverride ReadOnly Property EnabledActions As String()

		''' <summary>是否强制需要存在来源字段才能处理(EntityCustom 中 Source 是否必须有效)</summary>
		Protected MustOverride ReadOnly Property SourceForce As Boolean

		''' <summary>排序</summary>
		Public Overridable ReadOnly Property Order As Integer Implements IPlugin.Order
			Get
				Return 0
			End Get
		End Property

		''' <summary>启用</summary>
		Public Overridable ReadOnly Property Enabled As Boolean Implements IPlugin.Enabled
			Get
				' 检查接口，正常则启用
				Return True
			End Get
		End Property

#Region "字段属性"

		''' <summary>字段属性缓存</summary>
		Private Shared _FieldsCache As ImmutableDictionary(Of String, ImmutableDictionary(Of PropertyInfo, ImmutableList(Of (Target As PropertyInfo, Action As String, EmptyOnly As Boolean)))) = ImmutableDictionary.Create(Of String, ImmutableDictionary(Of PropertyInfo, ImmutableList(Of (Target As PropertyInfo, Action As String, EmptyOnly As Boolean))))

		''' <summary>获取属性列表</summary>
		''' <remarks>返回结果中的 Action 为小写；如果仅空值才行修改，则将过滤掉非空字段</remarks>
		Protected Function GetProperties(Of T As IEntity)(entity As T) As ImmutableDictionary(Of PropertyInfo, ImmutableList(Of (Target As PropertyInfo, Action As String, EmptyOnly As Boolean)))
			Dim pros = Properties(entity.GetType)

			Return pros.ToDictionary(Function(x) x.Key,
									 Function(x) x.Value.
													Select(Function(attr)
															   If Not attr.EmptyOnly Then Return attr

															   ' 检查内容是否为空
															   Dim value = attr.Target.GetValue(entity)
															   If value Is Nothing OrElse value.ToString.IsEmpty Then Return attr

															   ' 非空
															   Return Nothing
														   End Function).
													Where(Function(attr) attr.Target IsNot Nothing).
													ToImmutableList).
						Where(Function(x) x.Value.NotEmpty).
						ToImmutableDictionary
		End Function

		''' <summary>获取属性列表</summary>
		''' <remarks>返回结果中的 Action 为小写</remarks>
		Protected ReadOnly Property Properties(type As Type) As ImmutableDictionary(Of PropertyInfo, ImmutableList(Of (Target As PropertyInfo, Action As String, EmptyOnly As Boolean)))
			Get
				Dim cacheName = $"{type.FullName}@@{ProviderName}"
				If _FieldsCache.ContainsKey(cacheName) Then Return _FieldsCache.Item(cacheName)

				' 获取所有有效的属性
				Dim attrs = type.GetEntityCustomAttributes(ProviderName, Not SourceForce)?.
							 Where(Function(x)
									   If EnabledActions.IsEmpty Then
										   Return x.Action.IsEmpty
									   Else
										   Return EnabledActions.Contains(x.Action, StringComparer.OrdinalIgnoreCase)
									   End If
								   End Function).
							 GroupBy(Function(x) x.Source).
							 ToImmutableDictionary(Function(x) x.Key, Function(x) x.Select(Function(y) (y.Target, y.Action?.ToLower, y.EmptyOnly)).ToImmutableList)

				_FieldsCache = _FieldsCache.Add(cacheName, attrs)
				Return attrs
			End Get
		End Property

		''' <summary>更新指定字段</summary>
		''' <param name="entity">实体项目</param>
		''' <param name="properties">属性列表</param>
		''' <param name="value">当前值</param>
		''' <param name="action">指定的操作</param>
		Protected Sub UpdateValue(Of T)(entity As T,
										properties As IEnumerable(Of (Target As PropertyInfo, Action As String)),
										context As IAppContext,
										value As Object,
										action As String,
										Optional valueAction As Func(Of PropertyInfo, Object, Object) = Nothing)
			properties.Where(Function(x) x.Action.Equals(action, StringComparison.OrdinalIgnoreCase)).
				Select(Function(x) x.Target).
				ToList.
				ForEach(Sub(x)
							' 二次处理值
							Dim data = If(valueAction Is Nothing, value, valueAction.Invoke(x, value))

							' 更新值
							UpdateValue(entity, x, context, data)
						End Sub)
		End Sub

		''' <summary>更新指定字段</summary>
		''' <param name="entity">实体项目</param>
		''' <param name="properties">属性列表</param>
		''' <param name="value">当前值</param>
		''' <param name="actions">指定的操作</param>
		Protected Sub UpdateValue(Of T)(entity As T,
										properties As IEnumerable(Of (Target As PropertyInfo, Action As String)),
										context As IAppContext,
										value As Object,
										actions As String(),
										Optional valueAction As Func(Of PropertyInfo, Object, Object) = Nothing)
			properties.Where(Function(x) actions.Contains(x.Action, StringComparer.OrdinalIgnoreCase)).
				Select(Function(x) x.Target).
				ToList.
				ForEach(Sub(x)
							' 二次处理值
							Dim data = If(valueAction Is Nothing, value, valueAction.Invoke(x, value))

							' 更新值
							UpdateValue(entity, x, context, data)
						End Sub)
		End Sub

		''' <summary>更新指定字段</summary>
		''' <param name="entity">实体项目</param>
		''' <param name="pro">属性</param>
		''' <param name="value">当前值</param>
		Protected Sub UpdateValue(Of T)(entity As T, pro As PropertyInfo, context As IAppContext, value As Object)
			' 设置值
			pro.SetValue(entity, value)

			' 标记字段已经修改
			context.Fields.Add(pro.Name, value)
		End Sub

		''' <summary>更新指定字段</summary>
		''' <param name="properties">属性列表</param>
		''' <param name="action">指定的操作</param>
		Protected Function ExistAction(properties As IEnumerable(Of (Target As PropertyInfo, Action As String)), action As String) As Boolean
			Return properties.Any(Function(x) x.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
		End Function


		''' <summary>更新指定字段</summary>
		''' <param name="properties">属性列表</param>
		''' <param name="actions">指定的操作</param>
		Protected Function ExistAction(properties As IEnumerable(Of (Target As PropertyInfo, Action As String)), actions As String()) As Boolean
			Return properties.Any(Function(x) actions.Contains(x.Action, StringComparer.OrdinalIgnoreCase))
		End Function

#End Region

#Region "操作"

		''' <summary>项目操作之前的验证</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Public Overridable Sub ExecuteValidate(Of T As IEntity)(action As EntityActionEnum, entity As T, context As IAppContext, errorMessage As ErrorMessage, db As IFreeSql, Optional source As T = Nothing) Implements IEntityAction.ExecuteValidate
		End Sub

		''' <summary>项目操作完成后处理事件</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="data">单项操作时单个数值，多项时为数组</param>
		''' <param name="source">单项编辑时更新前的原始值</param>
		Public Overridable Sub ExecuteFinish(Of T As IEntity)(action As EntityActionEnum, data As ObjectArray(Of T), context As IAppContext, errorMessage As ErrorMessage, db As IFreeSql, Optional source As T = Nothing) Implements IEntityAction.ExecuteFinish
		End Sub

		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Public Overridable Sub ExecuteQuery(Of T As {Class, IEntity})(action As EntityActionEnum, query As ISelect(Of T), Optional queryVM As QueryBase(Of T) = Nothing) Implements IEntityAction.ExecuteQuery
		End Sub

#End Region
	End Class

End Namespace