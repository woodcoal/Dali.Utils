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
' 	公共参数枚举
'
' 	name: Model
' 	create: 2019-03-14
' 	memo: GUID 生成类型枚举
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>GUID 生成类型枚举</summary>
	Public Enum GuidEnum
		''' <summary>无规则，原始方式</summary>
		NONE

		''' <summary>字符排列（MySQL / PostgreSQL）</summary>
		STRING_SEQUENTIAL

		''' <summary>二进制排列（Oracle）</summary>
		BINARY_SEQUENTIAL

		''' <summary>末尾部分排列（SQL Server）</summary>
		END_SEQUENTIAL
	End Enum

End Namespace
