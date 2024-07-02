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
' 	规则基类
'
' 	name: Auto.RuleBase
' 	create: 2022-12-30
' 	memo: 规则基类
'
' ------------------------------------------------------------

Namespace Auto
	''' <summary>规则基类</summary>
	Public MustInherit Class RuleBase
		Implements IRule

		''' <summary>类型</summary>
		Public Overridable ReadOnly Property Type As String = [GetType].Name Implements IRule.Type

		''' <summary>模块名称</summary>
		Public MustOverride ReadOnly Property Name As String Implements IRule.Name

		''' <summary>默认输出字段</summary>
		Public Property Output As String Implements IRule.Output

		''' <summary>对于值已经处理包含变量或者对于包含子模块的项目是否将子模块的执行结果一并输出到主流程，如果不含子流程则不用设置此值</summary>
		''' <remarks>如：计时器。True 则将计时器的结果与计时器内的模块值都输出到主流程 ，False 则只返回计时器的结果；如：http 下载，执行后已经将变量结果输出到结果中则无需再次处理</remarks>
		Public Overridable ReadOnly Property OutResult As String Implements IRule.OutResult
			Get
				Return False
			End Get
		End Property

		''' <summary>忽略错误</summary>
		Public Property ErrorIgnore As Boolean Implements IRule.ErrorIgnore

		''' <summary>是否忽略无结果，True：当未分析到任何内容时报错；False 当未分析到任何你内容时忽略此问题</summary>
		Public Property EmptyIgnore As Boolean Implements IRule.EmptyIgnore

		''' <summary>友好错误信息</summary>
		Public Property ErrorMessage As String Implements IRule.ErrorMessage

		''' <summary>启用</summary>
		Public Property Enabled As Boolean Implements IRule.Enabled

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected MustOverride Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Public Function Execute(data As IDictionary(Of String, Object)) As AutoMessage Implements IRule.Execute
			Dim message As New AutoMessage(Me)
			If data Is Nothing OrElse TypeOf data IsNot SafeKeyValueDictionary Then data = New SafeKeyValueDictionary(data)

			' 禁用不执行
			message.Message = "规则已经被禁用"
			If Not Enabled Then Return Nothing

			' 检查规则是否异常
			If Not Validate(message.Message) Then Throw New Exception($"规则[{Name}]验证失败：{message.Message}")

			' 执行操作
			Try
				message.Result = ExecuteRule(data, message)

				' 校验异常 
				If Not message.Success Then
					' 调试模式不返回异常，结果清空
					If AutoHelper.IsDebug(data) Then
						If message.Message.IsEmpty Then message.Message = "规则执行失败！"
						message.Result = Nothing
						Return message
					Else
						Throw New AutoException(ExceptionEnum.EXECUTE_ERROR, message, message.Message.EmptyValue("规则执行失败"))
					End If
				End If

				' 需要返回变量值，但是返回空，需要报异常
				If message.Result Is Nothing AndAlso Not EmptyIgnore AndAlso Output.NotEmpty Then Throw New AutoException(ExceptionEnum.NO_RESULT, message)

			Catch ex As AutoException
				' 操作指令异常，不拦截
				message.SetSuccess(False, $"规则[{Name}]中断：" & ex.Message)
				Throw

			Catch ex As Exception
				Dim errMessage = ErrorMessage.EmptyValue(ex.Message).FormatTemplate(data, True)
				message.SetSuccess(False, $"规则[{Name}]执行异常：" & errMessage)

				' 禁止忽略错误，继续产生异常
				If Not ErrorIgnore Then Throw New AutoException(ExceptionEnum.INNER_EXCEPTION, message, ex.Message, ex)
			End Try

			Return message
		End Function

		''' <summary>克隆</summary>
		Public Overridable Function Clone() As Object Implements IRule.Clone
			Return MemberwiseClone()
		End Function

		''' <summary>验证规则是否存在异常</summary>
		Public Overridable Function Validate(Optional ByRef message As String = Nothing) As Boolean Implements IRule.Validate
			message = Nothing
			Return True
		End Function

	End Class
End Namespace