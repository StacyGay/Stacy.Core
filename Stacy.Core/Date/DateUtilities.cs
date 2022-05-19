using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace Stacy.Core.Date
{
	public static class DateUtilities
	{
		/// <summary>
		///     Allows use of foreach to loop through a start and ending date by day. Optionaly provide the number of days to add. Example: foreach (DateTime day in EachDay(StartDate, EndDate))
		/// </summary>
		/// <param name="from"></param>
		/// <param name="thru"></param>
		/// <returns></returns>
		public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru, int step = 1)
		{
			for (DateTime day = from.Date; day.Date <= thru.Date; day = day.AddDays(step))
			{
				yield return day;
			}
		}

		public static bool IsValidSqlDatetime(DateTime date)
		{
			var minDateTime = DateTime.MaxValue;
			var maxDateTime = DateTime.MinValue;

			minDateTime = new DateTime(1753, 1, 1);
			maxDateTime = new DateTime(9999, 12, 31, 23, 59, 59, 997);

			return date >= minDateTime && date <= maxDateTime;
		}

		public static bool IsValidSqlDatetime(string date)
		{
			DateTime testDate;
			return DateTime.TryParse(date, out testDate) && IsValidSqlDatetime(testDate);
		}

        public static List<DateRange> DatesToDateRanges(List<DateTime> dates)
        {
            var dateRanges = new List<DateRange>();

            if (dates == null || !dates.Any())
                return dateRanges;

            DateTime? startDate = dates.First();
            DateTime? tempDate = null;

            dates.Sort();

            foreach (var date in dates)
            {
                if (tempDate != null && (date - tempDate.Value).Days > 1)
                {
                    dateRanges.Add(new DateRange(startDate.Value, tempDate.Value));
                    startDate = date;
                }

                tempDate = date;
            }

            if (startDate != null && tempDate != null)
                dateRanges.Add(new DateRange(startDate.Value, tempDate.Value));

            return dateRanges;
        }
	}
}