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
' 	验证码操作
'
' 	name: Helper.CaptchaHelper
' 	create: 2023-12-12
' 	memo: 验证码操作
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.IO
Imports Microsoft.AspNetCore.Http

Namespace Helper

	''' <summary>验证码操作</summary>
	Public NotInheritable Class CaptchaHelper

		''' <summary>验证码组件</summary>
		Private ReadOnly _Instance As ImmutableDictionary(Of String, ICaptcha) = ImmutableDictionary.Create(Of String, ICaptcha)

		Public Sub New()
			'' 获取所有验证码接口
			'Dim captchas = SYS.Plugins.GetInstances(Of ICaptcha)
			'captchas.ForEach(Sub(captcha)
			'					 Dim key = captcha.Name.EmptyValue(captcha.GetType.FullName)
			'					 If Not _Instance.ContainsKey(key) Then _Instance.Add(key, captcha)
			'				 End Sub)

			_Instance = SYS.Plugins.GetInstances(Of ICaptcha).ToImmutableDictionary(Function(x) x.Name, Function(x) x)
		End Sub

		''' <summary>获取验证码组件，不存在则返回第一条数据</summary>
		''' <param name="name">组件名称</param>
		Default Public ReadOnly Property Item(name As String) As ICaptcha
			Get
				If name.NotEmpty AndAlso _Instance.ContainsKey(name) Then Return _Instance(name)
				Return _Instance.FirstOrDefault.Value
			End Get
		End Property

		''' <summary>验证码对象</summary>
		Public Shared ReadOnly Instance As New Lazy(Of CaptchaHelper)(Function() New CaptchaHelper())

		''' <summary>生成验证码图形与验证内容</summary>
		''' <param name="name">验证码规则标识，不设置则使用默认规则</param>
		''' <param name="params">组件附加参数，无则不用填写</param>
		''' <returns>图形与文本</returns>
		Public Shared Function MakeCaptcha(Optional name As String = "", Optional params As IDictionary(Of String, Object) = Nothing) As (Image As MemoryStream, Code As String)
			Return Instance.Value.Item(name)?.MakeCaptcha(params)
		End Function

		''' <summary>生成 session 名称</summary>
		Private Shared ReadOnly Property MakeKey(name As String) As (Name As String, Count As String)
			Get
				Dim key = $"RndCode_{name.GetHashCode}"
				Return (key, $"{key}_count")
			End Get
		End Property

		''' <summary>生成验证码图形与验证内容</summary>
		''' <param name="name">验证码规则标识，不设置则使用默认规则</param>
		Public Shared Function MakeCaptcha(http As HttpContext, Optional name As String = "", Optional params As IDictionary(Of String, Object) = Nothing) As MemoryStream
			If http Is Nothing Then Return Nothing

			Dim imgCode = MakeCaptcha(name, params)
			If imgCode.Image Is Nothing Then Return Nothing

			Dim key = MakeKey(name)
			http.Session.SetString(key.Name, imgCode.Code.MD5)
			http.Session.SetInt32(key.Count, 0)

			Return imgCode.Image
		End Function

		''' <summary>验证验证码</summary>
		Public Shared Function ValidateCaptcha(code As String, Optional name As String = "") As Boolean
			If code.IsEmpty Then Return False
			Return Instance.Value.Item(name)?.ValidateCaptcha(code)
		End Function

		''' <summary>使用 http session 验证验证码；次数超过 3 次，验证码将失效</summary>
		''' <returns>返回错误次数,大于 0 表示验证错误</returns>
		Public Shared Function ValidateCaptcha(http As HttpContext, code As String, Optional name As String = "") As Integer
			If http Is Nothing OrElse code.IsEmpty Then Return False

			Dim captcha = Instance.Value.Item(name)
			If captcha Is Nothing Then Return False

			Dim key = MakeKey(name)

			' 记录读取一次性，下次再读取需要重新刷新创建验证码
			Dim enCode = http.Session.GetString(key.Name)
			Dim succ = enCode.NotEmpty AndAlso code.NotEmpty AndAlso code.MD5 = enCode

			' 失败记录错误次数
			Dim count = http.Session.GetInt32(key.Count)
			If count Is Nothing OrElse count < 0 Then count = 0

			If succ Then
				' 验证成功
				http.Session.Remove(key.Name)
				http.Session.Remove(key.Count)
			Else
				' 记录错误
				count += 1
				http.Session.SetInt32(key.Count, count)

				' 错误3次清除原始验证码数据
				If count > 3 Then http.Session.Remove(key.Name)
			End If

			Return count
		End Function
	End Class

End Namespace