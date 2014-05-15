using System;
using System.Collections.Generic;

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
	}
}