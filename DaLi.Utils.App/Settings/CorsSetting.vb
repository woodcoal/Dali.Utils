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
' 	跨域参数
'
' 	name: Setting.CorsSetting
' 	create: 2023-02-14
' 	memo: 跨域参数
'
' ------------------------------------------------------------

Namespace Setting

	''' <summary>跨域参数</summary>
	Public Class CorsSetting
		Inherits LocalSettingBase(Of CorsSetting)

		''' <summary>是否开启跨域设置</summary>
		Public Property Enable As Boolean = False

		''' <summary>跨域规则</summary>
		Public Property Policies As NameValueDictionary

	End Class
End Namespace

