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
' 	数值区间
'
' 	name: Model.NumberRange
' 	create: 2022-06-08
' 	memo: 数值区间
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>数值区间</summary>
	Public Class NumberRange(Of T As Structure)

		''' <summary>最小值</summary>
		Public Property Min As T?

		''' <summary>最大值</summary>
		Public Property Max As T?

	End Class
End Namespace
