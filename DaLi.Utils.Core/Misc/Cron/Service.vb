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
' 	Cron 验证周期服务
'
' 	name: Cron.Service
' 	create: 2022-09-30
' 	memo: Cron 验证周期服务
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Threading

Namespace Misc.Cron

	''' <summary>Cron 验证周期服务</summary>
	Public NotInheritable Class Service

		''' <summary>时间记录器</summary>
		Private Shared ReadOnly _Instance As New ConcurrentDictionary(Of String, (Exps As String(), Last As Date, TimeUp As Boolean, onlyDay As Boolean))(StringComparer.OrdinalIgnoreCase)

		''' <summary>是否初始化</summary>
		Private Shared _Init As Boolean = False

		''' <summary>时间检查服务</summary>
		Public Shared Sub Start()
			If _Init Then Return
			_Init = True

			Task.Run(Sub()
						 Dim s As New Stopwatch

						 While True
							 s.Restart()

							 Parallel.ForEach(_Instance.Keys,
											  New ParallelOptions With {.MaxDegreeOfParallelism = 25},
											  Sub(key)
												  Dim Value = _Instance(key)
												  If Not Value.TimeUp Then
													  ' 时间未到的需要检查，一旦成功则不再检查
													  If Expression.Timeup(Value.Exps, SYS_NOW_DATE, Value.Last, Value.onlyDay) Then
														  Value.TimeUp = True
														  Value.Last = SYS_NOW_DATE
														  _Instance(key) = Value

														  'CON.Warn({key, Value.Exps.JoinString, SYS_NOW_DATE, Value.Last})
													  End If
												  End If
											  End Sub)

							 s.Stop()

							 Dim timelong = s.ElapsedMilliseconds
							 If timelong < 1000 Then Thread.Sleep(999 - timelong)
						 End While

						 _Init = False
					 End Sub)
		End Sub

		''' <summary>注册表达式</summary>
		''' <param name="id">唯一标识</param>
		''' <param name="exp">Cron 表达式</param>
		Public Shared Sub Register(id As String, exp As String, Optional onlyDay As Boolean = False)
			If id.IsEmpty OrElse exp.IsEmpty Then Return

			' 分析表达
			Dim exps = Expression.Update(exp.SplitEx({"|", vbCrLf}), onlyDay)
			If exps.IsEmpty Then Return

			' 启动服务
			Call Start()

			' 记录数据
			If _Instance.ContainsKey(id) Then
				Dim val = _Instance(id)
				val.Exps = exps
				val.onlyDay = onlyDay
				val.TimeUp = False

				_Instance(id) = val
			Else
				_Instance.TryAdd(id, (exps, New Date, False, onlyDay))
			End If
		End Sub

		''' <summary>时间是否到期，不存在则自动添加</summary>
		''' <param name="id">唯一标识</param>
		''' <param name="lastTime">最后操作时间，如果大于2000-1-1则表示有效，如果当前时间与最后时间秒相等，表示执行过任务，所以无需再比较。</param>
		Public Shared Function TimeUp(id As String, Optional lastTime As Date = Nothing) As Boolean
			Dim isUp = False

			If _Instance.ContainsKey(id) Then
				isUp = _Instance(id).TimeUp

				Dim val = _Instance(id)
				val.Last = If(lastTime.IsValidate, lastTime, SYS_NOW_DATE)
				val.TimeUp = False

				_Instance(id) = val
			End If

			Return isUp
		End Function

		''' <summary>注册表达式</summary>
		''' <param name="id">唯一标识</param>
		Public Shared Function Item(id As String) As (Exps As String, Last As Date, TimeUp As Boolean, onlyDay As Boolean)
			If id.NotEmpty AndAlso _Instance.ContainsKey(id) Then
				Dim ret = _Instance(id)
				Return (String.Join("|"c, ret.Exps), ret.Last, ret.TimeUp, ret.onlyDay)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>注册表达式</summary>
		''' <param name="id">唯一标识</param>
		Public Shared Function ItemDescript(id As String) As String
			If id.NotEmpty AndAlso _Instance.ContainsKey(id) Then
				Dim ret = _Instance(id)
				Return Expression.Description(ret.Exps, ret.onlyDay)
			Else
				Return Nothing
			End If
		End Function

	End Class
End Namespace