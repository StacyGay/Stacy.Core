using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Stacy.Core.Debug;

namespace Stacy.Core.Service
{
	public class SoapConnect
	{
		public SoapConnect(bool boolUseSSL, string strServiceAddress, string strPort = "80", string strSecurePort = "443")
		{
			useSSL = boolUseSSL;
			serviceAddress = strServiceAddress;
			port = strPort;
			securePort = strSecurePort;
		}

		public bool useSSL { get; set; }
		public string serviceAddress { get; set; }
		public string port { get; set; }
		public string securePort { get; set; }

		private XmlDocument CreateSoapEnvelope(string xml)
		{
			//string returnXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:web=\"http://iqwareinc.com/WebRes\"><soap:Body>";
			//returnXML += xml + "</soap:Body></soap:Envelope>";
			//return returnXML;

			var soapEnvelope = new XmlDocument();
			soapEnvelope.LoadXml(
				"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
				xml + "</s:Body></s:Envelope>");
			return soapEnvelope;
		}

		public DataSet ConnectService(string methodName, string xmlToSend)
		{
			string addressToUse = "";
			var returnData = new DataSet(methodName);

			if (useSSL)
			{
				string[] addressParts = serviceAddress.Split('/');
				addressParts[0] += ":" + securePort;
				serviceAddress = String.Join("/", addressParts);
				addressToUse = "https://" + serviceAddress;
			}
			else
			{
				string[] addressParts = serviceAddress.Split('/');
				addressParts[0] += ":" + port;
				serviceAddress = String.Join("/", addressParts);
				addressToUse = "http://" + serviceAddress;
			}

			XmlDocument soapRequest = CreateSoapEnvelope(xmlToSend);

			var debug = new DebugLog("IQWareRateSync", DebugLog.OutputType.Email);
			debug.SetEMailAddress("stacy.gay@fuelinteractive.com");
			debug.debugOn = true;
			debug.Log(soapRequest.OuterXml, true);

			byte[] requestBytes = Encoding.ASCII.GetBytes(soapRequest.OuterXml);
			var request = (HttpWebRequest) WebRequest.Create(addressToUse);
			request.Method = "POST";
			request.ContentLength = requestBytes.Length;
			request.Accept = "XML";
			request.Headers.Add("SOAPAction: \"" + methodName + "\"");
			request.ContentType = "text/xml;charset=utf-8";

			using (Stream requestStream = request.GetRequestStream())
			{
				requestStream.Write(requestBytes, 0, requestBytes.Length);
			}


			using (var response = (HttpWebResponse) request.GetResponse())
			{
				//StreamReader stReader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);
				//string responseString = stReader.ReadToEnd();

				//return responseString;

				returnData.ReadXml(response.GetResponseStream());
			}

			return returnData;
		}
	}
}