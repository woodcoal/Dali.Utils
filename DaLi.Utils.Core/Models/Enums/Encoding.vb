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
' 	memo: 编码枚举
' 	
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>编码枚举</summary>
	Public Enum EncodingEnum

		''' <summary>自动分析</summary>
		<Description("自动")>
		AUTO = 0

		''' <summary>日语</summary>
		<Description("日文")>
		JANPAN = 932

		''' <summary>全汉字</summary>
		<Description("中文")>
		GBK = 936

		''' <summary>韩文</summary>
		<Description("韩文")>
		KOREAN = 949

		''' <summary>繁体中文</summary>
		<Description("繁体中文")>
		BIG5 = 950

		''' <summary>纯字符</summary>
		<Description("ASCII")>
		ASCII = 20127

		''' <summary>UTF8</summary>
		<Description("UTF8")>
		UTF8 = 65001
	End Enum

End Namespace
