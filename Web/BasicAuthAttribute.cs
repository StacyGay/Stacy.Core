using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Stacy.Core.Web
{
    public class BasicAuthAttribute : ActionFilterAttribute
    {
        private const string AuthenticationHeaderName = "Authorization";
        private readonly Type _authType;

        public BasicAuthAttribute(Type authType)
        {
            _authType = authType;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {

	        if (Authenticate(actionContext))
		        return;
            
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            actionContext.Response = response;
            
        }

        private bool Authenticate(HttpActionContext actionContext)
        {
            var authMethod = (IBasicAuthMethod)Activator.CreateInstance(_authType);
            var headers = actionContext.Request.Headers;

            var authenticationString = GetHttpRequestHeader(headers, AuthenticationHeaderName);
            if (string.IsNullOrEmpty(authenticationString))
                return false;

			var authenticationStringParts = authenticationString.Split(' ');

			if (authenticationStringParts.Length != 2)
				return false;

	        authenticationString = authenticationStringParts[1];

	        var encoding = Encoding.GetEncoding("iso-8859-1");
	        var authDecoded = encoding.GetString(Convert.FromBase64String(authenticationString));

			var authenticationParts = authDecoded.Split(new[] { ":" },
                    StringSplitOptions.RemoveEmptyEntries);

            if (authenticationParts.Length != 2)
                return false;

            var username = authenticationParts[0].Trim();
            var password = authenticationParts[1].Trim();

            return authMethod.Authorize(username, password);
        }

        private static string GetHttpRequestHeader(HttpHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
                return string.Empty;

            return Enumerable.SingleOrDefault<string>(headers.GetValues(headerName));
        }
    }

    public interface IBasicAuthMethod
    {
        bool Authorize(string username, string password);
    }
}
