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
' 	RedisJson 操作
'
' 	name: Json
' 	create: 2024-06-22
' 	memo: RedisJson 操作
'
' ------------------------------------------------------------

Imports System.Text.Json
Imports FreeRedis

''' <summary>RedisJson 操作</summary>
Public Class RedisJson

	''' <summary>JSON 数据名称</summary>
	Public ReadOnly Name As String

	''' <summary>忽略大小写</summary>
	Public ReadOnly IgnoreCase As String

	''' <summary>Redis 客户端</summary>
	Protected ReadOnly Client As RedisClient

	''' <summary>构造</summary>
	Public Sub New(client As RedisClient, name As String, Optional ignoreCase As Boolean = False)
		If name.IsEmpty Then Throw New Exception("JSON 数据名称为设置")
		If client Is Nothing Then Throw New Exception("Redis 客户端无效")
		If Not client.ModuleExists("ReJSON") Then Throw New Exception("Redis 未安装 JSON 模块")

		Me.Name = name
		Me.Client = client
		Me.IgnoreCase = ignoreCase

		' 检查当前类型是否 JSON，如果不是则删除
		Dim type = client.TypeName(name)
		If type.IsEmpty OrElse Not type.StartsWith("ReJSON") Then client.Del(name)

		' 初始化值，防止无法后续操作
		client.JsonSet(name, "{}", "$", True, False)
	End Sub

#Region "基础操作"

	''' <summary>检查获取操作的字段名称</summary>
	Private Function GetPath(path As String, Optional ByRef errorMessage As String = "") As String
		errorMessage = "字段未设置"
		If path.IsEmpty Then Return ""

		errorMessage = ""
		Return If(IgnoreCase, path.ToLower, path)
	End Function

	'''' <summary>检查获取操作的字段名称</summary>
	'Private Function GetPaths(paths As String(), Optional ByRef errorMessage As String = "") As String()
	'	errorMessage = "字段未设置"
	'	If paths.IsEmpty Then Return Nothing

	'	errorMessage = ""
	'	Return If(IgnoreCase, paths.Select(Function(x) x.ToLower).ToArray, paths)
	'End Function

	''' <summary>JSON 序列化</summary>
	Private Function ToJson(Of T)(value As T) As String
		Dim opts = New JsonSerializerOptions With {
					.PropertyNamingPolicy = If(IgnoreCase, JsonNamingPolicy.KebabCaseLower, Nothing),
					.DictionaryKeyPolicy = If(IgnoreCase, JsonNamingPolicy.KebabCaseLower, Nothing),
					.WriteIndented = False
				}

		Return value.ToJson(opts, value.GetType)
	End Function

#End Region

#Region "GLOBAL"

	''' <summary>原始内容，JSON 原始字符串</summary>
	''' <remarks>未对异常进行拦截，任何错误都将直接抛出异常</remarks>
	Public Property Raw As String
		Get
			Return Client.JsonGet(Name)
		End Get
		Set(value As String)
			Client.JsonSet(Name, value)
		End Set
	End Property

	''' <summary>获取/设置指定路径的值,JSON 原始字符串</summary>
	''' <remarks>未对异常进行拦截，任何错误都将直接抛出异常</remarks>
	Default Public Property Item(path As String) As String
		Get
			Return Client.JsonGet(Name, vbTab, vbCrLf, Nothing, path)
		End Get
		Set(value As String)
			Client.JsonSet(Name, value, path, False, False)
		End Set
	End Property

	''' <summary>获取值的类型</summary>
	''' <param name="path">路径</param>
	Public Function Type(path As String, Optional ByRef errorMessage As String = "") As String()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonType(Name, path), errorMessage)
	End Function

#End Region

#Region "GET"

	''' <summary>获取全局数据</summary>
	Public Function GetAll(Of T)(Optional ByRef errorMessage As String = "") As T
		Return ExecuteResult(Function() Client.JsonGet(Name).ToJsonObject(Of T), errorMessage)
	End Function

	''' <summary>获取 JSON 字符串</summary>
	''' <param name="path">路径</param>
	''' <remarks>数组格式的 JSON 字符串 [****, ****]</remarks>
	Public Function [Get](path As String, Optional ByRef errorMessage As String = "") As String()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function()
								 Dim str = ""
								 If path.StartsWith("$.") Then
									 ' $. 开头返回数组格式的 JSON 字符串
									 str = Client.JsonGet(Name, Nothing, Nothing, Nothing, path)
									 If str.IsEmpty Then Return Nothing
								 Else
									 ' 非 $. 开头返回单个 JSON 字符串
									 str = Client.JsonGet(Name, Nothing, Nothing, Nothing, path)
									 If str.IsEmpty Then Return Nothing

									 str = $"[{str}]"
								 End If

								 Return str.ToJsonList?.Select(Function(x) JsonExtension.ToJson(x, True, False, False)).ToArray
							 End Function, errorMessage)
	End Function

	''' <summary>获取数据</summary>
	''' <param name="path">路径</param>
	Public Function [Get](Of T)(path As String, Optional ByRef errorMessage As String = "") As T()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function()
								 Dim str = ""
								 If path.StartsWith("$.") Then
									 ' $. 开头返回数组格式的 JSON 字符串
									 str = Client.JsonGet(Name, Nothing, Nothing, Nothing, path)
									 If str.IsEmpty Then Return Nothing
								 Else
									 ' 非 $. 开头返回单个 JSON 字符串
									 str = Client.JsonGet(Name, Nothing, Nothing, Nothing, path)
									 If str.IsEmpty Then Return Nothing

									 str = $"[{str}]"
								 End If

								 Return str.ToJsonObject(Of T())
							 End Function, errorMessage)
	End Function

