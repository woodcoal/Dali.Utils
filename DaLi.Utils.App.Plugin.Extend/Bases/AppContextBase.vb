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
' 	应用上下文基类
'
' 	name: Base.AppContextBase
' 	create: 2023-02-24
' 	memo: 应用上下文基类
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Caching.Distributed

Namespace Base

	''' <summary>应用上下文基类</summary>
	Public MustInherit Class AppContextBase
		Implements IAppContext

		Public Sub New(http As IHttpContextAccessor, dbProvider As IDatabaseProvider, cache As IDistributedCache, localizer As ILocalizerProvider)
			StartTime = SYS_NOW

			Me.Http = (http?.HttpContext)
			_DbProvider = dbProvider
			_Cache = cache
			_Localizer = localizer
		End Sub

		Public Sub New(httpContext As HttpContext)
			StartTime = SYS_NOW
			Http = httpContext
		End Sub

		''' <summary>开始执行时间</summary>
		Public ReadOnly Property StartTime As DateTimeOffset

		''' <summary>当前 HttpContext</summary>
		Public ReadOnly Property Http As HttpContext Implements IAppContext.Http

		''' <summary>数据库操作对象</summary>
		Private _DbProvider As IDatabaseProvider

		''' <summary>数据库操作对象</summary>
		Protected ReadOnly Property DbProvider As IDatabaseProvider
			Get
				If _DbProvider Is Nothing Then _DbProvider = GetService(Of IDatabaseProvider)()
				Return _DbProvider
			End Get
		End Property

		''' <summary>获取数据库驱动</summary>
		Public Function GetDb(Optional name As String = "default") As IFreeSql Implements IAppContext.GetDb
			Return DbProvider.GetDb(name)
		End Function

		''' <summary>缓存</summary>
		Private _Cache As IDistributedCache

		''' <summary>缓存</summary>
		Public ReadOnly Property Cache As IDistributedCache Implements IAppContext.Cache
			Get
				If _Cache Is Nothing Then _Cache = GetService(Of IDistributedCache)()
				Return _Cache
			End Get
		End Property

		''' <summary>本地化语言对象</summary>
		Private _Localizer As ILocalizerProvider

		''' <summary>本地化语言对象</summary>
		Public ReadOnly Property Localizer As ILocalizerProvider Implements IAppContext.Localizer
			Get
				If _Localizer Is Nothing Then _Localizer = GetService(Of ILocalizerProvider)()
				Return _Localizer
			End Get
		End Property

		''' <summary>错误消息项目</summary>
		Public ReadOnly Property ErrorMessage As ErrorMessage = New ErrorMessage Implements IAppContext.ErrorMessage

		''' <summary>请求参数列表，仅获取一维数据</summary>
		Private _Fields As KeyValueDictionary

		''' <summary>请求参数列表，仅获取一维数据</summary>
		Public ReadOnly Property Fields As KeyValueDictionary Implements IAppContext.Fields
			Get
				If _Fields Is Nothing Then
					_Fields = New KeyValueDictionary

					Dim Forms = Http.Request.FormEx
					If Forms.NotEmpty Then
						For Each Q In Forms
							_Fields.Add(Q.Key, Q.Value)
						Next
					Else
						_Fields.AddRange(Http.Request.Json)
					End If
				End If

				Return _Fields
			End Get
		End Property

		''' <summary>获取参数</summary>
		Public Function GetSetting(Of T As ISetting)() As T
			Return SYS.GetSetting(Of T)
		End Function

		''' <summary>获取注入项目</summary>
		Public Function GetService(Of T)() As T
			Return SYS.GetService(Of T)
		End Function

	End Class
End Namespace