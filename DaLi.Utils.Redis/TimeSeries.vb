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
' 	时序数据操作
'
' 	name: TimeSeries
' 	create: 2024-06-17
' 	memo: 基于 Redis Timeseries 时序数据操作。
' 		  以 collection 为一组数据集，每个序列都以 collection 开头，使用冒号间隔
'
' ------------------------------------------------------------

Imports FreeRedis
Imports FreeRedis.TimeSeries
Imports FreeRedis.TimeSeries.Model

''' <summary>时序数据操作</summary>
Public Class TimeSeries

	''' <summary>时序系列数据集</summary>
	Public ReadOnly Series As String

	''' <summary>Redis 客户端</summary>
	Protected ReadOnly Client As RedisClient

	''' <summary>所有参数</summary>
	Public ReadOnly Keys As NameValueDictionary

	''' <summary>构造</summary>
	Public Sub New(client As RedisClient, series As String)
		If series.IsEmpty Then Throw New Exception("时序数据集名称必须设置")
		If client Is Nothing Then Throw New Exception("Redis 客户端无效")
		If Not client.ModuleExists("timeseries") Then Throw New Exception("Redis 未安装 TimeSeries 模块")

		Me.Series = $"TimeSeries:{series}:"
		Me.Client = client

		Keys = New NameValueDictionary

		' 获取所有键
		Call UpdateKeys()
	End Sub

	''' <summary>更新所有键</summary>
	Private Sub UpdateKeys()
		SyncLock Keys
			Keys.Clear()

			Dim Prefix = Client.Prefix.EmptyValue
			Dim PrefixSeries = $"{Prefix}{Series}"

			Dim LenPrefix = Prefix.Length
			Dim LenSeries = PrefixSeries.Length

			While True
				Dim data = Client.Scan(0, $"*{Series}*", 100, "")
				If data.items.Length < 1 Then Exit While

				For Each item In data.items
					If item.StartsWith(PrefixSeries) Then Keys.Add(item.Substring(LenSeries), item.Substring(LenPrefix))
				Next

				If data.cursor = 0 Then Exit While
			End While
		End SyncLock
	End Sub

	''' <summary>初始化序列，完成后返回 Key 名称</summary>
	''' <param name="key">要插件的键名称</param>
	''' <param name="create">键不存在时是否创建，如果查询模式则不需要去创建</param>
	Private Function UpdateSeries(key As String, create As Boolean, Optional ByRef errorMessage As String = "") As String
		If key.IsEmpty Then
			errorMessage = "序列标识未设置"
			Return Nothing
		End If

		' 已经存在，无需处理
		If Keys.ContainsKey(key) Then
			errorMessage = ""
			Return Keys(key)
		End If

		' 不存在，但不新建时直接返回控制
		If Not create Then
			errorMessage = "序列标识不存在"
			Return Nothing
		End If

		' 创建序列
		Dim opt As New CreateOption
		opt.AddLabel("_series_", Series)
		opt.AddLabel("_name_", key)
		opt.AddLabel("_update_", SYS_NOW_STR)

		Dim command = New CommandPacket("TS.CREATE").InputKey($"{Series}{key}")
		opt?.UpdateCommand(command)

		Dim cmd = command.WriteTarget

		' 添加成功后重新生成键
		If Client.TSCreate($"{Series}{key}", opt, errorMessage) Then UpdateKeys()

		' 返回数据
		If Keys.ContainsKey(key) Then Return Keys(key) Else Return Nothing
	End Function

#Region "信息"

	''' <summary>获取序列名称</summary>
	Public ReadOnly Property SeriesName(key As String) As String
		Get
			Return UpdateSeries(key, False)
		End Get
	End Property

	''' <summary>获取序列信息</summary>
	Public Function Info(key As String, Optional ByRef errorMessage As String = "") As CollectionInfo
		key = UpdateSeries(key, False, errorMessage)
		If key.IsEmpty Then Return Nothing

		Return Client.TSInfo(key, True, errorMessage)
	End Function

	''' <summary>调整序列信息</summary>
	Public Function Alert(key As String, options As CreateOption, Optional ByRef errorMessage As String = "") As Boolean
		If options Is Nothing Then
			errorMessage = "参数无效"
			Return False
		End If

		key = UpdateSeries(key, False, errorMessage)
		If key.IsEmpty Then Return False


		' 添加默认数据
		options.AddLabel("_series_", Series)
		options.AddLabel("_name_", key)
		options.AddLabel("_update_", SYS_NOW_STR)

		Return Client.TSAlert(key, options, errorMessage)
	End Function

	''' <summary>创建规则</summary>
	''' <param name="key">字段</param>
	''' <param name="ruleName">规则名称</param>
	''' <param name="aggregation">聚合方式</param>
	''' <param name="duration">聚合时间间隔</param>
	Public Function CreateRule(key As String, ruleName As String, Optional aggregation As AggregationEnum = AggregationEnum.AVG, Optional duration As Long = 3600000, Optional ByRef errorMessage As String = "") As Boolean
		key = UpdateSeries(key, False, errorMessage)
		If key.IsEmpty Then Return -1

		ruleName = UpdateSeries(ruleName, True, errorMessage)
		If ruleName.IsEmpty Then Return -1

		Return Client.TSCreateRule(key, ruleName, aggregation, duration, errorMessage)
	End Function

	''' <summary>移除</summary>
	Public Function Remove(key As String, Optional ByRef errorMessage As String = "") As Boolean
		Dim series = UpdateSeries(key, False, errorMessage)
		If series.IsEmpty Then Return False

		Dim flag = Client.Del(series, errorMessage)
		If flag Then Keys.Remove(key)

		Return flag
	End Function

