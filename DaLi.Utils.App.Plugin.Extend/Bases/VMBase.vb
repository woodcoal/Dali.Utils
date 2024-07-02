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
' 	视图模型基类
'
' 	name: Base.VMBase
' 	create: 2023-02-17
' 	memo: 视图模型基类
'
' ------------------------------------------------------------

Imports Microsoft.Extensions.Caching.Distributed

Namespace Base

	''' <summary>试图模型基类</summary>
	Public MustInherit Class VMBase

#Region "属性"

		''' <summary>操作请求对象</summary>
		Private _AppContext As IAppContext

		''' <summary>操作请求对象</summary>
		Public Property AppContext As IAppContext
			Get
				Return _AppContext
			End Get
			Protected Set(value As IAppContext)
				_AppContext = value
			End Set
		End Property

		''' <summary>操作请求对象</summary>
		Private _Controller As CtrBase

		''' <summary>操作请求对象</summary>
		Public Property Controller As CtrBase
			Get
				Return _Controller
			End Get
			Protected Set(value As CtrBase)
				_Controller = value
			End Set
		End Property

		''' <summary>当前请求数据库名称</summary>
		Private _DbName As String

		''' <summary>当前请求数据库名称</summary>
		Public Property DbName As String
			Get
				If _DbName.IsEmpty Then _DbName = "default"
				Return _DbName
			End Get
			Set(value As String)
				_DbName = value
			End Set
		End Property

#End Region

#Region "初始化"

		Protected Sub New(Optional controller As CtrBase = Nothing, Optional dbName As String = "")
			SetContext(controller, dbName)
		End Sub

		''' <summary>设置操作请求对象</summary>
		Public Sub SetContext(controller As CtrBase, Optional dbName As String = "default")
			AppContext = controller?.AppContext
			Me.Controller = controller
			Me.DbName = dbName
		End Sub

		''' <summary>设置操作请求对象</summary>
		Public Sub SetContext(appContext As IAppContext, controller As CtrBase, Optional dbName As String = "default")
			Me.AppContext = appContext
			Me.Controller = controller
			Me.DbName = dbName
		End Sub

		''' <summary>复制视图</summary>
		Public Sub Copy(vm As VMBase)
			If vm IsNot Nothing Then
				SetContext(vm.AppContext, vm.Controller, vm.DbName)
			End If
		End Sub

#End Region

#Region "相关对象"

		''' <summary>构造时加载操作</summary>
		Public Overridable Sub Init()
		End Sub

		''' <summary>获取指定字段</summary>
		Protected ReadOnly Property Field(name As String) As Object
			Get
				If FieldExist(name) Then
					Return AppContext.Fields(name)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>是否存在指定字段</summary>
		Protected ReadOnly Property FieldExist(name As String) As Boolean
			Get
				If name.NotEmpty AndAlso AppContext.Fields.NotEmpty Then
					Return AppContext.Fields.ContainsKey(name)
				End If

				Return False
			End Get
		End Property

		''' <summary>数据库操作对象</summary>
		Private _Db As IFreeSql

		''' <summary>数据库操作对象</summary>
		Protected ReadOnly Property Db As IFreeSql
			Get
				If _Db Is Nothing Then
					_Db = AppContext.GetDb(DbName)

					If _Db Is Nothing Then Throw New Exception($"Database Connection Name {DbName} Not Exist!")
				End If

				Return _Db
			End Get
		End Property

		''' <summary>缓存</summary>
		Protected ReadOnly Property Cache As IDistributedCache
			Get
				Return AppContext.Cache
			End Get
		End Property

		''' <summary>本地化语言</summary>
		Protected ReadOnly Property Localizer(name As String, ParamArray args As Object()) As String
			Get
				Return AppContext.Localizer(name, args)
			End Get
		End Property

		''' <summary>错误消息项目</summary>
		Public ReadOnly Property ErrorMessage As ErrorMessage
			Get
				Return AppContext.ErrorMessage
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

#End Region

	End Class

End Namespace