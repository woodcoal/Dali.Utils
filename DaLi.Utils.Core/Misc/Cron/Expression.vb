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
' 	Cron 周期表达式
'
' 	name: Cron.Expression
' 	create: 2022-09-30
' 	memo: Cron 周期表达式
'
' ------------------------------------------------------------

Namespace Misc.Cron
	''' <summary>Cron 表达式，支持常规 Cron 格式，与具体时间集合（逗号间隔）</summary>
	''' <remarks>定时规则为 Cron 表达式，为了适应本地的习惯，这里进行了部分修改扩展。什么是 Cron 表达式，具体格式请自行百度，此处仅作简要说明：
	'''		１. 标准 Cron 表达式分为７段，每段用空格间隔，分别代表：秒，分，时，日，月，周，年；前三段表示时刻，后四段表示日期；如果年不设置则可以省略为６段
	'''		２. 每段不设置可以用星号（*）替代，但是不能为空。
	'''		３. 如果无需定时可以直接使用一个星号，表示全时段都适配。
	'''		４. 为了适应本地的习惯，星期中1~7分别代表周一到周日，而非原系统中的，0代表周日；
	'''		５. 原 Cron 中可以使用星期与月份英文缩写，此扩展表达式无效，请直接使用数字表示；
	'''		
	'''		【具体说明】
	'''		１. 标准通配符：所有字段都支持
	'''		　　逗号（,） 指定数字集合。 1,4,7 代表：1、4、7
	'''		　　斜线（/） 间隔数据，从第一个数字开始，每隔第二个数字重复（0为无效间隔，负数则倒计数）。  3/5 从3开始，每隔5：3、8、13、18……；55/-10 表示从55开始倒数，含55、45、35、25、15、5
	'''		　　横线（-） 代表区间，一段数字集合。 3-6 代表：3、4、5、6
	'''		　　星号（*） 忽略此值，此段值不限
	'''		  
	'''		２. 特殊通配符：
	'''		　　问号（?） 仅用于日与星期字段，且只能出现一次。比如：每月 XX 日则星期字段要使用?；而每月星期 xx 则日要使用?；如果日与星期都设置了数字，则日与星期都需要匹配才有效。
	'''		  
	'''		３. 日期字段专用符号
	'''		　　L 倒计数　用于日期字段时：表示月最后 xx 天，2L 表示月倒数第二天，9L 月倒数第９天；用于星期时：表示月最后一个星期几，1L表示月最后一个星期１；日期与星期的L不要混用，也不要在时刻段上使用L，否则出现异常
	'''		
	'''		４. 原通配符调整：
	'''		　　W 工作日　用于日期字段：表示本月的工作日；用于星期字段，表示每周的工作日；工作日会根据指定的调休日期进行修改
	'''		　　LW 最后一个工作日
	'''		　　FW 第一个工作日
	'''		
	'''		　　R 休息日　用于日期字段：表示本月的节假日；用于星期字段，表示每周的节假日；休息日会根据指定的节假日期进行修改（注意此休息日包含周末，除非指定了调休）
	'''		　　FR 休息日前一天
	'''		　　BR 休息日第一天
	'''		　　ER 休息日最后一天
	'''		　　LR 休息日后的第一天
	'''
	'''		　　H 法定节假日	用于日期字段：表示本月的节假日；用于星期字段，表示每周的节假日；节假日会根据指定的节假日期进行修改（注意此节假日未包含周末）
	'''		　　FH 节假日前一天
	'''		　　BH 节假日第一天
	'''		　　EH 节假日最后一天
	'''		　　LH 节假日后的第一天	
	'''		
	'''		【增加部分】
	'''		在原 Cron６／７段模式下扩展了两种模式，不指定具体秒循环时间规则（１～３段结构）与具体时间列表的规则（４段结构）
	'''		１. 秒循环规则：从指定时间开始，每隔？秒执行一次。
	'''		　　１～３段，分别表示秒、起始日期、起始时刻；起始日期与时刻可以忽略不填写
	'''		　　秒字段：使用具体数值。大于０：表示从指定时间开始每隔此值循环匹配；小于０：表示每隔此值循环直到执行到指定时间；０：具体的时刻
	'''		　　日期、时刻段，填写具体时间，如果忽略不填则表示：正向（秒大于０）模式默认２０００年１月１日零点；反向（秒小于０）模式默认２０5０年１月１日零点
	'''		　　例如：5 2021-03-18 15:00:00 表示 从 2021年3月18日下午3点开始，每隔5秒循环；-30 2099-12-31 23:00:00 表示每隔30秒循环直到2099年12月31日晚上11点整
	'''		
	'''		２. 具体时间列表：和标准规则一样，前三段不变，分别表示秒、分、时。区别时第四段为具体日期，多个日期用逗号间隔
	'''		　　例如：30 1/3 15-20 2021-03-18,2022-12-31 表示 2021年3月18日与2022年12月31日两天，从下午3点到8点之间，第一分钟开始，每隔3分钟的第30秒匹配
	''' </remarks>
	Public Class Expression

		''' <summary>默认初始时间</summary>
		Private _DateDefault As IEnumerable(Of Date)

		''' <summary>时间模式</summary>
		Private _Mode As TimeModeEnum

		''' <summary>表达式字段</summary>
		Private ReadOnly _Exp(6) As String

		''' <summary>构造</summary>
		Public Sub New(Optional exp As String = "", Optional onlyDay As Boolean = False)
			UpdateExpression(exp, onlyDay)
		End Sub

		''' <summary>获取描述</summary>
		Public Shared ReadOnly Property Description(exp As String, Optional onlyDay As Boolean = False) As String
			Get
				Return New Expression(exp, onlyDay).Description
			End Get
		End Property

		''' <summary>验证当前时间是否有效</summary>
		Public Shared Function Timeup(exp As String, dateNow As Date, Optional lastTime As Date? = Nothing, Optional onlyDay As Boolean = False) As Boolean
			Return New Expression(exp, onlyDay).Timeup(dateNow, lastTime)
		End Function

		''' <summary>检查表达式，并更新为正确格式</summary>
		''' <param name="exp">Cron 表达式</param>
		''' <param name="onlyDay">只有日期部分，时分秒未设置</param>
		Public Shared Function Update(exp As String, Optional onlyDay As Boolean = False, Optional ByRef message As String = "") As String
			Return New Expression().UpdateExpression(exp, onlyDay, message)
		End Function

		''' <summary>获取描述</summary>
		Public Shared ReadOnly Property Description(exps As IEnumerable(Of String), Optional onlyDay As Boolean = False) As String
			Get
				If exps.IsEmpty Then Return Nothing

				Return exps.Select(Function(x) Description(x, onlyDay)).Distinct.JoinString("；")
			End Get
		End Property

		''' <summary>验证当前时间是否有效</summary>
		Public Shared Function Timeup(exps As IEnumerable(Of String), dateNow As Date, Optional lastTime As Date? = Nothing, Optional onlyDay As Boolean = False) As Boolean
			If exps.IsEmpty Then Return Nothing

			Return exps.Any(Function(x) Timeup(x, dateNow, lastTime, onlyDay))
		End Function

		''' <summary>检查表达式，并更新为正确格式</summary>
		''' <param name="exps">Cron 表达式列表</param>
		''' <param name="onlyDay">只有日期部分，时分秒未设置</param>
		Public Shared Function Update(exps As IEnumerable(Of String), Optional onlyDay As Boolean = False) As String()
			If exps.IsEmpty Then Return Nothing

			Return exps.Select(Function(x) Update(x, onlyDay)).Distinct.ToArray
		End Function

#Region "更新"

		''' <summary>检查表达式，并更新为正确格式</summary>
		''' <param name="exp">Cron 表达式</param>
		''' <param name="onlyDay">只有日期部分，时分秒未设置</param>
		Public Function UpdateExpression(exp As String, Optional onlyDay As Boolean = False, Optional ByRef message As String = "") As String
			' 重置所有节点值
			For I = 0 To _Exp.Length - 1
				_Exp(I) = "*"
			Next

			_DateDefault = Nothing
			_Mode = If(onlyDay, TimeModeEnum.DAY, TimeModeEnum.STAND)
			message = ""

			exp = exp.TrimFull
			If exp.IsEmpty Then
				message = "无效表达式"
				Return "*"
			ElseIf exp = "*" Then
				Return "*"
			End If

			'' 如果只有日期则自动附加时分秒
			'If onlyDay Then exp = "* * * " & exp

			' 1/2/3 4/6/7 段，空格间隔
			Dim exps = exp.Split(" "c)

			' 对结构大于 5 作为标准结构，如果此时使用 onlyDay 则需要忽略前三段
			' 如果小于 5 段则当使用 onlyDay 时，需要增加前三段
			If onlyDay Then
				If exps.Length < 5 Then
					exps = ("* * * " & exp).Split(" "c)
				Else
					' 其他的将时分秒置 * ，防止检查
					exps(0) = "*"
					exps(1) = "*"
					exps(2) = "*"
				End If
			End If

			'-----------------------------
			' 1，2，3 三段方式（仅时分秒）
			'-----------------------------
			' １～３段，分别表示秒、起始日期、起始时刻；起始日期与时刻可以忽略不填写
			' 秒字段：使用具体数值。大于０：表示从指定时间开始每隔此值循环匹配；小于０：表示每隔此值循环直到执行到指定时间；０：具体的时刻
			' 日期、时刻段，填写具体时间，如果忽略不填则表示：正向（秒大于０）模式默认２０００年１月１日零点；反向（秒小于０）模式默认２０5０年１月１日零点
			' 例如：5 2021-03-18 15:00:00 表示 从 2021年3月18日下午3点开始，每隔5秒循环；-30 2099-12-31 23:00:00 表示每隔30秒循环直到2099年12月31日晚上11点整
			If exps.Length < 4 Then
				_Mode = TimeModeEnum.TIME

				' 补全长度
				ReDim Preserve exps(2)

				' 默认周期
				Dim interval = exps(0).ToInteger
				If exps(0).IsNumber Then
					_Exp(0) = exps(0)
				Else
					message = "(1 段) 非有效周期"
				End If

				' 默认起始时间
				Dim dateDefault = If(interval > 0, New Date(2020, 1, 1), New Date(3000, 1, 1))

				' 日期段
				If Not exps(1).IsDate Then
					If exps(1).NotEmpty Then message = "(2 段) 无效日期格式"
					exps(1) = dateDefault.ToString("yyyy-MM-dd")
				End If

				' 时间段
				If Not exps(2).IsTime Then
					If exps(1).NotEmpty Then message = "(3 段) 无效时间格式"
					exps(2) = "00:00:00"
				End If

				' 分析实际时间
				_DateDefault = New List(Of Date) From {
					$"{exps(1)} {exps(2)}".ToDateTime(dateDefault)
				}

				Return ToString()
			End If

			'-----------------
			' 标准时段方式分析
			'-----------------

			' 秒
			_Exp(0) = UpdateWildcard(exps(0), 0, 59)

			' 分
			_Exp(1) = UpdateWildcard(exps(1), 0, 59)

			' 时
			_Exp(2) = UpdateWildcard(exps(2), 0, 23)

			' 检查是否修改
			CheckError(_Exp, exps, 0, 2, message)

			' 4 段模式，4 段为具体日期列表，逗号间隔
			If exps.Length = 4 Then
				_DateDefault = exps(3).ToDateList

				' 存在有效的时间，表示规则正确，否则按标准 7 段处理
				If _DateDefault.NotEmpty Then Return ToString()
			End If

			'-----------------
			' 标准时段方式分析
			'-----------------
			' 补全不能存在的段
			ReDim Preserve exps(6)

			' 日
			_Exp(3) = exps(3).EmptyValue("*")

			' 月
			_Exp(4) = UpdateWildcard(exps(4), 1, 12)

			' 周
			_Exp(5) = exps(5).EmptyValue("*")

			' 年
			_Exp(6) = UpdateWildcard(exps(6), 2020, 3000)

			' ? 不能同时出现
			If _Exp(3) = "?" And _Exp(5) = "?" Then
				_Exp(3) = "*"
				_Exp(5) = "*"
			End If

			If _Exp(3) = "?" Or _Exp(3) = "*" Then
				' 只需要匹配周
				_Exp(5) = UpdateWeek(_Exp(5))
			ElseIf _Exp(5) = "?" Or _Exp(5) = "*" Then
				' 只需要匹配日
				_Exp(3) = UpdateDay(_Exp(3))
			Else
				' 周/日全匹配
				_Exp(3) = UpdateDay(_Exp(3))
				_Exp(5) = UpdateWeek(_Exp(5))
			End If

			' 检查是否修改
			CheckError(_Exp, exps, 3, 6, message)

			Return ToString()
		End Function

		''' <summary>检查字段是否修改过，修改过代表原数据格式不对</summary>
		Private Shared Sub CheckError(source As String(), target As String(), min As Integer, max As Integer, ByRef message As String)
			For I = min To max
				Dim s = source(I).NullValue().Replace(" ", "")
				Dim t = target(I).NullValue().Replace(" ", "")
				If s <> t Then message &= $"({I + 1} 段) 格式无效，"
			Next
		End Sub

		''' <summary>通用通配符更新(*/-,)</summary>
		''' <param name="hasChange">结果是否已经更新过，没有更新过表示为原值返回</param>
		''' <remarks>
		''' 逗号（,） 指定数字集合。 1,4,7 代表：1、4、7
		''' 横线（-） 代表区间，一段数字集合。 3-6 代表：3、4、5、6
		''' 斜线（/） 间隔数据，从第一个数字开始，每隔第二个数字重复（0为无效间隔，负数则倒计数）。  3/5 从3开始，每隔5：3、8、13、18……；55/-10 表示从55开始倒数，含55、45、35、25、15、5
		''' 星号（*） 忽略此值，此段值不限
		'''</remarks>
		Private Shared Function UpdateWildcard(s As String, min As Integer, max As Integer, Optional ByRef hasChange As Boolean = False) As String
			hasChange = True
			If s.IsEmpty OrElse s = "*" Then Return "*"

			' 逗号（,）
			If s.Contains(","c) Then
				Dim vals = s.ToIntegerList(True)
				Return If(vals.NotEmpty, vals.ToNumberString, "*")
			End If

			' 斜线（/）
			If s.Contains("/"c) Then
				Dim vals = s.Split("/"c)
				Dim vA = vals(0).ToInteger
				Dim vB = vals(1).ToInteger

				If vA < min Then vA = min
				If vA > max Then vA = max

				Return If(vB = 0, vA, $"{vA}/{vB}")
			End If

			' 横线（-）
			If s.Contains("-"c) Then
				Dim vals = s.Split("-"c)
				Dim vMin = vals(0).ToInteger
				Dim vMax = vals(1).ToInteger

				If vMin < min Then vMin = min
				If vMax < vMin Then vMax = vMin
				If vMax > max Then vMax = max

				' 实际最小值与最大值与规定的值一致表示全部数据有效
				' 比如 1-31 日 表示每日
				' 比如 1-7 周 表示整个周
				If vMin = min AndAlso vMax = max Then Return "*"

				Return If(vMax = vMin, vMin, $"{vMin}-{vMax}")
			End If

			' 表达式为最终数字
			If s.IsNumber Then
				Dim val = s.ToInteger
				If val > max Then val = max
				If val < min Then val = min

				Return val
			End If

			' 数据没有修改，原值返回
			hasChange = False
			Return s
		End Function

		''' <summary>日更新</summary>
		''' <remarks>
		'''日期字段专用符号
		''' L 倒计数　用于日期字段时：表示月最后 xx 天，2L 表示月倒数第二天，9L 月倒数第９天；用于星期时：表示月最后一个星期几，1L表示月最后一个星期１；日期与星期的L不要混用，也不要在时刻段上使用L，否则出现异常
		'''		
		'''原通配符调整：
		''' W 工作日　用于日期字段：表示本月的工作日；用于星期字段，表示每周的工作日；工作日会根据指定的调休日期进行修改
		''' LW 最后一个工作日
		''' FW 第一个工作日
		'''		
		''' R 休息日　用于日期字段：表示本月的节假日；用于星期字段，表示每周的节假日；休息日会根据指定的节假日期进行修改（注意此休息日包含周末，除非指定了调休）
		''' FR 休息日前一天
		''' BR 休息日第一天
		''' ER 休息日最后一天
		''' LR 休息日后的第一天
		'''
		''' H 法定节假日	用于日期字段：表示本月的节假日；用于星期字段，表示每周的节假日；节假日会根据指定的节假日期进行修改（注意此节假日未包含周末）
		''' FH 节假日前一天
		''' BH 节假日第一天
		''' EH 节假日最后一天
		''' LH 节假日后的第一天	
		'''</remarks>
		Private Shared Function UpdateDay(exp As String) As String
			Dim hasChange As Boolean
			Dim Ret = UpdateWildcard(exp, 1, 31, hasChange)
			If hasChange Then Return Ret

			' L 结束，月末最后一个周？
			Dim L = GetLastWildcard(exp)
			If L > 0 AndAlso L <= 31 Then Return If(L = 1, "L", $"{L}L")

			' W 工作日
			Dim W = GetWorkWildcard(exp)
			If W.HasValue Then
				Select Case W.Value
					Case 0
						Return "W"

					Case 1
						Return "FW"

					Case -1
						Return "LW"

					Case Else
						If Math.Abs(W.Value) < 31 Then Return $"{W}W"
				End Select
			End If

			' R 休息日
			If exp.EndsWith("R", StringComparison.OrdinalIgnoreCase) Then
				Select Case exp.ToUpper
					Case "R", "FR", "BR", "ER", "LR"
						Return exp.ToUpper
					Case Else
						Return "R"
				End Select
			End If

			' H 法定节假日
			If exp.EndsWith("H", StringComparison.OrdinalIgnoreCase) Then
				Select Case exp.ToUpper
					Case "H", "FH", "BH", "EH", "LH"
						Return exp.ToUpper
					Case Else
						Return "H"
				End Select
			End If

			Return "*"
		End Function

		''' <summary>星期更新</summary>
		Private Shared Function UpdateWeek(exp As String) As String
			Dim hasChange As Boolean
			Dim Ret = UpdateWildcard(exp, 1, 7, hasChange)
			If hasChange Then Return Ret

			' L 结束，月末最后一个周？
			Dim L = GetLastWildcard(exp)
			If L > 0 AndAlso L <= 7 Then Return If(L = 1, "L", $"{L}L")

			' W 工作日
			Dim W = GetWorkWildcard(exp)
			If W.HasValue Then
				Select Case W.Value
					Case 0
						Return "W"

					Case 1
						Return "FW"

					Case -1
						Return "LW"

					Case Else
						If Math.Abs(W.Value) < 8 Then Return $"{W}W"
				End Select
			End If

			Return "*"
		End Function

		''' <summary>获取 L 通配符，返回通配符前的数字</summary>
		Private Shared Function GetLastWildcard(exp As String) As Integer
			Dim R = 0

			If exp.EndsWith("L", StringComparison.OrdinalIgnoreCase) Then
				If exp.Length > 1 Then
					R = exp.Substring(0, exp.Length - 1).ToInteger
				Else
					R = 1
				End If
			End If

			Return R
		End Function

		''' <summary>获取 W 通配符，返回通配符前的数字，排除最后一个工作日</summary>
		Private Shared Function GetWorkWildcard(exp As String) As Integer?
			Dim R As Integer? = Nothing

			If exp.EndsWith("W", StringComparison.OrdinalIgnoreCase) Then
				If exp.Length > 1 Then
					exp = exp.Substring(0, exp.Length - 1)
					Select Case exp.ToLower
						Case "l"
							R = -1
						Case "f"
							R = 1
						Case Else
							R = exp.ToInteger
					End Select
				Else
					R = 0
				End If
			End If

			Return R
		End Function

