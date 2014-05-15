using System;
using Microsoft.Practices.Unity;

namespace Stacy.Core.Infrastructure
{
	public class PerSessionLifetimeManager : LifetimeManager, IDisposable
	{
		private readonly string _key = new Guid().ToString();

		private bool SessionKeyExists()
		{
			return HttpContext.Current != null && HttpContext.Current.Session != null
				&& HttpContext.Current.Session[_key] != null;
		}

		public override object GetValue()
		{
			if (SessionKeyExists())
				return HttpContext.Current.Session[_key];
			else
				return null;
		}

		public override void RemoveValue()
		{
			if (SessionKeyExists())
				HttpContext.Current.Session.Remove(_key);
		}

		public override void SetValue(object newValue)
		{
			if (HttpContext.Current != null && HttpContext.Current.Session != null)
				HttpContext.Current.Session[_key] = newValue;
		}

		public void Dispose()
		{
			RemoveValue();
		}
	}
}
