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
' 	数据库参数
'
' 	name: Setting.Database
' 	create: 2023-02-17
' 	memo: 数据库参数
'
' ------------------------------------------------------------

Namespace Setting

	''' <summary>数据库参数</summary>
	Public Class DatabaseSetting
		Inherits LocalSettingBase(Of DatabaseSetting)

		''' <summary>数据库名称前缀</summary>
		<FieldChange(FieldTypeEnum.ASCII)>
		Public Property Prefix As String

		''' <summary>系统数据库连接列表</summary>
		Public Property Connections As NameValueDictionary

		Protected Overrides Sub Initialize(provider As ISettingProvider)
			Connections = If(Connections, New NameValueDictionary)

			Dim defConnect = ""
			Dim logConnect = ""

			' 赋值默认连接，防止默认链接未设置时取第一条数据
			If Connections.NotEmpty Then
				defConnect = Connections("default").EmptyValue(Connections.First.Value)
				logConnect = Connections("log").EmptyValue(defConnect)
			End If

			If defConnect.IsEmpty Then
				Throw New Exception("未设置有效的数据库连接！！！")
			Else
				Connections("default") = defConnect
				Connections("log") = logConnect
			End If

			' 赋值前缀
			If Prefix.IsUserName Then DbTableAttribute.Prefix = Prefix
		End Sub

	End Class

End Namespace

