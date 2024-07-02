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
' 	系统运行环境
'
' 	name: App.Startup
' 	create: 2023-02-14
' 	memo: 系统运行环境
'
' ------------------------------------------------------------

Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports FreeRedis
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Localization
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.ApplicationParts
Imports Microsoft.AspNetCore.Mvc.Filters
Imports Microsoft.Extensions.Caching.Distributed
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.FileProviders
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Localization
Imports Microsoft.Extensions.Logging.Abstractions
Imports Microsoft.Extensions.Options
Imports Serilog
Imports Serilog.Events

Partial Public Module App

	''' <summary>系统运行环境</summary>
	Private Class Startup

		''' <summary>执行启动</summary>
		''' <param name="args">环境参数</param>
		Public Sub Run(sys As SystemApp, Optional args() As String = Nothing)
			' 启动信息
			CON.AppStart($"{sys.Name} / {sys.ID} 启动！({SYS_NOW_STR})")

			' 编码注册，防止编码无效
			EncodingRegister()

			' 加载本地参数
			sys.SettingProvider.Load(sys.Plugins.GetInstances(Of ILocalSetting))

			' 移除禁用的插件
			sys.Plugins.Remove(sys.GetSetting(Of PluginSetting).Disabled)

			' 初始化日志对象
			Log.Logger = sys.GetSetting(Of LogSetting).CreateLogger

			' 启动
			Log.Logger.Information("正在初始化运行环境，系统即将启动! {SYS_NOW_STR}", SYS_NOW_STR)

			Try
				' 模块列表
				Dim Modules = If(sys.Plugins.GetInstances(Of ModuleBase), New List(Of ModuleBase))

				' 创建应用
				Dim builder = WebApplication.CreateBuilder(args)
				builder.Host.UseSerilog
				Dim services = builder.Services

				' 启用注入
				AddService(services, Modules)

				' 设置服务集合
				sys.SetServices(services)

				' 应用创建
				Dim app = builder.Build()

				' 全局服务驱动赋值
				sys.SetApplication(app)

				' 使用注入
				UseApp(app, Modules)

				' 自定义端口 
				If CommonSetting.Urls.NotEmpty Then
					app.Urls.Clear()

					For Each uri In CommonSetting.Urls
						app.Urls.Add(uri)
					Next
				End If

				' 生命周期注册
				RegisterLifetime(app.Lifetime, Modules, sys)

				' 注册事件
				RegisterOutEvents()

				' 启动应用
				app.Run()

				' 应用接口注销
				sys.Close()

				' 记录项目完成
				Log.Logger.Information("系统结束运行! {date}，累计运行：{long}", SYS_NOW_STR, SYS_NOW.ShowDiff(SYS_START))

			Catch ex As Exception
				Thread.Sleep(1500)

				' 系统异常，捕获并显示
				Log.Logger.Fatal(ex, "系统异常")
			End Try

			' 日志结束
			Log.CloseAndFlush()

			' 结束提示
			CON.AppFinish($"{sys.Name} ，结束！({SYS_NOW_STR})", RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		End Sub

#Region "相关操作"

		''' <summary>通用参数</summary>
		Private _CommonSetting As CommonSetting

		''' <summary>通用参数</summary>
		Private ReadOnly Property CommonSetting As CommonSetting
			Get
				If _CommonSetting Is Nothing Then _CommonSetting = SYS.GetSetting(Of CommonSetting)
				Return _CommonSetting
			End Get
		End Property

		''' <summary>输出启动消息</summary>
		Private Shared Sub ShowSuccessInfo()
			' 清除界面，输出 LOGO
			If Not SYS.Debug Then CON.Clear()

			CON.Err(SYS.Logo)
			CON.Line()
			CON.Warn($"{vbTab}应用名称： {SYS.Name} / {SYS.ID}")
			CON.Err($"{vbTab}启动时间： {SYS_NOW_STR("yyyy年MM月dd日HH时mm分")}")
			CON.Info($"{vbTab}项目目录： {SYS.Application.Environment.ContentRootPath}")
			CON.Info($"{vbTab}应用入口： {SYS.Application.Environment.ApplicationName}")
			'CON.Succ($"{vbTab}站点目录： {SYS.Application.Environment.WebRootPath}")
			CON.Succ($"{vbTab}当前地址： {SYS.Application.Urls.JoinString("；")}")
			CON.Echo()

			If SYS.Debug Then
				CON.Warn("【注意！！！】 您正以【调试模式】启动系统，如果你用于工作环境，请关闭调试模式！ 设置方式：请在 config 目录下，将 common.json 节点下的 Debug 字段设置为 false，并重启项目")
				CON.Echo()
			End If

			CON.Warn($"当前系统加载插件：{vbCrLf}{vbTab}{SYS.Plugins.GetAssemblies.OrderBy(Function(x) x.Name).Select(Function(x) x.Name & vbTab & "(" & x.GetName.Version.ToString & ")").ToArray.JoinString(vbCrLf & vbTab)}")
			CON.Echo()
			CON.Line()
		End Sub

