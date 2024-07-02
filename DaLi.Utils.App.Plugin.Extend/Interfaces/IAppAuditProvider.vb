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
' 	审计数据接口
'
' 	name: Interface.IAppAuditProvider
' 	create: 2024-01-15
' 	memo: 审计数据接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>审计数据接口</summary>
	Public Interface IAppAuditProvider

		''' <summary>对象审计</summary>
		''' <param name="fields">审计字段</param>
		''' <param name="entity">对象</param>
		''' <param name="source">原始对象，不存在则从对象中查询</param>
		''' <param name="user">操作用户，不存在则从对象中获取</param>
		Function Audit(Of T As {IEntity, Class})(fields As String(), entity As T, source As IEntity, Optional user As String = "") As IDictionary(Of String, String)

		''' <summary>数据审计</summary>
		''' <param name="entity">对象</param>
		''' <param name="source">原始对象，不存在则从对象中查询</param>
		''' <param name="user">操作用户，不存在则从对象中获取</param>
		Function Audit(Of T As {IEntity, Class})(ByRef entity As T, Optional source As IEntity = Nothing, Optional user As String = "") As IDictionary(Of String, String)

		''' <summary>批量数据审计</summary>
		''' <param name="entities">批量对象</param>
		''' <param name="user">操作用户，不存在则从对象中获取</param>
		Function Audit(Of T As {IEntity, Class})(entities As IEnumerable(Of T), Optional user As String = "") As Integer

		''' <summary>审计确认</summary>
		''' <param name="audit">审计数据实体</param>
		''' <param name="message">审计结果，通过为空，不通过输入未通过信息</param>
		''' <param name="user">操作人</param>
		Function Validate(audit As AuditEntity, Optional message As String = "", Optional user As String = "") As String
	End Interface

End Namespace