using System;
using System.ServiceModel;

namespace Stacy.Core.Connection
{
	public abstract class SoapConnectionFactory<TClass, TInterface> : ISoapConnectionFactory<TClass, TInterface>
		where TClass : ClientBase<TInterface>, TInterface, new()
		where TInterface : class
	{
		public virtual string ServiceUrl { get { return "http://dummy.url/"; } }
		public TClass Connection { get; private set; }
		public bool EnableLogging;
		private InspectorBehavior _inspector;

		public virtual ICommunicationLogger Logger { get { return null; } }

		public string Request
		{
			get { return _inspector != null ? _inspector.mInspector.XmlRequest : ""; }
		}

		public string Response
		{
			get { return _inspector != null ? _inspector.mInspector.XmlResponse : ""; }
		}

		public virtual TClass GetConnection()
		{
			// Create the binding  
			var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
			{
				ReceiveTimeout = new TimeSpan(0, 20, 0),
				SendTimeout = new TimeSpan(0, 20, 0),
				MaxReceivedMessageSize = int.MaxValue,
				MaxBufferSize = int.MaxValue,
				MaxBufferPoolSize = int.MaxValue,
			};

			binding.ReaderQuotas.MaxArrayLength = int.MaxValue/2;
			binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue/2;
			binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue/2;
			binding.ReaderQuotas.MaxDepth = 64;

			// Set the transport security to UsernameOverTransport for Plaintext usernames  
			var endpoint = new EndpointAddress(ServiceUrl);

			Connection = (TClass) Activator.CreateInstance(typeof (TClass), binding, endpoint);

			_inspector = new InspectorBehavior(Logger);

			Connection.Endpoint.EndpointBehaviors.Add(_inspector);

			return Connection;
		}
	}
}
