using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Web
{
	public class FilterIpAddressAttribute : AuthorizeAttribute
	{
		private readonly IEnumerable<string> _allowedIpList;

		public FilterIpAddressAttribute(string ipAddresses)
		{
			_allowedIpList = ipAddresses.Split(',');
		}

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext == null || httpContext.Request == null || httpContext.Request.UserHostAddress == null)
				throw new ArgumentNullException();

			return _allowedIpList.Contains(httpContext.Request.UserHostAddress);
		}
	}
}
