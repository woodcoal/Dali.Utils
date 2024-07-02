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
' 	循环中断
'
' 	name: Auto.Rule.WhileBreak
' 	create: 2023-01-04
' 	memo: 循环中断
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>循环中断</summary>
	Public Class WhileBreak
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "循环中断"
			End Get
		End Property

		''' <summary>中断、退出</summary>
		Public Property [Stop] As Boolean

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 无返回内容不报错
			ErrorIgnore = False

			message.SetSuccess()
			Throw New AutoException(If([Stop], ExceptionEnum.LOOP_STOP, ExceptionEnum.LOOP_BREAK), message)
		End Function

#End Region

	End Class
End Namespace