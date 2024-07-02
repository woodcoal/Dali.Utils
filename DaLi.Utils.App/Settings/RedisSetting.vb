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
' 	Redis 参数设置
'
' 	name: RedisSetting
' 	create: 2023-02-15
' 	memo: Redis 参数设置
'		https://github.com/2881099/FreeRedis/blob/master/README.zh-CN.md
'
' ------------------------------------------------------------

Imports FreeRedis

Namespace Setting

	''' <summary>Redis 参数设置</summary>
	Public Class RedisSetting
		Inherits LocalSettingBase(Of RedisSetting)

		''' <summary>生成连接字符串</summary>
		Public Property Conns As String()

		''' <summary>创建 Redis 对象</summary>
		Public Shared Function CreateClient(ParamArray conns() As String) As RedisClient
			' 连接字符串未设置
			If conns.IsEmpty Then Return Nothing

			' 检查字符串
			Dim ConnStrs = conns.Select(Function(x) ConnectionStringBuilder.Parse(x)).Where(Function(x) x.Host.NotEmpty).ToArray
			If ConnStrs.IsEmpty Then Return Nothing


			Dim client As RedisClient = Nothing
			Dim IsConnect = False

			Try
				If ConnStrs.Length > 1 Then
					client = New RedisClient(ConnStrs)
				Else
					client = New RedisClient(ConnStrs(0), Array.Empty(Of ConnectionStringBuilder))
				End If

				If client.Ping.NotEmpty Then IsConnect = True
			Catch ex As Exception
			End Try

			' 对无法正常连接的对象直接结束
			If client IsNot Nothing AndAlso Not IsConnect Then client.Dispose()

			Return client
		End Function

		''' <summary>创建 Redis 对象</summary>
		Public Function CreateClient() As RedisClient
			Return CreateClient(Conns)
		End Function
	End Class

End Namespace

' 127.0.0.1:6379,password=123,defaultDatabase=13
'参数				默认值		说明
'protocol			RESP2		若使用 RESP3 协议，你需要 Redis 6.0 环境
'user				<empty>		Redis 服务端用户名，要求 Redis 6.0 环境
'password			<empty>		Redis 服务端密码
'defaultDatabase	0			Redis 服务端数据库
'max poolsize		100			连接池最大连接数
'min poolsize		5			连接池最小连接数
'idleTimeout		20000		连接池中元素的空闲时间（单位为毫秒 ms），适用于连接到远程服务器
'connectTimeout		10000		连接超时，单位为毫秒（ms）
'receiveTimeout		10000		接收超时，单位为毫秒（ms）
'sendTimeout		10000		发送超时，单位为毫秒（ms）
'encoding			utf-8		字符串字符集
'retry				0			协议发生错误时，重试执行的次数
'ssl				false		启用加密传输
'name				<empty>		连接名，使用 CLIENT LIST 命令查看
'prefix				<empty>		key 前辍，所有方法都会附带此前辍，cli.Set(prefix + "key", 111);