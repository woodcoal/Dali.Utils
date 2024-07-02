' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	上下文对象
'
' 	name: ExtendContext
' 	create: 2023-02-27
' 	memo: 上下文对象
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.Caching.Distributed

''' <summary>上下文对象</summary>
Public Class ExtendContext
	Inherits AppContextBase

	Public Sub New(http As IHttpContextAccessor, dbProvider As IDatabaseProvider, cache As IDistributedCache, localizer As ILocalizerProvider)
		MyBase.New(http, dbProvider, cache, localizer)
	End Sub

End Class
