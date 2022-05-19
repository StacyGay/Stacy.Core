using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;

namespace Stacy.Core.Date
{
	public class DateRange
	{
		public enum Relation { Before = -2, OverlapStart = -1, Overlap = 0, OverlapEnd = 1, After = 2 }

	    [NotMapped] [XmlIgnore] private DateTime _startDate;
        [NotMapped] [XmlIgnore] private DateTime _endDate;

        public DateTime StartDate { get { return _startDate; } set { _startDate = value.Date.SpecifyDateOnly(); } }
		public DateTime EndDate { get { return _endDate; } set { _endDate = value.Date.SpecifyDateOnly(); } }
        public int NumberOfNights
		{
			get
			{
				return (EndDate.Date - StartDate.Date).Days;
			}
		}

		public TimeSpan DateSpan
		{
			get
			{
				return EndDate - StartDate;
			}
		}

		public List<DateTime> DateList
		{
			get
			{
				if (StartDate.Date == EndDate.Date)
					return new List<DateTime> { StartDate.Date };

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

		public Relation RangeRelation(DateRange compare)
		{
			if (compare.EndDate <= StartDate)
				return Relation.Before;
			if(compare.StartDate >= EndDate)
				return Relation.After;
			if (compare.StartDate <= StartDate && compare.EndDate >= EndDate)
				return Relation.Overlap;
			if (compare.EndDate < EndDate && compare.StartDate < StartDate)
				return Relation.OverlapStart;
			return Relation.OverlapEnd;
		}

		public DateRange AddDate(DateTime date)
		{
			if (date.Date < StartDate.Date)
				StartDate = date;
			if (date.Date > EndDate.Date)
				EndDate = date;
			return this;
		}

		public DateRange AddDays(int days)
		{
			return new DateRange(StartDate.AddDays(days), EndDate.AddDays(days));
		}

		public DateRange AddMonths(int months)
		{
			return new DateRange(StartDate.AddMonths(months), EndDate.AddMonths(months));
		}

		public DateRange AddYears(int years)
		{
			return new DateRange(StartDate.AddYears(years), EndDate.AddYears(years));
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
