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
' 	JieBa 分词
'
' 	name: JieBa
' 	create: 2021-07-17
' 	memo: JieBa 分词
'		  词性请参考：https://blog.csdn.net/yellow_python/article/details/83991967
' 	
' ------------------------------------------------------------

Imports JiebaNet.Analyser
Imports JiebaNet.Segmenter
Imports JiebaNet.Segmenter.PosSeg

Public Class Segment

#Region "全局引用"

	''' <summary>默认全局分词组件</summary>
	Private Shared ReadOnly _Default As New Lazy(Of Segment)

	''' <summary>默认全局分词组件</summary>
	Public Shared ReadOnly Property [Default] As Segment
		Get
			Return _Default.Value
		End Get
	End Property

#End Region

#Region "公共属性"

	''' <summary>分词组件</summary>
	Private _Segmenter As JiebaSegmenter

	''' <summary>分词组件</summary>
	Private ReadOnly Property Segmenter As JiebaSegmenter
		Get
			If _Segmenter Is Nothing Then _Segmenter = New JiebaSegmenter
			Return _Segmenter
		End Get
	End Property

	''' <summary>词性组件</summary>
	Private _PosSeg As PosSegmenter

	''' <summary>词性组件</summary>
	Private ReadOnly Property PosSeg As PosSegmenter
		Get
			If _PosSeg Is Nothing Then _PosSeg = New PosSegmenter
			Return _PosSeg
		End Get
	End Property

	''' <summary>TF-IDF 关键词提取组件</summary>
	Private _Tfidf As TfidfExtractor

	''' <summary>TF-IDF 关键词提取组件</summary>
	Private ReadOnly Property Tfidf As TfidfExtractor
		Get
			If _Tfidf Is Nothing Then _Tfidf = New TfidfExtractor
			Return _Tfidf
		End Get
	End Property

	''' <summary>TF-IDF 关键词提取组件</summary>
	Private _TextRank As TextRankExtractor

	''' <summary>TF-IDF 关键词提取组件</summary>
	Private ReadOnly Property TextRank As TextRankExtractor
		Get
			If _TextRank Is Nothing Then _TextRank = New TextRankExtractor
			Return _TextRank
		End Get
	End Property

#End Region

#Region "词组处理"

	''' <summary>添加的新词</summary>
	Public Shared Segment_Keyword_Insert As String()

	''' <summary>移除的词组</summary>
	Public Shared Segment_Keyword_Remove As String()

	''' <summary>关键词分析停用词</summary>
	Public Shared Extractor_Stopwords As String()

	Private _SegmentInit As Boolean = False
	Private _ExtractorInit As Boolean = False

	''' <summary>分词初始化</summary>
	Private Sub Segment_Init()
		If _SegmentInit Then Exit Sub
		_SegmentInit = True

		' 用户字典
		Dim userDic = "Resources/user.txt"
		If PathHelper.FileExist(userDic) Then Segmenter.LoadUserDict(userDic)


		If Segment_Keyword_Insert.NotEmpty Then
			If Segment_Keyword_Insert.NotEmpty Then
				For Each key In Segment_Keyword_Insert
					Segmenter.AddWord(key)
				Next
			End If
		End If

		If Segment_Keyword_Remove.NotEmpty Then
			If Segment_Keyword_Remove.NotEmpty Then
				For Each key In Segment_Keyword_Remove
					Segmenter.DeleteWord(key)
				Next
			End If
		End If

	End Sub

	''' <summary>关键词初始化</summary>
	Private Sub Extractor_Init()
		If _ExtractorInit Then Exit Sub
		_ExtractorInit = True

		Call Segment_Init()

		If Extractor_Stopwords.NotEmpty Then
			If Extractor_Stopwords.NotEmpty Then
				Tfidf.AddStopWords(Extractor_Stopwords)
				TextRank.AddStopWords(Extractor_Stopwords)
			End If
		End If
	End Sub

	''' <summary>添加新词</summary>
	''' <param name="word">新词</param>
	''' <param name="freq">词频，不设置自动分析</param>
	''' <param name="tag">标签</param>
	Public Sub AddWord(word As String, Optional freq As Integer = 0, Optional tag As String = Nothing)
		Segmenter.AddWord(word, freq, tag)
	End Sub

	''' <summary>移除关键词</summary>
	Public Sub DeleteWord(word As String)
		Segmenter.DeleteWord(word)
	End Sub

