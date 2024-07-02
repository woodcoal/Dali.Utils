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
' 	审计实体
'
' 	name: Entity.AuditEntity
' 	create: 2024-01-15
' 	memo: 数据审计
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports FreeSql.Internal.Model

Namespace Entity

	''' <summary>数据审计</summary>
	<DbTable("App_Audit_{yyyy}", AsTable:="CreateTime=2024-01-01(1 year)")>
	<DbIndex({"ModuleValue", "ModuleID"})>
	<DbModule(3, "数据审计")>
	Public Class AuditEntity
		Inherits EntityDateExtendBase
		Implements IEntityFlag, IEntityModule

		<DbColumn(IsPrimary:=True, IsIdentity:=True)>
		Public Overrides Property ID As Long

		''' <summary>操作模块标识</summary>
		<DbQuery>
		Public Property ModuleId As UInteger? Implements IEntityModule.ModuleId

		''' <summary>操作模块数据标识</summary>
		<DbQuery>
		Public Property ModuleValue As Long? Implements IEntityModule.ModuleValue

		''' <summary>字段</summary>
		<MaxLength(100)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property Name As String

		''' <summary>修改前的值</summary>
		<NotMapped>
		Public Property SourceValue As String
			Get
				Return Extension("SourceValue")
			End Get
			Set(value As String)
				Extension("SourceValue") = value
			End Set
		End Property

		''' <summary>修改后的值</summary>
		<NotMapped>
		Public Property TargetValue As String
			Get
				Return Extension("TargetValue")
			End Get
			Set(value As String)
				Extension("TargetValue") = value
			End Set
		End Property

		''' <summary>类型</summary>
		<NotMapped>
		Public Property Type As String
			Get
				Return Extension("Type")
			End Get
			Set(value As String)
				Extension("Type") = value
			End Set
		End Property

		''' <summary>结果</summary>
		<MaxLength(250)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property Result As String

		''' <summary>是否通过</summary>
		<DbQuery>
		Public Property Pass As TristateEnum

		''' <summary>标记</summary>
		Public Property Flag As Integer Implements IEntityFlag.Flag

	End Class
End Namespace