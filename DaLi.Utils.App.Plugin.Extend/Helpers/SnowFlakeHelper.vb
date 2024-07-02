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
' 	name: Helper.SnowFlakeHelper
' 	create: 2023-02-18
' 	memo: 验证字段处理
'
' ------------------------------------------------------------

Imports DaLi.Utils.Misc.SnowFlake

Namespace Helper
	''' <summary>雪花算法</summary>
	Public NotInheritable Class SnowFlakeHelper

#Region "基础构造"

		Public Sub New(serverId As UShort)
			If serverId < 1 OrElse serverId > 8 Then serverId = 1
			_ServerId = serverId
		End Sub

		''' <summary>服务器编号（适合用于雪花算法机器号码，限制 1~8）</summary>
		Private ReadOnly _ServerId As UShort

		''' <summary>雪花算法对象（不含模块）</summary>
		Private _SnowFlake As SnowFlake

		''' <summary>雪花算法对象（含模块）</summary>
		Private _SnowFlake_Model As SnowFlake

		''' <summary>雪花算法对象</summary>
		Private ReadOnly Property SnowFlake(Optional hasModule As Boolean = False) As SnowFlake
			Get
				If hasModule Then
					' 总长度：53，时间戳：30，机器码：3(8)，随机：5(32)，模块：5(32)，序列号：10 (1024)
					If _SnowFlake_Model Is Nothing Then _SnowFlake_Model = New SnowFlake(New SnowFlakeOptions(_ServerId) With {
											  .BaseTime = New Date,
											  .DataCenterIdLength = 0,
											  .JsLong = True,
											  .ModuleIdLength = 6,
											  .ReturnMode = False,
											  .RndLength = 4,
											  .TicksMode = TristateEnum.TRUE,
											  .WorkerIdLength = 3
										})
					Return _SnowFlake_Model
				Else
					' 总长度：53，时间戳：30，机器码：3(8)，随机：6(64)，模块：0，序列号：14 (16384)
					If _SnowFlake Is Nothing Then _SnowFlake = New SnowFlake(New SnowFlakeOptions(_ServerId) With {
											  .BaseTime = New Date,
											  .DataCenterIdLength = 0,
											  .JsLong = True,
											  .ModuleIdLength = 0,
											  .ReturnMode = False,
											  .RndLength = 6,
											  .TicksMode = TristateEnum.TRUE,
											  .WorkerIdLength = 3
										})
					Return _SnowFlake
				End If
			End Get
		End Property

		''' <summary>雪花算法标识</summary>
		Public ReadOnly Property GetID(Optional moduleId As Integer? = Nothing) As Long
			Get
				' 仅支持 32 个ID
				If moduleId.HasValue AndAlso moduleId.Value > 0 AndAlso moduleId.Value < 33 Then
					Return SnowFlake(True).NextId(moduleId)
				Else
					Return SnowFlake(False).NextId
				End If
			End Get
		End Property

		''' <summary>雪花算法模块范围</summary>
		Public ReadOnly Property GetIDRange(moduleId As Integer) As (Min As Long, Max As Long)
			Get
				' 仅支持 32 个ID
				If moduleId > 0 AndAlso moduleId < 33 Then
					Return SnowFlake(True).ModuleRange(moduleId)
				Else
					Return (1, 1 << (53 - 1))
				End If
			End Get
		End Property

#End Region

#Region "雪花算法"

		Private Shared _SnowFlakeHelper As SnowFlakeHelper

		''' <summary>雪花算法对象</summary>
		Public Shared ReadOnly Property SnowFlake As SnowFlakeHelper
			Get
				If _SnowFlakeHelper Is Nothing Then _SnowFlakeHelper = New SnowFlakeHelper(SYS.GetSetting(Of ICommonSetting)?.ServerId)
				Return _SnowFlakeHelper
			End Get
		End Property

		''' <summary>雪花算法标识</summary>
		Public Shared ReadOnly Property NextID(Optional moduleId As Integer? = Nothing) As Long
			Get
				Return SnowFlake.GetID(moduleId)
			End Get
		End Property

		''' <summary>雪花算法模块范围</summary>
		Public Shared ReadOnly Property NextID_Range(moduleId As Integer) As (Min As Long, Max As Long)
			Get
				Return SnowFlake.GetIDRange(moduleId)
			End Get
		End Property

#End Region

	End Class
End Namespace