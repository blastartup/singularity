﻿using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using Singularity.DataService;

// ReSharper disable once CheckNamespace

namespace Singularity.EfDataService
{
	public static class EntityCollectionExtension
	{
		/// <summary>
		/// Only return Active Singularity Entities
		/// </summary>
		/// <typeparam name="TEntity">An IActivatable object context entity.</typeparam>
		/// <param name="collection">An IQueryable list of IActivatables.</param>
		/// <returns>An IQueryable list of only those entities that are Active.</returns>
		public static IEnumerable<TEntity> Actives<TEntity>(this EntityCollection<TEntity> collection) where TEntity : class, IDeletable
		{
			return collection.Where(o => o.DeletedDate == null);
		}
	}
}
