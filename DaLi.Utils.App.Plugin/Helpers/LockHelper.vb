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
' 	参数锁定操作
'
' 	name: Helper.LockHelper
' 	create: 2023-02-17
' 	memo: 参数锁定操作，防止多次启动时重复操作
'
' ------------------------------------------------------------

Namespace Helper

	''' <summary>参数锁定操作，防止多次启动时重复操作</summary>
	Public NotInheritable Class LockHelper

		''' <summary>锁定对象值</summary>
		Private Shared _Lock As KeyValueDictionary

		''' <summary>锁定文件位置</summary>
		Private Const LOCK_FILE As String = ".lock"

		''' <summary>检查指定的键是否锁定</summary>
		''' <param name="key">键名称</param>
		Public Shared Function GetLock(key As String) As Boolean
			If key.IsEmpty Then Return False

			If _Lock Is Nothing Then
				_Lock = PathHelper.ReadJson(Of KeyValueDictionary)(LOCK_FILE)
				_Lock = If(_Lock, New KeyValueDictionary)

				_Lock("说明") = "本文件为参数锁定文件，如果系统初次使用初始化，请勿删除此文件。如果您对本文件参数不熟悉也请勿随意修改。"
			End If

			Return If(_Lock(key), "").ToString.ToBoolean
		End Function

		''' <summary>检查指定的键是否锁定</summary>
		''' <param name="key">键名称</param>
		''' <param name="comment">键注释</param>
		''' <param name="value">键值，默认 True</param>
		Public Shared Sub SetLock(key As String, Optional comment As String = "", Optional value As Boolean = True)
			If key.IsEmpty Then Return

			' 结果未变化，无需保存
			If GetLock(key) = value Then Return

			_Lock(key) = value
			If comment.NotEmpty Then _Lock($"{key}:COMMENT") = comment

			PathHelper.SaveJson(LOCK_FILE, _Lock)
		End Sub

	End Class

End Namespace