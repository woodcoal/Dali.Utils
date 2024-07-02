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
' 	枚举
'
' 	name: Model
' 	create: 2023-02-27
' 	memo: 数据操作枚举
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>数据操作枚举</summary>
	<Flags>
	Public Enum EntityActionEnum
		''' <summary>其他</summary>
		<Description("其他")>
		NONE = 0

		''' <summary>单项</summary>
		<Description("单项")>
		ITEM = 1

		''' <summary>添加</summary>
		<Description("添加")>
		ADD = 2

		''' <summary>编辑</summary>
		<Description("编辑")>
		EDIT = 4

		''' <summary>删除</summary>
		<Description("删除")>
		DELETE = 8

		''' <summary>列表</summary>
		<Description("列表")>
		LIST = 16

		''' <summary>导出</summary>
		<Description("导出")>
		EXPORT = 32

		''' <summary>导入</summary>
		<Description("导入")>
		IMPORT = 64
	End Enum

End Namespace
