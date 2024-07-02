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
' 	扩展入口
'
' 	name: Extend
' 	create: 2023-02-17
' 	memo: 扩展入口
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Caching.Distributed
Imports Microsoft.Extensions.DependencyInjection

''' <summary>扩展入口</summary>
Public Class Extend
	Inherits ModuleBase

	''' <summary>启动项目</summary>
	Public Shared Sub Start(Optional systemApp As SystemExtend = Nothing, Optional args() As String = Nothing)
		systemApp = If(systemApp, New SystemExtend)
		Call App.Start(systemApp, args)
	End Sub

	Public Class SystemExtend
		Inherits SystemApp
		Implements ISystemExtendBase

		Public Overrides Function ConfigureServices(services As IServiceCollection) As Boolean
			' 自动同步结构是否开启，调试模式下，默认开启
			Dim isLock = LockHelper.GetLock("DB_INIT")
			Dim AutoSyncStructure = Not isLock OrElse SYS.Debug
			LockHelper.SetLock("DB_INIT", "数据库结构自动同步功能锁定，仅初始化系统数据库时才使用。", True)

			' 数据库连接管理器
			' 自动同步实体结构到数据库，仅调试模式有效
			Dim dbProvider = New DatabaseProvider(Sub(fsb) fsb.UseAutoSyncStructure(AutoSyncStructure))

			' 提示为调试模式
			If AutoSyncStructure Then CON.Err("当前数据库结构自动更新已经开启，如数据库异常，请检查此开关！")

			' 注入默认数据库驱动
			services.AddSingleton(Of IDatabaseProvider)(dbProvider)

			' 注入默认数据库连接
			services.AddSingleton(dbProvider.Default)

			' 注入数据仓库过滤器
			services.AddFreeRepository(Sub(filter) filter.Apply(Of IEntityFlag)("SoftDelete", Function(x) x.Flag > 0))

			' 注入模块标识操作
			services.AddSingleton(Of AppModuleProvider)

			' 注入审核标识操作
			services.AddSingleton(Of AppAuditProvider)

			' 注入字典数据操作
			services.AddSingleton(Of AppDictionaryProvider)

			' 注入黑名单关键词
			services.AddSingleton(Of BadKeywordProvider)

			' 当前请求的上下文
			services.AddScoped(AddressOf GetAppContext)

			Return MyBase.ConfigureServices(services)
		End Function

		Public Overrides Function ConfigureApp(app As IApplicationBuilder) As Boolean
			' 加载数据库设置
			SYS.SettingProvider.Load(SYS.Plugins.GetInstances(Of IDbSetting))

			' 重载所有数据驱动
			SYS.GetServices(Of ProviderBase)?.ForEach(Sub(pro) pro.Reload())

			Return MyBase.ConfigureApp(app)
		End Function

		''' <summary>创建应用上下文</summary>
		Public Overridable Function GetAppContext(services As IServiceProvider) As IAppContext Implements ISystemExtendBase.GetAppContext
			Dim http = services.GetService(Of IHttpContextAccessor)
			Dim db = services.GetService(Of IDatabaseProvider)
			Dim cache = services.GetService(Of IDistributedCache)
			Dim loc = services.GetService(Of ILocalizerProvider)
			Return New ExtendContext(http, db, cache, loc)
		End Function
	End Class

	''' <summary>启动初始化操作</summary>
	Public Overrides Sub Initialize(app As ISystemBase)
		' 初始化字典数据
		Dim dic = SYS.GetService(Of IAppDictionaryProvider)

		' 基础环境标识
		dic.DictionaryExist(VAR_DICTIONARY_SYSTEM_KEY,
							Sub()
								Dim CONS = $"
									0,	{VAR_DICTIONARY_SYSTEM_ID},	-1,	基础数据,	{VAR_DICTIONARY_SYSTEM_KEY}
									{VAR_DICTIONARY_SYSTEM_ID},	{VAR_DICTIONARY_DEV_ID},	999999,	应用设备,	{VAR_DICTIONARY_DEV_KEY}
									{VAR_DICTIONARY_DEV_ID},	{VAR_DICTIONARY_DEVALL_ID},	0,	不限,	{VAR_DICTIONARY_DEVALL_KEY}
								"
								CONS.Replace(vbTab, "").
									 SplitDistinct(vbCrLf).
									 ToList.
									 ForEach(Sub(item)
												 If item.NotEmpty Then
													 Dim data = item.Split(","c)
													 dic.DictionaryInsert(data(0), data(1), data(3), data(2), data(4))
												 End If
											 End Sub)
							End Sub)

		' 记录设备信息
		DevHelper.Register(SYS.ID, $"{SYS.Name}({Environment.MachineName})")
		'dic.DictionaryInsertValues(VAR_DICTIONARY_DEV_ID, New Dictionary(Of Long, String) From {{SYS.ID, $"{SYS.Name}({Environment.MachineName})"}})
	End Sub

	'''' <summary>结束操作</summary>
	'Public Overrides Sub Termination(app As ISystemBase)
	'	' 禁用设备
	'	DevHelper.Unregister(SYS.ID, False)
	'End Sub

	''' <summary>启动排序，越大越先执行</summary>
	Public Overrides ReadOnly Property Order As Integer
		Get
			Return Integer.MaxValue
		End Get
	End Property

End Class
