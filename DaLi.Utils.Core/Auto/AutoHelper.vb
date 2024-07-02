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
' 	操作指令
'
' 	name: Auto.AutoHelper
' 	create: 2023-01-02
' 	memo: 操作指令
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Data
Imports System.Reflection
Imports DaLi.Utils.Http
Imports DaLi.Utils.Misc.SnowFlake

Namespace Auto
	''' <summary>操作指令</summary>
	Public NotInheritable Class AutoHelper

		''' <summary>获取基础数据</summary>
		Public Shared Function SysInfo() As Dictionary(Of String, Object)
			Dim Ass = Assembly.GetEntryAssembly
			Return New Dictionary(Of String, Object) From {
					{"powerby", $"<a href=""https://www.hunandali.com/"">{Ass.Company.EmptyValue("大沥网络")}</a>"},
					{"url", "https://www.hunandali.com/"},
					{"company", Ass.Company.EmptyValue("大沥网络")},
					{"copyright", Ass.Copyright.EmptyValue($"©{SYS_NOW.Year} 大沥网络")},
					{"software", Ass.Product.EmptyValue(Ass.Title, Ass.Name)},
					{"description", Ass.Description},
					{"version", Ass.Version},
					{"start", SYS_START},
					{"date", SYS_NOW_DATE},
					{"guid", Guid.NewGuid.ToString},
					{"rnd", SnowFlakeHelper.JsID}
				}
		End Function

		''' <summary>数据合并</summary>
		Public Shared Sub UpdateData(data As SafeKeyValueDictionary, result As IDictionary(Of String, Object))
			If data Is Nothing OrElse result.IsEmpty Then Return

			' 合并结果
			Dim Kvs As KeyValuePair(Of String, Object)()
			SyncLock result
				Kvs = result.ToArray
			End SyncLock

			SyncLock data
				For Each kv In Kvs
					If data.ContainsKey(kv.Key) Then
						' 如果值为字典数据，则会与之前的数据合并
						Dim typeOld = data(kv.Key)?.GetType.IsDictionary(Of String, Object)
						Dim typeNew = kv.Value?.GetType.IsDictionary(Of String, Object)

						If typeOld.HasValue AndAlso typeOld.Value = True AndAlso typeNew.HasValue AndAlso typeNew.Value = True Then
							' 需要合并
							Dim dicOld = TryCast(data(kv.Key), IDictionary(Of String, Object))
							Dim dicNew = TryCast(kv.Value, IDictionary(Of String, Object))

							Dim dic As New SafeKeyValueDictionary(dicOld)
							dic.Update(dicNew)

							data.Update(kv.Key, dic)
						Else
							' 直接替换
							data.Update(kv.Key, kv.Value)
						End If
					Else
						data.Add(kv.Key, kv.Value)
					End If
				Next
			End SyncLock
		End Sub

		''' <summary>代理模式，当本机无此规则时，使用远程代理运行方式</summary>
		Public Shared ProxyMode As Boolean = False

		''' <summary>代理模式，获取 API 客户端的函数</summary>
		Public Shared Property ProxyClient As Func(Of ApiClient)
			Get
				Return RuleProxy.GetApiClient
			End Get
			Set(value As Func(Of ApiClient))
				RuleProxy.GetApiClient = value
			End Set
		End Property

		''' <summary>是否调试模式</summary>
		Public Shared ReadOnly Property IsDebug(data As IDictionary(Of String, Object)) As Boolean
			Get
				If data.IsEmpty OrElse Not data.ContainsKey("_DEBUG_") Then Return False

				Return data("_DEBUG_").Equals(True)
			End Get
		End Property

