using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stacy.Core.Data
{
	public class JsonDecimalConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(decimal) || objectType == typeof(decimal?));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
			{
				return token.ToObject<decimal>();
			}
			if (token.Type == JTokenType.String)
			{
				// customize this to suit your needs
				return decimal.Parse(token.ToString(),
					   CultureInfo.InvariantCulture);
			}
			if (token.Type == JTokenType.Null && objectType == typeof(decimal?))
			{
				return null;
			}
			throw new JsonSerializationException("Unexpected token type: " +
												  token.Type.ToString());
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Decimal? d = default(Decimal?);
			if (value != null)
			{
				d = value as Decimal?;
				if (d.HasValue) // If value was a decimal?, then this is possible
				{
					d = new Decimal?(new Decimal(Decimal.ToDouble(d.Value))); // The ToDouble-conversion removes all unnessecary precision
				}
			}
			JToken.FromObject(d).WriteTo(writer);
		}
	}
}
