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
' 	按对象循环
'
' 	name: Auto.Rule.WhileObject
' 	create: 2023-01-03
' 	memo: 按对象循环
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Auto.Rule
	''' <summary>按对象循环</summary>
	Public Class WhileObject
		Inherits WhileBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "按对象循环"
			End Get
		End Property

		''' <summary>原始数据变量名（对应变量值必须为数组 或者 对象）</summary>
		Public Property Source As String

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

			message.Message = "无效循环变量值，变量值必须存在且为对象数据"
			Dim value = JsonExtension.ToJson(AutoHelper.GetVarObject(Source, data)).ToJsonDictionary(True)
			If value.IsEmpty Then Return Nothing

			Dim idx = 0
			Dim ret As New ConcurrentBag(Of Object)
			Dim retSum As New ConcurrentBag(Of Single)
			Dim retIndex As New ConcurrentBag(Of String)

			' 非安全数组重建
			If data Is Nothing OrElse TypeOf data IsNot SafeKeyValueDictionary Then data = New SafeKeyValueDictionary(data)

			' 规则
			Dim ruleList = AutoHelper.RuleList(Rules, True)

			' 循环执行操作
			Dim countValue = value.Count
			Dim loopExecute = Function(kv As KeyValuePair(Of String, Object)) As Boolean
								  Dim loopValue = AutoHelper.List2Dictionary(kv.Value, False)
								  Dim loopData = New Dictionary(Of String, Object) From {
									  {"_index", idx},
									  {"_count", countValue},
									  {"_key", kv.Key},
									  {"_value", loopValue},
									  {"_item", loopValue}
								  }
								  idx += 1

								  ' 运行索引
								  retIndex.Add(kv.Key)

								  Return WhileExecute(ruleList, data, loopData, Result, ret, Sum, retSum, message)
							  End Function

			' 小于 2 单线程，否则并行多线程处理循环
			If ParallelNumber < 2 Then
				For Each kv In value
					' 执行循环，一旦失败则退出循环
					If Not loopExecute(kv) Then Exit For
				Next

			Else
				Dim options As New ParallelOptions With {.MaxDegreeOfParallelism = ParallelNumber.Range(1, 100)}
				Parallel.ForEach(value, Sub(kv, state)
											' 执行循环，一旦失败则强制停止其他循环
											If Not loopExecute(kv) Then state.Stop()
										End Sub)
			End If

			Return New Dictionary(Of String, Object) From {{"count", ret.Count}, {"run", retIndex.Count}, {"data", ret}, {"sum", retSum.Sum}, {"index", retIndex}}
		End Function


#End Region

	End Class
End Namespace