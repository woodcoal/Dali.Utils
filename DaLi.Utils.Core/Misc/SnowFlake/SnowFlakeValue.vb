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
' 	雪花算法值
'
' 	name: SnowFlakeOptions
' 	create: 2022-07-31
' 	memo: 雪花算法值
'
' ------------------------------------------------------------

Namespace Misc.SnowFlake

	''' <summary>雪花算法参数</summary>
	Public Class SnowFlakeValue

		''' <summary>机器码</summary>
		Public ReadOnly Property WorkerId As UShort = 0

		''' <summary>数据中心ID</summary>
		Public ReadOnly Property DataCenterId As UShort = 0

		''' <summary>序列数</summary>
		Private _Seq As Integer = 0

		''' <summary>随机数</summary>
		Private _Rnd As Integer = 0

		''' <summary>时间戳</summary>
		Private _Ticks As Long = 0

		''' <summary>实际值</summary>
		Private _Value As Long

		''' <summary>模块</summary>
		Private _ModuleId As UShort = 0

		''' <summary>序列数</summary>
		Public ReadOnly Property Seq As Integer
			Get
				Return _Seq
			End Get
		End Property

		''' <summary>随机数</summary>
		Public ReadOnly Property Rnd As Integer
			Get
				Return _Rnd
			End Get
		End Property

		''' <summary>时间戳</summary>
		Public ReadOnly Property Ticks As Long
			Get
				Return _Ticks
			End Get
		End Property

		''' <summary>实际值</summary>
		Public ReadOnly Property Value As Long
			Get
				Return _Value
			End Get
		End Property

		''' <summary>模块</summary>
		Public ReadOnly Property ModuleId As UShort
			Get
				Return _ModuleId
			End Get
		End Property

		''' <summary>构造</summary>
		Public Sub New(workerId As UShort, Optional dataCenterId As UShort = 0)
			Me.DataCenterId = dataCenterId
			Me.WorkerId = workerId
		End Sub

		Public Sub SetValue(ticks As Long, seq As Integer, rnd As Integer, value As Long, Optional moduleId As UShort = 0)
			_Ticks = ticks
			_Seq = seq
			_Rnd = rnd
			_Value = value
			_ModuleId = moduleId
		End Sub

		''' <summary>返回内容</summary>
		Public Overrides Function ToString() As String
			Return $"{Value}{vbTab}{Ticks}{vbTab}{Seq}{vbTab}{Rnd}{vbTab}{ModuleId}{vbTab}{WorkerId}{vbTab}{DataCenterId}"
		End Function
	End Class
End Namespace