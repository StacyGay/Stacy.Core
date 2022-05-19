using System;
using System.Collections.Generic;
using System.Linq;

namespace Stacy.Core.Date
{
	public class EntityCalendar<TEntity> 
		where TEntity: IDated, new()
	{
		public List<EntityDate<TEntity>> Dates;
		public readonly IEnumerable<TEntity> Data;

		public EntityCalendar(IEnumerable<TEntity> data)
		{
			Data = data;
			Dates = Data
				.GroupBy(d => d.Date)
				.Select(g => new EntityDate<TEntity> { Date = g.Key, Entities = g.Select(d => d)})
				.ToList();
		}

		public EntityMonth<TEntity> GetMonth(int year = 0, int month = 0)
		{
			year = year < DateTime.Now.AddYears(-1).Year ? DateTime.Now.Year : year;
			month = month < 0 || month > 12 ? DateTime.Now.Month : month;

			var monthDateRange 
				= new DateRange(new DateTime(year, month, 1), new DateTime(year, month, DateTime.DaysInMonth(year, month),23,59,59));

			var daysInMonth = DateTime.DaysInMonth(year, month);
			var dates = Enumerable.Range(1, daysInMonth)
				.Select(d =>
				{
					var currentDate = new DateTime(year, month, d);
					return new EntityDate<TEntity>
					{
						Date = currentDate,
						Entities = Data.Where(e => e.Date.Date == currentDate)
					};
				}).ToList();

			DateTime minDate;
			while ((minDate = dates.Min(d => d.Date).Date).DayOfWeek != DayOfWeek.Sunday)
			{
				dates.Add(new EntityDate<TEntity> { Date = minDate.AddDays(-1) });
			}

			DateTime maxDate;
			while ((maxDate = dates.Max(d => d.Date).Date).DayOfWeek != DayOfWeek.Saturday)
			{
				dates.Add(new EntityDate<TEntity> { Date = maxDate.AddDays(1) });
			}

			var entityMonth = new EntityMonth<TEntity>
			{
				Month = month,
				Year = year,
				DateRange = monthDateRange,
				Data = Data.Where(e => e.Date >= monthDateRange.StartDate && e.Date <= monthDateRange.EndDate)
			};
			var currentWeek = new EntityWeek<TEntity> { Week = 1 };
			var index = 0;
			dates.OrderBy(d => d.Date).ToList().ForEach(d =>
			{
				index++;
				currentWeek.Days.Add(d);

				if (index < 7)
					return;
				
				index = 0;
				currentWeek.Days = currentWeek.Days.OrderBy(day => day.Date).ToList();
				currentWeek.DateRange = new DateRange(currentWeek.Days.Min(day => day.Date), currentWeek.Days.Max(day => day.Date));
				currentWeek.Data = currentWeek.Days.SelectMany(day => day.Entities);
				entityMonth.Weeks.Add(currentWeek);
				currentWeek = new EntityWeek<TEntity> { Week = entityMonth.Weeks.Count + 1};
			});

			return entityMonth;
		}
	}

	public class EntityMonth<TEntity>
		where TEntity : IDated, new()
	{
		public int Month { get; set; }
		public int Year { get; set; }
		public DateRange DateRange { get; set; }
		public List<EntityWeek<TEntity>> Weeks { get; set; }
		public IEnumerable<TEntity> Data { get; set; }

		public EntityMonth()
		{
			Weeks = new List<EntityWeek<TEntity>>();
		}
	}

	public class EntityWeek<TEntity>
		where TEntity : IDated, new()
	{
		public int Week { get; set; }
		public List<EntityDate<TEntity>> Days { get; set; }
		public IEnumerable<TEntity> Data { get; set; }
		public DateRange DateRange { get; set; }

		public EntityWeek()
		{
			Days = new List<EntityDate<TEntity>>();
		}

		public EntityDate<TEntity> GetDayOfWeek(DayOfWeek dayOfWeek)
		{
			return Days.FirstOrDefault(d => d.Date.DayOfWeek == dayOfWeek);
		}
	}

	public class EntityDate<TEntity>
		where TEntity : IDated, new()
	{
		public DateTime Date { get; set; }
		public IEnumerable<TEntity> Entities { get; set; }

		public EntityDate()
		{
			Date = new DateTime();
			Entities = new List<TEntity>();
		}
	}
}
