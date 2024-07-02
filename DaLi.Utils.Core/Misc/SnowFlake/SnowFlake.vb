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
' 	雪花算法
'
' 	name: SnowFlake
' 	create: 2022-07-30
' 	memo: 标准与叠加模式
'
'	------------------
'		标准雪花算法
'	------------------
'	参考：https://segmentfault.com/a/1190000011282426
'	Long 字符长度 64 位 / Js 只有 53 位；首页不能位 1，否则为负数
'	
'	1     5     9     13    17    21    25    29    33    37    41    45    49    53    57    61
'	0000  0000  0000  0000  0000  0000  0000  0000  0000  0000  0000  0000  0000  0000  0000  0000
'	|<-----------------------  41 位时间戳  --------------------->|<- 5+5 位机器码 ->|<- 12 序列号 ->|
'
'	· 1 位，不用。二进制中最高位为 1 的都是负数，但是我们生成的 id 一般都使用整数，所以这个最高位固定是 0。
'	· 41 位，用来记录时间戳（毫秒）。41 位可以表示 2^{41}-1 个数字，如果只用来表示正整数（计算机中正数包含0），可以表示的数值范围是：0 至 2^{41}-1 ，减1是因为可表示的数值范围是从0开始算的，而不是1。也就是说41位可以表示 2 ^ {41} - 1 个毫秒的值，转化成单位年则是 (2 ^ 41 - 1) / (1000 * 60 * 60 * 24 * 365) = 69 年
'	· 10 位，用来记录工作机器 id。可以部署在 2 ^ 10 = 1024 个节点， 包括 5 位 datacenterId 和 5 位 workerId; 5 位可以表示的最大正整数是 2^5-1 = 31，即可以用 0、1、2、3、....31 这 32 个数字，来表示不同的 datecenterId 或 workerId。
'	· 12 位，序列号，用来记录同毫秒内产生的不同 id。12 位可以表示的最大正整数是 2^12-1 = 4095，即可以用 0、1、2、3、....4094 这 4095 个数字，来表示同一机器同一时间截（毫秒)内产生的 4095 个 ID 序号。
'
'	------------------
'		扩展雪花算法
'	------------------
'	1. 根据业务需要调整机器码长度，序列号长度；
'	2. 可以使用秒为时间戳
'	3. 序列号的最小与最大值可以控制
'	4. 为防止递增 ID 容易被猜到，可以在序列号后叠加随机数
'	5. 使用叠加模式，在最大序列号未使用完之前不使用新时间戳
'
'	------------------
'	  占位长度值范围
'	------------------
'	3	=>	8		4	=>	16		5	=>	32
'	6	=>	64		7	=>	128		8	=>	256
'	9	=>	512		10	=>	1024	11	=>	2048
'	12	=>	4096	13	=>	8192	14	=>	16384
'
' ------------------------------------------------------------

Imports System.Threading

