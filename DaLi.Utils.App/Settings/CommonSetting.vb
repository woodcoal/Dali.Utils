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
' 	全局通用参数
'
' 	name: Setting.CommonSetting
' 	create: 2023-02-14
' 	memo: 全局通用参数
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Setting

	''' <summary>全局通用参数</summary>
	Public Class CommonSetting
		Inherits LocalSettingBase(Of CommonSetting)
		Implements ICommonSetting

		''' <summary>当前应用名称</summary>
		Public Property AppName As String Implements ICommonSetting.AppName

		''' <summary>系统时间差</summary>
		Public Property TimeDelay As Integer = 0 Implements ICommonSetting.TimeDelay

		''' <summary>用于修改默认监听的域名与端口。如： http://*:6000,http://localhost:8000</summary>
		Public Property Urls As String() Implements ICommonSetting.Urls

		''' <summary>语言列表：zh,en</summary>
		Public Property Languages As String() = {"zh"} Implements ICommonSetting.Languages

		''' <summary>应用资源静态文件目录</summary>
		<FieldChange(FieldTypeEnum.FOLDER)>
		Public Property RootFolder As String = ".web" Implements ICommonSetting.RootFolder

		''' <summary>应用日志等数据目录</summary>
		<FieldChange(FieldTypeEnum.FOLDER)>
		Public Property DataFolder As String = ".data" Implements ICommonSetting.DataFolder

		''' <summary>静态目录数据</summary>
		Public Property StaticFolder As New NameValueDictionary Implements ICommonSetting.StaticFolder

		''' <summary>是否系统调试模式，调试模式无需权限全部可以操作</summary>
		Public Property Debug As Boolean = False Implements ICommonSetting.Debug

		''' <summary>是否使用静态文件</summary>
		Public Property UseStatic As Boolean = False Implements ICommonSetting.UseStatic

		''' <summary>Cookies 参数前缀</summary>
		<FieldChange(FieldTypeEnum.ASCII)>
		Public Property CookiePrefix As String = "DaLi" Implements ICommonSetting.CookiePrefix

		''' <summary>Cookies 失效时长</summary>
		<Range(1, 999999999)>
		Public Property CookieTimeout As Integer = 3600 Implements ICommonSetting.CookieTimeout

		''' <summary>服务器标识，雪花算法时用于标识设备 ID</summary>
		<Range(1, 8)>
		Public Property ServerId As Integer = 1 Implements ICommonSetting.ServerId

		''' <summary>路由全局前缀</summary>
		<FieldChange(FieldTypeEnum.ASCII)>
		Public Property RoutePrefix As String = "" Implements ICommonSetting.RoutePrefix

		''' <summary>参数初始化</summary>
		Protected Overrides Sub Initialize(provider As ISettingProvider)
			Dim folders = StaticFolder
			If folders.NotEmpty Then
				For Each Folder In folders
					If Folder.Key.NotEmpty AndAlso Folder.Value.NotEmpty Then
						Dim dir = PathHelper.Root(Folder.Value, True, True)
						If PathHelper.FolderExist(dir) Then
							StaticFolder.Add(Folder.Key, dir)
						End If
					End If
				Next
			End If

			' 系统时间差
			TIME_DELAY = TimeDelay
		End Sub

	End Class

End Namespace

