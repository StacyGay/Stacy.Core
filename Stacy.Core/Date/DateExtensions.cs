using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stacy.Core.Date
{
	public static class DateExtensions
	{
		public static DateTime? NullIfZero(this DateTime when)
		{
			return when == DateTime.MinValue ? (DateTime?) null : when;
		}

		public static bool IsMinValue(this DateTime when)
		{
			if (when == null)
				return true;

			return when < new DateTime(1950, 1, 1);
		}

	    public static DateTime SpecifyDateOnly(this DateTime date)
	    {
	        return DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
	    }

        public static DateTime? SpecifyDateOnly(this DateTime? date)
        {
            if (date == null)
                return null;

            return DateTime.SpecifyKind(date.Value, DateTimeKind.Unspecified);
        }
    }
}