Namespace Misc.SnowFlake

	''' <summary>雪花算法</summary>
	Public Class SnowFlake

		''' <summary>线程锁</summary>
		Protected Shared ReadOnly Lock As New Object

		''' <summary>最后时间</summary>
		Protected LastTime As Long

		''' <summary>当前序列</summary>
		Protected SeqCurrent As Long

		''' <summary>基础值，moduleId/workId/dateCenterId 组合值</summary>
		Protected ReadOnly BaseValue As Long

		''' <summary>最大随机数</summary>
		Protected ReadOnly RndMax As Integer

		''' <summary>最小随机数</summary>
		Protected ReadOnly RndMin As Integer

		''' <summary>随机对象</summary>
		Protected ReadOnly Rnd As Random

		''' <summary>时间戳移位</summary>
		Protected ReadOnly TimeShift As Integer

		''' <summary>模块移位</summary>
		Protected ReadOnly ModuleShift As Integer

		''' <summary>模块最大值</summary>
		Protected ReadOnly ModuleMax As UShort

		''' <summary>选项</summary>
		Public ReadOnly Options As SnowFlakeOptions

		Public Sub New(workerId As UShort, Optional dataCenterId As UShort = 0)
			Me.New(New SnowFlakeOptions(workerId, dataCenterId))
		End Sub

		Public Sub New(options As SnowFlakeOptions)
			If options Is Nothing Then Throw New Exception("无效参数，请设置有效雪花参数")
			Me.Options = options.Validate.Options

			' 检查最大时间戳
			Dim maxTicks As Long = 1
			Select Case options.TicksMode
				Case TristateEnum.DEFAULT
					maxTicks <<= 41     ' 约 69 年

				Case TristateEnum.TRUE
					maxTicks <<= 30     ' 约 34 年

				Case TristateEnum.FALSE
					maxTicks <<= 27     ' 约 21 年

			End Select

			If GetCurrentTimeTick() >= maxTicks Then Throw New Exception("无效参数，时间已经超过系统允许范围")

			' 随机数
			TimeShift = options.RndLength
			If options.RndLength > 2 Then
				Rnd = New Random
				RndMax = (1 << options.RndLength) - 1
				RndMin = 1 << (options.RndLength - 2)
			End If

			' 序列号
			TimeShift += options.SeqLength

			' 机器码
			BaseValue = (CLng(options.WorkerId) << TimeShift) - 1
			TimeShift += options.WorkerIdLength

			' 数据中心
			If options.DataCenterIdLength > 0 Then
				BaseValue += (CLng(options.DataCenterId) << TimeShift) - 1
				TimeShift += options.DataCenterIdLength
			End If

			' 模块
			If options.ModuleIdLength > 0 Then
				ModuleShift = If(options.JsLong, 53, 63) - options.ModuleIdLength
				ModuleMax = 1 << options.ModuleIdLength ' 最大容量
			Else
				ModuleShift = 0
				ModuleMax = 0
			End If

			SeqCurrent = options.SeqMin
		End Sub

		''' <summary>获取当前时间戳</summary>
		Protected Overridable Function GetCurrentTimeTick() As Long
			Select Case Options.TicksMode
				Case TristateEnum.DEFAULT
					' 毫秒模式
					Return (SYS_NOW_DATE - Options.BaseTime).TotalMilliseconds

				Case TristateEnum.TRUE
					' 秒模式
					Return (SYS_NOW_DATE - Options.BaseTime).TotalSeconds

				Case TristateEnum.FALSE
					' 分钟模式
					Return (SYS_NOW_DATE - Options.BaseTime).TotalMinutes

			End Select

			Return 0
		End Function

		''' <summary>获取下一个时间戳</summary>
		Protected Overridable Function GetNextTimeTick() As Long
			Dim ticks = GetCurrentTimeTick()

			While ticks <= LastTime
				' 延时
				Select Case Options.TicksMode
					Case TristateEnum.DEFAULT

					Case TristateEnum.TRUE
						Thread.Sleep(1)

					Case TristateEnum.FALSE
						Thread.Sleep(100)
				End Select

				ticks = GetCurrentTimeTick()
			End While

			Return ticks
		End Function

		''' <summary>获取当前时间戳</summary>
		Public Overridable Function NextValue(Optional moduleId As UShort = 0) As SnowFlakeValue
			Dim id = CalcId(moduleId)

			Dim ret = New SnowFlakeValue(Options.WorkerId, Options.DataCenterId)
			ret.SetValue(id.Ticks, id.Seq, id.Rnd, id.Value, moduleId)

			Return ret
		End Function

		''' <summary>获取当前时间戳</summary>
		Public Overridable Function NextId(Optional moduleId As UShort = 0) As Long
			Return CalcId(moduleId).Value
		End Function

		''' <summary>获取模块区间大小</summary>
		Public Function ModuleRange(moduleId As UShort) As (Min As Long, Max As Long)
			If Options.ModuleIdLength > 0 Then
				Return ((CLng(moduleId - 1) << ModuleShift) + 1, CLng(moduleId) << ModuleShift)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>获取当前时间戳</summary>
		Public Overridable Function CalcId(Optional moduleId As UShort = 0) As (Ticks As Long, Seq As Long, Rnd As Integer, Value As Long)
			SyncLock Lock
				If Options.ReturnMode Then
					' 累计模式，未分配完成不进行时间处理
				Else
					' 通用模式，时间不同重新分配
					Dim ticks = GetCurrentTimeTick()
					If ticks > LastTime Then
						' 进入新时段
						SeqCurrent = Options.SeqMin
						LastTime = ticks

					ElseIf ticks = LastTime Then
						' 同一时段，超限则使用下一时段，未超限继续操作
					Else
						' 异常
						Throw New Exception("无效时间戳")
					End If
				End If

				' 如果最后时间未设置或者序列号超过指定值，重置
				If LastTime < 1 OrElse SeqCurrent > Options.SeqMax Then
					SeqCurrent = Options.SeqMin
					LastTime = GetNextTimeTick()
				End If

				Dim rndValue As Integer = -1
				Dim seqNow = SeqCurrent
				Dim seq = (LastTime << TimeShift) + BaseValue
				If RndMax > 0 Then
					' 存在随机数
					rndValue = Rnd.Next(RndMin, RndMax)
					seq += (seqNow << Options.RndLength) + rndValue
				Else
					seq += seqNow
				End If

				' 附加模块标识
				' 从 1 开始，默认则需要减一
				If moduleId <= ModuleMax AndAlso moduleId > 0 Then seq += CLng(moduleId - 1) << ModuleShift

				' 自增 1
				Interlocked.Increment(SeqCurrent)

				Return (LastTime, seqNow, rndValue, seq)
			End SyncLock
		End Function

		'''' <summary>获取当前时间戳</summary>
		'Public Overridable Function NextId() As Long
		'	SyncLock Lock
		'		If Options.ReturnMode Then
		'			' 累计模式，未分配完成不进行时间处理
		'			'If LastTime < 1 OrElse seqCurrent > Options.SeqMax Then
		'			'	seqCurrent = Options.SeqMin
		'			'	LastTime = GetNextTimeTick()
		'			'End If
		'		Else
		'			' 通用模式，时间不同重新分配
		'			Dim ticks = GetCurrentTimeTick()
		'			If ticks > LastTime Then
		'				' 进入新时段
		'				seqCurrent = Options.SeqMin
		'				LastTime = ticks

		'			ElseIf ticks = LastTime Then
		'				' 同一时段，超限，使用下一时段
		'				'If seqCurrent > Options.SeqMax Then
		'				'	ticks = GetNextTimeTick()
		'				'	seqCurrent = Options.SeqMin
		'				'End If
		'			Else
		'				' 异常
		'				Throw New Exception("无效时间戳")
		'			End If
		'		End If

		'		Return CalcId()
		'	End SyncLock
		'End Function

		'''' <summary>计算标识</summary>
		'Protected Overridable Function CalcId() As Long
		'	' 如果最后时间未设置或者序列号超过指定值，重置
		'	If LastTime < 1 OrElse seqCurrent > Options.SeqMax Then
		'		SeqCurrent = Options.SeqMin
		'		LastTime = GetNextTimeTick()
		'	End If

		'	Dim rndValue As Integer = -1
		'	Dim seq = (LastTime << timeShift) + BaseValue
		'	If RndMax > 0 Then
		'		' 存在随机数
		'		rndValue = Rnd.Next(RndMin, RndMax)
		'		seq += (SeqCurrent << Options.RndLength) + ID.Rnd
		'	Else
		'		seq += seqCurrent
		'	End If

		'	' 存值
		'	ID.SetValue(LastTime, seqCurrent, rndValue, seq)

		'	' 自增 1
		'	Interlocked.Increment(seqCurrent)

		'	Return seq
		'End Function

	End Class
End Namespace