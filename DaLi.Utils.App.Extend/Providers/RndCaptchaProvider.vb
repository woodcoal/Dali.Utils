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
' 	随机文本图片验证码
'
' 	name: Provider.RndCaptchaProvider
' 	create: 2023-02-16
' 	memo: 随机产生指定长度文本生成变形字母图片。为防止歧义，已经过滤 IO 等字母
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.IO
Imports SkiaSharp

Namespace Provider

	''' <summary>随机文本图片验证码</summary>
	Public Class RndCaptchaProvider
		Implements ICaptcha

		''' <summary>接口唯一标识，当使用多种验证码方式时以便区分</summary>
		Public ReadOnly Property Name As String Implements ICaptcha.Name
			Get
				Return "rnd"
			End Get
		End Property

		''' <summary>生成验证码图形与验证码文本</summary>
		Public Function MakeCaptcha(Optional params As IDictionary(Of String, Object) = Nothing) As (Image As MemoryStream, Code As String) Implements ICaptcha.MakeCaptcha
			Dim Len = 4

			If params.NotEmpty Then Len = ChangeType(Of Integer)(If(params("len"), params("Len")))

			Dim Code = MakeCode(Len)
			Dim Image = MakeCodeImage(Code)
			Return (Image, Code)
		End Function

		''' <summary>验证验证码</summary>
		Public Function ValidateCaptcha(code As String) As Boolean Implements ICaptcha.ValidateCaptcha
			If code.IsEmpty Then Return False
			If Not Cache.ContainsKey(code) Then Return False

			' 获取缓存时间
			Dim last = Cache(code)

			' 移除缓存
			CacheRemove(code)

			' 未超时表示成功
			Return last > GetTicks(Date.Now)
		End Function

#Region "构造及初始化"

		''' <summary>随机字符</summary>
		Private Const LETTERS = "0BC9DMN3VW7JKL615PQX2YZAEF8GHR4STU"

		''' <summary>随机对象</summary>
		Private ReadOnly _Rnd As New Random

		''' <summary>缓存超时时间（秒）</summary>
		Private _Timeout As Integer = 180

		''' <summary>缓存超时时间（30-600秒）</summary>
		Public WriteOnly Property Timeout As Integer
			Set(value As Integer)
				_Timeout = value.Range(30, 600)
			End Set
		End Property

#End Region

