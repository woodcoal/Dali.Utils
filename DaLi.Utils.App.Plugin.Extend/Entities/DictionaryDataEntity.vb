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
' 	字典关系
'
' 	name: Entity.DictionaryDataEntity
' 	create: 2023-02-19
' 	memo: 字典关系
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Entity

	''' <summary>字典关系</summary>
	<DbTable("App_Dictionary_Data")>
	<DbIndex({"DictionaryId", "ModuleId", "ModuleValue"}, IsUnique:=True)>
	<DbIndex({"ModuleId", "ModuleValue"})>
	<DbIndex("DictionaryId")>
	<DbModule(False)>
	Public Class DictionaryDataEntity
		Inherits EntityBase
		Implements IEntityModule

		<DbColumn(IsPrimary:=True, IsIdentity:=True)>
		Public Overrides Property ID As Long

		''' <summary>操作模块标识</summary>
		Public Property ModuleId As UInteger? Implements IEntityModule.ModuleId

		''' <summary>操作模块数据标识</summary>
		Public Property ModuleValue As Long? Implements IEntityModule.ModuleValue

		''' <summary>字典</summary>
		<Display(Name:="Dictionary")>
		<MaxLength(100)>
		Public Property DictionaryId As Long

		''' <summary>字典</summary>
		<Display(Name:="Dictionary")>
		Public Property Dictionary As DictionaryEntity
	End Class

End Namespace