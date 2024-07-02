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
' 	验证码缓存数据结构
'
' 	name: Model.CaptcheValue
' 	create: 2024-03-05
' 	memo: 验证码缓存数据结构
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>验证码缓存数据结构</summary>
	Public Class CaptcheValue
		''' <summary>验证码</summary>
		Public Property Code As String

		''' <summary>最后操作时间</summary>
		Public Property Last As Date

		''' <summary>IP</summary>
		Public Property IP As String

		''' <summary>发送次数</summary>
		Public Property Count As Integer = 0

	End Class
End Namespace
