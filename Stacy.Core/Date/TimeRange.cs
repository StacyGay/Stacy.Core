using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Date
{
	public class TimeRange
	{
		public enum Relation { Before = -2, OverlapStart = -1, Overlap = 0, OverlapEnd = 1, After = 2 }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }

		public TimeRange() : this(DateTime.Now, DateTime.Now) { }

		public TimeRange(DateTime startTime, DateTime endTime) : this(startTime.TimeOfDay, endTime.TimeOfDay) { }

		public TimeRange(TimeSpan startTime, TimeSpan endTime)
		{
			StartTime = startTime;
			EndTime = endTime;
		}

		public TimeSpan TimeSpan
		{
			get { return EndTime - StartTime; }
		}

		public bool ContainsTime(TimeSpan time)
		{
			return StartTime <= time && time <= EndTime;
		}

		public bool ContainsTime(DateTime time)
		{
			return ContainsTime(time.TimeOfDay);
		}

		public bool Equals(TimeRange compareRange)
		{
			return StartTime == compareRange.StartTime && EndTime == compareRange.EndTime;
		}

		public Relation RangeRelation(TimeRange compare)
		{
			if (compare.StartTime <= StartTime)
				return Relation.Before;
			if (compare.StartTime >= EndTime)
				return Relation.After;
			if (compare.StartTime <= StartTime && compare.EndTime >= EndTime)
				return Relation.Overlap;
			if (compare.EndTime < EndTime && compare.StartTime < StartTime)
				return Relation.OverlapStart;
			return Relation.OverlapEnd;
		}
	}

	public class TimeRange<TEntity> : TimeRange
	{
		public List<TEntity> Data { get; set; }

		public TimeRange() : this(DateTime.Now, DateTime.Now) { }

		public TimeRange(DateTime startTime, DateTime endTime) : this(startTime.TimeOfDay, endTime.TimeOfDay) { }

		public TimeRange(TimeSpan startTime, TimeSpan endTime) : this(startTime, endTime, new List<TEntity>()) { }

		public TimeRange(IEnumerable<TEntity> data) : this(DateTime.Now, DateTime.Now, data) { }

		public TimeRange(DateTime startTime, DateTime endTime, IEnumerable<TEntity> data) : this(startTime.TimeOfDay,endTime.TimeOfDay,data)
		{
		}

		public TimeRange(TimeSpan startTime, TimeSpan endTime, IEnumerable<TEntity> data) :  base(startTime,endTime)
		{
			Data = data.ToList();
		}
	}
}
