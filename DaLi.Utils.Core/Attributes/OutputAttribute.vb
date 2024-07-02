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
' 	字段是否显示
'
' 	name: Attribute.OutputAttribute
' 	create: 2022-06-28
' 	memo: 字段类型转换时是否显示该字段
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>字段类型转换时是否显示该字段</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
	Public Class OutputAttribute
		Inherits System.Attribute

		''' <summary>是否显示：False 不输出，Defualt 任何时段都输出，True 详情调用时才输出</summary>
		Public Status As TristateEnum = TristateEnum.DEFAULT

		''' <summary>字段输出时，显示字段字符长度，作为概要（列表）显示时，默认显示内容长度，详情调用时参数无效；-1 未做设置，按实际系统允许最大值输出</summary>
		Public Length As Integer

		''' <summary>字段是否显示</summary>
		''' <param name="status">是否显示：False 不输出，Defualt 任何时段都输出，True 详情调用时才输出</param>
		''' <param name="length">字段输出时，显示字段字符长度，作为概要（列表）显示时，默认显示内容长度，详情调用时参数无效；-1 未做设置，按实际系统允许最大值输出</param>
		Public Sub New(Optional status As TristateEnum = TristateEnum.DEFAULT, Optional length As Integer = -1)
			Me.Status = status
			Me.Length = length
		End Sub

		''' <summary>字段是否显示</summary>
		''' <param name="length">字段输出时，显示字段字符长度，作为概要（列表）显示时，默认显示内容长度，详情调用时参数无效</param>
		Public Sub New(length As Integer)
			Status = TristateEnum.DEFAULT
			Me.Length = length
		End Sub
	End Class
End Namespace