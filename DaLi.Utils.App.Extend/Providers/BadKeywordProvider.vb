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
'	非法关键词处理
'
' 	name: Provider.BadKeywordProvider
' 	create: 2023-02-19
' 	memo: 非法关键词处理
'
' ------------------------------------------------------------

Imports ToolGood.Words

Namespace Provider

	''' <summary>非法关键词处理</summary>
	Public Class BadKeywordProvider
		Implements IBadKeywordProvider

		''' <summary>用户检测</summary>
		Private ReadOnly _User As StringSearchEx

		''' <summary>关键词检测</summary>
		Private ReadOnly _Check As StringSearchEx

		''' <summary>替换关键词</summary>
		Private ReadOnly _Replace As StringSearchEx

		Public Sub New()
			_User = New StringSearchEx
			_Check = New StringSearchEx
			_Replace = New StringSearchEx

			_User.SetKeywords(Array.Empty(Of String))
			_Check.SetKeywords(Array.Empty(Of String))
			_Replace.SetKeywords(Array.Empty(Of String))

			' 加载参数
			SetBadKeyword(SYS.GetSetting(Of KeywordSetting))
		End Sub

		''' <summary>设置关键词</summary>
		Public Sub SetBadKeyword(keywords As KeywordSetting)
			If keywords IsNot Nothing Then
				If keywords.UserName.NotEmpty Then _User.SetKeywords(keywords.UserName)
				If keywords.Checks.NotEmpty Then _Check.SetKeywords(keywords.Checks)

				If keywords.Replaces.NotEmpty Then _Replace.SetKeywords(keywords.Replaces)
				If keywords.ReplaceWithCheck AndAlso keywords.Checks.NotEmpty Then _Replace.SetKeywords(keywords.Checks)
			End If
		End Sub

		''' <summary>是否包含无效用户名</summary>
		Public Function BadUser(source As String) As Boolean Implements IBadKeywordProvider.BadUser
			Return _User.ContainsAny(source)
		End Function

		''' <summary>是否包含无效关键词</summary>
		Public Function Contains(source As String) As Boolean Implements IBadKeywordProvider.Contains
			Return _Check.ContainsAny(source)
		End Function

		''' <summary>替换关键词</summary>
		Public Function Replace(source As String) As String Implements IBadKeywordProvider.Replace
			Return _Replace.Replace(source)
		End Function

	End Class

End Namespace