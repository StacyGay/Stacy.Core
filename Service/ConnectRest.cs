using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Stacy.Core.Service
{
	public class ConnectRest<TClass, TInterface> : IConnectService<TClass, TInterface>
		where TClass : ClientBase<TInterface>, TInterface, new()
		where TInterface : class
	{
		public string addressToUse = "";
		public BasicHttpBinding bindingSetup;
		public InspectorBehavior inspector = new InspectorBehavior();
		public TClass service = new TClass();

		public string GetXMLReply()
		{
			return inspector.mInspector.xmlReply;
		}

		public string GetXMLRequest()
		{
			return inspector.mInspector.xmlRequest;
		}

		public IConnectService<TClass, TInterface> SetEndpoint(bool boolUseSSL, string strServiceAddress, string port = "80",
		                                                       string securePort = "443")
		{
			try
			{
				service = new TClass();
				inspector = new InspectorBehavior();

				if (boolUseSSL)
				{
					string[] addressParts = strServiceAddress.Split('/');
					addressParts[0] += ":" + securePort;
					strServiceAddress = String.Join("/", addressParts);
					addressToUse = "https://" + strServiceAddress;
					bindingSetup = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
				}
				else
				{
					string[] addressParts = strServiceAddress.Split('/');
					addressParts[0] += ":" + port;
					strServiceAddress = String.Join("/", addressParts);
					addressToUse = "http://" + strServiceAddress;
					bindingSetup = new BasicHttpBinding(BasicHttpSecurityMode.None);
				}

				// setup new binding settings, still tinkering with these values (security vs large return sizes)
				bindingSetup.ReceiveTimeout = new TimeSpan(0, 10, 0);
				bindingSetup.SendTimeout = new TimeSpan(0, 10, 0);
				bindingSetup.MaxReceivedMessageSize = int.MaxValue/2;
				bindingSetup.MaxBufferSize = int.MaxValue/2;
				bindingSetup.MaxBufferPoolSize = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxArrayLength = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxBytesPerRead = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxNameTableCharCount = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxDepth = 64;

				// setup new endpoint address
				service.Endpoint.Address = new EndpointAddress(addressToUse);
				service.Endpoint.Behaviors.Add(inspector);
				service.Endpoint.Binding = bindingSetup;
			}
			catch (Exception e)
			{
				throw new ConnectServiceException("Error setting new endpoint address: " + e.Message, e);
			}
			return this;
		}

		public string GetInspectorReply()
		{
			return inspector.mInspector.xmlReply;
		}

		public string GetInspectorRequest()
		{
			return inspector.mInspector.xmlRequest;
		}

		public void logXML(string ecrmClientID, DateTime WhenSent, string receiveStatus, bool isError)
		{
			// TODO: add implementation
			throw new NotImplementedException();
		}

		public static ArrayList manualXMLParsing(string xml, string startTag)
		{
			var returnVar = new ArrayList();
			Hashtable elements;

			var doc = new XmlDocument();
			doc.LoadXml(xml);

			XmlNodeList root = doc.GetElementsByTagName(startTag);

			for (int i = 0; i < root.Count; i++)
			{
				elements = new Hashtable();
				string currentElement = "";
				string instanceXML = "<NewRoot>" + root[i].InnerXml + "</NewRoot>";
				using (XmlReader reader = XmlReader.Create(new StringReader(instanceXML)))
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element)
							currentElement = reader.Name;
						if (reader.NodeType == XmlNodeType.Text && !elements.ContainsKey(currentElement))
							elements.Add(currentElement, reader.Value);
					}
				returnVar.Add(elements);
			}

			return returnVar;
		}
	}
}