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
'	数据表名称属性扩展
'
' 	name: Attribute.DbTableAttribute
' 	create: 2023-02-15
' 	memo: 可以给表加前缀，另外防止与 EF 中的 Column 冲突
'
' ------------------------------------------------------------

Imports FreeSql.DataAnnotations

Namespace Attribute

	''' <summary>数据表名称属性扩展</summary>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
	Public Class DbTableAttribute
		Inherits TableAttribute

		''' <summary>数据表自定义前缀</summary>
		Public Shared Prefix As String

		Public Sub New(name As String)
			MyBase.Name = $"{Prefix}{name}"
		End Sub
	End Class
End Namespace
