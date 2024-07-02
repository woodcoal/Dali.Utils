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
' 	类型相关扩展操作
'
' 	name: Extension.TypeExtension
' 	create: 2020-11-10
' 	memo: 类型相关扩展操作
' 	
' ------------------------------------------------------------

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>类型相关扩展操作</summary>
	Public Module TypeExtension

		''' <summary>执行操作并返回指定类型</summary>
		<Extension>
		Public Function ExecuteResult(Of T)(this As Func(Of T), Optional ByRef errorMessage As String = "") As T
			errorMessage = "无效操作"
			If this Is Nothing Then Return Nothing

			Try
				errorMessage = ""
				Return this.Invoke()
			Catch ex As Exception
				errorMessage = ex.Message
				Return Nothing
			End Try
		End Function

#Region "类型判断"

		''' <summary>是否来自泛类</summary>
		''' <param name="this">当前类型</param>
		''' <param name="innerType">泛类</param>
		<Extension>
		Public Function IsGeneric(this As Type, innerType As Type) As Boolean
			Return this IsNot Nothing AndAlso innerType IsNot Nothing AndAlso this.GetTypeInfo().IsGenericType AndAlso this.GetGenericTypeDefinition() = innerType
		End Function

		''' <summary>判断是否为 Nullable 类型</summary>
		<Extension>
		Public Function IsNullable(this As Type) As Boolean
			Return IsGeneric(this, GetType(Nullable(Of)))
		End Function

		''' <summary>判断是否为 IEnumerable(of ) 类型</summary>
		<Extension>
		Public Function IsList(this As Type) As Boolean
			Return IsGeneric(this, GetType(IEnumerable(Of)))
		End Function

		''' <summary>判断是否为 IEnumerable(of T) 类型</summary>
		<Extension>
		Public Function IsList(Of T)(this As Type) As Boolean
			Return GetType(IEnumerable(Of T)).IsAssignableFrom(this)
		End Function

		''' <summary>判断是否为 IEnumerable 类型</summary>
		<Extension>
		Public Function IsEnumerable(this As Type) As Boolean
			Return GetType(IEnumerable).IsAssignableFrom(this) AndAlso Not this.IsString
		End Function

		''' <summary>判断是否为 IDictionary 类型，注意 IEnumerable 包含 IDictionary，IEnumerable 是 IDictionary 基类</summary>
		<Extension>
		Public Function IsDictionary(this As Type) As Boolean
			Return GetType(IDictionary).IsAssignableFrom(this)
		End Function

		''' <summary>判断是否为 IDictionary 类型</summary>
		<Extension>
		Public Function IsDictionary(Of Tkey, TValue)(this As Type) As Boolean
			Return GetType(IDictionary(Of Tkey, TValue)).IsAssignableFrom(this)
		End Function

		''' <summary>判断是否为值类型</summary>
		<Extension>
		Public Function IsPrimitive(this As Type) As Boolean
			Return this.IsPrimitive OrElse this = GetType(Decimal)
		End Function

		''' <summary>判断是否为枚举或者可空枚举</summary>
		<Extension>
		Public Function IsNullableEnum(this As Type) As Boolean
			Return this.IsEnum OrElse (this.IsNullable AndAlso this.GetGenericArguments?(0).IsEnum)
		End Function

		''' <summary>判断是否为数字类型</summary>
		<Extension>
		Public Function IsNumber(this As Type) As Boolean
			If this IsNot Nothing Then
				Select Case Type.GetTypeCode(this)
					Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
						Return True
				End Select
			End If

			Return False
		End Function

		''' <summary>判断是否为数字类型或为空数字类型</summary>
		<Extension>
		Public Function IsNullableNumber(this As Type) As Boolean
			Return this.IsNumber OrElse (this.IsNullable AndAlso this.GetGenericArguments?(0).IsNumber)
		End Function

		''' <summary>判断是否为 Boolean 类型</summary>
		<Extension>
		Public Function IsGuid(this As Type) As Boolean
			Return this IsNot Nothing AndAlso this = GetType(Guid)
		End Function

		''' <summary>判断是否为 Boolean 或者 Boolean? 类型</summary>
		<Extension>
		Public Function IsNullableGuid(this As Type) As Boolean
			Return this IsNot Nothing AndAlso (this = GetType(Guid) OrElse this = GetType(Guid?))
		End Function

		''' <summary>判断是否为 Integer 类型</summary>
		<Extension>
		Public Function IsInteger(this As Type) As Boolean
			Return this IsNot Nothing AndAlso this = GetType(Integer)
		End Function

		''' <summary>判断是否为 Integer 或者 Integer? 类型</summary>
		<Extension>
		Public Function IsNullableInteger(this As Type) As Boolean
			Return this IsNot Nothing AndAlso (this = GetType(Integer) OrElse this = GetType(Integer?))
		End Function

		''' <summary>判断是否为 Long 类型</summary>
		<Extension>
		Public Function IsLong(this As Type) As Boolean
			Return this IsNot Nothing AndAlso this = GetType(Long)
		End Function

		''' <summary>判断是否为 Long 或者 Long? 类型</summary>
		<Extension>
		Public Function IsNullableLong(this As Type) As Boolean
			Return this IsNot Nothing AndAlso (this = GetType(Long) OrElse this = GetType(Long?))
		End Function

		''' <summary>判断是否为 Boolean 类型</summary>
		<Extension>
		Public Function IsBoolean(this As Type) As Boolean
			Return this IsNot Nothing AndAlso this = GetType(Boolean)
		End Function

		''' <summary>判断是否为 Boolean 或者 Boolean? 类型</summary>
		<Extension>
		Public Function IsNullableBoolean(this As Type) As Boolean
			Return this IsNot Nothing AndAlso (this = GetType(Boolean) OrElse this = GetType(Boolean?))
		End Function

		''' <summary>判断是否为时间类型</summary>
		<Extension>
		Public Function IsDate(this As Type) As Boolean
			Return this IsNot Nothing AndAlso this = GetType(Date)
		End Function

		''' <summary>判断是否为 Date 或者 Date? 类型</summary>
		<Extension>
		Public Function IsNullableDate(this As Type) As Boolean
			'Return this.IsDate OrElse (this.IsNullable AndAlso this.GetGenericArguments?(0).IsDate)
			Return this IsNot Nothing AndAlso (this = GetType(Date) OrElse this = GetType(Date?))
		End Function

		''' <summary>判断是否为字符类型</summary>
		<Extension>
		Public Function IsString(this As Type) As Boolean
			Return this = GetType(String)
		End Function


		'''' <summary>判断是否为函数类型</summary>
		'<Extension>
		'Public Function IsFunction(this As Type) As Boolean
		'	Return GetType(Delegate).IsAssignableFrom(this)
		'End Function

		''' <summary>判断是自定义类型，非系统内置类型</summary>
		<Extension>
		Public Function IsExtendClass(this As Type) As Boolean
			Return this.Assembly <> GetType(String).Assembly AndAlso
				this.IsClass AndAlso
				Not this.IsValueType AndAlso
				Not this.IsEnum AndAlso
				Not this.IsInterface AndAlso
				Not this.IsAbstract
		End Function

		''' <summary>判断是否某个类型的基类</summary>
		''' <param name="this">当前类型</param>
		''' <param name="baseType">要验证的基类</param>
		''' <param name="enabledEquals">是否允许当前类型与要验证的基类相同</param>
		''' <param name="enabledAbstract">是否允许当前类型为基类</param>
		<Extension>
		Public Function IsComeFrom(this As Type, baseType As Type, Optional enabledEquals As Boolean = True, Optional enabledAbstract As Boolean = False) As Boolean
			If baseType Is Nothing OrElse this Is Nothing OrElse (Not enabledAbstract AndAlso this.IsAbstract) Then Return False

			' 两类型相同
			If this Is baseType Then Return enabledEquals

			' 验证是否从属
			Return baseType.IsAssignableFrom(this)
		End Function

		''' <summary>判断是否某个类型的基类</summary>
		''' <param name="this">当前类型</param>
		''' <param name="enabledEquals">是否允许当前类型与要验证的基类相同</param>
		''' <param name="enabledAbstract">是否允许当前类型为基类</param>
		''' <typeparam name="T">要验证的基类</typeparam>
		<Extension>
		Public Function IsComeFrom(Of T)(this As Type, Optional enabledEquals As Boolean = True, Optional enabledAbstract As Boolean = False) As Boolean
			Return this.IsComeFrom(GetType(T), enabledEquals, enabledAbstract)
		End Function

#End Region

#Region "转换"

		''' <summary>获取对象名称</summary>
		''' <param name="this">类型</param>
		<Extension>
		Public Function GetTypeName(this As Object) As String
			Return this?.GetType.Name
		End Function

		''' <summary>获取对象类型</summary>
		''' <param name="this">类型</param>
		''' <remarks>
		''' Empty = 0		Object = 1		DBNull = 2
		''' Boolean = 3		Char = 4
		''' SByte = 5		Byte = 6		Int16 = 7		UInt16 = 8		Int32 = 9		UInt32 = 10
		''' Int64 = 11		UInt64 = 12		Single = 13		Double = 14		Decimal = 15
		''' DateTime = 16	String = 18
		''' </remarks>
		<Extension>
		Public Function GetTypeCode(this As Object) As TypeCode
			Return Type.GetTypeCode(this?.GetType)
		End Function


		''' <summary>获取对象类型</summary>
		''' <param name="this">类型</param>
		''' <remarks>
		''' Empty = 0		Object = 1		DBNull = 2
		''' Boolean = 3		Char = 4
		''' SByte = 5		Byte = 6		Int16 = 7		UInt16 = 8		Int32 = 9		UInt32 = 10
		''' Int64 = 11		UInt64 = 12		Single = 13		Double = 14		Decimal = 15
		''' DateTime = 16	String = 18
		''' </remarks>
		<Extension>
		Public Function GetTypeCode(this As Type) As TypeCode
			Return Type.GetTypeCode(this)
		End Function

		''' <summary>生成对象，仅返回系统常用的类型；异常返回字符类型</summary>
		''' <param name="this">类型</param>
		<Extension>
		Public Function MakeType(this As TypeCode) As Type
			Select Case this
				Case TypeCode.Empty
					Return Nothing

				Case TypeCode.Object
					Return GetType(Object)

				Case TypeCode.DBNull
					Return GetType(DBNull)

				Case TypeCode.Boolean
					Return GetType(Boolean)

				Case TypeCode.Char
					Return GetType(Char)

				Case TypeCode.SByte
					Return GetType(SByte)

				Case TypeCode.Byte
					Return GetType(Byte)

				Case TypeCode.Int16
					Return GetType(Short)

				Case TypeCode.UInt16
					Return GetType(UShort)

				Case TypeCode.Int32
					Return GetType(Integer)

				Case TypeCode.UInt32
					Return GetType(UInteger)

				Case TypeCode.Int64
					Return GetType(Long)

				Case TypeCode.UInt64
					Return GetType(ULong)

				Case TypeCode.Single
					Return GetType(Single)

				Case TypeCode.Double
					Return GetType(Double)

				Case TypeCode.Decimal
					Return GetType(Decimal)

				Case TypeCode.DateTime
					Return GetType(Date)

				Case TypeCode.String
					Return GetType(String)
			End Select

			Return Nothing
		End Function

		''' <summary>生成对象，仅返回系统常用的类型；异常返回字符类型</summary>
		''' <param name="this">类型</param>
		<Extension>
		Public Function MakeType(this As String) As Type
			If this.NotEmpty Then
				If Not this.Contains("."c) Then this = "System." & this
				Return Type.GetType(this, False, True)
			End If

			Return Nothing
		End Function

		''' <summary>获取常用系统数据类型的默认值</summary>
		''' <param name="this">类型</param>
		<Extension>
		Public Function DefaultValue(this As TypeCode) As Object
			Select Case this

				Case TypeCode.Boolean
					Return False

				Case TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
					Return 0

				Case TypeCode.DateTime
					Return New Date

				Case TypeCode.String
					Return ""
			End Select

			Return Nothing
		End Function

		''' <summary>获取常用系统数据类型的默认值</summary>
		''' <param name="this">类型</param>
		<Extension>
		Public Function DefaultValue(this As Type) As Object
			If this.IsBoolean Then
				Return False

			ElseIf this.IsNumber OrElse this.IsEnum Then
				Return 0

			ElseIf this.IsDate Then
				Return New Date

			ElseIf this = GetType(TimeSpan) Then
				Return New TimeSpan

			ElseIf this = GetType(IntPtr) Then
				Return New IntPtr

			ElseIf this = GetType(UIntPtr) Then
				Return New UIntPtr

			ElseIf this = GetType(Guid) Then
				Return New Guid

			ElseIf this = GetType(TristateEnum) Then
				Return TristateEnum.DEFAULT

			ElseIf this.IsString Then
				Return ""

			Else
				Return Nothing
			End If
		End Function

		''' <summary>将系统类数据转换成数据类型</summary>
		''' <param name="this">要转换的原始字符串</param>
		''' <param name="code">数据类型</param>
		<Extension>
		Public Function ToValue(this As String, code As TypeCode) As Object
			Select Case code
				Case TypeCode.Boolean
					Return this.ToBoolean

				Case TypeCode.Char
					Return this.ToChar

				Case TypeCode.SByte
					Return this.ToSByte

				Case TypeCode.Byte
					Return this.ToByte

				Case TypeCode.Int16
					Return this.ToShort

				Case TypeCode.UInt16
					Return this.ToUShort

				Case TypeCode.Int32
					Return this.ToInteger

				Case TypeCode.UInt32
					Return this.ToUInteger

				Case TypeCode.Int64
					Return this.ToLong

				Case TypeCode.UInt64
					Return this.ToULong

				Case TypeCode.Single
					Return this.ToSingle

				Case TypeCode.Double
					Return this.ToDouble

				Case TypeCode.Decimal
					Return this.ToNumber

				Case TypeCode.DateTime
					Return this.ToDateTime

				Case TypeCode.String
					Return this

					'Case TypeCode.Empty, TypeCode.DBNull
					'	Return Nothing

					'Case TypeCode.Object
					'	Return Nothing
			End Select

			Return Nothing
		End Function

		''' <summary>将系统类数据转换成数据类型</summary>
		''' <param name="this">要转换的原始字符串</param>
		''' <param name="type">数据类型</param>
		<Extension>
		Public Function ToValue(this As String, type As Type) As Object
			If type Is Nothing Then Return Nothing

			If type.IsEnum Then Return this.ToInteger

			Dim Code = Type.GetTypeCode(type)
			If Code <> TypeCode.Object Then Return this.ToValue(Code)

			Select Case type
				Case GetType(Guid)
					Return this.ToGuid

				Case GetType(Uri)
					Return New Uri(this.ToUrl)

				Case GetType(TimeSpan)
					Return New TimeSpan(this.ToLong)

				Case GetType(TristateEnum)
					Return this.ToTriState

				Case GetType(Integer())
					Return this.ToIntegerList

				Case GetType(Long())
					Return this.ToLongList

				Case GetType(Guid())
					Return this.ToGuidList

				Case GetType(Date())
					Return this.ToDateList?.ToArray

				Case GetType(String())
					Return this.SplitEx

				Case GetType(List(Of String))
					Return this.SplitEx(vbCrLf)

				Case GetType(System.Enum)
					Return this.ToInteger

				Case Else
					' 其他类型暂时无法转换，可以尝试序列化
					Return this.ToJsonObject(type)

			End Select
		End Function

		''' <summary>将系统类数据转换成数据类型</summary>
		''' <param name="this">要转换的原始字符串</param>
		''' <param name="type">数据类型</param>
		<Extension>
		Public Function ToValue(this As String, type As String) As Object
			Return this.ToValue(type.MakeType)
		End Function

		''' <summary>将系统类数据转换成数据类型</summary>
		''' <param name="this">要转换的原始字符串</param>
		<Extension>
		Public Function ToValue(Of T)(this As String) As T
			Try
				Return this.ToValue(GetType(T))
			Catch ex As Exception
				Return Nothing
			End Try
		End Function

		''' <summary>将对象转成字符串</summary>
		''' <param name="this">要转换的对象</param>
		<Extension>
		Public Function ToObjectString(this As Object) As String
			If this Is Nothing Then Return ""

			Dim thisType = this.GetType
			Dim thisCode = thisType.GetTypeCode

			If thisType.IsEnum Then
				' 枚举
				Return CInt(this)

			ElseIf thisCode = TypeCode.DateTime Then
				' 处理时间
				Dim v As Date = this
				Return v.ToString("yyyy-MM-dd HH:mm:ss").Replace(" 00:00:00", "")

			ElseIf thisCode = TypeCode.Object Then
				' 处理对象
				Select Case thisType
					Case GetType(String)
						Return this

					Case GetType(Guid)
						Return this.ToString

					Case GetType(Uri)
						Dim v = ChangeType(Of Uri)(this)
						Return v.OriginalString

					Case GetType(TimeSpan)
						Dim v = ChangeType(Of TimeSpan)(this)
						Return v.Ticks

					Case GetType(Integer())
						Dim v = ChangeType(Of Integer())(this)
						Return v.ToNumberString

					Case GetType(Long())
						Dim v = ChangeType(Of Long())(this)
						Return v.ToNumberString

					Case GetType(Guid())
						Dim v = ChangeType(Of Guid())(this)
						Return If(v.IsEmpty, "", String.Join(",", v))

					Case GetType(Date())
						Dim v = ChangeType(Of Date())(this)
						Return v?.Select(Function(x) x.ToString("yyyy-MM-dd HH:mm:ss")).JoinString?.Replace(" 00:00:00", "")

					Case GetType(String())
						Dim v = ChangeType(Of String())(this)
						Return If(v.IsEmpty, "", String.Join(",", v))

					Case GetType(List(Of String))
						Dim v = ChangeType(Of List(Of String))(this)
						Return If(v.IsEmpty, "", String.Join(vbCrLf, v))

					Case GetType(System.Enum)
						Return CInt(this)

					Case Else
						Return ToJson(this, False, False, False)
				End Select
			End If

			Return this.ToString
		End Function

		''' <summary>将对象加密字符串</summary>
		''' <param name="this">要转换的对象</param>
		''' <param name="key">用于加密的密钥</param>
		''' <remarks>先转换成 JSON ，然后 DES 加密，请确保数据可以被 JSON 序列化</remarks>
		<Extension>
		Public Function ToEncodeString(Of T)(this As T, Optional key As String = "19491001") As String
			If this Is Nothing Then Return ""

			Dim str = this.ToJson(False, False)
			If str.IsEmpty Then Return ""

			str = this.GetType.FullName + "|" + str
			Return New SecurityHelper.Des().Encrypt(str, key)
		End Function

		''' <summary>将字符串解密为对象</summary>
		''' <param name="this">要转换的字符串</param>
		''' <param name="key">用于解密的密钥</param>
		''' <remarks>先 DES 解密 后通过 JSON 反序列化，请确保数据原始类型可以被 JSON 序列化</remarks>
		<Extension>
		Public Function ToDecodeObject(this As String, Optional key As String = "19491001") As Object
			If this.IsEmpty Then Return Nothing

			Dim str = New SecurityHelper.Des().Decrypt(this, key)
			If str.IsEmpty OrElse Not str.Contains("|"c) Then Return Nothing

			Dim idx = str.IndexOf("|")
			Dim type = System.Type.GetType(str.Substring(0, idx), False, True)
			If type Is Nothing Then Return Nothing

			str = str.Substring(idx + 1)
			If str.IsEmpty Then Return Nothing

			Return str.ToJsonObject(type)
		End Function

