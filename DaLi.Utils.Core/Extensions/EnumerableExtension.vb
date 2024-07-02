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
' 	集合扩展操作
'
' 	name: Extension.EnumerableExtension
' 	create: 2020-12-08
' 	memo: 集合扩展操作
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>集合扩展操作</summary>
	Public Module EnumerableExtension

		''' <summary>转换成字符串名值字典数据</summary>
		<Extension()>
		Public Function ToNameValueDictionary(Of T)(this As IEnumerable(Of T), keySelector As Func(Of T, String), elementSelector As Func(Of T, String)) As NameValueDictionary
			Dim Ret = New NameValueDictionary

			Dim Dic = this.ToDictionary(keySelector, elementSelector, StringComparer.OrdinalIgnoreCase)
			If Dic.NotEmpty Then Ret.AddRange(Dic)

			Return Ret
		End Function

		''' <summary>移除重复数据</summary>
		<Extension()>
		Public Function Distinct(Of T, V)(this As IEnumerable(Of T), keySelector As Func(Of T, V)) As IEnumerable(Of T)
			If this IsNot Nothing AndAlso keySelector IsNot Nothing Then
				Return this.Distinct(New CommonEqualityComparer(Of T, V)(keySelector))
			Else
				Return Nothing
			End If
		End Function

		''' <summary>按固定长度分割对象列表</summary>
		''' <param name="count">切割长度</param>
		<Extension()>
		Public Function Split(Of T)(this As IEnumerable(Of T), count As Integer) As List(Of List(Of T))
			If this Is Nothing Then Return Nothing

			If count < 1 Then count = 1
			Dim all = this.Count
			Dim max = Math.Ceiling(all / count) - 1

			Dim ret As New List(Of List(Of T))
			For I = 0 To max
				ret.Add(this.Skip(I * count).Take(count).ToList)
			Next
			Return ret
		End Function

	End Module
End Namespace