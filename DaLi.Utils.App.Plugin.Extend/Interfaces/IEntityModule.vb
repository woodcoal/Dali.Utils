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
' 	带模块信息数据接口
'
' 	name: Interface.IEntityModule
' 	create: 2023-02-15
' 	memo: 带模块信息数据接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>带模块信息数据接口</summary>
	Public Interface IEntityModule
		Inherits IEntity

		''' <summary>操作模块标识</summary>
		Property ModuleId As UInteger?

		''' <summary>操作模块数据标识</summary>
		Property ModuleValue As Long?

	End Interface

End Namespace
