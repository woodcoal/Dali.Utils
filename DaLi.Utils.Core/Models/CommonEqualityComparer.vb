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
' 	通用比较接口
'
' 	name: Model.CommonEqualityComparer
' 	create: 2020-12-08
' 	memo: 通用比较接口
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>通用比较接口</summary>
	Public Class CommonEqualityComparer(Of T, V)
		Implements IEqualityComparer(Of T)

		Private ReadOnly _KeySelector As Func(Of T, V)

		Public Sub New(keySelector As Func(Of T, V))
			_KeySelector = keySelector
		End Sub

		Public Overloads Function Equals(x As T, y As T) As Boolean Implements IEqualityComparer(Of T).Equals
			Return EqualityComparer(Of V).Default.Equals(_KeySelector(x), _KeySelector(y))
		End Function

		Public Overloads Function GetHashCode(obj As T) As Integer Implements IEqualityComparer(Of T).GetHashCode
			Return EqualityComparer(Of V).Default.GetHashCode(_KeySelector(obj))
		End Function

	End Class
End Namespace