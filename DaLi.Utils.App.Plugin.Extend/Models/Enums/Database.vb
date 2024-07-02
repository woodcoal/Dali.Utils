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
' 	create: 2023-02-15
' 	memo: 数据库类型
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>数据库类型</summary>
	Public Enum DatabaseTypeEnum
		''' <summary>SQL Server</summary>
		<Description("SQL Server")>
		SQLSERVER

		''' <summary>MySQL</summary>
		<Description("MySQL")>
		MYSQL

		''' <summary>PostgreSQL</summary>
		<Description("PostgreSQL")>
		POSTGRESQL

		''' <summary>内存</summary>
		<Description("内存")>
		MEMORY

		''' <summary>SQLite</summary>
		<Description("SQLite")>
		SQLITE

		''' <summary>Oracle</summary>
		<Description("Oracle")>
		ORACLE
	End Enum

End Namespace
