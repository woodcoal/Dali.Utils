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
' 	系统全局对象
'
' 	name: App.SystemApp
' 	create: 2023-02-14
' 	memo: 系统全局对象
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Reflection
Imports DaLi.Utils.Misc.SnowFlake
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging

Partial Public Module App

	''' <summary>系统全局对象</summary>
	Public Class SystemApp
		Implements ISystemBase

		''' <summary>LOGO</summary>
		Public Overridable ReadOnly Property Logo As String = "

	##     ## ##    ## ########  ##           ##     ## #### ########  
	##     ## ###   ## ##     ## ##           ##     ##  ##  ##     ## 
	##     ## ####  ## ##     ## ##           ##     ##  ##  ##     ## 
	######### ## ## ## ##     ## ##           ##     ##  ##  ########  
	##     ## ##  #### ##     ## ##            ##   ##   ##  ##        
	##     ## ##   ### ##     ## ##       ###   ## ##    ##  ##        
	##     ## ##    ## ########  ######## ###    ###    #### ##        

		" Implements ISystemBase.Logo

		''' <summary>应用名称</summary>
		Public Overridable ReadOnly Property Name As String Implements ISystemBase.Name
			Get
				Dim AppName = GetSetting(Of CommonSetting)()?.AppName
				Return AppName.EmptyValue(Assembly.GetEntryAssembly.Title)
			End Get
		End Property

		''' <summary>应用版本</summary>
		Public Overridable ReadOnly Property Version As String = Assembly.GetEntryAssembly.GetName.Version.ToString Implements ISystemBase.Version

		''' <summary>应用标识</summary>
		Public ReadOnly Property ID As Long Implements ISystemBase.ID

		''' <summary>启动时间</summary>
		Public ReadOnly Property Start As DateTimeOffset Implements ISystemBase.Start

		''' <summary>运行时长</summary>
		Public ReadOnly Property Runtimes As TimeSpan
			Get
				Return SYS_NOW.Subtract(Start)
			End Get
		End Property

		''' <summary>插件管理</summary>
		Public ReadOnly Property Plugins As IPluginsProvider = New PluginProvider Implements ISystemBase.Plugins

		''' <summary>多语言</summary>
		Public ReadOnly Property Localizer As ILocalizerProvider = New LocalizerProvider Implements ISystemBase.Localizer

		''' <summary>构造</summary>
		Public Sub New()
			' 启动时间
			Start = SYS_NOW

			' 生成应用唯一标识
			ID = GetID()
		End Sub

#Region "运行模式"

		''' <summary>是否调试模式</summary>
		Private _Debug As Boolean?

		''' <summary>是否调试模式</summary>
		Public ReadOnly Property Debug As Boolean Implements ISystemBase.Debug
			Get
				If _Debug Is Nothing Then _Debug = GetSetting(Of CommonSetting)()?.Debug

				Return _Debug
			End Get
		End Property

		''' <summary>是否启动完成</summary>
		Private _Started As Boolean = False

		''' <summary>是否启动完成</summary>
		Public ReadOnly Property Started As Boolean Implements ISystemBase.Started
			Get
				Return _Started
			End Get
		End Property

		''' <summary>标记启动完成，初始化完成后一定要标记</summary>
		Public Sub SetStarted()
			_Started = True

			' 清除一次信息，防止插件数据未获取完全
			_Information = Nothing
		End Sub
#End Region

#Region "目录"

		''' <summary>网站目录</summary>
		Private _FolderRoot As String

		''' <summary>网站目录</summary>
		Public ReadOnly Property FolderRoot As String Implements ISystemBase.FolderRoot
			Get
				If _FolderRoot.IsEmpty Then _FolderRoot = GetSetting(Of CommonSetting).RootFolder
				Return _FolderRoot
			End Get
		End Property

		''' <summary>数据目录</summary>
		Private _FolderData As String

		''' <summary>网站目录</summary>
		Public ReadOnly Property FolderData As String Implements ISystemBase.FolderData
			Get
				If _FolderData.IsEmpty Then _FolderData = GetSetting(Of CommonSetting).DataFolder
				Return _FolderData
			End Get
		End Property

		''' <summary>获取网站数据根目录，非网站根目录！</summary>
		''' <param name="path">根目录</param>
		''' <param name="isData">是否返回数据/页面目录（true：数据目录；false：页面目录）</param>
		Public Function Root(Optional path As String = "", Optional isData As Boolean = True) As String Implements ISystemBase.Root
			Dim Folder = If(isData, _FolderData.EmptyValue(".data"), _FolderRoot.EmptyValue(".web"))
			Folder = PathHelper.Root(Folder, True, True)

			If path.IsEmpty Then
				Return Folder
			Else
				Return IO.Path.Combine(Folder, path)
			End If
		End Function

