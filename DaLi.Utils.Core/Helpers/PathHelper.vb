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
' 	文件操作
'
' 	name: Helper.PathHelper
' 	create: 2020-11-15
' 	memo: 文件操作
' 	
' ------------------------------------------------------------

Imports System.Environment
Imports System.Text.Json
Imports System.Xml.Serialization

Namespace Helper

	''' <summary>文件操作</summary>
	Public NotInheritable Class PathHelper

#Region "Root"

		''' <summary>系统启动的目录</summary>
		Private Shared _Root As String

		''' <summary>系统启动的目录</summary>
		Public Shared Function Root() As String
			If _Root.IsEmpty Then
				_Root = GetType(PathHelper).Assembly.Location
				If _Root.IsEmpty Then
					'_Root = Process.GetCurrentProcess().MainModule.FileName
					_Root = ProcessPath
					If _Root.IsEmpty Then
						_Root = AppDomain.CurrentDomain.BaseDirectory
					Else
						_Root = IO.Path.GetDirectoryName(_Root)
					End If
				Else
					_Root = IO.Path.GetDirectoryName(_Root)
				End If
			End If

			Return _Root
		End Function

		''' <summary>系统特殊目录</summary>
		Public Shared Function Root(specialFolder As SpecialFolder) As String
			Return GetFolderPath(specialFolder)
		End Function

		''' <summary>自动检测并获取实际路径</summary>
		''' <param name="source"></param>
		''' <param name="tryCreate">是否尝试创建此路径的上级目录，如：d:\a\b\c True 则自动创建 d:\a\b 的目录</param>
		''' <param name="isFolder">当前获取的是目录还是文件地址，以便建立对应的目录</param>
		Public Shared Function Root(source As String, Optional tryCreate As Boolean = False, Optional isFolder As Boolean = False) As String
			Dim Ret = PathHelper.Root
			If source.IsEmpty Then Return Ret

			' 分析计算路径
			Dim sp = IO.Path.DirectorySeparatorChar
			source = source.Replace("\"c, sp).Replace("/"c, sp)

			If Ret(1) = ":"c AndAlso source.StartsWith(sp) Then
				Ret = String.Concat(Ret.AsSpan(0, 2), source)
			Else
				Ret = IO.Path.Combine(Ret, source)
			End If

			If tryCreate Then
				Dim f = If(isFolder, Ret, IO.Path.GetDirectoryName(Ret))
				Try
					If Not IO.Directory.Exists(f) Then IO.Directory.CreateDirectory(f)
				Catch ex As Exception
				End Try
			End If

			Return Ret
		End Function

#End Region

#Region "文件操作"

		''' <summary>文件是否存在，并更新路径为全路径</summary>
		''' <param name="path">指定文件</param>
		Public Shared Function FileExist(ByRef path As String) As Boolean
			If path.NotNull Then
				path = Root(path)
				Return IO.File.Exists(path)
			Else
				Return False
			End If
		End Function

		''' <summary>复制或移动文件</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Private Shared Function FileCopyOrMove(source As String, target As String, Optional isOverwrite As Boolean = True， Optional delSource As Boolean = False) As Boolean
			Dim R = False

			' 源文件存在，且目标文件与源文件不相同
			If source.NotEmpty AndAlso target.NotEmpty AndAlso Not source.Equals(target, StringComparison.OrdinalIgnoreCase) Then
				If FileExist(source) Then
					Dim exist = FileExist(target)
					If exist Then
						If isOverwrite Then
							' 尝试删除文件
							Try
								IO.File.Delete(target)

								exist = IO.File.Exists(target)
							Catch ex As Exception
							End Try
						End If
					End If

					' 如果文件不存在则可以复制
					If Not exist Then
						Try
							With New IO.DirectoryInfo(IO.Path.GetDirectoryName(target))
								If Not .Exists Then .Create()
							End With

							If delSource Then
								IO.File.Move(source, target)
							Else
								IO.File.Copy(source, target)
							End If

							R = True

							If delSource AndAlso IO.File.Exists(source) Then IO.File.Delete(source)
						Catch ex As Exception
						End Try
					End If
				End If
			End If

			Return R
		End Function

		''' <summary>复制文件</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FileCopy(source As String, target As String, Optional isOverwrite As Boolean = True) As Boolean
			Return FileCopyOrMove(source, target, isOverwrite, False)
		End Function

		''' <summary>移动文件</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FileMove(source As String, target As String, Optional isOverwrite As Boolean = True) As Boolean
			Return FileCopyOrMove(source, target, isOverwrite, True)
		End Function

		''' <summary>删除文件</summary>
		Public Shared Function FileRemove(path As String) As Boolean
			If FileExist(path) Then
				Try
					IO.File.Delete(path)
					Return True
				Catch ex As Exception
				End Try
			End If

			Return False
		End Function

		''' <summary>删除文件</summary>
		Public Shared Function FileRemove(ParamArray paths() As String) As Integer
			Return paths?.Where(Function(x) FileRemove(x)).Count
		End Function

		''' <summary>如果存在则重命名文件</summary>
		Public Shared Function FileRename(path As String) As String
			If FileExist(path) Then
				Dim I As Integer = 1
				While True
					Dim Dir As String = IO.Path.GetDirectoryName(path)
					Dim Name As String = IO.Path.GetFileNameWithoutExtension(path) & "_" & I
					Dim Ext As String = IO.Path.GetExtension(path)
					Dim File As String = IO.Path.Combine(Dir, Name & Ext)

					If Not IO.File.Exists(File) Then
						path = File
						Exit While
					End If
					I += 1
				End While
			End If

			Return path
		End Function

		''' <summary>获取指定目录的下的所有文件列表</summary>
		''' <param name="folder">指定目录</param>
		''' <param name="pattern">指定目录</param>
		''' <param name="allDirectories">是否包含子目录</param>
		''' <param name="excludes">需要排除的文件/目录，用 * 表示变化的部分，如：*xxx, xxx*, xxx*yyy, *xxx*</param>
		Public Shared Function FileList(folder As String, Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional excludes As String() = Nothing) As String()
			folder = Root(folder)
			If IO.Directory.Exists(folder) Then
				Try
					Dim Ret = IO.Directory.GetFiles(folder, pattern.EmptyValue("*"), If(allDirectories, IO.SearchOption.AllDirectories, IO.SearchOption.TopDirectoryOnly))
					If Ret?.Length > 0 AndAlso excludes?.Length > 0 Then
						Return Ret.Where(Function(x) Not x.Like(excludes)).ToArray
					Else
						Return Ret
					End If
				Catch ex As Exception
				End Try
			End If

			Return Nothing
		End Function

		''' <summary>读取文本文件</summary>
		Public Shared Function FileRead(path As String, Optional encoding As Text.Encoding = Nothing) As String
			If FileExist(path) Then
				Return IO.File.ReadAllText(path, If(encoding, Text.Encoding.Default))
			Else
				Return ""
			End If
		End Function

		''' <summary>读取数据文件</summary>
		Public Shared Function FileReadBytes(path As String) As Byte()
			If FileExist(path) Then
				Return IO.File.ReadAllBytes(path)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>保存文件</summary>
		Public Shared Function FileSave(path As String, content As String, Optional encoding As Text.Encoding = Nothing) As Boolean
			Dim R = False

			If path.NotEmpty Then
				path = Root(path, True)
				encoding = If(encoding, Text.Encoding.UTF8)

				Try
					IO.File.WriteAllText(path, content, If(encoding, Text.Encoding.Default))
					R = True
				Catch ex As Exception
				End Try
			End If

			Return R
		End Function

		''' <summary>保存文件</summary>
		Public Shared Function FileSave(path As String, contents As Byte()) As Boolean
			Dim R = False

			If path.NotEmpty Then
				path = Root(path, True)

				Try
					IO.File.WriteAllBytes(path, contents)
					R = True
				Catch ex As Exception
				End Try
			End If

			Return R
		End Function

#Region "Json 文件读写"

		''' <summary>保存为 Json 文件</summary>
		''' <param name="path">文件路径</param>
		''' <param name="jsonObject">Json 对象</param>
		Public Shared Function SaveJson(path As String, jsonObject As Object, options As JsonSerializerOptions, Optional type As Type = Nothing) As Boolean
			If path.NotEmpty Then
				If jsonObject Is Nothing Then
					Return FileSave(path, "")
				Else
					Return FileSave(path, JsonExtension.ToJson(jsonObject, options, type))
				End If
			Else
				Return False
			End If
		End Function

		''' <summary>保存为 Json 文件</summary>
		''' <param name="path">文件路径</param>
		''' <param name="jsonObject">Json 对象</param>
		Public Shared Function SaveJson(path As String, jsonObject As Object, Optional indented As Boolean = True, Optional camelCase As Boolean = False) As Boolean
			If path.NotEmpty Then
				If jsonObject Is Nothing Then
					Return FileSave(path, "")
				Else
					Return FileSave(path, JsonExtension.ToJson(jsonObject, indented, camelCase))
				End If
			Else
				Return False
			End If
		End Function

		''' <summary>保存为 Json 文件</summary>
		''' <param name="path">文件路径</param>
		''' <param name="jsonObject">Json 对象</param>
		Public Shared Function SaveJson(Of T)(path As String, jsonObject As Object, Optional indented As Boolean = True, Optional camelCase As Boolean = False) As Boolean
			If path.NotEmpty Then
				If jsonObject Is Nothing Then
					Return FileSave(path, "")
				Else
					Return FileSave(path, JsonExtension.ToJson(Of T)(jsonObject, indented, camelCase))
				End If
			Else
				Return False
			End If
		End Function

		''' <summary>读取 Json 文件</summary>
		''' <param name="path">文件路径</param>
		Public Shared Function ReadJson(path As String, Optional type As Type = Nothing, Optional options As JsonSerializerOptions = Nothing) As Object
			Return FileRead(path).ToJsonObject(type, options)
		End Function

		''' <summary>读取 Json 文件</summary>
		''' <param name="path">文件路径</param>
		Public Shared Function ReadJson(Of T)(path As String, Optional options As JsonSerializerOptions = Nothing) As T
			Return FileRead(path).ToJsonObject(Of T)(options)
		End Function

		''' <summary>读取 Json 文件为数据集合，根据JSON内容可能为字典或者列表</summary>
		''' <param name="path">文件路径</param>
		''' <param name="removeNothing">是否移除无效内容节点</param>
		Public Shared Function ReadJsonCollection(path As String, Optional removeNothing As Boolean = False) As (Value As Object, IsList As Boolean)
			Return FileRead(path).ToJsonCollection(removeNothing)
		End Function

#End Region

#Region "Xml 文件读写"

		''' <summary>序列化对象为 XML 文件</summary>
		Public Shared Function SaveXml(Of T)(path As String, objXml As T, Optional isCompression As Boolean = False) As Boolean
			Dim Ret = False

			If path.NotEmpty Then
				If objXml Is Nothing Then
					Ret = PathHelper.FileSave(path, "")
				Else
					path = PathHelper.Root(path, True)
					Using fs As IO.FileStream = IO.File.Create(path)
						If isCompression Then
							Using Zip As New IO.Compression.GZipStream(fs, IO.Compression.CompressionMode.Compress)
								Ret = SaveXml(Of T)(Zip, objXml)
							End Using
						Else
							Ret = SaveXml(Of T)(fs, objXml)
						End If
					End Using
				End If
			End If

			Return Ret
		End Function

		''' <summary>序列化对象为 XML 文件</summary>
		Private Shared Function SaveXml(Of T)(fileStream As IO.Stream, obj As T) As Boolean
			Dim R = False

			Try
				Using XmlWriter As New IO.StreamWriter(fileStream)
					Call New XmlSerializer(GetType(T)).Serialize(XmlWriter, obj)
				End Using

				R = True
			Catch ex As Exception
			End Try

			Return R
		End Function

		'''<summary>读取 Xml 文件，反序列化对象</summary>
		''' <typeparam name="T"></typeparam>
		''' <param name="path"></param>
		''' <param name="hasCompression"></param>
		''' <returns></returns>
		Public Shared Function ReadXml(Of T)(path As String, Optional hasCompression As Boolean = False) As T
			Dim Ret As T = Nothing

			If FileExist(path) Then
				If IO.File.Exists(path) Then
					Try
						Using fs As IO.FileStream = IO.File.OpenRead(path)
							If hasCompression Then
								Using Zip As New IO.Compression.GZipStream(fs, IO.Compression.CompressionMode.Decompress)
									Ret = ReadXml(Of T)(Zip)
								End Using
							Else
								Ret = ReadXml(Of T)(fs)
							End If
						End Using
					Catch ex As Exception
					End Try
				End If
			End If

			Return Ret
		End Function

		''' <summary>XML 文件反序列为对象</summary>
		Private Shared Function ReadXml(Of T)(fileStream As IO.Stream) As T
			Dim Value As T = Nothing

			Try
				Using XmlReader As New IO.StreamReader(fileStream)
					Dim mSerializer As New XmlSerializer(GetType(T))
					Value = CType(mSerializer.Deserialize(XmlReader), T)
				End Using
			Catch ex As Exception
			End Try

			Return Value
		End Function

#End Region



#End Region

#Region "目录操作"

		''' <summary>目录是否存在，并更新路径为全路径</summary>
		''' <param name="path">指定目录</param>
		Public Shared Function FolderExist(ByRef path As String, Optional tryCreate As Boolean = False) As Boolean
			If path.NotNull Then
				path = Root(path, tryCreate, True)
				Return IO.Directory.Exists(path)
			Else
				Return False
			End If
		End Function

		''' <summary>创建文件所在的目录，如果不存在则创建</summary>
		Public Shared Sub FolderCreate(ByRef path As String, isFolder As Boolean)
			path = Root(path, True, isFolder)
		End Sub

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FolderCopy(source As String, target As String, Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional isOverwrite As Boolean = True) As Integer
			Return FolderCopyOrMove(source, target, pattern, allDirectories, isOverwrite, False)
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FolderCopy(source As String, target As String, disFiles As String(), Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional isOverwrite As Boolean = True) As Integer
			Return FolderCopyOrMove(source, target, disFiles, pattern, allDirectories, isOverwrite, False)
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="Target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FolderCopy(source As String, target As String, disExts As String(), Optional isOverwrite As Boolean = True) As Integer
			Return FolderCopyOrMove(source, target, disExts, isOverwrite, False)
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FolderMove(source As String, target As String, Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional isOverwrite As Boolean = True) As Integer
			Return FolderCopyOrMove(source, target, pattern, allDirectories, isOverwrite, True)
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FolderMove(source As String, target As String, disFiles As String(), Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional isOverwrite As Boolean = True) As Integer
			Return FolderCopyOrMove(source, target, disFiles, pattern, allDirectories, isOverwrite, True)
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="Target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Public Shared Function FolderMove(source As String, target As String, disExts As String(), Optional isOverwrite As Boolean = True) As Integer
			Return FolderCopyOrMove(source, target, disExts, isOverwrite, True)
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="Target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Private Shared Function FolderCopyOrMove(source As String, target As String, Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional isOverwrite As Boolean = True， Optional delSource As Boolean = False) As Integer
			Dim R = 0

			If Not String.IsNullOrWhiteSpace(source) AndAlso Not String.IsNullOrWhiteSpace(target) AndAlso Not target.StartsWith(source, StringComparison.OrdinalIgnoreCase） Then
				source = Root(source)
				target = Root(target)

				If IO.File.Exists(source) Then
					Dim SFs = FileList(source, pattern, allDirectories)
					If SFs?.Length > 0 Then
						Dim sLen = source.Length + 1

						For Each SF In SFs
							Dim TF = IO.Path.Combine(target, SF.Substring(sLen))
							If FileCopyOrMove(SF, TF, isOverwrite, delSource) Then R += 1
						Next
					End If
				End If
			End If

			Return R
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="Target">新路径</param>
		''' <param name="Excludes">不能包含内容</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Private Shared Function FolderCopyOrMove(source As String, target As String, excludes As String(), Optional pattern As String = "", Optional allDirectories As Boolean = True, Optional isOverwrite As Boolean = True， Optional delSource As Boolean = False) As Integer
			Dim R = 0

			If Not String.IsNullOrWhiteSpace(source) AndAlso Not String.IsNullOrWhiteSpace(target) AndAlso Not target.StartsWith(source, StringComparison.OrdinalIgnoreCase） Then
				source = Root(source)
				target = Root(target)

				If IO.File.Exists(source) Then
					Dim SFs = FileList(source, pattern, allDirectories, excludes)
					If SFs?.Length > 0 Then
						Dim sLen = source.Length + 1

						For Each SF In SFs
							Dim TF = IO.Path.Combine(target, SF.Substring(sLen))
							If FileCopyOrMove(SF, TF, isOverwrite, delSource) Then R += 1
						Next
					End If
				End If
			End If

			Return R
		End Function

		''' <summary>复制或移动目录</summary>
		''' <param name="source">原路径</param>
		''' <param name="Target">新路径</param>
		''' <param name="isOverwrite">如果存在是否覆盖</param>
		Private Shared Function FolderCopyOrMove(source As String, target As String, excludes As String(), Optional isOverwrite As Boolean = True， Optional delSource As Boolean = False) As Integer
			Dim R = 0

			If Not String.IsNullOrWhiteSpace(source) AndAlso Not String.IsNullOrWhiteSpace(target) AndAlso Not target.StartsWith(source, StringComparison.OrdinalIgnoreCase） Then
				source = Root(source)
				target = Root(target)

				If IO.File.Exists(source) Then
					Dim SFs = FileList(source,,, excludes)
					If SFs?.Length > 0 Then
						Dim sLen = source.Length + 1

						For Each SF In SFs
							Dim TF = IO.Path.Combine(target, SF.Substring(sLen))
							If FileCopyOrMove(SF, TF, isOverwrite, delSource) Then R += 1
						Next
					End If
				End If
			End If

			Return R
		End Function

		''' <summary>删除文件</summary>
		Public Shared Function FolderRemove(path As String, Optional pattern As String = "", Optional allDirectories As Boolean = True) As Boolean
			Dim R = False

			If FolderExist(path) Then
				Try
					If String.IsNullOrWhiteSpace(pattern) AndAlso allDirectories Then
						IO.Directory.Delete(path, True)
						R = True
					Else
						R = FileRemove(FileList(path, pattern, allDirectories)) > 0
					End If
				Catch ex As Exception
				End Try
			End If

			Return R
		End Function

		''' <summary>删除文件</summary>
		Public Shared Function FolderRemove(source As String, excludes As String(), Optional pattern As String = "", Optional allDirectories As Boolean = True) As Integer
			Return FileRemove(FileList(source, pattern, allDirectories, excludes))
		End Function

		''' <summary>删除文件</summary>
		Public Shared Function FolderRemove(source As String, ParamArray excludes As String()) As Integer
			Return FileRemove(FileList(source,,, excludes))
		End Function

		''' <summary>如果存在则重命名目录</summary>
		Public Shared Function FolderRename(path As String) As String
			If FolderExist(path) Then
				If path.EndsWith("\") Or path.EndsWith("/") Then path = path.Substring(0, path.Length - 1)

				Dim I As Integer = 1
				While True
					Dim Dir As String = path & "_" & I
					If Not IO.Directory.Exists(Dir) Then
						path = Dir
						Exit While
					End If
					I += 1
				End While
			End If

			Return path
		End Function
#End Region

#Region "其他操作"

		''' <summary>获取目录/文件最后一级名称，文件含扩展名</summary>
		Public Shared Function GetName(path As String) As String
			If path.IsEmpty Then Return ""

			Dim arr = path.Split({"/"c, "\"c}, StringSplitOptions.RemoveEmptyEntries Or StringSplitOptions.TrimEntries)
			If arr.IsEmpty Then Return ""

			Return arr.Last
		End Function
#End Region
	End Class

End Namespace