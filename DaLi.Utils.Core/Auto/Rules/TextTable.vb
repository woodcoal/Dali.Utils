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
' 	表格文本
'
' 	name: Auto.Rule.TextTable
' 	create: 2023-01-01
' 	memo: 表格文本
' 	
' ------------------------------------------------------------

Imports System.Data

Namespace Auto.Rule
	''' <summary>表格文本</summary>
	Public Class TextTable
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "表格文本"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Source As String

		''' <summary>表格开始区域</summary>
		Public Property Table As String = "<table[*]</table>"

		''' <summary>行开始区域</summary>
		Public Property Tr As String = "<tr[*]</tr>"

		''' <summary>单元格开始区域</summary>
		Public Property Td As String = "(<(td|th)(.|\n)*?</(td|th)>)"

		''' <summary>清除单元格中的标签</summary>
		Public Property ClearTags As String()

		''' <summary>是否合并多个相同表格</summary>
		Public Property Muti As Boolean

		''' <summary>忽略表格的前几行（防止表头），默认：1</summary>
		Public Property IgnoreIndex As Integer = 1

		''' <summary>表格值操作记录，名称为字段名，值为字段处理字符串，用 data.index 来代替第 N 列，从 0 开始</summary>
		Public Property ValueAction As NameValueDictionary

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "原始内容未设置"
			If Source.IsEmpty Then Return False

			message = "未设置有效的规则内容"
			If Table.IsEmpty AndAlso Tr.IsEmpty AndAlso Td.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>处理表格区域内容，返回表格数据</summary>
		Private Shared Function GetTable(area As String， rule As TextTable, errorMessage As String) As List(Of String())
			errorMessage = "无效表格区域内容"
			If area.IsEmpty Then Return Nothing

			errorMessage = "未获取到表格行数据"
			Dim trList As String() = area.Cut(rule.Tr, 0, True)
			trList = trList.Skip(rule.IgnoreIndex).ToArray
			If trList.IsEmpty Then Return Nothing

			Dim Rets As New List(Of String())

			For Each trItem In trList
				Dim Tds As String() = trItem.Cut(rule.Td, 0, True)
				If Tds.NotEmpty Then
					If rule.ClearTags.NotEmpty Then Tds = Tds.Select(Function(x) x.ClearHtml(rule.ClearTags)).ToArray
					Rets.Add(Tds)
				End If
			Next

			Return Rets
		End Function

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 克隆防止修改原始数据
			Dim rule As TextTable = Clone

			' -----------------
			' 1. 基本参数调整
			' -----------------
			If data.NotEmpty Then
				rule.Source = AutoHelper.GetVarString(rule.Source, data)
				rule.Table = AutoHelper.GetVarString(rule.Table, data)
				rule.Tr = AutoHelper.GetVarString(rule.Tr, data)
				rule.Td = AutoHelper.GetVarString(rule.Td, data)
			End If

			'Dim content As String = ""

			' -----------------
			' 2. 区域切割
			' -----------------
			Dim area = rule.Source.Cut(rule.Table, 0, rule.Muti)
			If area Is Nothing Then
				message.Message = "未获取到表格区域数据"
				Return Nothing
			End If

			' -----------------
			' 2. 获取表格
			' -----------------
			Dim Rets As List(Of String()) = Nothing
			Dim errorMessage = ""
			If rule.Muti Then
				Rets = New List(Of String())

				For Each item In TryCast(area, String())
					Dim strs = GetTable(item, rule, errorMessage)
					If strs.NotEmpty Then Rets.AddRange(strs)

					If errorMessage.NotEmpty Then Exit For ' 有错误直接退出
				Next
			Else
				Rets = GetTable(area, rule, errorMessage)
			End If

			If errorMessage.NotEmpty Then
				message.SetSuccess(False, errorMessage)
				Return Nothing
			End If

			'-------------------
			' 3. 调整为同大小数组表
			'-------------------
			If Rets.NotEmpty Then
				Dim max = Rets.Max(Function(x) x.Length) - 1
				Rets = Rets.Select(Function(x)
									   ReDim Preserve x(max)
									   Return x
								   End Function).ToList
			End If

			'-------------------
			' 4. 记录处理
			'-------------------
			message.SetSuccess()
			If ValueAction.IsEmpty Then Return Rets

			Dim dic As New KeyValueDictionary(data)
			Return Rets.Select(Function(x)
								   Dim KVs As New KeyValueDictionary

								   ValueAction.ForEach(Sub(k, v)
														   dic.Item("data") = x

														   v = AutoHelper.GetVarObject(v, dic)
														   If v IsNot Nothing Then KVs.Add(k, v)
													   End Sub)

								   Return KVs
							   End Function).
							   Where(Function(x) x.NotEmpty).
							   ToList
		End Function

#End Region

	End Class
End Namespace
