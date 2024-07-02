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
' 	数据列表类型
'
' 	name: Model.DataList
' 	create: 2023-02-19
' 	memo: 数据列表类型
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>数据列表类型</summary>
	Public Class DataList
		Public Sub New()
		End Sub

		Public Sub New(dic As IDictionary(Of String, Object),
					   textKey As String,
					   valueKey As String,
					   Optional parentKey As String = Nothing,
					   Optional disabledKey As String = Nothing,
					   Optional extKey As String = Nothing)

			If dic.IsEmpty Then Return

			If textKey.NotEmpty Then Text = dic.Where(Function(x) x.Key.Equals(textKey, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString
			If valueKey.NotEmpty Then Value = dic.Where(Function(x) x.Key.Equals(valueKey, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault
			If parentKey.NotEmpty Then Parent = dic.Where(Function(x) x.Key.Equals(parentKey, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault
			If disabledKey.NotEmpty Then Disabled = dic.Where(Function(x) x.Key.Equals(disabledKey, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString.ToBoolean
			If extKey.NotEmpty Then Ext = dic.Where(Function(x) x.Key.Equals(extKey, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault
		End Sub

		''' <summary>名称</summary>
		Public Property Text As String

		''' <summary>值</summary>
		Public Property Value As Object

		''' <summary>上级</summary>
		Public Property Parent As Object

		''' <summary>禁用</summary>
		Public Property Disabled As Boolean?

		''' <summary>扩展数据</summary>
		Public Property Ext As Object

	End Class
End Namespace
