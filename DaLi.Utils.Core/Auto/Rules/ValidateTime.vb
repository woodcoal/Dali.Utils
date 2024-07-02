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
' 	时间验证
'
' 	name: Auto.Rule.ValidateTime
' 	create: 2023-01-03
' 	memo: 时间验证
'
' ------------------------------------------------------------

Namespace Auto.Rule
	''' <summary>时间验证</summary>
	Public Class ValidateTime
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "时间验证"
			End Get
		End Property

		''' <summary>原始日期数据，不设置为当前时间</summary>
		Public Property Source As String

		''' <summary>开始时间 (HH:mm:ss)</summary>
		Public Property TimeStart As String

		''' <summary>结束时间 (HH:mm:ss)</summary>
		Public Property TimeEnd As String

		''' <summary>触发异常。True：匹配成功异常； False：匹配失败异常； Default：忽略，成功或失败都不触发</summary>
		Public Property ThrowValue As TristateEnum

		''' <summary>为匹配成功时输出的内容</summary>
		Public Property SuccValue As String

		''' <summary>为匹配失败时输出的内容</summary>
		Public Property FailValue As String

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "起始时间未设置"
			If TimeStart.IsEmpty AndAlso TimeEnd.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			' 用于比较的时间
			Dim dateNow = SYS_NOW_DATE
			If Source.NotEmpty Then dateNow = AutoHelper.GetVarString(Source, data)

			message.Message = "用于验证的时间无效"
			If Not dateNow.IsValidate Then Return False

			' 分析
			Dim timeS = $"{dateNow:yyyy-MM-dd} {TimeStart}".Trim.ToDateTime
			Dim timeE = $"{dateNow:yyyy-MM-dd} {TimeEnd}".Trim.ToDateTime

			' 验证
			Dim Ret = dateNow.IsTimeWork(timeS, timeE)

			Select Case ThrowValue
				Case TristateEnum.TRUE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("禁止在指定时间内").FormatTemplate(data, True)
					If Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

				Case TristateEnum.FALSE
					ErrorIgnore = False
					Dim Err = ErrorMessage.EmptyValue("未到指定时间").FormatTemplate(data, True)
					If Not Ret Then Throw New AutoException(ExceptionEnum.VALUE_VALIDATE, message, Err)

			End Select

			message.SetSuccess()

			If Ret Then
				Return If(SuccValue.NotEmpty, AutoHelper.GetVar(SuccValue, data), True)
			Else
				Return If(FailValue.NotEmpty, AutoHelper.GetVar(FailValue, data), False)
			End If
		End Function

#End Region

	End Class
End Namespace