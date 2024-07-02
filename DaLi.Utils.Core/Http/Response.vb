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
' 	Http 反馈
'
' 	name: Http.Response
' 	create: 2020-11-17
' 	memo: Http 反馈
' 	
' ------------------------------------------------------------

Imports System.IO
Imports System.Net

Namespace Http

	''' <summary>Http 反馈</summary>
	Public Class Response

#Region "参数"
		''' <summary>数据读取缓存大小，一般无需处理</summary>
		Public BufferSize As Integer = 10240

		''' <summary>当前操作的网址</summary>
		Public Url As String

		''' <summary>获取响应的字符集。</summary>
		Public CharacterSet As String

		''' <summary>获取用于对响应体进行编码的方法。</summary>
		Public ContentEncoding As String

		''' <summary>获取请求返回的内容的长度。</summary>
		Public ContentLength As Long

		''' <summary>获取响应的内容类型。</summary>
		Public ContentType As String

		''' <summary>获取或设置与此响应关联的 Cookie。</summary>
		Public Cookies As CookieCollection

		''' <summary>获取来自服务器的与此响应关联的标头。</summary>
		Public Headers As NameValueDictionary

		''' <summary>获取响应的状态。</summary>
		Public StatusCode As HttpStatusCode

		''' <summary>获取与响应一起返回的状态说明。</summary>
		Public StatusDescription As String

#End Region

#Region "属性"
		''' <summary>获取的内容。</summary>
		Private _Content As Stream

		''' <summary>获取的内容。</summary>
		Public Property Content As Stream
			Get
				Return _Content
			End Get
			Set(value As Stream)
				_Content = value
				_BytesContent = Nothing
			End Set
		End Property

		''' <summary>字节类型内容，用于缓存字节数据，防止二次读取将丢失。每次写入 Stream 自动重置</summary>
		Private _BytesContent As Byte()

		''' <summary>字节类型内容</summary>
		Public ReadOnly Property BytesContent() As Byte()
			Get
				If _BytesContent Is Nothing Then
					If Content IsNot Nothing Then
						BufferSize = BufferSize.Range(512, Integer.MaxValue)

						Using Memory As New IO.MemoryStream
							Using Content
								Dim Buffer = New Byte(BufferSize - 1) {}

								While True
									Dim size As Integer = Content.Read(Buffer, 0, Buffer.Length)
									If size > 0 Then
										Memory.Write(Buffer, 0, size)
									Else
										Exit While
									End If
								End While
							End Using

							_BytesContent = Memory.ToArray()
						End Using
					End If
				End If

				Return _BytesContent
			End Get
		End Property

		''' <summary>网页类型内容（自动分析编码）</summary>
		Public ReadOnly Property HtmlContent(Optional encoding As Text.Encoding = Nothing) As String
			Get
				Dim Data = BytesContent
				If Data?.Length > 0 Then
					' 自动分析编码
					If encoding Is Nothing Then
						EncodingRegister()

						Dim encodeName = NetHelper.GetEncodeName(Data, ContentType)
						encoding = Text.Encoding.GetEncoding(encodeName)
					End If

					Return encoding.GetString(Data)
				End If

				Return ""
			End Get
		End Property

		''' <summary>字符串类型内容</summary>
		Public ReadOnly Property StringContent(Optional encoding As Text.Encoding = Nothing) As String
			Get
				Dim Data = BytesContent
				If Data?.Length > 0 Then
					encoding = If(encoding, Text.Encoding.UTF8)
					Return encoding.GetString(Data)
				End If

				Return ""
			End Get
		End Property

		''' <summary>Json 类型内容</summary>
		Public ReadOnly Property JsonContent(Optional encoding As Text.Encoding = Nothing) As (Value As Object, IsList As Boolean)
			Get
				Dim data = HtmlContent
				If data.NotEmpty Then
					Return data.ToJsonCollection
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>Json 类型内容</summary>
		Public ReadOnly Property JsonContent(type As Type, Optional encoding As Text.Encoding = Nothing) As Object
			Get
				Dim data = HtmlContent
				If data.NotEmpty Then
					Return data.ToJsonObject(type)
				Else
					Return Nothing
				End If
			End Get
		End Property

		''' <summary>保存附件</summary>
		''' <param name="fileFloder">文件保存的路径</param>
		''' <param name="fileExists">文件存在的操作</param>
		''' <param name="fileName">文件名</param>
		Public ReadOnly Property FileContent(fileFloder As String, Optional fileExists As ExistsActionEnum = ExistsActionEnum.CANCEL, Optional fileName As String = "") As (Path As String, Flag As Boolean)
			Get
				If Content IsNot Nothing Then
					Dim Flag = False

					fileFloder = PathHelper.Root(fileFloder.ToPath)
					fileName = fileName.ToFileName

					' 文件名不存在，从反馈信息中获取文件名
					If fileName.IsEmpty Then
						Dim info = Headers("Content-Disposition")
						If info.NotEmpty Then
							info = info.ToLower
							If info.Contains("filename=") Then
								info = info.Split("filename=")(1)
								fileName = info.Replace("""", "").Replace("'", "")
							End If
						End If
					End If

					' 文件名不存在，从网址分析
					If fileName.IsEmpty Then
						fileName = Url.Replace("?", " ").Replace("#", " ").Split(" "c)(0)

						' '从 Mime 获取文件格式
						Dim Ext As String = NetHelper.Mime2Ext(ContentType)
						If Ext.NotEmpty Then Ext = Ext.Split(","c)(0)

						' 不存在扩展名，从网址获取
						If Ext.IsEmpty OrElse Ext = "*" Then Ext = IO.Path.GetExtension(fileName)

						' 从网址获取文件名
						fileName = IO.Path.GetFileNameWithoutExtension(fileName).EmptyValue(Url.MD5(False))

						fileName &= Ext
					End If

					' 检查文件是否存在
					Dim filePath As String = IO.Path.Combine(fileFloder, fileName)
					If PathHelper.FileExist(filePath) Then
						If fileExists = ExistsActionEnum.CANCEL Then
							' 如果存在，则忽略
							Flag = True

						ElseIf fileExists = ExistsActionEnum.RENAME Then
							' 重命名文件
							Dim I As Integer = 1
							Dim namePart = IO.Path.GetFileNameWithoutExtension(fileName)
							Dim extPart = IO.Path.GetExtension(fileName)

							While True
								filePath = IO.Path.Combine(fileFloder, $"{namePart}({I}){extPart}")
								If PathHelper.FileExist(filePath) Then
									I += 1
								Else
									Exit While
								End If
							End While
						End If
					End If

					If Not Flag Then
						PathHelper.FolderCreate(filePath, False)

						' 保存数据
						Try
							If fileExists = ExistsActionEnum.APPEND Then
								Using fs As New IO.FileStream(filePath, IO.FileMode.Append, IO.FileAccess.Write)
									SaveStream(fs)
								End Using
							Else
								Using fs As New IO.FileStream(filePath, IO.FileMode.Create, IO.FileAccess.Write)
									SaveStream(fs)
								End Using
							End If

							Flag = True
						Catch ex As Exception
						End Try
					End If

					Return (filePath, Flag)
				End If

				Return ("", False)
			End Get
		End Property

		''' <summary>获取指定头部内容。</summary>
		Public ReadOnly Property Header(name As String) As String
			Get
				Return Headers?(name)
			End Get
		End Property

		''' <summary>获取当前跳转地址。</summary>
		Public ReadOnly Property Location As String
			Get
				Return Header("Location")
			End Get
		End Property

#End Region

#Region "相关操作"

		''' <summary>重新初始化参数</summary>
		Public Sub Reset()
			Url = ""
			CharacterSet = ""
			ContentEncoding = ""
			ContentLength = 0
			ContentType = ""

			Cookies = Nothing
			Headers = Nothing

			StatusCode = 0
			StatusDescription = ""

			Content?.Dispose()
			Content = Nothing
		End Sub

		''' <summary>保存数据流</summary>
		Private Sub SaveStream(fs As FileStream)
			BufferSize = BufferSize.Range(512, Integer.MaxValue)

			Using Content
				Dim Buffer = New Byte(BufferSize - 1) {}

				While True
					Dim size As Integer = Content.Read(Buffer, 0, Buffer.Length)
					If size > 0 Then
						fs.Write(Buffer, 0, size)
					Else
						Exit While
					End If
				End While
			End Using
		End Sub

#End Region

	End Class
End Namespace
