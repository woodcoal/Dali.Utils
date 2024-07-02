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
' 	配置数据接口
'
' 	name: Interface.IDbSetting
' 	create: 2023-02-17
' 	memo: 配置数据接口（来自 数据库，可读写）
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>配置数据接口（来自 数据库，可读写）</summary>
	Public Interface IDbSetting
		Inherits ISetting

		''' <summary>数据库模块名称</summary>
		ReadOnly Property ModuleName As String

	End Interface

End Namespace
