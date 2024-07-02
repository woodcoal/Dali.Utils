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
' 	按次数循环
'
' 	name: Auto.Rule.WhileTimes
' 	create: 2023-01-03
' 	memo: 按次数循环
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Auto.Rule
	''' <summary>按次数循环</summary>
	Public Class WhileTimes
		Inherits WhileBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "按次数循环"
			End Get
		End Property

		''' <summary>循环次数</summary>
		Public Property Count As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "循环次数必须大于0"
			If Count < 1 Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim ret As New ConcurrentBag(Of Object)
			Dim retIndex As New ConcurrentBag(Of Integer)
			Dim retSum As New ConcurrentBag(Of Single)

			' 记录数
			Count = Count.Range(1, Integer.MaxValue)

			' 非安全数组重建
			If data Is Nothing OrElse TypeOf data IsNot SafeKeyValueDictionary Then data = New SafeKeyValueDictionary(data)

			' 规则列表
			Dim ruleList = AutoHelper.RuleList(Rules, True)

			' 循环执行操作
			Dim loopExecute = Function(idx As Integer) As Boolean
								  Dim loopData As New Dictionary(Of String, Object) From {
									  {"_index", idx},
									  {"_min", 1},
									  {"_max", Count},
									  {"_count", Count}
								  }

								  ' 运行索引
								  retIndex.Add(idx)

								  Return WhileExecute(ruleList, data, loopData, Result, ret, Sum, retSum, message)
							  End Function

			' 小于 2 单线程，否则并行多线程处理循环
			If ParallelNumber < 2 Then
				For I = 1 To Count
					' 执行循环，一旦失败则退出循环
					If Not loopExecute(I) Then Exit For
				Next

			Else
				' Parallel 循环最大值没有包含值本身，所以需要 + 1，否则少一项
				Dim options As New ParallelOptions With {.MaxDegreeOfParallelism = ParallelNumber.Range(1, 100)}
				Parallel.For(1, Count + 1, Sub(idx, state)
											   ' 执行循环，一旦失败则强制停止其他循环
											   If Not loopExecute(idx) Then state.Stop()
										   End Sub)
			End If

			Return New Dictionary(Of String, Object) From {{"count", ret.Count}, {"run", retIndex.Count}, {"data", ret}, {"sum", retSum.Sum}, {"index", retIndex}}
		End Function

#End Region

	End Class
End Namespace