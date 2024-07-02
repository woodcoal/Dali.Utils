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
' 	带状态标记的数据接口
'
' 	name: Interface.IEntityFlag
' 	create: 2023-02-15
' 	memo: 带状态标记的数据接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>带删除标记的数据接口</summary>
	Public Interface IEntityFlag
		Inherits IEntity

		''' <summary>状态标记，自定义，原则上 0 为删除（隐藏）</summary>
		<DbColumn(Position:=-1)>
		Property Flag As Integer

	End Interface

End Namespace
