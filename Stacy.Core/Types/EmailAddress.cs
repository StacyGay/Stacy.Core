using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stacy.Core.Types
{
	public class EmailAddress
	{
		public static bool IsValid(string emailAddress)
		{
            if (string.IsNullOrEmpty(emailAddress))
                return false;

			return Regex.IsMatch(emailAddress,
                @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
		}
	}
}
