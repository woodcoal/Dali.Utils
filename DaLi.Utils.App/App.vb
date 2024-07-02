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
' 	应用全局操作
'
' 	name: Program
' 	create: 2023-02-14
' 	memo: 应用全局操作
'
' ------------------------------------------------------------

''' <summary>应用全局操作</summary>
Partial Public Module App

	''' <summary>启动项目</summary>
	Public Sub Start(Optional systemApp As SystemApp = Nothing, Optional args() As String = Nothing)
		systemApp = If(systemApp, New SystemApp)
		Core.Init(systemApp)
		Call New Startup().Run(systemApp, args)
	End Sub

End Module
