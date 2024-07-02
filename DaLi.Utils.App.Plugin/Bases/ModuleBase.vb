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
' 	模块启动及注入基类
'
' 	name: Base.ModuleBase
' 	create: 2023-02-14
' 	memo: 模块启动及注入基类
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection

Namespace Base

	''' <summary>模块启动及注入基类</summary>
	Public MustInherit Class ModuleBase
		Implements IPlugin

		''' <summary>排序，同类插件使用时的优先顺序</summary>
		Public Overridable ReadOnly Property Order As Integer Implements IPlugin.Order
			Get
				Return 0
			End Get
		End Property

		''' <summary>启用</summary>
		Public ReadOnly Property Enabled As Boolean = True Implements IPlugin.Enabled

		''' <summary>启动初始化操作</summary>
		Public Overridable Sub Initialize(app As ISystemBase)
		End Sub

		''' <summary>终止并结束</summary>
		Public Overridable Sub Termination(app As ISystemBase)
		End Sub

		''' <summary>配置服务时，需要注入的操作 ConfigureServices 节的操作，只会再此节最后加入</summary>
		Public Overridable Sub AddServices(services As IServiceCollection)
		End Sub

		''' <summary>配置时，需要启用的操作， Configure 节最先使用部分，注意不要进行系统性操作，仅仅方便配置而已，只会再此节最后加入</summary>
		Public Overridable Sub UseApp(app As IApplicationBuilder)
		End Sub

	End Class
End Namespace