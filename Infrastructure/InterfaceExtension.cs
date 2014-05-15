using Microsoft.Practices.Unity;

namespace Stacy.Core.Infrastructure
{
	public abstract class InterfaceExtension: UnityContainerExtension
	{
		protected override void Initialize()
		{
			if (!ValidExtension()) return;

			LoadDependencies();
			LoadExtensions();
		}

		public abstract bool ValidExtension();

		public abstract void LoadExtensions();

		public abstract void LoadDependencies();
	}
}
