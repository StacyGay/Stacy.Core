using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel;
using System.Xml;

namespace Stacy.Core.Connection
{
	public class ConnectService<TClass, TInterface> : IConnectService<TClass, TInterface>
		where TClass : ClientBase<TInterface>, TInterface, new()
		where TInterface : class
	{
		public string addressToUse = "";
		public BasicHttpBinding bindingSetup;
		public InspectorBehavior inspector = new InspectorBehavior();
		public TClass service = new TClass();

        public TClass Service
        {
            get { return service; }
            set { service = value; }
        }

		public ConnectService()
		{
			Init();
		}

		public string GetXMLReply()
		{
			return inspector.mInspector.XmlResponse;
		}

		public string GetXMLRequest()
		{
			return inspector.mInspector.XmlRequest;
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
				bindingSetup.ReceiveTimeout = new TimeSpan(0, 20, 0);
				bindingSetup.SendTimeout = new TimeSpan(0, 20, 0);
				bindingSetup.MaxReceivedMessageSize = int.MaxValue;
				bindingSetup.MaxBufferSize = int.MaxValue;
				bindingSetup.MaxBufferPoolSize = int.MaxValue;
				bindingSetup.ReaderQuotas.MaxArrayLength = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxBytesPerRead = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxNameTableCharCount = int.MaxValue/2;
				bindingSetup.ReaderQuotas.MaxDepth = 64;

				// setup new endpoint address
				service.Endpoint.Address = new EndpointAddress(addressToUse);
				service.Endpoint.EndpointBehaviors.Add(inspector);
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
			return inspector.mInspector.XmlResponse;
		}

		public string GetInspectorRequest()
		{
			return inspector.mInspector.XmlRequest;
		}

		public void logXML(string ecrmClientID, DateTime WhenSent, string receiveStatus, bool isError)
		{
			// TODO: Set up default log behavior
		}

		public void Init()
		{
			// TODO: Set up default endpoint
			// SetEndpoint(true, @"");
		}

		public static ArrayList consolidateDataSet(DataSet ds, bool singleChildRow)
		{
			var returnArray = new ArrayList();
			var rootTable = new DataTable();

			// find rootTable in relations
			foreach (DataTable t in ds.Tables)
				if (t.ParentRelations.Count == 0)
					rootTable = t;

			foreach (DataRow r in rootTable.Rows)
			{
				if (singleChildRow)
					returnArray.Add(GetChildFirstRow(r));
				else
					returnArray.Add(GetChildData(r));
			}

			return returnArray;
		}

		public static Hashtable GetChildFirstRow(DataRow r)
		{
			var returnTable = new Hashtable();
			// first add row columns
			foreach (DataColumn c in r.Table.Columns)
				returnTable.Add(c.ColumnName, r[c]);

			// now get child ArrayLists
			foreach (DataRelation dr in r.Table.ChildRelations)
			{
				DataRow[] childRows = r.GetChildRows(dr);
				if (childRows.Length > 0)
				{
					var childData = new Hashtable();
					childData = GetChildFirstRow(childRows[0]);
					foreach (DictionaryEntry de in childData)
						if (!returnTable.ContainsKey(de.Key.ToString()))
							returnTable[de.Key] = de.Value;
				}
			}

			return returnTable;
		}

		public static Hashtable GetChildData(DataRow r)
		{
			var returnTable = new Hashtable();
			// first add row columns
			foreach (DataColumn c in r.Table.Columns)
				returnTable.Add(c.ColumnName, r[c]);

			// now get child ArrayLists
			foreach (DataRelation dr in r.Table.ChildRelations)
			{
				var currentChildData = new ArrayList();
				foreach (DataRow cr in r.GetChildRows(dr))
				{
					currentChildData.Add(GetChildData(cr));
				}
				returnTable.Add(dr.ChildTable.TableName, currentChildData);
			}

			return returnTable;
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

	public class ConnectServiceException : Exception
	{
		public ConnectServiceException(string errorMessage) : base(errorMessage)
		{
		}

		public ConnectServiceException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx)
		{
		}
	}
}