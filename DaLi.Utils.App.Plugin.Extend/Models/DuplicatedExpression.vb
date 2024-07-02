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
' 	字段表达式
'
' 	name: Model.DuplicatedExpression
' 	create: 2023-02-20
' 	memo: 字段表达式
' 	
' ------------------------------------------------------------

Imports System.Linq.Expressions
Imports FreeSql

Namespace Model

	''' <summary>字段表达式</summary>
	Public Class DuplicatedExpression(Of T As {IEntity, Class})

		''' <summary>表达式集合</summary>
		Private ReadOnly _Instance As New List(Of Expression(Of Func(Of T, Object)))

		''' <summary>创建表达式</summary>
		Public Sub Insert(exp As Expression(Of Func(Of T, Object)))
			If exp IsNot Nothing AndAlso Not _Instance.Contains(exp) Then _Instance.Add(exp)
		End Sub

		''' <summary>创建表达式</summary>
		Public Sub Insert(ParamArray exps() As Expression(Of Func(Of T, Object)))
			For Each exp In exps
				Insert(exp)
			Next
		End Sub

		''' <summary>是否存在表达式</summary>
		Public ReadOnly Property HasExpression As Boolean
			Get
				Return _Instance.Count > 0
			End Get
		End Property

		''' <summary>获取组合表达式</summary>
		Public Function MakeQuery(db As IFreeSql, obj As T) As (Query As ISelect(Of T), Name As String)
			If db Is Nothing OrElse obj Is Nothing OrElse Not HasExpression Then Return Nothing

			' 已经添加全局过滤器，无需 HiddenFilter
			Dim query = db.Select(Of T).WhereNotEquals(obj.ID, Function(x) x.ID)
			'Dim query = db.Select(Of T).HiddenFilter.WhereNotEquals(obj.ID, Function(x) x.ID)
			Dim names As New List(Of String)

			For Each exp In _Instance
				Dim v = exp.Compile.Invoke(obj)
				If v IsNot Nothing Then
					' 当检查表达式项目只有一条，且当前对象的值：文本为空则不进行检测
					If _Instance.Count = 1 AndAlso v.GetType.GetTypeCode = TypeCode.String AndAlso v.ToString.IsEmpty Then Continue For

					' 继续检测
					Dim e = TryCast(exp.Body, UnaryExpression)
					If e IsNot Nothing Then
						Dim p = TryCast(e.Operand, MemberExpression)
						Dim name = p?.Member.Name
						If name.NotEmpty AndAlso Not names.Contains(name) Then
							' 名称重复不操作
							names.Add(name)

							' 对于 nullabled 的处理
							Dim equExp = e.Operand
							If equExp.Type.IsNullable Then equExp = Expression.PropertyOrField(equExp, "Value")

							Dim equal = Expression.Equal(equExp, Expression.Constant(v))
							Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, exp.Parameters(0))

							query = query.Where(where)
						End If
					End If
				End If
			Next

			If names.Count > 0 Then
				Return (query, names.JoinString(","))
			Else
				Return Nothing
			End If
		End Function

	End Class

End Namespace