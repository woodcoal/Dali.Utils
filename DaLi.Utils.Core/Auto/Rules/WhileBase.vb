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
' 	循环基类
'
' 	name: Auto.Rule.WhileBase
' 	create: 2023-01-23
' 	memo: 循环基类
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Auto.Rule
	''' <summary>循环基类</summary>
	Public MustInherit Class WhileBase
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>循环结果值，如果存在此变量名，则使用变量值，否则使用文本</summary>
		Public Property Result As String

		''' <summary>求和字段，循环中用于累积求和的变量或者文本</summary>
		Public Property Sum As String

		''' <summary>循环体中执行的规则</summary>
		Public Property Rules As String

		''' <summary>单线程或多线程并行执行，小于 2 单线程，大于 1 异步并行执行线程数</summary>
		Public Property ParallelNumber As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "循环执行规则未设置或者异常"
			Dim ruleList = AutoHelper.RuleList(Rules, True)
			If ruleList.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>循环内部规则操作</summary>
		''' <param name="rules">内部规则列表</param>
		''' <param name="loopData">循环中数据</param>
		''' <param name="contextData">上下文中数据</param>
		''' <param name="message">消息状态</param>
		''' <param name="varResult">返回值模板</param>
		''' <param name="result">返回值列表</param>
		''' <param name="varSum">求和字段</param>
		''' <param name="resultSUM">求和值</param>
		Protected Shared Function WhileExecute(rules As IList(Of IRule), contextData As SafeKeyValueDictionary, loopData As IDictionary(Of String, Object), varResult As String, result As ConcurrentBag(Of Object), varSum As String, resultSum As ConcurrentBag(Of Single), message As AutoMessage) As Boolean
			Try
				Dim exchange As New SafeKeyValueDictionary(contextData)
				exchange.Update(loopData)

				Dim dic = AutoHelper.FlowExecute(rules, False, False, exchange, message)

				If varResult.NotEmpty AndAlso result IsNot Nothing Then
					Dim loopValue = AutoHelper.GetVar(varResult, exchange)
					If loopValue IsNot Nothing Then result.Add(loopValue)
				End If

				If varSum.NotEmpty AndAlso resultSum IsNot Nothing Then
					Dim loopSum = AutoHelper.GetVarString(varSum, exchange, True).ToSingle(True)
					If loopSum <> 0 Then resultSum.Add(loopSum)
				End If

				' 异常退出
				If dic Is Nothing Then
					Return False
				Else
					' 排除所有下划线变量
					dic = dic.Where(Function(x) Not x.Key.StartsWith("_")).ToDictionary(Function(x) x.Key, Function(x) x.Value)
					AutoHelper.UpdateData(contextData, dic)
				End If
			Catch ex As AutoException
				message.Message = ex.Message

				' 退出循环
				If ex.AutoType = ExceptionEnum.LOOP_STOP Then Return False

				' 其他异常无需处理
			End Try

			Return True
		End Function

#End Region

	End Class
End Namespace