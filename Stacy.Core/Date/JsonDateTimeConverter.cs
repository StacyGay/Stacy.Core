using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Stacy.Core.Date
{
	public class JsonDateTimeConverter : IsoDateTimeConverter
	{
		//private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

		public JsonDateTimeConverter()
		{
			DateTimeFormat = "yyyy-MM-dd";
		}

		public JsonDateTimeConverter(string format)
		{
			if (!string.IsNullOrEmpty(format))
				DateTimeFormat = format;

			DateTimeFormat = "yyyy-MM-dd";
		}

		/*public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			bool nullable = objectType.IsNullableType();

			if (reader.TokenType == JsonToken.Null)
			{
				if (!nullable)
					throw new JsonSerializationException($"Cannot convert null value to {objectType}.");

				return null;
			}

			if (reader.TokenType == JsonToken.Date)
			{
				return reader.Value;
			}

			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException($"Unexpected token parsing date. Expected String, got {reader.TokenType}.");

			string dateText = reader.Value.ToString();

			if (string.IsNullOrEmpty(dateText) && nullable)
				return null;

			
			DateTime result;
			if(DateTime.TryParseExact(dateText, DateTimeFormat, Culture, _dateTimeStyles, out result))
				return result;
			else
				return DateTime.Parse(dateText, Culture, _dateTimeStyles);
		}*/
	}
}