#End Region

#Region "注册系统"

		''' <summary>注册系统信息</summary>
		Private Shared Sub RegisterSystemInfo()
			Dim redis = SYS.GetService(Of RedisClient)
			If redis Is Nothing Then Return

			StatusCacheProvider.SystemStatus.SET(SYS.ID, SYS.Information)
			Log.Information("系统信息已经注册到 Redis")
		End Sub

		''' <summary>注销系统信息</summary>
		Private Shared Sub UnregisterSystemInfo()
			StatusCacheProvider.SystemStatus.DEL(SYS.ID)
			Log.Information("系统信息已经从 Redis 注销")
		End Sub

		''' <summary>通用输出事件注册</summary>
		Private Shared Sub RegisterOutEvents()
			SYS.Events.Register(E_OUT_CONSOLE, Sub(level As LogEventLevel, message As String)
												   Select Case level
													   Case LogEventLevel.Information
														   CON.Info(message)
													   Case LogEventLevel.Warning
														   CON.Warn(message)
													   Case LogEventLevel.Error
														   CON.Err(message)
													   Case LogEventLevel.Verbose
														   CON.Succ(message)
													   Case Else
														   CON.Echo(message)
												   End Select
											   End Sub, 1000)

			SYS.Events.Register(E_OUT_CONSOLE, Sub(message As String)
												   CON.Echo(message)
											   End Sub, 1000)

			SYS.Events.Register(E_OUT_LOG, Sub(message As String) Log.Information(message), 1000)

			SYS.Events.Register(E_OUT_LOG, Sub(level As LogEventLevel, message As String) Log.Logger.Write(level, message), 1000)
		End Sub

#End Region

#Region "注入项目"

		''' <summary>注入服务</summary>
		Private Function AddService(services As IServiceCollection, modules As List(Of ModuleBase)) As IServiceCollection

#Region "System.Text.Json"

			services.AddControllers().
				AddJsonOptions(Sub(opt)
								   With opt.JsonSerializerOptions
									   .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
									   .PropertyNamingPolicy = JsonNamingPolicy.CamelCase
									   .DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
									   .PropertyNameCaseInsensitive = True

									   ' 获取或设置一个对象，该对象指定序列化或反序列化时应如何处理数字类型。
									   .NumberHandling = JsonNumberHandling.AllowReadingFromString

									   ' 获取或设置一个值，该值定义反序列化过程中如何处理注释。
									   .ReadCommentHandling = JsonCommentHandling.Skip

									   ' todo: Utils.Core.CSharp 如果需要记得启用
									   ' .Converters.Add(New Misc.JsonConverter(Function(x) x.JsonElementParse))  ' Object 反序列化时返回的类型
									   .Converters.Add(New Misc.Json.JsonObjectConverter)
								   End With
							   End Sub)
