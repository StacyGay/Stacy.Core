using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Stacy.Core.Data;
using Microsoft.AspNetCore.Http;

namespace Stacy.Core.Connection
{
    public class ContextDetailUtilities
    {
        public static string GetContextDetails(HttpContext context)
        {
            var headers = context.Request.Headers.Keys.ToDictionary(k => k, k => context.Request.Headers[k]);

            string body;
            try
            {
                using (var reader = new StreamReader(context.Request.Body))
                    body = reader.ReadToEnd();
            }
            catch (Exception)
            {
                body = "";
            }

            var details = new
            {
                Headers = headers,
                Body = body,
                Url = context.Request.Path,
                Method = context.Request.Method
            };

            return details.ToJson();
        }
    }
}
