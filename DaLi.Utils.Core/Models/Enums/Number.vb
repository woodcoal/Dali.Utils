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
' 	进制索引
'
' 	name: Model
' 	create: 2021-11-28
' 	memo: 进制索引
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>进制索引：2, 8, 10, 16, 26, 32, 34, 36, 52, 62, 64</summary>
	Public Enum RadixEnum
		''' <summary>二进制</summary>
		_2 = 2

		''' <summary>二进制</summary>
		BIN = 2

		''' <summary>八进制</summary>
		_8 = 8

		''' <summary>八进制</summary>
		OCT = 8

		''' <summary>十进制(纯数字)</summary>
		_10 = 10

		''' <summary>十进制(纯数字)</summary>
		NUMBER = 10

		''' <summary>十进制(纯数字)</summary>
		DEC = 10

		''' <summary>十六进制</summary>
		_16 = 16

		''' <summary>十六进制</summary>
		HEX = 16

		''' <summary>26进制（全大写字母）</summary>
		_26 = 26

		''' <summary>26进制（全大写字母）</summary>
		LETTER = 26

		''' <summary>32进制（BASE32）</summary>
		_32 = 32

		''' <summary>32进制（BASE32）</summary>
		BASE32 = 32

		''' <summary>34进制（数字、大写字母，去除IO）</summary>
		_34 = 34

		''' <summary>34进制（数字、大写字母，去除IO）</summary>
		NUMBER_LETTER_CLEAR = 34

		''' <summary>36进制（数字、大写字母）</summary>
		_36 = 36

		''' <summary>36进制（数字、大写字母）</summary>
		NUMBER_LETTER = 36

		''' <summary>52进制（大写、小写字母）</summary>
		_52 = 52

		''' <summary>52进制（大写、小写字母）</summary>
		LETTER_MIX = 52

		''' <summary>62进制（数字、大写、小写字母）</summary>
		_62 = 62

		''' <summary>62进制（数字、大写、小写字母）</summary>
		NUMBER_LETTER_MIX = 62

		''' <summary>64进制（BASE64）</summary>
		_64 = 64

		''' <summary>64 进制（BASE64）</summary>
		BASE64 = 64

	End Enum

End Namespace
