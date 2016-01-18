using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Singularity
{
	public static class NameValueCollectionExtension
	{
		public static IEnumerable<KeyValuePair<String, String>> ToKeyValuePairs(this NameValueCollection collection)
		{
			var result = new List<KeyValuePair<String, String>>();
			foreach (var key in collection.AllKeys)
			{
				result.Add(new KeyValuePair<String, String>(key, collection[key]));
			}
			return result;
		}
	}
}
