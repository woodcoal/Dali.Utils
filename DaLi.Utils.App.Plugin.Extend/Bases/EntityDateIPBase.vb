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
' 	带IP时间数据模型基类
'
' 	name: Base.EntityDateIPBase
' 	create: 2023-02-25
' 	memo: 带IP时间数据模型基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace Base

	''' <summary>带IP时间数据模型基类</summary>
	Public MustInherit Class EntityDateIPBase
		Inherits EntityDateBase
		Implements IEntityIP

		''' <summary>添加 IP</summary>
		<Output(TristateEnum.TRUE)>
		<MaxLength(50)>
		<DbColumn(Position:=-4)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property CreateIP As String Implements IEntityIP.CreateIP

		''' <summary>更新 IP</summary>
		<Output(TristateEnum.TRUE)>
		<MaxLength(50)>
		<DbColumn(Position:=-2)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property UpdateIP As String Implements IEntityIP.UpdateIP

	End Class

End Namespace