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
' 	分布式缓存扩展操作
'
' 	name: Extension.DistributedCacheExtensions
' 	create: 2023-02-14
' 	memo: 分布式缓存扩展操作
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.Extensions.Caching.Distributed

Namespace Extension

	''' <summary>分布式缓存扩展操作</summary>
	Public Module DistributedCacheExtensions

		''' <summary>获取缓存名称</summary>
		Private Function CacheKey(key As String) As String
			Return key.EmptyValue.ToLower
		End Function

		Private Function GetOptions(expireTime As Integer, expirationMode As Boolean) As DistributedCacheEntryOptions
			Dim options As New DistributedCacheEntryOptions

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
		Public Function Read(this As IDistributedCache, key As String) As String
			If this IsNot Nothing AndAlso key.NotEmpty Then
				Return this.GetString(CacheKey(key))
			Else
				Return ""
			End If
		End Function

		''' <summary>读取缓存</summary>
		<Extension>
		Public Async Function ReadAsync(this As IDistributedCache, key As String, Optional token As CancellationToken = Nothing) As Task(Of String)
			If this IsNot Nothing AndAlso key.NotEmpty Then
				Return Await this.GetStringAsync(CacheKey(key), token)
			Else
				Return ""
			End If
		End Function

		''' <summary>读取缓存</summary>
		<Extension>
		Public Function Read(Of T)(this As IDistributedCache, key As String) As T
			If this IsNot Nothing AndAlso key.NotEmpty Then
				Dim value = this.GetString(CacheKey(key))
				If value.NotEmpty Then Return value.ToJsonObject(Of T)
			End If

			Return Nothing
		End Function

		''' <summary>获取缓存</summary>
		<Extension>
		Public Async Function ReadAsync(Of T)(this As IDistributedCache, key As String, Optional token As CancellationToken = Nothing) As Task(Of T)
			If this IsNot Nothing AndAlso key.NotEmpty Then
				Dim value = Await this.GetStringAsync(CacheKey(key), token)
				If value.NotEmpty Then Return value.ToJsonObject(Of T)
			End If

			Return Nothing
		End Function

#End Region

#Region "SAVE 保存"

		''' <summary>写入缓存</summary>
		<Extension>
		Public Sub Save(this As IDistributedCache, key As String, value As String, Optional options As DistributedCacheEntryOptions = Nothing)
			If this IsNot Nothing AndAlso key.NotEmpty Then
				key = CacheKey(key)

				If value.IsNull Then
					this.Remove(key)
				Else
					If options IsNot Nothing Then
						this.SetString(key, value, options)
					Else
						this.SetString(key, value)
					End If
				End If
			End If
		End Sub

		''' <summary>写入缓存</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Sub Save(this As IDistributedCache, key As String, value As String, expireTime As Integer, Optional expirationMode As Boolean = False)
			If expireTime < 1 Then Return
			this.Save(key, value, GetOptions(expireTime, expirationMode))
		End Sub

		''' <summary>写入缓存</summary>
		<Extension>
		Public Sub Save(this As IDistributedCache, key As String, value As String, expireDate As Date)
			this.Save(key, value, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False)
		End Sub

		'---------------------------------------------------------------------------

		''' <summary>写入缓存</summary>
		<Extension>
		Public Sub Save(Of T)(this As IDistributedCache, key As String, value As T, Optional options As DistributedCacheEntryOptions = Nothing)
			this.Save(key, value?.ToJson, options)
		End Sub

		''' <summary>写入缓存</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Sub Save(Of T)(this As IDistributedCache, key As String, value As T, expireTime As Integer, Optional expirationMode As Boolean = False)
			If expireTime < 1 Then Return
			this.Save(Of T)(key, value, GetOptions(expireTime, expirationMode))
		End Sub

		''' <summary>写入缓存</summary>
		<Extension>
		Public Sub Save(Of T)(this As IDistributedCache, key As String, value As T, expireDate As Date)
			this.Save(Of T)(key, value, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False)
		End Sub

		'---------------------------------------------------------------------------

		''' <summary>写入缓存</summary>
		<Extension>
		Public Async Function SaveAsync(this As IDistributedCache, key As String, value As String, Optional options As DistributedCacheEntryOptions = Nothing, Optional token As CancellationToken = Nothing) As Task
			If this IsNot Nothing AndAlso key.NotEmpty Then
				key = CacheKey(key)

				If value.IsNull Then
					Await this.RemoveAsync(key, token)
				Else
					If options IsNot Nothing Then
						Await this.SetStringAsync(key, value, options, token)
					Else
						Await this.SetStringAsync(key, value, token)
					End If
				End If
			End If
		End Function

		''' <summary>写入缓存</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Async Function SaveAsync(this As IDistributedCache, key As String, value As String, expireTime As Integer, Optional expirationMode As Boolean = False, Optional token As CancellationToken = Nothing) As Task
			If expireTime < 1 Then Return
			Await this.SaveAsync(key, value, GetOptions(expireTime, expirationMode), token)
		End Function

		''' <summary>写入缓存</summary>
		<Extension>
		Public Async Function SaveAsync(this As IDistributedCache, key As String, value As String, expireDate As Date, Optional token As CancellationToken = Nothing) As Task
			Await this.SaveAsync(key, value, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False, token)
		End Function

		'---------------------------------------------------------------------------

		''' <summary>写入缓存</summary>
		<Extension>
		Public Async Function SaveAsync(Of T)(this As IDistributedCache, key As String, value As T, Optional options As DistributedCacheEntryOptions = Nothing, Optional token As CancellationToken = Nothing) As Task
			Await this.SaveAsync(key, value?.ToJson, options, token)
		End Function

		''' <summary>写入缓存</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Async Function SaveAsync(Of T)(this As IDistributedCache, key As String, value As T, expireTime As Integer, Optional expirationMode As Boolean = False, Optional token As CancellationToken = Nothing) As Task
			If expireTime < 1 Then Return
			Await this.SaveAsync(Of T)(key, value, GetOptions(expireTime, expirationMode), token)
		End Function

		''' <summary>写入缓存</summary>
		<Extension>
		Public Async Function SaveAsync(Of T)(this As IDistributedCache, key As String, value As T, expireDate As Date, Optional token As CancellationToken = Nothing) As Task
			Await this.SaveAsync(Of T)(key, value, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False, token)
		End Function

#End Region

#Region "READ OR SAVE 读取，不存在新添加"

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Function ReadOrSave(this As IDistributedCache, key As String, nullValueAction As Func(Of String), Optional options As DistributedCacheEntryOptions = Nothing) As String
			Dim value = this.Read(key)

			If value.IsNull AndAlso nullValueAction IsNot Nothing Then
				value = nullValueAction.Invoke
				this.Save(key, value, options)
			End If

			Return value
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Function ReadOrSave(this As IDistributedCache, key As String, nullValueAction As Func(Of String), expireTime As Integer, Optional expirationMode As Boolean = False) As String
			If expireTime < 1 Then
				Return this.Read(key)
			Else
				Return this.ReadOrSave(key, nullValueAction, GetOptions(expireTime, expirationMode))
			End If
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Function ReadOrSave(this As IDistributedCache, key As String, nullValueAction As Func(Of String), expireDate As Date) As String
			Return this.ReadOrSave(key, nullValueAction, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False)
		End Function

		'---------------------------------------------------------------------------

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Async Function ReadOrSaveAsync(this As IDistributedCache, key As String, nullValueAction As Func(Of Task(Of String)), Optional options As DistributedCacheEntryOptions = Nothing, Optional token As CancellationToken = Nothing) As Task(Of String)
			Dim value = Await this.ReadAsync(key, token)

			If value.IsNull AndAlso nullValueAction IsNot Nothing Then
				value = Await nullValueAction.Invoke
				Await this.SaveAsync(key, value, options, token)
			End If

			Return value
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Async Function ReadOrSaveAsync(this As IDistributedCache, key As String, nullValueAction As Func(Of Task(Of String)), expireTime As Integer, Optional expirationMode As Boolean = False, Optional token As CancellationToken = Nothing) As Task(Of String)
			If expireTime < 1 Then
				Return Await this.ReadAsync(key, token)
			Else
				Return Await this.ReadOrSaveAsync(key, nullValueAction, GetOptions(expireTime, expirationMode), token)
			End If
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Async Function ReadOrSaveAsync(this As IDistributedCache, key As String, nullValueAction As Func(Of Task(Of String)), expireDate As Date, Optional token As CancellationToken = Nothing) As Task(Of String)
			Return Await this.ReadOrSaveAsync(key, nullValueAction, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False, token)
		End Function

		'---------------------------------------------------------------------------

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Function ReadOrSave(Of T)(this As IDistributedCache, key As String, nullValueAction As Func(Of T), Optional options As DistributedCacheEntryOptions = Nothing) As T
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
		Public Function ReadOrSave(Of T)(this As IDistributedCache, key As String, nullValueAction As Func(Of T), expireTime As Integer, Optional expirationMode As Boolean = False) As T
			If expireTime < 1 Then
				Return this.Read(Of T)(key)
			Else
				Return this.ReadOrSave(Of T)(key, nullValueAction, GetOptions(expireTime, expirationMode))
			End If
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Function ReadOrSave(Of T)(this As IDistributedCache, key As String, nullValueAction As Func(Of T), expireDate As Date) As T
			Return this.ReadOrSave(Of T)(key, nullValueAction, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False)
		End Function

		'---------------------------------------------------------------------------

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Async Function ReadOrSaveAsync(Of T)(this As IDistributedCache, key As String, nullValueAction As Func(Of Task(Of T)), Optional options As DistributedCacheEntryOptions = Nothing, Optional token As CancellationToken = Nothing) As Task(Of T)
			Dim value = Await this.ReadAsync(Of T)(key, token)

			If value Is Nothing AndAlso nullValueAction IsNot Nothing Then
				value = Await nullValueAction.Invoke
				Await this.SaveAsync(key, value, options, token)
			End If

			Return value
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		''' <param name="expireTime">超时时间（单位：秒）</param>
		''' <param name="expirationMode">缓存移除方式：True 超过指定时间未访问此缓存，则移除，一旦中间访问过则继续缓存；False：仅缓存指定时长，超过则直接移除</param>
		<Extension>
		Public Async Function ReadOrSaveAsync(Of T)(this As IDistributedCache, key As String, nullValueAction As Func(Of Task(Of T)), expireTime As Integer, Optional expirationMode As Boolean = False, Optional token As CancellationToken = Nothing) As Task(Of T)
			If expireTime < 1 Then
				Return Await this.ReadAsync(Of T)(key, token)
			Else
				Return Await this.ReadOrSaveAsync(Of T)(key, nullValueAction, GetOptions(expireTime, expirationMode), token)
			End If
		End Function

		''' <summary>缓存读取，不存在新添加</summary>
		<Extension>
		Public Async Function ReadOrSaveAsync(Of T)(this As IDistributedCache, key As String, nullValueAction As Func(Of Task(Of T)), expireDate As Date, Optional token As CancellationToken = Nothing) As Task(Of T)
			Return Await this.ReadOrSaveAsync(Of T)(key, nullValueAction, expireDate.Subtract(SYS_NOW_DATE).TotalSeconds, False, token)
		End Function

#End Region

#Region "DELETE 删除"

		''' <summary>移除缓存</summary>
		<Extension()>
		Public Sub Delete(this As IDistributedCache, key As String)
			this.Remove(CacheKey(key))
		End Sub

		''' <summary>移除缓存</summary>
		<Extension()>
		Public Async Function DeleteAsync(this As IDistributedCache, key As String, Optional token As CancellationToken = Nothing) As Task
			Await this.RemoveAsync(CacheKey(key), token)
		End Function

#End Region

	End Module
End Namespace