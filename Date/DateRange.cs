using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Date
{
	public class DateRange
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int NumberOfNights { get { return (EndDate.Date - StartDate.Date).Days; } }

		public TimeSpan DateSpan { get { return EndDate - StartDate; } }

		public List<DateTime> DateList
		{
			get
			{
				return Enumerable.Range(0, NumberOfNights)
					.Select(d => StartDate.AddDays(d))
					.ToList();
			}
		}

		public IEnumerable<DayOfWeek> DaysOfWeek
		{
			get
			{
				var days = EndDate.Subtract(StartDate).Days;
				for (var d = 0; d < days; d++)
					yield return StartDate.AddDays(d).DayOfWeek;
			}
		}

		public DateRange() : this(DateTime.Now, DateTime.Now) { }
		public DateRange(DateTime startDate, int days) : this(startDate, startDate.AddDays(days)) { }

		public DateRange(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public bool ContainsDate(DateTime date)
		{
			return date.Date >= StartDate.Date && date.Date <= EndDate.Date;
		}

		public bool Equals(DateRange compare)
		{
			return compare.StartDate.Date == StartDate.Date && compare.EndDate.Date == EndDate.Date;
		}

		public DateRange AddDate(DateTime date)
		{
			if (date.Date < StartDate.Date)
				StartDate = date;
			if (date.Date > EndDate.Date)
				EndDate = date;
			return this;
		}
	}

	public class DateRange<TEntity> : DateRange
	{
		public List<TEntity> Data { get; set; }

		public DateRange() : this(DateTime.Now, DateTime.Now) { }

		public DateRange(DateTime startDate, DateTime endDate) : this(startDate, endDate, new List<TEntity>()) { }

		public DateRange(IEnumerable<TEntity> data) : this(DateTime.Now, DateTime.Now, data) { }

		public DateRange(DateTime startDate, DateTime endDate, IEnumerable<TEntity> data) :base(startDate,endDate)
		{
			Data = data.ToList();
		}
	}
}