#End Region

#Region "返回结果"

		''' <summary>生成字符串</summary>
		Public Overrides Function ToString() As String
			Select Case _Mode

				Case TimeModeEnum.TIME
					' 时间周期
					Return $"{_Exp(0)} {_DateDefault(0):yyyy-MM-dd HH:mm:ss}"

				Case Else
					' 固定时间模式
					If _DateDefault.NotEmpty Then
						Return $"{GetString(0, 2)} {_DateDefault.Select(Function(x) x.ToString("yyyy-MM-dd")).JoinString}"
					Else
						Return If(_Mode = TimeModeEnum.DAY, GetString(3, 6), GetString(0, 6))
					End If
			End Select
		End Function

		''' <summary>获取值</summary>
		Private Function GetString(min As Integer, max As Integer) As String
			With New Text.StringBuilder
				For I = min To max
					.Append(_Exp(I))
					.Append(" "c)
				Next

				Return .ToString.Trim
			End With
		End Function

#End Region

#Region "验证时间"

		''' <summary>验证当前时间是否有效</summary>
		''' <param name="dateNow">用于比较的时间</param>
		''' <param name="lastTime">最后操作时间，如果大于2020-1-1则表示有效，如果当前时间与最后时间秒相等，表示执行过任务，所以无需再比较。</param>
		Public Function Timeup(dateNow As Date, Optional lastTime As Date? = Nothing) As Boolean
			' 如果最后时间与当前时间秒相等则表示再这秒内执行过任务，不再操作
			If lastTime.IsValidate AndAlso EqualsSecond(dateNow, lastTime) Then Return False

			'--------------
			' 秒周期
			'--------------
			If _Mode = TimeModeEnum.TIME Then
				Dim dateDefault = _DateDefault(0)
				Dim interval = Convert.ToInt32(_Exp(0))

				' 指定时间
				If interval = 0 Then Return EqualsSecond(dateNow, dateDefault)

				' 正向间隔不能早于默认时间，反向间隔不能晚于默认时间
				If interval > 0 AndAlso dateNow < dateDefault Then Return False
				If interval < 0 AndAlso dateNow > dateDefault Then Return False


				' 因为使用 ticks 计算，所以需要优化下当前时间，防止带入微秒
				dateNow = dateNow.Date.AddHours(dateNow.Hour).AddMinutes(dateNow.Minute).AddSeconds(dateNow.Second)
				Dim delay = Math.Abs(dateNow.Subtract(dateDefault).TotalSeconds)
				interval = Math.Abs(interval)

				Return delay Mod interval = 0
			End If

			' 验证秒
			Dim inc = ValidateWildcard(_Exp(0), dateNow.Second, 0, 59)
			If Not inc Then Return False

			' 验证分
			inc = ValidateWildcard(_Exp(1), dateNow.Minute, 0, 59)
			If Not inc Then Return False

			' 验证时
			inc = ValidateWildcard(_Exp(2), dateNow.Hour, 0, 23)
			If Not inc Then Return False

			' 全部通过，比对日期
			dateNow = dateNow.Date

			' 指定日期模式
			If _DateDefault.NotEmpty Then Return _DateDefault.Any(Function(x) x = dateNow)

			' 正常模式（7 段）

			' 验证年（7段）
			inc = ValidateWildcard(_Exp(6), dateNow.Year, 2020, 3000)
			If Not inc Then Return False

			' 验证月
			inc = ValidateWildcard(_Exp(4), dateNow.Month, 1, 12)
			If Not inc Then Return False

			' 验证日与星期
			Try
				' ? 不能同时出现
				If _Exp(3) = "?" And _Exp(5) = "?" Then
					Return False

				ElseIf _Exp(3) = "?" Or _Exp(3) = "*" Then
					' 只需要匹配周
					Return ValidateWeek(_Exp(5), dateNow)

				ElseIf _Exp(5) = "?" Or _Exp(5) = "*" Then
					' 只需要匹配日
					Return ValidateDay(_Exp(3), dateNow)

				Else
					' 周/日全匹配
					Return ValidateDay(_Exp(3), dateNow) AndAlso ValidateWeek(_Exp(5), dateNow)
				End If
			Catch ex As Exception
			End Try

			Return False
		End Function

		''' <summary>单独日期验证</summary>
		Private Shared Function ValidateDay(exp As String, dateNow As Date) As Boolean
			Dim Day = dateNow.Day
			Dim DayLast = New Date(dateNow.Year, dateNow.Month, 1).AddMonths(1).AddDays(-1).Day

			' 默认验证
			Dim R = ValidateWildcard(exp, Day, 1, DayLast)
			If R Then Return True

			' L 结束
			Dim L = GetLastWildcard(exp)
			If L > 0 Then
				' 超过当前月的最后一天，以最后一天为准
				If L > DayLast Then L = DayLast

				Return Day = DayLast - L + 1
			End If

			' W 工作日
			Dim W = GetWorkWildcard(exp)
			If W IsNot Nothing Then
				Dim inc = False

				If W > 0 Then
					'--------------
					' 正向工作日
					'--------------
					For I = 1 To DayLast
						Dim work = New Date(dateNow.Year, dateNow.Month, I)

						' 是否工作日
						If Not IsRestday(work) Then
							W -= 1
							If W < 1 Then
								' 第 W 个工作日
								inc = work.Day = Day
								Exit For
							End If
						End If
					Next

				ElseIf W < 0 Then
					'--------------
					' 反向工作日
					'--------------
					For I = DayLast To 1 Step -1
						Dim work = New Date(dateNow.Year, dateNow.Month, I)

						' 是否工作日
						If Not IsRestday(work) Then
							W += 1
							If W > -1 Then
								' 倒数第 W 个工作日
								inc = work.Day = Day
								Exit For
							End If
						End If
					Next
				Else
					'--------------
					' 所有工作日
					'--------------
					inc = Not IsRestday(dateNow)
				End If

				Return inc
			End If

			' R 休息日
			If exp.EndsWith("R", StringComparison.OrdinalIgnoreCase) Then
				Dim inc = False

				Select Case exp.ToUpper
					Case "R"
						' 所有假期
						inc = IsRestday(dateNow)

					Case "FR"
						' FR 休息日前一天
						inc = IsBeforeRestday(dateNow)

					Case "BR"
						' BR 休息日第一天
						inc = IsFirstRestday(dateNow)

					Case "ER"
						' ER 休息日最后一天
						inc = IsLastRestday(dateNow)

					Case "LR"
						' LR 休息日后的第一天
						inc = IsAfterRestday(dateNow)
				End Select

				Return inc
			End If

			' H 法定节假日
			If exp.EndsWith("H", StringComparison.OrdinalIgnoreCase) Then
				Dim inc = False

				Select Case exp.ToUpper
					Case "H"
						' 所有假期
						inc = IsHoliday(dateNow)

					Case "FH"
						' FH 节假日前一天
						inc = IsBeforeHoliday(dateNow)

					Case "BH"
						' BH 节假日第一天
						inc = IsFirstHoliday(dateNow)

					Case "EH"
						' EH 节假日最后一天
						inc = IsLastHoliday(dateNow)

					Case "LH"
						' LH 节假日后的第一天
						inc = IsAfterHoliday(dateNow)
				End Select

				Return inc
			End If

			Return False
		End Function

		''' <summary>单独星期验证</summary>
		Private Shared Function ValidateWeek(exp As String, dateNow As Date) As Boolean
			Dim Week = dateNow.DayOfWeek
			If Week = 0 Then Week = 7

			dateNow = dateNow.Date

			Dim R = ValidateWildcard(exp, Week, 1, 7)
			If R Then Return True

			' L 结束，月末最后一个周？
			Dim L = GetLastWildcard(exp)
			If L > 0 Then
				If L <= 7 Then
					' 本月的最后一天的星期
					Dim DateLast = New Date(dateNow.Year, dateNow.Month, 1).AddMonths(1).AddDays(-1)

					Dim WeekLast = DateLast.DayOfWeek
					If WeekLast = 0 Then WeekLast = 7

					' 计算要查日期与最后一天的时间差
					Dim Delay = L - WeekLast
					If Delay > 0 Then Delay -= 7

					Return dateNow = DateLast.AddDays(Delay).Date
				Else
					Return False
				End If
			End If

			' W 工作日
			Dim W = GetWorkWildcard(exp)
			If W IsNot Nothing Then
				Dim inc = False

				If W > 0 Then
					'--------------
					' 正向工作日
					'--------------
					' 周一时间
					Dim Work = dateNow.AddDays(1 - Week)

					For I = 1 To 7
						' 是否工作日
						If Not IsRestday(Work) Then
							W -= 1
							If W < 1 Then
								' 第 W 个工作日
								inc = Work = dateNow
								Exit For
							End If
						End If

						Work = Work.AddDays(1)
					Next

				ElseIf W < 0 Then
					'--------------
					' 反向工作日
					'--------------
					' 本周最后一天
					Dim Work = dateNow.AddDays(7 - Week)

					For I = 7 To 1 Step -1
						' 是否工作日
						If Not IsRestday(Work) Then
							W += 1
							If W > -1 Then
								' 倒数第 W 个工作日
								inc = Work = dateNow
								Exit For
							End If
						End If

						Work = Work.AddDays(-1)
					Next
				Else
					'--------------
					' 所有工作日
					'--------------
					inc = Not IsRestday(dateNow)
				End If

				Return inc
			End If

			Return False
		End Function

		''' <summary>通用通配符验证(*/-,)</summary>
		''' <remarks>
		''' 逗号（,） 指定数字集合。 1,4,7 代表：1、4、7
		''' 横线（-） 代表区间，一段数字集合。 3-6 代表：3、4、5、6
		''' 斜线（/） 间隔数据，从第一个数字开始，每隔第二个数字重复（0为无效间隔，负数则倒计数）。  3/5 从3开始，每隔5：3、8、13、18……；55/-10 表示从55开始倒数，含55、45、35、25、15、5
		''' 星号（*） 忽略此值，此段值不限
		'''</remarks>
		Private Shared Function ValidateWildcard(s As String, v As Integer, min As Integer, max As Integer) As Boolean
			' 如果未设置或者 * 直接表示通过
			If s.IsEmpty OrElse s = "*" Then Return True

			' 逗号（,）
			If s.Contains(","c) Then
				Dim vals = s.ToIntegerList(True)
				Return vals?.Contains(v)
			End If

			'斜线（/）
			If s.Contains("/"c) Then
				Dim vals = s.Split("/"c)
				Dim vA = vals(0).ToInteger
				Dim vB = vals(1).ToInteger

				If vB > 0 Then
					Dim inc = False
					For I = vA To max Step vB
						If v = I Then
							inc = True
							Exit For
						End If
					Next
					Return inc
				ElseIf vB < 0 Then
					Dim inc = False
					For I = vA To min Step vB
						If v = I Then
							inc = True
							Exit For
						End If
					Next
					Return inc
				Else
					Return vA = v
				End If
			End If

			'横线（-）
			If s.Contains("-"c) Then
				Dim vals = s.Split("-"c)
				Dim vMin = vals(0).ToInteger
				Dim vMax = vals(1).ToInteger

				If vMin < min Then vMin = min
				If vMax < vMin Then vMax = vMin
				If vMax > max Then vMax = max

				Return v >= vMin AndAlso v <= vMax
			End If

			' 表达式为最终数字
			If s.IsNumber Then
				Dim val = s.ToInteger
				If val > max Then val = max
				If val < min Then val = min

				Return val = v
			End If

			Return False
		End Function

