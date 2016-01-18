using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Singularity
{
	[DebuggerStepThrough]
	public static class ArrayExtension
	{
		public static List<T> ToListSafe<T>(this Array array)
		{
			var result = new List<T>();
			if (array != null)
			{
				foreach (var item in array)
				{
					try
					{
						result.Add((T)item);
					}
					catch (InvalidCastException) { }
					catch (NullReferenceException) { }
				}
			}
			return result;
		}
	}
}
