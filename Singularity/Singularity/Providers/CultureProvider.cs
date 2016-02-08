using System;
using System.Globalization;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public static class CultureProvider
	{
		public static CultureInfo Current
		{
			get { return _current ?? (_current = CultureInfo.CurrentCulture); }
			set { _current = value; }
		}
		[ThreadStatic]
		private static CultureInfo _current;

	}
}
