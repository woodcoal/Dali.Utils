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
' 	不记录日志属性
'
' 	name: Attribute.NoLogAttribute
' 	create: 2023-02-14
' 	memo: 不记录日志属性
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>不记录日志属性</summary>
	<AttributeUsage(AttributeTargets.Method Or AttributeTargets.Class, AllowMultiple:=False)>
	Public Class NoLogAttribute
		Inherits System.Attribute

	End Class
End Namespace