using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Stacy.Core.Data
{
	public static class DataExtensions
	{
		public static TResult IfNotNull<TSource, TResult>(this TSource source, Func<TSource, TResult> accessor, TResult @default = default(TResult))
			where TSource : class
		{
			return source != null
				? accessor(source)
				: @default;
		}

		public static DataTable ToDataTable<T>(this IEnumerable<T> items)
		{
			PropertyInfo[] props = typeof (T).GetProperties();

			var dt = new DataTable();
			dt.Columns.AddRange(
				props.Select(p => new DataColumn(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)).ToArray()
				);

			items.ToList().ForEach(
				i => dt.Rows.Add(props.Select(p => p.GetValue(i, null) ?? DBNull.Value).ToArray()));

			return dt;
		}

		public static string ToXml(this Object obj)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (TextWriter streamWriter = new StreamWriter(memoryStream))
				{
					try
					{
						var xmlSerializer = new XmlSerializer(obj.GetType());
						xmlSerializer.Serialize(streamWriter, obj);
						return Encoding.UTF8.GetString(memoryStream.ToArray());
					}
					catch (Exception e)
					{
						return "<ObjectSerializerError>" + e + "</ObjectSerializerError>";
					}
				}
			}
		}

		public static string ToXmlNode(this Object obj)
		{
			return obj.ToXml().Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", "");
		}

		public static string ToJson(this Object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public static string ToJson<K, V>(this Dictionary<K, V> obj)
		{
			/*var jDict = new JsonDictionary<K, V>();
			foreach (K key in obj.Keys)
				jDict.Add(key, obj[key]);*/

			string json = "{ ";
			foreach (K key in obj.Keys)
			{
				json += "\"" + key + "\": " + obj[key].ToJson() + ", ";
			}
			json += " }";
			return json;
		}

	    public static string StripHtml(this string str)
	    {
            return str == null ? null : Regex.Replace(str, "<.*?>", string.Empty);
	    }
	}

	[Serializable]
	public class JsonDictionary<K, V> : ISerializable
	{
		private readonly Dictionary<K, V> dict = new Dictionary<K, V>();

		public JsonDictionary()
		{
		}

		protected JsonDictionary(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		public V this[K index]
		{
			set { dict[index] = value; }
			get { return dict[index]; }
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach (K key in dict.Keys)
			{
				info.AddValue(key.ToString(), dict[key]);
			}
		}

		public void Add(K key, V value)
		{
			dict.Add(key, value);
		}
	}
}