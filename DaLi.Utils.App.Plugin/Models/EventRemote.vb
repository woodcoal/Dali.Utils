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
' 	总线事件远程操作
'
' 	name: Model.EventRemote
' 	create: 2023-02-14
' 	memo: 总线事件远程操作
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>总线事件远程操作</summary>
	Public Class EventRemote

		''' <summary>操作命令</summary>
		Public Property Command As String

		''' <summary>执行机器标识</summary>
		Public Property DevIds As IEnumerable(Of Long)

		''' <summary>执行参数</summary>
		Public Property Params As Object

		''' <summary>是否异步执行，True 所有相同命令下的操作都同时执行，False 按提交顺序依次执行</summary>
		Public Property IsAsync As Boolean = False

	End Class

End Namespace
