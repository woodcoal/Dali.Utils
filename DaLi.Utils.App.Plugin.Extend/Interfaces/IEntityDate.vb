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
' 	带操作时间及对象的数据接口
'
' 	name: Interface.IEntityDate
' 	create: 2023-02-15
' 	memo: 带操作时间及对象的数据接口
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace [Interface]

	''' <summary>带操作时间及对象的数据接口</summary>
	Public Interface IEntityDate
		Inherits IEntity

		''' <summary>添加时间</summary>
		<DbColumn(Position:=-4)>
		<Output(TristateEnum.TRUE)>
		<DbQuery(DynamicFilterOperator.DateRange)>
		Property CreateTime As Date?

		''' <summary>添加操作者</summary>
		<Output(TristateEnum.TRUE)>
		<MaxLength(50)>
		<DbColumn(Position:=-3)>
		<DbQuery(DynamicFilterOperator.Equal)>
		Property CreateBy As String

		''' <summary>更新时间</summary>
		<Display(Name:="UpdateTime")>
		<DbColumn(Position:=-2)>
		<DbQuery(DynamicFilterOperator.DateRange)>
		Property UpdateTime As Date?

		''' <summary>更新操作者</summary>
		<Display(Name:="UpdateBy")>
		<MaxLength(50)>
		<DbColumn(Position:=-1)>
		<DbQuery(DynamicFilterOperator.Equal)>
		Property UpdateBy As String

	End Interface

End Namespace
