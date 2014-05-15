using System;
using Microsoft.Practices.Unity;

namespace Stacy.Core.Service
{
	class DiServiceHost: ServiceHost
	{
		private readonly IUnityContainer _container;

		public DiServiceHost(Type serviceType, Uri[] baseAddresses, IUnityContainer container)
			: base(serviceType, baseAddresses)
		{
			_container = container;
		}

		protected override void OnOpen(TimeSpan timeout)
		{
			Description.Behaviors.Add(new DiServiceBehavior(_container));
			base.OnOpen(timeout);
		}
	}
}
