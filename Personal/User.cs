using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stacy.Core.Personal
{
	public class User : Person
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}
