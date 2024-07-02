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
' 	参数设置
'
' 	name: Entity.ConfigEntity
' 	create: 2023-02-18
' 	memo: 参数设置
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations

Namespace Entity

	''' <summary>参数设置</summary>
	<DbTable("App_Config")>
	<DbIndex("CreateTime")>
	<DbModule(1, "参数")>
	Public Class ConfigEntity
		Inherits EntityDateBase

		<DbColumn(IsPrimary:=True, IsIdentity:=True)>
		Public Overrides Property ID As Long

		''' <summary>模块</summary>
		Private _Module As String

		''' <summary>模块</summary>
		<Display(Name:="模块")>
		<Required(AllowEmptyStrings:=False, ErrorMessage:="Validate.Required")>
		<RegularExpression("^[a-zA-Z]{1,1}[0-9\.\-_a-zA-Z]{0,99}$", ErrorMessage:="{0}必须为字母开头的字符串，仅可以使用字母和数字、小数点、减号和下划线")>
		<MaxLength(100)>
		<DbQuery>
		Public Property [Module] As String
			Get
				Return _Module
			End Get
			Set(value As String)
				_Module = value.EmptyValue.ToUpper
			End Set
		End Property

		''' <summary>字段</summary>
		Private _Field As String

		''' <summary>字段</summary>
		<Required(AllowEmptyStrings:=False, ErrorMessage:="Validate.Required")>
		<RegularExpression("^[a-zA-Z]{1,1}[0-9\.a-zA-Z]{0,99}$", ErrorMessage:="{0}必须为字母开头的字符串，仅可以使用字母和数字、小数点")>
		<MaxLength(100)>
		<DbQuery>
		Public Property Field As String
			Get
				Return _Field
			End Get
			Set(value As String)
				_Field = value.EmptyValue.ToLower
			End Set
		End Property

		''' <summary>值</summary>
		<MaxLength(-1)>
		<Output(0)>
		<DbQuery(FreeSql.Internal.Model.DynamicFilterOperator.Contains)>
		Public Property Value As String

		''' <summary>验证属性</summary>
		<Display(Name:="验证属性")>
		<MaxLength(1000)>
		<Output(TristateEnum.FALSE)>
		Public Property Attributes As String

		''' <summary>参数序列化描述</summary>
		<MaxLength(500)>
		<Output(0)>
		Public Property Desc As String
	End Class
End Namespace