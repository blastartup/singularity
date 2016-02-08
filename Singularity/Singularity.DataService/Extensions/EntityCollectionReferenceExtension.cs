using System;
using System.Data.Entity.Core.Objects.DataClasses;

namespace Singularity.DataService
{
	public static class EntityCollectionReferenceExtension
	{
		public static void LoadAsRequired<TEntity>(this EntityCollection<TEntity> collection) where TEntity : class
		{
			if (!collection.IsLoaded)
			{
				try
				{
					collection.Load();
				}
				catch (InvalidOperationException) { }
			}
		}
	}
}
