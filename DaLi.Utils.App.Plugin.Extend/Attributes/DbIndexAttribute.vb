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
'	数据表索引属性扩展
'
' 	name: Attribute.DbIndexAttribute
' 	create: 2023-02-15
' 	memo: 防止与系统冲突
'
' ------------------------------------------------------------

Imports FreeSql.DataAnnotations

Namespace Attribute

	''' <summary>数据表索引属性扩展</summary>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=True)>
	Public Class DbIndexAttribute
		Inherits IndexAttribute

		'''' <summary>FreeSQL 索引标记扩展，已经自动加入表标识]</summary>
		'''' <param name="name">索引名，增加占位符 {TableName} 表名区分索引名 （解决 AsTable 分表 CodeFirst 导致索引名重复的问题）</param>
		'''' <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
		'''' <param name="isUnique">是否唯一</param>
		'Public Sub New(name As String, fields As String, Optional isUnique As Boolean = False)
		'	MyBase.New(name, fields, isUnique)
		'End Sub

		''' <summary>FreeSQL 索引标记扩展，简化设置，自动生成索引名称</summary>
		''' <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
		''' <param name="isUnique">是否唯一</param>
		Public Sub New(fields As String, Optional isUnique As Boolean = False)
			MyBase.New(Fields2Name(fields), fields, isUnique)
		End Sub

		''' <summary>FreeSQL 索引标记扩展，简化设置，自动生成索引名称</summary>
		''' <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
		''' <param name="isUnique">是否唯一</param>
		Public Sub New(fields As String(), Optional isUnique As Boolean = False)
			MyBase.New(Fields2Name(fields), fields.JoinString(", "), isUnique)
		End Sub

		'''' <summary>FreeSQL 索引标记扩展，简化设置，自动生成索引名称</summary>
		'''' <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
		'Public Sub New(ParamArray fields() As String)
		'	MyBase.New(Fields2Name(fields), fields.JoinString(", "), False)
		'End Sub

		''' <summary>通过字段生成索引名称</summary>
		''' <returns></returns>
		Private Shared Function Fields2Name(ParamArray fields() As String) As String
			If fields.NotEmpty Then
				Return "{TableName}_Idx_" & fields.
					JoinString("|").
					ClearSpace.
					Replace(" ", "_").
					Replace(",", "|").
					MD5(False)
			End If

			Return "{TableName}_Idx"
		End Function

	End Class
End Namespace