#Region "规则"
		''' <summary>当前系统中的规则类型</summary>
		Private Shared _RuleTypes As ImmutableList(Of Type) = Nothing

		''' <summary>当前系统中的规则类型</summary>
		Public Shared Property RuleTypes As ImmutableList(Of Type)
			Get
				If _RuleTypes Is Nothing Then
					' 加载所有 Assembly
					Dim typeList As New List(Of Type)
					Dim bastType = GetType(IRule)

					For Each Ass In AssemblyHelper.Assemblies
						Dim Types = AssemblyHelper.Types(Ass, True).Where(Function(x) x.IsClass AndAlso Not x.IsAbstract).ToList
						If Types.NotEmpty Then
							For Each type In Types
								If bastType.IsAssignableFrom(type) Then typeList.Add(type)
							Next
						End If
					Next

					_RuleTypes = typeList.ToImmutableList
				End If

				Return _RuleTypes
			End Get
			Set(value As ImmutableList(Of Type))
				If value.NotEmpty Then _RuleTypes = value
			End Set
		End Property

		''' <summary>获取规则</summary>
		Public Shared ReadOnly Property RuleItem(rule As String) As IRule
			Get
				Dim dic = rule.ToJsonDictionary
				If dic.IsEmpty OrElse Not dic.ContainsKey("type") Then Return Nothing

				' 分析当前规则类型
				Dim type = dic("type")?.ToString
				If type.IsEmpty Then Return Nothing

				' 获取实际规则类型
				Dim ruleType = RuleTypes.
					Where(Function(x) x.Name.Equals(type, StringComparison.OrdinalIgnoreCase)).
					Select(Function(x) x).
					FirstOrDefault

				If ruleType IsNot Nothing Then
					' 反序列为实际规则
					Return TryCast(rule.ToJsonObject(ruleType), IRule)
				ElseIf ProxyMode Then
					' 代理运行模式，返回代理规则
					Return New RuleProxy With {.Rule = rule}
				Else
					' 返回无效规则
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>获取规则列表</summary>
		''' <param name="rules">规则列表</param>
		''' <param name="validate">是否进行检查，移除无效规则</param>
		Public Shared ReadOnly Property RuleList(rules As String, validate As Boolean) As List(Of IRule)
			Get
				Dim list = rules.ToJsonList
				If list.IsEmpty Then Return Nothing

				' 获取规则，如果存在任一无效规则，则此规则列表无效
				Dim ret = list.Select(Function(x) RuleItem(JsonExtension.ToJson(x, False, False, False))).ToList
				If ret.Any(Function(x) x Is Nothing) Then Return Nothing

				' 不检查直接返回
				If Not validate Then Return ret

				' 移除禁用规则
				ret = ret?.Where(Function(x) x IsNot Nothing AndAlso x.Enabled).ToList
				If ret.IsEmpty Then Return Nothing

				' 检查是否存在无效规则
				If ret.Any(Function(x) Not x.Validate) Then Return Nothing

				Return ret
			End Get
		End Property

		''' <summary>执行操作</summary>
		Public Shared Function RuleExecute(rule As String, ByRef data As IDictionary(Of String, Object)) As AutoMessage
			Dim item = RuleItem(rule)
			If item Is Nothing Then Return New AutoMessage With {.Message = "规则参数错误或者规则无效"}

			If data.IsEmpty Then data = New SafeKeyValueDictionary
			If data.ContainsKey("_sys") Then
				data("_sys") = SysInfo()
			Else
				data.Add("_sys", SysInfo())
			End If

			Return item.Execute(data)
		End Function

#End Region

