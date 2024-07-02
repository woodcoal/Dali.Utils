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
' 	流程结束
'
' 	name: Auto.Rule.FlowFinish
' 	create: 2023-01-04
' 	memo: 流程结束
' 	
' ------------------------------------------------------------

Namespace Auto.Rule

	''' <summary>控制台打印</summary>
	Public Class FlowFinish
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "流程结束"
			End Get
		End Property

		''' <summary>对于值已经处理包含变量或者对于包含子模块的项目是否将子模块的执行结果一并输出到主流程，如果不含子流程则不用设置此值</summary>
		''' <remarks>如：计时器。True 则将计时器的结果与计时器内的模块值都输出到主流程 ，False 则只返回计时器的结果；如：http 下载，执行后已经将变量结果输出到结果中则无需再次处理</remarks>
		Public Overrides ReadOnly Property OutResult As String
			Get
				Return True
			End Get
		End Property

		''' <summary>结果内容变量名，模板</summary>
		Public Property VarContent As String

		''' <summary>状态值变量名，模板，赋值为 True 表示整个流程执行成功，未赋值则默认成功</summary>
		Public Property VarFlag As String

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 忽略错误
			ErrorIgnore = True

			Dim flag = If(VarFlag.NotEmpty, AutoHelper.GetVar(VarFlag, data), True)
			If flag Is Nothing Then flag = True
			flag = flag.ToString.ToBoolean

			Dim result = AutoHelper.GetVar(VarContent, data)
			Dim ret = New KeyValueDictionary From {
				{"_Result", result},
				{"_Flag", flag},
				{"_Finish", SYS_NOW_DATE}
			}

			message.SetSuccess(flag, TypeExtension.ToObjectString(result))

			Return ret
		End Function

#End Region

	End Class
End Namespace
