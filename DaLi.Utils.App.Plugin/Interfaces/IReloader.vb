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
' 	自动重载数据接口
'
' 	name: Interface.IReloader
' 	create: 2023-02-14
' 	memo: 自动重载数据接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>自动重载数据接口</summary>
	Public Interface IReloader
		Inherits IBase

		''' <summary>更新状态，通知重载</summary>
		Sub UpdateReloadStatus()

		''' <summary>重载数据，可后台定时更新此任务, 10 分钟最多允许更新一次</summary>
		Function ReloadAsync(stoppingToken As Threading.CancellationToken) As Task

		''' <summary>强制重新更新数据，不建议使用，最好定时后台调用 ReloadAsync 异步操作</summary>
		''' <param name="must">是否立即重载，True 立即操作；False 如果间隔时间小于指定时间，则不进行重载</param>
		Sub Reload(Optional must As Boolean = False)

		''' <summary>最后操作时间</summary>
		ReadOnly Property Last As Date

	End Interface

End Namespace