using System;
using System.Diagnostics;

namespace Singularity
{
	[DebuggerStepThrough]
	public static class DoubleExtension
	{
		/// <summary>
		/// Calculates the percentage of the number
		/// </summary>
		/// <param name="val"></param>
		/// <param name="value">The value against percentage to be calculated</param>
		/// <param name="roundOffTo">Precision of the result</param>
		/// <returns></returns>
		public static Double Percentage(this Double value, Double aPercentile, Int32 aRoundOffTo)
		{
			return Math.Round((value / 100d) * aPercentile, aRoundOffTo);
		}

		public static Double ToRadians(this Double degrees)
		{
			return (degrees * Math.PI / 180.0);
		}

		public static Double ToDegrees(this Double radians)
		{
			return (radians / Math.PI * 180.0);
		}

		#region Limits

		/// <summary>
		/// Returns either the given input or maximum value, effectively limiting the given input value to the given maximum argument.
		/// </summary>
		/// <param name="input">Number to be limited.</param>
		/// <param name="maxValue">The maximum value.</param>
		/// <returns></returns>
		public static Double LimitMax(this Double input, Double aMaxValue)
		{
			return aMaxValue * Convert.ToDouble(input > aMaxValue) + input * Convert.ToDouble(input <= aMaxValue);
		}

		/// <summary>
		/// Returns either the given input or minimum value, effectively limiting the given input value to the given minimum argument.
		/// </summary>
		/// <param name="input">Number to be limited.</param>
		/// <param name="maxValue">The minimum value.</param>
		/// <returns></returns>
		public static Double LimitMin(this Double input, Double aMinValue)
		{
			return aMinValue * Convert.ToDouble(input < aMinValue) + input * Convert.ToDouble(input >= aMinValue);
		}

		/// <summary>
		/// Returns either the given input, maximum or minimum value, effectively limiting the given input value to range within the high low limits.
		/// </summary>
		/// <param name="input">Number to be limited</param>
		/// <param name="aLowLimt">The minimum value.</param>
		/// <param name="aHighLimit">The maximum value.</param>
		/// <returns>A number between the low and high limits inclusive.</returns>
		public static Double LimitInRange(this Double input, Double aLowLimt, Double aHighLimit)
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
		public static Double LimitInRange(this Double input, Double aLowLimt, Double aHighLimit, out Boolean aWasOutOfRange)
		{
			if (aLowLimt > aHighLimit)
			{
				aLowLimt = aLowLimt.Swap(ref aHighLimit);
			}

			var result = input.LimitMin(aLowLimt).LimitMax(aHighLimit);
			aWasOutOfRange = input != result;
			return result;
		}

		public static Boolean IsOutOfRange(this Double input, Double aLowLimt, Double aHighLimit)
		{
			var result = false;
			LimitInRange(input, aLowLimt, aHighLimit, out result);
			return result;
		}

		#endregion

	}
}
