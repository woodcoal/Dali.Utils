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
' 	全局扩展应用接口
'
' 	name: Interface.ISystemExtendBase
' 	create: 2023-02-24
' 	memo: 全局扩展应用接口
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>全局扩展应用接口</summary>
	Public Interface ISystemExtendBase
		Inherits ISystemBase

		''' <summary>创建应用上下文</summary>
		Function GetAppContext(services As IServiceProvider) As IAppContext
	End Interface
End Namespace