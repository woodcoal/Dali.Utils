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
' 	按范围循环
'
' 	name: Auto.Rule.WhileInterval
' 	create: 2023-01-23
' 	memo: 按范围循环
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Auto.Rule
	''' <summary>按范围循环</summary>
	Public Class WhileInterval
		Inherits WhileBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "按范围循环"
			End Get
		End Property

		''' <summary>最小值</summary>
		Public Property Min As String

		''' <summary>最大值</summary>
		Public Property Max As String

		''' <summary>进度。大于零：从小到大；小于零：从大到小</summary>
		Public Property Interval As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "进度不能为零"
			If Interval.IsEmpty OrElse Interval = "0" Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim ret As New ConcurrentBag(Of Object)
			Dim retSum As New ConcurrentBag(Of Single)
			Dim retIndex As New ConcurrentBag(Of Integer)

			Dim min = AutoHelper.GetVarString(Me.Min, data, True).ToInteger(True)
			Dim max = AutoHelper.GetVarString(Me.Max, data, True).ToInteger(True)
			Dim interval = AutoHelper.GetVarString(Me.Interval, data, True).ToInteger(True)

			message.Message = "最大值必须大于最小值"
			If min > max Then Return Nothing

			message.Message = "进度不能为0"
			If interval = 0 Then Return Nothing

			message.Message = "无效循环参数"
			Dim count = System.Math.Ceiling(System.Math.Abs((max - min) / interval))
			If count < 1 Then Return Nothing

			' 非安全数组重建
			If data Is Nothing OrElse TypeOf data IsNot SafeKeyValueDictionary Then data = New SafeKeyValueDictionary(data)

			' 规则列表
			Dim ruleList = AutoHelper.RuleList(Rules, True)

			' 循环执行操作
			Dim loopExecute = Function(idx As Integer) As Boolean
								  Dim loopData As New Dictionary(Of String, Object) From {
									  {"_index", idx},
									  {"_min", min},
									  {"_max", max},
									  {"_interval", interval},
									  {"_count", count}
								  }

								  ' 运行索引
								  retIndex.Add(idx)

								  Return WhileExecute(ruleList, data, loopData, Result, ret, Sum, retSum, message)
							  End Function

			' 小于 2 单线程，否则并行多线程处理循环
			If ParallelNumber < 2 Then
				' 周期小于 0，倒序
				If interval < 0 Then Swap(min, max)

				' 循环
				For I = min To max Step interval
					' 执行循环，一旦失败则退出循环
					If Not loopExecute(I) Then Exit For
				Next

			Else

				' 是否倒序
				Dim isDesc = interval < 0

				' 并行本身无法按顺序执行
				interval = System.Math.Abs(interval)

				' 是否指定周期
				Dim notStep = Function(idx As Integer) As Boolean
								  If isDesc Then
									  ' 倒序比较最大值与当前值的周期
									  Return (max - idx) Mod interval <> 0
								  Else
									  ' 顺序比较当前值与最小值的周期
									  Return (idx - min) Mod interval <> 0
								  End If
							  End Function

				' Parallel 循环最大值没有包含值本身，所以需要 + 1，否则少一项
				Dim options As New ParallelOptions With {.MaxDegreeOfParallelism = ParallelNumber.Range(1, 100)}
				Parallel.For(min, max + 1, Sub(idx, state)
											   ' 周期检查, 非指定周期不执行操作
											   Dim no = notStep(idx)
											   CON.Echo({idx, no})
											   If notStep(idx) Then Return

											   ' 执行循环，一旦失败则强制停止其他循环
											   If Not loopExecute(idx) Then state.Stop()
										   End Sub)
			End If

			message.SetSuccess()
			Return New Dictionary(Of String, Object) From {{"count", ret.Count}, {"run", retIndex.Count}, {"data", ret}, {"sum", retSum.Sum}, {"index", retIndex}}
		End Function

#End Region

	End Class
End Namespace