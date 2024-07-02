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
' 	FreeRedis 扩展操作
'
' 	name: FreeRedisExtension
' 	create: 2024-06-19
' 	memo: FreeRedis 扩展操作
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports FreeRedis


''' <summary>FreeRedis 扩展操作</summary>
Public Module FreeRedisExtension

	''' <summary>获取 FreeRedis 前缀</summary>
	''' <param name="client">Redis 客户端</param>
	<Extension>
	Public Function Prefix(client As RedisClient) As String
		Return client?.GetType.GetSingleProperty("Prefix").GetValue(client)
	End Function

	''' <summary>获取 FreeRedis 项目的类型</summary>
	''' <param name="client">Redis 客户端</param>
	<Extension>
	Public Function TypeName(client As RedisClient, key As String) As String
		Dim cmd As New CommandPacket("TYPE")
		Dim data = client.Call(cmd.InputKey(key))

		Return data?.ToString
	End Function

	''' <summary>获取 FreeRedis 包含模块</summary>
	''' <param name="client">Redis 客户端</param>
	<Extension>
	Public Function Modules(client As RedisClient) As RedisModule()
		Dim cmd As New CommandPacket("MODULE", "LIST")
		Dim data = client.Call(cmd)
		Return RedisModule.Convert(data)
	End Function

	''' <summary>获取 FreeRedis 包含模块</summary>
	''' <param name="client">Redis 客户端</param>
	<Extension>
	Public Function ModuleExists(client As RedisClient, moduleName As String) As Boolean
		Return client.Modules?.Any(Function(x) x.Name.IsSame(moduleName))
	End Function

	'#Region "HASH 扩展"

	'	''' <summary>获取 Hash 对象</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function Hash(client As RedisClient, key As String) As Dictionary(Of String, String)
	'		Return client.HGetAll(key)
	'	End Function

	'	''' <summary>获取 Hash 对象</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function Hash(Of T)(client As RedisClient, key As String) As Dictionary(Of String, T)
	'		Return client.Hash(key)?.ToDictionary(Function(x) x.Key, Function(x) x.Value.ToValue(Of T))
	'	End Function

	'	''' <summary>获取 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="field">Hash 字段名</param>
	'	<Extension>
	'	Public Function HashGet(client As RedisClient, key As String, field As String) As String
	'		Return client.HGet(key, field).EmptyValue
	'	End Function

	'	''' <summary>获取 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="field">Hash 字段名</param>
	'	<Extension>
	'	Public Function HashGet(Of T)(client As RedisClient, key As String, field As String) As T
	'		Return client.HashGet(key, field).ToValue(Of T)
	'	End Function

	'	''' <summary>获取 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="fields">多个 Hash 字段名</param>
	'	<Extension>
	'	Public Function HashGet(client As RedisClient, key As String, ParamArray fields As String()) As String()
	'		Return client.HMGet(key, fields)
	'	End Function

	'	''' <summary>获取 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="fields">多个 Hash 字段名</param>
	'	<Extension>
	'	Public Function HashGet(Of T)(client As RedisClient, key As String, ParamArray fields As String()) As T()
	'		Return client.HashGet(key, fields)?.Select(Function(x) x.EmptyValue.ToValue(Of T)).ToArray
	'	End Function

	'	''' <summary>设置 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="field">Hash 字段名</param>
	'	''' <param name="value">Hash 值</param>
	'	<Extension>
	'	Public Function HashSet(client As RedisClient, key As String, field As String, value As String) As Boolean
	'		Return client.HSet(key, field, value) > 0
	'	End Function

	'	''' <summary>设置 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="data">Hash 数据</param>
	'	<Extension>
	'	Public Function HashSet(client As RedisClient, key As String, data As IDictionary(Of String, String)) As String
	'		Return client.HSet(Of String)(key, data)
	'	End Function

	'	''' <summary>设置 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="field">Hash 字段名</param>
	'	''' <param name="value">Hash 值</param>
	'	<Extension>
	'	Public Function HashSet(Of T)(client As RedisClient, key As String, field As String, value As T) As Boolean
	'		Return client.HashSet(key, field, value?.ToObjectString)
	'	End Function

	'	''' <summary>设置 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="data">Hash 数据</param>
	'	<Extension>
	'	Public Function HashSet(Of T)(client As RedisClient, key As String, data As IDictionary(Of String, T)) As String
	'		Return client.HashSet(key, data?.ToDictionary(Function(x) x.Key, Function(x) x.Value?.ToObjectString))
	'	End Function

	'	''' <summary>移除 Hash 数据</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function HashDelete(client As RedisClient, key As String) As Boolean
	'		Return client.Del(key) > 0
	'	End Function

	'	''' <summary>删除 Hash 值</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	''' <param name="fields">要删除的 Hash 字段名</param>
	'	<Extension>
	'	Public Function HashDelete(client As RedisClient, key As String, ParamArray fields As String()) As Long
	'		Return client.HDel(key, fields)
	'	End Function

	'	''' <summary>Hash 数据是否存在指定的字段</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function HashExist(client As RedisClient, key As String, field As String) As Boolean
	'		Return client.HExists(key, field)
	'	End Function

	'	''' <summary>获取 Hash 所有键</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function HashKeys(client As RedisClient, key As String) As String()
	'		Return client.HKeys(key)
	'	End Function

	'	''' <summary>获取 Hash 所有键</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function HashValues(client As RedisClient, key As String) As String()
	'		Return client.HVals(key)
	'	End Function

	'	''' <summary>获取 Hash 所有键</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function HashValues(Of T)(client As RedisClient, key As String) As T()
	'		Return client.HashValues(key)?.Select(Function(x) x.ToValue(Of T)).ToArray
	'	End Function

	'	''' <summary>获取 Hash 的数据数量</summary>
	'	''' <param name="client">Redis 客户端</param>
	'	''' <param name="key">Hash 数据键</param>
	'	<Extension>
	'	Public Function HashCount(client As RedisClient, key As String) As Long
	'		Return client.HLen(key)
	'	End Function

	'#End Region

End Module