#End Region

#Region "注入缓存"

			' 内存缓存
			services.AddMemoryCache

			' 分布式缓存
			Dim redisClient = SYS.GetSetting(Of RedisSetting).CreateClient
			If redisClient Is Nothing Then
				' 未创建或者无法连接，注册内存缓存
				services.AddDistributedMemoryCache

				Log.Logger.Warning("加载 Redis 缓存失败，请检查设置")
			Else
				' 注册分布式缓存
				services.AddSingleton(redisClient)
				services.AddSingleton(Of IDistributedCache)(New DistributedCache(redisClient))

				Log.Logger.Debug("加载 Redis 缓存成功")
			End If

#End Region

			' 注入 GET 缓存
			services.AddResponseCaching

			' 注入 HttpContextAccessor
			services.AddHttpContextAccessor

			' 注入 HttpClient
			services.AddHttpClient

			' 注入缓存 HTTP 响应
			services.AddResponseCompression

#Region "注入 Session"

			services.AddSession(Sub(options)
									options.Cookie.Name = CommonSetting.CookiePrefix & ".Session"
									options.Cookie.SameSite = SameSiteMode.None
									options.IdleTimeout = TimeSpan.FromSeconds(CommonSetting.CookieTimeout)
								End Sub)
#End Region

#Region "注入 跨域"

			Dim SettingCors = SYS.GetSetting(Of CorsSetting)

			If SettingCors.Enable Then
				services.AddCors(Sub(options)
									 If SettingCors.Policies.NotEmpty Then
										 For Each nv In SettingCors.Policies
											 Dim hosts = nv.Value.SplitDistinct
											 options.AddPolicy(nv.Key, Sub(builder)
																		   builder.WithOrigins(hosts)
																		   builder.AllowAnyHeader()
																		   builder.AllowAnyMethod()
																		   builder.AllowCredentials()
																	   End Sub)

										 Next
									 Else
										 options.AddPolicy("default", Sub(builder)
																		  builder.SetIsOriginAllowed(Function(x) True)
																		  builder.AllowAnyHeader()
																		  builder.AllowAnyMethod()
																		  builder.AllowCredentials()
																	  End Sub)

									 End If
								 End Sub)
			End If

#End Region

#Region "注入 多语言"

			' 注入本地化语言
			services.AddSingleton(SYS.Localizer)

			If CommonSetting.Languages.NotEmpty Then
				Dim list = CommonSetting.Languages.Where(Function(x) x.NotEmpty).Select(Function(x) New CultureInfo(x)).ToList
				If list.IsEmpty Then Return services

				services.AddLocalization(Sub(options) options.ResourcesPath = "Resources")
				services.Configure(Of RequestLocalizationOptions)(Sub(options)
																	  options.DefaultRequestCulture = New RequestCulture(list(0))
																	  options.SupportedCultures = list
																	  options.SupportedUICultures = list
																  End Sub)
			End If


			' 本地语言接口
			Dim localizationOptions = Options.Create(New LocalizationOptions With {.ResourcesPath = ""})
			Dim resourceManager = New ResourceManagerStringLocalizerFactory(localizationOptions, NullLoggerFactory.Instance)
			Dim stringLocalizer = resourceManager.Create("Resource", GetType(App).Assembly.Name)

			SYS.Localizer.SetLocalizer(stringLocalizer)

#End Region

