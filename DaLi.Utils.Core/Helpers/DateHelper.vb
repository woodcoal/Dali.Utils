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
' 	日期时间处理相关操作
'
' 	name: Helper.DateHelper
' 	create: 2019-03-14
' 	memo: 日期时间处理相关操作
' 	
' ------------------------------------------------------------

Namespace Helper

	''' <summary>日期时间处理相关操作</summary>
	Public NotInheritable Class DateHelper

		''' <summary>当前时区</summary>
		Public Shared Function TimeZone() As Integer
			'Return System.TimeZone.CurrentTimeZone.GetUtcOffset(Date.Now).TotalHours
			Return Date.Now.ToString("%z").ToInteger
		End Function

		''' <summary>通过网络获取当前时间（标准时间）</summary>
		''' <remarks>端口 13 时间服务</remarks>
		Public Shared Function NetDate(Optional timeZone As Byte = 8) As Date
			Dim Result As New Date

			' 返回国际标准时间
			' 只使用 timeServers 的 IP 地址，未使用域名
			Try
				Dim Servers = "128.138.140.44,129.6.15.28,129.6.15.29,131.107.1.10,132.163.4.101,132.163.4.102,132.163.4.103,192.43.244.18,207.126.98.204,207.200.81.113,208.184.49.9,216.200.93.8,64.236.96.53,69.25.96.13,chronos.csr.net,clock.cmc.ec.gc.ca,nist1.aol-ca.truetime.com,nist1.aol-va.truetime.com,nist1.datum.com,nist1.symmetricom.com,nist1-dc.glassey.com,nist1-ny.glassey.com,nist1-sj.glassey.com,ptbtime1.ptb.de,time.nist.gov,time-a.nist.gov,time-a.timefreq.bldrdoc.gov,time-b.nist.gov,time-b.timefreq.bldrdoc.gov,time-c.timefreq.bldrdoc.gov,time-nw.nist.gov,utcnist.colorado.edu,www.time.ac.cn"
				Dim Port = 13

				Dim Bytes() As Byte = Nothing
				Dim ByteLen = 0

				Dim s = ""
				For Each Server As String In Servers.SplitDistinct()
					ByteLen = 0
					ReDim Bytes(100)

					Using TCP As New System.Net.Sockets.TcpClient
						Try
							TCP.Connect(Server, Port)

							Using Ns As System.Net.Sockets.NetworkStream = TCP.GetStream
								ByteLen = Ns.Read(Bytes, 0, Bytes.Length)
							End Using
						Catch ex As Exception
						End Try
					End Using

					If ByteLen > 0 Then Exit For
				Next

				If ByteLen > 0 Then
					Dim strDate = Text.Encoding.ASCII.GetString(Bytes, 0, ByteLen)

					If Not String.IsNullOrWhiteSpace(strDate) Then
						Dim DL = strDate.Split(" "c)

						If DL.Length > 1 Then
							Result = (DL(1) & " " & DL(2)).ToDateTime
							Result = Result.AddHours(timeZone)
						End If
					End If
				End If
			Catch ex As Exception
			End Try

			If Result > New Date(2019, 1, 1) Then
				Return Result
			Else
				Return New Date
			End If
		End Function

		'''' <summary>通过网页获取当前时间（标准时间）</summary>
		'''' <remarks>从网页获取</remarks>
		'Public Shared Function WebDate(Optional TimeZone As Byte = 8) As Date
		'	Dim Value = SYS_NOW_DATE

		'	Dim Http As New Core.Net.HttpClient With {
		'	.Url = "http://time.tianqi.com/?t=" & Part.Random.Timer,
		'	.Referer = .Url
		'}

		'	Dim Html = Http.GetString
		'	If Not String.IsNullOrEmpty(Html) Then
		'		Html = Html.Cut("var timestamp = """, """", False, False)
		'		Value = Html.toDateTime(SYS_NOW_DATE)
		'	End If

		'	' 从自己的网站获取时间
		'	If String.IsNullOrEmpty(Html) Then
		'		Dim Rnd = Guid.NewGuid.ToString
		'		Http.Url = "http://service.xiongdi.org/api/Common/Ping/" & Rnd
		'		Http.Referer = Http.Url
		'		Html = Http.GetString

		'		'Hello:Rnd!From:36.157.96.182,At:2019-12-12 16:27:20
		'		If Not String.IsNullOrEmpty(Html) AndAlso Html.Contains(Rnd) Then
		'			Dim Arrs = Core.String.Split(Html, "At:")
		'			If Arrs?.Length > 0 Then Value = Core.String.Parse.DateTime(Arrs(Arrs.Length - 1), Timer.Now)
		'		End If
		'	End If

		'	If Value > New Date(2019, 12, 1) Then
		'		Return Value.AddHours(TimeZone - 8)
		'	Else
		'		Return New Date
		'	End If
		'End Function

	End Class

End Namespace