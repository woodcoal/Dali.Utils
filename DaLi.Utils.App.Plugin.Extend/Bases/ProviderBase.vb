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
' 	数据操作基类
'
' 	name: Base.ProviderBase
' 	create: 2023-02-20
' 	memo: 数据操作基类
'
' ------------------------------------------------------------

Imports Microsoft.Extensions.Logging

Namespace Base

	''' <summary>试图模型基类</summary>
	Public MustInherit Class ProviderBase

		''' <summary>数据库对象</summary>
		Protected ReadOnly Db As IFreeSql

		''' <summary>数据库对象</summary>
		Protected ReadOnly Log As ILogger(Of ProviderBase)

		''' <summary>构造</summary>
		''' <param name="db">数据库对象</param>
		''' <param name="log">日志</param>
		''' <param name="eventCommand">Reload 远程事件</param>
		Public Sub New(db As IFreeSql, log As ILogger(Of ProviderBase), eventCommand As String)
			Me.Db = db
			Me.Log = log

			If eventCommand.NotEmpty Then SYS.Events.Register(eventCommand, Sub() Reload())
		End Sub

		''' <summary>重载数据集</summary>
		Public MustOverride Sub Reload()

	End Class

End Namespace