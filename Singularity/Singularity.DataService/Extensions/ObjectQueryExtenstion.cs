using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace Singularity.DataService
{
	public static class ObjectQueryExtenstion
	{
		/// <summary>
		/// Apply inclide calls to ObjectQuery. blanks and nulls are okay
		/// </summary>
		/// <typeparam name="T">Entity</typeparam>
		/// <param name="query">The query to apply includes to</param>
		/// <param name="includes">comma separated list of related objects to inclde in the query results</param>
		/// <param name="allowedIncludes">allowed related object names</param>
		/// <returns>a new query with includes applied</returns>
		public static ObjectQuery<T> ApplyIncludes<T>(this ObjectQuery<T> query, IEnumerable<string> includes, IEnumerable<string> allowedIncludes = null)
		{
			if (includes.IsEmpty())
			{
				return query;
			}

			var includeList = includes.Select(x => x.Trim()).Where(x => allowedIncludes == null || allowedIncludes.Contains(x));
			foreach (var include in includeList)
			{
				query = query.Include(include);
			}
			return query;
		}

	}
}
