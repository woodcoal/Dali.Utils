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
' 	自定义格式数据保存
'
' 	name: Store
' 	create: 2019-03-14
' 	memo: 专用格式数据保存
'   文件结构： ****** ***  ****** *            ****** ****************** ****************
'             名称   版本  时间   干扰码长度    干扰码         内容             Hash
'               5    3    6      1                                            16
' 	
' ------------------------------------------------------------

Imports System.Text

''' <summary>专用格式数据保存</summary>
Public Class Store

	Private _Name As String
	Private _Version As String
	Private _Key As String
	Private ReadOnly _EditDate(5) As Byte

	'------------------------------------------------

	''' <summary>文件名称，5 个 Ascii 字符</summary>
	Public Property Name As String
		Get
			Return _Name
		End Get
		Set(value As String)
			_Name = (value.GetAscii.TrimFull & New String("."c, 5)).Left(5)
		End Set
	End Property

	''' <summary>文件版本，3 个 Ascii 字符</summary>
	Public Property Version As String
		Get
			Return _Version
		End Get
		Set(value As String)
			_Version = (value.GetAscii.TrimFull & New String("."c, 3)).Left(3)
		End Set
	End Property

	''' <summary>密匙，8 个 Ascii 字符</summary>
	Public WriteOnly Property Key As String
		Set(value As String)
			_Key = value.EmptyValue("MuTan.2020").MD5(False)
		End Set
	End Property

	''' <summary>编辑时间</summary>
	Public ReadOnly Property EditDate As Date
		Get
			Try
				Return New Date(_EditDate(0) + 2000, _EditDate(2), _EditDate(4), _EditDate(1), _EditDate(3), _EditDate(5))
			Catch ex As Exception
				Return New Date
			End Try
		End Get
	End Property

	''' <summary>数据内容</summary>
	Public Property Content As Byte()

	'------------------------------------------------

	Public Sub New()
		Me.New("", "")
	End Sub

	Public Sub New(path As String)
		Call Read(path)
	End Sub

	Public Sub New(name As String, version As String)
		Me.Name = name.EmptyValue("MuTan")
		Me.Version = version.EmptyValue(".20")
	End Sub

#Region "内容加解密处理"

	''' <summary>内容加密及生成</summary>
	Private Function Encrypt() As Byte()
		Dim Value As Byte() = Nothing

		Try
			'前十字节为结构定义
			'前六字节为名称，后四字节为版本
			Using fileStream As New IO.MemoryStream
				'名称
				fileStream.Write(Encoding.ASCII.GetBytes(_Name), 0, 5)

				'版本
				fileStream.Write(Encoding.ASCII.GetBytes(_Version), 0, 3)

				'时间
				'---------------------------------
				Dim Dt = SYS_NOW_DATE
				_EditDate(0) = Dt.Year - 2000
				_EditDate(2) = Dt.Month
				_EditDate(4) = Dt.Day
				_EditDate(1) = Dt.Hour
				_EditDate(3) = Dt.Minute
				_EditDate(5) = Dt.Second
				fileStream.Write(_EditDate, 0, _EditDate.Length)

				'干扰
				'---------------------------------
				Dim _Salt As Byte = CInt(RandomHelper.Number(2) / 2) + 1
				fileStream.WriteByte(_Salt)

				'干扰数据
				'---------------------------------
				fileStream.Write(Encoding.ASCII.GetBytes(RandomHelper.Mix(_Salt)), 0, _Salt)

				'内容加密
				'---------------------------------
				If Content IsNot Nothing Then
					Content = New SecurityHelper.Des().Encrypt(Content, _Key)
					fileStream.Write(Content, 0, Content.Length)
				Else
					fileStream.WriteByte(0)
				End If

				'Hash
				'---------------------------------
				Dim _Hash As String = HashHelper.ComputeHash(Content, HashModeEnum.MD5) & vbCrLf & HashHelper.ComputeHash(_Key, HashModeEnum.MD5) & vbCrLf & HashHelper.ComputeHash(_EditDate, HashModeEnum.MD5)
				fileStream.Write(Text.Encoding.ASCII.GetBytes(_Hash.MD5(False)), 0, 16)

				'输出
				'---------------------------------
				Value = fileStream.ToArray
			End Using
		Catch ex As Exception
		End Try

		Return Value
	End Function

	''' <summary>内容解密及还原</summary>
	Public Sub Decrypt(contentByte As Byte())
		_Name = ""
		_Version = ""
		Content = Nothing

		If contentByte?.Length > 32 Then
			Using fileStream As New IO.MemoryStream(contentByte, False)
				Try
					Dim _ContentByte(0) As Byte

					'名称
					'---------------------------------
					ReDim _ContentByte(4)
					fileStream.Read(_ContentByte, 0, _ContentByte.Length)
					_Name = Encoding.ASCII.GetString(_ContentByte)

					'版本
					'---------------------------------
					ReDim _ContentByte(2)
					fileStream.Read(_ContentByte, 0, _ContentByte.Length)
					_Version = Encoding.ASCII.GetString(_ContentByte)

					'时间
					'---------------------------------
					fileStream.Read(_EditDate, 0, 6)

					'干扰
					'---------------------------------
					Dim _Salt As Byte = fileStream.ReadByte()
					If fileStream.Length > 16 + _Salt Then

						'干扰数据
						'---------------------------------
						ReDim _ContentByte(_Salt - 1)
						fileStream.Read(_ContentByte, 0, _ContentByte.Length)

						If fileStream.Length > 32 + _Salt Then
							'内容解密
							'---------------------------------
							ReDim Content(fileStream.Length - 32 - _Salt)
							fileStream.Read(Content, 0, Content.Length)

							'Hash
							'---------------------------------
							ReDim _ContentByte(15)
							fileStream.Read(_ContentByte, 0, 16)
							Dim _Hash As String = HashHelper.ComputeHash(Content, HashModeEnum.MD5) & vbCrLf & HashHelper.ComputeHash(_Key, HashModeEnum.MD5) & vbCrLf & HashHelper.ComputeHash(_EditDate, HashModeEnum.MD5)
							_Hash = _Hash.MD5(False)
							If _Hash = Encoding.ASCII.GetString(_ContentByte) Then
								'解密
								Content = New SecurityHelper.Des().Decrypt(Content, _Key)
							End If
						End If
					End If
				Catch ex As Exception
					Content = Nothing
				End Try
			End Using
		End If
	End Sub

