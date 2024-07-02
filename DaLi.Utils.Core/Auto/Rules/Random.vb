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
' 	随机
'
' 	name: Auto.Rule.Random
' 	create: 2023-01-15
' 	memo: 随机数，大于等于下限，小于上限
' 	
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>随机数</summary>
	Public Class Random
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "随机"
			End Get
		End Property

		''' <summary>最小值</summary>
		Public Property Min As String

		''' <summary>最大值</summary>
		Public Property Max As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "最大值必须设置"
			If Max.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim min = AutoHelper.GetVarString(Me.Min, data)
			Dim max = AutoHelper.GetVarString(Me.Max, data)

			Dim minValue = If(min.IsEmpty, 0, min.ToInteger(True))
			Dim maxValue = If(max.IsEmpty, 0, max.ToInteger(True))
			If minValue > maxValue Then Swap(minValue, maxValue)

			ErrorIgnore = True
			message.SetSuccess()
			Return New System.Random().Next(minValue, maxValue)
		End Function

#End Region

	End Class
End Namespace