#End Region

#Region "分词操作"

	''' <summary>分词</summary>
	''' <param name="text">内容</param>
	''' <param name="cutAll">精确模式(false) / 全模式(true) 精确模式是最基础和自然的模式，试图将句子最精确地切开，适合文本分析；而全模式，把句子中所有的可以成词的词语都扫描出来, 速度更快，但是不能解决歧义，因为它不会扫描最大概率路径，也不会通过HMM去发现未登录词。</param>
	''' <param name="hmm">未登录词分析</param>
	Public Function Cut(text As String, Optional cutAll As Boolean = False, Optional hmm As Boolean = True) As IEnumerable(Of String)
		Call Segment_Init()
		Return Segmenter.Cut(text, cutAll, hmm)
	End Function

	''' <summary>带词性切词</summary>
	''' <param name="text">内容</param>
	''' <param name="hmm">未登录词分析</param>
	Public Function Cut(text As String, Optional hmm As Boolean = True) As IEnumerable(Of Pair)
		Call Segment_Init()
		Return PosSeg.Cut(text, hmm)
	End Function

	''' <summary>带词性切词</summary>
	''' <param name="text">内容</param>
	''' <param name="count">返回数量</param>
	''' <param name="allowPos">允许的词性（n：普通名词；  nr：人名；  nz：其他专名；  a：形容词；  m：数量词；  c：连词；  PER：人名；  f：方位名词；  ns：地名；  v：普通动词；  ad：副形词；  q：量词；  u：助词；  LOC：地名；  s：处所名词；  nt：机构名；  vd：动副词；  an：名形词；  r：代词；  xc：其他虚词；  ORG：机构名；  t：时间；  nw：作品名；  vn：名动词；  d：副词；  p：介词；  w：标点符号；  TIME：时间），TextRank 模式默认使用名词与动词；默认值（Constants.NounPos：名词； Constants.VerbPos：动词； Constants.NounAndVerbPos：名词与动词； Constants.IdiomPos：形容词） </param>
	''' <param name="textRankSpan">TextRank 模式 Span 长度，默认为 5，0 则 表示使用 IF-IDF 模式，小于 0 则表示综合两者的交集</param>
	Public Function Keywords(text As String, Optional count As Integer = 20, Optional allowPos As IEnumerable(Of String) = Nothing, Optional textRankSpan As Integer = 5) As IEnumerable(Of String)
		Call Extractor_Init()
		Call UpdateText(text)

		If text.IsEmpty Then Return Nothing
		If allowPos.IsEmpty Then allowPos = JiebaNet.Segmenter.Constants.NounAndVerbPos

		Dim ret As IEnumerable(Of String)

		If textRankSpan > 0 Then
			TextRank.Span = textRankSpan
			ret = TextRank.ExtractTags(text, count, allowPos)
		ElseIf textRankSpan = 0 Then
			ret = Tfidf.ExtractTags(text, count, allowPos)
		Else
			TextRank.Span = 0 - textRankSpan
			Dim num As Integer = Math.Ceiling(count * 1.5)
			Dim Tr = TextRank.ExtractTags(text, num, allowPos)
			Dim Tf = Tfidf.ExtractTags(text, num, allowPos)

			If Tr.Any And Tf.Any Then
				ret = Tr.Intersect(Tf).Take(count)
			ElseIf Tr.Any Then
				ret = Tr
			Else
				ret = Tf
			End If
		End If

		Return ret.ToList
	End Function

	''' <summary>带词性切词</summary>
	''' <param name="text">内容</param>
	''' <param name="count">返回数量</param>
	''' <param name="allowPos">允许的词性（n：普通名词；  nr：人名；  nz：其他专名；  a：形容词；  m：数量词；  c：连词；  PER：人名；  f：方位名词；  ns：地名；  v：普通动词；  ad：副形词；  q：量词；  u：助词；  LOC：地名；  s：处所名词；  nt：机构名；  vd：动副词；  an：名形词；  r：代词；  xc：其他虚词；  ORG：机构名；  t：时间；  nw：作品名；  vn：名动词；  d：副词；  p：介词；  w：标点符号；  TIME：时间），TextRank 模式默认使用名词与动词；默认值（Constants.NounPos：名词； Constants.VerbPos：动词； Constants.NounAndVerbPos：名词与动词； Constants.IdiomPos：形容词） </param>
	''' <param name="textRankSpan">TextRank 模式 Span 长度，默认为 5，小于 1 则 表示使用 IF-IDF 模式</param>
	Public Function KeywordsWithWeight(text As String, Optional count As Integer = 20, Optional allowPos As IEnumerable(Of String) = Nothing, Optional textRankSpan As Integer = 5) As IEnumerable(Of WordWeightPair)
		Call Extractor_Init()
		Call UpdateText(text)

		If text.IsEmpty Then Return Nothing
		If allowPos.IsEmpty Then allowPos = JiebaNet.Segmenter.Constants.NounAndVerbPos

		Dim ret As IEnumerable(Of WordWeightPair)

		If textRankSpan > 0 Then
			TextRank.Span = textRankSpan
			ret = TextRank.ExtractTagsWithWeight(text, count, allowPos)
		ElseIf textRankSpan = 0 Then
			Return Tfidf.ExtractTagsWithWeight(text, count, allowPos)
		Else
			TextRank.Span = 0 - textRankSpan
			Dim num As Integer = count / 4 * 3
			Dim Tr = TextRank.ExtractTagsWithWeight(text, num, allowPos)
			Dim Tf = Tfidf.ExtractTagsWithWeight(text, num, allowPos)

			If Tr.Any And Tf.Any Then
				ret = Tr.Union(Tf).OrderBy(Function(x) x.Weight).Distinct(Function(x) x.Word).Take(count)
			ElseIf Tr.Any Then
				ret = Tr
			Else
				ret = Tf
			End If
		End If

		Return ret.ToList
	End Function

	''' <summary>更新要分词的数据，方便进行切割</summary>
	''' <param name="text">内容</param>
	Private Shared Sub UpdateText(ByRef text As String)
		If text.NotEmpty Then
			Dim sp = Guid.NewGuid.ToString
			text = text.Replace(vbCr, sp).Replace(vbLf, sp)
			text = text.ClearHtml("all")
			text = text.Replace(sp, vbCrLf)
		End If
	End Sub

#End Region

#Region "获取摘要"

	''' <summary>摘要提取</summary>
	''' <param name="text">原始文章</param>
	''' <param name="max">摘要最多返回文字数量</param>
	''' <param name="keywords">返回关键词</param>
	''' <param name="clear">是否清理摘要结果，去掉空格回车等</param>
	Public Function Summary(text As String, Optional max As Integer = 200, Optional clear As Boolean = True, Optional ByRef keywords As IEnumerable(Of String) = Nothing) As String
		keywords = Nothing

		Call UpdateText(text)
		If text.IsEmpty OrElse max < 10 Then Return ""

		' 分析句子
		Dim sents = GetSentence(text)
		If sents.IsEmpty Then Return ""

		' 分析关键词
		Dim Count = text.Length / 4
		If Count < 20 Then Count = 20
		Dim keys = TextRank.ExtractTagsWithWeight(text, Count)
		If keys.IsEmpty Then Return ""
		keywords = keys.Select(Function(x) x.Word).ToList

		Dim dic = New Dictionary(Of Integer, (Content As String, Mark As Double))
		For I = 0 To sents.Count - 1
			Dim sent = sents(I)

			' 计算评分
			Dim v As Double = 0
			For Each k In keys
				Dim num = sent.Times(k.Word)
				If num > 0 Then
					v += k.Weight
				End If
			Next

			dic.Add(I, (sent, v))
		Next

		Dim maxLen = max
		Dim RetList As New Dictionary(Of Integer, String)
		'For Each item In dic.OrderByDescending(Function(x) x.Value.Mark)
		'	Dim content = item.Value.Content
		'	If clear Then content = content.TrimFull

		'	' 最大允许长度小于实际长度则需要剪裁内容
		'	Dim itemLen = content.Length
		'	If maxLen < itemLen Then content = content.ShortShow(maxLen)

		'	RetList.Add(item.Key, content)

		'	' 文档长度少于 5 个则不再处理，防止过长文字 ShortShow 剪裁时出现内容过短
		'	maxLen -= itemLen
		'	If maxLen < 5 Then Exit For
		'Next

		For Each item In dic.OrderByDescending(Function(x) x.Value.Mark)
			Dim content = item.Value.Content
			If clear Then content = content.TrimFull

			' 最大允许长度小于实际长度跳过词条数据
			Dim itemLen = content.Length
			If maxLen >= itemLen Then
				RetList.Add(item.Key, content)
				maxLen -= itemLen
			End If

			' 文档长度少于 5 个则不再处理，防止过长文字 ShortShow 剪裁时出现内容过短
			If maxLen < 5 Then Exit For
		Next



		'Dim orderList = RetList.OrderBy(Function(x) x.Key)
		'Dim ret = orderList.Select(Function(x) x.Value).JoinString(vbCrLf)
		'If ret.Length <= max Then
		'	ret = text.ShortShow(max, " …… ")
		'Else
		'	' 检查头尾，看看谁更大
		'	Dim s = dic(orderList.First.Key).Mark
		'	Dim e = dic(orderList.Last.Key).Mark

		'	max -= 2

		'	If s > e Then
		'		ret = ret.Left(max) & "……"
		'	ElseIf s < e Then
		'		ret = "……" & ret.Right(max)
		'	Else
		'		ret = ret.ShortShow(max, " …… ")
		'	End If
		'End If
		'If clear Then ret = ret.TrimFull

		Dim ret = RetList.OrderBy(Function(x) x.Key).Select(Function(x) x.Value).JoinString("")

		If clear Then ret = ret.TrimFull

		Return ret
	End Function

	''' <summary>划分句子，系统将忽略多余空格，HTML会清理标签</summary>
	Private Shared Function GetSentence(text As String) As IEnumerable(Of String)
		If text.IsEmpty Then Return Nothing

		Dim Ret As New List(Of String)

		Dim sp = Guid.NewGuid.ToString
		For Each s In {".", "。", "!", "！", "?", "？", "…", vbCr, vbLf}
			text = text.Replace(s, s & sp)
		Next

		Dim Sents = text.SplitEx(sp, SplitEnum.REMOVE_EMPTY_ENTRIES Or SplitEnum.CLEAR_HTML)

		' 处理小数点的问题
		' ssxx.xxss 如：今天收入8888.88元
		' x.ssssss 如：1. 第一步
		If Sents.NotEmpty Then
			For I = 0 To Sents.Length - 1
				Dim s = Sents(I)

				If s.NotEmpty Then
					If s.Length > 1 AndAlso s.EndsWith(".") Then
						Dim Flag = False

						Dim sb = s.Substring(0, s.Length - 1)
						If sb.IsUInt Then
							' 1.
							' xxx.ssss
							Flag = True
						Else
							sb = sb.Substring(sb.Length - 1)
							If sb.IsUInt AndAlso I < Sents.Length - 2 Then
								' xxx12.234xxx
								' sssx.xxsss
								' 检查下一段是否开始也是数字
								Dim sn = Sents(I + 1)
								If sn.NotEmpty AndAlso sn.Substring(0, 1).IsUInt Then
									Flag = True
								End If
							End If
						End If

						If Flag AndAlso I < Sents.Length - 2 Then
							s &= Sents(I + 1)
							I += 1
						End If
					End If

					Ret.Add(s)
				End If
			Next
		End If

		Return Ret
	End Function

#End Region

#Region "词性"

	'符号	词性	相关解释
	'Ag	形语素	形容词性语素。形容词代码为 a，语素代码ｇ前面置以A。
	'a	形容词	取英语形容词 adjective的第1个字母。
	'ad	副形词	直接作状语的形容词。形容词代码 a和副词代码d并在一起。
	'an	名形词	具有名词功能的形容词。形容词代码 a和名词代码n并在一起。
	'b	区别词	取汉字“别”的声母。
	'c	连词	取英语连词 conjunction的第1个字母。
	'dg	副语素	副词性语素。副词代码为 d，语素代码ｇ前面置以D。
	'd	副词	取 adverb的第2个字母，因其第1个字母已用于形容词。
	'e	叹词	取英语叹词 exclamation的第1个字母。
	'f	方位词	取汉字“方”
	'g	语素	绝大多数语素都能作为合成词的“词根”，取汉字“根”的声母。
	'h	前接成分	取英语 head的第1个字母。
	'i	成语	取英语成语 idiom的第1个字母。
	'j	简称略语	取汉字“简”的声母。
	'k	后接成分	
	'l	习用语	习用语尚未成为成语，有点“临时性”，取“临”的声母。
	'm	数词	取英语 numeral的第3个字母，n，u已有他用。
	'Ng	名语素	名词性语素。名词代码为 n，语素代码ｇ前面置以N。
	'n	名词	取英语名词 noun的第1个字母。
	'nr	人名	名词代码 n和“人(ren)”的声母并在一起。
	'nrgt	古代人名。
	'ns	地名	名词代码 n和处所词代码s并在一起。
	'nt	机构团体	“团”的声母为 t，名词代码n和t并在一起。
	'nz	其他专名	“专”的声母的第 1个字母为z，名词代码n和z并在一起。
	'o	拟声词	取英语拟声词 onomatopoeia的第1个字母。
	'p	介词	取英语介词 prepositional的第1个字母。
	'q	量词	取英语 quantity的第1个字母。
	'r	代词	取英语代词 pronoun的第2个字母,因p已用于介词。
	's	处所词	取英语 space的第1个字母。
	'tg	时语素	时间词性语素。时间词代码为 t,在语素的代码g前面置以T。
	't	时间词	取英语 time的第1个字母。
	'u	助词	取英语助词 auxiliary
	'vg	动语素	动词性语素。动词代码为 v。在语素的代码g前面置以V。
	'v	动词	取英语动词 verb的第一个字母。
	'vd	副动词	直接作状语的动词。动词和副词的代码并在一起。
	'vn	名动词	指具有名词功能的动词。动词和名词的代码并在一起。
	'w	标点符号	
	'x	非语素字	非语素字只是一个符号，字母 x通常用于代表未知数、符号。
	'y	语气词	取汉字“语”的声母。
	'z	状态词	取汉字“状”的声母的前一个字母。
	'un	未知词	不可识别词及用户自定义词组。取英文Unkonwn首两个字母。(非北大标准，CSW分词中定义)

	'附：结巴分词词性对照表（按词性英文首字母排序）

	'形容词(1个一类，4个二类)
	'a 形容词
	'ad 副形词
	'an 名形词
	'ag 形容词性语素
	'al 形容词性惯用语

	'区别词(1个一类，2个二类)
	'b 区别词
	'bl 区别词性惯用语

	'连词(1个一类，1个二类)
	'c 连词
	'cc 并列连词

	'副词(1个一类)
	'd 副词

	'叹词(1个一类)
	'e 叹词

	'方位词(1个一类)
	'f 方位词

	'前缀(1个一类)
	'h 前缀

	'后缀(1个一类)
	'k 后缀

	'数词(1个一类，1个二类)
	'm 数词
	'mq 数量词

	'名词 (1个一类，7个二类，5个三类)
	'名词分为以下子类：
	'n 名词
	'nr 人名
	'nr1 汉语姓氏
	'nr2 汉语名字
	'nrj 日语人名
	'nrf 音译人名
	'ns 地名
	'nsf 音译地名
	'nt 机构团体名
	'nz 其它专名
	'nl 名词性惯用语
	'ng 名词性语素

	'拟声词(1个一类)
	'o 拟声词

	'介词(1个一类，2个二类)
	'p 介词
	'pba 介词“把”
	'pbei 介词“被”

	'量词(1个一类，2个二类)
	'q 量词
	'qv 动量词
	'qt 时量词

	'代词(1个一类，4个二类，6个三类)
	'r 代词
	'rr 人称代词
	'rz 指示代词
	'rzt 时间指示代词
	'rzs 处所指示代词
	'rzv 谓词性指示代词
	'ry 疑问代词
	'ryt 时间疑问代词
	'rys 处所疑问代词
	'ryv 谓词性疑问代词
	'rg 代词性语素

	'处所词(1个一类)
	's 处所词

	'时间词(1个一类，1个二类)
	't 时间词

	'tg 时间词性语素

	'助词(1个一类，15个二类)
	'u 助词
	'uzhe 着
	'ule 了 喽
	'uguo 过
	'ude1 的 底
	'ude2 地
	'ude3 得
	'usuo 所
	'udeng 等 等等 云云
	'uyy 一样 一般 似的 般
	'udh 的话
	'uls 来讲 来说 而言 说来
	'uzhi 之
	'ulian 连（“连小学生都会”）

	'动词(1个一类，9个二类)
	'v 动词
	'vd 副动词
	'vn 名动词
	'vshi 动词“是”
	'vyou 动词“有”
	'vf 趋向动词
	'vx 形式动词
	'vi 不及物动词（内动词）
	'vl 动词性惯用语
	'vg 动词性语素

	'标点符号(1个一类，16个二类)
	'w 标点符号
	'wkz 左括号， 全角 ： （ 〔 ［ ｛ 《 【 〖 〈 半角：( [ { <
	'wky 右括号， 全角 ： ） 〕 ］ ｝ 》 】 〗 〉 半角： ) ] { >
	'wyz 左引号， 全角 ： “ ‘ 『
	'wyy 右引号，全角：” ' 』
	'wj 句号， 全角 ： 。
	'ww 问号， 全角 ： ？ 半角 ： ?
	'wt 叹号， 全角 ： !半角 ： !
	'wd 逗号， 全角 ： ， 半角：,
	'wf 分号， 全角 ： ； 半角： ;
	'wn 顿号， 全角 ： 、
	'wm 冒号， 全角 ：  半角： :
	'ws 省略号， 全角 ： …… …
	'wp 破折号， 全角 ： —— -- ——- 半角：— ----
	'wb 百分号千分号， 全角 ： ％ ‰ 半角：%
	'wh 单位符号， 全角 ： ￥ ＄ ￡ ° ℃ 半角：$

	'字符串(1个一类，2个二类)
	'x 字符串
	'xx 非语素字
	'xu 网址URL

	'语气词(1个一类)
	'y 语气词(delete yg)

	'状态词(1个一类)
	'z 状态词
#End Region

End Class
