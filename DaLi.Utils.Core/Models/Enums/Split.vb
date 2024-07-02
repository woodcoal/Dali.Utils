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
' 	memo: 字符串分隔选项
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>字符串分隔选项</summary>
	<Flags>
	Public Enum SplitEnum
		''' <summary>默认，不做任何处理，返回值包括含有空字符串的数组元素</summary>
		NONE = 0

		''' <summary>返回值不包括含有空字符串的数组元素</summary>
		REMOVE_EMPTY_ENTRIES = 1

		''' <summary>返回值移除重复项目</summary>
		REMOVE_SAME = 2

		''' <summary>原始字符串清除控制符（回车，制表符等）</summary>
		CLEAR_CONTROL = 4

		''' <summary>原始字符串清除控制符（回车，制表符等）和空格等</summary>
		CLEAR_TRIM = 8

		''' <summary>原始字符串清除HTML标签</summary>
		CLEAR_HTML = 16

		''' <summary>全部转换成大写</summary>
		RETRUN_UPPERCASE = 32

		''' <summary>全部转换成小写</summary>
		RETRUN_LOWERCASE = 64

		''' <summary>全部转换成半角</summary>
		RETRUN_DBC = 128

		''' <summary>全部转换成全角</summary>
		RETRUN_SBC = 256
	End Enum

End Namespace
