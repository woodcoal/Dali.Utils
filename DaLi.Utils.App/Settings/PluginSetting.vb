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
' 	插件设置
'
' 	name: Setting.PluginSetting
' 	create: 2023-02-15
' 	memo: 插件设置
'
' ------------------------------------------------------------

Namespace Setting

	''' <summary>插件设置</summary>
	Public Class PluginSetting
		Inherits LocalSettingBase(Of PluginSetting)

		''' <summary>禁用插件列表，可以使用通配符 *，匹配 Assembly</summary>
		Public Property Disabled As String()

	End Class

End Namespace

