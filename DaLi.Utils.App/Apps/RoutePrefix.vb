' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	路由前缀
'
' 	name: App.RoutePrefix
' 	create: 2023-10-26
' 	memo: 路由前缀
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.ApplicationModels

Partial Public Module App

	''' <summary>路由前缀扩展</summary>
	<Extension()>
	Public Sub UseRoutePrefix(opts As MvcOptions, prefix As String)
		opts.Conventions.Insert(0, New RoutePrefix(prefix))
	End Sub

	''' <summary>路由前缀</summary>
	Public Class RoutePrefix
		Implements IApplicationModelConvention

		''' <summary>定义一个路由前缀变量</summary>
		Private ReadOnly _Prefix As AttributeRouteModel

		''' <summary>调用时传入指定的路由前缀</summary>
		Public Sub New(prefix As String)
			If prefix.IsEmpty Then
				_Prefix = Nothing
			Else
				_Prefix = New AttributeRouteModel(New RouteAttribute(prefix.Trim.ToLower))
			End If
		End Sub

		Public Sub Apply(application As ApplicationModel) Implements IApplicationModelConvention.Apply
			If _Prefix Is Nothing Then Return

			' 遍历所有的 Controller
			For Each ctr In application.Controllers
				' 1、已经标记了 RouteAttribute 的 Controller
				' 这一块需要注意，如果在控制器中已经标注有路由了，则会在路由的前面再添加指定的路由内容
				Dim matchedSelectors = ctr.Selectors.Where(Function(x) x.AttributeRouteModel IsNot Nothing).ToList()
				If matchedSelectors.Any() Then
					For Each selectorModel In matchedSelectors
						' 在 当前路由上 再 添加一个 路由前缀
						selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_Prefix, selectorModel.AttributeRouteModel)
					Next
				End If

				'2、 没有标记 RouteAttribute 的 Controller
				Dim unmatchedSelectors = ctr.Selectors.Where(Function(x) x.AttributeRouteModel Is Nothing).ToList()
				If unmatchedSelectors.Any() Then
					For Each selectorModel In unmatchedSelectors
						' 添加一个 路由前缀
						selectorModel.AttributeRouteModel = _Prefix
					Next
				End If
			Next
		End Sub
	End Class

End Module