#End Region

#Region "SET"

	''' <summary>设置全局数据</summary>
	''' <param name="value">值</param>
	Public Function SetAll(Of T)(value As T, Optional ByRef errorMessage As String = "") As Boolean
		ExecuteResult(Function()
						  Client.JsonSet(Name, ToJson(value))
						  Return True
					  End Function, errorMessage)

		Return errorMessage.IsEmpty
	End Function

	''' <summary>使用原始字符串更新键数据，不存在则创建，存在则更新</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <param name="status">设置模式：True: 当键不存在时才插入值；False: 当键存时，仅更新值；Default: 不存在新建，存在在更新</param>
	Public Function [Set](path As String, value As String, Optional status As TristateEnum = TristateEnum.DEFAULT, Optional ByRef errorMessage As String = "") As Boolean
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		ExecuteResult(Function()
						  Select Case status
							  Case TristateEnum.TRUE
								  Client.JsonSet(Name, value, path, True, False)

							  Case TristateEnum.FALSE
								  Client.JsonSet(Name, value, path, False, True)

							  Case Else
								  Client.JsonSet(Name, value, path, False, False)
						  End Select
						  Return True
					  End Function, errorMessage)

		Return errorMessage.IsEmpty
	End Function

	''' <summary>更新键数据，不存在则创建，存在则更新</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <param name="status">设置模式：True: 当键存时，仅更新值；False: 当键不存在时才插入值；Default: 不存在新建，存在在更新</param>
	Public Function [Set](Of T)(path As String, value As T, Optional status As TristateEnum = TristateEnum.DEFAULT, Optional ByRef errorMessage As String = "") As Boolean
		Return [Set](path, ToJson(value), status, errorMessage)
	End Function

	''' <summary>更新键数据，不存在则创建，存在则更新</summary>
	Public Function [Set](data As IDictionary(Of String, Object), Optional ByRef errorMessage As String = "") As Boolean
		errorMessage = "无效数据"
		If data.IsEmpty Then Return False

		errorMessage = ""
		Dim paths = If(IgnoreCase, data.Keys.Select(Function(x) x.ToLower).ToArray, data.Keys.ToArray)
		Dim values = data.Values.Select(Function(x) ToJson(x)).ToArray
		Dim names = paths.Select(Function(x) Name).ToArray
		ExecuteResult(Function()
						  Client.JsonMSet(names, values, paths)
						  Return True
					  End Function, errorMessage)

		Return errorMessage.IsEmpty
	End Function

#End Region

#Region "DELETE"

	''' <summary>删除数据</summary>
	Public Function DeleteAll(Optional ByRef errorMessage As String = "") As Long
		Return ExecuteResult(Function() Client.JsonDel(Name), errorMessage)
	End Function

	''' <summary>删除路径数据</summary>
	''' <param name="path">路径</param>
	Public Function Delete(path As String, Optional ByRef errorMessage As String = "") As Long
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonDel(Name, path), errorMessage)
	End Function

#End Region

#Region "MERGE"

	''' <summary>使用原始字符串数据合并</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <remarks>
	''' 值规则
	''' 将现有对象键与值null合并将删除该键
	''' 将现有对象键与非空值合并将更新该值
	''' 合并不存在的对象键会添加键和值
	''' 将现有数组与任何合并值合并，用该值替换整个数组
	''' </remarks>
	Public Function Merge(path As String, value As String, Optional ByRef errorMessage As String = "") As Boolean
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		ExecuteResult(Function()
						  Client.JsonMerge(Name, path, value)
						  Return True
					  End Function, errorMessage)

		Return errorMessage.IsEmpty
	End Function

	''' <summary>数据合并</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <remarks>
	''' 值规则
	''' 将现有对象键与值null合并将删除该键
	''' 将现有对象键与非空值合并将更新该值
	''' 合并不存在的对象键会添加键和值
	''' 将现有数组与任何合并值合并，用该值替换整个数组
	''' </remarks>
	Public Function Merge(Of T)(path As String, value As T, Optional ByRef errorMessage As String = "") As Boolean
		Return Merge(path, ToJson(value), errorMessage)
	End Function

