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
' 	实体数据附加操作
'
' 	name: Helper.EntityHelper
' 	create: 2023-10-11
' 	memo: 实体数据附加操作
'
' ------------------------------------------------------------

Namespace Helper
	''' <summary>实体数据附加操作</summary>
	Public NotInheritable Class EntityHelper

		''' <summary>从对象数组中获取单个对象，并禁止批量操作</summary>
		''' <param name="data">对象数组</param>
		''' <param name="errorMessage">错误消息对象</param>
		Public Shared Function GetEntity(Of T As {IEntity, Class})(data As ObjectArray(Of T), Optional errorMessage As ErrorMessage = Nothing, Optional defaultValue As T = Nothing) As T
			Dim entity = data?.ToOne
			If entity Is Nothing Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NoData"
				Return defaultValue
			End If

			' 不允许批量操作
			If data.IsMuti Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NoBatch"
				Return defaultValue
			End If

			Return entity
		End Function

		''' <summary>从对象获取原始数据</summary>
		''' <param name="errorMessage">错误消息对象</param>
		Public Shared Function GetSource(Of T As {IEntity, Class})(entity As T, db As IFreeSql, Optional errorMessage As ErrorMessage = Nothing) As T
			If entity.ID = 0 Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 原始对象
			Dim source = db.Select(Of T).WhereID(entity.ID).ToOne
			If source Is Nothing Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NotFound"
				Return Nothing
			End If

			Return source
		End Function

		''' <summary>从对象数组中获取单个对象，并禁止批量操作，同时从数据库获取存在的数据</summary>
		''' <param name="data">对象数组</param>
		''' <param name="errorMessage">错误消息对象</param>
		Public Shared Function GetEntitySource(Of T As {IEntity, Class})(data As ObjectArray(Of T), db As IFreeSql, Optional errorMessage As ErrorMessage = Nothing) As (Entity As T, Source As T)
			Dim entity = GetEntity(data, errorMessage)
			If entity Is Nothing Then Return Nothing

			Dim source = GetSource(entity, db, errorMessage)

			Return (entity, source)
		End Function

		''' <summary>操作验证，检查后续的操作是否在当前操作列表中</summary>
		''' <param name="action">当前操作</param>
		''' <returns>是否允许操作</returns>
		Public Shared Function EnActions(action As EntityActionEnum, actions As EntityActionEnum(), Optional errorMessage As ErrorMessage = Nothing) As Boolean
			' 检查是否允许操作
			Dim isPass = actions.Contains(action)

			' 未通过后的操作
			If Not isPass Then
				Dim enMessage = {EntityActionEnum.ADD, EntityActionEnum.EDIT, EntityActionEnum.DELETE}.Contains(action) AndAlso errorMessage IsNot Nothing
				If enMessage Then errorMessage.Notification = "Error.NoAction"
			End If

			Return isPass
		End Function

		''' <summary>操作验证，检查后续的操作是否不在当前操作列表中</summary>
		''' <param name="action">当前操作</param>
		''' <param name="actions">允许的操作列表</param>
		''' <param name="errorMessage">对于加、改、删操作，在不允许的时候是否提示异常，提示则需要设置消息对象</param>
		''' <returns>是否允许操作</returns>
		Public Shared Function DisActions(action As EntityActionEnum, actions As EntityActionEnum(), Optional errorMessage As ErrorMessage = Nothing) As Boolean
			Return Not EnActions(action, actions, errorMessage)
		End Function

		''' <summary>类型值验证</summary>
		''' <param name="field">值字段</param>
		''' <param name="value">原始字符</param>
		''' <param name="type">数据类型</param>
		''' <returns>有效的数据转换后的文本</returns>
		Public Shared Function TypeValueValidate(field As String, value As String, type As FieldTypeEnum, errorMessage As ErrorMessage, Optional enEmpty As Boolean = False) As String
			' 验证值类型
			Dim data = value.EmptyValue().ToValue(type)

			If data Is Nothing Then
				If Not enEmpty Then errorMessage.Add(field, "您提交的值无效")
			Else
				Select Case type
					Case FieldTypeEnum.BOOLEAN, FieldTypeEnum.TRISTATE, FieldTypeEnum.NUMBER, FieldTypeEnum.BYTES, FieldTypeEnum.INTEGER, FieldTypeEnum.LONG, FieldTypeEnum.SINGLE, FieldTypeEnum.DOUBLE, FieldTypeEnum.DATETIME, FieldTypeEnum.JSON
								' 无需检查

					Case FieldTypeEnum.GUID
						If data = Guid.Empty Then errorMessage.Add(field, "无效 GUID 数据")

					Case Else
						' 不能为空
						If String.IsNullOrWhiteSpace(data) And Not enEmpty Then errorMessage.Add(field, "您提交的值无效")
				End Select
			End If

			Return If(errorMessage.IsPass, TypeExtension.ToObjectString(data), "")
		End Function
	End Class
End Namespace