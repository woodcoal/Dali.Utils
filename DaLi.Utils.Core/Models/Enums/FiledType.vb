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
' 	memo: 字段类型
' 	
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>字段类型</summary>
	Public Enum FieldTypeEnum
		''' <summary>文本，不限字符类型</summary>
		<Description("文本")>
		TEXT = 0

		''' <summary>英文(ASCII)</summary>
		<Description("ASCII")>
		ASCII

		''' <summary>中文</summary>
		<Description("中文")>
		CHINESE

		''' <summary>数字(+/- 数字)</summary>
		<Description("数字")>
		NUMBER

		''' <summary>整数</summary>
		<Description("整数")>
		[INTEGER]

		''' <summary>长整数</summary>
		<Description("长整数")>
		[LONG]

		''' <summary>单精度</summary>
		<Description("单精度")>
		[SINGLE]

		''' <summary>双精度</summary>
		<Description("双精度")>
		[DOUBLE]

		''' <summary>0-255</summary>
		<Description("BYTE")>
		BYTES   ' 加了个 S ，ifree 创建 MYSQL 数据库时，byte 枚举会自动加一个空格，导致无法正常使用

		''' <summary>日期/时间</summary>
		<Description("时间")>
		DATETIME

		''' <summary>日期/时间</summary>
		<Description("日期")>
		[DATE]

		''' <summary>日期/时间</summary>
		<Description("时钟")>
		TIME

		''' <summary>三态，是非默认</summary>
		<Description("三态")>
		TRISTATE

		''' <summary>是非类型</summary>
		<Description("是非")>
		[BOOLEAN]

		''' <summary>大写文本</summary>
		<Description("大写文本")>
		UPPER_CASE

		''' <summary>小写文本</summary>
		<Description("小写文本")>
		LOWER_CASE

		''' <summary>邮箱</summary>
		<Description("邮箱")>
		EMAIL

		''' <summary>网址</summary>
		<Description("网址")>
		URL

		''' <summary>手机号</summary>
		<Description("手机号")>
		MOBILEPHONE

		''' <summary>目录</summary>
		<Description("目录")>
		FOLDER

		''' <summary>GUID</summary>
		<Description("GUID")>
		GUID

		''' <summary>JSON 对象</summary>
		<Description("JSON")>
		JSON

		''' <summary>XML 数据</summary>
		<Description("XML")>
		XML

		''' <summary>HTML 数据</summary>
		<Description("HTML")>
		HTML

		''' <summary>Markdown 数据</summary>
		<Description("Markdown")>
		MARKDOWN

		'''' <summary>JSON 数组</summary>
		'<Description("ARRAY")>
		'ARRAY
	End Enum

End Namespace
