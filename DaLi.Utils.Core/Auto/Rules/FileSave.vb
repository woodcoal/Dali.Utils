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
' 	文件保存
'
' 	name: Auto.Rule.FileSave
' 	create: 2023-01-03
' 	memo: 文件读取
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>文件保存</summary>
	Public Class FileSave
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "文件保存"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Path As String

		''' <summary>内容，不设置则使用上下文内容，设置为 _Delete 将移除文件</summary>
		Public Property Content As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "文件路径未设置"
			If Path.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			message.Message = "路径不能为空"
			Dim filePath = AutoHelper.GetVarString(Path, data)
			If filePath.IsEmpty Then Return Nothing

			Content = AutoHelper.GetVarString(Content, data)

			Dim ret As Boolean
			If Content.IsEmpty OrElse Content.IsSame("_Delete") Then
				ret = PathHelper.FileRemove(filePath)
				If Not ret Then message.Message = "文件不存在或者删除失败"
			Else
				ret = PathHelper.FileSave(filePath, Content)
				If Not ret Then message.Message = "文件保存失败"
			End If

			message.SetSuccess(ret)
			Return ret
		End Function

#End Region
	End Class
End Namespace