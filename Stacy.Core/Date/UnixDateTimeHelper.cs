﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stacy.Core.Date
{
	public static class UnixDateTimeHelper
	{
		private const string InvalidUnixEpochErrorMessage = "Unix epoc starts January 1st, 1970";
		/// <summary>
		///   Convert a long into a DateTime
		/// </summary>
		public static DateTime FromUnixTime(this Int64 self)
		{
			var ret = new DateTime(1970, 1, 1);
			return ret.AddSeconds(self);
		}

		/// <summary>
		///   Convert a DateTime into a long
		/// </summary>
		public static Int64 ToUnixTime(this DateTime self)
		{

			if (self == DateTime.MinValue)
			{
				return 0;
			}

			var epoc = new DateTime(1970, 1, 1);
			var delta = self - epoc;

			if (delta.TotalSeconds < 0) throw new ArgumentOutOfRangeException(InvalidUnixEpochErrorMessage);

			return (long)delta.TotalSeconds;
		}
	}
}
