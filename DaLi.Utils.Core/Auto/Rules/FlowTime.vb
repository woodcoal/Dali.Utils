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
' 	计时器
'
' 	name: Auto.Rule.FlowTime
' 	create: 2023-12-27
' 	memo: 流程执行计时器
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>流程执行计时器</summary>
	Public Class FlowTime
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "流程计时器"
			End Get
		End Property

		''' <summary>对于值已经处理包含变量或者对于包含子模块的项目是否将子模块的执行结果一并输出到主流程，如果不含子流程则不用设置此值</summary>
		''' <remarks>如：计时器。True 则将计时器的结果与计时器内的模块值都输出到主流程 ，False 则只返回计时器的结果；如：http 下载，执行后已经将变量结果输出到结果中则无需再次处理</remarks>
		Public Overrides ReadOnly Property OutResult As String
			Get
				Return True
			End Get
		End Property

		''' <summary>判断后执行的规则</summary>
		Public Property Rules As String

#End Region

#Region "INFORMATION"

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "输出变量未设置"
			If Output.IsEmpty Then Return Nothing

			' 检查规则是否有效
			Dim ruleList = AutoHelper.RuleList(Rules, True)
			If ruleList.IsEmpty Then
				message.Message = "无任何可执行模块"
				Return Nothing
			End If

			message.SetSuccess()
			message.Message = "计时启动"

			Dim s As New Stopwatch
			s.Start()
			Dim result = AutoHelper.FlowExecute(ruleList, False, False, data, message)
			s.Stop()

			Dim ret = New KeyValueDictionary From {
				{Output, s.ElapsedMilliseconds},
				{$"{Output}.Desc", s.Elapsed.Show(False)},
				{$"{Output}.Step", ruleList.Count},
				{$"{Output}.Ticks", s.ElapsedTicks}
			}

			message.Message = $"计时结束，共耗时 {ret("Desc")}"

			' OutResult = True
			' 需要将内部执行结果公开到外部
			ret.AddRange(result)

			Return ret
		End Function

#End Region


	End Class
End Namespace