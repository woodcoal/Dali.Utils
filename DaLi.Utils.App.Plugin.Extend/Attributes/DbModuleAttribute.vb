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
' 	模型属性
'
' 	name: Attribute.DbModuleAttribute
' 	create: 2023-10-05
' 	memo: 标记默认模型标识及名称，如果模型中存在 Dictionary As IEnumerable(Of Long) 属性则必须设置此参数，否则分组数据无法提交
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>登录授权属性</summary>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
	Public Class DbModuleAttribute
		Inherits System.Attribute

		''' <summary>标识，注意不能重复</summary>
		Public ReadOnly ID As UInteger

		''' <summary>名称</summary>
		Public ReadOnly Name As String

		''' <summary>是否显示</summary>
		Public ReadOnly Show As Boolean

		''' <summary>是否允许搜索</summary>
		Public ReadOnly Searchable As Boolean

		''' <summary>模型属性</summary>
		''' <param name="id">标识，注意不能重复</param>
		''' <param name="name">名称</param>
		''' <param name="searchable">是否允许搜索，默认不允许</param>
		Public Sub New(id As UInteger, name As String, Optional searchable As Boolean = False)
			Me.ID = id
			Me.Name = name
			Show = True
			Me.Searchable = searchable
		End Sub

		''' <summary>模型属性</summary>
		''' <param name="name">名称</param>
		''' <param name="searchable">是否允许搜索，默认不允许</param>
		Public Sub New(name As String, Optional searchable As Boolean = False)
			ID = UInteger.MaxValue
			Me.Name = name
			Show = True
			Me.Searchable = searchable
		End Sub

		''' <summary>模型属性</summary>
		''' <param name="show">是否显示</param>
		Public Sub New(show As Boolean)
			ID = UInteger.MaxValue
			Name = ""
			Me.Show = show
			Searchable = False
		End Sub
	End Class
End Namespace