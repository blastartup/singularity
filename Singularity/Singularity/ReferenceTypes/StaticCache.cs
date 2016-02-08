using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Singularity
{
	[DebuggerStepThrough]
	public class StaticCache<TKey, TValue> : Cacher<TKey, TValue>
	{
		protected override IDictionary<TKey, TValue> InitCache()
		{
			return new Dictionary<TKey, TValue>();
		}

		public override Boolean Get(TKey key, out TValue value)
		{
			var result = false;
			if (Cache.ContainsKey(key))
			{
				value = Cache[key];
				result = true;
			}
			else
			{
				value = default(TValue);
			}

			return result;
		}

		public override void Set(TKey key, TValue value)
		{
			if (Cache.ContainsKey(key))
			{
				Cache[key] = value;
			}
			else
			{
				Cache.Add(key, value);
			}
		}

		public TValue GetOrEvaluate(TKey key, Func<TKey, TValue> value)
		{
			TValue result;
			if (Cache.ContainsKey(key))
			{
				result = Cache[key];
			}
			else
			{
				result = value(key);
				Cache.Add(key, result);
			}
			return result;
		}

		public override void Clear(TKey key)
		{
			Cache.Remove(key);
		}

		public override IEnumerable<KeyValuePair<TKey, TValue>> GetAll()
		{
			return Cache.Select(item => new KeyValuePair<TKey, TValue>(item.Key, item.Value));
		}
	}
}
