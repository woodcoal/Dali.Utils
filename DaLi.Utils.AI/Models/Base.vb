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
' 	基础类型
'
' 	name: Model.Base
' 	create: 2024-06-05
' 	memo: 基础类型
'
' ------------------------------------------------------------


Namespace Model

	''' <summary>模型</summary>
	Public Class Model
		Inherits KeyValueDictionary

		''' <summary>模型名称</summary>
		Public Property Name As String
			Get
				Return GetValue("name")
			End Get
			Set(value As String)
				Item("name") = value
			End Set
		End Property

		''' <summary>创建时间</summary>
		Public Property Created As Date
			Get
				Return GetValue("created", 0).ToDate(True)
			End Get
			Set(value As Date)
				Item("created") = value.UnixTicks
			End Set
		End Property
	End Class

	''' <summary>Token 信息</summary>
	Public Class TokensInfo
		''' <summary>输入 Token 数量</summary>
		Public Property Input As Integer

		''' <summary>输出 Token 数量</summary>
		Public Property Output As Integer

		''' <summary>总 Token 数量</summary>
		Public Property Total As Integer

		''' <summary>加载模型所花费的时间，单位：毫秒</summary>
		Public Property TimeLoad As Long

		''' <summary>评估提示所花费的时间，单位：毫秒</summary>
		Public Property TimePrompt As Long

		''' <summary>生成响应所花费的时间，单位：毫秒</summary>
		Public Property TimeEval As Long

		''' <summary>总花费的时间，单位：毫秒</summary>
		Public Property TimeTotal As Long

	End Class

End Namespace