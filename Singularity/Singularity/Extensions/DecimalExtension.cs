using System;
using System.Diagnostics;
using System.Globalization;

namespace Singularity
{
	[DebuggerStepThrough]
	public static class DecimalExtension
	{
		public static Boolean IsWithinSqlPrecisionAndScale(this Decimal value, Int32 precision, Int32 scale)
		{
			var maxIntegralPart = GetMaxIntegralPart(precision, scale);
			return (Math.Abs(Decimal.Truncate(value)) <= maxIntegralPart);
		}

		public static Int32 DecimalPlaces(this Decimal value)
		{
			var decimalPlaces = 0;
			var decimalPart = value - Decimal.Truncate(value);	// To stop potential overflow
			while (Decimal.Truncate(decimalPart) != decimalPart)
			{
				decimalPart *= 10;
				decimalPlaces++;
			}
			return decimalPlaces;
		}

		private static String DecimalSeparator => Factory.CurrentCultureInfo.NumberFormat.NumberDecimalSeparator;

		private static String GroupSeparator => Factory.CurrentCultureInfo.NumberFormat.NumberGroupSeparator;

		private static Decimal GetMaxIntegralPart(Int32 precision, Int32 scale)
		{
			return (Decimal)Math.Pow(10, precision - scale) - 1;
		}

		#region Limits

		/// <summary>
		/// Returns either the given input or maximum value, effectively limiting the given input value to the given maximum argument.
		/// </summary>
		/// <param name="input">Number to be limited.</param>
		/// <param name="maxValue">The maximum value.</param>
		/// <returns></returns>
		public static Decimal LimitMax(this Decimal input, Decimal maxValue)
		{
			return maxValue * Convert.ToDecimal(input > maxValue) + input * Convert.ToDecimal(input <= maxValue);
		}

		/// <summary>
		/// Returns either the given input or minimum value, effectively limiting the given input value to the given minimum argument.
		/// </summary>
		/// <param name="input">Number to be limited.</param>
		/// <param name="maxValue">The minimum value.</param>
		/// <returns></returns>
		public static Decimal LimitMin(this Decimal input, Decimal aMinValue)
		{
			return aMinValue * Convert.ToDecimal(input < aMinValue) + input * Convert.ToDecimal(input >= aMinValue);
		}

		/// <summary>
		/// Returns either the given input, maximum or minimum value, effectively limiting the given input value to range within the high low limits.
		/// </summary>
		/// <param name="input">Number to be limited</param>
		/// <param name="aLowLimt">The minimum value.</param>
		/// <param name="aHighLimit">The maximum value.</param>
		/// <returns>A number between the low and high limits inclusive.</returns>
		public static Decimal LimitInRange(this Decimal input, Decimal aLowLimt, Decimal aHighLimit)
		{
			Boolean notUsed;
			return LimitInRange(input, aLowLimt, aHighLimit, out notUsed);
		}

		/// <summary>
		/// Returns either the given input, maximum or minimum value, effectively limiting the given input value to range within the high low limits.
		/// If the value is adjusted then set the aWasOutOfRange flag.
		/// </summary>
		/// <param name="input">Number to be limited</param>
		/// <param name="aLowLimt">The minimum value.</param>
		/// <param name="aHighLimit">The maximum value.</param>
		/// <param name="aWasOutOfRange">Returning value indicating whether the given value was outside the range.</param>
		/// <returns></returns>
		public static Decimal LimitInRange(this Decimal input, Decimal aLowLimt, Decimal aHighLimit, out Boolean aWasOutOfRange)
		{
			if (aLowLimt > aHighLimit)
			{
				aLowLimt = aLowLimt.Swap(ref aHighLimit);
			}

			var result = input.LimitMin(aLowLimt).LimitMax(aHighLimit);
			aWasOutOfRange = input != result;
			return result;
		}

		public static Boolean IsOutOfRange(this Decimal input, Decimal aLowLimt, Decimal aHighLimit)
		{
			var result = false;
			LimitInRange(input, aLowLimt, aHighLimit, out result);
			return result;
		}

		#endregion

		public static String FormatMoney(this Decimal amount)
		{
			return amount.ToString("0.00");
		}

		public static String FormatCurrency(this Decimal amount)
		{
			return amount.ToString("C2", CultureInfo.CreateSpecificCulture("en-AU"));
		}
	}
}
