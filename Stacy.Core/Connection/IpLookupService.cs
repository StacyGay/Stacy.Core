using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Stacy.Core.Connection
{
    public class IpLookupService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpLookupService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetRequestIP(bool tryUseXForwardHeader = true)
        {
            try
            {
                string ip = null;

                // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

                // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
                // for 99% of cases however it has been suggested that a better (although tedious)
                // approach might be to read each IP from right to left and use the first public IP.
                // http://stackoverflow.com/a/43554000/538763
                //
                if (tryUseXForwardHeader)
                {
                    var forwarded = GetHeaderValueAs("X-Forwarded-For");

                    if (!string.IsNullOrEmpty(forwarded))
                    {
                        ip = forwarded
                            .TrimEnd(',')
                            .Split(',')
                            .Select(s => s.Trim())
                            .ToList()
                            .FirstOrDefault();
                    }
                }

                // Check for cf-connecting-ip
                if (string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(GetHeaderValueAs("CF-Connecting-IP")))
                    ip = GetHeaderValueAs("CF-Connecting-IP");

                // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
                if (string.IsNullOrEmpty(ip) && _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
                    ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                if (string.IsNullOrEmpty(ip))
                    ip = GetHeaderValueAs("REMOTE_ADDR");

                // _httpContextAccessor.HttpContext?.Request?.Host this is the local host.

                return ip;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private string GetHeaderValueAs(string headerName)
        {
            if (_httpContextAccessor.HttpContext?.Request?.Headers?.ContainsKey(headerName) ?? false)
            {
                var rawValues = _httpContextAccessor.HttpContext.Request.Headers[headerName].ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                    return rawValues;
            }
            return "";
        }
    }
}
