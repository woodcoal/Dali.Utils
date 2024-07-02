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
' 	本地参数接口
'
' 	name: Interface.ILocalSetting
' 	create: 2023-02-14
' 	memo: 本地参数接口（来自 .config/*.json）
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>本地参数接口（来自 .config/*.json）</summary>
	Public Interface ILocalSetting
		Inherits ISetting

		''' <summary>JSON 文件名称</summary>
		ReadOnly Property Filename As String

	End Interface
End Namespace
