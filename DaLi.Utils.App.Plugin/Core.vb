' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	全局操作
'
' 	name: Core
' 	create: 2023-02-14
' 	memo: 全局操作
'
' ------------------------------------------------------------

''' <summary>全局操作</summary>
Public Module Core

	''' <summary>全局系统项目</summary>
	Private _SYS As ISystemBase

	''' <summary>全局系统项目</summary>
	Public ReadOnly Property SYS As ISystemBase
		Get
			If _SYS Is Nothing Then Throw New Exception("全局系统对象为注入，请先执行 Init(ISystemBase) 初始化系统")
			Return _SYS
		End Get
	End Property

	''' <summary>初始化</summary>
	Public Sub Init(sys As ISystemBase)
		_SYS = sys
	End Sub

End Module
