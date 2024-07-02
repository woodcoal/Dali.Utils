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
' 	HTTP服务相关操作
'
' 	name: Helper.NetHelper
' 	create: 2019-03-22
' 	memo: HTTP服务相关操作
' 	
' ------------------------------------------------------------

Imports System.Data
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Text.RegularExpressions
Imports DaLi.Utils.Http
Imports DaLi.Utils.Http.Model

Namespace Helper
	''' <summary>HTTP服务相关操作</summary>
	Public NotInheritable Class NetHelper

#Region "Mime & UserAgent"

		Private Shared ReadOnly _Mimes As New Generic.Dictionary(Of String, String) From {{".323", "text/h323"}, {".aaf", "application/octet-stream"}, {".aca", "application/octet-stream"}, {".accdb", "application/msaccess"}, {".accdt", "application/msaccess"}, {".acx", "application/internet-property-stream"}, {".afm", "application/octet-stream"}, {".ai", "application/postscript"}, {".aifc", "audio/aiff"}, {".aiff", "audio/aiff"}, {".asf", "video/x-ms-asf"}, {".asi", "application/octet-stream"}, {".asm", "text/plain"}, {".asr", "video/x-ms-asf"}, {".asx", "video/x-ms-asf"}, {".atom", "application/atom+xml"}, {".au", "audio/basic"}, {".avi", "video/x-msvideo"}, {".bcpio", "application/x-bcpio"}, {".bin", "application/octet-stream"}, {".bmp", "image/bmp"}, {".cab", "application/octet-stream"}, {".cat", "application/vnd.ms-pki.seccat"}, {".cdf", "application/x-cdf"}, {".chm", "application/octet-stream "}, {".class", "application/x-java-applet"}, {".clp", "application/x-msclip"}, {".cmx", "image/x-cmx"}, {".cnf", "text/plain"}, {".cod", "image/cis-cod"}, {".cpio", "application/x-cpio"}, {".crd", "application/x-mscardfile"}, {".crl", "application/pkix-crl"}, {".crt", "application/x-x509-ca-cert"}, {".csh", "application/x-csh"}, {".csv", "application/octet-stream"}, {".cur", "application/octet-stream"}, {".der", "application/x-x509-ca-cert.pfx"}, {".dib", "image/bmp"}, {".dir", "application/x-director"}, {".disco", "text/xml"}, {".dll", "application/x-msdownload"}, {".dlm", "text/dlm"}, {".doc", "application/msword"}, {".docm", "application/vnd.ms-word.document.macroEnabled.12"}, {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"}, {".dot", "application/msword"}, {".dotm", "application/vnd.ms-word.template.macroEnabled.12"}, {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"}, {".dsp", "application/octet-stream"}, {".dtd", "text/xml"}, {".dvi", "application/x-dvi"}, {".dwf", "drawing/x-dwf"}, {".dwp", "application/octet-stream"}, {".dxr", "application/x-director"}, {".emz", "application/octet-stream"}, {".eot", "application/octet-stream"}, {".eps", "application/postscript"}, {".etx", "text/x-setext"}, {".evy", "application/envoy"}, {".fdf", "application/vnd.fdf"}, {".fla", "application/octet-stream"}, {".flr", "x-world/x-vrml"}, {".FLV", "flv-application/octet-stream"}, {".gif", "image/gif"}, {".gz", "application/x-gzip"}, {".h", "text/plain"}, {".hdf", "application/x-hdf"}, {".hdml", "text/x-hdml"}, {".hhk", "application/octet-stream"}, {".hhp", "application/octet-stream"}, {".hlp", "application/winhlp"}, {".hqx", "application/mac-binhex40"}, {".hta", "application/hta"}, {".htc", "text/x-component"}, {".htm", "text/html"}, {".html", "text/html"}, {".htt", "text/webviewhtml"}, {".ico", "image/x-icon"}, {".ics", "application/octet-stream"}, {".ief", "image/ief"}, {".iii", "application/x-iphone"}, {".inf", "application/octet-stream"}, {".ins", "application/x-internet-signup"}, {".isp", "application/x-internet-signup"}, {".IVF", "video/x-ivf"}, {".jar", "application/java-archive"}, {".java", "application/octet-stream"}, {".jck", "application/liquidmotion"}, {".jcz", "application/liquidmotion"}, {".jfif", "image/pjpeg"}, {".jpb", "application/octet-stream"}, {".jpg", "image/jpeg"}, {".jpeg", "image/jpeg"}, {".jpe", "image/jpeg"}, {".js", "application/x-javascript"}, {".latex", "application/x-latex"}, {".lit", "application/x-ms-reader"}, {".lpk", "application/octet-stream"}, {".lsf", "video/x-la-asf"}, {".lsx", "video/x-la-asf"}, {".lzh", "application/octet-stream"}, {".m13", "application/x-msmediaview"}, {".m14", "application/x-msmediaview"}, {".m1v", "video/mpeg"}, {".m3u", "audio/x-mpegurl"}, {".man", "application/x-troff-man"}, {".map", "text/plain"}, {".mdb", "application/x-msaccess"}, {".mdp", "application/octet-stream"}, {".me", "application/x-troff-me"}, {".mht", "message/rfc822"}, {".mhtml", "message/rfc822"}, {".mid", "audio/mid"}, {".midi", "audio/mid"}, {".mix", "application/octet-stream"}, {".mmf", "application/x-smaf"}, {".mno", "text/xml"}, {".mny", "application/x-msmoney"}, {".movie", "video/x-sgi-movie"}, {".mp2", "video/mpeg"}, {".mpa", "video/mpeg"}, {".mpe", "video/mpeg"}, {".mpeg", "video/mpeg"}, {".mpp", "application/vnd.ms-project"}, {".mpv2", "video/mpeg"}, {".ms", "application/x-troff-ms"}, {".mso", "application/octet-stream"}, {".mvb", "application/x-msmediaview"}, {".mvc", "application/x-miva-compiled"}, {".nc", "application/x-netcdf"}, {".nsc", "video/x-ms-asf"}, {".nws", "message/rfc822"}, {".oda", "application/oda"}, {".ods", "application/oleobject"}, {".one", "application/onenote"}, {".onea", "application/onenote"}, {".onepkg", "application/onenote"}, {".onetmp", "application/onenote"}, {".onetoc", "application/onenote"}, {".p10", "application/pkcs10"}, {".p12", "application/x-pkcs12"}, {".p7b", "application/x-pkcs7-certificates"}, {".p7c", "application/pkcs7-mime"}, {".p7r", "application/x-pkcs7-certreqresp"}, {".p7s", "application/pkcs7-signature"}, {".pbm", "image/x-portable-bitmap "}, {".pcz", "application/octet-stream"}, {".pdf", "application/pdf"}, {".pfb", "application/octet-stream"}, {".pfm", "application/octet-stream"}, {".pgm", "image/x-portable-graymap"}, {".pma", "application/x-perfmon"}, {".pmc", "application/x-perfmon"}, {".pml", "application/x-perfmon"}, {".pmr", "application/x-perfmon"}, {".pmw", "application/x-perfmon"}, {".png", "image/png"}, {".pnm", "image/x-portable-anymap"}, {".pnz", "image/png"}, {".pot", "application/vnd.ms-powerpoint"}, {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"}, {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"}, {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"}, {".ppm", "image/x-portable-pixmap"}, {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"}, {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"}, {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12 "}, {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"}, {".prf", "application/pics-rules"}, {".prm", "application/octet-stream"}, {".prx", "application/octet-stream"}, {".ps", "application/postscript"}, {".psd", "application/octet-stream"}, {".psm", "application/octet-stream"}, {".psp", "application/octet-stream"}, {".pub", "application/x-mspublisher"}, {".qt", "video/quicktime"}, {".qtl", "application/x-quicktimeplayer"}, {".qxd", "application/octet-stream"}, {".ra", "audio/x-pn-realaudio"}, {".ram", "audio/x-pn-realaudio"}, {".rar", "application/octet-stream"}, {".ras", "image/x-cmu-raster"}, {".rf", "image/vnd.rn-realflash"}, {".rgb", "image/x-rgb"}, {".rm", "application/vnd.rn-realmedia"}, {".rmi", "audio/mid"}, {".rpm", "audio/x-pn-realaudio-plugin.aif"}, {".rtf", "application/rtf"}, {".rtx", "text/richtext"}, {".sct", "text/scriptlet"}, {".sea", "application/octet-stream"}, {".setpay", "application/set-payment-initiation"}, {".setreg", "application/set-registration-initiation"}, {".sgml", "text/sgml"}, {".sh", "application/x-sh"}, {".shar", "application/x-shar"}, {".sit", "application/x-stuffit"}, {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"}, {".smd", "audio/x-smd"}, {".smi", "application/octet-stream"}, {".smx", "audio/x-smd"}, {".smz", "audio/x-smd"}, {".snd", "audio/basic"}, {".snp", "application/octet-stream"}, {".spc", "application/x-pkcs7-certificates"}, {".spl", "application/futuresplash"}, {".src", "application/x-wais-source"}, {".ssm", "application/streamingmedia .axs"}, {".stl", "application/vnd.ms-pki.stl"}, {".sv4cpio", "application/x-sv4cpio"}, {".sv4crc", "application/x-sv4crc"}, {".swf", "application/x-shockwave-flash "}, {".tcl", "application/x-tcl"}, {".tex", "application/x-tex"}, {".texi", "application/x-texinfo"}, {".texinfo", "application/x-texinfo"}, {".tgz", "application/x-compressed"}, {".thmx", "application/vnd.ms-officetheme"}, {".thn", "application/octet-stream"}, {".tif", "image/tiff"}, {".tiff", "image/tiff"}, {".toc", "application/octet-stream"}, {".tr", "application/x-troff"}, {".trm", "application/x-msterminal"}, {".tsv", "text/tab-separated-values"}, {".ttf", "application/octet-stream"}, {".txt", "text/plain"}, {".uls", "text/iuls"}, {".ustar", "application/x-ustar"}, {".vbs", "text/vbscript"}, {".vcf", "text/x-vcard"}, {".vdx", "application/vnd.ms-visio.viewer"}, {".vsd", "application/vnd.visio"}, {".vss", "application/vnd.visio"}, {".vst", "application/vnd.visio"}, {".vsw", "application/vnd.visio"}, {".vsx", "application/vnd.visio"}, {".vtx", "application/vnd.visio"}, {".wav", "audio/wav"}, {".wax", "audio/x-ms-wax"}, {".wbmp", "image/vnd.wap.wbmp"}, {".wcm", "application/vnd.ms-works"}, {".wdb", "application/vnd.ms-works"}, {".wks", "application/vnd.ms-works"}, {".wm", "video/x-ms-wm"}, {".wma", "audio/x-ms-wma"}, {".wmd", "application/x-ms-wmd"}, {".wmf", "application/x-msmetafile"}, {".wml", "text/vnd.wap.wml"}, {".wmlc", "application/vnd.wap.wmlc"}, {".wmlsc", "application/vnd.wap.wmlscriptc"}, {".wmp", "video/x-ms-wmp "}, {".wmv", "video/x-ms-wmv"}, {".wmx", "video/x-ms-wmx"}, {".wps", "application/vnd.ms-works"}, {".wri", "application/x-mswrite"}, {".wrl", "x-world/x-vrml"}, {".wrz", "x-world/x-vrml"}, {".wsdl", "text/xml"}, {".wvx", "video/x-ms-wvx"}, {".x", "application/directx"}, {".xaf", "x-world/x-vrml"}, {".xbm", "image/x-xbitmap"}, {".xla", "application/vnd.ms-excel"}, {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"}, {".xlc", "application/vnd.ms-excel"}, {".xlm", "application/vnd.ms-excel"}, {".xls", "application/vnd.ms-excel"}, {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"}, {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"}, {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}, {".xlt", "application/vnd.ms-excel"}, {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"}, {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"}, {".xlw", "application/vnd.ms-excel"}, {".xml", "text/xml"}, {".xof", "x-world/x-vrml"}, {".xpm", "image/x-xpixmap"}, {".xsd", "text/xml"}, {".xsf", "text/xml"}, {".xsl", "text/xml"}, {".xslt", "text/xml"}, {".xwd", "image/x-xwindowdump "}, {".z", "application/x-compress"}, {".zip", "application/x-zip-compressed"}}

		''' <summary>通过服务器 Mime 获取文件类型</summary>
		Public Shared ReadOnly Property Mime2Ext(mime As String) As String
			Get
				If mime.NotEmpty Then
					mime = mime.Split(";"c)(0).Trim.ToLower

					If mime.NotEmpty Then
						Return String.Join(",", _Mimes.Where(Function(x) x.Value = mime).Select(Function(x) x.Key))
					End If
				End If

				Return "*"
			End Get
		End Property

		''' <summary>通过文件类型 获取 服务器 Mime </summary>
		Public Shared ReadOnly Property Ext2Mime(ext As String) As String
			Get
				Dim Ret = ""

				If ext.NotEmpty Then
					If Not ext.StartsWith(".") Then ext = "." & ext
					ext = ext.ToLower
					Ret = _Mimes.Where(Function(x) x.Key = ext).Select(Function(x) x.Value).FirstOrDefault
				End If

				Return Ret.EmptyValue("application/octet-stream")
			End Get
		End Property

		''' <summary>常用 UserAgents</summary>
		Public Shared ReadOnly Property UserAgents As String()
			Get
				Return New String() {' ⬇ IE ⬇
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)",
					"Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",
					"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)",
					"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)",
					"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)",' ⬇ 其他浏览器 ⬇
					"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_6_8) AppleWebKit/537.13+ (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2",
					"Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.15 (KHTML, like Gecko) Chrome/24.0.1295.0 Safari/537.15",
					"Mozilla/5.0 (Windows NT 6.1; rv:15.0) Gecko/20120716 Firefox/15.0a2",' ⬇ 国内双核浏览器 ⬇
					"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; baidubrowser 1.x)",
					"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.3 (KHTML, like Gecko)  Chrome/6.0.472.33 Safari/534.3 SE 2.X MetaSr 1.0",
					"Mozilla/5.0 (Windows NT 6.1) AppleWebKit/534.36 (KHTML, like Gecko) Chrome/12.0.742.53 Safari/534.36 QQBrowser/6.5.9225.201",
					"Mozilla/5.0 (Windows; U; Windows NT 5.1; zh-CN) AppleWebKit/533.9 (KHTML, like Gecko) Maxthon/3.0 Safari/533.9",' ⬇ 移动终端 ⬇
					"Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25",
					"Mozilla/5.0 (iPhone; U; CPU iPhone OS 5_1 like Mac OS X; en-us) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B176 Safari/7534.48.3",
					"Mozilla/5.0 (SymbianOS/9.4; Series60/5.0 NokiaN97-1/20.0.019; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) BrowserNG/7.1.18124",
					"Mozilla/5.0 (Linux; U; Android 2.3.3; en-au; GT-I9100 Build/GINGERBREAD) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
					"Mozilla/5.0 (Linux; U; Android 4.0.3; de-ch; HTC Sensation Build/IML74K) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30",
					"Mozilla/5.0 (BlackBerry; U; BlackBerry 9800; en) AppleWebKit/534.1+ (KHTML, like Gecko) Version/6.0.0.337 Mobile Safari/534.1+",' ⬇ 蜘蛛 ⬇
					"Baiduspider+(+http://www.baidu.com/search/spider.htm)",
					"Baiduspider-image+(+http://www.baidu.com/search/spider.htm)",
					"Googlebot/2.1 (+http://www.google.com/bot.html)",
					"Googlebot/2.1 (+http://www.googlebot.com/bot.html)",
					"Googlebot-Image/1.0",
					"360spider(http://webscan.360.cn)",
					"Sogou web spider/4.0(+http://www.sogou.com/docs/help/webmasters.htm#07)"
				}
			End Get
		End Property

		''' <summary>是否移动浏览器</summary>
		Public Shared Function IsMobileBrowser(userAgent As String) As Boolean
			Dim mobileKeywords() As String = {"Mobile", "Android", "Silk/", "Kindle", "BlackBerry", "Opera Mini", "Opera Mobi", "iPhone", "MicroMessenger", "AlipayClient"}

			For Each keyword As String In mobileKeywords
				If userAgent.Contains(keyword, StringComparison.OrdinalIgnoreCase) Then
					Return True
				End If
			Next

			Return False
		End Function
#End Region

		''' <summary>将相对地址转换为实际的网址</summary>
		''' <param name="baseUrl">当前网址，如： http://www.***.com/***</param>
		''' <param name="relativePath">相对地址，如： **/***/**</param>
		Public Shared Function AbsoluteUrl(baseUrl As String, relativePath As String) As String
			Dim Value As String = ""

			If relativePath.IsUrl Then
				' 相对地址如果包含 :// 则直接返回相对地址
				Return relativePath

			ElseIf baseUrl.IsUrl Then
				' 当前网址有效网址，

				If relativePath.IsEmpty Then
					' 相对地址为，空则直接返回
					Return baseUrl

				Else
					' 计算组合地址
					Try
						'通过 Uri 本身转换
						Dim baseUri As New Uri(baseUrl)
						If relativePath.StartsWith("?"c) Then
							Value = String.Concat(baseUri.AbsoluteUri.AsSpan(0, baseUri.AbsoluteUri.IndexOf("/"c, 8)), baseUri.AbsolutePath, relativePath)
						Else
							Value = New Uri(baseUri, relativePath).AbsoluteUri
						End If

						baseUri = Nothing
					Catch ex As Exception
						Value = relativePath
					End Try
				End If
			End If

			Return Value
		End Function

		'-------------------------------------------------------------------------------

		''' <summary>启动防止转向模式来获取系统相关数据</summary>
		Public Shared Function TrueUrl(url As String, Optional referer As String = "", Optional cookies As String = "", Optional userAgent As String = "") As String
			Dim strUrl = ""

			If url.IsUrl Then
				Try
					Dim oRequest = TryCast(HttpWebRequest.Create(url), HttpWebRequest)
					If oRequest IsNot Nothing Then
						With oRequest
							.Accept = "*/*"
							.AllowAutoRedirect = False
							.Timeout = 60000
							.UserAgent = userAgent
							.Referer = referer
							.Headers.Add("Cookie", cookies)
							.AddRange(0, 0)
							.Method = "GET"
						End With

						Using oResponse = TryCast(oRequest.GetResponse(), HttpWebResponse)
							Select Case oResponse.StatusCode
								Case 301, 302
									strUrl = oResponse.GetResponseHeader("Location")
									If strUrl.NotEmpty Then strUrl = AbsoluteUrl(url, strUrl)
							End Select

							oResponse.Close()
						End Using

						oRequest = Nothing
					End If
				Catch ex As Exception
				End Try
			End If

			Return strUrl.EmptyValue(url)
		End Function

		''' <summary>循环获取最终地址</summary>
		''' <param name="maxDeep">检测深度</param>
		Public Shared Function TrueUrl(url As String, maxDeep As Integer) As String
			Dim Ret = ""

			While maxDeep > 0
				maxDeep -= 1

				Ret = TrueUrl(url)
				If Not Ret.IsUrl OrElse Ret.IsSame(url) Then
					Ret = url
					Exit While
				Else
					url = Ret
				End If
			End While

			Return Ret
		End Function


		'-------------------------------------------------------------------------------

		''' <summary>检测制定的地址是否有效</summary>
		Public Shared Function Exists(url As String, Optional referer As String = "", Optional cookies As String = "", Optional userAgent As String = "", Optional webProxy As System.Net.WebProxy = Nothing) As Boolean
			Dim Value = False

			If url.IsUrl Then
				Try
					Dim oRequest = TryCast(HttpWebRequest.Create(url), HttpWebRequest)
					If oRequest IsNot Nothing Then
						With oRequest
							.Accept = "*/*"
							.AllowAutoRedirect = False
							.Timeout = 60000
							.UserAgent = userAgent
							.Referer = referer
							.Headers.Add("Cookie", cookies)
							.AddRange(0, 0)
							.Method = "GET"
							.Proxy = webProxy
						End With

						Using oResponse = TryCast(oRequest.GetResponse(), HttpWebResponse)
							Select Case oResponse.StatusCode
								Case 200 To 299
									Value = True

								Case 301, 302
									Dim strUrl = oResponse.GetResponseHeader("Location")
									If strUrl.NotEmpty Then
										strUrl = AbsoluteUrl(url, strUrl)
										If Not strUrl.IsSame(url) Then Value = Exists(strUrl)
									End If
							End Select

							oResponse.Close()
						End Using

						oRequest = Nothing
					End If
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		''' <summary>获取文档长度</summary>
		Public Shared Function Length(url As String, Optional referer As String = "", Optional cookies As String = "", Optional userAgent As String = "", Optional webProxy As System.Net.WebProxy = Nothing) As Long
			Dim Value = 0

			If url.IsUrl Then
				Try
					Dim oRequest = TryCast(HttpWebRequest.Create(url), HttpWebRequest)
					If oRequest IsNot Nothing Then
						With oRequest
							.Accept = "*/*"
							.AllowAutoRedirect = True
							.Timeout = 60000
							.UserAgent = userAgent
							.Referer = referer
							.Headers.Add("Cookie", cookies)
							.Method = "GET"
							.Proxy = webProxy
						End With

						Using oResponse = TryCast(oRequest.GetResponse(), HttpWebResponse)
							Value = oResponse.ContentLength
							oResponse.Close()
						End Using

						oRequest = Nothing
					End If
				Catch ex As Exception
				End Try
			End If

			Return Value
		End Function

		'-------------------------------------------------------------------------------

		''' <summary>获取域名的IP</summary>
		Public Shared Function DomainIP(domain As String) As String
			If domain.IsEmpty Then Return ""

			Dim Value = ""

			Try
				Dim IPList = Dns.GetHostEntry(domain).AddressList
				If IPList?.Length > 0 Then
					For I As Integer = 0 To IPList.Length - 1
						If IPList(I).AddressFamily = AddressFamily.InterNetwork Then
							Value = IPList(I).ToString
							Exit For
						End If
					Next
				End If
			Catch ex As Exception
			End Try

			Return Value
		End Function

		''' <summary>获取服务器的IP</summary>
		Public Shared Function ServerIP() As String
			Return DomainIP(Dns.GetHostName())
		End Function

		'-------------------------------------------------------------------------------

		''' <summary>网络是否畅通</summary>
		Public Shared Function IsNetworkAvailable() As Boolean
			Return NetworkInterface.GetIsNetworkAvailable()
		End Function

		''' <summary>网络是否畅通，并返回响应时间，如果时间小于1则表示网络不通</summary>
		Public Shared Function Ping(Optional domain As String = "", Optional timeout As Integer = 0) As Long
			domain = domain.EmptyValue("baidu.com")
			If timeout < 1 Then timeout = 3000

			Dim P As New NetworkInformation.Ping
			Dim R = P.Send(domain, timeout)
			If R.Status = IPStatus.Success Then
				Return R.RoundtripTime
			Else
				Return -1
			End If
		End Function

		''' <summary>验证服务器IP</summary>
		''' <remarks>
		''' 0.0.0.0 / 255.255.255.255 / 127.0.0.1
		''' 组播地址: 224.0.0.0 - 239.255.255.255 
		''' DHCP 无效分配: 169.254.x.x
		''' 私有地址(企业内网): 10.x.x.x / 172.16.x.x - 172.31.x.x / 192.168.x.x
		''' </remarks>
		Public Shared Function ValidateServer(serverDomain As String) As Boolean
			Return IsPublicIPv4(DomainIP(serverDomain))
		End Function

		''' <summary>验证是否公用 IPv4</summary>
		''' <remarks>
		''' 0.0.0.0 / 255.255.255.255 / 127.0.0.1
		''' 组播地址: 224.0.0.0 - 239.255.255.255 
		''' DHCP 无效分配: 169.254.x.x
		''' 私有地址(企业内网): 10.x.x.x / 172.16.x.x - 172.31.x.x / 192.168.x.x
		''' </remarks>
		Public Shared Function IsPublicIPv4(ip As String) As Boolean
			' 检查是否有效 IP
			Dim addr As IPAddress = Nothing
			If Not IPAddress.TryParse(ip, addr) Then Return False

			' 确保这是IPv4地址
			If addr.AddressFamily <> AddressFamily.InterNetwork Then Return False

			ip = addr.ToString
			If ip = "0.0.0.0" Or ip = "255.255.255.255" Or ip = "127.0.0.1" Or ip.StartsWith("169.254.") Or ip.StartsWith("10.") Or ip.StartsWith("192.168.") Then Return False

			' 判读是否 172.16.x.x - 172.31.x.x
			Dim IPs = addr.GetAddressBytes
			If IPs(0) = 172 AndAlso (IPs(1) > 15 Or IPs(1) < 32) Then Return False

			Return True
		End Function

		''' <summary>验证是否公用 IPv6</summary>
		''' <remarks>这个函数并不是完备的，它只检查了几种最常见的非公用IPv6地址类型。IPv6地址管理较为复杂，可能还有其他的特殊情况。此外，IsIPv6SiteLocal属性已被弃用（因为唯一本地地址（fc00::/7）取代了站点本地地址（fec0::/10），但可以用来简单地判断地址是否为私有地址。</remarks>
		Public Shared Function IsPublicIPv6(ip As String) As Boolean
			' 检查是否有效 IP
			Dim addr As IPAddress = Nothing
			If Not IPAddress.TryParse(ip, addr) Then Return False

			' 确保这是IPv6地址
			If addr.AddressFamily <> AddressFamily.InterNetworkV6 Then Return False

			' 链路本地地址 (fe80::/10)
			If addr.IsIPv6LinkLocal Then Return False

			' 唯一本地地址 (fc00::/7)
			If addr.IsIPv6SiteLocal Then Return False

			' 多播地址 (ff00::/8)
			If addr.IsIPv6Multicast Then Return False

			' 其他保留地址, 如文档地址 (2001:db8::/32), Teredo隧道地址 (2001::/32), 6to4隧道地址 (2002::/16)
			' 可根据需要扩展这个列表

			' 如果不是上述类型的地址，则认为它是公用IPv6地址
			Return True
		End Function

		''' <summary>分析编码类型</summary>
		''' <param name="content">内容</param>
		''' <param name="contentType">从 Header 中分析出来的类型 Content-Type</param>
		Public Shared Function GetEncodeName(content As Byte(), Optional contentType As String = "") As String
			Dim Value = ""

			If content?.Length > 3 Then
				'1. BOM(Byte Order Mark) 提取编码
				If content(0) > &HEE Then
					If content(0) = &HEF And content(1) = &HBB And content(2) = &HBF Then
						Value = "UTF-8"
					ElseIf content(0) = &HFE And content(1) = &HFF Then
						Value = "Big-Endian"
					ElseIf content(0) = &HFF And content(1) = &HFE Then
						Value = "Unicode"
					End If
				End If

				If Value.IsEmpty Then
					'2. Header 提取编码
					'   Content-Type: text/html; charset=UTF-8
					'   正则表达式：charset\b\s*=\s*(?<charset>[^""]*)
					If contentType.NotEmpty Then
						Dim pa = "charset\b\s*=\s*(?<charset>[^""]*)"
						If contentType.Contains("charset=", StringComparison.OrdinalIgnoreCase) AndAlso Regex.IsMatch(contentType, pa) Then Value = Regex.Match(contentType, pa).Groups("charset").Value
					End If
				End If

				If Value.IsEmpty Then
					'3. HTML 代码中提取
					'   <meta http-equiv=content-type content="text/html; charset=GB2312">
					'   正则表达式：(<meta[^>]*charset=(?<charset>[^>'""]*)[\s\S]*?>)|(xml[^>]+encoding=(""|')*(?<charset>[^>'""]*)[\s\S]*?>)
					Dim Match = Regex.Match(Text.Encoding.Default.GetString(content), "(<meta[^>]*charset=(?<charset>[^>'""]*)[\s\S]*?>)|(xml[^>]+encoding=(""|')*(?<charset>[^>'""]*)[\s\S]*?>)", RegexOptions.IgnoreCase)
					If Match.Length > 0 Then Value = If(Match.Captures.Count <> 0, Match.Result("${charset}"), "")
				End If
			End If

			Return Value.EmptyValue("UTF-8")
		End Function

		''' <summary>分析编码类型</summary>
		Public Shared Function NameValue2QueryString(data As NameValueDictionary, Optional url As String = "") As String
			Dim R = url

			If data?.Count > 0 Then
				Dim s = data.ToQueryString

				If url.IsEmpty Then
					R = s
				ElseIf R.Contains("?"c) Then
					R &= "&"c & s
				Else
					R &= "?"c & s
				End If
			End If

			Return R
		End Function

		''' <summary>分析编码类型</summary>
		Public Shared Function NameValue2QueryString(data As Specialized.NameValueCollection, Optional url As String = "") As String
			Dim R = url

			If data?.Count > 0 Then
				Dim s = String.Join("&"c, data.AllKeys.Select(Function(a) $"{Web.HttpUtility.UrlEncode(a)}={Web.HttpUtility.UrlEncode(data(a))}"))

				If url.IsEmpty Then
					R = s
				ElseIf R.Contains("?"c) Then
					R &= "&"c & s
				Else
					R &= "?"c & s
				End If
			End If

			Return R
		End Function

		'-------------------------------------------------------------------------------

		''' <summary>HTTP 请求</summary>
		Public Shared Function Ajax(params As IDictionary(Of String, Object)) As KeyValueDictionary
			If params Is Nothing Then Throw New Exception("无效请求参数")

			Dim options As New KeyValueDictionary(params)

			' 请求地址
			Dim url = options.GetValue("url")
			If url.IsEmpty Then Throw New Exception("无效请求地址")

			' 创建客户端
			Dim client As New HttpClient With {
				.Url = url,
				.Referer = options.GetValue("referer"),
				.UserAgent = options.GetValue("useragent"),
				.AllowAutoRedirect = options.GetValue("autoredirect", True),
				.ContentType = options.GetValue("contenttype"),
				.PostType = options.GetValue("posttype", HttpPostEnum.DEFAULT),
				.Timeout = options.GetValue("timeout", 30000)
			}

			' 请求方式
			client.SetMethod(options.GetValue("method", "GET"))

			' 分析头部信息
			Dim headers = options.GetValue(Of IDictionary(Of String, Object))("headers", Nothing)
			If headers IsNot Nothing Then
				' 附加头部数据
				For Each key In headers.Keys
					client.SetHeader(key, headers(key))
				Next
			End If

			' 如果为字符串则直接使用，如果为对象则转换成 JSON
			Dim data = options("data")
			If data IsNot Nothing Then
				Dim dataType = data.GetType
				If dataType.IsString Then
					client.SetRawContent(data)
				ElseIf dataType.IsDictionary Then
					Dim dic = TryCast(data, IDictionary)
					If dic Is Nothing Then
						For Each key In dic.Keys
							client.SetContent(key, dic(key))
						Next
					End If
				End If
			End If

			' Cookies 数据
			Dim cookies = options("cookies")
			If cookies IsNot Nothing Then
				Dim dataType = cookies.GetType
				If dataType.IsString Then
					client.SetCookies(cookies)
				ElseIf dataType.IsDictionary Then
					Dim dic = TryCast(data, IDictionary)
					If dic Is Nothing Then
						For Each key In dic.Keys
							client.SetCookie(key, dic(key))
						Next
					End If
				End If
			End If

			client.Execute()

			Dim response = client.Response

			Return New KeyValueDictionary From {
				{"StatusCode", response.StatusCode},
				{"StatusDescription", response.StatusDescription},
				{"Content", response.HtmlContent},
				{"Headers", response.Headers},
				{"url", response.Url}
			}
		End Function

		'-------------------------------------------------------------------------------

		''' <summary>API 请求</summary>
		''' <returns>返回获取到的 JSON 结果</returns>
		Public Shared Function Api(params As IDictionary(Of String, Object)) As String
			If params Is Nothing Then Throw New Exception("无效请求参数")

			Dim options As New KeyValueDictionary(params)

			' 请求地址
			Dim url = options.GetValue("url")
			If url.IsEmpty Then Throw New Exception("无效 API 地址")

			' 请求方式
			Dim method = options.GetValue("method", HttpMethodEnum.GET)

			' 创建客户端
			Dim client As New ApiClient With {
				.Token = options.GetValue("token"),
				.Referer = options.GetValue("referer"),
				.UserAgent = options.GetValue("useragent"),
				.Timeout = options.GetValue("timeout", 30000)
			}

			' 附加 Cookies
			client.Cookies.Add(options.GetValue("cookies").ToCookies)

			' 分析头部信息
			Dim headers = options.GetValue(Of IDictionary(Of String, Object))("headers", Nothing)
			If headers IsNot Nothing Then
				' 附加头部数据
				For Each key In headers.Keys
					client.SetHeader(key, headers(key))
				Next
			End If

			' 检查提交数据
			Dim json = ""

			' 如果为字符串则直接使用，如果为对象则转换成 JSON
			Dim data = options("data")
			If data IsNot Nothing Then
				Dim dataType = data.GetType
				If dataType.IsString Then
					json = data
				ElseIf dataType.IsDictionary OrElse dataType.IsList OrElse dataType.IsArray Then
					json = Extension.ToJson(data, False, False)
				End If
			End If

			Return client.Execute(method, url, json)
		End Function

	End Class

End Namespace