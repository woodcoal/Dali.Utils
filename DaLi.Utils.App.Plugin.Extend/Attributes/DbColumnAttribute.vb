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
' 	FreeSQL Column 属性扩展
'
' 	name: Attribute.DbColumnAttribute
' 	create: 2023-02-15
' 	memo: 防止与 EF 中的 Column 冲突
'
' ------------------------------------------------------------

Imports FreeSql.DataAnnotations

Namespace Attribute

	''' <summary>FreeSQL Column 属性扩展</summary>
	Public Class DbColumnAttribute
		Inherits ColumnAttribute

	End Class
End Namespace
