using System;
using System.Data.SqlTypes;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public static class SqlDateTimeExtension
	{
		public static DateTime MinSqlValue
		{
			get { return SqlDateTime.MinValue.Value; }
		}
	}
}
