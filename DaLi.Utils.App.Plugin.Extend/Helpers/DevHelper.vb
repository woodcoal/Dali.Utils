' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.App Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	设备信息附加操作
'
' 	name: Helper.DevHelper
' 	create: 2023-12-20
' 	memo: 设备信息附加操作
'
' ------------------------------------------------------------

Namespace Helper
	''' <summary>设备信息附加操作</summary>
	Public NotInheritable Class DevHelper

		''' <summary>当前设备列表</summary>
		Public Shared ReadOnly Property Devs As Dictionary(Of Long, String)
			Get
				Dim pro = SYS.GetService(Of IAppDictionaryProvider)
				If pro Is Nothing Then Return Nothing

				Dim items = pro.List(VAR_DICTIONARY_DEV_ID, True, False)
				If items.IsEmpty Then Return Nothing

				Return items.ToDictionary(Function(x) ChangeType(Of Long)(x.Value), Function(x) x.Text)
			End Get
		End Property

		''' <summary>是否包含当前设备</summary>
		Public Shared Function HasCurrent(Of T As {IEntity, Class})(entity As T) As Boolean
			If entity Is Nothing OrElse entity.ID < 1 Then Return False

			' 获取相关字典数据
			Dim pro = SYS.GetService(Of IAppDictionaryProvider)
			If pro Is Nothing Then Return False

			Return HasCurrent(pro.ModuleDictionayIds(entity))
		End Function

		''' <summary>是否包含当前设备</summary>
		Public Shared Function HasCurrent(dictionaryIds As IEnumerable(Of Long)) As Boolean
			If dictionaryIds.IsEmpty Then Return False

			' 是否包含本机标识，包含才可以运行
			Return dictionaryIds.Contains(SYS.ID) OrElse dictionaryIds.Contains(VAR_DICTIONARY_DEVALL_ID)
		End Function

		''' <summary>是否包含当前设备</summary>
		Public Shared Function HasDev(dictionaryIds As IEnumerable(Of Long)) As Boolean
			If dictionaryIds.IsEmpty Then Return False

			' 包含全部应用，则其他应用无需选择
			Dim ds = Devs
			If ds.IsEmpty Then Return False

			' 是否包含任何设备标识
			Return dictionaryIds.Any(Function(x) ds.ContainsKey(x))
		End Function

		''' <summary>更新字典中应用信息，如果选择不限则其他应用无需选择</summary>
		Public Shared Sub Update(ByRef dictionaryIds As List(Of Long))
			If dictionaryIds.IsEmpty OrElse Not dictionaryIds.Contains(VAR_DICTIONARY_DEVALL_ID) Then Return

			' 包含全部应用，则其他应用无需选择
			Dim ds = Devs
			If ds.IsEmpty Then Return

			SyncLock dictionaryIds
				For Each id In ds.Keys
					dictionaryIds.Remove(id)
				Next

				' 全部设备移除后，添加全部
				dictionaryIds.Add(VAR_DICTIONARY_DEVALL_ID)
			End SyncLock
		End Sub

		''' <summary>记录设备信息</summary>
		Public Shared Sub Register(id As Long, name As String)
			Dim pro = SYS.GetService(Of IAppDictionaryProvider)
			If pro Is Nothing Then Return

			' 记录设备信息
			pro.DictionaryInsertValues(VAR_DICTIONARY_DEV_ID, New Dictionary(Of Long, String) From {{id, name}})
		End Sub

		''' <summary>注销设备记录</summary>
		''' <param name="id">设备标识</param>
		''' <param name="remove">是否移除，否则标记禁用</param>
		Public Shared Sub Unregister(id As Long, remove As Boolean)
			Dim pro = SYS.GetService(Of IAppDictionaryProvider)
			If pro Is Nothing Then Return

			' 移除设备
			If remove Then pro.DictionaryRemove(id)

			' 禁用设备
			pro.DictionarySwitchEnable(id, False)
		End Sub
	End Class
End Namespace