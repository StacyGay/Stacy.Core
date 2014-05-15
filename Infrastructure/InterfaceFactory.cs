using System;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Stacy.Core.Packages;

namespace Stacy.Core.Infrastructure
{
	public abstract class InterfaceFactory<TFactory, TExtension>
		where TFactory : InterfaceFactory<TFactory, TExtension>
		where TExtension : UnityContainerExtension
	{
		private IUnityContainer _container;
		private IClient _client;
		private IPackage _package;

		protected InterfaceFactory(IUnityContainer container)
		{
			_container = container.CreateChildContainer();
			
			if (!container.IsRegistered<IClient>())
				return;
			
			_client = container.Resolve<IClient>();
			_package = container.IsRegistered<IPackage>() 
				? container.Resolve<IPackage>() 
				: _package = _client.systemPackage ?? _client.packages.FirstOrDefault().Value;

			LoadExtensions();	
			
		}

		public TFactory GetNewFactoryInstance()
		{
			return (TFactory)Activator.CreateInstance(typeof(TFactory), new object[] { _container.Parent });
		}

		public TFactory GetNewFactoryInstance(IClient client, IPackage package = null)
		{
			var instance = GetNewFactoryInstance();
			instance.Configure(client, package);
			return instance;
		}

		private void LoadExtensions()
		{
			_container.RegisterInstance(_client, new ExternallyControlledLifetimeManager());
			if(_package != null)
				_container.RegisterInstance(_package, new ExternallyControlledLifetimeManager());

			var extensions = _container.ResolveAll<TExtension>().ToList();
			extensions.ForEach(extension => _container.AddExtension(extension));
		}

		public void Configure(IClient client, IPackage package = null)
		{
			if (_client == null || client.clientName != _client.clientName
				|| (package != null && package.ID != _package.ID))
			{
				_client = client;
				_package = package ?? _client.systemPackage;
				var parentContainer = _container.Parent;
				_container.Dispose();
				_container = parentContainer.CreateChildContainer();
				LoadExtensions();
			}
		}

		public T GetService<T>(IClient client, IPackage package = null)
		{
			Configure(client, package);

			return GetService<T>();
		}

		public T GetService<T>()
		{
			if(_client == null)
				throw new DependencyMissingException("InterfaceFactory has not been loaded with a client");
			try
			{
				return _container.Resolve<T>();

			}
			catch(Exception ex)
			{
				throw new DependencyMissingException("InterfaceFactory cannot resolve type: " + typeof(T).FullName, ex);	
			}
		}

		public void ReleaseInstance<T>()
		{
			var instance = _container.Registrations.FirstOrDefault(r => r.RegisteredType == typeof (T));
			if(instance != null)
				instance.LifetimeManager.RemoveValue();
		}
	}
}
