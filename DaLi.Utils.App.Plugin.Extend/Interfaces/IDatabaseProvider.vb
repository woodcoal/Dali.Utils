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
' 	数据接口
'
' 	name: Interface.IDatabaseProvider
' 	create: 2023-02-24
' 	memo: 数据接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>数据接口</summary>
	Public Interface IDatabaseProvider

		''' <summary>获取数据库连接对象</summary>
		Function GetDb(name As String) As IFreeSql

		''' <summary>所有设置中的链接信息</summary>
		ReadOnly Property Connections As NameValueDictionary

	End Interface

End Namespace
