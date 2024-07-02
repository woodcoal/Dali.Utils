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
' 	数据操作接口
'
' 	name: IEntityAction
' 	create: 2024-06-28
' 	memo: 数据操作接口，数据实体在插入、更新、删除时会触发的操作；基于插件，将自动加载
'
' ------------------------------------------------------------

Imports FreeSql

Namespace [Interface]
	''' <summary>数据操作接口</summary>
	Public Interface IEntityAction
		Inherits IPlugin

		''' <summary>项目操作之前的验证</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Sub ExecuteValidate(Of T As IEntity)(action As EntityActionEnum,
							entity As T,
							context As IAppContext,
							errorMessage As ErrorMessage,
							db As IFreeSql,
							Optional source As T = Nothing)


		''' <summary>项目操作完成后处理事件</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="data">单项操作时单个数值，多项时为数组</param>
		''' <param name="source">单项编辑时更新前的原始值</param>
		Sub ExecuteFinish(Of T As IEntity)(action As EntityActionEnum,
						  data As ObjectArray(Of T),
						  context As IAppContext,
						  errorMessage As ErrorMessage,
						  db As IFreeSql,
						  Optional source As T = Nothing)


		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Sub ExecuteQuery(Of T As {IEntity, Class})(action As EntityActionEnum, query As ISelect(Of T), Optional queryVM As QueryBase(Of T) = Nothing)

	End Interface
End Namespace