#End Region

#Region "添加样本数据"

	''' <summary>插入记录，并返回时间戳</summary>
	Public Function Insert(key As String, value As Double, Optional time As Date = Nothing, Optional ByRef errorMessage As String = "") As Long
		key = UpdateSeries(key, True, errorMessage)
		If key.IsEmpty Then Return -1

		Return Client.TSAdd(key, New TimeStamp(time), value, Nothing, errorMessage)?.Ticks
	End Function

	''' <summary>批量插入记录，并返回插入数量</summary>
	Public Function Insert(key As String, data As IDictionary(Of Date, Double), Optional ByRef errorMessage As String = "") As Integer
		If data.IsEmpty Then
			errorMessage = "无任何有效数据"
			Return -1
		End If

		key = UpdateSeries(key, True, errorMessage)
		If key.IsEmpty Then Return -1

		' 分析并调整数据
		Dim datas = data.Select(Function(x) (key, New TimeStamp(TimeStampEnum.NOW), x.Value))

		' 批量添加
		Return Client.TSMAdd(datas, errorMessage)?.Length
	End Function

	''' <summary>批量插入记录，并返回插入数量</summary>
	''' <param name="data">序列标识，时间，数据 集合</param>
	Public Function Insert(data As IEnumerable(Of (key As String， time As Date, value As Double)), Optional ByRef errorMessage As String = "") As Integer
		If data.IsEmpty Then
			errorMessage = "无任何有效数据"
			Return -1
		End If

		' 字段处理
		Dim keys = data.Select(Function(x) x.key).ToDictionary(Function(x) x, Function(x) UpdateSeries(x, True))
		If keys.Values.Any(Function(x) x.IsEmpty) Then
			errorMessage = "存在序列标识异常"
			Return -1
		End If

		' 分析并调整数据
		Dim datas = data.Select(Function(x) (keys(x.key), New TimeStamp(x.time), x.value))

		' 批量添加
		Return Client.TSMAdd(datas, errorMessage)?.Length
	End Function

	''' <summary>以当前时间为时间戳批量插入记录</summary>
	''' <param name="data">序列标识，数据 集合</param>
	Public Function Insert(data As IDictionary(Of String, Double), Optional time As Date = Nothing, Optional ByRef errorMessage As String = "") As Integer
		If data.IsEmpty Then
			errorMessage = "无任何有效数据"
			Return -1
		End If

		' 分析并调整数据
		time = If(time.IsValidate, time, SYS_NOW_DATE)
		Dim datas = data.Select(Function(x) (x.Key, Date.Now, x.Value)).ToList

		' 批量添加
		Return Insert(datas, errorMessage)
	End Function

#End Region

#Region "移除样本数据"

	''' <summary>删除记录</summary>
	Public Function Delete(key As String, time As Date, Optional ByRef errorMessage As String = "") As Boolean
		key = UpdateSeries(key, False, errorMessage)
		If key.IsEmpty Then Return False

		Dim timeStamp = New TimeStamp(time)
		Return Client.TSDel(key, timeStamp, timeStamp, errorMessage) > 0
	End Function

	''' <summary>删除记录</summary>
	Public Function Delete(key As String, timeStart As Date, timeEnd As Date, Optional ByRef errorMessage As String = "") As Long
		key = UpdateSeries(key, False, errorMessage)
		If key.IsEmpty Then Return -1

		Return Client.TSDel(key, New TimeStamp(timeStart), New TimeStamp(timeEnd), errorMessage)
	End Function

#End Region

