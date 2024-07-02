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
' 	重复字段的表达式
'
' 	name: Model.DuplicatedFields
' 	create: 2023-02-20
' 	memo: 重复字段的表达式
' 	
' ------------------------------------------------------------

Imports System.Linq.Expressions
Imports FreeSql

Namespace Model

	''' <summary>重复字段的表达式</summary>
	Public Class DuplicatedFields(Of T As {IEntity, Class})

		Private ReadOnly _Instance As New List(Of DuplicatedExpression(Of T))

		''' <summary>创建表达式</summary>
		Public Function Insert(exps As DuplicatedExpression(Of T)) As DuplicatedFields(Of T)
			If exps IsNot Nothing AndAlso exps.HasExpression AndAlso Not _Instance.Contains(exps) Then _Instance.Add(exps)
			Return Me
		End Function

		''' <summary>创建表达式</summary>
		Public Function Insert(exp As Expression(Of Func(Of T, Object))) As DuplicatedFields(Of T)
			Dim exps As New DuplicatedExpression(Of T)
			exps.Insert(exp)

			Return Insert(exps)
		End Function

		''' <summary>创建表达式</summary>
		Public Function Insert(ParamArray expArr As Expression(Of Func(Of T, Object))()) As DuplicatedFields(Of T)
			Dim exps As New DuplicatedExpression(Of T)
			For Each exp In expArr
				exps.Insert(exp)
			Next

			Return Insert(exps)
		End Function

		''' <summary>获取组合表达式</summary>
		Public Function MakeQuerys(db As IFreeSql, obj As T) As Dictionary(Of String, ISelect(Of T))
			Dim Ret As New Dictionary(Of String, ISelect(Of T))

			For Each exp In _Instance
				Dim res = exp.MakeQuery(db, obj)
				If res.Name.NotEmpty AndAlso Not Ret.ContainsKey(res.Name) Then
					Ret.Add(res.Name, res.Query)
				End If
			Next

			Return Ret
		End Function
	End Class

End Namespace