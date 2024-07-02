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
' 	JSON 数据转换
'
' 	name: Auto.Rule.JsonObject
' 	create: 2023-01-03
' 	memo: JSON 数据转换
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>JSON 数据转换</summary>
	Public Class JsonObject
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "JSON 数据转换"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>获取路径，按顺序往下获取</summary>
		Public Property Path As String()

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			'Dim Ret = AutoHelper.GetVarString(Source, data).ToJsonCollection(False, data)


			message.Message = "无效 JSON 数据"
			Dim ret = AutoHelper.GetVarObject(Source, data)
			If ret Is Nothing Then Return Nothing

			' 存在 path 需要获取值
			If Path.NotEmpty Then
				message.Message = "无此路径 JSON 数据：" + Path.JoinString

				For I = 0 To Path.Length - 1
					If ret Is Nothing Then Return Nothing

					' 如果是数字，则返回列表第几项
					' 否则返回对应的键
					Dim key = AutoHelper.GetVarString(Path(I), data)
					If key.IsEmpty Then Return Nothing

					If key.IsNumber Then
						' 数字键
						Dim count = key.ToInteger

						Dim list = TryCast(ret, List(Of Object))
						If list Is Nothing OrElse list.Count < count Then Return Nothing

						ret = list.Item(count)
					Else
						' 字符键
						Dim dic = TryCast(ret, IDictionary(Of String, Object))
						If dic Is Nothing OrElse Not dic.ContainsKey(key) Then Return Nothing

						ret = dic(key)
					End If
				Next
			End If

			message.SetSuccess()
			Return ret
		End Function

#End Region

	End Class
End Namespace