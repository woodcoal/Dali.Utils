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
' 	对数据进行编码，防止明文展示
'
' 	name: Attribute.FieldEncodeAttribute
' 	create: 2024-02-24
' 	memo: 对数据进行编码，防止明文展示
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>对数据进行编码，防止明文展示</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
	Public NotInheritable Class FieldEncodeAttribute
		Inherits System.Attribute

		''' <summary>密钥</summary>
		Public ReadOnly Property Key As String

		''' <param name="key">加密密钥</param>
		Public Sub New(Optional key As String = "19491001")
			Me.Key = key
		End Sub

		''' <summary>对象加密</summary>
		Public Function Encode(value As Object) As String
			Return TypeExtension.ToEncodeString(value, Key)
		End Function

		''' <summary>字符解密</summary>
		Public Function Decode(str As String) As Object
			Return str.ToDecodeObject(Key)
		End Function

	End Class

End Namespace