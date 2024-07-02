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
' 	核心操作公共参数
'
' 	name: Program
' 	create: 2020-11-16
' 	memo: 核心操作公共参数
'
' ------------------------------------------------------------

Imports System.Net
Imports System.Text

''' <summary>核心操作公共参数</summary>
Public Module Main

	''' <summary>国家法定休息日列表</summary>
	Public DATE_HOLIDAY As Date()

	''' <summary>国家法定调休日期，原本是周末休息日，但是因为放假需要调休的日期</summary>
	Public DATE_ADJUST As Date()

	''' <summary>当前系统时间与实际时间的时差（单位：秒）</summary>
	Public TIME_DELAY As Integer = 0

	''' <summary>当前时间</summary>
	Public ReadOnly Property SYS_NOW As DateTimeOffset
		Get
			Return If(TIME_DELAY = 0, DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(TIME_DELAY))
		End Get
	End Property

	''' <summary>当前时间</summary>
	Public ReadOnly Property SYS_NOW_STR(Optional format As String = "yyyy-MM-dd HH:mm:ss") As String
		Get
			Return SYS_NOW.ToString(format)
		End Get
	End Property

	''' <summary>当前时间</summary>
	Public ReadOnly Property SYS_NOW_DATE As Date
		Get
			Return SYS_NOW.DateTime
		End Get
	End Property

	''' <summary>代理服务器，用于 Net.HttpClient 或者  Net.WebClient</summary>
	Public SYS_PROXY As WebProxy

	''' <summary>系统启动时间</summary>
	Public ReadOnly SYS_START As DateTimeOffset = SYS_NOW

	''' <summary>系统初始路径</summary>
	Public ReadOnly SYS_ROOT As String = PathHelper.Root

#Region "注册字符编码，否则无法使用 Text.Encoding"

	''' <summary>编码是否注册</summary>
	Private _EncodingRegister As Boolean = False

	''' <summary>注册编码，防止中文 GB2312 无法使用</summary>
	Public Sub EncodingRegister()
		If _EncodingRegister Then Exit Sub
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
		_EncodingRegister = True
	End Sub

	''' <summary>GB2312(GBK)</summary>
	Private _GB2312 As Encoding = Nothing

	''' <summary>GB2312(GBK)</summary>
	Public ReadOnly Property GB2312 As Encoding
		Get
			If _GB2312 Is Nothing Then
				EncodingRegister()
				_GB2312 = Encoding.GetEncoding(936)
			End If

			Return _GB2312
		End Get
	End Property

	''' <summary>UTF8</summary>
	Public ReadOnly Property UTF8 As Encoding
		Get
			Return Encoding.UTF8
		End Get
	End Property

#End Region

End Module
