using Stacy.Core.Data;
using Stacy.Core.Debug;
using Stacy.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Stacy.Core.Connection
{
    // initial implementation handles unclosed connections
    // work to add more reusable connection factory methods moving forward

    public class ConnectionConfig
    {
        public string WebserviceUrl { get; set; }
        public bool WebserviceUseSsl { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool KeepAlive { get; set; } = true;
    }

    public class ConnectionFactory : IDisposable
    {
        public List<ICommunicationObject> Clients { get; set; } = new List<ICommunicationObject>();
        public ConnectionConfig ConnectionConfig { get; set; }

        public InspectorBehavior Inspector { get; set; }
        public bool EnableLogging { get; set; } = false;
        public bool DisableInspector { get; set; } = false;
        public List<ICommunicationLogger> CommunicationLoggers { get; set; } = new List<ICommunicationLogger>();

        public string XmlRequest => Inspector?.mInspector?.XmlRequest ?? "";

        public string XmlResponse => Inspector?.mInspector?.XmlResponse ?? "";

        public Dictionary<string, List<CommunicationLog>> OperationLogs
            => Inspector?.mInspector?.OperationLogs ?? new Dictionary<string, List<CommunicationLog>>();
        public List<CommunicationLog> Logs
            => Inspector?.mInspector?.Logs ?? new List<CommunicationLog>();

        public string TransactionJson => new { XmlRequest, XmlResponse }.ToJson();

        public TimeSpan Timeout
        {
            get
            {
                return ConnectionConfig.Timeout;
            }

            set
            {
                ConnectionConfig.Timeout = value;
            }
        }

        public bool KeepAlive
        {
            get
            {
                return ConnectionConfig.KeepAlive;
            }

            set
            {
                ConnectionConfig.KeepAlive = value;
            }
        }

        public TC GetWebServiceClient<TC, TI>() 
            where TC : ClientBase<TI>, ICommunicationObject, new()
            where TI : class
        {
            if (ConnectionConfig == null)
                throw new ArgumentException("ConnectionConfig has not correctly been defined by this interface: " + GetType().Name);

            if (string.IsNullOrEmpty(ConnectionConfig.WebserviceUrl))
                throw new CoreInternalException("No endpoint has been configured");

            var httpSecurityMode = ConnectionConfig.WebserviceUseSsl ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None;
            var binding = new BasicHttpBinding(httpSecurityMode);


            // setup new binding settings, still tinkering with these values (security vs large return sizes)
            binding.SendTimeout = ConnectionConfig.Timeout;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue / 2;
            binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue / 2;
            binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue / 2;
            binding.ReaderQuotas.MaxDepth = 64;

            var customBinding = new CustomBinding(binding);
            var transportElement = customBinding.Elements.Find<HttpTransportBindingElement>();
            transportElement.KeepAliveEnabled = ConnectionConfig.KeepAlive;

            var endpoint = new EndpointAddress(ConnectionConfig.WebserviceUrl);
            // var client =  new TC(customBinding, endpoint);
            var client = (TC)Activator.CreateInstance(typeof(TC), new object[] { customBinding, endpoint });
            Clients.Add(client);

            if (Inspector == null)
            {
                Inspector = new InspectorBehavior();
            }

            if (EnableLogging)
            {
                foreach (var logger in CommunicationLoggers)
                {
                    logger.ServiceType = client.GetType().FullName;
                    Inspector.AddLogger(logger);
                }
            }

            if (!DisableInspector)
                client.Endpoint.EndpointBehaviors.Add(Inspector);

            return client;
        }

        public void Dispose()
        {
            try
            {
                foreach (var client in Clients)
                {
                    if (client == null)
                        continue;

                    if (client.State == CommunicationState.Faulted)
                        client.Abort();

                    client.Close();

                    var disposable = client as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                Clients.RemoveAll(c => true);
            }
            catch (Exception)
            {
                // TODO: Add logging
            }
        }
    }
}
