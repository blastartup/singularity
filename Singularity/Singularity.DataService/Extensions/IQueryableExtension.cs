﻿using System.Linq;

// ReSharper disable InconsistentNaming

namespace Singularity.DataService
{
	/// <summary>
	/// Static class to hold extenstion methods for the IQueryable type
	/// </summary>
	public static class IQueryableExtension
	{
		/// <summary>
		/// Only return Active Singularity Entities
		/// </summary>
		/// <typeparam name="TEntity">An IActivatable object context entity.</typeparam>
		/// <param name="collection">An IQueryable list of IActivatables.</param>
		/// <returns>An IQueryable list of only those entities that are Active.</returns>
		public static IQueryable<TEntity> Actives<TEntity>(this IQueryable<TEntity> collection) where TEntity : class, IDeleteion
		{
			return collection.Where(x => !x.IsDeleted);
		}
	}
}
