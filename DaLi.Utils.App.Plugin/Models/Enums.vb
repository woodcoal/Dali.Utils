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
' 	枚举
'
' 	name: Model.Enums
' 	create: 2023-02-14
' 	memo: 枚举
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model
	Public Module Enums

		''' <summary>验证字段类型</summary>
		Public Enum FieldValidateEnum

			''' <summary>GUID</summary>
			<Description("GUID")>
			GUID

			''' <summary>英文(ASCII)</summary>
			<Description("ASCII")>
			ASCII

			''' <summary>JSON</summary>
			<Description("JSON")>
			JSON

			''' <summary>XML</summary>
			<Description("XML")>
			XML

			''' <summary>中文</summary>
			<Description("中文")>
			CHINESE

			''' <summary>大写文本</summary>
			<Description("大写文本")>
			UPPER_CASE

			''' <summary>小写文本</summary>
			<Description("小写文本")>
			LOWER_CASE

			''' <summary>数字(+/- 数字)</summary>
			<Description("数字")>
			NUMBER

			''' <summary>正整数</summary>
			<Description("正整数")>
			[UINTEGER]

			''' <summary>整数</summary>
			<Description("整数")>
			[INTEGER]

			''' <summary>正长整数</summary>
			<Description("正长整数")>
			[ULONG]

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
			[BYTE]   ' 加了个 S ，ifree 创建 MYSQL 数据库时，byte 枚举会自动加一个空格，导致无法正常使用

			''' <summary>日期/时间</summary>
			<Description("时间")>
			DATETIME

			''' <summary>三态，是非默认</summary>
			<Description("三态")>
			TRISTATE

			''' <summary>是非类型</summary>
			<Description("是非")>
			[BOOLEAN]

			''' <summary>邮箱</summary>
			<Description("邮箱")>
			EMAIL

			''' <summary>网址</summary>
			<Description("网址")>
			URL

			''' <summary>IPv4 / IPv6</summary>
			<Description("IP")>
			IP

			''' <summary>手机号</summary>
			<Description("手机号")>
			MOBILEPHONE

			''' <summary>电话</summary>
			<Description("电话")>
			PHONE

			''' <summary>路径</summary>
			<Description("路径")>
			PATH

			''' <summary>身份证</summary>
			<Description("身份证")>
			CARDID

			''' <summary>字母数字及横线</summary>
			<Description("字母数字及横线")>
			LETTERNUMBER

			''' <summary>判断是否有效用户名，字母开头，0-9/A-Z，最少3个字符</summary>
			<Description("用户名不含点")>
			USERNAME

			''' <summary>判断是否有效用户名，字母开头，0-9/A-Z，最少3个字符</summary>
			<Description("用户名含点")>
			USERNAME_ENDOT

			''' <summary>判断密码是否有效（6-20位长度；必须包含字母）</summary>
			<Description("密码")>
			PASSWORD

			''' <summary>判断密码是否有效（6-20位长度；必须包含数字或大写字母）</summary>
			<Description("加强密码")>
			PASSWORD_NUMBERLETTER

			''' <summary>判断密码是否有效（6-20位长度；必须包含数字；必须包含小写或大写字母；必须包含特殊符号）</summary>
			<Description("复杂密码")>
			PASSWORD_COMPLEX

			''' <summary>MD5 32位 Hash</summary>
			<Description("MD5")>
			MD5HASH

			''' <summary>车牌</summary>
			<Description("车牌")>
			CAR

			''' <summary>IPv4</summary>
			<Description("IPv4")>
			IPv4

			''' <summary>IPv6</summary>
			<Description("IPv6")>
			IPv6
		End Enum

		''' <summary>运行环境模式</summary>
		Public Enum EnvRunEnum
			''' <summary>运行环境不限</summary>
			ALL

			''' <summary>仅调试模式运行</summary>
			DEBUG

			''' <summary>仅正式环境运行</summary>
			PRODUCT
		End Enum

	End Module
End Namespace