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
' 	基础数据模型接口
'
' 	name: Interface.IEntity
' 	create: 2023-02-15
' 	memo: 基础数据模型接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>基础数据模型接口</summary>
	Public Interface IEntity
		Inherits ICloneable, IBase

		''' <summary>编号</summary>
		<DbSnowflake>
		<DbColumn(IsPrimary:=True, IsIdentity:=False)>
		Property ID As Long

		''' <summary>文本标识</summary>
		ReadOnly Property ID_ As String

	End Interface

End Namespace
