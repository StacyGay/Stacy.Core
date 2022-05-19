using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Stacy.Core.Date;

namespace Stacy.Core.Personal
{
	public class Person
	{
		public string Email { get; set; }
		public Name Name { get; set; }
		public DateTime DateOfBirth { get; set; }
		public MailAddress MailingAddress { get; set; }
		public MailAddress BillingAddress { get; set; }
		public MailAddress ShippingAddress { get; set; }
	}
}
