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
' 	程序集相关扩展操作
'
' 	name: Extension.AssemblyExtension
' 	create: 2020-11-10
' 	memo: 程序集相关扩展操作
' 	
' ------------------------------------------------------------

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>程序集相关扩展操作</summary>
	<Obsolete("待整理")>
	Public Module AssemblyExtension

		''' <summary>版权</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Copyright(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyCopyrightAttribute)?.Copyright
		End Function

		''' <summary>公司</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Company(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyCompanyAttribute)?.Company
		End Function

		''' <summary>配置</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Configuration(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyConfigurationAttribute)?.Configuration
		End Function

		''' <summary>商标</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Trademark(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyTrademarkAttribute)?.Trademark
		End Function

		''' <summary>区域</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Culture(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyCultureAttribute)?.Culture
		End Function

		''' <summary>描述</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Description(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyDescriptionAttribute)?.Description
		End Function

		''' <summary>产品</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Product(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyProductAttribute)?.Product
		End Function

		''' <summary>标题</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Title(this As Assembly) As String
			Return this.GetCustomAttribute(Of AssemblyTitleAttribute)?.Title
		End Function

		''' <summary>名称</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Name(this As Assembly) As String
			Return this?.GetName.Name
		End Function

		''' <summary>主次版本，将主次版本合并成数字</summary>
		''' <param name="this">程序集</param>
		<Extension>
		Public Function Version(this As Assembly) As Single
			Dim Ver = this?.GetName.Version
			If Ver IsNot Nothing Then
				Return (Ver.Major & "." & Ver.Minor).ToSingle(False)
			Else
				Return 0
			End If
		End Function

	End Module
End Namespace