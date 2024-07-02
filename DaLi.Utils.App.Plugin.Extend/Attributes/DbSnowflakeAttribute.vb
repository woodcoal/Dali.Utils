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
' 	雪花 ID 字段标记
'
' 	name: Attribute.DbSnowflakeAttribute
' 	create: 2023-02-15
' 	memo: 雪花 ID 字段标记
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>雪花 ID 字段标记</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
	Public Class DbSnowflakeAttribute
		Inherits System.Attribute

		''' <summary>模块标识</summary>
		Public ModuleId As Integer? = Nothing

		''' <summary>是否强制使用数据库模型方式雪花算法</summary>
		''' <param name="moduleId">1-32 强制使用模型，0 不使用模型，未设置(空值)则自动分析字段是否为系统标注字段，标注则使用标注标识检测</param>
		Public Sub New(moduleId As Integer?)
			Me.ModuleId = moduleId
		End Sub

		''' <summary>是否强制使用数据库模型方式雪花算法</summary>
		Public Sub New()
			ModuleId = Nothing
		End Sub

	End Class
End Namespace