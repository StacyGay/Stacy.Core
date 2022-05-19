using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Stacy.Core.Connection
{
    public static class HttpClientExtension
    {
        public static TResponse RestGet<TResponse>(this HttpClient client, string path)
            where TResponse : class
        {
            var responseTask = client.GetAsync(path);
            var resultTask = responseTask.ContinueWith(t =>
            {
                t.Result.EnsureSuccessStatusCode();
                return t.Result.Content.ReadAsStringAsync();
            })
                .Unwrap()
                .ContinueWith(t => Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TResponse>(t.Result)))
                .Unwrap();
            resultTask.Wait();

            return resultTask.Result;
        }

        /*public  static TResponse RestPost<TPost, TResponse>(this HttpClient client, string path, TPost request)
			where TResponse : class
			where TPost : class
		{
			var responseTask = client.PostAsJsonAsync(path, request);
			var resultTask = responseTask.ContinueWith(t =>
			{
				t.Result.EnsureSuccessStatusCode();
				return t.Result.Content.ReadAsStringAsync();
			})
				.Unwrap()
				.ContinueWith(t => Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TResponse>(t.Result)))
				.Unwrap();
			resultTask.Wait();

			return resultTask.Result;
		}*/

        public static TResponse RestPost<TResponse>(this HttpClient client, string path, string content = null, bool isJsonContentType = false)
            where TResponse : class
        {
            StringContent contentItem = new StringContent("");

            if (!string.IsNullOrEmpty(content))
            {
                if (isJsonContentType)
                {
                    contentItem = new StringContent(content, Encoding.UTF8, "application/json");
                }
                else
                {
                    contentItem = new StringContent(content);
                }
            }

            var responseTask = client.PostAsync(new Uri(client.BaseAddress + path), contentItem);

            var resultTask = responseTask.ContinueWith(t =>
            {
                t.Result.EnsureSuccessStatusCode();
                return t.Result.Content.ReadAsStringAsync();
            })
                .Unwrap()
                .ContinueWith(t => Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TResponse>(t.Result)))
                .Unwrap();
            resultTask.Wait();

            return resultTask.Result;
        }

        public static TResponse RestPut<TResponse>(this HttpClient client, string path, string content = null, bool isJsonContentType = false)
            where TResponse : class
        {
            StringContent contentItem = new StringContent("");

            if (!string.IsNullOrEmpty(content))
            {
                if (isJsonContentType)
                {
                    contentItem = new StringContent(content, Encoding.UTF8, "application/json");
                }
                else
                {
                    contentItem = new StringContent(content);
                }
            }

            var responseTask = client.PutAsync(new Uri(client.BaseAddress + path), contentItem);
            var resultTask = responseTask.ContinueWith(t =>
            {
                t.Result.EnsureSuccessStatusCode();
                return t.Result.Content.ReadAsStringAsync();
            })
                .Unwrap()
                .ContinueWith(t => Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TResponse>(t.Result)))
                .Unwrap();
            resultTask.Wait();

            return resultTask.Result;
        }

        public static TResponse RestDelete<TResponse>(this HttpClient client, string path)
            where TResponse : class
        {
            var responseTask = client.DeleteAsync(path);
            var resultTask = responseTask.ContinueWith(t =>
            {
                t.Result.EnsureSuccessStatusCode();
                return t.Result.Content.ReadAsStringAsync();
            })
                .Unwrap()
                .ContinueWith(t => Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TResponse>(t.Result)))
                .Unwrap();
            resultTask.Wait();

            return resultTask.Result;
        }
    }
}
