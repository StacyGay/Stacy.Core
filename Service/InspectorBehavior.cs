namespace Stacy.Core.Service
{
	public class InspectorBehavior : IEndpointBehavior
	{
		public MInspector mInspector { get; set; }

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			// No implementation necessary
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRunTime)
		{
			mInspector = new MInspector();
			clientRunTime.MessageInspectors.Add(mInspector);
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