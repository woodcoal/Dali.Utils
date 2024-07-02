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
' 	补全结构
'
' 	name: Model.Text
' 	create: 2024-06-05
' 	memo: 补全结构
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>文本补全信息</summary>
	Public Class TextResult

		''' <summary>生成内容</summary>
		Public Property Content As String

		''' <summary>型号名称</summary>
		Public Property Model As String

		''' <summary>最后更新时间</summary>
		Public Property Last As Date

		''' <summary>是否成功返回，成功则 Message 为结果信息，否则 Message 为错误信息</summary>
		Public Property Success As Boolean

		''' <summary>花费 Token 数量及时间信息</summary>
		Public Property Tokens As TokensInfo
	End Class

End Namespace