#Region "注入 API 文档（已经移入插件）"

			'			If CommonSetting.DocumentPrefix.NotEmpty Then
			'				Dim SecurityScheme As New OpenApiSecurityScheme With {
			'					.Description = "JWT Bearer",
			'					.Name = "Authorization",
			'					.[In] = ParameterLocation.Header,
			'					.Type = SecuritySchemeType.ApiKey
			'				}

			'				Dim SecurityRequirement As New OpenApiSecurityRequirement From {
			'					{New OpenApiSecurityScheme With {.Reference = New OpenApiReference With {.Type = ReferenceType.SecurityScheme, .Id = "Bearer"}}, Array.Empty(Of String)}
			'				}

			'				Dim Action = Sub(opt As SwaggerGenOptions)
			'								 For Each ass In ControllerAssembies
			'									 Dim Info = New OpenApiInfo With {
			'											.Title = ass.Title,
			'											.Version = ass.Version,
			'											.Description = ass.Description & "<br />" & ass.Copyright,
			'											.Contact = New OpenApiContact With {
			'												.Name = ass.Company,
			'												.Url = New Uri("http://" & ass.Company)
			'											}
			'										 }

			'									 opt.SwaggerDoc(ass.Name, Info)
			'								 Next

			'								 opt.AddSecurityDefinition("Bearer", SecurityScheme)
			'								 opt.AddSecurityRequirement(SecurityRequirement)
			'								 opt.DocInclusionPredicate(Function(name, apiDesc)
			'															   ' Action 存在，筛选，分类
			'															   Dim Info = TryCast(apiDesc.ActionDescriptor, ControllerActionDescriptor)
			'															   If Info IsNot Nothing Then
			'																   Return Info.ControllerTypeInfo.Assembly.GetName.Name = name
			'															   End If

			'															   Return False
			'														   End Function)

			'								 Dim XMLs = IO.Directory.GetFiles(PathHelper.Root, "*.xml")
			'								 If XMLs?.Length > 0 Then
			'									 For Each f In XMLs
			'										 opt.IncludeXmlComments(f)
			'									 Next
			'								 End If
			'							 End Sub

			'				services.AddSwaggerGen(Action)
			'			End If
#End Region

#Region "手工注入 MVC"
			' 过滤器
			Dim fileters = SYS.Plugins.
				GetTypes(Of ActionFilterAttribute).
				Select(Function(type)
						   Dim attr = type.GetCustomAttribute(Of FilterInfoAttribute)
						   If attr Is Nothing Then attr = New FilterInfoAttribute(0)
						   Return (type, attr.Order, attr.IsGlobal)
					   End Function).
				ToList

			' 非全局过滤器注入
			fileters.Where(Function(x) Not x.IsGlobal).ToList.ForEach(Sub(x) services.AddScoped(x.type))

			' 注入相关服务，如果注入服务返回 False 则表示未处理 MVC 控件
			' 需要手工注入 MVC
			If Not SYS.ConfigureServices(services) Then
				Dim iMvc = services.AddMvc(Sub(options)
											   ' 全局过滤器注入
											   fileters.Where(Function(x) x.IsGlobal).ToList.ForEach(Sub(x) options.Filters.Add(x.type, x.Order))

											   ' 此应用程序允许的最大验证错误数
											   options.MaxModelValidationErrors = 15

											   ' 使用路由器前缀
											   If CommonSetting.RoutePrefix.NotEmpty Then options.UseRoutePrefix(CommonSetting.RoutePrefix)
										   End Sub)

				iMvc.AddDataAnnotationsLocalization
				iMvc.ConfigureApiBehaviorOptions(Sub(options)
													 options.SuppressModelStateInvalidFilter = True

													 ' 对象验证失败时执行的操作
													 options.InvalidModelStateResponseFactory = Function(context)
																									If context.ModelState.IsValid Then Return New OkResult

																									' 错误对象集
																									Dim errorMessage As New NameValueDictionary

																									context.ModelState.
																									Where(Function(x) x.Value.Errors.Count > 0).
																									ToList.
																									ForEach(Sub(x) errorMessage.Add(x.Key, x.Value.Errors.FirstOrDefault.ErrorMessage))

																									Return ResponseJson.Default(400, errorMessage, context.HttpContext)
																								End Function
												 End Sub)

				iMvc.ConfigureApplicationPartManager(Sub(appPartsManager)
														 Dim ass = SYS.Plugins.GetAssemblies(Of ControllerBase)(Function(type)
																													' 环境状态
																													Dim attr = type.GetCustomAttribute(Of EnvAttribute)
																													Return attr Is Nothing OrElse attr.IsRuntime(SYS.Debug)
																												End Function)
														 If ass.NotEmpty Then
															 For Each a In ass
																 appPartsManager.ApplicationParts.Add(New AssemblyPart(a))
															 Next
														 End If
													 End Sub)
			End If
