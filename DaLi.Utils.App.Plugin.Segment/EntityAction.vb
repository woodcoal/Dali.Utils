' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	Dali.App Is licensed under GPLv3
'
' ------------------------------------------------------------
'
' 	对实体数据进行关键词与总结
'
' 	name: EntityAction
' 	create: 2024-06-30
' 	memo: 对实体数据进行关键词与总结。
' 		  只处理属性上存在 EntityCustomAttribute 属性的字段，如： EntityCustom(Provider = "segment", Action = "keyword", Source = "content")；
' 		  EntityCustomAttribute 的 Provider 必须是 segment，Action 必须是关键词或者总结。
'
' ------------------------------------------------------------


Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Extension
Imports DaLi.Utils.App.Interface
Imports DaLi.Utils.App.Model

''' <summary>对实体数据进行 AI 处理</summary>
Public Class EntityAction
	Inherits EntityActionBase

	''' <summary>默认操作源名称(EntityCustom 中 Provider 的值)</summary>
	Protected Overrides ReadOnly Property ProviderName As String
		Get
			Return "segment"
		End Get
	End Property

	''' <summary>默认允许的操作，为空则表示不设置才生效(EntityCustom 中 Action 是否必须在此范围)</summary>
	Protected Overrides ReadOnly Property EnabledActions As String()
		Get
			Return {"keyword", "keywords", "summary"}
		End Get
	End Property

	''' <summary>是否强制需要存在来源字段才能处理(EntityCustom 中 Source 是否必须有效)</summary>
	Protected Overrides ReadOnly Property SourceForce As Boolean
		Get
			Return True
		End Get
	End Property

	''' <summary>项目操作之前的验证</summary>
	''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
	''' <param name="entity">当前实体</param>
	''' <param name="source">编辑时更新前的原始值</param>
	Public Overrides Sub ExecuteValidate(Of T As IEntity)(action As EntityActionEnum, entity As T, context As IAppContext, errorMessage As ErrorMessage, db As IFreeSql, Optional source As T = Nothing)
		Select Case action
			Case EntityActionEnum.ADD, EntityActionEnum.EDIT
				' 添加，编辑模式下检查关键词
				Dim pros = GetProperties(entity)
				If pros Is Nothing Then Return

				' 处理字段
				For Each kv In pros
					' 少于 50 个字符，不处理
					Dim content = kv.Key.GetValue(entity)?.ToString
					If content.IsEmpty Then Continue For

					' 获取文本字段长度
					Dim Lens = kv.Value.Select(Function(x) x.Target).
						Distinct.
						ToDictionary(Function(x) x, Function(x) x.GetStringLength).
						Where(Function(x) x.Value >= 10 OrElse x.Value = 0).
						ToDictionary(Function(x) x.Key, Function(x) x.Value)

					If Lens.IsEmpty Then Continue For

					' 要修改的属性，必须是文本字段，且允许长度大于0
					Dim attrs = kv.Value.Where(Function(x) Lens.ContainsKey(x.Target)).ToList

					' 分析获取字段文本最大长度
					Dim max = Lens.Max(Function(x) x.Value)

					' 分析获取总结
					If ExistAction(attrs, "summary") Then
						Dim summary = Segment.Default.Summary(content, max, True)
						If summary.NotEmpty Then
							UpdateValue(entity, attrs, context,
										summary, "summary",
										Function(pro, value)
											Dim len = Lens(pro)
											Return If(len > 0, summary.ShortShow(len), summary)
										End Function)
						End If
					End If

					' 分析获取关键词
					If ExistAction(attrs, {"keyword", "keywords"}) Then
						' 修改关键词，支持两个 Action，分别是关键词和总结
						Dim keywords = Segment.Default.Keywords(content, 20, JiebaNet.Segmenter.Constants.NounPos, -20)
						If keywords.NotEmpty Then
							UpdateValue(entity, attrs, context,
										keywords, {"keyword", "keywords"},
										Function(pro, value)
											Dim len = Lens(pro)
											Return keywords.JoinString(",", len)
										End Function)
						End If
					End If
				Next
		End Select

	End Sub

End Class