#End Region

#Region "ARRAY"

	''' <summary>数组中追加值</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayAppend(path As String, value As String, Optional ByRef errorMessage As String = "") As Long()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonArrAppend(Name, path, value), errorMessage)
	End Function

	''' <summary>数组中追加值</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayAppend(Of T)(path As String, value As T, Optional ByRef errorMessage As String = "") As Long()
		Return ArrayAppend(path, ToJson(value), errorMessage)
	End Function

	''' <summary>数组中插入值</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <param name="index">开始位置，不设置则从最前面插入</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayInsert(path As String, value As String, Optional index As Long = 0, Optional ByRef errorMessage As String = "") As Long()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonArrInsert(Name, path, index, value), errorMessage)
	End Function

	''' <summary>数组中插入值</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <param name="index">开始位置，不设置则从最前面插入</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayInsert(Of T)(path As String, value As T, Optional index As Long = 0, Optional ByRef errorMessage As String = "") As Long()
		Return ArrayInsert(path, ToJson(value), index, errorMessage)
	End Function

	''' <summary>数组查找，并获取索引值</summary>
	''' <param name="path">路径</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayIndexOf(Of T As Structure)(path As String, value As T, Optional ByRef errorMessage As String = "") As Long()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonArrIndex(Name, path, value), errorMessage)
	End Function

	''' <summary>从数组中的索引中移除并返回一个元素</summary>
	''' <param name="path">路径</param>
	''' <param name="index">索引</param>
	Public Function ArrayPop(path As String, Optional index As Integer = -1, Optional ByRef errorMessage As String = "") As Object()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonArrPop(Name, path, index), errorMessage)
	End Function

	''' <summary>数组修剪，仅保留其实部分的内容</summary>
	''' <param name="path">路径</param>
	''' <param name="start">开始索引</param>
	''' <param name="stop">结束索引</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayTrim(path As String, start As Integer, [stop] As Integer, Optional ByRef errorMessage As String = "") As Long()
		Return ExecuteResult(Function() Client.JsonArrTrim(Name, path, start, [stop]), errorMessage)
	End Function

	''' <summary>数组长度</summary>
	''' <param name="path">路径</param>
	''' <remarks>返回数组的长度</remarks>
	Public Function ArrayLength(path As String, Optional ByRef errorMessage As String = "") As Long()
		Return ExecuteResult(Function() Client.JsonArrLen(Name, path), errorMessage)
	End Function


#End Region

#Region "NUMBER"

	''' <summary>数值相加</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	Public Function NumberIncrBy(path As String, value As Double, Optional ByRef errorMessage As String = "") As String
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonNumIncrBy(Name, path, value), errorMessage)
	End Function

	''' <summary>数值相乘</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	Public Function NumberMultBy(path As String, value As Double, Optional ByRef errorMessage As String = "") As String
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonNumMultBy(Name, path, value), errorMessage)
	End Function

#End Region

#Region "OBJECT"

	''' <summary>对象键列表</summary>
	''' <param name="path">路径</param>
	Public Function ObjectKeys(path As String, Optional ByRef errorMessage As String = "") As String()()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonObjKeys(Name, path), errorMessage)
	End Function

	''' <summary>对象键数量</summary>
	''' <param name="path">路径</param>
	Public Function ObjectLength(path As String, Optional ByRef errorMessage As String = "") As Long()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonObjLen(Name, path), errorMessage)
	End Function

#End Region

#Region "STRING"

	''' <summary>字符串附加文本</summary>
	''' <param name="path">路径</param>
	''' <param name="value">值</param>
	''' <remarks>返回文本的长度</remarks>
	Public Function StringAppend(path As String, value As String, Optional ByRef errorMessage As String = "") As Long()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonStrAppend(Name, value, path), errorMessage)
	End Function

	''' <summary>文本长度</summary>
	''' <param name="path">路径</param>
	Public Function StringLength(path As String, Optional ByRef errorMessage As String = "") As Long()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonStrLen(Name, path), errorMessage)
	End Function

#End Region

#Region "BOOLEAN"

	''' <summary>是非切换</summary>
	''' <param name="path">路径</param>
	Public Function BooleanToggle(path As String, Optional ByRef errorMessage As String = "") As Boolean()
		path = GetPath(path, errorMessage)
		If path.IsEmpty Then Return Nothing

		Return ExecuteResult(Function() Client.JsonToggle(Name, path), errorMessage)
	End Function

#End Region
End Class
