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
' 	图片基础操作
'
' 	name: ImageHelper
' 	create: 2024-01-22
' 	memo: 图片基础操作
'
' ------------------------------------------------------------

Imports System.IO
Imports SkiaSharp

''' <summary>图片基础操作</summary>
Public NotInheritable Class ImageHelper

#Region "调整到指定大小"

	''' <summary>将图片调整到指定大小</summary>
	''' <param name="file">图片文件</param>
	''' <param name="width">宽度</param>
	''' <param name="height">高度</param>
	''' <param name="zoom">小于尺寸时是否放大,False 不放大</param>
	''' <param name="format">转换格式</param>
	''' <param name="quality">转换质量</param>
	Public Shared Function FixedSize(file As String, width As Integer, height As Integer, Optional zoom As Boolean = False, Optional format As SKEncodedImageFormat = SKEncodedImageFormat.Jpeg, Optional quality As Integer = 80, Optional newFile As String = "") As MemoryStream
		Dim ret As MemoryStream = Nothing
		If Not PathHelper.FileExist(file) Then Return Nothing

		Using inputStream As New FileStream(file, FileMode.Open, FileAccess.Read)
			ret = FixedSize(inputStream, width, height, zoom, format, quality)
		End Using

		If ret IsNot Nothing AndAlso newFile.NotEmpty Then
			newFile = PathHelper.Root(newFile)
			Using fileStream As New FileStream(newFile, FileMode.Create, FileAccess.Write)
				ret.WriteTo(fileStream)
			End Using
		End If

		Return ret
	End Function

	''' <summary>将图片调整到指定大小</summary>
	''' <param name="stream">图像流</param>
	''' <param name="width">宽度</param>
	''' <param name="height">高度</param>
	''' <param name="zoom">小于尺寸时是否放大,False 不放大</param>
	''' <param name="format">转换格式</param>
	''' <param name="quality">转换质量</param>
	Public Shared Function FixedSize(stream As Stream, width As Integer, height As Integer, Optional zoom As Boolean = False, Optional format As SKEncodedImageFormat = SKEncodedImageFormat.Jpeg, Optional quality As Integer = 80) As MemoryStream
		Dim ret As MemoryStream = Nothing
		If stream Is Nothing Then Return ret

		Using bmp = SKBitmap.Decode(stream)
			' 计算缩放比例
			Dim scaleX As Single = width / bmp.Width
			Dim scaleY As Single = height / bmp.Height
			Dim scale As Single = Math.Min(scaleX, scaleY)

			' 如果图片为放大则使用原尺寸
			If scale > 1 AndAlso Not zoom Then scale = 1

			' 创建目标大小的位图
			Using img As New SKBitmap(bmp.Width * scale, bmp.Height * scale)
				' 使用SkiaSharp的画布进行缩放
				Using resizedCanvas As New SKCanvas(img)
					resizedCanvas.Clear(SKColors.Transparent)
					resizedCanvas.Scale(scale)

					' 将原图绘制到目标大小的位图上
					resizedCanvas.DrawBitmap(bmp, 0, 0)
				End Using

				ret = New MemoryStream
				img.Encode(format, quality).SaveTo(ret)
			End Using
		End Using

		Return ret
	End Function

	''' <summary>将图片调整到指定大小</summary>
	''' <param name="data">图像数据</param>
	''' <param name="width">宽度</param>
	''' <param name="height">高度</param>
	''' <param name="zoom">小于尺寸时是否放大,False 不放大</param>
	''' <param name="format">转换格式</param>
	''' <param name="quality">转换质量</param>
	Public Shared Function FixedSize(data As Byte(), width As Integer, height As Integer, Optional zoom As Boolean = False, Optional format As SKEncodedImageFormat = SKEncodedImageFormat.Jpeg, Optional quality As Integer = 80) As Byte()
		Dim ret As Byte() = Nothing
		If data Is Nothing Then Return ret

		Using Input As New MemoryStream(ret)
			Using output = FixedSize(Input, width, height, zoom, format, quality)
				ret = output?.ToArray()
			End Using
		End Using

		Return ret
	End Function

#End Region

