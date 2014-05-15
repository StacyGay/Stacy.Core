using System.Linq;
using Microsoft.Practices.Unity;

namespace Stacy.Core.Infrastructure
{
	public class ExtensionLoader
	{
		private readonly IUnityContainer _container;
		private readonly IClientRepository _clientRepo;

		public ExtensionLoader(IUnityContainer container, IClientRepository clientRepo)
		{
			_clientRepo = clientRepo;
			_container = container;
		}

		public void LoadExtensions(string clientName, int? packageId)
		{
			var client = _clientRepo.GetClient(clientName);
			if (client == null)
				return;

			packageId = packageId == 0 ? (client.systemPackage ?? client.packages.FirstOrDefault().Value).ID : packageId;
			var package = client.packages[packageId ?? client.systemPackage.ID] ?? client.systemPackage;

			_container.RegisterInstance(client);
			_container.RegisterInstance(package);

			if (package.modules == null)
				package.Init();

			var modules = _container.ResolveAll<InterfaceExtension>().ToList();
			modules.ForEach(module => _container.AddExtension(module));
		}
	}
}
