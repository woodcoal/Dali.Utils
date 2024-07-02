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
' 	API 文档注入
'
' 	name: Swashbuckle
' 	create: 2023-02-16
' 	memo: API 文档注入
'
' ------------------------------------------------------------

Imports System.Reflection
Imports DaLi.Utils.App.Base
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.Controllers
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.OpenApi.Models
Imports Swashbuckle.AspNetCore.SwaggerGen

''' <summary>API 文档注入</summary>
Public Class Swashbuckle
	Inherits ModuleBase

	''' <summary>文档路径</summary>
	Private ReadOnly Property DocumentPath As String = SYS.GetSetting(Of SwashbuckleSetting).Path

	''' <summary>文档路径</summary>
	Private ReadOnly Property ControllerAssembies As List(Of Assembly) = SYS.Plugins.GetAssemblies(Of ControllerBase)

	''' <summary>配置服务时，需要注入的操作 ConfigureServices 节的操作，只会再此节最后加入</summary>
	Public Overrides Sub AddServices(services As IServiceCollection)
		If DocumentPath.NotEmpty Then
			Dim SecurityScheme As New OpenApiSecurityScheme With {
				.Description = "JWT Bearer",
				.Name = "Authorization",
				.[In] = ParameterLocation.Header,
				.Type = SecuritySchemeType.ApiKey
			}

			Dim SecurityRequirement As New OpenApiSecurityRequirement From {
				{New OpenApiSecurityScheme With {.Reference = New OpenApiReference With {.Type = ReferenceType.SecurityScheme, .Id = "Bearer"}}, Array.Empty(Of String)}
			}

			Dim Action = Sub(opt As SwaggerGenOptions)
							 For Each ass In ControllerAssembies
								 Dim Info = New OpenApiInfo With {
										.Title = ass.Title,
										.Version = ass.Version,
										.Description = ass.Description & "<br />" & ass.Copyright,
										.Contact = New OpenApiContact With {
											.Name = ass.Company,
											.Url = New Uri("http://" & ass.Company)
										}
									 }

								 opt.SwaggerDoc(ass.Name, Info)
							 Next

							 opt.AddSecurityDefinition("Bearer", SecurityScheme)
							 opt.AddSecurityRequirement(SecurityRequirement)
							 opt.DocInclusionPredicate(Function(name, apiDesc)
														   ' Action 存在，筛选，分类
														   Dim Info = TryCast(apiDesc.ActionDescriptor, ControllerActionDescriptor)
														   If Info IsNot Nothing Then
															   Return Info.ControllerTypeInfo.Assembly.GetName.Name = name
														   End If

														   Return False
													   End Function)


							 Dim XMLs = IO.Directory.GetFiles(PathHelper.Root, "*.xml")
							 If XMLs?.Length > 0 Then
								 For Each f In XMLs
									 opt.IncludeXmlComments(f)
								 Next
							 End If
						 End Sub

			services.AddSwaggerGen(Action)
		End If
	End Sub

	''' <summary>配置时，需要启用的操作， Configure 节最先使用部分，注意不要进行系统性操作，仅仅方便配置而已，只会再此节最后加入</summary>
	Public Overrides Sub UseApp(app As IApplicationBuilder)
		If DocumentPath.NotEmpty Then
			app.UseSwagger(Sub(c) c.RouteTemplate = DocumentPath & "/{documentName}/swagger.json")
			app.UseSwaggerUI(Sub(c)
								 c.RoutePrefix = DocumentPath

								 For Each Ass In ControllerAssembies
									 c.SwaggerEndpoint($"{Ass.Name}/swagger.json", Ass.Title)
								 Next
							 End Sub)
		End If
	End Sub

End Class