#End Region

			' 注入模块组件
			modules.ForEach(Sub(m) m.AddServices(services))

#Region "注入后台服务"

			' 总线事件注入
			services.AddSingleton(Of Provider.EventBusProvider)

			' 自动重载数据服务
			'services.AddSingleton(Of ReloaderService)

			' 注入后台服务，强制启用，存在守护进程，必须开启
			services.AddHostedService(Of BackServiceProvider)
#End Region

			Return services
		End Function

		''' <summary>使用服务</summary>
		Private Function UseApp(app As IApplicationBuilder, modules As List(Of ModuleBase)) As IApplicationBuilder
			' 解决 Request Body 获取不都内容的问题
			' 缓存 Body 数据
			app.Use(Async Function(context, nextAction)
						' 启用缓存，以便获取 Request.Body 的内容
						context.Request.EnableBuffering

						Dim exp = ""
						Try
							Await nextAction.Invoke()
						Catch ex As Exception
							exp = $"服务器内部错误，请求编号：{context.TraceIdentifier} / {ex.Message}"
							Log.Error(ex, "服务器内部错误，请求编号：{RequestId}", context.TraceIdentifier)
						End Try

						'发生异常
						If exp.NotEmpty Then
							Dim response As HttpResponse = context.Response
							response.StatusCode = 500
							response.ContentType = "application/json"

							Await context.Response.WriteAsync(New ResponseJson(500, exp, context).ToJson(True, True))
						End If
					End Function)

#Region "中间件顺序"

			' https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0

			'' Configure the HTTP request pipeline.
			'If app.Environment.IsDevelopment Then
			'	app.UseMigrationsEndPoint
			'Else
			'	app.UseExceptionHandler("/Error")
			'	' The default HSTS value Is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			'	app.UseHsts()
			'End If

			'app.UseHttpsRedirection
			'app.UseCookiePolicy

			'app.UseRouting
			'app.UseRequestLocalization
			'app.UseCors

			'app.UseAuthentication
			'app.UseAuthorization
			'app.UseSession
			'app.UseResponseCaching
			'app.UseResponseCompression
			'app.UseStaticFiles

			'app.MapRazorPages
			'app.MapControllerRoute
#End Region

			' 调试模式显示具体错误信息
			If SYS.Debug Then app.UseDeveloperExceptionPage

#Region "使用静态目录"
			' 使用静态目录
			' 使用静态文件必须放在 useMvc 前面
			If CommonSetting.UseStatic Then
				app.UseFileServer(New FileServerOptions With {
													   .EnableDefaultFiles = True,
													   .EnableDirectoryBrowsing = False,
													   .RequestPath = New PathString(""),
													   .FileProvider = New PhysicalFileProvider(SYS.Root(, False))
													})

				If CommonSetting.StaticFolder.NotEmpty Then
					For Each Folder In CommonSetting.StaticFolder
						If Folder.Key.NotEmpty AndAlso Folder.Value.NotEmpty Then
							Dim path = Folder.Key
							If Not path.StartsWith("/") Then path = "/" & path

							Dim dir = Folder.Value
							If PathHelper.FolderExist(dir) Then
								app.UseFileServer(New FileServerOptions With {
														   .EnableDefaultFiles = True,
														   .EnableDirectoryBrowsing = False,
														   .RequestPath = New PathString(path),
														   .FileProvider = New PhysicalFileProvider(dir)
														})
							End If
						End If
					Next
				End If
			End If
#End Region

			' 添加 Serilog 日志请求数据
			If SYS.GetSetting(Of LogSetting).Http > 0 Then
				app.UseSerilogRequestLogging(Sub(opt)
												 opt.GetLevel = AddressOf Filter.FrameworkFilter.SerilogLevel
												 opt.EnrichDiagnosticContext = AddressOf Filter.FrameworkFilter.SerilogContent
											 End Sub)
			End If

			' 添加 Cookie 隐私要求
			app.UseCookiePolicy

			' 添加路由
			app.UseRouting

#Region "添加跨域"
			Dim SettingCors = SYS.GetSetting(Of CorsSetting)

			If SettingCors.Enable Then
				If SettingCors.Policies.NotEmpty Then
					app.UseCors(SettingCors.Policies.FirstOrDefault.Key)
				Else
					app.UseCors("default")
				End If
			End If
#End Region

			' 添加认证
			app.UseAuthentication

			' 添加授权
			app.UseAuthorization

			' 添加 Session
			app.UseSession

			' 添加缓存 HTTP 响应
			app.UseResponseCaching

			' 添加 Gzip / Br 压缩
			app.UseResponseCompression

#Region "注入 API 文档（已经移入插件）"
			'			If CommonSetting.DocumentPrefix.NotEmpty Then
			'				app.UseSwagger(Sub(c) c.RouteTemplate = CommonSetting.DocumentPrefix & "/{documentName}/swagger.json")
			'				app.UseSwaggerUI(Sub(c)
			'									 c.RoutePrefix = CommonSetting.DocumentPrefix

			'									 For Each Ass In ControllerAssembies
			'										 c.SwaggerEndpoint($"{Ass.Name}/swagger.json", Ass.Title)
			'									 Next
			'								 End Sub)

			'			End If
#End Region

#Region "添加多语言"
			If CommonSetting.Languages.NotEmpty Then
				Dim list = CommonSetting.Languages.Where(Function(x) x.NotEmpty).Select(Function(x) New CultureInfo(x)).ToList
				If list.IsEmpty Then Return app

				app.UseRequestLocalization(New RequestLocalizationOptions With {
										.DefaultRequestCulture = New RequestCulture(list(0)),
										.SupportedCultures = list,
										.SupportedUICultures = list
									})

				Thread.CurrentThread.CurrentCulture = list(0)
				Thread.CurrentThread.CurrentUICulture = list(0)
			End If
#End Region

			' 启用注入
			modules.ForEach(Sub(m) m.UseApp(app))

			' 注入相关服务，如果返回 False 表示未配置路由
			' 需要手工配置路由
			If Not SYS.ConfigureApp(app) Then
				app.UseEndpoints(Sub(endpoints) endpoints.MapDefaultControllerRoute)
			End If

			' 更新参数事件
			SYS.Events.Register(E_SETTING_UPDATE, Sub(typeName As String) SYS.SettingProvider.Reload(typeName))

			Return app
		End Function

		''' <summary>生命周期事件注册</summary>
		Private Function RegisterLifetime(lifetime As IHostApplicationLifetime, modules As List(Of ModuleBase), sys As SystemApp) As IHostApplicationLifetime
			' 启动结束加载参数
			With lifetime
				.ApplicationStarted.Register(Sub()
												 ' 注册系统信息
												 Call RegisterSystemInfo()

												 ' 应用启动
												 modules.ForEach(Sub(m) m.Initialize(sys))

												 ' 显示启动信息
												 Call ShowSuccessInfo()

												 ' 标记启动完成
												 sys.SetStarted()
											 End Sub)

				.ApplicationStopped.Register(Sub()
												 ' 注销系统信息
												 Call UnregisterSYStemInfo()

												 ' 应用结束，需要反向排序
												 modules.OrderBy(Function(x) x.Order).ToList.ForEach(Sub(x) x.Termination(sys))
											 End Sub, False)
			End With

			Return lifetime
		End Function

#End Region

	End Class
End Module