#Region "流程"

		''' <summary>执行操作</summary>
		''' <param name="rules">规则列表</param>
		''' <param name="sourceData">交换数据</param>
		''' <param name="message">消息</param>
		Public Shared Function FlowExecute(rules As String, Optional fullFlow As Boolean = True, Optional ByRef sourceData As SafeKeyValueDictionary = Nothing, Optional ByRef message As AutoMessage = Nothing) As IDictionary(Of String, Object)
			Return FlowExecute(RuleList(rules, False), True, fullFlow, sourceData, message)
		End Function

		''' <summary>执行操作</summary>
		''' <param name="rules">规则列表</param>
		''' <param name="data">交换数据</param>
		''' <param name="fullFlow">是否包含整个流程，即包含 FlowStart 与 FlowEnd</param>
		''' <param name="message">消息</param>
		Public Shared Function FlowExecute(rules As IEnumerable(Of IRule), Optional validateRule As Boolean = True, Optional fullFlow As Boolean = True, Optional ByRef data As SafeKeyValueDictionary = Nothing, Optional ByRef message As AutoMessage = Nothing) As IDictionary(Of String, Object)
			message = If(message, New AutoMessage)

			If validateRule Then
				' 移除禁用规则
				Dim ruleList = rules?.Where(Function(x) x IsNot Nothing AndAlso x.Enabled).ToList
				If ruleList.IsEmpty Then
					message.Message = "无任何有效规则"
					Return Nothing
				End If

				' 检查是否存在无效规则
				If ruleList.Any(Function(x) Not x.Validate) Then
					message.Message = "任务中存在无效规则"
					Return Nothing
				End If

				rules = ruleList
			End If

			' 流程中的数据
			data = If(data, New SafeKeyValueDictionary)
			Dim debug = IsDebug(data)

			' 非调试模式移除调试模块
			If Not debug Then rules = rules.Where(Function(x) x IsNot Nothing AndAlso Not x.Type.IsSame("debug")).ToList

			If rules.IsEmpty Then
				message.Message = "无任何有效规则"
				Return Nothing
			End If

			If fullFlow Then
				' 验证为完整流程
				message.Message = "完整流程应该以【流程开始】模块开始；以【流程结束】模块结束；并且还至少包含一个有效规则"
				If rules.Count < 3 Then Return Nothing
				If Not rules.First.Type.IsSame("FlowStart") OrElse Not rules.Last.Type.IsSame("FlowFinish") Then Return Nothing
			End If

			' 获取流程名字，尝试从上下文数据中获取
			Dim flowName = ""
			Dim flow = TryCast(data("_flow"), IDictionary(Of String, Object))
			If flow.NotEmpty AndAlso flow.ContainsKey("name") Then flowName = flow("name")
			flowName = If(flowName.IsEmpty, "", $"[{flowName}]")

			' 默认执行成功，当发生中断或者异常是标记 false
			Dim flag = False
			Dim flowResult As New SafeKeyValueDictionary

			Dim s As New Stopwatch
			s.Start()

			Try
				' 流程结果，仅记录明确公开数据
				Dim forData As New SafeKeyValueDictionary(data)

				For Each item In rules
					' 执行规则，一旦非有效执行结果通过异常跳出
					Dim msg = item.Execute(forData)

					' 重新复制，防止规则自己变更了 output
					msg.Output = item.Output

					' 记录日志
					message.Add(msg)

					' 检查结果是否字典数据
					If msg.Result IsNot Nothing Then
						Dim ret As IDictionary(Of String, Object) = Nothing
						Dim isDic = msg.Result.GetType.IsDictionary(Of String, Object)

						If item.OutResult AndAlso isDic Then
							' 需要将执行结果直接公开
							ret = msg.Result

						ElseIf msg.Output.NotEmpty Then
							' 不公开，赋值到变量
							ret = New Dictionary(Of String, Object) From {{msg.Output, msg.Result}}

						ElseIf isDic Then
							' 其他结果为字典则直接赋值
							ret = msg.Result
						End If

						' 循环结果合并
						If ret IsNot Nothing Then
							UpdateData(forData, ret)
							UpdateData(flowResult, ret)
						End If
					End If

					' 未执行成功，退出
					If Not msg.Success Then Exit For
				Next

				' 验证结果
				' 完整模式以 _flow.flag 结果为最终状态
				' 其他模式直接返回成功
				Dim resultContent = $"执行完成"
				If fullFlow Then
					' 最后消息的内容
					Dim last = message.Children.LastOrDefault
					If last IsNot Nothing Then
						flag = last.Success
						resultContent = last.Message

						' 记录执行结果
						' FlowFinish 结果会将最终结果存储 在 _Result 中
						message.Result = TryCast(last.Result, IDictionary(Of String, Object))?.Item("_Result")
					End If
				Else
					message.Result = message.Children.LastOrDefault?.Result
					flag = True
				End If

				message.SetSuccess(flag, resultContent)
			Catch ex As AutoException
				' 继续触发异常，非完整流程保持异常，以便上级捕获
				message.SetSuccess(False, $"流程{flowName}中断 {ex.Message}")

				' 附加错误信息
				message.Add(ex.AutoMessage)

				If Not fullFlow Then Throw

			Catch ex As Exception
				' 其他异常，记录标记错误
				message.SetSuccess(False, $"流程{flowName}异常 {ex.Message}")
			End Try

			s.Stop()

			' 全局数据合并
			UpdateData(data, flowResult)
			UpdateData(data, New Dictionary(Of String, Object) From {
					   {"_flow", New Dictionary(Of String, Object) From {
								{"message", message.GetMessage},
								{"duration", s.ElapsedMilliseconds},
								{"durationDisplay", s.Elapsed.Show}
							}
					   }
					})

			' 执行成功，返回结果组合，否则返回空值
			Return If(flag, flowResult, Nothing)
		End Function

		'''' <summary>循环，判断内执行规则</summary>
		'''' <param name="rules">规则列表</param>
		'''' <param name="data">当前流程交换数据</param>
		'''' <param name="message">消息</param>
		'''' <returns></returns>
		'Public Shared Function FlowExecute(rules As IEnumerable(Of IRule), Optional ByRef data As SafeKeyValueDictionary = Nothing, Optional ByRef message As AutoMessage = Nothing) As IDictionary(Of String, Object)
		'	' 创建新的消息对象，防止多线程时异常
		'	' 复制一份参数，以便保持调试方式
		'	Dim resultMessage As New AutoMessage
		'	resultMessage.Copy(message)

		'	data = If(data, New SafeKeyValueDictionary)

		'	Dim dic = FlowExecute(rules, False, False, data, resultMessage)

		'	If message IsNot Nothing Then
		'		SyncLock message
		'			message.SetSuccess(dic IsNot Nothing, resultMessage.Message)
		'			message.Children = resultMessage.Children
		'		End SyncLock
		'	End If

		'	Return dic
		'End Function

#End Region

