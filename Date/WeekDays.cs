using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Date
{
	public class WeekDays : List<DayOfWeek>
	{
		public WeekDays(IEnumerable<int> days)
		{
			AddRange(days.Select(d => (DayOfWeek)d));
		}

		public WeekDays(string dayList)
		{
			AddRange(dayList.Split().Select(d => (DayOfWeek)int.Parse(d)));
		}

		public bool ContainsDate(DateTime date)
		{
			return Contains(date.DayOfWeek);
		}
	}
}
