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
' 	name: Http.Model
' 	create: 2020-11-17
' 	memo: 枚举
' 	
' ------------------------------------------------------------

Imports System.ComponentModel

Namespace Http.Model

	''' <summary>网络状态枚举</summary>
	Public Enum HttpStatusEnum

		''' <summary>未知</summary>
		<Description("未知")>
		UNKNOWN = 0

		''' <summary>准备</summary>
		<Description("准备")>
		PREPARE = 1

		''' <summary>请求</summary>
		<Description("请求")>
		REQUEST = 2

		''' <summary>回复</summary>
		<Description("回复")>
		RESPONSE = 3

		''' <summary>忽略</summary>
		<Description("忽略")>
		ABORT = 4

		''' <summary>成功</summary>
		<Description("成功")>
		FINISH = 5

		''' <summary>失败</summary>
		<Description("失败")>
		FAILURE = 6

	End Enum

	''' <summary>Http 请求方式</summary> 
	Public Enum HttpMethodEnum
		[GET]
		POST
		PUT
		PATCH
		DELETE
		OPTIONS
		HEAD
		TRACE
	End Enum

	''' <summary>Http上传表单编码类型</summary> 
	Public Enum HttpPostEnum
		''' <summary>默认表单提交/ APPLICATION</summary>
		<Description("默认")>
		[DEFAULT]

		''' <summary>表单传送</summary>
		<Description("文件上传")>
		MULTIPART

		''' <summary>JSON请求</summary>
		<Description("JSON请求")>
		JSON

		''' <summary>原始内容，不处理直接提交</summary>
		<Description("RAW")>
		RAW
	End Enum

	''' <summary>Http上传表单编码类型</summary> 
	Public Enum HttpFieldTypeEnum
		''' <summary>普通字段</summary>
		<Description("默认")>
		[DEFAULT]

		''' <summary>上传文件路径</summary>
		<Description("路径")>
		PATH

		''' <summary>文件内容</summary>
		<Description("内容")>
		CONTENT
	End Enum

End Namespace