#Region "剪裁到指定大小"

	''' <summary>将图片剪裁到指定大小</summary>
	''' <param name="file">图片文件</param>
	''' <param name="width">宽度</param>
	''' <param name="height">高度</param>
	''' <param name="zoom">小于尺寸时是否放大,False 不放大</param>
	''' <param name="format">转换格式</param>
	''' <param name="quality">转换质量</param>
	''' <param name="newFile">是否保存修改后的文件，保存则设置文件路径</param>
	Public Shared Function CutSize(file As String, width As Integer, height As Integer, Optional zoom As Boolean = False, Optional format As SKEncodedImageFormat = SKEncodedImageFormat.Jpeg, Optional quality As Integer = 80, Optional newFile As String = "") As MemoryStream
		Dim ret As MemoryStream = Nothing
		If Not PathHelper.FileExist(file) Then Return Nothing

		Using inputStream As New FileStream(file, FileMode.Open, FileAccess.Read)
			ret = CutSize(inputStream, width, height, zoom, format, quality)
		End Using

		If ret IsNot Nothing AndAlso newFile.NotEmpty Then
			newFile = PathHelper.Root(newFile)
			Using fileStream As New FileStream(newFile, FileMode.Create, FileAccess.Write)
				ret.WriteTo(fileStream)
			End Using
		End If

		Return ret
	End Function

	''' <summary>将图片剪裁到指定大小</summary>
	''' <param name="stream">图像流</param>
	''' <param name="width">宽度</param>
	''' <param name="height">高度</param>
	''' <param name="zoom">小于尺寸时是否放大,False 不放大</param>
	''' <param name="format">转换格式</param>
	''' <param name="quality">转换质量</param>
	Public Shared Function CutSize(stream As Stream, width As Integer, height As Integer, Optional zoom As Boolean = False, Optional format As SKEncodedImageFormat = SKEncodedImageFormat.Jpeg, Optional quality As Integer = 80) As MemoryStream
		Dim ret As MemoryStream = Nothing
		If stream Is Nothing Then Return ret

		Using bmp = SKBitmap.Decode(stream)
			' 计算缩放比例
			Dim scaleX As Single = width / bmp.Width
			Dim scaleY As Single = height / bmp.Height
			Dim scale As Single = Math.Max(scaleX, scaleY)

			' 如果图片为放大则使用原尺寸
			If scale > 1 AndAlso Not zoom Then scale = 1

			' 创建目标大小的位图
			Using img As New SKBitmap(width, height)
				' 使用SkiaSharp的画布进行缩放
				Using resizedCanvas As New SKCanvas(img)
					resizedCanvas.Clear(SKColors.Transparent)
					resizedCanvas.Scale(scale)

					' 将原图绘制到目标大小的位图上
					Dim left = CInt((width - (bmp.Width * scale)) / 2)
					Dim top = CInt((height - (bmp.Height * scale)) / 2)

					resizedCanvas.DrawBitmap(bmp, left, top)
				End Using

				ret = New MemoryStream
				img.Encode(format, quality).SaveTo(ret)
			End Using
		End Using

		'' 计算缩放比例
		'Dim scaleX As Single = targetWidth / bitmap.Width
		'Dim scaleY As Single = targetHeight / bitmap.Height
		'Dim scale As Single = Math.Max(scaleX, scaleY)

		'' 创建缩放后的位图
		'Dim scaledBitmap As New SKBitmap(CInt(bitmap.Width * scale), CInt(bitmap.Height * scale))

		'' 使用SkiaSharp的画布进行缩放
		'Using scaledCanvas As New SKCanvas(scaledBitmap)
		'	scaledCanvas.Clear(SKColors.Transparent)
		'	scaledCanvas.Scale(scale)

		'	' 将原图绘制到缩放后的位图上
		'	scaledCanvas.DrawBitmap(bitmap, 0, 0)
		'End Using

		'' 计算剪裁的矩形区域
		'Dim sourceRect As New SKRectI(CInt((scaledBitmap.Width - targetWidth) / 2), CInt((scaledBitmap.Height - targetHeight) / 2), CInt((scaledBitmap.Width + targetWidth) / 2), CInt((scaledBitmap.Height + targetHeight) / 2))

		'' 创建目标大小的位图
		'Dim croppedBitmap As New SKBitmap(targetWidth, targetHeight)

		'' 使用SkiaSharp的画布进行剪裁
		'Using croppedCanvas As New SKCanvas(croppedBitmap)
		'	croppedCanvas.Clear(SKColors.Transparent)

		'	' 将缩放后的图像按剪裁大小绘制到目标位图上
		'	croppedCanvas.DrawBitmap(scaledBitmap, sourceRect, New SKRect(0, 0, targetWidth, targetHeight))
		'End Using


		Return ret
	End Function

	''' <summary>将图片剪裁到指定大小</summary>
	''' <param name="data">图像数据</param>
	''' <param name="width">宽度</param>
	''' <param name="height">高度</param>
	''' <param name="zoom">小于尺寸时是否放大,False 不放大</param>
	''' <param name="format">转换格式</param>
	''' <param name="quality">转换质量</param>
	Public Shared Function CutSize(data As Byte(), width As Integer, height As Integer, Optional zoom As Boolean = False, Optional format As SKEncodedImageFormat = SKEncodedImageFormat.Jpeg, Optional quality As Integer = 80) As Byte()
		Dim ret As Byte() = Nothing
		If data Is Nothing Then Return ret

		Using Input As New MemoryStream(ret)
			Using output = CutSize(Input, width, height, zoom, format, quality)
				ret = output?.ToArray()
			End Using
		End Using

		Return ret
	End Function

#End Region

End Class
