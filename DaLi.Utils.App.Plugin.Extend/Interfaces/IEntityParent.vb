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
' 	基础数据模型接口
'
' 	name: Interface.IEntity
' 	create: 2023-02-15
' 	memo: 基础数据模型接口
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace [Interface]

	''' <summary>基础数据模型接口</summary>
	Public Interface IEntityParent
		Inherits IEntity

		''' <summary>上级</summary>
		<Display(Name:="Parent")>
		<DbQuery(DynamicFilterOperator.Equal)>
		Property ParentId As Long?

	End Interface

End Namespace
