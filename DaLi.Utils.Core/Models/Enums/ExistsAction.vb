﻿' ------------------------------------------------------------
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
' 	公共参数枚举
'
' 	name: Model
' 	create: 2019-03-14
' 	memo: 存在项目的操作
' 	
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>存在项目的操作</summary> 
	Public Enum ExistsActionEnum
		<Description("覆盖")>
		OVERWRITE

		<Description("追加")>
		APPEND

		<Description("取消")>
		CANCEL

		<Description("重命名")>
		RENAME
	End Enum

End Namespace
