using System;
using System.Runtime.Caching;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public class MemoryCacher
	{
		public Object GetValue(String key)
		{
			var memoryCache = MemoryCache.Default;
			return memoryCache.Get(key);
		}

		public Boolean Add(String key, Object value, DateTimeOffset absExpiration)
		{
			var memoryCache = MemoryCache.Default;
			return memoryCache.Add(key, value, absExpiration);
		}

		public void Delete(String key)
		{
			var memoryCache = MemoryCache.Default;
			if (memoryCache.Contains(key))
			{
				memoryCache.Remove(key);
			}
		}
	}
}
