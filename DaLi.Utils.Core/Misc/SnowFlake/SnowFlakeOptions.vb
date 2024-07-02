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
' 	雪花算法选项
'
' 	name: SnowFlakeOptions
' 	create: 2022-07-30
' 	memo: 雪花算法参数，算法模式：全自定义；累计模式，只有最大序列数全部使用完后才使用新时间戳计算
'
' ------------------------------------------------------------

Namespace Misc.SnowFlake

	''' <summary>雪花算法参数</summary>
	Public Class SnowFlakeOptions

		''' <summary>返回模式：True 累计模式，False 传统模式；累计模式建议单机使用，多机有可能某台机器序列号未用完而时间已经很长，导致早时间另一太机器的序列号比他大</summary>
		Public Property ReturnMode As Boolean = False

		''' <summary>基础时间（UTC格式），不能超过当前系统时间</summary>
		Public Property BaseTime As Date = New DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)

		''' <summary>模块位长，默认值5，支持 32 个模块，取值范围 [0, 10] 0 禁用模块</summary>
		Public Property ModuleIdLength As Byte = 0

		''' <summary>机器码，必须由外部设定，最大值 2^WorkerIdLength-1</summary>
		Public ReadOnly Property WorkerId As UShort = 0

		''' <summary>机器码位长，默认值5，支持 32 个机器，取值范围 [1, 10]（要求：机器码位 + 数据中心位长不超过10）</summary>
		Public Property WorkerIdLength As Byte = 5

		''' <summary>数据中心ID（默认0）</summary>
		Public ReadOnly Property DataCenterId As UInteger = 0

		''' <summary>数据中心ID长度，默认0，取值范围 [1, 10]（要求：机器码位 + 数据中心位长不超过10）</summary>
		Public Property DataCenterIdLength As Byte = 0

		''' <summary>序列数位长，默认值0 自动分配（要求：随机数位长 + 序列数位长 + 机器码位 + 数据中心位长不超过默认长度）</summary>
		Public Property SeqLength As Byte = 0

		''' <summary>最大序列数（含），设置范围 [SeqMin, 2^SeqLength-1]，默认值0，表示最大序列数取最大值（2^SeqLength-1]）</summary>
		Public Property SeqMax As Integer = 0

		''' <summary>最小序列数（含），默认值5，取值范围 [5, SeqMax]，每毫秒的前5个序列数对应编号0-4是保留位，其中1-4是时间回拨相应预留位，0是手工新值预留位</summary>
		Public Property SeqMin As UShort = 5

		''' <summary>随机数位长，默认值0，取值范围 [0，12]（要求：自增数位长 + 随机数位长 + 序列数位长 + 机器码位 + 数据中心位长不超过22）</summary>
		Public Property RndLength As Byte = 0

		''' <summary>时间戳类型：DEFAULT 毫秒，TRUE 秒，FALSE 5 秒，默认 DEFAULT （注意，其他频率将导致生成速度问题，如频率为 5 秒，但是生成数量仅为1个，那么当天1个生成完成后，系统将锁定知道下 5 秒才能生成，所以注意合理分配，仅支持到）</summary>
		Public Property TicksMode As TristateEnum = TristateEnum.DEFAULT

		''' <summary>固定长整形数字长度，仅适合配合前端 JS 使用，JS 只允许 53 位</summary>
		Public Property JsLong As Boolean = False

		''' <summary>构造</summary>
		Public Sub New(workerId As UShort, Optional dataCenterId As UShort = 0)
			Me.DataCenterId = dataCenterId
			Me.WorkerId = workerId
		End Sub

		''' <summary>验证参数是否异常，并自动修正错误</summary>
		Public Function Validate() As (Options As SnowFlakeOptions, Succ As Boolean)
			Dim Succ = True

			' 返回长整型数据的长度
			' .Net long 最高位应该为 0 ，所以长度只有 63
			' Js long 长度就是 53
			Dim Len = If(JsLong, 53, 63)

			' 模块
			If ModuleIdLength > 10 OrElse ModuleIdLength < 0 Then
				Succ = False
				ModuleIdLength = 10
			End If
			Len -= ModuleIdLength

			' 使用分时，时间戳 26 位，可以使用 127 年
			' 使用秒位时间戳时，时间戳长度位 32 位，可以使用 130 多年
			' 使用毫秒时，时间戳 41 位，可以使用 69 年
			Select Case TicksMode
				Case TristateEnum.DEFAULT
					Len -= 41     ' 约 69 年

				Case TristateEnum.TRUE
					Len -= 30     ' 约 34 年

				Case TristateEnum.FALSE
					Len -= 27
			End Select

			'Len -= If(TicksMode, 32, 41)

			' 基础时间点，最早为 2020 年
			Dim date2022 = New DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
			If BaseTime < date2022 OrElse BaseTime > SYS_NOW_DATE Then
				Succ = False
				BaseTime = date2022
			End If

			' 机器码与数据中心长度，不能超过 10
			' 数据中心长度为 0，表示忽略数据中心数据
			If WorkerIdLength < 1 OrElse DataCenterIdLength < 0 OrElse WorkerIdLength + DataCenterIdLength > 10 Then
				Succ = False

				If DataCenterIdLength < 0 Then DataCenterIdLength = 0
				If WorkerIdLength < 1 Then WorkerIdLength = 10 - DataCenterIdLength
			End If

			Len -= WorkerIdLength
			Len -= DataCenterIdLength

			' 序列号长度过小，不论随机位与序列位如何设置都忽略
			If SeqLength > 0 AndAlso Len <= SeqLength Then
				Succ = False
				RndLength = 0
				SeqLength = Len

			Else
				' 随机位长度，不能超过剩余位数
				If RndLength < 0 OrElse RndLength > 12 OrElse RndLength > Len Then
					Succ = False
					RndLength = 0
				End If

				' 最大序列数位长
				Len -= RndLength
				If SeqLength < 1 Or SeqLength > Len Then
					Succ = False
					SeqLength = Len
				End If
			End If

			' 允许最大的序列数
			Dim seqMaxNum = (1 << SeqLength) - 1
			If SeqMax = 0 Then
				SeqMax = seqMaxNum
			ElseIf SeqMax > seqMaxNum Then
				Succ = False
				SeqMax = seqMaxNum
			End If

			' 最小序列数
			If SeqMin >= SeqMax Then
				Succ = False
				SeqMin = 0
			End If

			Dim workerId = If(Me.WorkerId > 0, Me.WorkerId, 1)
			Dim dataCenterId = If(Me.DataCenterId > 0, Me.DataCenterId, 0)

			' 返回修正结果
			Return (New SnowFlakeOptions(workerId, dataCenterId) With {
				.ReturnMode = ReturnMode,
				.BaseTime = BaseTime,
				.ModuleIdLength = ModuleIdLength,
				.WorkerIdLength = WorkerIdLength,
				.DataCenterIdLength = DataCenterIdLength,
				.SeqLength = SeqLength,
				.SeqMax = SeqMax,
				.SeqMin = SeqMin,
				.RndLength = RndLength,
				.TicksMode = TicksMode,
				.JsLong = JsLong
			}, Succ)
		End Function

