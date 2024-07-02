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
' 	自定义参数
'
' 	name: Auto.Rule.Parameter
' 	create: 2023-01-03
' 	memo: 自定义参数
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>自定义参数</summary>
	Public Class Parameter
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "自定义参数"
			End Get
		End Property

		''' <summary>参数列表</summary>
		Public Property Vars As NameValueDictionary

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "参数未设置"
			If Vars.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

		''' <summary>克隆</summary>
		Public Overrides Function Clone() As Object
			Dim R As Parameter = MemberwiseClone()
			R.Vars = Vars?.Clone
			Return R
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim Ret As New KeyValueDictionary

			For Each Kv In Vars
				Dim value = AutoHelper.GetVarObject(Kv.Value, data)
				If value IsNot Nothing Then Ret.Add(Kv.Key, value)
			Next

			message.SetSuccess()
			Return Ret
		End Function

#End Region

	End Class
End Namespace