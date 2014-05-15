using Microsoft.Practices.Unity;

namespace Stacy.Core.Service
{
	class DiServiceBehavior: IServiceBehavior
	{
		private readonly IUnityContainer _container;

		public DiServiceBehavior(IUnityContainer container)
		{
			_container = container;
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
		{
			
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
		{
			foreach (var cdb in serviceHostBase.ChannelDispatchers)
			{
				var cd = cdb as ChannelDispatcher;

				if (cd != null)
				{
					foreach (var ed in cd.Endpoints)
					{
						ed.DispatchRuntime.InstanceProvider = new DiInstanceProvider(serviceDescription.ServiceType, _container);
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
		{

		}
	}
}
