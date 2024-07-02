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
' 	信息通知接口
'
' 	name: IMessageNotify
' 	create: 2024--
' 	memo: 信息通知接口
'
' ------------------------------------------------------------

Namespace Misc.Notifier

	''' <summary>信息通知接口</summary>
	Public Interface INotifier

		''' <summary>发送消息</summary>
		''' <param name="message">发送的消息</param>
		''' <param name="receiver">接收人</param>
		''' <param name="exts">扩展消息、参数</param>
		''' <param name="errorMessage">错误信息</param>
		Function Send(message As String, receiver As String, ByRef Optional errorMessage As String = "", Optional exts As KeyValueDictionary = Nothing) As Boolean

	End Interface
End Namespace