#End Region

#Region "描述"

		''' <summary>获取描述</summary>
		Public ReadOnly Property Description As String
			Get
				If _Exp.All(Function(x) x = "*") AndAlso _DateDefault.IsEmpty Then Return "不限"

				'--------------
				' 时间周期
				'--------------
				If _Mode = TimeModeEnum.TIME Then
					Dim timeDefault = _DateDefault(0).ToString("yyyy-MM-dd HH:mm:ss")
					Dim interval = _Exp(0).ToInteger

					Select Case interval
						Case Is > 0
							' 正向间隔
							If timeDefault = New Date(2020, 1, 1) Then
								Return $"每隔 {interval} 秒执行"
							Else
								Return $"从 {timeDefault} 开始每隔 {interval} 秒执行"
							End If

						Case Is < 0
							If timeDefault = New Date(3000, 1, 1) Then
								Return $"每隔 {interval} 秒执行"
							Else
								Return $"每隔 {interval} 秒执行直到 {timeDefault} 结束"
							End If

						Case Else
							' 固定时间
							Return $"仅 {timeDefault} 执行一次"

					End Select
				End If

				' 秒
				Dim Sec = GetWildcardDesc(_Exp(0), "秒", 0, 59)

				' 分
				Dim Min = GetWildcardDesc(_Exp(1), "分", 0, 59)

				' 时
				Dim Hour = GetWildcardDesc(_Exp(2), "时", 0, 23)

				' 时钟字段
				Dim Time = $"{Hour}{Min}{Sec}"
				If Hour = "每时" AndAlso Min = "每分" AndAlso Sec = "每秒" Then Time = "全天"

				'--------------
				' 指定日期
				'--------------
				If _DateDefault.NotEmpty Then
					Dim days = _DateDefault.Select(Function(x) x.ToString("yyyy-MM-dd")).JoinString
					Return $"指定日期：{days}，{Time}"
				End If

				'--------------
				' 标准段
				'--------------

				' 年
				Dim Year = GetWildcardDesc(_Exp(6), "年", 2020, 3000)

				' 月
				Dim Month = GetWildcardDesc(_Exp(4), "月", 1, 12)

				' 日与星期
				Dim ExpDay = _Exp(3).EmptyValue("*")
				Dim ExpWeek = _Exp(5).EmptyValue("*")

				' 特殊通配符处理
				Dim Week = "每周"
				Dim Day = "每日"

				If ExpDay = "?" Or ExpDay = "*" Then
					Week = GetWeekDesc(ExpWeek)
				ElseIf ExpWeek = "?" Or ExpWeek = "*" Then
					Day = GetDayDesc(ExpDay)
				Else
					Day = GetDayDesc(ExpDay)
					Week = GetWeekDesc(ExpWeek)
				End If

				If Week <> "每周" Then
					If Day = "每日" Then
						Day = Week
					Else
						Day &= $"({Week})"
					End If
				End If

				Dim Ret = $"{Year}{Month}{Day}".
					Replace("每年", "").
					Replace("每月每日", "每日")

				'---------------------------
				' 非日期模式，附加时分秒
				'---------------------------
				If _Mode = TimeModeEnum.DAY Then
					Return Ret
				Else
					Return $"{Ret}，{Time}"
				End If
			End Get
		End Property

		''' <summary>日描述</summary>
		Private Shared Function GetDayDesc(exp As String) As String
			Dim R = GetWildcardDesc(exp, "日", 1, 31)
			If R.NotEmpty Then Return R

			' L 结束，月末最后一个周？
			Dim L = GetLastWildcard(exp)
			If L > 0 Then Return $"月倒数第 {L} 日"

			' W 工作日
			Dim W = GetWorkWildcard(exp)
			If W IsNot Nothing Then
				If W = 0 Then
					' 所有工作日
					Return "每个工作日"

				ElseIf W > 0 AndAlso W < 31 Then
					' 正向工作日
					Return $"每月第 {W} 个工作日"

				ElseIf W < 0 AndAlso W > -31 Then
					' 反向工作日
					W = -W
					If W = 1 Then
						Return $"每月最后一个工作日"
					Else
						Return $"每月倒数第 {W} 个工作日"
					End If
				End If
			End If

			' R 休息日
			If exp.EndsWith("R", StringComparison.OrdinalIgnoreCase) Then
				Select Case exp.ToUpper
					Case "R"
						' 所有假期
						Return "所有假期"

					Case "FR"
						' FR 休息日前一天
						Return "休息日前一天"

					Case "BR"
						' BR 休息日第一天
						Return "休息日第一天"

					Case "ER"
						' ER 休息日最后一天
						Return "休息日最后一天"

					Case "LR"
						' LR 休息日后的第一天
						Return "休息日后的第一天"
				End Select
			End If

			' H 法定节假日
			If exp.EndsWith("H", StringComparison.OrdinalIgnoreCase) Then
				Select Case exp.ToUpper
					Case "H"
						' 所有假期
						Return "所有法定假期"

					Case "FH"
						' FH 节假日前一天
						Return "法定假期前一天"

					Case "BH"
						' BH 节假日第一天
						Return "法定假期第一天"

					Case "EH"
						' EH 节假日最后一天
						Return "法定假期最后一天"

					Case "LH"
						' LH 节假日后的第一天
						Return "法定假期后的第一天"
				End Select
			End If

			Return ""
		End Function

		''' <summary>星期名称</summary>
		Private Shared Function GetWeekName(val As Integer) As String
			Dim weekName = "日"
			Select Case val
				Case 1
					weekName = "一"
				Case 2
					weekName = "二"
				Case 3
					weekName = "三"
				Case 4
					weekName = "四"
				Case 5
					weekName = "五"
				Case 6
					weekName = "六"
			End Select

			Return weekName
		End Function

		''' <summary>星期描述</summary>
		Private Shared Function GetWeekDesc(exp As String) As String
			Dim R = GetWildcardDesc(exp, "周", 1, 7)
			If R.NotEmpty Then Return R

			' L 结束，月末最后一个周？
			Dim L = GetLastWildcard(exp)
			If L > 0 Then Return $"月最后一个周{GetWeekName(L)}"

			' W 工作日
			Dim W = GetWorkWildcard(exp)
			If W IsNot Nothing Then
				If W = 0 Then
					' 所有工作日
					Return "每周所有工作日"

				ElseIf W > 0 AndAlso W < 8 Then
					' 正向工作日
					Return $"每周第 {W} 个工作日"

				ElseIf W < 0 AndAlso W > -8 Then
					' 反向工作日
					W = -W
					If W = 1 Then
						Return $"每周最后一个工作日"
					Else
						Return $"每周倒数第 {W} 个工作日"
					End If
				End If
			End If

			Return ""
		End Function

		''' <summary>通用通配符描述(*/-,)</summary>
		Private Shared Function GetWildcardDesc(s As String, prefix As String, Optional min As Integer = 0, Optional max As Integer = 59) As String
			If s.IsEmpty OrElse s = "*" Then Return $"每{prefix}"

			' 逗号（,）
			If s.Contains(","c) Then
				Dim vals = s.ToIntegerList(True)

				Select Case prefix
					Case "周"
						Return vals.Select(Function(x) $"周{GetWeekName(x)}").JoinString("、")

					Case "年"
						Return $"{vals.ToNumberString} 年"

					Case Else
						Return $"第 {vals.ToNumberString} {prefix}"
				End Select
			End If

			'斜线（/）
			If s.Contains("/"c) Then
				Dim vals = s.Split("/"c)
				Dim vA = vals(0).ToInteger
				Dim vB = vals(1).ToInteger

				If vB > 0 Then
					' 正向递增
					Select Case prefix
						Case "周"
							Return $"周{GetWeekName(vA)}~{GetWeekName(max)}每隔 {vB} 天"

						Case "年"
							Return $"{vA} 年起每隔 {vB} 年"

						Case Else
							Return $"第 {vA}~{max} {prefix}每隔 {vB} {prefix}"
					End Select

				Else
					' 反向递减
					vB = Math.Abs(vB)

					Select Case prefix
						Case "周"
							Return $"周{GetWeekName(min)}~{GetWeekName(vA)}每隔 {vb} 天"

						Case "年"
							Return $"{min} 年起每隔 {vB} 年"

						Case Else
							Return $"第 {min}~{vA} {prefix}每隔 {vB} {prefix}"
					End Select

				End If
			End If

			'横线（-）
			If s.Contains("-"c) Then
				Dim vals = s.Split("-"c)
				Dim vMin = vals(0).ToInteger
				Dim vMax = vals(1).ToInteger

				If vMin < min Then vMin = min
				If vMax < vMin Then vMax = vMin
				If vMax > max Then vMax = max

				' 实际最小值与最大值与规定的值一致表示全部数据有效
				' 比如 1-31 日 表示每日
				' 比如 1-7 周 表示整个周
				If vMin = min AndAlso vMax = max Then Return $"每{prefix}"

				Select Case prefix
					Case "周"
						If vMin = 1 AndAlso vMax = 7 Then Return "每周"
						Return $"周{GetWeekName(vMin)}～周{GetWeekName(vMax)}"

					Case "年"
						Return $"{vMin}～{vMax} 年"

					Case Else
						Return $"第 {vMin}～{vMax} {prefix}"
				End Select
			End If

			' 表达式为最终数字
			If s.IsNumber Then
				Dim val = s.ToInteger
				If val > max Then val = max
				If val < min Then val = min

				Select Case prefix
					Case "周"
						Return $"周{GetWeekName(val)}"

					Case "年"
						Return $"{val} 年"

					Case Else
						Return $"第 {val} {prefix}"
				End Select
			End If

			Return ""
		End Function

#End Region

	End Class
End Namespace