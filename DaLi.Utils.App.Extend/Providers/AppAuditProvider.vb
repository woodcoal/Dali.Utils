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
' 	数据审计
'
' 	name: AppAuditProvider
' 	create: 2024-01-15
' 	memo: 数据审计
'
' ------------------------------------------------------------

Namespace Provider
	Public Class AppAuditProvider
		Implements IAppAuditProvider

		''' <summary>数据库对象</summary>
		Protected ReadOnly Db As IFreeSql

		''' <summary>模型操作</summary>
		Private ReadOnly _ModuleProvider As AppModuleProvider

		''' <summary>构造</summary>
		''' <param name="db">数据库对象</param>
		Public Sub New(db As IFreeSql, moduleProvider As AppModuleProvider)
			Me.Db = db
			_ModuleProvider = moduleProvider
		End Sub

		''' <summary>对象审计</summary>
		''' <param name="fields">审计字段</param>
		''' <param name="entity">对象</param>
		''' <param name="source">原始对象，不存在则从对象中查询</param>
		''' <param name="user">操作用户，不存在则从对象中获取</param>
		Public Function Audit(Of T As {IEntity, Class})(fields As String(), entity As T, source As IEntity, Optional user As String = "") As IDictionary(Of String, String) Implements IAppAuditProvider.Audit
			If fields.IsEmpty OrElse entity Is Nothing Then Return Nothing

			' 原始数据，不存在则查询
			source = If(source, Db.Select(Of T).WhereID(entity.ID).ToOne)
			If source Is Nothing Then Return Nothing

			' 对象类型
			Dim entityType = GetType(T)

			' 更新时间
			If user.IsEmpty AndAlso entityType.IsComeFrom(Of IEntityDate) Then
				Dim entityData = TryCast(entity, IEntityDate)
				If entityData IsNot Nothing Then user = entityData.UpdateBy
			End If

			' 存在审计字段，检查是否有数据变化
			Dim pros = fields.Select(Function(x) entityType.GetSingleProperty(x)).Where(Function(x) x IsNot Nothing).ToList
			If pros.IsEmpty Then Return Nothing

			' 审计
			Dim data = New List(Of AuditEntity)
			Dim moduleId = ExtendHelper.GetModuleId(entityType)
			For Each x In pros
				' 原始值
				Dim sourceValue = x.GetValue(source)

				' 新值
				Dim targetValue = x.GetValue(entity)

				' 两值未变化则不处理
				If (sourceValue Is Nothing AndAlso targetValue Is Nothing) OrElse sourceValue = targetValue Then Continue For

				' 禁止更新此字段
				x.SetValue(entity, sourceValue)

				' 检查同用户是否重复提交，重复则先取消之前的申请
				Dim items = Db.Select(Of AuditEntity).
					WhereEquals(moduleId, Function(e) e.ModuleId).
					WhereEquals(entity.ID, Function(e) e.ModuleValue).
					WhereEquals(x.Name, Function(e) e.Name).
					WhereEquals(TristateEnum.DEFAULT, Function(e) e.Pass).
					WhereEquals(user, Function(e) e.CreateBy).
					ToList

				If items.NotEmpty Then
					items.ForEach(Sub(item)
									  item.Pass = TristateEnum.FALSE
									  item.Result = "已有新申请提交，旧申请直接取消"
									  item.UpdateTime = SYS_NOW_DATE
									  item.UpdateBy = user
								  End Sub)

					Db.Update(Of AuditEntity).SetSource(items).ExecuteAffrows()
				End If

				data.Add(New AuditEntity With {
					 .Flag = 1,
					 .CreateTime = SYS_NOW_DATE,
					 .CreateBy = user,
					 .Name = x.Name,
					 .SourceValue = TypeExtension.ToObjectString(sourceValue),
					 .TargetValue = TypeExtension.ToObjectString(targetValue),
					 .Type = targetValue?.GetType.Name,
					 .ModuleId = ExtendHelper.GetModuleId(entityType),
					 .ModuleValue = entity.ID,
					 .Result = "",
					 .Pass = TristateEnum.DEFAULT
				})
			Next

			If data.NotEmpty Then Db.Insert(data).ExecuteAffrows()

			Return data.ToDictionary(Function(x) x.Name, Function(x) x.TargetValue)
		End Function

		''' <summary>数据审计</summary>
		Public Function Audit(Of T As {IEntity, Class})(ByRef entity As T, Optional source As IEntity = Nothing, Optional user As String = "") As IDictionary(Of String, String) Implements IAppAuditProvider.Audit
			Dim entityType = entity.GetType

			' 审计字段检测
			Dim audits = _ModuleProvider.GetModuleAudit(entityType)
			If audits Is Nothing Then Return Nothing

			Return Audit(audits, entity, source)
		End Function

		''' <summary>批量数据审计</summary>
		Public Function Audit(Of T As {IEntity, Class})(entities As IEnumerable(Of T), Optional user As String = "") As Integer Implements IAppAuditProvider.Audit
			If entities.IsEmpty Then Return 0

			' 类型
			Dim entityType = GetType(T)

			' 审计字段检测
			Dim moduleId = _ModuleProvider.GetModuleId(entityType)
			Dim audits = _ModuleProvider.GetModuleAudit(moduleId)
			If audits Is Nothing Then Return 0

			Return entities.
				ToList.
				Select(Function(x) Audit(audits, x, Nothing, user)).
				Where(Function(x) x IsNot Nothing).
				Count
		End Function

		''' <summary>审计确认</summary>
		Public Function Validate(audit As AuditEntity, Optional message As String = "", Optional user As String = "") As String Implements IAppAuditProvider.Validate
			If audit Is Nothing OrElse
				audit.Pass <> TristateEnum.DEFAULT OrElse
				audit.Name.IsEmpty OrElse
				audit.ModuleId < 1 OrElse
				audit.ModuleValue = 0 Then Return "无效审计信息"

			audit.UpdateTime = SYS_NOW_DATE
			audit.UpdateBy = user

			' 审计未通过
			If message.NotEmpty Then
				audit.Pass = TristateEnum.FALSE
				audit.Result = message
				Dim flag = Db.Update(Of AuditEntity).SetSource(audit).ExecuteAffrows() > 0
				Return If(flag, "", "审计异常")
			End If

			' 审计字段
			Dim type = _ModuleProvider.GetModuleType(audit.ModuleId)
			If type Is Nothing Then Return "无此类型审计数据"

			' 检查是否存在多条类似的资讯，如果存在多条需要先拒绝其他后再处理
			Dim count = Db.Select(Of AuditEntity).
				WhereEquals(audit.ModuleId, Function(x) x.ModuleId).
				WhereEquals(audit.ModuleValue, Function(x) x.ModuleValue).
				WhereEquals(audit.Name, Function(x) x.Name).
				WhereEquals(TristateEnum.DEFAULT, Function(x) x.Pass).
				Count
			If count > 1 Then Return "存在多条类似审计数据"

			' 审计通过
			audit.Pass = TristateEnum.TRUE

			' 处理
			Dim tableName = ExtendHelper.TableName(type)
			If tableName.Name.IsEmpty Then Return False

			Dim value = audit.TargetValue.ToValue(audit.Type)
			If value Is Nothing Then Return "要更新的数据无效，无法审计"

			Dim dic As New Dictionary(Of String, Object) From {
				{"ID", audit.ModuleValue},
				{audit.Name, value}
			}

			If Db.UpdateDict(dic).AsTable(tableName.Name).WherePrimary("id").ExecuteAffrows() < 1 Then Return "更新审计数据失败"
			If Db.Update(Of AuditEntity).SetSource(audit).ExecuteAffrows() < 1 Then Return "审计数据已经更新，但是审计结果更新异常"

			Return ""
		End Function

	End Class
End Namespace