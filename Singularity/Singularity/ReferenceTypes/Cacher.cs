using System;
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public abstract class Cacher<TKey, TValue> : ICacheProvider<TKey, TValue>
	{
		protected Cacher()
		{
			Cache = InitCache();
		}

		protected IDictionary<TKey, TValue> Cache;

		protected abstract IDictionary<TKey, TValue> InitCache();

		public abstract Boolean Get(TKey key, out TValue value);

		public abstract void Set(TKey key, TValue value);

		public abstract void Clear(TKey key);

		public abstract IEnumerable<KeyValuePair<TKey, TValue>> GetAll();

	}
}
