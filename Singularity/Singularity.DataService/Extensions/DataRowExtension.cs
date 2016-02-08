using System;
using System.Data;

namespace Singularity.DataService
{
	public static class DataRowExtension
	{
		public static Object GetValue(this DataRow row, int? fieldIdx)
		{
			Object result = null;
			if (fieldIdx != null)
			{
				result = row[fieldIdx.ValueOnNull(0)];
			}
			return result;
		}
	}
}
