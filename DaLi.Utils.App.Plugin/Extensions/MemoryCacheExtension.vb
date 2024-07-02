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
' 	内存缓存扩展操作
'
' 	name: Extension.MemoryCacheExtension
' 	create: 2023-02-14
' 	memo: 内存缓存扩展操作
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports Microsoft.Extensions.Caching.Memory

Namespace Extension

	''' <summary>内存缓存扩展操作</summary>
	Public Module MemoryCacheExtension

		Private Function GetOptions(expireTime As Integer, expirationMode As Boolean) As MemoryCacheEntryOptions
			Dim options As New MemoryCacheEntryOptions

			If expirationMode Then
				options.SlidingExpiration = TimeSpan.FromSeconds(expireTime)
			Else
				options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expireTime)
			End If

			Return options
		End Function

#Region "READ 读取"

		''' <summary>读取缓存</summary>
		<Extension>
		Public Function Read(this As IMemoryCache, key As String) As String
			If this IsNot Nothing AndAlso key.NotEmpty Then
				Return this.Get(Of String)(key)
			Else
				Return ""
			End If
		End Function

		''' <summary>读取缓存</summary>
		<Extension>
		Public Function Read(Of T)(this As IMemoryCache, key As String) As T
			If this IsNot Nothing AndAlso key.NotEmpty Then
				Return this.Get(Of T)(key)
			End If

			Return Nothing
		End Function

#End Region

#Region "SAVE 保存"

		''' <summary>写入缓存</summary>
		<Extension>
		Public Sub Save(Of T)(this As IMemoryCache, key As String, value As T, Optional options As MemoryCacheEntryOptions = Nothing)
			If this IsNot Nothing AndAlso key.NotEmpty Then
				If value Is Nothing Then
					this.Remove(key)
				Else
					If options IsNot Nothing Then
						this.Set(key, value, options)
					Else
						this.Set(key, value)
					End If
				End If
			End If
		End Sub

		''' <summary>写入缓存</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Sub Save(Of T)(this As IMemoryCache, key As String, value As T, expireTime As Integer, Optional expirationMode As Boolean = False)
			If expireTime < 1 Then Return
			this.Save(key, value, GetOptions(expireTime, expirationMode))
		End Sub

		''' <summary>写入缓存</summary>
		<Extension>
		Public Sub Save(Of T)(this As IMemoryCache, key As String, value As T, expireDate As Date)
			this.Save(key, value, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False)
		End Sub

#End Region

#Region "READ OR SAVE 读取，不存在新添加"

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Function ReadOrSave(Of T)(this As IMemoryCache, key As String, nullValueAction As Func(Of T), Optional options As MemoryCacheEntryOptions = Nothing) As T
			Dim value = this.Read(Of T)(key)

			If value Is Nothing AndAlso nullValueAction IsNot Nothing Then
				value = nullValueAction.Invoke
				this.Save(key, value, options)
			End If

			Return value
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Function ReadOrSave(Of T)(this As IMemoryCache, key As String, nullValueAction As Func(Of T), expireTime As Integer, Optional expirationMode As Boolean = False) As T
			If expireTime < 1 Then
				Return this.Read(Of T)(key)
			Else
				Return this.ReadOrSave(Of T)(key, nullValueAction, GetOptions(expireTime, expirationMode))
			End If
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Function ReadOrSave(Of T)(this As IMemoryCache, key As String, nullValueAction As Func(Of T), expireDate As Date) As T
			Return this.ReadOrSave(Of T)(key, nullValueAction, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False)
		End Function

#End Region

#Region "DELETE 删除"

		''' <summary>移除缓存</summary>
		<Extension()>
		Public Sub Delete(this As IMemoryCache, key As String)
			this.Remove(key)
		End Sub

#End Region
	End Module
End Namespace