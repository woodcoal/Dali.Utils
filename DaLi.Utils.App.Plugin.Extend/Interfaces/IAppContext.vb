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
' 	应用上下文接口
'
' 	name: Interface.IAppContext
' 	create: 2023-02-17
' 	memo: 应用上下文接口
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Caching.Distributed

Namespace [Interface]

	''' <summary>应用上下文接口</summary>
	Public Interface IAppContext

		''' <summary>当前 HttpContext</summary>
		ReadOnly Property Http As HttpContext

		''' <summary>请求参数列表，仅获取一维数据</summary>
		ReadOnly Property Fields As KeyValueDictionary

		''' <summary>缓存</summary>
		ReadOnly Property Cache As IDistributedCache

		''' <summary>本地化语言对象</summary>
		ReadOnly Property Localizer As ILocalizerProvider

		''' <summary>错误消息项目</summary>
		ReadOnly Property ErrorMessage As ErrorMessage

		''' <summary>获取数据库驱动</summary>
		Function GetDb(Optional name As String = "default") As IFreeSql

	End Interface

End Namespace