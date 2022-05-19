using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Stacy.Core.Connection
{
	public class WebRequestFactory
	{
		public string BaseUrl { get; set; }

		public WebRequestFactory SetUrl(string baseUrl)
		{
			BaseUrl = baseUrl;
			return this;
		}

		public WebResponse Post(string path, object formData)
		{
			var request = (HttpWebRequest)WebRequest.Create(BaseUrl+path);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.KeepAlive = true;
			request.AllowAutoRedirect = false;
			
			var postBody = GetPostBody(formData);
			request.ContentLength = postBody.Length;

			var dataStream = request.GetRequestStream();
			dataStream.Write(postBody,0,postBody.Length);
			dataStream.Close();

			return request.GetResponse();
		}

		private static byte[] GetPostBody(object formObj)
		{
			var props = formObj.GetType().GetProperties();
			var postData = String.Join("&", props.Select(p => p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(formObj, null).ToString())));
			return Encoding.UTF8.GetBytes(postData);
		}
	}
}
