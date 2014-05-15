using System;
using Microsoft.Practices.Unity;

namespace Stacy.Core.Service
{
	class DiInstanceProvider: IInstanceProvider
	{
		private readonly Type _serviceType;
		private readonly IUnityContainer _container;

		public DiInstanceProvider(Type serviceType, IUnityContainer container)
		{
			_container = container;
			_serviceType = serviceType;
		}

		public object GetInstance(System.ServiceModel.InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
		{
			return _container.Resolve(_serviceType);
		}

		public object GetInstance(System.ServiceModel.InstanceContext instanceContext)
		{
			return GetInstance(instanceContext, null);
		}

		public void ReleaseInstance(System.ServiceModel.InstanceContext instanceContext, object instance)
		{
		}
	}
}
