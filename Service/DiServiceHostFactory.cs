using System;
using Microsoft.Practices.Unity;

namespace Stacy.Core.Service
{
	public class DiServiceHostFactory : ServiceHostFactory
	{
		private readonly IUnityContainer _container;

		public DiServiceHostFactory(IUnityContainer container)
		{
			_container = container;
		}

		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			_container.RegisterType(serviceType);

			return new DiServiceHost(serviceType, baseAddresses, _container);
		}
	}
}
