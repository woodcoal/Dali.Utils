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
' 	输出异常
'
' 	name: Auto.Rule.FlowException
' 	create: 2023-01-03
' 	memo: 输出异常
' 	
' ------------------------------------------------------------

Namespace Auto.Rule

	''' <summary>输出异常</summary>
	Public Class FlowException
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "输出异常"
			End Get
		End Property

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 忽略错误
			ErrorIgnore = False

			message.Message = AutoHelper.GetVarString(ErrorMessage, data)
			Throw New AutoException(ExceptionEnum.NORMAL, message, message.Message)
		End Function

#End Region

	End Class
End Namespace
