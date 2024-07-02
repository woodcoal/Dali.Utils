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
' 	时间数据模型基类
'
' 	name: Base.EntityDateBase
' 	create: 2023-02-25
' 	memo: 时间数据模型基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace Base

	''' <summary>时间数据模型基类</summary>
	Public MustInherit Class EntityDateBase
		Inherits EntityBase
		Implements IEntityDate

		''' <summary>添加时间</summary>
		<DbColumn(Position:=-4)>
		<Output(TristateEnum.TRUE)>
		<DbQuery(DynamicFilterOperator.DateRange)>
		Public Property CreateTime As Date? Implements IEntityDate.CreateTime

		''' <summary>添加操作者</summary>
		<Output(TristateEnum.TRUE)>
		<MaxLength(50)>
		<DbColumn(Position:=-3)>
		<DbQuery(DynamicFilterOperator.Equal)>
		Public Property CreateBy As String Implements IEntityDate.CreateBy

		''' <summary>更新时间</summary>
		<DbColumn(Position:=-2)>
		<DbQuery(DynamicFilterOperator.DateRange)>
		Public Property UpdateTime As Date? Implements IEntityDate.UpdateTime

		''' <summary>更新操作者</summary>
		<MaxLength(50)>
		<DbColumn(Position:=-1)>
		<DbQuery(DynamicFilterOperator.Equal)>
		Public Property UpdateBy As String Implements IEntityDate.UpdateBy
	End Class

End Namespace