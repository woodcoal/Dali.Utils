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
' 	数据模型基类
'
' 	name: Base.EntityBase
' 	create: 2023-02-18
' 	memo: 数据模型基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Text.Json
Imports FreeSql

Namespace Base

	''' <summary>数据模型基类</summary>
	Public MustInherit Class EntityBase
		Implements IEntity

		''' <summary>编号</summary>
		<DbSnowflake>
		<DbColumn(IsPrimary:=True, IsIdentity:=False)>
		Public Overridable Property ID As Long Implements IEntity.ID

		''' <summary>文本标识</summary>
		Public ReadOnly Property _ID_ As String Implements IEntity.ID_
			Get
				Return ID.ToString
			End Get
		End Property

		''' <summary>克隆</summary>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return MemberwiseClone()
		End Function

		''' <summary>获取本模块的标识</summary>
		Public Function GetModuleId() As UInteger
			Return ExtendHelper.GetModuleId([GetType])
		End Function

#Region "附加参数处理"

		''' <summary>扩展属性，仅用于传递数据，不存入数据库</summary>
		<NotMapped>
		<Output(TristateEnum.FALSE)>
		Public Property Ext As Object

		''' <summary>尝试将扩展数据转换对象</summary>
		Public Function TryExtObject() As Object
			If Ext?.GetType = GetType(JsonElement) Then
				Return JsonElementParse(Ext, True)
			Else
				Return Ext
			End If
		End Function

		''' <summary>尝试将扩展数据转换成数组，如果是字符串则使用都好分割获取</summary>
		Public Function TryExtArray() As String()
			Dim data = TryExtObject()
			If data Is Nothing Then Return Nothing

			If data.GetType.IsString Then
				' 如果未包含引号，直接分解未数组，否则使用 Json 解析
				Dim sExt = data.ToString
				If sExt.NotEmpty Then
					If sExt.Contains(""""c) Then
						Return sExt.ToJsonObject(Of String())
					Else
						Return sExt.SplitEx
					End If
				End If
			ElseIf data.GetType.IsEnumerable Then
				Dim list = TryCast(data, IEnumerable(Of Object))
				If list IsNot Nothing Then Return list.Select(Function(x) x.ToString).Distinct.ToArray
			End If

			Return Nothing
		End Function

		''' <summary>尝试将扩展数据转换成标识列表，如果是字符串则使用都好分割获取</summary>
		Public Function TryExtIds() As Long()
			Return TryExtArray()?.Select(Function(x) x.ToLong).Where(Function(x) x.NotEmpty).Distinct.ToArray
		End Function

#End Region

	End Class

End Namespace