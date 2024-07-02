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
' 	插件管理
'
' 	name: Provider.PluginProvider
' 	create: 2023-02-14
' 	memo: 插件管理
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Reflection
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.Filters

Namespace Provider

	''' <summary>插件管理</summary> 
	Public Class PluginProvider
		Implements IPluginsProvider

		''' <summary>数据集</summary>
		Public Instance As ImmutableDictionary(Of Assembly, ImmutableList(Of Type)) = ImmutableDictionary.Create(Of Assembly, ImmutableList(Of Type))

		''' <summary>锁定对象</summary>
		Private ReadOnly _Lock As New Object

		''' <summary>系统包含的程序集，含插件</summary>
		Public ReadOnly SystemAssemblies As ImmutableList(Of Assembly)

		''' <summary>构造</summary>
		Public Sub New()
			'GetType(BackServiceBase),  BackServiceBase 使用手动注入
			'Dim baseTypes = {GetType(ModuleBase), GetType(ISetting), GetType(IReloader), GetType(ControllerBase), GetType(ActionFilterAttribute)}

			' 基于 IBase，IPlugin 接口的类都将加载
			Dim baseTypes = {GetType(IBase), GetType(ControllerBase), GetType(ActionFilterAttribute)}

			' 从启动程序集获取所有插件基类的接口
			SystemAssemblies = AssemblyHelper.Assemblies(True, "plugins", True)

			' 加载所有类型
			UpdateLoad(baseTypes)
		End Sub

		''' <summary>检查类型是否来自 IPlugin 接口</summary>
		Public Function IsPlugin(type As Type) As Boolean
			Return type?.IsComeFrom(GetType(IPlugin))
		End Function

		''' <summary>检查类型是否来自 IPlugin 接口</summary>
		Public Function IsPlugin(Of T)() As Boolean
			Return IsPlugin(GetType(T))
		End Function

		''' <summary>从目前的程序集中再此分析加载指定类型的；已经存在的类型将不再处理，只处理新增类型</summary>
		Public Sub UpdateLoad(ParamArray newTypes As Type()) Implements IPluginsProvider.UpdateLoad
			If newTypes.IsEmpty Then Return

			SyncLock _Lock
				' 当前所有程序集
				Dim Dic As New Dictionary(Of Assembly, ImmutableList(Of Type))(Instance)

				' 类型分析
				For Each Ass In SystemAssemblies
					Dim types As New List(Of Type)

					AssemblyHelper.Types(Ass, True).
							Where(Function(x) x.IsClass AndAlso Not x.IsAbstract).
							ToList.
							ForEach(Sub(t) If newTypes.Where(Function(x) t.IsComeFrom(x)).Any Then types.Add(t))

					types?.ForEach(Sub(type)
									   If Not Dic.ContainsKey(Ass) Then Dic.Add(Ass, ImmutableList.Create(Of Type))
									   Dim list = Dic(Ass)
									   If Not list.Contains(type) Then Dic(Ass) = list.Add(type)
								   End Sub)
				Next

				Instance = Dic.ToImmutableDictionary
			End SyncLock
		End Sub

		''' <summary>批量操作</summary>
		Public Sub ForEach(action As Action(Of Assembly, Type))
			If action IsNot Nothing Then
				For Each ass In Instance
					If ass.Value.NotEmpty Then
						For Each type In ass.Value
							action.Invoke(ass.Key, type)
						Next
					End If
				Next
			End If
		End Sub

		''' <summary>批量操作</summary>
		Public Sub ForEach(baseType As Type, action As Action(Of Assembly, Type))
			If baseType IsNot Nothing Then
				ForEach(Sub(ass, type)
							If type.IsComeFrom(baseType) Then action.Invoke(ass, type)
						End Sub)
			End If
		End Sub

		''' <summary>获取所有类型</summary>
		Public Function GetTypes(Of T)() As List(Of Type) Implements IPluginsProvider.GetTypes
			Dim Ret As New List(Of Type)
			Dim type = GetType(T)

			For Each Ass In Instance
				Dim Types = Ass.Value.Where(Function(x) x.IsComeFrom(type)).ToList
				If Types?.Count > 0 Then Ret.AddRange(Types)
			Next

			Return Ret
		End Function

		''' <summary>获取指定类型</summary>
		Public Function GetInterface(ass As Assembly, baseType As Type) As List(Of Type)
			If Instance.ContainsKey(ass) Then
				Return Instance(ass).Where(Function(x) x.IsComeFrom(baseType)).ToList
			Else
				Return Nothing
			End If
		End Function

		''' <summary>获取指定类型的接口</summary>
		''' <param name="checkPlugin">是否检查是否 IPlugin 接口，检查则将忽略掉为启用的插件</param>
		Public Function GetInstances(Of T)(Optional checkPlugin As Boolean = False) As List(Of T) Implements IPluginsProvider.GetInstances
			Dim Ret As New List(Of T)

			ForEach(GetType(T), Sub(ass, type)
									Dim obj = ass.CreateInstance(type.FullName)
									Dim ins = ChangeType(Of T)(obj)
									If ins IsNot Nothing Then Ret.Add(ins)
								End Sub)

			' 不检查插件
			If Ret.IsEmpty OrElse Not checkPlugin OrElse Not IsPlugin(Of T)() Then Return Ret

			Return Ret.Where(Function(x) TryCast(x, IPlugin).Enabled).
				OrderByDescending(Function(x) TryCast(x, IPlugin).Order).
				ToList
		End Function

		''' <summary>获取指定类型的接口</summary>
		''' <remarks>将强制检查来自 IPlugin 接口，移除未启用的项目</remarks>
		Public Function GetInstances(Of T)(ParamArray args() As Object) As List(Of T) Implements IPluginsProvider.GetInstances
			Dim Ret As New List(Of T)

			ForEach(GetType(T), Sub(ass, type)
									Dim obj = ass.CreateInstance(type.FullName, True, BindingFlags.Default, Nothing, args, Nothing, Nothing)
									Dim ins = ChangeType(Of T)(obj)
									If ins IsNot Nothing Then Ret.Add(ins)
								End Sub)

			' 不检查插件
			If Ret.IsEmpty OrElse Not IsPlugin(Of T)() Then Return Ret

			Return Ret.Where(Function(x) TryCast(x, IPlugin).Enabled).
				OrderByDescending(Function(x) TryCast(x, IPlugin).Order).
				ToList
		End Function

		''' <summary>获取所有程序集</summary>
		Public Function GetAssemblies() As List(Of Assembly) Implements IPluginsProvider.GetAssemblies
			Return Instance.Keys.ToList
		End Function

		''' <summary>获取包含指定类型的程序集</summary>
		Public Function GetAssemblies(Of T)(Optional validate As Func(Of Type, Boolean) = Nothing) As List(Of Assembly) Implements IPluginsProvider.GetAssemblies
			Return Instance.Where(Function(ins) ins.Value.Where(Function(x)
																	Dim flag = x.IsComeFrom(Of T)
																	If flag AndAlso validate IsNot Nothing Then flag = validate(x)
																	Return flag
																End Function).Any).Select(Function(x) x.Key).ToList
		End Function

		''' <summary>更新程序集，保留指定的程序集，支持 like 通配符匹配</summary>
		Public Sub Update(ParamArray assemblyNames() As String) Implements IPluginsProvider.Update
			If assemblyNames.IsEmpty Then Exit Sub

			' 调整程序集，移除不允许程序集
			SyncLock _Lock
				Instance = Instance.Where(Function(x) x.Key.Name.Like(assemblyNames)).ToImmutableDictionary
			End SyncLock
		End Sub

		''' <summary>更新程序集，去除指定的程序集，支持 like 通配符匹配</summary>
		Public Sub Remove(ParamArray assemblyNames() As String) Implements IPluginsProvider.Remove
			If assemblyNames.IsEmpty Then Exit Sub

			' 调整程序集，移除不允许程序集
			SyncLock _Lock
				Instance = Instance.Where(Function(x) Not x.Key.Name.Like(assemblyNames)).ToImmutableDictionary
			End SyncLock
		End Sub

	End Class

End Namespace