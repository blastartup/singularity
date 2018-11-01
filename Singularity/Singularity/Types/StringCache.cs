using System;
using System.Collections.Generic;

namespace Singularity.Types
{
	/// <summary>
	/// A quick performance trick provided the data set (column of data) has a high instance of repetitive strings.
	/// </summary>

	public class StringCache
	{
		private readonly Dictionary<String, String> _cache;

		public StringCache()
		{
			_cache = new Dictionary<String, String>();
		}

		public virtual String Get(String value)
		{
			if (value == null) return null;

			String result;
			if (!_cache.TryGetValue(value, out result))
			{
				result = value;
				_cache[result] = result;
			}
			return result;
		}
	}
}
