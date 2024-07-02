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
' 	Redis 模块信息
'
' 	name: RedisModule
' 	create: 2024-06-24
' 	memo: Redis 模块信息
'
' ------------------------------------------------------------

'1) name
'   2) ReJSON
'   3) ver
'   4) 20610
'   5) path
'   6) /opt/redis-stack/lib/rejson.so
'   7) args
'   8) (empty array)
'4) 1) name
'   2) search
'   3) ver
'   4) 20814
'   5) path
'   6) /opt/redis-stack/lib/redisearch.so
'   7) args
'   8) 1) MAXSEARCHRESULTS
'      2) 10000
'      3) MAXAGGREGATERESULTS
'      4) 10000



''' <summary>Redis 模块信息</summary>
Public Class RedisModule

	''' <summary>模块名称</summary>
	Public Property Name As String

	''' <summary>模块版本</summary>
	Public Property Ver As Long

	''' <summary>模块路径</summary>
	Public Property Path As String

	''' <summary>模块参数</summary>
	Public Property Args As KeyValueDictionary

	Public Sub New()
	End Sub

	Public Sub New(data As Object)
		Dim values = TryCast(data, Object())
		If values.IsEmpty Then Return

		For I = 0 To values.Length - 2 Step 2
			Dim name = values(I)?.ToString
			Dim value = values(I + 1)
			If name.IsEmpty OrElse value Is Nothing Then Continue For

			Select Case name.ToLower
				Case "name"
					Me.Name = value.ToString

				Case "ver"
					Ver = value.ToString.ToLong

				Case "path"
					Path = value.ToString

				Case "args"
					Dim pars = TryCast(value, Object())
					If pars.IsEmpty Then Continue For

					Args = New KeyValueDictionary
					For J = 0 To pars.Length - 2 Step 2
						Dim parName = pars(J)?.ToString
						Dim parValue = pars(J + 1)

						If parName.IsEmpty OrElse parValue Is Nothing Then Continue For
						Args.Add(parName, parValue)

					Next
			End Select
		Next
	End Sub

	''' <summary>批量转换</summary>
	Public Shared Function Convert(data As Object) As RedisModule()
		Return TryCast(data, Object())?.
			Select(Function(x) New RedisModule(x)).
			Where(Function(x) x IsNot Nothing).
			ToArray
	End Function

End Class
