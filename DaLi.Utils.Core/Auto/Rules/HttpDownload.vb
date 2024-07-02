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
' 	网络下载
'
' 	name: Auto.Rule.HttpDownload
' 	create: 2023-01-23
' 	memo: 网络下载
' 	
' ------------------------------------------------------------

Imports System.Net
Imports DaLi.Utils.Http

Namespace Auto.Rule

	''' <summary>网络下载</summary>
	Public Class HttpDownload
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "网络下载"
			End Get
		End Property

		''' <summary>网址</summary>
		Public Property URL As String

		''' <summary>自动跳转</summary>
		Public Property AutoRedirect As Boolean

		''' <summary>超时，最少 5 秒</summary>
		Public Property Timeout As Integer = 5

		''' <summary>是否提交 Json 数据</summary>
		Public Property Json As Boolean

		''' <summary>请求方式</summary>
		Public Property Method As String

		''' <summary>来源地址</summary>
		Public Property Referer As String

		''' <summary>浏览器头</summary>
		Public Property UserAgent As String

		''' <summary>Headers</summary>
		Public Property Headers As NameValueDictionary

		''' <summary>提交 Cookiese 信息</summary>
		Public Property Cookies As NameValueDictionary

		''' <summary>Post 提交数据</summary>
		Public Property SendDatas As KeyValueDictionary

		''' <summary>编码</summary>
		Public Property Encoding As EncodingEnum

		''' <summary>Unicode是否需要反编码？用于 json 中汉字进行 Unicode 编码后，进行反转码</summary>
		Public Property UnicodeDecode As Boolean

		''' <summary>下载目录</summary>
		Public Property Folder As String

		''' <summary>文件名</summary>
		Public Property FileName As String

		''' <summary>文件存在处理</summary>
		Public Property Exists As ExistsActionEnum

		''' <summary>初始化</summary>
		Public Sub New()
			Encoding = EncodingEnum.AUTO
			UnicodeDecode = False
			Exists = ExistsActionEnum.RENAME
		End Sub

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "网址未设置"
			If URL.IsEmpty Then Return False

			message = "下载目录未设置"
			If Folder.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

		''' <summary>克隆</summary>
		Public Overrides Function Clone() As Object
			Dim R As HttpDownload = MemberwiseClone()
			R.Headers = Headers?.Clone
			R.Cookies = Cookies?.Clone
			R.SendDatas = SendDatas?.Clone
			Return R
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage) As Object
			Dim rule As HttpDownload = Clone()

			' 注册编码
			Call EncodingRegister()

			' 替换时间标签
			If data.NotEmpty Then
				' 调整网址 %5BYYYY%5D 
				'rule.URL = rule.URL.Replace("%7B", "{", StringComparison.OrdinalIgnoreCase)
				'rule.URL = rule.URL.Replace("%7D", "}", StringComparison.OrdinalIgnoreCase)
				'rule.URL = rule.URL.Replace("%28", "(", StringComparison.OrdinalIgnoreCase)
				'rule.URL = rule.URL.Replace("%29", ")", StringComparison.OrdinalIgnoreCase)
				rule.URL = AutoHelper.GetVarString(rule.URL, data)

				' 其他参数
				rule.UserAgent = AutoHelper.GetVarString(rule.UserAgent, data)
				rule.Referer = AutoHelper.GetVarString(rule.Referer, data)
				rule.Headers = rule.Headers?.FormatAction(Function(x) AutoHelper.GetVarString(x, data))
				rule.Cookies = rule.Cookies?.FormatAction(Function(x) AutoHelper.GetVarString(x, data))
				rule.SendDatas = rule.SendDatas?.FormatAction(Function(x) If(x IsNot Nothing AndAlso x.GetType.IsString, AutoHelper.GetVarString(x, data), x))

				' 路径
				rule.Folder = AutoHelper.GetVarString(rule.Folder, data)
				rule.FileName = AutoHelper.GetVarString(rule.FileName, data)
			End If

			message.Message = "无效网址：" + rule.URL
			If Not rule.URL.IsUrl Then Return Nothing

			message.Message = "无效附件保存目录：" + rule.Folder
			If Not PathHelper.FolderExist(rule.Folder, True) Then Return Nothing

			' 创建请求对象
			Dim http As New HttpClient

			' Cookies
			If rule.Cookies.NotEmpty Then rule.Cookies.ForEach(Sub(k, v) http.SetCookie(k, v))

			' URL 
			http.Url = rule.URL

			' 请求头部信息 
			If rule.Headers.NotEmpty Then rule.Headers.ForEach(Sub(k, v) http.SetHeader(k, v))

			' 提交数据
			If rule.SendDatas.NotEmpty Then
				Try
					' JSON 提交
					If rule.Json Then
						http.PostType = Utils.Http.Model.HttpPostEnum.JSON
						http.SetRawContent(rule.SendDatas.ToJson)
					Else
						For Each item In rule.SendDatas
							http.SetPostContent(item.Key, item.Value?.ToString)
						Next
					End If
				Catch ex As System.Exception
				End Try
			End If

			' 验证网址时，不能自动跳转
			http.AllowAutoRedirect = rule.AutoRedirect

			' UserAgent / Referer
			If rule.UserAgent.NotEmpty Then http.UserAgent = rule.UserAgent
			If rule.Referer.NotEmpty Then http.UserAgent = rule.Referer

			' 执行操作
			http.Execute()

			' 输出 
			Dim ret As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)

			' 保存
			Dim save = http.SaveFile(rule.Folder, rule.Exists, rule.FileName)
			ret.Add("Path", save.Path)
			ret.Add("Flag", save.Flag)

			' 状态码
			ret.Add("StatusCode", http.StatusCode)

			' Header
			http.ResponseHeaders?.ForEach(Sub(k, v) ret.Add($"Header_{k}", v))

			' Cookies
			Dim cookies = http.GetCookies
			If cookies.NotEmpty Then
				For Each cookie As Cookie In cookies
					ret.Add($"Cookie_{cookie.Name}", cookie.Value)
				Next
			End If

			' 网址
			ret.Add("Url", http.Url)

			message.SetSuccess()
			Return ret
		End Function

#End Region

	End Class
End Namespace
