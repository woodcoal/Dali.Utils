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
' 	流程开始
'
' 	name: Auto.Rule.FlowStart
' 	create: 2023-01-04
' 	memo: 流程开始
' 	
' ------------------------------------------------------------

Namespace Auto.Rule

	''' <summary>控制台打印</summary>
	Public Class FlowStart
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "流程开始"
			End Get
		End Property

		''' <summary>对于值已经处理包含变量或者对于包含子模块的项目是否将子模块的执行结果一并输出到主流程，如果不含子流程则不用设置此值</summary>
		''' <remarks>如：计时器。True 则将计时器的结果与计时器内的模块值都输出到主流程 ，False 则只返回计时器的结果；如：http 下载，执行后已经将变量结果输出到结果中则无需再次处理</remarks>
		Public Overrides ReadOnly Property OutResult As String
			Get
				Return True
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Content As NameValueDictionary

#End Region

#Region "INFORMATION"

		''' <summary>克隆</summary>
		Public Overrides Function Clone() As Object
			Dim R As FlowStart = MemberwiseClone()
			R.Content = Content?.Clone
			Return R
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 忽略错误
			ErrorIgnore = True

			Dim Params As New KeyValueDictionary

			If Content.NotEmpty Then
				For Each Kv In Content
					If data.ContainsKey(Kv.Key) Then
						Params.Add(Kv.Key, data(Kv.Key))
					Else
						Dim value = AutoHelper.GetVarObject(Kv.Value, data)
						If value IsNot Nothing Then Params.Add(Kv.Key, value)
					End If
				Next
			End If

			Dim Ret As New KeyValueDictionary(Params)
			Ret("_Start") = SYS_NOW_DATE
			Ret("_") = Params

			message.SetSuccess()
			Return Ret
		End Function

#End Region

	End Class
End Namespace
