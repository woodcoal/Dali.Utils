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
' 	name: SnowFlakeHelper
' 	create: 2022-10-27
' 	memo: 雪花算法
'
' ------------------------------------------------------------

Namespace Misc.SnowFlake

	''' <summary>雪花算法参数</summary>
	Public NotInheritable Class SnowFlakeHelper

		''' <summary>线程锁</summary>
		Private Shared ReadOnly _Lock As New Object

		''' <summary>算法</summary>
		Private Shared _Worker As SnowFlake

		''' <summary>算法名称，方式重复创建</summary>
		Private Shared _WorkerName As String

		''' <summary>更新选项状态</summary>
		Public Shared Function GetID(name As String, workerId As UShort, dataCenterId As UShort, moduleId As UShort, opts As Func(Of SnowFlakeOptions)) As Long
			Dim ret As Long

			SyncLock _Lock
				Dim nameId = $"{name}_{workerId}_{dataCenterId}"
				If _WorkerName.IsEmpty OrElse
					_WorkerName <> nameId OrElse
					_Worker Is Nothing Then
					_WorkerName = nameId
					_Worker = New SnowFlake(opts.Invoke())
				End If

				ret = _Worker.NextId(moduleId)
			End SyncLock

			Return ret
		End Function

		''' <summary>更新选项状态</summary>
		Public Shared Function GetValue(name As String, workerId As UShort, dataCenterId As UShort, moduleId As UShort, opts As Func(Of SnowFlakeOptions)) As SnowFlakeValue
			Dim ret As SnowFlakeValue

			SyncLock _Lock
				Dim nameId = $"{name}_{workerId}_{dataCenterId}"
				If _WorkerName.IsEmpty OrElse
					_WorkerName <> nameId OrElse
					_Worker Is Nothing Then
					_WorkerName = nameId
					_Worker = New SnowFlake(opts.Invoke())
				End If

				ret = _Worker.NextValue(moduleId)
			End SyncLock

			Return ret
		End Function

		''' <summary>更新选项状态</summary>
		Public Shared Function GetModuleRange(name As String, workerId As UShort, dataCenterId As UShort, moduleId As UShort, opts As Func(Of SnowFlakeOptions)) As (Min As Long, Max As Long)
			Dim ret As (Min As Long, Max As Long)

			SyncLock _Lock
				Dim nameId = $"{name}_{workerId}_{dataCenterId}"
				If _WorkerName.IsEmpty OrElse
					_WorkerName <> nameId OrElse
					_Worker Is Nothing Then
					_WorkerName = nameId
					_Worker = New SnowFlake(opts.Invoke())
				End If

				ret = _Worker.ModuleRange(moduleId)
			End SyncLock

			Return ret
		End Function

		''' <summary>Twitter 雪花算法</summary>
		Public Shared Function TwitterID(Optional workerId As UShort = 1, Optional dataCenterId As UShort = 0) As Long
			Return GetID("twitter", workerId, dataCenterId, 0, Function() SnowFlakeOptions.TwitterOptions(workerId, dataCenterId))
		End Function

		''' <summary>JS 雪花算法，只能使用最长 53 位</summary>
		Public Shared Function JsID(Optional workerId As UShort = 1, Optional dataCenterId As UShort = 0) As Long
			Return GetID("js", workerId, dataCenterId, 0, Function() SnowFlakeOptions.JsOptions(workerId, dataCenterId))
		End Function

		''' <summary>数据库 雪花算法，为与前端 JS 兼容只能使用最长 53 位，4 位随机码</summary>
		''' <param name="workerId">机器码</param>
		''' <param name="dataCenterId">数据中心</param>
		''' <param name="moduleId">模块标识(64 模块)</param>
		Public Shared Function DbID(Optional workerId As UShort = 1, Optional dataCenterId As UShort = 0, Optional moduleId As UShort = 0) As Long
			Return GetID($"db_{moduleId}", workerId, dataCenterId, moduleId, Function() SnowFlakeOptions.DbOptions(workerId, dataCenterId， If(moduleId > 0, 6, 0)))
		End Function

		''' <summary>通用雪花算法，4 位随机码，10</summary>
		Public Shared Function NormalID(Optional workerId As UShort = 1, Optional dataCenterId As UShort = 0, Optional moduleId As UShort = 0) As Long
			Return GetID($"normal_{moduleId}", workerId, dataCenterId, 0, Function() SnowFlakeOptions.NormalOptions(workerId, dataCenterId， If(moduleId > 0, 10, 0)))
		End Function

	End Class
End Namespace