#Region "搜索查询"

	''' <summary>指定序列查询</summary>
	''' <param name="key">要查询的 Key 名称</param>
	''' <param name="queryOption">搜索条件</param>
	''' <param name="desc">是否倒序排列结果</param>
	Public Function Query(key As String, queryOption As Action(Of QueryOption), Optional desc As Boolean = False, Optional ByRef errorMessage As String = "") As SampleBase()
		Dim options As New QueryOption(New TimeStamp(TimeStampEnum.MIN), New TimeStamp(TimeStampEnum.MAX)) With {.Aggregation = (AggregationEnum.AVG, 3600000)}
		queryOption(options)

		Return Query(key, options, desc, errorMessage)
	End Function

	''' <summary>指定序列查询</summary>
	''' <param name="key">要查询的 Key 名称</param>
	''' <param name="queryOption">搜索条件</param>
	''' <param name="desc">是否倒序排列结果</param>
	Public Function Query(key As String, queryOption As QueryOption, Optional desc As Boolean = False, Optional ByRef errorMessage As String = "") As SampleBase()
		If queryOption Is Nothing Then Return Nothing

		key = UpdateSeries(key, False, errorMessage)
		If key.IsEmpty Then Return Nothing

		If desc Then
			Return Client.TSRevRange(key, queryOption, errorMessage)
		Else
			Return Client.TSRange(key, queryOption, errorMessage)
		End If
	End Function

	''' <summary>指定序列查询</summary>
	''' <param name="key">要查询的 Key 名称</param>
	''' <param name="timeStart">开始时间，设置则为最早时间</param>
	''' <param name="timeEnd">结束时间，不设置则为最晚时间</param>
	''' <param name="duration">数据集合时长（毫秒），不设置则为 1 小时</param>
	''' <param name="align">
	''' 聚合初始数据对其方式
	''' start Or -: The reference timestamp will be the query start interval time (fromTimestamp) which can't be -
	''' end Or +: The reference timestamp will be the query End interval time (toTimestamp) which can't be +
	''' A specific timestamp: align the reference timestamp To a specific time
	''' </param>
	''' <param name="desc">是否倒序排列结果</param>
	Public Function Query(key As String,
						  Optional timeStart As Date = Nothing,
						  Optional timeEnd As Date = Nothing,
						  Optional aggregation As AggregationEnum = AggregationEnum.AVG,
						  Optional duration As Long = 3600000,
						  Optional align As String = "",
						  Optional desc As Boolean = False,
						  Optional ByRef errorMessage As String = "") As SampleBase()
		Dim ts = If(timeStart.IsValidate, New TimeStamp(timeStart), New TimeStamp(TimeStampEnum.MIN))
		Dim te = If(timeStart.IsValidate, New TimeStamp(timeEnd), New TimeStamp(TimeStampEnum.MAX))

		Dim opt As New QueryOption(ts, te) With {
			.Aggregation = (aggregation, duration),
			.Align = align
		}

		Return Query(key, opt, desc, errorMessage)
	End Function

	''' <summary>多序列查询</summary>
	''' <param name="queryOption">搜索条件</param>
	''' <param name="desc">是否倒序排列结果</param>
	Public Function Query(queryOption As Action(Of QueryOptionEx), Optional desc As Boolean = False, Optional ByRef errorMessage As String = "") As SampleData()
		Dim options As New QueryOptionEx(New TimeStamp(TimeStampEnum.MIN), New TimeStamp(TimeStampEnum.MAX), New LabelFilter("type", True, Keys.Keys.ToArray)) With {
			.Aggregation = (AggregationEnum.AVG, 3600000)
		}

		queryOption(options)

		Return Query(options, desc, errorMessage)
	End Function

	''' <summary>多序列查询</summary>
	''' <param name="queryOption">搜索条件</param>
	''' <param name="desc">是否倒序排列结果</param>
	Public Function Query(queryOption As QueryOptionEx, Optional desc As Boolean = False, Optional ByRef errorMessage As String = "") As SampleData()
		If queryOption Is Nothing Then Return Nothing

		' 附加默认筛选
		Dim filter As New List(Of LabelFilter) From {New LabelFilter("_series_", True, Series)}
		If queryOption.Filter?.Any Then filter.AddRange(queryOption.Filter)
		queryOption.Filter = filter.Where(Function(x) x IsNot Nothing).ToList

		If desc Then
			Return Client.TSMRevRange(queryOption, errorMessage)
		Else
			Return Client.TSMRange(queryOption, errorMessage)
		End If
	End Function

	''' <summary>多序列查询</summary>
	''' <param name="timeStart">开始时间，设置则为最早时间</param>
	''' <param name="timeEnd">结束时间，不设置则为最晚时间</param>
	''' <param name="filter">标签筛选</param>
	''' <param name="duration">数据集合时长（毫秒），不设置则为 1 小时</param>
	''' <param name="align">
	''' 聚合初始数据对其方式
	''' start Or -: The reference timestamp will be the query start interval time (fromTimestamp) which can't be -
	''' end Or +: The reference timestamp will be the query End interval time (toTimestamp) which can't be +
	''' A specific timestamp: align the reference timestamp To a specific time
	''' </param>
	''' <param name="desc">是否倒序排列结果</param>
	Public Function Query(Optional timeStart As Date = Nothing,
						  Optional timeEnd As Date = Nothing,
						  Optional filter As LabelFilter = Nothing,
						  Optional aggregation As AggregationEnum = AggregationEnum.AVG,
						  Optional duration As Long = 3600000,
						  Optional align As String = "",
						  Optional desc As Boolean = False,
						  Optional ByRef errorMessage As String = "") As SampleData()
		Dim ts = If(timeStart.IsValidate, New TimeStamp(timeStart), New TimeStamp(TimeStampEnum.MIN))
		Dim te = If(timeStart.IsValidate, New TimeStamp(timeEnd), New TimeStamp(TimeStampEnum.MAX))

		Dim opt As New QueryOptionEx(ts, te, filter) With {
			.Aggregation = (aggregation, duration),
			.Align = align
		}

		Return Query(opt, desc, errorMessage)
	End Function
#End Region

End Class
