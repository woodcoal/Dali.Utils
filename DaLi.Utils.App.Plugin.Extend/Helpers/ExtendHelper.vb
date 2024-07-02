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
' 	扩展操作
'
' 	name: Helper.ExtendHelper
' 	create: 2023-02-21
' 	memo: 扩展操作
'
' ------------------------------------------------------------

Imports System.Reflection

Namespace Helper
	''' <summary>扩展操作</summary>
	Public NotInheritable Class ExtendHelper

#Region "字典字段检查"

		''' <summary>获取字典字段，非 IEnumerable(Of String) 基类 Dictionary 则字段无效</summary>
		Public Shared Function GetDictionarProperty(Of T As {IEntity, Class})(entity As T) As PropertyInfo
			If entity Is Nothing Then Return Nothing

			Return GetDictionarProperty(entity.GetType)
		End Function

		''' <summary>获取字典字段，非 IEnumerable(Of String) 基类 Dictionary 则字段无效</summary>
		Public Shared Function GetDictionarProperty(Of T As {IEntity, Class})() As PropertyInfo
			Return GetDictionarProperty(GetType(T))
		End Function

		''' <summary>获取字典字段，非 IEnumerable(Of String) 基类 Dictionary 则字段无效</summary>
		Public Shared Function GetDictionarProperty(entityType As Type) As PropertyInfo
			If entityType Is Nothing Then Return Nothing

			If Not entityType.IsComeFrom(Of IEntity)(False) Then Return Nothing

			' ID 字段必须为 Long
			Dim idPro = entityType.GetSingleProperty("ID").PropertyType
			If Not idPro.IsLong Then Return Nothing

			' 需要存在 Dictionary 字段且类型为 IEnumerable(Of String)
			Dim dicPro = entityType.GetSingleProperty("Dictionary")
			If dicPro Is Nothing Then Return Nothing

			Dim dicType = dicPro.PropertyType
			Dim baseType = GetType(IEnumerable(Of Long))
			If baseType = dicType OrElse baseType.IsAssignableFrom(dicType) Then
				Return dicPro
			Else
				Return Nothing
			End If
		End Function

#End Region

#Region "获取模块数据"

		''' <summary>模块对象</summary>
		Private Shared _ModuleProvider As IAppModuleProvider

		''' <summary>模块对象</summary>
		Public Shared ReadOnly Property ModuleProvider As IAppModuleProvider
			Get
				If _ModuleProvider Is Nothing Then _ModuleProvider = SYS.GetService(Of IAppModuleProvider)
				Return _ModuleProvider
			End Get
		End Property

		''' <summary>获取模块编号</summary>
		''' <param name="this">需要基于 Entity_Base 的类型</param>
		Public Shared Function GetModuleId(this As Type) As UInteger
			Return ModuleProvider.GetModuleId(this)
		End Function

		''' <summary>获取模块编号</summary>
		Public Shared Function GetModuleId(Of T As IEntity)() As UInteger
			Return ModuleProvider.GetModuleId(GetType(T))
		End Function

		''' <summary>获取模块标识</summary>
		Public Shared Function GetModuleId(moduleName As String) As UInteger
			Return ModuleProvider.GetModuleId(moduleName)
		End Function

		''' <summary>获取模块名称</summary>
		Public Shared Function GetModuleName(moduleId As UInteger) As String
			Return ModuleProvider.GetModuleName(moduleId)
		End Function

		''' <summary>获取模块类型</summary>
		Public Shared Function GetModuleType(moduleName As String) As Type
			Return ModuleProvider.GetModuleType(ModuleProvider.GetModuleId(moduleName))
		End Function

		''' <summary>获取模块类型</summary>
		''' <param name="moduleId">有效的模块标识</param>
		Public Shared Function GetModuleType(moduleId As UInteger) As Type
			Return ModuleProvider.GetModuleType(moduleId)
		End Function

		''' <summary>获取模块审计字段</summary>
		Public Shared Function GetModuleAudit(type As Type) As String()
			Return ModuleProvider.GetModuleAudit(GetModuleId(type))
		End Function

		''' <summary>获取模块审计字段</summary>
		Public Shared Function GetModuleAudit(moduleName As String) As String()
			Return ModuleProvider.GetModuleAudit(GetModuleId(moduleName))
		End Function

		''' <summary>是否包含系统内置模块</summary>
		Public Shared Function ModuleNames(Optional incSystem As Boolean = False) As List(Of String)
			Return ModuleProvider.ModuleNames(incSystem)
		End Function

		''' <summary>是否包含系统内置模块</summary>
		''' <param name="mode">Default 仅返回可模糊查询的数据，true 全部返回，false 仅返回系统数据</param>
		Public Shared Function ModuleDatas(Optional mode As TristateEnum = TristateEnum.DEFAULT) As List(Of DataList)
			Return ModuleProvider.ModuleDatas(mode)
		End Function

		''' <summary>验证对象是否有效，并返回有效对象</summary>
		Public Shared Function ModuleValidate(moduleId As UInteger?, moduleValue As Long?) As Boolean
			Return ModuleProvider.Validate(moduleId, moduleValue)
		End Function

#End Region

		''' <summary>获取表名称</summary>
		Public Shared Function TableName(entityType As Type) As (Name As String, AsTable As String)
			If entityType Is Nothing Then Return Nothing

			'Dim attr = entityType.GetCustomAttribute(Of DbTableAttribute)(False)
			'If attr IsNot Nothing Then Return (attr.Name, attr.AsTable)

			Dim attr = entityType.GetCustomAttribute(Of DbTableAttribute)(True)
			If attr IsNot Nothing Then Return (attr.Name, attr.AsTable)

			Return (entityType.Name, entityType.Name)
		End Function
	End Class
End Namespace