using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

namespace Stacy.Core.Connection
{
    public class MInspector : IClientMessageInspector
    {
        public string XmlResponse = "";
        public string XmlRequest = "";
        private string _previousRequest = "";
        public readonly List<ICommunicationLogger> Loggers = new List<ICommunicationLogger>();

        public Dictionary<string, List<CommunicationLog>> OperationLogs { get; set; } = new Dictionary<string, List<CommunicationLog>>();
        public List<CommunicationLog> Logs { get; set; } = new List<CommunicationLog>();

        public MInspector()
        {

        }

        public MInspector(ICommunicationLogger logger)
        {
            if (logger != null)
                Loggers.Add(logger);
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var action = request.Headers.Action;
            var operation = request.Headers.Action.Substring(action.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);

            var loggingData = new CommunicationLog
            {
                
                StackTrace = Environment.StackTrace,
                Operation = operation,
                RequestWhen = DateTime.Now,
                Request = request.ToString(),
                Response = "Connection Failed",
                StatusCode = ((int)HttpStatusCode.BadGateway).ToString()
                    + " - " + HttpStatusCode.BadGateway.ToString(),
            };

            loggingData.Retry = !string.IsNullOrEmpty(_previousRequest) && string.Equals(loggingData.Request, _previousRequest);

            _previousRequest = XmlRequest = loggingData.Request;

            if (!OperationLogs.ContainsKey(operation))
            {
                OperationLogs[operation] = new List<CommunicationLog>();
            }

            OperationLogs[operation].Add(loggingData);
            Logs.Add(loggingData);

            if (Loggers.Any())
            {
                Task.Factory.StartNew(() => Loggers.ForEach(l => l.LogRequest(loggingData)), TaskCreationOptions.LongRunning);
            }

            return loggingData;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var loggingData = correlationState as CommunicationLog;

            if (loggingData == null)
                return;

            XmlRequest = loggingData.Request;
            XmlResponse = loggingData.Response = reply.ToString();
            loggingData.StackTrace = Environment.StackTrace;
            loggingData.ResponseWhen = DateTime.Now;

            var httpResponseProperty = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
            loggingData.StatusCode = ((int)httpResponseProperty.StatusCode).ToString()
                + " - " + httpResponseProperty.StatusCode.ToString();

            if (Loggers.Any())
            {
                Task.Factory.StartNew(() => Loggers.ForEach(l => l.LogResponse(loggingData)), TaskCreationOptions.LongRunning);
            }
        }
    }
}