#End Region

#Region "自定义字段类型 FieldType"

		''' <summary>获取常用系统数据类型的默认值</summary>
		<Extension>
		Public Function DefaultValue(this As FieldTypeEnum) As Object
			Select Case this
				Case FieldTypeEnum.BOOLEAN
					Return False

				Case FieldTypeEnum.TRISTATE
					Return TristateEnum.DEFAULT

				Case FieldTypeEnum.BYTES, FieldTypeEnum.DOUBLE, FieldTypeEnum.INTEGER, FieldTypeEnum.LONG, FieldTypeEnum.NUMBER, FieldTypeEnum.SINGLE
					Return 0

				Case FieldTypeEnum.DATETIME
					Return New Date

				Case FieldTypeEnum.TIME
					Return New Date.ToString("HH:mm:ss")

				Case FieldTypeEnum.DATE
					Return New Date.ToString("yyyy-MM-dd")

				Case FieldTypeEnum.GUID
					Return Guid.Empty

				Case Else
					Return ""
			End Select
		End Function

		''' <summary>获取常用系统数据类型空值数组</summary>
		<Extension>
		Public Function DefaultArrayValue(this As FieldTypeEnum) As Array
			Select Case this
				Case FieldTypeEnum.BOOLEAN
					Return Array.Empty(Of Boolean)

				Case FieldTypeEnum.TRISTATE
					Return Array.Empty(Of TristateEnum)

				Case FieldTypeEnum.BYTES
					Return Array.Empty(Of Byte)

				Case FieldTypeEnum.DOUBLE
					Return Array.Empty(Of Double)

				Case FieldTypeEnum.INTEGER
					Return Array.Empty(Of Integer)

				Case FieldTypeEnum.LONG
					Return Array.Empty(Of Long)

				Case FieldTypeEnum.NUMBER
					Return Array.Empty(Of Decimal)

				Case FieldTypeEnum.SINGLE
					Return Array.Empty(Of Single)

				Case FieldTypeEnum.DATETIME
					Return Array.Empty(Of Date)

				Case FieldTypeEnum.GUID
					Return Array.Empty(Of Guid)

				Case Else
					Return Array.Empty(Of String)
			End Select
		End Function

		''' <summary>根据指定格式数据转换成文本，异常则返回默认值</summary>
		''' <param name="this">要转换的对象</param>
		''' <param name="type">数据类型</param>
		''' <param name="validate">用于校验数据是否合法的过程，比如字符长度，数字大小等</param>
		<Extension>
		Public Function GetString(this As Object, type As FieldTypeEnum, Optional validate As Func(Of Object, Boolean) = Nothing) As String
			Dim Ret = ""

			If this IsNot Nothing AndAlso validate IsNot Nothing Then
				If Not validate(this) Then this = Nothing
			End If

			If this IsNot Nothing Then
				Select Case type
					Case FieldTypeEnum.ASCII
						Ret = this.ToString.GetAscii

					Case FieldTypeEnum.CHINESE
						Ret = this.ToString.GetChinese

					Case FieldTypeEnum.LOWER_CASE
						Ret = this.ToString.ToLower

					Case FieldTypeEnum.UPPER_CASE
						Ret = this.ToString.ToUpper

					Case FieldTypeEnum.EMAIL, FieldTypeEnum.URL, FieldTypeEnum.MOBILEPHONE, FieldTypeEnum.GUID
						Ret = this.ToString

						If Ret.NotEmpty Then
							If type = FieldTypeEnum.EMAIL Then
								If Not Ret.IsEmail Then Ret = ""

							ElseIf type = FieldTypeEnum.URL Then
								If Not Ret.IsUrl Then Ret = ""

							ElseIf type = FieldTypeEnum.MOBILEPHONE Then
								If Not Ret.IsMobilePhone Then Ret = ""

							Else
								If Not Ret.IsGUID Then Ret = ""
							End If
						Else
							Ret = ""
						End If

					Case FieldTypeEnum.NUMBER, FieldTypeEnum.INTEGER, FieldTypeEnum.LONG, FieldTypeEnum.SINGLE, FieldTypeEnum.DOUBLE， FieldTypeEnum.BYTES
						Ret = this.ToString.ToNumber

					Case FieldTypeEnum.DATETIME
						Try
							Ret = Date.Parse(this.ToString).ToString("yyyy-MM-dd HH:mm:ss")
						Catch ex As Exception
						End Try

					Case FieldTypeEnum.DATE
						Try
							Ret = Date.Parse(this.ToString).ToString("yyyy-MM-dd")
						Catch ex As Exception
						End Try

					Case FieldTypeEnum.TIME
						Try
							Ret = Date.Parse(this.ToString).ToString("HH:mm:ss")
						Catch ex As Exception
						End Try

					Case FieldTypeEnum.JSON
						Ret = ToJson(this, False, False, False)

					Case FieldTypeEnum.XML
						Ret = this.ToString.ToXML

					Case Else
						Ret = this.ToString
				End Select
			End If

			Return Ret
		End Function

		''' <summary>根据指定数组、列表格式数据转换成文本，异常则返回默认值</summary>
		''' <param name="this">要转换的对象</param>
		''' <param name="type">数据类型</param>
		''' <param name="validate">用于校验数据是否合法的过程，比如字符长度，数字大小等</param>
		<Extension>
		Public Function GetArrayString(this As Object, type As FieldTypeEnum, arrayString As String, Optional arrayRepeat As Boolean = False, Optional validate As Func(Of Object, Boolean) = Nothing) As String
			If this IsNot Nothing Then
				If GetType(ICollection).IsAssignableFrom(this.GetType) Then
					Dim Ret As New List(Of String)

					For Each item In TryCast(this, ICollection)
						Ret.Add(GetString(item, type, validate))
					Next

					If Not arrayRepeat Then Ret = Ret.Distinct.ToList

					Return String.Join(arrayString, Ret)
				End If
				'Dim objList As List(Of Object) = Nothing

				'Dim objType = this.GetType
				'If objType.IsArray Then
				'	objList = New List(Of Object)
				'	objList.AddRange(this)

				'ElseIf objType.IsList Then
				'	objList = this
				'End If

				'If objList?.Count > 0 Then
				'	Dim Ret = objList.Select(Function(x) GetString(x, type, validate)).Where(Function(x) x.NotNull)
				'	If Not arrayRepeat Then Ret = Ret.Distinct

				'	Return String.Join(arrayString, Ret)
				'End If
			End If

			Return ""
		End Function

		''' <summary>根据类型转换成指定格式数据，异常则返回默认值</summary>
		''' <param name="this">要转换的原始字符串</param>
		''' <param name="type">数据类型</param>
		''' <param name="validate">用于校验数据是否合法的过程，比如字符长度，数字大小等</param>
		''' <param name="converAll">对于数字，是否尝试进行字符处理，过滤掉的字符串</param>
		<Extension>
		Public Function ToValue(this As String, type As FieldTypeEnum, Optional validate As Func(Of Object, Boolean) = Nothing, Optional converAll As Boolean = False) As Object
			Dim Ret

			Select Case type
				Case FieldTypeEnum.ASCII
					Ret = this.GetAscii

				Case FieldTypeEnum.CHINESE
					Ret = this.GetChinese

				Case FieldTypeEnum.BOOLEAN
					Ret = this.ToBoolean

				Case FieldTypeEnum.TRISTATE
					Ret = this.ToTriState

				Case FieldTypeEnum.NUMBER
					Ret = this.ToNumber(converAll)

				Case FieldTypeEnum.BYTES
					Ret = this.ToByte(converAll)

				Case FieldTypeEnum.INTEGER
					Ret = this.ToInteger(converAll)

				Case FieldTypeEnum.LONG
					Ret = this.ToLong(converAll)

				Case FieldTypeEnum.SINGLE
					Ret = this.ToSingle(converAll)

				Case FieldTypeEnum.DOUBLE
					Ret = this.ToDouble(converAll)

				Case FieldTypeEnum.DATETIME
					Ret = this.ToDateTime(New Date)

				Case FieldTypeEnum.DATE
					Dim v = this.ToDateTime(New Date)
					If v > New Date Then
						Ret = v.ToString("yyyy-MM-dd")    ' 0001-01-01 的时间标记为无效时间
					Else
						Ret = ""
					End If

				Case FieldTypeEnum.TIME
					Ret = ("2000-01-01 " & this).ToDateTime(New Date).ToString("HH:mm:ss")

				Case FieldTypeEnum.UPPER_CASE
					Ret = this.ToUpper

				Case FieldTypeEnum.LOWER_CASE
					Ret = this.ToLower

				Case FieldTypeEnum.EMAIL
					Ret = If(this.IsEmail, this, "")

				Case FieldTypeEnum.URL
					Ret = this.ToUrl

				Case FieldTypeEnum.MOBILEPHONE
					Ret = If(this.IsMobilePhone, this, "")

				Case FieldTypeEnum.FOLDER
					Dim Root = PathHelper.Root
					Dim startWithLine = (this.StartsWith("/") OrElse this.StartsWith("\")) AndAlso Not this.StartsWith("//")
					this = PathHelper.Root(this.ToPath)
					If this.StartsWith(Root, StringComparison.OrdinalIgnoreCase) Then
						Dim p = Root.Length
						If Not startWithLine Then p += 1

						this = this.Substring(p)
					End If
					Ret = this

				Case FieldTypeEnum.GUID
					Ret = this.ToGuid

				Case FieldTypeEnum.JSON
					' 字典对象，列表对象
					Ret = this.ToJsonCollection.Value

				Case FieldTypeEnum.XML
					Ret = this.ToString.ToXML

				Case Else
					Ret = this
			End Select

			If Ret IsNot Nothing AndAlso validate IsNot Nothing Then
				If Not validate(Ret) Then Ret = Nothing
			End If

			Return Ret
		End Function

		''' <summary>将系统类数据转换成数据类型数组</summary>
		''' <param name="this">要转换的原始字符串</param>
		''' <param name="type">数据类型</param>
		''' <param name="arrayString">数组分隔字符</param>
		''' <param name="arrayRepeat">数组是否允许重复项目</param>
		''' <param name="validate">用于校验数据是否合法的过程，比如字符长度，数字大小等</param>
		<Extension>
		Public Function ToArrayValue(this As String, type As FieldTypeEnum, Optional arrayString As String = "", Optional arrayRepeat As Boolean = False, Optional validate As Func(Of Object, Boolean) = Nothing) As Array
			Select Case type
				Case FieldTypeEnum.BYTES, FieldTypeEnum.TRISTATE
					Return ToArrayValue(Of Byte)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.NUMBER
					Return ToArrayValue(Of Decimal)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.INTEGER
					Return ToArrayValue(Of Integer)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.LONG
					Return ToArrayValue(Of Long)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.DOUBLE
					Return ToArrayValue(Of Double)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.SINGLE
					Return ToArrayValue(Of Single)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.DATETIME ' FiledType.Date, FiledType.Time Date 与 Time 非完整时间值，所以交由字符处理
					Return ToArrayValue(Of Date)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.BOOLEAN
					Return ToArrayValue(Of Boolean)(this, type, arrayString, arrayRepeat, validate)

				Case FieldTypeEnum.GUID
					Return ToArrayValue(Of Guid)(this, type, arrayString, arrayRepeat, validate)

				Case Else
					Return ToArrayValue(Of String)(this, type, arrayString, arrayRepeat, validate)
			End Select
		End Function

		Private Function ToArrayValue(Of T)(this As String, type As FieldTypeEnum, Optional arrayString As String = "", Optional arrayRepeat As Boolean = False, Optional validate As Func(Of Object, Boolean) = Nothing) As T()
			Dim splitOption = If(arrayRepeat, SplitEnum.REMOVE_EMPTY_ENTRIES, SplitEnum.REMOVE_SAME Or SplitEnum.REMOVE_EMPTY_ENTRIES)
			Dim ret = this.SplitEx(arrayString, splitOption)?.Select(Function(x)
																		 Try
																			 Dim y As T = x.ToValue(type, validate, True)
																			 Return y
																		 Catch ex As Exception
																			 Return Nothing
																		 End Try
																	 End Function).Where(Function(x) x IsNot Nothing)

			' 再次移除值中的重复项目
			If Not arrayRepeat Then ret = ret.Distinct

			Return ret.ToArray
		End Function

		''' <summary>转换类型</summary>
		Public Function ChangeType(Of T)(this As Object) As T
			Try
				Return this
			Catch ex As Exception
				Return Nothing
			End Try
		End Function

		''' <summary>转换类型</summary>
		Public Function ChangeType(this As Object, targetType As Type) As Object
			If this Is Nothing Then Return Nothing

			' 如果类型一致或者继承类型则直接返回
			Dim thisType = this.GetType
			If thisType.IsComeFrom(targetType) Then Return this

			' 尝试转换，系统方式转换失败则使用字符串（JSON）转换
			Try
				Return Convert.ChangeType(this, targetType)
			Catch ex As Exception
				Return ToObjectString(this).ToValue(targetType)
			End Try
		End Function

#End Region

	End Module
End Namespace