#End Region

#Region "全局服务提供器"

		''' <summary>系统注入的实例类型，过滤系统项</summary>
		Private _ServiceTypes As ImmutableDictionary(Of Type, ServiceLifetime)

		''' <summary>系统注入的实例类型，过滤系统项</summary>
		Public ReadOnly Property ServiceTypes As ImmutableDictionary(Of Type, ServiceLifetime) Implements ISystemBase.ServiceTypes
			Get
				Return _ServiceTypes
			End Get
		End Property

		''' <summary>获取系统注入的实例，过滤系统项；从基类进行匹配</summary>
		''' <param name="lifetime">生命周期类型</param>
		Public Function GetServices(Of T)(Optional lifetime As ServiceLifetime? = Nothing) As ImmutableList(Of T) Implements ISystemBase.GetServices
			Return ServiceTypes.
				Where(Function(x) x.Key.IsComeFrom(GetType(T)) AndAlso (Not lifetime.HasValue OrElse x.Value = lifetime.Value)).
				Select(Function(x) ChangeType(Of T)(GetService(x.Key))).
				Where(Function(x) x IsNot Nothing).
				ToImmutableList
		End Function

		''' <summary>获取系统注入的实例，过滤系统项；从基类进行匹配</summary>
		''' <param name="lifetime">生命周期类型</param>
		Public Function GetServices(baseType As Type, Optional lifetime As ServiceLifetime? = Nothing) As ImmutableList(Of Object) Implements ISystemBase.GetServices
			If baseType Is Nothing Then Return Nothing

			Return ServiceTypes.
				Where(Function(x) x.Key.IsComeFrom(baseType) AndAlso (Not lifetime.HasValue OrElse x.Value = lifetime.Value)).
				Select(Function(x) GetService(x.Key)).
				Where(Function(x) x IsNot Nothing).
				ToImmutableList
		End Function

		''' <summary>设置服务描述符集合</summary>
		Public Sub SetServices(services As IServiceCollection)
			_ServiceTypes = services.
				Where(Function(x) AssemblyHelper.NameFilter(x.ServiceType.FullName, False)).
				ToImmutableDictionary(Function(x) x.ServiceType, Function(x) x.Lifetime)
		End Sub

		''' <summary>web 应用程序</summary>
		Private _Application As WebApplication

		''' <summary>设置 web 应用程序</summary>
		Public Sub SetApplication(application As WebApplication)
			_Application = application
		End Sub

		''' <summary>全局服务提供器</summary>
		Public ReadOnly Property Application As WebApplication Implements ISystemBase.Application
			Get
				If _Application Is Nothing Then Throw New Exception("尚未设置有效的 Web 应用程序，请检查系统初始化设置")
				Return _Application
			End Get
		End Property

		''' <summary>获取服务对象</summary>
		''' <remarks>尝试获取指定的对象，因为需要类型完全匹配所以有可能获取不到指定类型数据；此时通过查询所有数据，找出所有基类相同的类型然后返回第一项</remarks>
		Public Function GetService(Of T)() As T Implements ISystemBase.GetService
			Return If(Application.Services.GetService(Of T), GetServices(Of T)(ServiceLifetime.Singleton).FirstOrDefault)
		End Function

		''' <summary>获取服务对象</summary>
		''' <remarks>尝试获取指定的对象，因为需要类型完全匹配所以有可能获取不到指定类型数据；此时通过查询所有数据，找出所有基类相同的类型然后返回第一项</remarks>
		Public Function GetService(type As Type) As Object Implements ISystemBase.GetService
			Return If(Application.Services.GetService(type), GetServices(type, ServiceLifetime.Singleton).FirstOrDefault)
		End Function

		''' <summary>总线事件</summary>
		Private _Events As IEventBusProvider

		''' <summary>总线事件</summary>
		Public ReadOnly Property Events As IEventBusProvider Implements ISystemBase.Events
			Get
				If _Events Is Nothing Then _Events = GetService(Of IEventBusProvider)()
				Return _Events
			End Get
		End Property

		''' <summary>全局日志对象</summary>
		Public Function GetLOG(Of T)() As ILogger(Of T) Implements ISystemBase.GetLOG
			Return GetService(Of ILogger(Of T))()
		End Function

#End Region

#Region "全局参数管理"

		''' <summary>设置</summary>
		Public ReadOnly Property SettingProvider As ISettingProvider = New SettingProvider Implements ISystemBase.SettingProvider

		''' <summary>获取参数</summary>	
		Public Function GetSetting(Of T As ISetting)() As T Implements ISystemBase.GetSetting
			Return SettingProvider.GetSetting(Of T)
		End Function

#End Region

#Region "系统信息及标识"

		''' <summary>生成并获取应用唯一标识</summary>
		Private Function GetID() As Long
			' 尝试从 config 目录，根目录下获取 id ,根目录优先

			Dim path1 = PathHelper.Root(".id")
			Dim path2 = PathHelper.Root(".config/.id")
			Dim path = If(IO.File.Exists(path1), path1, path2)

			Dim storeName = "DaLi"
			Dim storeVersion = 22
			Dim storeKey = Assembly.GetEntryAssembly.Name

			Dim file = Store.Read(path, storeKey)
			Dim content = EncodeExtension.ToString(file.Content)
			Dim nvs = NameValueDictionary.FromString(content, False)

			Dim appID = nvs("id")?.ToLong
			If appID < 1 Then
				appID = SnowFlakeHelper.JsID

				' 记录
				nvs("id") = appID
			End If

			' 版本变化
			If nvs("version") <> Version Then
				nvs("version") = Version
				nvs("versions") &= $"[{SYS_NOW_STR}] {nvs("version")} ;"
			End If

			' 保存路径发生变化，记录历史路径
			If nvs("root") <> PathHelper.Root Then
				nvs("root") = PathHelper.Root
				nvs("roots") &= $"[{SYS_NOW_STR}] {nvs("root")} ;"
			End If

			' 系统信息
			nvs("name") = Name
			nvs("count") = nvs("count").ToLong + 1      ' 操作数 +1
			nvs("last") = nvs("update")                 ' 上次启动时间
			nvs("update") = SYS_NOW_DATE                ' 本次操作时间

			file.Content = EncodeExtension.ToBytes(nvs.ToString(False))
			Store.Save(path1, file.Content, storeKey, storeName, storeVersion)
			Store.Save(path2, file.Content, storeKey, storeName, storeVersion)

			Return appID
		End Function

		''' <summary>获取系统信息</summary>
		Private _Information As KeyValueDictionary

		''' <summary>获取系统信息</summary>
		Public ReadOnly Property Information() As KeyValueDictionary Implements ISystemBase.Information
			Get
				' 如果系统没有完全启动，也需要重新生成，因为插件可能没有更新完成
				If _Information Is Nothing Then
					Dim ass = Assembly.GetEntryAssembly
					Dim plugins = Me.Plugins.GetAssemblies.Select(Function(x) New With {x.Name, x.GetName.Version, x.Description}).OrderBy(Function(x) x.Name).ToArray
					Dim root = PathHelper.Root
					_Information = New KeyValueDictionary From {
						{"Logo", Logo},
						{"ID", ID},
						{"Name", Name},
						{"Title", ass.Title},
						{"Copyright", ass.Copyright},
						{"Company", ass.Company},
						{"Product", ass.Product},
						{"Description", ass.Description},
						{"Version", ass.Version},
						{"Start", SYS_START},
						{"Plugins", plugins},
						{"Machine", Environment.MachineName}
					}
				End If

				Return _Information

				' 部分涉及安全，暂不公开
				'{"Root", root},
				'{"Machine", New KeyValueDictionary From {
				'	{"user", Environment.UserName},
				'	{"machine", Environment.MachineName},
				'	{"os", Environment.OSVersion}
				'}}
			End Get
		End Property

#End Region

#Region "运行配置"

		''' <summary>配置系统服务</summary>
		Public Overridable Function ConfigureServices(services As IServiceCollection) As Boolean Implements ISystemBase.ConfigureServices
			Return False
		End Function

		''' <summary>配置应用</summary>
		Public Overridable Function ConfigureApp(app As IApplicationBuilder) As Boolean Implements ISystemBase.ConfigureApp
			Return False
		End Function

		''' <summary>全局结束操作</summary>
		Public Overridable Sub Close() Implements ISystemBase.Close
		End Sub

#End Region
	End Class
End Module
