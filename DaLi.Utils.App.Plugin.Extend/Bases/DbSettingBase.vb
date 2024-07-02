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
' 	数据参数基类
'
' 	name: Base.DbSettingBase
' 	create: 2023-02-17
' 	memo: 数据参数基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports System.Reflection
Imports System.Text.Json.Serialization
Imports Microsoft.Extensions.Logging

Namespace Base

	''' <summary>数据参数基类</summary>
	Public MustInherit Class DbSettingBase(Of T As {IDbSetting, New})
		Implements IDbSetting

		'''' <summary>数据库对象</summary>
		Private _Db As IFreeSql

		'''' <summary>数据库对象</summary>
		Protected ReadOnly Property Db As IFreeSql
			Get
				If _Db Is Nothing Then
					_Db = SYS.GetService(Of IFreeSql)
					If _Db Is Nothing Then Throw New Exception("数据库参数无法读取，默认数据库连接未设置")
				End If

				Return _Db
			End Get
		End Property

		'''' <summary>日志对象</summary>
		Private _Log As ILogger(Of T)

		'''' <summary>日志对象</summary>
		Protected ReadOnly Property Log As ILogger(Of T)
			Get
				If _Log Is Nothing Then _Log = SYS.GetLOG(Of T)
				Return _Log
			End Get
		End Property

		'''' <summary>设置参数文件名称</summary>
		<JsonIgnore>
		Public Overridable ReadOnly Property ModuleName As String = [GetType].FullName Implements IDbSetting.ModuleName

		'''' <summary>是否需要开启监控，文件变化则自动更新参数值</summary>
		<JsonIgnore>
		Protected Overridable ReadOnly Property AutoUpdate As Boolean = True Implements ISetting.AutoUpdate

		''' <summary>是否注入到配置清单，True：注入，下次可以通过 SYS.GetSetting 获取；False：不注入，参数初始完成后立即销毁，无法再次调用</summary>
		<JsonIgnore>
		Protected Overridable ReadOnly Property Inject As Boolean = True Implements ISetting.Inject

		''' <summary>读取设置</summary>
		Public Sub Load(provider As ISettingProvider) Implements IDbSetting.Load
			If ModuleName.IsEmpty Then Return

			' 将模块名过滤掉 SETTING
			' DALI.FRAMEWORK.SETTING.APPSETTING => DALI.FRAMEWORK.APP

			Dim modName = CommonHelper.UpdateName(ModuleName, "setting")

			' 从数据库获取参数值
			Dim configs = Db.Select(Of ConfigEntity).WhereEquals(modName, Function(x) x.Module).ToList
			configs = If(configs, New List(Of ConfigEntity))

			' 创建默认参数集合
			Dim setting As New T
			Dim pros = setting.GetType.GetAllProperties
			For Each pro In pros
				If pro.CanRead AndAlso pro.CanWrite AndAlso pro.IsPublic Then
					Dim field = configs.Where(Function(x) x.Field.IsSame(pro.Name)).FirstOrDefault
					Dim fieldEncode = pro.GetCustomAttribute(Of FieldEncodeAttribute)
					If field Is Nothing Then
						Try
							' 获取所有属性
							Dim Attrs = ValidationAttributeHelper.AttributeValues(pro)

							' 字段是否需要加密
							Dim value = pro.GetValue(setting)

							If fieldEncode IsNot Nothing Then
								value = fieldEncode.Encode(value)
							Else
								value = TypeExtension.ToObjectString(value)
							End If

							' 入库
							Dim entity As New ConfigEntity With {
								.Module = modName,
								.Field = pro.Name,
								.Value = value,
								.Attributes = Attrs.ToJson,
								.Desc = ValidationAttributeHelper.AttributeDescription(pro),
								.CreateTime = SYS_NOW_DATE,
								.CreateBy = "Auto"
							}

							Db.Insert(Of ConfigEntity).AppendData(entity).ExecuteAffrows()
						Catch ex As Exception
							'CON.Err(ex, $"参数异常 {pro.Name} / {pro.PropertyType.FullName}")
							Log.LogError(ex, "数据参数 {name} {action} 异常：参数来源：{source}", pro.Name, "加载", pro.PropertyType.FullName)
						End Try
					Else
						Try
							Dim value As Object
							If fieldEncode IsNot Nothing Then
								value = fieldEncode.Decode(field.Value)
							Else
								value = field.Value.ToValue(pro.PropertyType)
							End If

							If value IsNot Nothing Then pro.SetValue(setting, value)
						Catch ex As Exception
						End Try
					End If
				End If
			Next

			' 属性验证
			' 验证主要字段
			Dim valContext = New ValidationContext(setting)
			Dim result As New List(Of ValidationResult)
			Dim errorFields As List(Of String) = Nothing
			Try
				' 序列化后的值检测
				If Not Validator.TryValidateObject(setting, valContext, result, True) Then
					errorFields = result.Select(Function(x) x.MemberNames.FirstOrDefault).Where(Function(x) x.NotEmpty).ToList
				End If
			Catch ex As Exception
			End Try
			errorFields = If(errorFields, New List(Of String))

			' 更新前检查
			BeforeInitialize(provider)

			' 重新赋值参数
			setting.GetType.GetAllProperties _
				.Where(Function(x) Not errorFields.Contains(x.Name)) _
				.ToList _
				.ForEach(Sub(pro)
							 Try
								 If pro.CanWrite Then pro.SetValue(Me, pro.GetValue(setting))
							 Catch ex As Exception
								 Log.LogError(ex, "数据参数 {name} {action} 异常：参数来源：{source}；当前参数：{local}", pro.Name, "赋值", setting.GetType.FullName, [GetType].FullName)
							 End Try
						 End Sub)

			' 初始化一次操作
			Initialize(provider)
		End Sub

		''' <summary>更新参数前的操作</summary>
		Protected Overridable Sub BeforeInitialize(provider As ISettingProvider)
		End Sub

		''' <summary>获取数据后初始化操作，如果不注入的话，初始化一定要操作，否则参数加载无意义</summary>
		Protected Overridable Sub Initialize(provider As ISettingProvider)
		End Sub

		''' <summary>更新参数值，并写入数据库。注意：此操作不会校验数据，因此在此操作前请先确保数据正确</summary>
		''' <param name="proName">字段</param>
		''' <param name="value">值</param>
		''' <param name="user">操作员</param>
		Public Function Update(proName As String, value As Object, Optional user As String = "Auto") As Boolean
			Dim pro = [GetType].GetSingleProperty(proName)
			If Not pro.CanRead OrElse Not pro.CanWrite OrElse Not pro.IsPublic Then Return False

			Dim modName = CommonHelper.UpdateName(ModuleName, "setting")

			Try
				' 数据记录
				pro.SetValue(Me, value)

				' 入库
				Dim fieldEncode = pro.GetCustomAttribute(Of FieldEncodeAttribute)
				If fieldEncode IsNot Nothing Then
					value = fieldEncode.Encode(value)
				Else
					value = TypeExtension.ToObjectString(value)
				End If

				Using repo = Db.GetRepository(Of ConfigEntity)
					Dim entity = repo.Where(Function(x) x.Module = modName AndAlso x.Field = proName).ToOne
					If entity IsNot Nothing Then
						entity.Value = value
						entity.UpdateTime = SYS_NOW_DATE
						entity.UpdateBy = user

						repo.Update(entity)
					End If
				End Using

				' 更新对象字段，远程更新
				SYS.Events.Publish(E_SETTING_UPDATE, modName)

				Return True
			Catch ex As Exception
				Log.LogError(ex, "更新 {modName} 参数 {field} 值异常：{message}", modName, proName, ex.Message)
				Return False
			End Try
		End Function
	End Class

End Namespace
