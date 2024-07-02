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
' 	全局通用参数接口
'
' 	name: Interface.ICommonSetting
' 	create: 2023-02-27
' 	memo: 全局通用参数接口
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace [Interface]

	''' <summary>参数接口</summary>
	Public Interface ICommonSetting
		Inherits ISetting

		''' <summary>当前应用名称</summary>
		Property AppName As String

		''' <summary>系统时间差</summary>
		Property TimeDelay As Integer

		''' <summary>用于修改默认监听的域名与端口。如： http://*:6000,http://localhost:8000</summary>
		Property Urls As String()

		''' <summary>语言列表：zh,en</summary>
		Property Languages As String()

		''' <summary>应用资源静态文件目录</summary>
		<FieldChange(FieldTypeEnum.FOLDER)>
		Property RootFolder As String

		''' <summary>应用日志等数据目录</summary>
		<FieldChange(FieldTypeEnum.FOLDER)>
		Property DataFolder As String

		''' <summary>静态目录数据</summary>
		Property StaticFolder As NameValueDictionary

		''' <summary>是否系统调试模式，调试模式无需权限全部可以操作</summary>
		Property Debug As Boolean

		''' <summary>是否使用静态文件</summary>
		Property UseStatic As Boolean

		''' <summary>Cookies 参数前缀</summary>
		<FieldChange(FieldTypeEnum.ASCII)>
		Property CookiePrefix As String

		''' <summary>Cookies 失效时长</summary>
		<Range(1, 999999999)>
		Property CookieTimeout As Integer

		''' <summary>服务器标识，雪花算法时用于标识设备 ID</summary>
		<Range(1, 8)>
		Property ServerId As Integer

		''' <summary>路由全局前缀</summary>
		<FieldChange(FieldTypeEnum.ASCII)>
		Property RoutePrefix As String


	End Interface
End Namespace
