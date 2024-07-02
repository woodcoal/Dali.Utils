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
' 	权限管理
'
' 	name: Provider.DatabaseProvider
' 	create: 2023-02-17
' 	memo: 权限管理
'
' ------------------------------------------------------------

Imports System.Collections.Immutable
Imports System.Reflection
Imports FreeSql
Imports FreeSql.Aop
Imports FreeSql.DataAnnotations
Imports FreeSql.Internal.Model

Namespace Provider

	''' <summary>数据库驱动</summary>
	Public Class DatabaseProvider
		Implements IDatabaseProvider

		''' <summary>数据库集合</summary>
		Private _Instance As ImmutableDictionary(Of String, IFreeSql)

		''' <summary>构造数据库连接时参数</summary>
		Private ReadOnly _BuilderAction As Action(Of FreeSqlBuilder) = Nothing

		''' <summary>默认数据库连接</summary>
		Public ReadOnly [Default] As IFreeSql

		''' <summary>日志数据库连接</summary>
		Public ReadOnly Log As IFreeSql

		''' <summary>数据库设置参数</summary>
		Private ReadOnly _SettingDatabase As DatabaseSetting

		''' <summary>构造</summary>
		Public Sub New(Optional builderAction As Action(Of FreeSqlBuilder) = Nothing)
			_SettingDatabase = SYS.GetSetting(Of DatabaseSetting)

			If _SettingDatabase.Connections.IsEmpty Then Throw New Exception("未设置有效的数据库连接参数")

			' 赋值构造参数，一定要早于创建
			_BuilderAction = builderAction

			' 创建默认数据库连接
			[Default] = MakeConn("default")
			If [Default] Is Nothing Then Throw New Exception("未设置有效的默认数据库连接参数，或者默认数据库异常，无法连接")

			' 创建日志数据库连接
			Dim logConn = _SettingDatabase.Connections("log").EmptyValue("default")
			Log = MakeConn(logConn, builderAction)
			If Log Is Nothing Then Throw New Exception("未设置有效的日志数据库连接参数，或者日志数据库异常，无法连接")

			' 数据连接字典
			Dim Dic As New Dictionary(Of String, IFreeSql) From {{"default", [Default]}, {"log", Log}}
			_Instance = Dic.ToImmutableDictionary

			' 注册全局类型转换器
			' https://freesql.net/guide/type-mapping.html
			FreeSql.Internal.Utils.TypeHandlers.TryAdd(GetType(NameValueDictionary), New NameValueMap)
			FreeSql.Internal.Utils.TypeHandlers.TryAdd(GetType(KeyValueDictionary), New KeyValueMap)
		End Sub

		''' <summary>获取数据库连接对象</summary>
		Public Function GetDb(name As String) As IFreeSql Implements IDatabaseProvider.GetDb
			If name.IsEmpty Then Throw New ArgumentNullException(NameOf(name))

			name = name.ToLower

			' 检查是否已经存在，不存在创建数据库连接
			If Not _Instance.ContainsKey(name) Then
				' 获取数据库连接
				Dim Conn = MakeConn(name)
				Conn = If(Conn, [Default])

				_Instance = _Instance.Add(name, Conn)
			End If

			Return _Instance(name)
		End Function

		''' <summary>所有设置中的链接信息</summary>
		Public ReadOnly Property Connections As NameValueDictionary Implements IDatabaseProvider.Connections
			Get
				Return _SettingDatabase.Connections
			End Get
		End Property

		''' <summary>通过连接名称创建数据连接</summary>
		Private Function MakeConn(connName As String) As IFreeSql
			' 获取数据库连接
			Dim Conn = _SettingDatabase.Connections(connName)
			If Conn.NotEmpty Then
				Try
					Return MakeConn(_SettingDatabase.Connections(connName), _BuilderAction)
				Catch ex As Exception
					' 出现参数异常
				End Try
			End If

			Return Nothing
		End Function

		''' <summary>通过连接字符串初始化并创建数据库</summary>
		Public Shared Function MakeConn(conn As String, Optional action As Action(Of FreeSqlBuilder) = Nothing) As IFreeSql
			Dim Provider As IFreeSql = Nothing

			If conn.NotEmpty Then
				Dim type = ""
				Dim connection = conn
				If conn.Contains("://") Then
					Dim arr = conn.Split("://")

					type = arr(0)
					connection = arr(1)
				End If

				' 存在数据库连接字符串，创建连接
				If connection.NotEmpty Then

					' 创建数据库连接
					Dim fsb = New FreeSqlBuilder()

					Select Case GetDatabaseType(type)
						Case DatabaseTypeEnum.MYSQL
							fsb.UseConnectionString(DataType.MySql, connection)

						Case DatabaseTypeEnum.SQLITE
							fsb.UseConnectionString(DataType.Sqlite, connection)

						Case DatabaseTypeEnum.POSTGRESQL
							fsb.UseConnectionString(DataType.PostgreSQL, connection)

						Case DatabaseTypeEnum.ORACLE
							fsb.UseConnectionString(DataType.Oracle, connection)

						Case Else
							fsb.UseConnectionString(DataType.SqlServer, connection)
					End Select

					' 自动同步实体结构到数据库
					'fsb.UseAutoSyncStructure(True)
					action?.Invoke(fsb)

					' 创建连接
					Provider = fsb.Build

					' 使用 JsonMap
					Provider.UseJsonMap
					'MakeJsonMap(Provider)

					' AOP 切入，Long ID 使用雪花算法
					AddHandler Provider.Aop.AuditValue, Sub(s As Object, e As AuditValueEventArgs)
															If (e.Column.CsType Is GetType(Long) OrElse e.Column.CsType Is GetType(ULong)) AndAlso e.Value?.ToString = "0" Then
																' 存在自增则不用处理
																Dim attr = e.Property.GetCustomAttribute(Of ColumnAttribute)(True)
																If attr?.IsIdentity Then Return

																' 是否使用雪花算法
																Dim pro = e.Property.GetCustomAttribute(Of DbSnowflakeAttribute)(False)
																If pro IsNot Nothing Then
																	Dim moduleId = pro.ModuleId

																	' 如果模型标识未设置则分析类型
																	' e.Object 标识操作的 Model
																	' 其中模型标识 1xxx-32xxx 中 32 个模型生成雪花数据时使用 moduleId 字段
																	If Not moduleId.HasValue AndAlso e.Object IsNot Nothing Then
																		Dim mId = ExtendHelper.GetModuleId(e.Object.GetType)
																		If mId >= 1000 AndAlso mId < 33000 Then moduleId = Math.Floor(mId / 1000)
																	End If

																	e.Value = SnowFlakeHelper.NextID(moduleId)
																End If
															End If
														End Sub

					' 修正 TableAttribute 位于基类时，数据表名称无法获取的问题
					' freesql 默认不从基类获取表名称，如果类型从基类扩展，但是表明不变的情况下，将导致无法获取表名而使用类型名称导致查询失败
					' https://github.com/dotnetcore/FreeSql/blob/fd798c965b053cb81fb7c5209dc2a56f361698b3/FreeSql/Internal/CommonUtils.cs#L128
					AddHandler Provider.Aop.ConfigEntity, Sub(s As Object, e As ConfigEntityEventArgs)
															  Dim attr = e.EntityType.GetCustomAttribute(Of DbTableAttribute)(False)
															  If attr Is Nothing Then
																  attr = e.EntityType.GetCustomAttribute(Of DbTableAttribute)(True)
																  If attr IsNot Nothing Then
																	  e.ModifyResult.Name = attr.Name
																	  e.ModifyResult.OldName = attr.OldName
																	  e.ModifyResult.DisableSyncStructure = attr.DisableSyncStructure
																	  e.ModifyResult.AsTable = attr.AsTable
																  Else
																	  ' 无法获取有效的表名称，产生异常
																	  e.ModifyResult.DisableSyncStructure = True
																	  'Throw New Exception("无法获取有效的表名称，请检查数据操作是否异常")
																  End If
															  End If
														  End Sub

					' 字段配置
					AddHandler Provider.Aop.ConfigEntityProperty, Sub(s As Object, e As ConfigEntityPropertyEventArgs)
																	  ' MySql Enum 映射 转换成 Interger
																	  If e.Property.PropertyType.IsEnum Then e.ModifyResult.MapType = GetType(Integer)

																	  ' Map 类型参数设置 
																	  If e.Property.PropertyType = GetType(NameValueDictionary) OrElse e.Property.PropertyType = GetType(KeyValueDictionary) Then
																		  e.ModifyResult.MapType = GetType(String)
																		  e.ModifyResult.StringLength = -2
																	  End If
																  End Sub
				End If
			End If

			If Provider Is Nothing Then
				Throw New Exception("数据库连接操作无效，请检查是否设置有效的数据库连接字符串")
			Else
				' 标记软删除
				Provider.GlobalFilter.Apply(Of IEntityFlag)("SoftDelete", Function(x) x.Flag > 0)

				Return Provider
			End If
		End Function

		''' <summary>获取数据库类型</summary>
		Public Shared Function GetDatabaseType(type As String) As DatabaseTypeEnum
			If type.NotEmpty Then
				If type.Contains("mysql", StringComparison.OrdinalIgnoreCase) Then
					Return DatabaseTypeEnum.MYSQL

				ElseIf type.Contains("pgsql", StringComparison.OrdinalIgnoreCase) Then
					Return DatabaseTypeEnum.POSTGRESQL

				ElseIf type.Contains("postgresql", StringComparison.OrdinalIgnoreCase) Then
					Return DatabaseTypeEnum.POSTGRESQL

				ElseIf type.Contains("memory", StringComparison.OrdinalIgnoreCase) Then
					Return DatabaseTypeEnum.MEMORY

				ElseIf type.Contains("sqlite", StringComparison.OrdinalIgnoreCase) Then
					Return DatabaseTypeEnum.SQLITE

				ElseIf type.Contains("oracle", StringComparison.OrdinalIgnoreCase) Then
					Return DatabaseTypeEnum.ORACLE
				End If
			End If

			Return DatabaseTypeEnum.SQLSERVER
		End Function

		''' <summary>获取数据库类型</summary>
		Public Shared Function GetDatabaseType(db As IFreeSql) As DatabaseTypeEnum
			Return GetDatabaseType(db.GetType.FullName)
		End Function

		''' <summary>根据数据库类型创建 GUID</summary>
		''' <remarks>
		''' SQL Server	|	uniqueidentifier	| 	SequentialAtEnd
		''' MySQL		|	char(36)			|	SequentialAsString 
		''' Oracle		|	raw(16)				|	SequentialAsBinary 
		''' PostgreSQL	|	uuid				|	SequentialAsString 
		''' SQLite		|	varies				|	varies 
		''' </remarks>
		Public Shared Function MakeGuid(type As DatabaseTypeEnum) As Guid
			Dim mode = GuidEnum.NONE

			Select Case type
				Case DatabaseTypeEnum.SQLSERVER
					mode = GuidEnum.END_SEQUENTIAL

				Case DatabaseTypeEnum.ORACLE
					mode = GuidEnum.BINARY_SEQUENTIAL

				Case DatabaseTypeEnum.MYSQL, DatabaseTypeEnum.POSTGRESQL
					mode = GuidEnum.STRING_SEQUENTIAL
			End Select

			Return GuidHelper.Create(mode)
		End Function

		''' <summary>根据数据库类型创建 GUID</summary>
		Public Shared Function MakeGuid(db As IFreeSql) As Guid
			Return MakeGuid(GetDatabaseType(db))
		End Function

		''' <summary>NameValueDictionary 数据转换</summary>
		''' <remarks>https://freesql.net/guide/type-mapping.html</remarks>
		Public Class NameValueMap
			Inherits TypeHandler(Of NameValueDictionary)

			Public Overrides Function Deserialize(value As Object) As NameValueDictionary
				Return NameValueDictionary.FromJson(value)
			End Function

			Public Overrides Function Serialize(value As NameValueDictionary) As Object
				Return value?.ToJson()
			End Function
		End Class

		''' <summary>KeyValueDictionary 数据转换</summary>
		''' <remarks>https://freesql.net/guide/type-mapping.html</remarks>
		Public Class KeyValueMap
			Inherits TypeHandler(Of KeyValueDictionary)

			Public Overrides Function Deserialize(value As Object) As KeyValueDictionary
				Return KeyValueDictionary.FromJson(value)
			End Function

			Public Overrides Function Serialize(value As KeyValueDictionary) As Object
				Return value?.ToJson()
			End Function
		End Class
	End Class

End Namespace
