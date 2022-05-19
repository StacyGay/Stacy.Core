using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stacy.Core.Types
{
	public class PhoneNumber
	{
		public string Raw { get; set; }
		public string Formatted { get; set; }
		public string AsString { get { return Formatted; } }

		public long AsNumber
		{
			get
			{
				long phoneNo;
				return long.TryParse(Raw, out phoneNo) ? phoneNo : 0;
			}
		}

		public PhoneNumber(string phone = "")
		{
			Raw = phone;
			Formatted = Format(phone);
		}

		public override string ToString()
		{
			return AsString;
		}

		public static string Format(string phone)
		{
			if (phone.Contains("-"))
				return phone;

			long phoneNo;
			if (!long.TryParse(phone.Trim(), out phoneNo))
				return phone;

			return String.Format("{0:(###) ###-####}", phoneNo);
		}
	}
}
