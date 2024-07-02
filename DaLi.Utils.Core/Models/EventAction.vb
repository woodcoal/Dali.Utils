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
' 	事件总线操作模型
'
' 	name: Model.EventAction
' 	create: 2023-02-16
' 	memo: 事件总线操作模型
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>事件总线操作模型</summary>
	Public Class EventAction

		''' <summary>结果结构</summary>
		Public Class ActionResult
#Region "属性"
			''' <summary>是否异步任务</summary>
			Private _Async As Boolean

			''' <summary>是否异步任务</summary>
			Public Property Async As Boolean
				Get
					Return _Async
				End Get
				Private Set(value As Boolean)
					_Async = value
				End Set
			End Property

			''' <summary>是否正在执行</summary>
			Private _Busy As Boolean

			''' <summary>是否正在执行</summary>
			Public Property Busy As Boolean
				Get
					Return _Busy
				End Get
				Private Set(value As Boolean)
					_Busy = value
				End Set
			End Property

			''' <summary>是否成功</summary>
			Private _Success As Boolean

			''' <summary>是否成功</summary>
			Public Property Success As Boolean
				Get
					Return _Success
				End Get
				Private Set(value As Boolean)
					_Success = value
				End Set
			End Property

			''' <summary>执行次数</summary>
			Private _Count As Integer

			''' <summary>执行次数</summary>
			Public Property Count As Integer
				Get
					Return _Count
				End Get
				Private Set(value As Integer)
					_Count = value
				End Set
			End Property

			''' <summary>最后操作时间</summary>
			Private _Last As Date

			''' <summary>最后操作时间</summary>
			Public Property Last As Date
				Get
					Return _Last
				End Get
				Private Set(value As Date)
					_Last = value
				End Set
			End Property

			''' <summary>最后执行任务时长</summary>
			Private _Duration As Long

			''' <summary>最后执行任务时长</summary>
			Public Property Duration As Long
				Get
					Return _Duration
				End Get
				Private Set(value As Long)
					_Duration = value
				End Set
			End Property

			''' <summary>累积时长</summary>
			Private _TotalTime As Double

			''' <summary>累积时长</summary>
			Public Property TotalTime As Double
				Get
					Return _TotalTime
				End Get
				Private Set(value As Double)
					_TotalTime = value
				End Set
			End Property

			''' <summary>最后执行结果</summary>
			Private _Result As Object

			''' <summary>最后执行结果</summary>
			Public Property Result As Object
				Get
					Return _Result
				End Get
				Private Set(value As Object)
					_Result = value
				End Set
			End Property

			''' <summary>累计消息记录</summary>
			Public ReadOnly Property Message As List(Of String)

#End Region

			''' <summary>构造</summary>
			Public Sub New(Optional isAsync As Boolean = False)
				Async = isAsync
				Result = Nothing
				Success = False
				Busy = False
				Count = 0
				Last = New Date
				Duration = 0
				TotalTime = 0
				Result = Nothing
				Message = New List(Of String)
			End Sub

			''' <summary>设置执行结果</summary>
			Public Sub SetFnish(success As Boolean, result As Object, Optional message As String = "")
				SyncLock Me.Message
					Me.Success = success
					Me.Result = result
					Me.Message.Add(message)

					Count += 1
					Duration = SYS_NOW_DATE.Subtract(Last).Ticks
					TotalTime += Duration

					' 最后时间更新须在最后，以便计算时长
					Last = SYS_NOW_DATE
					Busy = False
				End SyncLock
			End Sub

			''' <summary>设置执行状态</summary>
			Public Sub SetStart()
				Busy = True
				Last = SYS_NOW_DATE
			End Sub
		End Class

		''' <summary>执行事件</summary>
		Public ReadOnly Execute As Action(Of Object)

		''' <summary>HASH</summary>
		Protected ReadOnly Hash As Integer

		''' <summary>执行结果</summary>
		Public ReadOnly Result As ActionResult

		''' <summary>构造</summary>
		''' <param name="delay">执行方式。0：收到请求立即执行；大于0：防抖，时间到后才执行；小于0：防抖，立即执行；单位：毫秒</param>
		Public Sub New(action As [Delegate], Optional delay As Integer = 0)
			If action Is Nothing Then Throw New Exception("委托事件无效")

			Hash = action.GetHashCode
			Result = New ActionResult(delay <> 0)

			Dim Execute = Sub(params)
							  If Result.Busy Then Return

							  Dim success = False
							  Dim message = ""
							  Dim ret = Nothing


							  Result.SetStart()

							  Try
								  Dim paramValue As Object() = Nothing
								  Dim paramInfos = action.Method.GetParameters
								  Dim paramCount = paramInfos.Length

								  If params IsNot Nothing Then
									  Dim argsType = params.GetType

									  If argsType.IsArray Then
										  paramValue = params
									  ElseIf GetType(List(Of Object)).IsAssignableFrom(argsType) Then
										  ' 对于列表对象换成数组
										  paramValue = TryCast(params, List(Of Object)).ToArray
									  Else
										  paramValue = {params}
									  End If

									  ' 参数数量不一致，不执行
									  If paramValue.Length <> paramCount Then
										  message = "参数数量不一致，无法执行"
										  Exit Try
									  End If

									  ' 调整类型
									  For I = 0 To paramCount - 1
										  paramValue(I) = ChangeType(paramValue(I), paramInfos(I).ParameterType)
									  Next
								  End If

								  ret = action.Method.Invoke(action.Target, paramValue)
								  success = True
							  Catch ex As Exception
								  message = "执行异常：" & ex.Message
							  End Try

							  Result.SetFnish(success, ret, message)
						  End Sub

			' 防抖
			If delay <> 0 Then
				Me.Execute = Debounce(Sub(x) Execute(x), Math.Abs(delay), delay < 0)
			Else
				Me.Execute = Execute
			End If
		End Sub

		''' <summary>是否与当前内置事件为相同事件</summary>
		Public Overrides Function Equals(obj As Object) As Boolean
			Return Hash.Equals(obj.GetHashCode)
		End Function

		Public Overrides Function GetHashCode() As Integer
			Return Hash
		End Function

	End Class
End Namespace
