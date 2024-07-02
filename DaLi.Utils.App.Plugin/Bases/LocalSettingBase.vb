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
' 	本地参数基类
'
' 	name: Base.LocalSettingBase
' 	create: 2023-02-14
' 	memo: 本地参数基类（来自 .config/*.json）
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports System.Text.Json.Serialization
Imports Microsoft.Extensions.FileProviders
Imports Microsoft.Extensions.Primitives

Namespace Base

	''' <summary>本地参数基类（来自 .config/*.json）</summary>
	Public MustInherit Class LocalSettingBase(Of T As ILocalSetting)
		Implements ILocalSetting, IDisposable

		''' <summary>参数文件监控对象</summary>
		Protected Shared FileProvider As PhysicalFileProvider

		'''' <summary>设置参数文件名称</summary>
		<JsonIgnore>
		Protected Overridable ReadOnly Property Filename As String Implements ILocalSetting.Filename
			Get
				Return CommonHelper.UpdateName([GetType].Name, "setting") & ".json"
			End Get
		End Property

		'''' <summary>是否需要开启监控，文件变化则自动更新参数值</summary>
		<JsonIgnore>
		Protected Overridable ReadOnly Property AutoUpdate As Boolean = False Implements ISetting.AutoUpdate

		''' <summary>是否注入到配置清单，True：注入，下次可以通过 SYS.GetSetting 获取；False：不注入，参数初始完成后立即销毁，无法再次调用</summary>
		<JsonIgnore>
		Protected Overridable ReadOnly Property Inject As Boolean = True Implements ISetting.Inject

		''' <summary>读取设置</summary>
		Public Sub Load(provider As ISettingProvider) Implements ILocalSetting.Load
			If Filename.IsEmpty Then Return

			' 注意使用小写文件名，从 .config 获取
			Dim root = PathHelper.Root(".config", False, True)
			Dim file = Filename.ToLower
			Dim path = IO.Path.Combine(root, file)

			' 文件不存在则尝试从插件目录获取
			If Not IO.File.Exists(path) Then
				root = PathHelper.Root($"plugins/{[GetType].Assembly.Name}/.config", False, True)
				path = IO.Path.Combine(root, file)
			End If

			' 文件不存在也需要做一次初始化
			If Not IO.File.Exists(path) Then
				Initialize(provider)
				Return
			End If

			' 检查是否需要检测
			If AutoUpdate Then
				FileProvider = If(FileProvider, New PhysicalFileProvider(root))

				Dim tokenFunc = Function() FileProvider.Watch(file)
				Dim actionFunc = Debounce(Sub() LoadSetting(path, provider), 30000)   ' 防抖，30 秒内不重复操作，以最后操作为准

				ChangeToken.OnChange(tokenFunc, actionFunc)
			End If

			' 加载参数
			LoadSetting(path, provider)
		End Sub

		''' <summary>加载结果</summary>
		Private Sub LoadSetting(path As String, provider As ISettingProvider)
			' 加载 JSON
			Dim setting = PathHelper.ReadJson(Of T)(path)
			If setting IsNot Nothing Then
				' 属性验证
				' 验证主要字段
				Dim valContext = New ValidationContext(setting)
				Dim result As New List(Of ValidationResult)
				Dim errorFields As List(Of String) = Nothing
				Try
					' 序列化后的值检测
					If Not Validator.TryValidateObject(setting, valContext, result, True) Then
						errorFields = result.Select(Function(x) x.MemberNames.FirstOrDefault).Where(Function(x) x.NotEmpty).ToList
					End If
				Catch ex As Exception
				End Try
				errorFields = If(errorFields, New List(Of String))

				' 更新前检查
				BeforeInitialize(provider)

				' 重新赋值参数
				Try
					setting.GetType.GetAllProperties.
						Where(Function(x) Not errorFields.Contains(x.Name)).
						ToList.
						ForEach(Sub(pro) If pro.CanWrite Then pro.SetValue(Me, pro.GetValue(setting)))
				Catch ex As Exception
				End Try
			End If

			' 初始化一次操作
			Initialize(provider)
		End Sub

		''' <summary>更新参数前的操作</summary>
		Protected Overridable Sub BeforeInitialize(provider As ISettingProvider)
		End Sub

		''' <summary>获取数据后初始化操作，如果不注入的话，初始化一定要操作，否则参数加载无意义</summary>
		Protected Overridable Sub Initialize(provider As ISettingProvider)
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			FileProvider?.Dispose()
			GC.SuppressFinalize(Me)
		End Sub

	End Class

End Namespace
