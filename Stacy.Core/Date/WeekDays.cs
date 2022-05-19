using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Stacy.Core.Data;
using MoreLinq;

namespace Stacy.Core.Date
{
    [Flags]
    public enum WeekDays : byte
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekends = Saturday | Sunday,
        All = Weekdays | Weekends
    }

    public static class WeekDaysExtensions
    {
        public static WeekDays FromCollection(IEnumerable<int> days)
        {
            var weekdays = WeekDays.None;
            days.Cast<DayOfWeek>().ForEach(d => weekdays = weekdays.Add(d));
            return weekdays;
        }

        public static WeekDays FromCollection(IEnumerable<DayOfWeek> days)
        {
            var weekdays = WeekDays.None;
            days.ForEach(d => weekdays = weekdays.Add(d));
            return weekdays;
        }

        public static WeekDays FromCsv(string dayList)
        {
            var weekdays = WeekDays.None;
            try
            {
                dayList.Split(',')
                    .Select(int.Parse)
                    .Cast<DayOfWeek>()
                    .ForEach(d => weekdays = weekdays.Add(d));
            }
            catch (Exception)
            {
                return weekdays;
            }

            return weekdays;
        }

        public static WeekDays Add(this WeekDays weekDays, WeekDays day)
        {
            return weekDays | day;
        }

        public static WeekDays Remove(this WeekDays weekDays, WeekDays day)
        {
            return weekDays & ~day;
        }

        public static WeekDays Add(this WeekDays weekDays, DayOfWeek day)
        {
            return weekDays | day.ToWeekdays();
        }

        public static WeekDays Remove(this WeekDays weekDays, DayOfWeek day)
        {
            return weekDays & ~day.ToWeekdays();
        }

        public static bool Has(this WeekDays weekDays, DayOfWeek day)
        {
            return weekDays.HasFlag(day.ToWeekdays());
        }

        public static bool Has(this WeekDays weekDays, WeekDays day)
        {
            return (weekDays == WeekDays.None && day == WeekDays.None)
                || (weekDays & day) != 0;
        }

        public static List<DayOfWeek> ToDayOfWeeks(this WeekDays weekDays)
        {
            var dowList = new List<DayOfWeek>();
            if (weekDays.HasFlag(WeekDays.Sunday))
                dowList.Add(DayOfWeek.Sunday);
            if (weekDays.HasFlag(WeekDays.Monday))
                dowList.Add(DayOfWeek.Monday);
            if (weekDays.HasFlag(WeekDays.Tuesday))
                dowList.Add(DayOfWeek.Tuesday);
            if (weekDays.HasFlag(WeekDays.Wednesday))
                dowList.Add(DayOfWeek.Wednesday);
            if (weekDays.HasFlag(WeekDays.Thursday))
                dowList.Add(DayOfWeek.Thursday);
            if (weekDays.HasFlag(WeekDays.Friday))
                dowList.Add(DayOfWeek.Friday);
            if (weekDays.HasFlag(WeekDays.Saturday))
                dowList.Add(DayOfWeek.Saturday);

            return dowList;
        }

        public static List<string> ToDayNames(this WeekDays weekDays)
        {
            return weekDays
                .ToDayOfWeeks()
                .Select(d => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d))
                .ToList();
        }

        public static WeekDays ToWeekdays(this DayOfWeek dow)
        {
            return (WeekDays)Math.Pow(2, (int)dow);
        }

        public static bool ContainsDate(this WeekDays weekdays, DateTime date)
        {
            return weekdays.Has(date.DayOfWeek);
        }
    }
}
