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
' 	扩展数据模型基类
'
' 	name: Base.EntityExtendBase
' 	create: 2023-02-25
' 	memo: 扩展数据模型基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports System.Text.Json.Serialization
Imports FreeSql.DataAnnotations
Imports FreeSql.Internal.Model

Namespace Base

	''' <summary>扩展数据模型基类</summary>
	Public MustInherit Class EntityExtendBase
		Inherits EntityBase
		Implements IEntityExtend

		''' <summary>扩展内容</summary>
		<Display(Name:="Extension")>
		<JsonIgnore>
		<JsonMap>
		<DbColumn(Position:=-5)>
		<Output(TristateEnum.FALSE)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property Extension As New NameValueDictionary Implements IEntityExtend.Extension

	End Class

End Namespace