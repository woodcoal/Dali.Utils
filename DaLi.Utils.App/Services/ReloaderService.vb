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
' 	自动重载数据守护
'
' 	name: Service.ReloaderService
' 	create: 2023-02-14
' 	memo: 自动重载数据守护
'		  构造每60秒轮询一次项目，不写配置，禁止修改，无任何后台重载任务时关闭此功能
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports Microsoft.Extensions.Logging

Namespace Service

	''' <summary>自动重载数据守护</summary>
	Public Class ReloaderService
		Inherits BackServiceBase

		''' <summary>构造每60秒轮询一次项目，不写配置，禁止修改，无任何后台重载任务时关闭此功能</summary>
		Public Sub New(log As ILogger(Of ReloaderService))
			MyBase.New(log, 60, False)
		End Sub

		''' <summary>唯一名称标识</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "数据自动重载守护进程"
			End Get
		End Property

		''' <summary>是否强制禁止任务</summary>
		Public Overrides ReadOnly Property Disabled As Boolean
			Get
				Return Instance.IsEmpty
			End Get
		End Property

		''' <summary>内置驱动数据</summary>
		Private Shared _Instance As ImmutableList(Of IReloader)

		''' <summary>内置驱动数据</summary>
		Public Shared ReadOnly Property Instance As ImmutableList(Of IReloader)
			Get
				If _Instance Is Nothing Then _Instance = If(SYS.GetServices(Of IReloader), ImmutableList.Create(Of IReloader))

				Return _Instance
			End Get
		End Property

		''' <summary>执行服务</summary>
		Protected Overrides Function ExecuteAsync(stoppingToken As Threading.CancellationToken) As Task
			If stoppingToken.IsCancellationRequested Then Return Task.CompletedTask

			Dim Tasks = Instance.Select(Function(x) x.ReloadAsync(stoppingToken)).ToArray
			If Tasks.NotEmpty Then Task.WaitAll(Tasks, stoppingToken)

			Return Task.CompletedTask
		End Function

	End Class

End Namespace


