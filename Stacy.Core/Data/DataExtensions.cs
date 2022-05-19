using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
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

		public static List<dynamic> ToDynamic(this DataTable dt)
		{
			var dynamicDt = new List<dynamic>();
			foreach (DataRow row in dt.Rows)
			{
				dynamic dyn = new ExpandoObject();
				foreach (DataColumn column in dt.Columns)
				{
					var dic = (IDictionary<string, object>)dyn;
					dic[column.ColumnName] = row[column];
					dynamicDt.Add(dyn);
				}
			}
			return dynamicDt;
		}

		public static List<T> ToObjectList<T>(this DataTable dt) where T : new()
		{
			var type = typeof (T);
			var properties = type.GetProperties();
			var result = new List<T>();
			foreach (DataRow row in dt.Rows)
			{
				var item = new T();
				foreach (var property in properties)
					foreach (DataColumn column in dt.Columns)
						if (property.Name.ToLower().Equals(column.ColumnName.ToLower()))
						{
							var value = row[column.ColumnName];
							if (value is DBNull)
								continue;

							try
							{
								property.SetValue(item, value, null);
							}
							catch (Exception ex)
							{
								var message = String.Format("Error converting column {0}: {1}", column.ColumnName, ex.Message);
								throw new Exception(message, ex);
							}

							break;
						}
				result.Add(item);
			}
			return result;
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

	    public static T DeserializeXml<T>(this string xml)
	    {
            var serializer = new XmlSerializer(typeof(T));
            var memStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            return (T)serializer.Deserialize(memStream);
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
						var namespaces = new XmlSerializerNamespaces();
						namespaces.Add("","");
						xmlSerializer.Serialize(streamWriter, obj, namespaces);
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

		public static string ToJson(this Object obj, bool format = false)
		{
			return JsonConvert.SerializeObject(obj, format ? Formatting.Indented : Formatting.None, 
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
		}

        public static T DeserializeJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
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

		/// <summary>
		/// Returns the first few characters of the string with a length
		/// specified by the given parameter. If the string's length is less than the 
		/// given length the complete string is returned. If length is zero or 
		/// less an empty string is returned
		/// </summary>
		/// <param name="s">the string to process</param>
		/// <param name="length">Number of characters to return</param>
		/// <returns></returns>
		public static string Left(this string s, int length)
		{
			length = Math.Max(length, 0);

			if (string.IsNullOrEmpty(s))
				return s;

			return s.Length > length ? s.Substring(0, length) : s;
		}

		public static Dictionary<string, object> ToDictionary(this DataRow row)
		{
			var dict = new Dictionary<string, object>();

			foreach (DataColumn column in row.Table.Columns)
			{
				dict.Add(column.ColumnName, row[column.ColumnName]);
			}

			return dict;
		}

        /// <summary>
        /// Dirty hack for checking if IDataRecord has a column
        /// </summary>
        /// <param name="r"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool HasColumn(this IDataRecord r, string columnName)
        {
            try
            {
                return r.GetOrdinal(columnName) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two objects and returns the differences
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static List<Variance> Compare<T>(this T val1, T val2)
        {
            var variances = new List<Variance>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var v = new Variance
                {
                    PropertyName = property.Name,
                    ValA = property.GetValue(val1),
                    ValB = property.GetValue(val2)
                };

                if (v.ValA == null && v.ValB == null)
                    continue;

                if ((v.ValA == null && v.ValB != null) ||
                    (v.ValA != null && v.ValB == null))
                {
                    variances.Add(v);
                    continue;
                }

                if (!v.ValA.Equals(v.ValB))
                    variances.Add(v);
            }

            return variances;
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

    public class Variance
    {
        public string PropertyName { get; set; }
        public object ValA { get; set; }
        public object ValB { get; set; }
    }
}