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
' 	参数设置接口
'
' 	name: Interface.ISettingProvider
' 	create: 2023-02-14
' 	memo: 参数设置接口
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>参数设置接口</summary>
	Public Interface ISettingProvider

		''' <summary>获取配置</summary>
		Function GetSetting(Of T As ISetting)() As T

		''' <summary>加载并更新本地参数</summary>
		Sub Load(settings As IEnumerable(Of ISetting))

		''' <summary>重载指定类型，需要开启自动加载的项目才允许重载</summary>
		Sub Reload(typeName As String)

		''' <summary>所有缓存的类型</summary>
		ReadOnly Property Keys As Type()

		''' <summary>所有缓存的模块名称，模块名称为类型的简化名称</summary>
		ReadOnly Property Modules As String()
	End Interface
End Namespace
