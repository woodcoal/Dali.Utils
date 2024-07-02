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
' 	关键词相关参数
'
' 	name: Setting.KeywordSetting
' 	create: 2023-02-19
' 	memo: 关键词相关参数
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Setting

	''' <summary>关键词相关参数</summary>
	Public Class KeywordSetting
		Inherits DbSettingBase(Of KeywordSetting)

		''' <summary>无效用户名</summary>
		<Description("不能使用的用户名，JSON 文本数组")>
		Public Property UserName As String() = {"管理", "admin", "administrator"}

		''' <summary>检测的非法关键词</summary>
		<Description("检测的非法关键词，JSON 文本数组")>
		Public Property Checks As String()

		''' <summary>替换的非法关键词</summary>
		<Description("替换的非法关键词，检测到后将用星号代替，JSON 文本数组")>
		Public Property Replaces As String()

		''' <summary>替换关键词检测时，包含禁用关键词，检测到后将用星号代替</summary>
		<Description("替换关键词检测时，包含禁用关键词，检测到后将用星号代替")>
		Public Property ReplaceWithCheck As Boolean = True

	End Class

End Namespace

