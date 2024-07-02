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
' 	程序集反射
'
' 	name: Helper.AssemblyHelper
' 	create: 2020-11-14
' 	memo: 程序集反射
' 	
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Reflection
Imports System.Runtime.Loader

Namespace Helper

	''' <summary>程序集反射</summary>
	Public NotInheritable Class AssemblyHelper

		''' <summary>插件加载上下文对象，以便隔离与主程序的进程，直接加载对于存在外部引用的插件可能导致无法正常使用。具体参考：https://learn.microsoft.com/zh-cn/dotnet/core/tutorials/creating-app-with-plugin-support</summary>
		Private Class PluginLoadContext
			Inherits AssemblyLoadContext

			''' <summary>依赖解析器</summary>
			Private ReadOnly _Resolver As AssemblyDependencyResolver

			''' <summary>默认路径</summary>
			Private ReadOnly _Path As String

			Public Sub New(path As String)
				If path.NotEmpty AndAlso IO.File.Exists(path) Then
					_Path = path
					_Resolver = New AssemblyDependencyResolver(path)
				End If
			End Sub

			''' <summary>根据 AssemblyName 解析并加载程序集</summary>
			Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly
				Dim assemblyPath = _Resolver.ResolveAssemblyToPath(assemblyName)
				If assemblyPath.NotEmpty Then Return LoadFromAssemblyPath(assemblyPath)
				Return Nothing
			End Function

			''' <summary>允许派生的类按名称加载非托管库</summary>
			Protected Overrides Function LoadUnmanagedDll(unmanagedDllName As String) As IntPtr
				Dim libraryPath = _Resolver.ResolveUnmanagedDllToPath(unmanagedDllName)
				If libraryPath.NotEmpty Then Return LoadUnmanagedDllFromPath(libraryPath)
				Return IntPtr.Zero
			End Function

			''' <summary>加载默认的程序集</summary>
			Public Function LoadAssembly() As Assembly
				Return If(_Path.NotEmpty, LoadFromAssemblyPath(_Path), Nothing)
			End Function
		End Class

		''' <summary>系统 Assembly 名，以便过滤掉系统 Assembly</summary>
		Private Shared ReadOnly _AssemblyFilter As New List(Of String) From {"system.*", "microsoft.*", "mscorlib*", "netstandard*", "vshost.*", "interop.*", "google.*", "icsharpcode.*", "newtonsoft.*", "windowsbase*", "swashbuckle.*", "csrediscore*", "mysql*", "npgsql.*", "npoi.*", "sqlite*", "serilog*", "freeredis*", "freesql*", "pomelo.*", "oracle.*"}

		''' <summary>系统 Type 名，以便过滤掉系统 Type</summary>
		Private Shared ReadOnly _TypeFilter As New List(Of String)(_AssemblyFilter)

#Region "过滤器操作"

		''' <summary>添加更多需要过滤的 Assembly 名称</summary>
		Public Shared Sub AssemblyFilterInsert(ParamArray names As String())
			If names?.Length > 0 Then
				For Each N In names
					If Not String.IsNullOrWhiteSpace(N) Then
						N = N.Trim.ToLower

						If Not _AssemblyFilter.Contains(N) Then _AssemblyFilter.Add(N)
					End If
				Next
			End If
		End Sub

		''' <summary>添加更多需要过滤的 Type 名称</summary>
		Public Shared Sub TypeFilterInsert(ParamArray names As String())
			If names?.Length > 0 Then
				For Each N In names
					If Not String.IsNullOrWhiteSpace(N) Then
						N = N.Trim.ToLower

						If Not _TypeFilter.Contains(N) Then _TypeFilter.Add(N)
					End If
				Next
			End If
		End Sub

		''' <summary>滤器无效名称</summary>
		''' <returns>首尾都包含或者不含点则只要存在此值即可；如果只有末尾有点则比较开头，如果只有开始有点则比较末尾</returns>
		Public Shared Function NameFilter(checkName As String, isAssembly As Boolean) As Boolean
			Return Not checkName.Like(If(isAssembly, _AssemblyFilter, _TypeFilter).ToArray)
		End Function

#End Region

#Region "加载 Assembly"

		''' <summary>动态引用类</summary>
		''' <param name="filepath">Assembly 文件路径</param>
		''' <param name="fullName">Assembly 中类的全名</param>
		Public Shared Function CreateInstance(filepath As String, fullName As String) As Object
			If Not String.IsNullOrWhiteSpace(filepath) AndAlso Not String.IsNullOrWhiteSpace(fullName) Then
				filepath = PathHelper.Root(filepath)

				If IO.File.Exists(filepath) Then
					Try
						Return Reflection.Assembly.LoadFrom(filepath)?.CreateInstance(fullName, True)
					Catch ex As Exception
					End Try
				End If
			End If

			Return Nothing
		End Function

		'------------------------------------------------------------------------

		''' <summary>指定路径的 Assembly</summary>
		''' <param name="includeNames">需要包含的 AssemblyName</param>
		Public Shared Function AssemblyLoad(file As String, ParamArray includeNames As String()) As Reflection.Assembly
			Dim ret As Reflection.Assembly = Nothing

			If NameFilter(IO.Path.GetFileNameWithoutExtension(file), True) Then
				Try
					'Dim Assembly = Reflection.Assembly.LoadFrom(file)
					' .net core 使用 AssemblyLoadContext
					Dim Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file)
					If Assembly IsNot Nothing Then
						Dim AssemblyName = Assembly.GetName.Name

						' 检查是否过滤
						Dim IsOK = NameFilter(AssemblyName, True)

						' 检查是否必须包含
						If IsOK AndAlso includeNames?.Length > 0 Then IsOK = AssemblyName.Like(includeNames)

						' 检查是否已经录入
						If IsOK Then ret = Assembly
					End If
				Catch ex As Exception
				End Try
			End If

			Return ret
		End Function

		''' <summary>获取当前系统所有程序集</summary>
		''' <param name="includeBin">是否包含 Bin 目录</param>
		''' <param name="pluginFolder">是否需要加载插件目录，需要则设置插件目录的名称。系统将扫描查询目录下与子目录名相同的程序集。如：plugins 则将扫描 /plugins/xxx/xxx.dll</param>
		''' <param name="skipSystemAssembly">是否过滤系统程序集</param>
		''' <returns>返回所有程序集</returns>
		Public Shared Function Assemblies(Optional includeBin As Boolean = False, Optional pluginFolder As String = "plugins", Optional skipSystemAssembly As Boolean = True) As ImmutableList(Of Assembly)
			'Return AppDomain.CurrentDomain.GetAssemblies.Where(Function(x) NameFilter(x.FullName, True)).ToImmutableList
			' .net core 使用 AssemblyLoadContext
			'Return AssemblyLoadContext.Default.Assemblies.Where(Function(x) NameFilter(x.FullName, True)).ToImmutableList

			Dim ret As IEnumerable(Of Assembly) = New List(Of Assembly)

			' 1. 获取系统程序集
			Dim ass = AssemblyLoadContext.Default.Assemblies
			If ass.NotEmpty Then ret = ret.Union(ass)

			' 2 加载关联程序集（不要尝试加载插件的关联程序集，有可能获取不到而导致异常！！！）
			For Each assembly In ass
				Try
					Dim list = assembly.GetReferencedAssemblies?.Select(Function(x) Assembly.Load(x)).Where(Function(x) x IsNot Nothing).ToList
					If list.NotEmpty Then ret = ret.Union(list)
				Catch ex As Exception
					CON.Err($"{assembly.Name} 关联程序集加载异常：${ex.Message}")
				End Try
			Next

			' 3. 加载 Bin 目录
			If includeBin Then
				Dim Path = PathHelper.Root("bin", True, True)
				Dim files = IO.Directory.GetFiles(Path, "*.dll")
				Dim assBin = files.
					Select(Function(x)
							   Try
								   Return AssemblyLoadContext.Default.LoadFromAssemblyPath(x)
							   Catch ex As Exception
								   CON.Err($"Bin 目录程序集 {x} 加载异常：${ex.Message}")
								   Return Nothing
							   End Try
						   End Function).
					Where(Function(x) x IsNot Nothing)
				If assBin.NotEmpty Then ret = ret.Union(assBin)
			End If

			' 4. 加载插件目录
			If pluginFolder.NotEmpty Then
				' 如果 plugins 目录下存在下级目录，则使用下级目录的目录名做为插件名，获取下级目录下同名 dll；更好的隔离划分插件
				' 注意加载方式，plugin 需要使用 PluginLoadContext 加载，bin 目录直接加载

				' 获取 plugin 目录
				Dim path = PathHelper.Root(pluginFolder, True, True)
				Dim files = IO.Directory.GetDirectories(path)?.Select(Function(x) IO.Path.Combine(x, PathHelper.GetName(x) & ".dll")).ToArray
				Dim assPlugins = files.
					Select(Function(x)
							   Try
								   Return New PluginLoadContext(x).LoadAssembly
							   Catch ex As Exception
								   CON.Err($"Plugin 目录程序集 {x} 加载异常：${ex.Message}")
								   Return Nothing
							   End Try
						   End Function).
					Where(Function(x) x IsNot Nothing)
				If assPlugins.NotEmpty Then ret = ret.Union(assPlugins)
			End If

			' 5. 过滤系统程序集
			If skipSystemAssembly Then ret = ret.Where(Function(x) NameFilter(x.FullName, True)).ToList

			'CON.Succ($"全局加载程序集：{ret.Select(Function(x) x.Name).JoinString(vbTab)}")

			Return ret.Distinct.ToImmutableList
		End Function

		''' <summary>所有指定目录下的 Assembly</summary>
		''' <param name="folder">获取目录，未指定则当前启动目录</param>
		''' <param name="includeNames">需要包含的 AssemblyName</param>
		Public Shared Function Assemblies(folder As String, ParamArray includeNames As String()) As List(Of Reflection.Assembly)
			Dim Files = PathHelper.FileList(folder, "*.dll", True)
			Dim ret = Files?.Select(Function(x) AssemblyLoad(x, includeNames)).Where(Function(x) x IsNot Nothing).Distinct.ToList

			Return If(ret, New List(Of Assembly))
		End Function

		'------------------------------------------------------------------------

		''' <summary>所有指定的模块</summary>
		''' <param name="folder">获取目录，未指定则当前启动目录</param>
		''' <param name="isPulic">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		''' <param name="includeNames">需要包含的 TypeName</param>
		Public Shared Function Types(folder As String, Optional isPulic As Boolean = False, Optional includeNames As String() = Nothing) As List(Of Type)
			Dim List As New List(Of Type)

			Dim AssemblyList = Assemblies(folder)
			If AssemblyList.Count > 0 Then
				For Each Assembly In AssemblyList
					Dim TypeList = Types(Assembly, isPulic, includeNames)
					If TypeList.Count > 0 Then List.AddRange(TypeList)
				Next
			End If

			Return List
		End Function

		''' <summary>所有指定的模块类型</summary>
		''' <param name="assembly">Assembly</param>
		''' <param name="isPulic">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		''' <param name="includeNames">需要包含的 TypeName</param>
		Public Shared Function Types(assembly As Reflection.Assembly, Optional isPulic As Boolean = False, Optional includeNames As String() = Nothing) As List(Of Type)
			Dim List As New List(Of Type)

			If assembly IsNot Nothing Then
				Try
					Dim TypeList = If(isPulic, assembly.GetExportedTypes(), assembly.GetTypes())
					If TypeList?.Length > 0 Then
						For Each Type In TypeList
							If Type IsNot Nothing Then
								Dim TypeName = Type.FullName

								' 检查是否过滤
								Dim IsOK = NameFilter(TypeName, False)

								' 检查是否必须包含
								If IsOK AndAlso includeNames?.Length > 0 Then IsOK = TypeName.Like(includeNames)

								' 检查是否已经录入
								If IsOK AndAlso Not List.Contains(Type) Then List.Add(Type)
							End If
						Next
					End If
				Catch ex As Exception
				End Try
			End If

			Return List
		End Function

		'------------------------------------------------------------------------

		''' <summary>所有指定类型的模块</summary>
		''' <typeparam name="T">返回的类型</typeparam>
		''' <param name="folder">获取目录，未指定则当前启动目录</param>
		''' <param name="includeAssemblyNames">需要包含的 AssemblyName</param>
		''' <param name="includeModuleNames">需要包含的 ModuleName</param>
		Public Shared Function Load(Of T)(Optional folder As String = "", Optional isPulic As Boolean = False, Optional includeAssemblyNames As IEnumerable(Of String) = Nothing, Optional includeModuleNames As IEnumerable(Of String) = Nothing) As IEnumerable(Of T)
			Dim List As New List(Of T)

			Dim ModuleList = Load(folder, isPulic, includeAssemblyNames, includeModuleNames)
			If ModuleList.Count > 0 Then
				For Each Item In ModuleList
					Try
						Dim TItem = CType(Item, T)
						If TItem IsNot Nothing Then List.Add(TItem)
					Catch ex As Exception
					End Try
				Next
			End If

			Return List
		End Function

		''' <summary>所有指定类型的模块</summary>
		''' <param name="folder">获取目录，未指定则当前启动目录</param>
		''' <param name="includeAssemblyNames">需要包含的 AssemblyName</param>
		''' <param name="includeModuleNames">需要包含的 ModuleName</param>
		Public Shared Function Load(Optional folder As String = "", Optional isPulic As Boolean = False, Optional includeAssemblyNames As IEnumerable(Of String) = Nothing, Optional includeModuleNames As IEnumerable(Of String) = Nothing) As List(Of Object)
			Dim List As New List(Of Object)

			Dim AssemblyList = Assemblies(folder, includeAssemblyNames?.ToArray)
			If AssemblyList.Count > 0 Then
				For Each Assembly In AssemblyList
					Dim ModuleList = Load(Assembly, isPulic, includeModuleNames?.ToArray)
					If ModuleList.Count > 0 Then List.AddRange(ModuleList)
				Next
			End If

			Return List
		End Function

		''' <summary>所有指定类型的模块</summary>
		''' <param name="assembly">Assembly</param>
		''' <param name="includeNames">需要包含的 TypeName</param>
		Public Shared Function Load(Of T)(assembly As Reflection.Assembly, Optional isPulic As Boolean = False, Optional includeNames As String() = Nothing) As IEnumerable(Of T)
			Dim List As New List(Of T)

			Dim ModuleList = Load(assembly, isPulic, includeNames)
			If ModuleList.Count > 0 Then
				For Each Item In ModuleList
					Try
						Dim TItem = CType(Item, T)
						If TItem IsNot Nothing Then List.Add(TItem)
					Catch ex As Exception
					End Try
				Next
			End If

			Return List
		End Function

		''' <summary>加载对象</summary>
		''' <param name="assembly">Assembly</param>
		''' <param name="isPulic">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		''' <param name="includeNames">需要包含的 TypeName</param>
		Public Shared Function Load(assembly As Reflection.Assembly, Optional isPulic As Boolean = False, Optional includeNames As String() = Nothing) As List(Of Object)
			Dim List As New List(Of Object)

			Dim TypeList = Types(assembly, isPulic, includeNames)
			If TypeList?.Count > 0 Then
				For Each Type In TypeList
					Try
						Dim Instance = assembly.CreateInstance(Type.FullName)
						If Instance IsNot Nothing Then List.Add(Instance)
					Catch ex As Exception
					End Try
				Next
			End If

			Return List
		End Function

#End Region

	End Class

End Namespace