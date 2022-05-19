using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Stacy.Core.Connection
{
	public class InspectorBehavior : IEndpointBehavior
	{
		private readonly ICommunicationLogger _logger;
		public MInspector mInspector { get; set; }

		public InspectorBehavior(ICommunicationLogger logger = null)
		{
			_logger = logger;
            mInspector = new MInspector(_logger);
        }

        public void AddLogger(ICommunicationLogger logger)
        {
            mInspector.Loggers.Add(logger);
        }

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			// No implementation necessary
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRunTime)
		{
			clientRunTime.ClientMessageInspectors.Add(mInspector);
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			// No implementation necessary
		}

		public void Validate(ServiceEndpoint endpoint)
		{
			// No implementation necessary
		}
	}
}