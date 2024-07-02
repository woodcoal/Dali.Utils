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
' 	插件操作接口
'
' 	name: Interface.IPluginsProvider
' 	create: 2023-02-14
' 	memo: 插件操作接口
'
' ------------------------------------------------------------

Imports System.Reflection

Namespace [Interface]
	''' <summary>插件操作接口</summary>
	Public Interface IPluginsProvider

		''' <summary>获取所有程序集</summary>
		Function GetAssemblies() As List(Of Assembly)

		''' <summary>获取包含指定类型的程序集</summary>
		Function GetAssemblies(Of T)(Optional validate As Func(Of Type, Boolean) = Nothing) As List(Of Assembly)

		''' <summary>获取指定类型的接口</summary>
		''' <param name="checkPlugin">是否检查是否 IPlugin 接口，检查则将忽略掉为启用的插件</param>
		Function GetInstances(Of T)(Optional checkPlugin As Boolean = False) As List(Of T)

		''' <summary>获取指定类型的接口</summary>
		Function GetInstances(Of T)(ParamArray args() As Object) As List(Of T)

		''' <summary>获取所有类型</summary>
		Function GetTypes(Of T)() As List(Of Type)

		''' <summary>从目前的程序集中再此分析加载指定类型的；已经存在的类型将不再处理，只处理新增类型</summary>
		Sub UpdateLoad(ParamArray newTypes As Type())

		''' <summary>更新程序集，保留指定的程序集，支持 like 通配符匹配</summary>
		Sub Update(ParamArray assemblyNames() As String)

		''' <summary>更新程序集，去除指定的程序集，支持 like 通配符匹配</summary>
		Sub Remove(ParamArray assemblyNames() As String)

	End Interface
End Namespace
