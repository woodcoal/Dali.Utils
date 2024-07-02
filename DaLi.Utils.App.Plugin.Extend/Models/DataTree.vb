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
' 	树形类型
'
' 	name: Model.DataTree
' 	create: 2023-02-19
' 	memo: 树形类型
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>树形类型</summary>
	Public Class DataTree
		Inherits DataList

		Public Sub New()
		End Sub

		Public Sub New(item As DataList)
			If item Is Nothing Then Return

			Text = item.Text
			Value = item.Value
			Parent = item.Parent
			Disabled = item.Disabled
			Ext = item.Ext
		End Sub

		Public Sub New(item As DataTree)
			If item Is Nothing Then Return

			Text = item.Text
			Value = item.Value
			Parent = item.Parent
			Disabled = item.Disabled
			Ext = item.Ext
			Children = item.Children
		End Sub

		''' <summary>下级</summary>
		Public Property Children As List(Of DataTree)

	End Class
End Namespace
