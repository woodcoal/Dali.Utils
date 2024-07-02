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
' 	错误枚举
'
' 	name: Model
' 	create: 2023-02-15
' 	memo: 错误枚举，系统错误以 1000 开头，-1 未知，0 成功
'
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Model

	''' <summary>系统错误枚举</summary>
	Public Enum SystemErrorEnum
		''' <summary>异常</summary>
		<Description("异常")>
		UNKNOWN = -1

		''' <summary>成功</summary>
		<Description("")>
		SUCCESS = 0

		''' <summary>失败</summary>
		<Description("失败")>
		FAILURE = 1

		''' <summary>参数无效</summary>
		<Description("参数无效")>
		INVALID = 1000

		''' <summary>无此项目</summary>
		<Description("无此项目")>
		NOTFOUND = 1001

		''' <summary>禁用</summary>
		<Description("禁用")>
		DISABLED = 1002

		''' <summary>未授权</summary>
		<Description("未授权")>
		UNAUTHORIZED = 1003

		''' <summary>禁止操作</summary>
		<Description("禁止操作")>
		ACTION = 1004

		''' <summary>无有效数据</summary>
		<Description("无有效数据")>
		NODATA = 1005

		'-------------------
		' 客户端
		'-------------------

		''' <summary>客户端不存在</summary>
		<Description("客户端不存在")>
		CLIENT_NOTEXIST = 1100

		''' <summary>无效客户端</summary>
		<Description("无效客户端")>
		CLIENT_INVALID = 1101

		''' <summary>禁用客户端</summary>
		<Description("禁用客户端")>
		CLIENT_DISABLED = 1102

		''' <summary>客户端未授权</summary>
		<Description("客户端未授权")>
		CLIENT_UNAUTHORIZED = 1103

		''' <summary>客户端版本过低</summary>
		<Description("客户端版本过低")>
		CLIENT_LOW_VERSION = 1104

		''' <summary>客户端ＩＰ未授权</summary>
		<Description("客户端ＩＰ未授权")>
		CLIENT_INVALID_IP = 1105

		''' <summary>客户端无权访问此接口</summary>
		<Description("客户端无权访问此接口")>
		CLIENT_INVALID_API = 1106

		''' <summary>客户端应用标识无效</summary>
		<Description("客户端应用标识无效")>
		CLIENT_INVALID_APP = 1107

		''' <summary>客户端签名无效</summary>
		<Description("客户端签名无效")>
		CLIENT_INVALID_SIGN = 1108

		'-------------------
		' 账户
		'-------------------

		''' <summary>账户不存在</summary>
		<Description("账户不存在")>
		USER_NOTEXIST = 1120

		''' <summary>账户参数无效</summary>
		<Description("账户无效")>
		USER_INVALID = 1121

		''' <summary>账号或密码错误</summary>
		<Description("账号或密码错误")>
		USER_INVALID_ACCOUNT = 1122

		''' <summary>账户已禁用</summary>
		<Description("账户已禁用")>
		USER_DISABLED = 1123

		''' <summary>账户未认证</summary>
		<Description("账户未登录")>
		USER_NOLOGIN = 1124

		''' <summary>账户无权限</summary>
		<Description("账户无权限")>
		USER_UNAUTHORIZED = 1125

	End Enum

End Namespace
