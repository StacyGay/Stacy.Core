using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stacy.Core.Data
{
	public class JsonIntConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(int));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jsonValue = serializer.Deserialize<JValue>(reader);

			switch (jsonValue.Type)
			{
				case JTokenType.Float:
					return (int)Math.Round(jsonValue.Value<double>());
				case JTokenType.Integer:
					return jsonValue.Value<int>();
			}

			throw new FormatException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var d = default(int?);
			if (value != null)
			{
				d = value as int?;
			}
			JToken.FromObject(d).WriteTo(writer);
		}
	}
}
