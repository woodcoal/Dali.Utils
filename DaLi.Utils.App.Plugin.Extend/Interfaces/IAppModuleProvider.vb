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
' 	字典数据操作接口
'
' 	name: Interface.IAppModuleProvider
' 	create: 2023-02-21
' 	memo: 字典数据操作接口
'
' ------------------------------------------------------------

Imports System.Collections.Immutable

Namespace [Interface]

	''' <summary>字典数据操作接口</summary>
	Public Interface IAppModuleProvider

		''' <summary>重载数据</summary>
		Sub Reload()

		''' <summary>验证对象是否有效，并返回有效对象</summary>
		Function Validate(moduleId As UInteger?, moduleValue As Long?) As Boolean

		''' <summary>内置数据，初始化默认基础项目</summary>
		ReadOnly Property ModuleList As ImmutableDictionary(Of UInteger, (Name As String, Title As String, Search As String(), Audit As String()))

		''' <summary>递归获取类型的模块标识</summary>
		Function GetModuleId(type As Type) As UInteger

		''' <summary>获取模块标识</summary>
		Function GetModuleId(moduleName As String) As UInteger

		''' <summary>获取模块名称</summary>
		Function GetModuleName(moduleId As UInteger) As String

		''' <summary>获取模块搜索字段</summary>
		Function GetModuleSearch(moduleId As UInteger) As String()

		''' <summary>获取模块审计字段</summary>
		Function GetModuleAudit(type As Type) As String()

		''' <summary>获取模块审计字段</summary>
		Function GetModuleAudit(moduleId As UInteger) As String()

		''' <summary>获取模块类型</summary>
		''' <param name="moduleId">有效的模块标识</param>
		Function GetModuleType(moduleId As UInteger) As Type

		''' <summary>获取资源标题</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="moduleValue">模块资源值</param>
		Function GetModuleInfo(moduleId As UInteger?, moduleValue As Long?) As String

		''' <summary>获取资源基础信息</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="moduleValue">模块资源值</param>
		Function Info(moduleId As UInteger?, moduleValue As Long?) As IDictionary(Of String, Object)

		''' <summary>是否包含系统内置模块</summary>
		Function ModuleNames(Optional incSystem As Boolean = False) As List(Of String)

		''' <summary>是否包含系统内置模块</summary>
		''' <param name="mode">Default 仅返回可模糊查询的数据，true 全部返回，false 仅返回系统数据</param>
		Function ModuleDatas(Optional mode As TristateEnum = TristateEnum.DEFAULT) As List(Of DataList)

		''' <summary>模糊搜索</summary>
		''' <param name="moduleId">有效的模块标识</param>
		''' <param name="keyword">关键词</param>
		Function Search(moduleId As UInteger, keyword As String, Optional count As Integer = 25) As List(Of DataList)
	End Interface

End Namespace