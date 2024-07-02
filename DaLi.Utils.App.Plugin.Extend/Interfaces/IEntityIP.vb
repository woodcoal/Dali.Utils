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
' 	带状态标记的数据接口
'
' 	name: Interface.IEntityIP
' 	create: 2023-02-15
' 	memo: 带状态标记的数据接口
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace [Interface]

	''' <summary>带删除标记的数据接口</summary>
	Public Interface IEntityIP
		Inherits IEntity

		''' <summary>添加 IP</summary>
		<Output(TristateEnum.TRUE)>
		<MaxLength(50)>
		<DbColumn(Position:=-1)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Property CreateIP As String

		''' <summary>更新 IP</summary>
		<Output(TristateEnum.TRUE)>
		<MaxLength(50)>
		<DbColumn(Position:=-1)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Property UpdateIP As String

	End Interface

End Namespace
