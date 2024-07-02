/* ------------------------------------------------------------
 * 
 * 	Copyright © 2021 湖南大沥网络科技有限公司.
 * 	Dali.Utils Is licensed under Mulan PSL v2.
 * 
 * 		  author:	木炭(WOODCOAL)
 * 		   email:	i@woodcoal.cn
 * 		homepage:	http://www.hunandali.com/
 * 
 * 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
 * 
 * ------------------------------------------------------------
 * 
 * 	JSON 自定义转换
 * 
 * 	name: Misc.JsonConverterBase
 * 	create: 2024-01-10
 * 	memo: JSON 自定义转换，对于 Object 类型数据，比如用户提交的数据类型为 Object 时如果不做自定义解析有可能无法识别
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaLi.Utils.Misc {

	/// <summary>JSON 自定义转换</summary>
	public abstract class JsonConverterBase : JsonConverter<object> {

		/// <summary>反序列化</summary>
		protected abstract object Read(JsonElement value);

		/// <summary>读取，已经处理基本类型 true, false, null</summary>
		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			switch (reader.TokenType) {
				// Case JsonTokenType.StartObject
				// ' 令牌类型是 JSON 对象的开头。

				// Case JsonTokenType.EndObject
				// ' 令牌类型是 JSON 对象的结尾。

				// Case JsonTokenType.StartArray
				// ' 令牌类型是 JSON 数组的开头。

				// Case JsonTokenType.EndArray
				// ' 令牌类型是 JSON 数组的结尾。

				case JsonTokenType.None: {
						// 没有值（不同于 Null）。 如果没有数据由 Utf8JsonReader 读取，则这是默认标记类型。
						return null;
					}

				case JsonTokenType.Number: {
						// ' 令牌类型是 JSON 数字。

						if (reader.TryGetByte(out byte b)) {
							return b;
						}
						if (reader.TryGetInt16(out short s)) {
							return s;
						}
						if (reader.TryGetInt32(out int i)) {
							return i;
						}
						if (reader.TryGetInt64(out long l)) {
							return l;
						}
						if (reader.TryGetSingle(out float f)) {
							return f;
						}
						if (reader.TryGetDouble(out double d)) {
							return d;
						}

						return reader.GetDecimal();
					}

				//case JsonTokenType.String: {
				//		// ' 令牌类型是 JSON 字符串。

				//		if (reader.TryGetDateTime(out DateTime d)) {
				//			return d;
				//		}
				//		if (reader.TryGetGuid(out Guid g)) {
				//			return g;
				//		}

				//		return reader.GetString()!;
				//	}

				case JsonTokenType.True: {
						// 令牌类型是 JSON 文本 true。
						return true;
					}

				case JsonTokenType.False: {
						// 令牌类型是 JSON 文本 false。
						return false;
					}

				case JsonTokenType.Null: {
						// 令牌类型是 JSON 文本 null。
						return null;
					}

				default: {
						JsonElement value = JsonDocument.ParseValue(ref reader).RootElement.Clone();
						return Read(value);
					}
			}

		}
	}
}