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
' 	GUID 扩展操作
'
' 	name: Extension.GuidExtension
' 	create: 2020-12-07
' 	memo: GUID 扩展操作
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>GUID 扩展操作</summary>
	Public Module GuidExtension

		''' <summary>是否空值</summary>
		''' <param name="this">要操作的 GUID</param>
		<Extension>
		Public Function IsEmpty(this As Guid?) As Boolean
			Return this Is Nothing OrElse this = Guid.Empty
		End Function

		''' <summary>是否空值</summary>
		''' <param name="this">要操作的 GUID</param>
		<Extension>
		Public Function IsEmpty(this As Guid) As Boolean
			Return this = Guid.Empty
		End Function

		''' <summary>是否存在值</summary>
		''' <param name="this">要操作的 GUID</param>
		<Extension>
		Public Function NotEmpty(this As Guid?) As Boolean
			Return this IsNot Nothing AndAlso this <> Guid.Empty
		End Function

		''' <summary>是否存在值</summary>
		''' <param name="this">要操作的 GUID</param>
		<Extension>
		Public Function NotEmpty(this As Guid) As Boolean
			Return this <> Guid.Empty
		End Function

		''' <summary>字符串转 GUID 列表</summary>
		''' <param name="this">要转换的字符串</param>
		''' <param name="separator">分割符，不设置将使用系统默认的逗号分号</param>
		<Extension>
		Public Function ToGuidList(this As String, Optional separator As String = Nothing) As Guid()
			Return this.ClearSpace.SplitDistinct(separator)?.Where(Function(x) x.IsGUID).Select(Function(x) x.ToGuid).Distinct.ToArray
		End Function

	End Module
End Namespace