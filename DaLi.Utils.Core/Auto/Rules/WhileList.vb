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
' 	按列表循环
'
' 	name: Auto.Rule.WhileList
' 	create: 2023-01-03
' 	memo: 按列表循环
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Auto.Rule
	''' <summary>按列表循环</summary>
	Public Class WhileList
		Inherits WhileBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "按列表循环"
			End Get
		End Property

		''' <summary>原始数据变量名（对应变量值必须为数组 或者 对象）</summary>
		Public Property Source As String

		''' <summary>忽略前几条</summary>
		Public Property Skip As Integer

		''' <summary>循环条数</summary>
		Public Property Count As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始数据变量名未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "变量数据不存在"
			If data.IsEmpty Then Return Nothing

			message.Message = "无效循环变量值，变量值必须存在且为列表数据"
			Dim value = JsonExtension.ToJson(AutoHelper.GetVarObject(Source, data)).ToJsonList(True)
			If value.IsEmpty Then Return Nothing

			' 检查循环
			If Count < 1 Then Count = value.Count
			If Skip < 1 Then Skip = 0

			Dim Max = Skip + Count
			If Max >= value.Count Then
				Max = value.Count - 1
			Else
				Max -= 1
			End If

			Dim ret As New ConcurrentBag(Of Object)
			Dim retSum As New ConcurrentBag(Of Single)
			Dim retIndex As New ConcurrentBag(Of Integer)

			' 非安全数组重建
			If data Is Nothing OrElse TypeOf data IsNot SafeKeyValueDictionary Then data = New SafeKeyValueDictionary(data)

			' 规则
			Dim ruleList = AutoHelper.RuleList(Rules, True)

			' 循环执行操作
			Dim minValue = Skip + 1
			Dim maxValue = minValue + Count - 1
			Dim countValue = value.Count
			Dim loopExecute = Function(idx As Integer) As Boolean
								  Dim loopData = New Dictionary(Of String, Object) From {
									  {"_index", idx + 1},
									  {"_min", minValue},
									  {"_max", maxValue},
									  {"_count", countValue},
									  {"_item", AutoHelper.List2Dictionary(value(idx), False)}
								  }

								  ' 运行索引
								  retIndex.Add(idx)

								  Return WhileExecute(ruleList, data, loopData, Result, ret, Sum, retSum, message)
							  End Function

			' 小于 2 单线程，否则并行多线程处理循环
			If ParallelNumber < 2 Then
				For idx = Skip To Max
					' 执行循环，一旦失败则退出循环
					If Not loopExecute(idx) Then Exit For
				Next

			Else
				' Parallel 循环最大值没有包含值本身，所以需要 + 1，否则少一项
				Dim options As New ParallelOptions With {.MaxDegreeOfParallelism = ParallelNumber.Range(1, 100)}
				Parallel.For(Skip, Max + 1, Sub(idx, state)
												' 执行循环，一旦失败则强制停止其他循环
												If Not loopExecute(idx) Then state.Stop()
											End Sub)
			End If

			Return New Dictionary(Of String, Object) From {{"count", ret.Count}, {"run", retIndex.Count}, {"data", ret}, {"sum", retSum.Sum}, {"index", retIndex}}
		End Function

#End Region

	End Class
End Namespace