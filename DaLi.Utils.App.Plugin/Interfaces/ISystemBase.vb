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
' 	全局应用接口
'
' 	name: Interface.ISystemBase
' 	create: 2023-02-24
' 	memo: 全局应用接口
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging

Namespace [Interface]
	''' <summary>全局应用接口</summary>
	Public Interface ISystemBase

		''' <summary>LOGO</summary>
		ReadOnly Property Logo As String

		''' <summary>应用名称</summary>
		ReadOnly Property Name As String

		''' <summary>应用版本</summary>
		ReadOnly Property Version As String

		''' <summary>应用标识</summary>
		ReadOnly Property ID As Long

		''' <summary>启动时间</summary>
		ReadOnly Property Start As DateTimeOffset

		''' <summary>程序集</summary>
		ReadOnly Property Plugins As IPluginsProvider

		''' <summary>多语言</summary>
		ReadOnly Property Localizer As ILocalizerProvider

		''' <summary>是否调试模式</summary>
		ReadOnly Property Debug As Boolean

		''' <summary>获取系统信息</summary>
		ReadOnly Property Information() As KeyValueDictionary

		''' <summary>系统已经完全启动</summary>
		ReadOnly Property Started As Boolean

#Region "目录"

		''' <summary>网站目录</summary>
		ReadOnly Property FolderRoot As String

		''' <summary>数据目录</summary>
		ReadOnly Property FolderData As String

		''' <summary>获取网站数据根目录，非网站根目录！</summary>
		''' <param name="path">根目录</param>
		''' <param name="isData">是否返回数据/页面目录（true：数据目录；false：页面目录）</param>
		Function Root(Optional path As String = "", Optional isData As Boolean = True) As String

#End Region

#Region "全局服务提供器"

		''' <summary>系统注入的实例</summary>
		ReadOnly Property ServiceTypes As ImmutableDictionary(Of Type, ServiceLifetime)

		''' <summary>获取系统注入的实例，过滤系统项；从基类进行匹配</summary>
		''' <param name="lifetime">生命周期类型</param>
		Function GetServices(Of T)(Optional lifetime As ServiceLifetime? = Nothing) As ImmutableList(Of T)

		''' <summary>获取系统注入的实例，过滤系统项；从基类进行匹配</summary>
		''' <param name="lifetime">生命周期类型</param>
		Function GetServices(baseType As Type, Optional lifetime As ServiceLifetime? = Nothing) As ImmutableList(Of Object)

		''' <summary>全局服务提供器</summary>
		ReadOnly Property Application As WebApplication

		''' <summary>获取服务对象</summary>
		''' <remarks>尝试获取指定的对象，因为需要类型完全匹配所以有可能获取不到指定类型数据；此时通过查询所有数据，找出所有基类相同的类型然后返回第一项</remarks>
		Function GetService(Of T)() As T

		''' <summary>获取服务对象</summary>
		''' <remarks>尝试获取指定的对象，因为需要类型完全匹配所以有可能获取不到指定类型数据；此时通过查询所有数据，找出所有基类相同的类型然后返回第一项</remarks>
		Function GetService(type As Type) As Object

		''' <summary>总线事件</summary>
		ReadOnly Property Events As IEventBusProvider

		''' <summary>全局日志对象</summary>
		Function GetLOG(Of T)() As ILogger(Of T)

#End Region

#Region "全局参数管理"

		''' <summary>全局参数管理器</summary>
		ReadOnly Property SettingProvider As ISettingProvider

		''' <summary>获取参数</summary>	
		Function GetSetting(Of T As ISetting)() As T

#End Region

#Region "运行配置"

		''' <summary>配置系统服务</summary>
		Function ConfigureServices(services As IServiceCollection) As Boolean

		''' <summary>配置应用</summary>
		Function ConfigureApp(app As IApplicationBuilder) As Boolean

		''' <summary>全局结束操作</summary>
		Sub Close()

#End Region

	End Interface
End Namespace