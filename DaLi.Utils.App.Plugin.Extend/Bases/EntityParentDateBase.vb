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
' 	父级时间数据模型基类
'
' 	name: Base.EntityParentDateBase
' 	create: 2023-02-27
' 	memo: 父级时间数据模型基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace Base

	''' <summary>父级时间数据模型基类</summary>
	Public MustInherit Class EntityParentDateBase
		Inherits EntityDateBase
		Implements IEntityParent

		''' <summary>上级</summary>
		<Display(Name:="Parent")>
		<DbQuery(DynamicFilterOperator.Equal)>
		Public Overridable Property ParentId As Long? Implements IEntityParent.ParentId
	End Class

End Namespace