#Region "获取值"

		''' <summary>获取变量数据</summary>
		''' <param name="source">原始值（变量名、模板）</param>
		''' <param name="data">数据</param>
		''' <remarks>
		''' 支持两种格式类似 asp 的 ＜% %＞；类似 js 的 ${}
		''' 1. 原始值包含${}则使用文本模板替换，返回文本值
		''' 2. 原始值使用＜% %＞包含检查上下文数据中是否包含此原始值，包含直接返回上下文数据
		''' 3. 如果原始值中包含小数点，则尝试分级从上下文获取数据，全部存在则返回上下文件数据
		''' 4. 都不成功，直接返回原始值文本
		''' 5. 如果不存在上下文件数据，直接移除${}内容
		''' </remarks>
		Public Shared Function GetVar(source As String, data As IDictionary(Of String, Object)) As Object
			' 未设置其中任意一项都返回原始内容
			If source.IsEmpty Then Return Nothing

			' 使用变量名
			If source.StartsWith("<%") AndAlso source.EndsWith("%>") Then
				' 无上下文数据，直接返回
				If data.IsEmpty Then Return Nothing

				' 存在此变量，直接返回对应值
				source = source.Substring(2, source.Length - 4)
				If data.ContainsKey(source) Then Return data(source)

				' 含小数点的变量名
				If source.Contains("."c) AndAlso Not source.StartsWith("."c) AndAlso Not source.EndsWith("."c) Then
					' 尝试将数据转换成 JSON 后变成集合
					Dim value As Object = data.ToJson.ToJsonCollection.Value
					If value Is Nothing Then Return Nothing

					Dim keys = source.Split("."c)
					Dim max = keys.Length - 1

					For I = 0 To max
						Dim key = keys(I)
						If key.IsEmpty OrElse value Is Nothing Then Exit For

						If key.IsNumber Then
							' 数字键
							Dim count = key.ToInteger

							Dim list = TryCast(value, IEnumerable(Of Object))
							If list Is Nothing OrElse list.Count < count Then Exit For

							value = list(count)
						Else
							' 字符键
							Dim dic = TryCast(value, IDictionary(Of String, Object))
							If dic Is Nothing OrElse Not dic.ContainsKey(key) Then Exit For

							value = dic(key)
						End If

						' 匹配到最后一条，返回结果
						If I = max Then Return value
					Next
				End If

				' 匹配不到任何数据，返回空值
				Return Nothing
			End If

			' 非文本模板，且非变量，直接返回原值
			Return source.FormatTemplateEx(data, "${", "}")
		End Function

		''' <summary>获取变量数据并转换成文本</summary>
		''' <param name="source">原始值（变量名、模板）</param>
		''' <param name="data">数据</param>
		''' <param name="math">是否将获取的值进行一次计算，计算失败则返回原值</param>
		Public Shared Function GetVarString(source As String, data As IDictionary(Of String, Object), Optional math As Boolean = False) As String
			Dim ret = TypeExtension.ToObjectString(GetVar(source, data))

			If math Then
				Try
					ret = New DataTable().Compute(ret, Nothing)
				Catch ex As Exception
				End Try
			End If

			Return ret
		End Function

		''' <summary>获取变量数据并转换成对象，转换不成功则尝试使用，JSON 对象或者集合，都不成功则返回文本</summary>
		''' <param name="source">原始值（变量名、模板）</param>
		''' <param name="data">数据</param>
		Public Shared Function GetVarObject(source As String, data As IDictionary(Of String, Object)) As Object
			Dim var = GetVar(source, data)
			If var Is Nothing Then Return Nothing

			' 对于文本尝试转换成 JSON
			If var.GetType.IsString Then
				Dim obj = var.ToString.ToJsonObject(Function(x, t) If(t.IsString, GetVar(x, data), x))
				If obj IsNot Nothing Then var = obj
			End If

			Return var
		End Function

#End Region

#Region "相关操作"

		''' <summary>自动分析数据，尝试将列表转换成字典数据</summary>
		''' <param name="value">需要转换的数据</param>
		''' <param name="returnNothing">转换无效是否返回空值</param>
		Public Shared Function List2Dictionary(value As Object, returnNothing As Boolean) As Object
			If value Is Nothing Then Return Nothing

			' 转换列表
			If value.GetType.IsList(Of Object) Then
				Dim items = TryCast(value, IEnumerable(Of Object))
				If items.NotEmpty Then
					' 有效数据，尝试转换
					Dim dic As New Dictionary(Of String, Object)
					For N = 0 To items.Count - 1
						dic.Add($"_{N + 1}", items(N))
					Next
					Return dic
				End If

			ElseIf value.GetType.IsDictionary(Of String, Object) Then
				' 字典直接返回
				Return value
			End If

			' 其他数据，按条件返回
			Return If(returnNothing, Nothing, value)
		End Function

#End Region

	End Class
End Namespace