#Region "缓存"

		''' <summary>缓存，验证码与到期时间</summary>
		Protected ReadOnly Cache As New ConcurrentDictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

		''' <summary>下次检查时间</summary>
		Private _NextTime As Integer = 0

		''' <summary>最早时间戳，用于计算当前时间与之的差值</summary>
		Private ReadOnly _TicksStrart As Long = New Date(2023, 1, 1).Ticks

		''' <summary>指定时间的时间戳，以 2023 年为起点计算</summary>
		Private Function GetTicks(time As Date) As Integer
			Return (time.ToUniversalTime.Ticks - _TicksStrart) / 10000000
		End Function

		''' <summary>移除到期缓存</summary>
		Private Sub CacheClear()
			Dim Now = GetTicks(Date.Now)
			If _NextTime > Now Then Return

			Cache.Keys.ToList.ForEach(Sub(key) If Now > Cache(key) Then Cache.TryRemove(key, 0))

			_NextTime = Now + _Timeout
		End Sub

		''' <summary>缓存验证码</summary>
		Private Sub CacheSet(code As String)
			Call CacheClear()
			Cache.TryAdd(code, GetTicks(Date.Now) + _Timeout)
		End Sub

		''' <summary>移除缓存验证码</summary>
		Private Sub CacheRemove(code As String)
			Cache.TryRemove(code, 0)
		End Sub

#End Region

#Region "验证码"

		''' <summary>生成验证码字符串</summary>
		''' <param name="len">字母长度 3 - 8 个字符</param>
		Public Function MakeCode(Optional len As Integer = 4) As String
			len = len.Range(3, 8)

			Dim chars = New List(Of Char)
			Dim lettersLen = LETTERS.Length

			For I = 1 To len
				Dim idx = _Rnd.Next(0, lettersLen)
				chars.Add(LETTERS(idx))
			Next

			Dim Code = New String(chars.ToArray)

			' 缓存
			CacheSet(Code)

			Return Code
		End Function

		'''' <summary>生成验证码图片</summary>
		'Public Function MakeCodeImage(code As String) As MemoryStream
		'	If code.IsEmpty Then Return Nothing

		'	Dim Stream = New MemoryStream()
		'	Dim codeLen = code.Length

		'	'-----------
		'	' 绘图
		'	'-----------
		'	Dim Rnd As New Random

		'	Dim rndColor = Function()
		'					   Dim Cr = Rnd.Next(200)
		'					   Dim Cg = Rnd.Next(250 - Cr)
		'					   Dim Cb = Rnd.Next(255 - Cr - Cg)

		'					   Return Color.FromArgb(Cr, Cg, Cb)
		'				   End Function

		'	Using img As New Bitmap(17 * codeLen, 30)
		'		Using g = Graphics.FromImage(img)
		'			g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
		'			g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

		'			g.Clear(Color.White)

		'			Dim left = 0
		'			Dim top = 0

		'			' 随机背景
		'			For I = 1 To Rnd.Next(50, 80)
		'				left = Rnd.Next(img.Width)
		'				top = Rnd.Next(img.Height)
		'				g.DrawRectangle(New Pen(Color.FromArgb(Rnd.Next(150, 255), Rnd.Next(150, 255), Rnd.Next(150, 255)), 0), left, top, 1, 1)
		'			Next

		'			' 画验证码
		'			left = 0
		'			For I = 0 To codeLen - 1
		'				Dim s = code.Substring(I, 1)

		'				Dim fontSize = Rnd.Next(12, 22)
		'				Dim fontColor = rndColor.Invoke
		'				Dim fontStyle = If(Rnd.Next(-1, 2) = 0, Drawing.FontStyle.Italic, Drawing.FontStyle.Regular) Or If(Rnd.Next(-1, 2) = 0, Drawing.FontStyle.Bold, Drawing.FontStyle.Strikeout)

		'				top = Rnd.Next(-1, Math.Max(1, 24 - fontSize))

		'				g.DrawString(s, New Font("", fontSize, fontStyle), New SolidBrush(fontColor), left, top)

		'				left += Rnd.Next(14, 18)
		'			Next

		'			' 画线条
		'			For I = 1 To Rnd.Next(5, 8)
		'				left = Rnd.Next(img.Width)
		'				top = Rnd.Next(img.Height)
		'				Dim x = Rnd.Next(img.Width)
		'				Dim y = Rnd.Next(img.Width)
		'				g.DrawLine(New Pen(rndColor.Invoke, 1), left, top, x, y)
		'			Next

		'			img.Save(Stream, Imaging.ImageFormat.Jpeg)
		'		End Using
		'	End Using

		'	Return Stream
		'End Function

		''' <summary>生成验证码图片</summary>
		Public Function MakeCodeImage(code As String) As MemoryStream
			If code.IsEmpty Then Return Nothing

			Dim stream = New MemoryStream()
			Dim codeLen = code.Length

			'-----------
			' 绘图
			'-----------
			Dim Rnd As New Random

			Dim rndColor = Function()
							   Dim Cr = Rnd.Next(200)
							   Dim Cg = Rnd.Next(250 - Cr)
							   Dim Cb = Rnd.Next(Math.Abs(255 - Cr - Cg))
							   Dim Ca = Rnd.Next(150, 250)

							   Return New SKColor(Cr, Cg, Cb, Ca)
						   End Function

			Using bitmap As New SKBitmap(17 * codeLen, 30, SKColorType.Rgba8888)
				Using canvas = New SKCanvas(bitmap)
					canvas.Clear(SKColors.White)

					Dim paint As New SKPaint With {
						.IsAntialias = True,
						.IsStroke = False
					}

					Dim left = 0
					Dim top = 0

					' 随机背景
					For I = 1 To Rnd.Next(50, 80)
						left = Rnd.Next(bitmap.Width)
						top = Rnd.Next(bitmap.Height)
						Dim w = Rnd.Next(1, 3)
						Dim h = Rnd.Next(1, 3)
						paint.Color = rndColor()
						canvas.DrawRect(left, top, w, h, paint)
					Next

					' 画验证码
					left = 0
					For I = 0 To codeLen - 1
						Dim s = code.Substring(I, 1)

						Dim fontSize = Rnd.Next(16, 28)
						Dim fontColor = rndColor()
						'Dim fontStyle = If(Rnd.Next(-1, 2) = 0, Drawing.FontStyle.Italic, Drawing.FontStyle.Regular) Or If(Rnd.Next(-1, 2) = 0, Drawing.FontStyle.Bold, Drawing.FontStyle.Strikeout)

						top = Rnd.Next(1, Math.Max(1, 12))

						Using style = New SKPaint
							style.IsAntialias = True ' 开启抗锯齿
							' style.Typeface = SkiaSharp.SKTypeface.FromFamilyName("微软雅黑", SKTypefaceStyle.Bold);//字体
							style.Color = fontColor
							style.TextSize = fontSize

							Dim fontStyle = SKFontStyle.Normal
							Select Case Rnd.Next(1, 5)
								Case 2
									fontStyle = SKFontStyle.Bold
								Case 3
									fontStyle = SKFontStyle.Italic
								Case 4
									fontStyle = SKFontStyle.BoldItalic
							End Select

							style.Typeface = SKTypeface.FromFamilyName(SKTypeface.Default.FamilyName, fontStyle)

							Dim size = New SKRect()
							style.MeasureText(s, size) ' 计算文字宽度以及高度
							top -= size.Top

							canvas.DrawText(s, left, top, style)
						End Using

						'	
						'
						'               float temp = (128 - size.Size.Width) / 2;
						'float temp1 = (128 - size.Size.Height) / 2;
						'canvas.DrawText(Str, temp, temp1 - size.Top, style);//画文字

						left += Rnd.Next(14, 18)

						'				var paint = New SKPaint()
						'  {
						'      StrokeWidth = 2, //画笔宽度
						'      Typeface = SKTypeface.FromFamilyName("宋体", SKFontStyle.Normal), //字体
						'      TextSize = 32,  //字体大小
						'      Style = SKPaintStyle.Stroke, //类型：填充 或 画边界 或全部
						'PathEffect = SKPathEffect.CreateDash(LongDash, 0),   //绘制虚线
						'  };

						'	

						'	Dim fontSize = random.Next(12, 22)
						'	Dim fontColor = rndColor.Invoke()
						'	Dim fontStyle = If(random.Next(-1, 2) = 0, SKFontStyleSlant.Italic, SKFontStyleSlant.Upright)

						'	top = random.Next(-1, Math.Max(1, 24 - fontSize))

						'	Using font As New SKFont(SKTypeface.Default, fontSize)
						'		paint.Color = fontColor
						'		paint.Typeface = font.Typeface
						'		paint.TextSize = font.Size
						'		paint.TextAlign = SKTextAlign.Left
						'		paint.FontMetrics = font.GetFontMetrics() ' 获取字体的基本信息，包括字体的宽度，高度，基准线，上边距，下边距，行距，字符间距

						'		canvas.DrawText(s, left, top + FontMetrics.Ascent, paint)
						'	End Using

						'	
					Next

					' 画线条
					For i = 1 To Rnd.Next(5, 8)
						left = Rnd.Next(bitmap.Width)
						top = Rnd.Next(bitmap.Height)
						Dim x = Rnd.Next(bitmap.Width)
						Dim y = Rnd.Next(bitmap.Width)
						Dim color = rndColor.Invoke()
						paint.Color = color
						canvas.DrawLine(left, top, x, y, paint)
					Next

					Using image = SKImage.FromBitmap(bitmap)
						Using data = image.Encode(SKEncodedImageFormat.Jpeg, 100)
							data.SaveTo(stream)
						End Using
					End Using
				End Using
			End Using

			Return stream
		End Function

#End Region

	End Class


End Namespace