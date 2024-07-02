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
' 	模块接口
'
' 	name: Auto.IRule
' 	create: 2022-12-30
' 	memo: 模块接口
'
' ------------------------------------------------------------

Namespace Auto

	''' <summary>模块接口</summary>
	Public Interface IRule
		Inherits ICloneable

		''' <summary>类型标识（不能重复）</summary>
		ReadOnly Property Type As String

		''' <summary>模块名称</summary>
		ReadOnly Property Name As String

		''' <summary>输出值</summary>
		ReadOnly Property Output As String

		''' <summary>对于值已经处理包含变量或者对于包含子模块的项目是否将子模块的执行结果一并输出到主流程，如果不含子流程则不用设置此值</summary>
		''' <remarks>如：计时器。True 则将计时器的结果与计时器内的模块值都输出到主流程 ，False 则只返回计时器的结果；如：http 下载，执行后已经将变量结果输出到结果中则无需再次处理</remarks>
		ReadOnly Property OutResult As String

		''' <summary>是否忽略无结果，False：当未分析到任何内容时报错；True：当未分析到任何你内容时忽略此问题</summary>
		Property EmptyIgnore As Boolean

		''' <summary>忽略错误</summary>
		Property ErrorIgnore As Boolean

		''' <summary>友好错误消息</summary>
		Property ErrorMessage As String

		''' <summary>启用</summary>
		Property Enabled As Boolean

		''' <summary>验证规则是否存在异常</summary>
		Function Validate(Optional ByRef message As String = Nothing) As Boolean

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Function Execute(data As IDictionary(Of String, Object)) As AutoMessage
	End Interface
End Namespace