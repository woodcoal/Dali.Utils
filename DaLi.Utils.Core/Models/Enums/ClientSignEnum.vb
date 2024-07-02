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
' 	公共参数枚举
'
' 	name: Model
' 	create: 2020-11-17
' 	memo: GUID 生成类型枚举
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>客户端签名模式</summary>
	Public Enum ClientSignEnum

		''' <summary>不验证，仅仅分析账号</summary>
		NONE = 0

		''' <summary>MD5(账号+密码)</summary>
		PASSWORD

		''' <summary>MD5(账号+密码+地址+参数排序)</summary>
		COMMAND

		''' <summary>MD5(账号+密码+5分钟时间迭代次数+地址+参数排序)</summary>
		SIGN

		''' <summary>MD5(账号+密码+10分钟时间迭代次数+地址+参数排序+随机码)</summary>
		SIGN_RND

		''' <summary>MD5(账号+密码+时间戳+地址+参数排序)</summary>
		SIGN_TIME

		''' <summary>MD5(账号+密码+时间戳+地址+参数排序+随机码)</summary>
		SIGN_TIME_RND
	End Enum

End Namespace
