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
' 	参数接口
'
' 	name: Interface.ISetting
' 	create: 2023-02-14
' 	memo: 参数接口
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>参数接口</summary>
	Public Interface ISetting
		Inherits IBase

		''' <summary>加载配置</summary>
		Sub Load(provider As ISettingProvider)

		''' <summary>是否注入到配置清单，True：注入，下次可以通过 SYS.GetSetting 获取；False：不注入，参数初始完成后立即销毁，无法再次调用</summary>
		ReadOnly Property Inject As Boolean

		''' <summary>是否更新配置数据时自动更新配置</summary>
		ReadOnly Property AutoUpdate As Boolean

	End Interface
End Namespace
