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
'	是否允许搜索属性
'
' 	name: Attribute.DbQueryAttribute
' 	create: 2023-02-22
' 	memo: 是否允许搜索属性
'
' ------------------------------------------------------------

Imports FreeSql.Internal.Model

Namespace Attribute

	''' <summary>是否允许搜索属性</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=True, Inherited:=True)>
	Public Class DbQueryAttribute
		Inherits System.Attribute

		''' <summary>当前字段是否允许被搜索(常用字段：parent dictionary createtime/createby updatetime/updateby 无需设置，默认开头搜索)</summary>
		Public Property QueryEnabled As Boolean

		''' <summary>搜索的字段名称，如果与当前属性名一致则无需设置</summary>
		Public QueryName As String

		''' <summary>匹配方式</summary>
		Public QueryOperator As DynamicFilterOperator

		Public Sub New(queryEnabled As Boolean, Optional queryOperator As DynamicFilterOperator = DynamicFilterOperator.Equal, Optional queryName As String = Nothing)
			Me.QueryEnabled = queryEnabled
			Me.QueryOperator = queryOperator
			Me.QueryName = queryName
		End Sub

		Public Sub New(Optional queryOperator As DynamicFilterOperator = DynamicFilterOperator.Equal, Optional queryName As String = Nothing)
			QueryEnabled = True
			Me.QueryOperator = queryOperator
			Me.QueryName = queryName
		End Sub
	End Class
End Namespace
