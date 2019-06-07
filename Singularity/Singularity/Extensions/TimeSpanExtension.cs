using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

// ReSharper disable once CheckNamespace
namespace Singularity
{
	[DebuggerStepThrough]
	public static class TimeSpanExtension
	{
		public static TimeSpan NewWeeks(this TimeSpan value, Int32 aWeeks)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return new TimeSpan(DaysPerWeek * aWeeks, 0, 0, 0);
		}

		public static TimeSpan NewDays(this TimeSpan value, Int32 dDays)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return new TimeSpan(dDays, 0, 0, 0);
		}

		public static TimeSpan NewYears(this TimeSpan value, Int32 years)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return new TimeSpan(DaysPerYear * years, 0, 0, 0);
		}

		public static TimeSpan NewHours(this TimeSpan value, Int32 hours)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return new TimeSpan(0, hours, 0, 0);
		}

		public static TimeSpan NewMinutes(this TimeSpan value, Int32 minutes)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return new TimeSpan(0, 0, minutes, 0);
		}

		public static TimeSpan NewSeconds(this TimeSpan value, Int32 seconds)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return new TimeSpan(0, 0, 0, seconds);
		}

		public static DateTime Ago(this TimeSpan value)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return DateTime.Now - value;
		}

		public static DateTime FromNow(this TimeSpan value)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return DateTime.Now + value;
		}

		public static DateTime AgoSince(this TimeSpan value, DateTime referenceDate)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			if (referenceDate.IsEmpty()) throw new ArgumentException("Given referenceDate argument shouldn't be empty.", "referenceDate");
			return referenceDate - value;
		}

		public static DateTime From(this TimeSpan value, DateTime referenceDate)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			if (referenceDate.IsEmpty()) throw new ArgumentException("Given referenceDate argument shouldn't be empty.", "referenceDate");
			return referenceDate + value;
		}

		public static String ToDescription(this TimeSpan value)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return TimeSpanArticulator.Articulate(value);
		}

		public static String ToDescription(this TimeSpan value, ETemporalGroupFlag accuracy)
		{
			if (value == null) throw new ArgumentException("Given value argument shouldn't be null.", "value");
			return TimeSpanArticulator.Articulate(value, accuracy);
		}

		public static Int32 DaysPerWeek = 7;
		public static Int32 DaysPerYear = 365;

		public static Boolean IsEmpty(this TimeSpan timeSpan)
		{
			return timeSpan == TimeSpan.MinValue;
		}

		public static TimeSpan Midday => !_midday.IsEmpty() ? _midday : (_midday = new TimeSpan(12, 0, 0));

		[ThreadStatic]
		private static TimeSpan _midday;
	}

}
