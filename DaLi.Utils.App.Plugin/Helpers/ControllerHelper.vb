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
' 	控制器操作
'
' 	name: Helper.ControllerHelper
' 	create: 2023-02-28
' 	memo: 控制器操作
'
' ------------------------------------------------------------

Imports System.Reflection
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.Abstractions
Imports Microsoft.AspNetCore.Mvc.Controllers

Namespace Helper

	''' <summary>控制器操作</summary>
	Public NotInheritable Class ControllerHelper

		''' <summary>获取路由</summary>
		''' <param name="controller">控制器</param>
		Public Shared Function GetRoutes(controller As Type) As List(Of String)
			Dim routes As New List(Of String)

			' 路由属性
			Dim attrRoutes = controller.GetCustomAttributes(Of RouteAttribute)(True)
			If attrRoutes.NotEmpty Then
				For Each attrRoute In attrRoutes
					Dim route = GetRoute(attrRoute, controller)
					If route.NotEmpty AndAlso Not routes.Contains(route) Then routes.Add(route)
				Next
			End If

			Return routes
		End Function

		''' <summary>获取路由</summary>
		''' <param name="controller">控制器</param>
		Public Shared Function GetRoute(controller As Type) As String
			Dim attrRoute = controller.GetCustomAttributes(Of RouteAttribute)(True).FirstOrDefault
			Return GetRoute(attrRoute, controller)
		End Function

		''' <summary>获取路由</summary>
		''' <param name="attrRoute">路由器属性</param>
		Public Shared Function GetRoute(attrRoute As RouteAttribute, controller As Type) As String
			Dim route = attrRoute?.Template
			If route.IsEmpty Then Return ""

			' 关键词替换
			Dim ctrName = controller.Name.Replace("controller", "", StringComparison.OrdinalIgnoreCase)
			route = route.Replace("[controller]", ctrName, StringComparison.OrdinalIgnoreCase)

			Dim attrArea = controller.GetCustomAttribute(Of AreaAttribute)(True)
			If attrArea IsNot Nothing Then route = route.Replace("[area]", attrArea.RouteValue, StringComparison.OrdinalIgnoreCase)

			route = ClearRoute(route)

			Return route
		End Function

		'''' <summary>获取路由前缀地址</summary>
		'Public Shared Function UpdateRoutePrefix(attrRoute As RouteAttribute) As String
		'	Dim route = attrRoute?.Template

		'	Dim prefix = SYS.GetSetting(Of ICommonSetting).RoutePrefix
		'	If prefix.NotEmpty Then
		'		If attrRoute IsNot Nothing Then
		'			Route = AttributeRouteModel.CombineAttributeRouteModel(New AttributeRouteModel(New RouteAttribute(prefix)), New AttributeRouteModel(attrRoute)).Template
		'		Else
		'			Route = New RouteAttribute(prefix).Template
		'		End If
		'	End If

		'	Return route
		'End Function

		''' <summary>清理路由中参数的属性</summary>
		Public Shared Function ClearRoute(route As String) As String
			If route.IsEmpty Then Return ""

			' 移除路由参数中的限制条件，如：{id:guid}  =>  {id}
			Dim ps As String() = route.Cut("{", "}", True, True)
			If ps.NotEmpty Then
				For Each p In ps
					If p.Contains(":"c) Then
						Dim cp = String.Concat(p.AsSpan(0, p.IndexOf(":"c)), "}")
						route = route.Replace(p, cp)
					End If
				Next
			End If

			route = route.Replace("//", "/")
			If route.EndsWith("/") Then route = route.Substring(0, route.Length - 1)

			Return route
		End Function

		''' <summary>获取控制器指定的属性</summary>
		Public Shared Function GetAttribute(Of T As System.Attribute)(action As ActionDescriptor, Optional inherit As Boolean = True) As T
			Dim actionDescriptor = TryCast(action, ControllerActionDescriptor)
			If actionDescriptor Is Nothing Then Return Nothing

			' 从过程分析属性
			Dim attr = actionDescriptor.MethodInfo.GetCustomAttribute(Of T)(inherit)
			If attr IsNot Nothing Then Return attr

			' 从控制器分析属性
			Return actionDescriptor.ControllerTypeInfo.GetCustomAttribute(Of T)(inherit)
		End Function

		''' <summary>获取控制器指定的属性</summary>
		Public Shared Function GetAttributes(Of T As System.Attribute)(action As ActionDescriptor, Optional inherit As Boolean = True) As IEnumerable(Of T)
			Dim actionDescriptor = TryCast(action, ControllerActionDescriptor)
			If actionDescriptor Is Nothing Then Return Nothing

			' 从过程分析属性
			Dim attr = actionDescriptor.MethodInfo.GetCustomAttributes(Of T)(inherit)
			If attr IsNot Nothing Then Return attr

			' 从控制器分析属性
			Return actionDescriptor.ControllerTypeInfo.GetCustomAttributes(Of T)(inherit)
		End Function

	End Class
End Namespace