#End Region

#Region "读写文件"

	''' <summary>保存到文件</summary>
	Public Function Save(file As String) As Boolean
		Dim Value = False

		Try
			If file.NotEmpty Then
				file = PathHelper.Root(file, True)

				IO.File.WriteAllBytes(file, Encrypt)
				Value = True
			End If
		Catch ex As Exception
		End Try

		Return Value
	End Function

	''' <summary>打开文件</summary>
	Public Sub Read(file As String)
		_Name = ""
		_Version = ""
		Content = Nothing

		If PathHelper.FileExist(file) Then
			Try
				Call Decrypt(IO.File.ReadAllBytes(file))
			Catch ex As Exception
			End Try
		End If
	End Sub

#End Region

#Region "共享操作"

	''' <summary>保存到文件</summary>
	Public Shared Function Save(path As String, content As Byte(), Optional key As String = "", Optional name As String = "", Optional ver As String = "") As Boolean
		Dim Value As Boolean = False

		Try
			If content IsNot Nothing And Not String.IsNullOrEmpty(path) Then
				'写入文件
				With New Store(name, ver)
					.Key = key
					.Content = content
					Value = .Save(path)
				End With
			End If
		Catch ex As Exception
		End Try

		Return Value
	End Function

	''' <summary>打开文件</summary>
	Public Shared Function Read(path As String, Optional key As String = "") As (Content As Byte(), Name As String, Version As String, EditDate As Date)
		Try
			With New Store
				.Key = key
				.Read(path)

				Return (.Content, .Name, .Version, .EditDate)
			End With
		Catch ex As Exception
		End Try

		Return Nothing
	End Function

	''' <summary>打开文件</summary>
	Public Shared Function Read(path As String, Optional key As String = "", Optional name As String = "", Optional ver As String = "") As Byte()
		Dim Ret = Read(path, key)
		If Ret.Name.IsSame(name) AndAlso Ret.Version.IsSame(ver) AndAlso Ret.EditDate > New Date(2000, 1, 1) AndAlso Ret.EditDate < SYS_NOW_DATE Then
			Return Ret.Content
		Else
			Return Nothing
		End If
	End Function

	''' <summary>最后时间</summary>
	Public Shared Function Last(path As String, Optional key As String = "", Optional name As String = "", Optional ver As String = "") As Date
		Dim Ret = Read(path, key)
		If Ret.Name.IsSame(name) AndAlso Ret.Version.IsSame(ver) AndAlso Ret.EditDate > New Date(2000, 1, 1) AndAlso Ret.EditDate < SYS_NOW_DATE Then
			Return Ret.EditDate
		Else
			Return Nothing
		End If
	End Function

#End Region

End Class
