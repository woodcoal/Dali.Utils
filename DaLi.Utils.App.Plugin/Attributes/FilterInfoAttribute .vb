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
' 	过滤器属性
'
' 	name: Attribute.FilterInfoAttribute 
' 	create: 2023-02-14
' 	memo: 过滤器属性，过滤器注入相关参数
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>过滤器属性，过滤器注入相关参数</summary>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
	Public Class FilterInfoAttribute
		Inherits System.Attribute

		''' <summary>是否全局过滤器</summary>
		Public IsGlobal As Boolean = True

		''' <summary>注入顺序（越小越先执行）</summary>
		Public Order As Integer = 0

		''' <summary>过滤器属性，过滤器注入相关参数</summary>
		''' <param name="order">注入顺序（越小越先执行）</param>
		''' <param name="isGlobal">是否全局过滤器</param>
		Public Sub New(order As Integer, Optional isGlobal As Boolean = True)
			Me.Order = order
			Me.IsGlobal = isGlobal
		End Sub

	End Class
End Namespace