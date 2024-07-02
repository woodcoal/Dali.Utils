' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	API 文档参数
'
' 	name: SwashbuckleSetting
' 	create: 2023-02-16
' 	memo: API 文档参数
'
' ------------------------------------------------------------

Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base

Public Class SwashbuckleSetting
	Inherits LocalSettingBase(Of SwashbuckleSetting)

	''' <summary>API 文档前缀，如果未设置，则关闭文档功能</summary>
	<FieldChange(FieldTypeEnum.ASCII)>
	Public Property Path As String = "docs"
End Class
