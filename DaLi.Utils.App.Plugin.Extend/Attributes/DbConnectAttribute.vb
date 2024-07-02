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
'	控制器默认操作数据库链接标识
'
' 	name: Attribute.DbConnectAttribute
' 	create: 2023-02-15
' 	memo: 控制器默认操作数据库链接标识
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>控制器默认操作数据库链接标识</summary>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
	Public Class DbConnectAttribute
		Inherits System.Attribute

		''' <summary>数据库连接名称</summary>
		Public Name As String

		Public Sub New(name As String)
			Me.Name = name
		End Sub
	End Class
End Namespace
