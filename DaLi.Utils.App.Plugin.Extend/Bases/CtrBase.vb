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
' 	控制器基类
'
' 	name: Base.CtrBase
' 	create: 2023-02-17
' 	memo: 控制器基类
'
' ------------------------------------------------------------

Imports System.Reflection

Namespace Base

	''' <summary>控制器基类</summary>
	Public MustInherit Class CtrBase
		Inherits ApiControllerBase

		''' <summary>创建视图</summary>
		Protected Function CreateVM(Of V As {VMBase, New})() As V
			Dim vm As New V

			' 赋值基本数据
			UpdateVM(vm)

			Return vm
		End Function

		''' <summary>更新视图</summary>
		Protected Sub UpdateVM(vm As VMBase)
			If vm IsNot Nothing Then
				' 赋值基本数据
				vm.SetContext(Me, DbName)

				' 视图初始化操作
				vm.Init()
			End If
		End Sub

		''' <summary>错误消息项目</summary>
		Public Overrides ReadOnly Property ErrorMessage As ErrorMessage
			Get
				Return AppContext.ErrorMessage
			End Get
		End Property

#Region "操作请求对象"

		''' <summary>操作请求对象</summary>
		Private _AppContext As IAppContext

		''' <summary>操作请求对象</summary>
		Public Property AppContext As IAppContext
			Get
				If _AppContext Is Nothing Then _AppContext = HttpContext.ContextItem(Of IAppContext)(VAR_CONTROLLER_CONTEXT)
				Return _AppContext
			End Get
			Set(value As IAppContext)
				_AppContext = value
			End Set
		End Property

#End Region

#Region "数据库操作对象"

		''' <summary>当前请求数据库名称</summary>
		Private _DbName As String

		''' <summary>当前请求数据库名称</summary>
		Public ReadOnly Property DbName As String
			Get
				If _DbName.IsEmpty Then
					' 从控制器属性中获取数据分类名称
					_DbName = [GetType].GetCustomAttribute(Of DbConnectAttribute)(True)?.Name
					_DbName = _DbName.EmptyValue("default")
				End If

				Return _DbName
			End Get
		End Property

		''' <summary>数据库操作对象</summary>
		Private _Db As IFreeSql

		''' <summary>数据库操作对象</summary>
		Public ReadOnly Property Db As IFreeSql
			Get
				If _Db Is Nothing Then
					_Db = AppContext.GetDb(DbName)

					If _Db Is Nothing Then Throw New Exception($"Database Connection Name {DbName} Not Exist!")
				End If

				Return _Db
			End Get
		End Property

#End Region

	End Class

End Namespace