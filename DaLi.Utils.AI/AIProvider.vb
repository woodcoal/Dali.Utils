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
' 	AI 统一接口
'
' 	name: AI
' 	create: 2024-06-28
' 	memo: AI 统一接口
'
' ------------------------------------------------------------

Imports DaLi.Utils.AI.Model

''' <summary>AI 统一接口</summary>
Public Class AIProvider

	''' <summary>接口类型</summary>
	Public ReadOnly Type As AITypeEnum

	''' <summary>接口地址</summary>
	Public ReadOnly Url As String

	''' <summary>接口密钥</summary>
	Public ReadOnly Key As String

	''' <summary>当前模型</summary>
	Public ReadOnly Model As String

	''' <summary>模型参数</summary>
	Public ReadOnly Params As IDictionary(Of String, Object)

	''' <summary>系统角色，具体的提示内容</summary>
	Public Property System As String

	''' <summary>文本补全接口</summary>
	Private _TextAI As Object

	''' <summary>聊天接口</summary>
	Private _ChatAI As Object

	''' <summary>构造</summary>
	''' <param name="type">AI 接口类型</param>
	''' <param name="url">Api 地址</param>
	''' <param name="model">模型</param>
	''' <param name="params">模型参数</param>
	Public Sub New(type As AITypeEnum, Optional url As String = "", Optional key As String = "", Optional model As String = "", Optional params As IDictionary(Of String, Object) = Nothing)
		Me.Type = type
		Me.Url = url
		Me.Key = key
		Me.Model = model
		Me.Params = params

		Select Case type
			Case AITypeEnum.OLLAMA, AITypeEnum.ONEAPI, AITypeEnum.DIFY
			Case Else
				Throw New Exception("暂无此 AI 接口")
		End Select
	End Sub

	''' <summary>生成补全</summary>
	''' <param name="prompt">生成响应的提示</param>
	''' <returns>同步方式，一次性返回所有结果</returns>
	Public Function Text(prompt As String) As TextResult
		Select Case Type
			Case AITypeEnum.OLLAMA
				_TextAI = If(_TextAI, New Ollama.Text(Url, Model, Params))

				Dim IO As Ollama.Text = _TextAI
				IO.System = System
				Return IO.Process(prompt)

			Case AITypeEnum.ONEAPI
				' 文本补全接口有问题，用聊天接口代替
				_ChatAI = If(_ChatAI, New OneApi.Chat(Url, Key, Model, Params))

				Dim IO As OneApi.Chat = _ChatAI
				IO.System = System
				Dim res = IO.Process(prompt)
				Return New TextResult With {.Model = Model, .Success = res.Success, .Content = res.Message?.Content, .Last = res.Last}

			Case AITypeEnum.DIFY
				_TextAI = If(_TextAI, New Dify.Text(Url, Model))

				Dim IO As Dify.Text = _TextAI
				Return IO.Process(prompt, Params)
		End Select

		Return Nothing
	End Function

	''' <summary>生成补全</summary>
	''' <param name="prompt">生成响应的提示</param>
	''' <returns>异步方式，流式返回结果</returns>
	Public Async Function TextAsync(prompt As String, Optional callback As Action(Of ProcessStatusEnum, String) = Nothing) As Task(Of TextResult)
		Select Case Type
			Case AITypeEnum.OLLAMA
				_TextAI = If(_TextAI, New Ollama.Text(Url, Model, Params))

				Dim IO As Ollama.Text = _TextAI
				IO.System = System
				Return Await IO.ProcessAsync(prompt, Nothing, callback)

			Case AITypeEnum.ONEAPI
				' 文本补全接口有问题，用聊天接口代替
				_ChatAI = If(_ChatAI, New OneApi.Chat(Url, Key, Model, Params))

				Dim IO As OneApi.Chat = _ChatAI
				IO.System = System
				Dim res = Await IO.ProcessAsync(prompt, Nothing, callback)
				Return New TextResult With {.Model = Model, .Success = res.Success, .Content = res.Message?.Content, .Last = res.Last}

			Case AITypeEnum.DIFY
				_TextAI = If(_TextAI, New Dify.Text(Url, Model))

				Dim IO As Dify.Text = _TextAI
				Return Await IO.ProcessAsync(prompt, Params, Nothing, callback)
		End Select

		Return Nothing
	End Function


	''' <summary>聊天</summary>
	''' <param name="prompt">生成响应的提示</param>
	''' <returns>同步方式，一次性返回所有结果</returns>
	Public Function Chat(prompt As String) As ChatResult
		Select Case Type
			Case AITypeEnum.OLLAMA
				_ChatAI = If(_ChatAI, New Ollama.Chat(Url, Model, Params))

				Dim IO As Ollama.Chat = _ChatAI
				IO.System = System
				Return IO.Process(prompt)

			Case AITypeEnum.ONEAPI
				_ChatAI = If(_ChatAI, New OneApi.Chat(Url, Key, Model, Params))

				Dim IO As OneApi.Chat = _ChatAI
				IO.System = System
				Return IO.Process(prompt)

			Case AITypeEnum.DIFY
				_ChatAI = If(_ChatAI, New Dify.Chat(Url, Model))

				Dim IO As Dify.Chat = _ChatAI
				Return IO.Process(prompt, Params)
		End Select

		Return Nothing
	End Function

	''' <summary>聊天</summary>
	''' <param name="prompt">生成响应的提示</param>
	''' <returns>异步方式，流式返回结果</returns>
	Public Async Function ChatAsync(prompt As String, Optional callback As Action(Of ProcessStatusEnum, String) = Nothing) As Task(Of ChatResult)
		Select Case Type
			Case AITypeEnum.OLLAMA
				_ChatAI = If(_ChatAI, New Ollama.Chat(Url, Model, Params))

				Dim IO As Ollama.Chat = _ChatAI
				IO.System = System
				Return Await IO.ProcessAsync(prompt, Nothing, callback)

			Case AITypeEnum.ONEAPI
				_ChatAI = If(_ChatAI, New OneApi.Chat(Url, Key, Model, Params))

				Dim IO As OneApi.Chat = _ChatAI
				IO.System = System
				Return Await IO.ProcessAsync(prompt, Nothing, callback)

			Case AITypeEnum.DIFY
				_ChatAI = If(_ChatAI, New Dify.Chat(Url, Model))

				Dim IO As Dify.Chat = _ChatAI
				Return Await IO.ProcessAsync(prompt, Params, Nothing, callback)
		End Select

		Return Nothing
	End Function

End Class
