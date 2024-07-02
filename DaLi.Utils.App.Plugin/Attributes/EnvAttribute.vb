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
' 	环境属性，目前仅支持在控制器上使用
'
' 	name: Attribute.EnvAttribute 
' 	create: 2023-05-30
' 	memo: 运行环境属性，是否开发模式还是正式模式
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>运行环境属性，目前仅支持在控制器上使用</summary>
	<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple:=False)>
	Public Class EnvAttribute
		Inherits System.Attribute

		''' <summary>是否正式环境、调试模式运行</summary>
		Public Property Run As EnvRunEnum

		''' <summary>是否有效的运行环境</summary>
		''' <param name="debug">当前是否调试模式</param>
		Public Function IsRuntime(debug As Boolean) As Boolean
			If Run = EnvRunEnum.ALL Then Return True

			If debug Then
				Return Run = EnvRunEnum.DEBUG
			Else
				Return Run = EnvRunEnum.PRODUCT
			End If
		End Function

		''' <summary>环境参数</summary>
		''' <param name="run">运行模式</param>
		Public Sub New(Optional run As EnvRunEnum = EnvRunEnum.ALL)
			Me.Run = run
		End Sub

	End Class
End Namespace