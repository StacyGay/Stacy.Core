using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Stacy.Core.Data;

namespace Stacy.Core.Connection
{
    public abstract class ManualSoapService
    {

        protected ManualSoapService()
        {
        }

        public abstract string Header { get; set; }
        public abstract string Footer { get; set; }
        public abstract string WebserviceUrl { get; set; }
        public abstract string LoggingTable { get; set; }
        public abstract bool EnableLogging { get; set; }
        public abstract AuthenticationHeaderValue Authorization { get; set; }
        public abstract int Timeout { get; set; }
        public string LastRequest { get; set; }
        public string LastResponse { get; set; }


        public async Task<T> Request<T>(IManualSoapRequest request)
        {
            var httpClient = new HttpClient {
                BaseAddress = new Uri(WebserviceUrl),
                Timeout = TimeSpan.FromSeconds(Timeout)
            };
            httpClient.DefaultRequestHeaders.Add("SOAPAction", request.SoapAction);

            if (Authorization != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = Authorization;
            }

            var xmlRequest = GetRequestXml(request);
            LastRequest = xmlRequest;

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, WebserviceUrl)
            {
                Content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml")
            };

            var startTime = DateTime.Now;
            var logItem = new ManualSoapLog
            {
                WebserviceUrl = WebserviceUrl,
                Operation = request.SoapAction,
                Request = xmlRequest,
                Response = ""
            };

            try
            {
                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();
                LastResponse = responseContent;
                var endTime = DateTime.Now;

                if (EnableLogging && !string.IsNullOrEmpty(LoggingTable))
                {
                    logItem.Response = responseContent;
                    logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                    Log(logItem);
                }

                return DeserializeInnerSoapObject<T>(responseContent);
            }
            catch (WebException ex)
            {
                var endTime = DateTime.Now;
                if (!EnableLogging || string.IsNullOrEmpty(LoggingTable))
                {
                    logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                    logItem.StackTrace = ex.StackTrace;
                    Log(logItem);
                    throw ex;
                }

                using (var responseStream = new StreamReader(ex.Response.GetResponseStream()))
                {
                    logItem.Response = responseStream.ReadToEnd();
                    logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                    logItem.StackTrace = ex.StackTrace;
                    Log(logItem);
                }

                throw ex;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.Now;
                logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                logItem.StackTrace = ex.StackTrace;
                Log(logItem);

                throw ex;
            }
        }

        public async Task<string> Request(string content, string soapAction)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(WebserviceUrl),
                Timeout = TimeSpan.FromSeconds(Timeout)
            };
            httpClient.DefaultRequestHeaders.Add("SOAPAction", soapAction);

            LastRequest = content;

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, WebserviceUrl)
            {
                Content = new StringContent(content, Encoding.UTF8, @"text/xml")
            };

            var startTime = DateTime.Now;
            var logItem = new ManualSoapLog
            {
                WebserviceUrl = WebserviceUrl,
                Operation = soapAction,
                Request = content,
                Response = ""
            };

            try
            {
                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();
                LastResponse = responseContent;
                var endTime = DateTime.Now;

                if (EnableLogging && !string.IsNullOrEmpty(LoggingTable))
                {
                    logItem.Response = responseContent;
                    logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                    Log(logItem);
                }

                return responseContent;
            }
            catch (WebException ex)
            {
                var endTime = DateTime.Now;
                if (!EnableLogging || string.IsNullOrEmpty(LoggingTable))
                {
                    logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                    logItem.StackTrace = ex.StackTrace;
                    Log(logItem);
                    throw ex;
                }

                using (var responseStream = new StreamReader(ex.Response.GetResponseStream()))
                {
                    logItem.Response = responseStream.ReadToEnd();
                    logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                    logItem.StackTrace = ex.StackTrace;
                    Log(logItem);
                }

                throw ex;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.Now;
                logItem.ElapsedSeconds = (endTime - startTime).TotalSeconds;
                logItem.StackTrace = ex.StackTrace;
                Log(logItem);

                throw ex;
            }
        }
        private string GetRequestXml<T>(T request) where T : IManualSoapRequest
        {
            return 
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope 
    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
    xmlns:wsa=""http://www.w3.org/2005/08/addressing"" 
    xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" 
    xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
    <env:Header xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">
        {Header}
    </env:Header>
    <soap:Body>
        {request.ToXmlNode()}
    </soap:Body>
</soap:Envelope>";
        }

        private static T DeserializeInnerSoapObject<T>(string soapResponse)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(soapResponse);

            var soapBody = xmlDocument.GetElementsByTagName("soap:Body")[0];

            return Regex.Replace(soapBody.InnerXml, @"xmlns=""[^\""]*""", "").DeserializeXml<T>();
        }

        public class ManualSoapLog
        {
            public string WebserviceUrl { get; set; }
            public string Operation { get; set; }
            public string Request { get; set; }
            public string Response { get; set; }
            public string StackTrace { get; set; }
            public double ElapsedSeconds { get; set; }
        }

        public void Log(ManualSoapLog log)
        {
            // TODO: Add default logging implementation
        }
    }
}

