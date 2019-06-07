using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

// ReSharper disable once CheckNamespace
namespace Singularity
{
	[DebuggerStepThrough]
	public static class CharExtension
	{
		/// <summary>
		/// Generates a String containing the requested count of a particular character
		/// </summary>
		public static String Replicate(this Char value, Int32 count)
		{
			count = count.LimitMin(0);

			return String.Empty.PadRight(count, value);
		}

	}
}
