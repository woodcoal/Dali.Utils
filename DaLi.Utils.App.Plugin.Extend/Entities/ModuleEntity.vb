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
' 	模块登记表
'
' 	name: Entity.ModuleEntity
' 	create: 2023-02-19
' 	memo: 模块登记表
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Entity

	''' <summary>模块登记表</summary>
	<DbTable("App_Module")>
	<DbModule(0, "模型")>
	Public Class ModuleEntity
		Inherits EntityBase

		''' <summary>模块标识，非自增，需手工设置 ID</summary>
		<DbColumn(IsPrimary:=True, IsIdentity:=False)>
		Public Shadows Property ID As UInteger
			Get
				Return MyBase.ID
			End Get
			Set(value As UInteger)
				MyBase.ID = value
			End Set
		End Property

		''' <summary>模块名称 (FullName)</summary>
		<Display(Name:="模块名称")>
		<Required>
		<MaxLength(150)>
		Public Property Name As String

		''' <summary>标题</summary>
		<MaxLength(150)>
		Public Property Title As String

		''' <summary>搜索字段，多个逗号间隔</summary>
		<MaxLength(250)>
		Public Property Search As String

		''' <summary>审计最短，多个逗号间隔；审计字段在修改时需要后台确认</summary>
		<MaxLength(250)>
		Public Property Audit As String

		''' <summary>更新时间</summary>
		Public Property Update As Date

	End Class
End Namespace