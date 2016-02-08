using System.Diagnostics.Contracts;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public static class DateTimeProvider
	{
		public static IDateTimeProvider Instance
		{
			get
			{
				return _instance;
			}

			set
			{
				Contract.Requires(value != null);
				_instance = value;
			}
		}

		private static IDateTimeProvider _instance;
	}
}
