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
' 	是否审计数据
'
' 	name: NoAuditAttribute
' 	create: 2024-01-16
' 	memo: 对于需要审核的编辑数据是否直接需要审计，默认设置则以此参数为准，否则按参数设置，请慎重添加此属性，对于用户操作是数据建议不要免审
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>是否审计数据</summary>
	''' <remarks>对于需要审核的编辑数据是否直接需要审计，默认设置则以此参数为准，否则按参数设置，请慎重添加此属性，对于用户操作是数据建议不要免审</remarks>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
	Public Class AuditAttribute
		Inherits System.Attribute

		''' <summary>是否审计</summary>
		Public Property Audit As Boolean

		Public Sub New(Optional audit As Boolean = False)
			Me.Audit = audit
		End Sub

	End Class
End Namespace