#Region "常用属性"

		''' <summary>Twitter 雪花算法默认参数</summary>
		Public Shared Function TwitterOptions(workerId As UShort, Optional dataCenterId As UShort = 0) As SnowFlakeOptions
			Return New SnowFlakeOptions(workerId, dataCenterId) With {
						.BaseTime = New Date,
						.WorkerIdLength = 5,
						.DataCenterIdLength = 5,
						.SeqLength = 12,
						.SeqMin = 0,
						.SeqMax = 0,
						.RndLength = 0,
						.TicksMode = TristateEnum.DEFAULT,
						.ReturnMode = False,
						.JsLong = False
					}

		End Function

		''' <summary>Js 雪花算法默认参数</summary>
		Public Shared Function JsOptions(workerId As UShort, Optional dataCenterId As UShort = 0) As SnowFlakeOptions
			Return New SnowFlakeOptions(workerId, dataCenterId) With {
						.BaseTime = New Date,
						.WorkerIdLength = 4,
						.DataCenterIdLength = 3,
						.SeqLength = 0,
						.SeqMin = 0,
						.SeqMax = 0,
						.RndLength = 0,
						.TicksMode = TristateEnum.TRUE,
						.ReturnMode = False，
						.JsLong = True
					}

		End Function

		''' <summary>数据库 雪花算法默认参数</summary>
		Public Shared Function DbOptions(workerId As UShort, Optional dataCenterId As UShort = 0, Optional moduleIdLength As Integer = 0) As SnowFlakeOptions
			Return New SnowFlakeOptions(workerId, dataCenterId) With {
						.BaseTime = New Date,
						.ModuleIdLength = moduleIdLength,
						.WorkerIdLength = If(moduleIdLength > 0, 2, 3),
						.DataCenterIdLength = 0,
						.SeqLength = 0,
						.SeqMin = 0,
						.SeqMax = 0,
						.RndLength = If(moduleIdLength > 0, 5, 7),
						.TicksMode = TristateEnum.TRUE,
						.ReturnMode = False，
						.JsLong = True
					}
		End Function

		''' <summary>叠加雪花算法默认参数</summary>
		Public Shared Function NormalOptions(workerId As UShort, Optional dataCenterId As UShort = 0, Optional moduleIdLength As Integer = 0) As SnowFlakeOptions
			Return New SnowFlakeOptions(workerId, dataCenterId) With {
						.BaseTime = New Date,
						.ModuleIdLength = moduleIdLength,
						.WorkerIdLength = 4,
						.DataCenterIdLength = 2,
						.SeqLength = 0,
						.SeqMin = 0,
						.SeqMax = 0,
						.RndLength = 8,
						.TicksMode = TristateEnum.DEFAULT,
						.ReturnMode = True，
						.JsLong = False
					}
		End Function

#End Region

	End Class
End Namespace