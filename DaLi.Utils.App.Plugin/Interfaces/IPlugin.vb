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
' 	插件接口
'
' 	name: Interface.IPlugin
' 	create: 2023-02-14
' 	memo: 插件接口，所有插件都必须基于此接口才可自动加载
'		  如果需要后续的模块能自动加载，则需要对应加载的接口都基于此接口基类，否则无法自动分析加载
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>插件接口</summary>
	Public Interface IPlugin
		Inherits IBase

		''' <summary>排序，同类插件使用时的优先顺序</summary>
		ReadOnly Property Order As Integer

		''' <summary>是否启用</summary>
		ReadOnly Property Enabled As Boolean

	End Interface
End Namespace
