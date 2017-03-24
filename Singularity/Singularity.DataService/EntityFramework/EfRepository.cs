using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class EfRepository<TEntity> where TEntity : class
	{
		protected EfRepository(DbContext context)
		{
			Context = context;
		}

		public virtual List<TEntity> GetList(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			return Get(filter, orderBy, paging, navigationProperties).ToList();
		}

		public virtual TEntity GetEntity(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			return Get(filter, orderBy, null, navigationProperties).FirstOrDefault();
		}

		protected IQueryable<TEntity> Get(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			IQueryable<TEntity> dbQuery = DbSet;
			_filter = filter;

			IncludeNavigation(navigationProperties, dbQuery);

			if (orderBy != null)
			{
				dbQuery = orderBy(dbQuery);
			}

			if (filter != null)
			{
				dbQuery = dbQuery.Where(filter);
			}

			if (paging != null)
			{
				dbQuery = dbQuery.Skip(paging.Skip).Take(paging.Take);
			}

			return dbQuery;
		}

		protected virtual void IncludeNavigation(Expression<Func<TEntity, Object>>[] navigationProperties, IQueryable<TEntity> dbQuery)
		{
			foreach (Expression<Func<TEntity, Object>> navigationProperty in navigationProperties)
			{
				dbQuery = dbQuery.Include(navigationProperty).AsNoTracking();
			}
		}

		public virtual TEntity GetById(Object id)
		{
			if (id is Guid || id is Int32)
			{
				TEntity entity = DbSet.Find(id);
				if (entity is IDeletable && ((IDeletable)entity).IsDeleted)
				{
					return null;
				}
				return entity;
			}
			return null;
		}

		public virtual Boolean Exists(Expression<Func<TEntity, Boolean>> filter)
		{
			return DbSet.Any(filter);
		}

		public virtual void Insert(TEntity entity)
		{
			var modifiableEntity = entity as IModifiable;
			if (modifiableEntity != null)
			{
				modifiableEntity.CreatedDate = NowDateTime;
				modifiableEntity.ModifiedDate = NowDateTime;
			}
			else if (entity is ICreatable)
			{
				((ICreatable)entity).CreatedDate = NowDateTime;
			}

			DbSet.Add(entity);
		}

		public virtual void Deactivate(Object id)
		{
			TEntity entityToDeactivate = DbSet.Find(id);
			Deactivate(entityToDeactivate);
		}

		public virtual void Deactivate(TEntity entityToDeactivate)
		{
			var deletable = entityToDeactivate as IDeletable;
			if (deletable != null)
			{
				deletable.IsDeleted = true;
			}

			var modifiable = entityToDeactivate as IModifiable;
			if (modifiable != null)
			{
				modifiable.ModifiedDate = NowDateTime;
			}

			DbSet.Attach(entityToDeactivate);
			Context.Entry(entityToDeactivate).State = EntityState.Modified;
		}

		public virtual void Delete(Object id)
		{
			TEntity entityToDelete = DbSet.Find(id);
			Delete(entityToDelete);
		}

		public virtual void Delete(TEntity entityToDelete)
		{
			if (Context.Entry(entityToDelete).State == EntityState.Detached)
			{
				DbSet.Attach(entityToDelete);
			}
			DbSet.Remove(entityToDelete);
		}

		public virtual void Update(TEntity entityToUpdate)
		{
			if (entityToUpdate is IModifiable)
			{
				((IModifiable)entityToUpdate).ModifiedDate = NowDateTime;
			}
			DbSet.Attach(entityToUpdate);
			Context.Entry(entityToUpdate).State = EntityState.Modified;
		}

		public Boolean Any(Expression<Func<TEntity, Boolean>> filter = null)
		{
			IQueryable<TEntity> dbQuery = DbSet;
			if (filter != null)
			{
				return dbQuery.Any(filter);
			}

			if (_filter != null)
			{
				return dbQuery.Any(_filter);
			}
			return dbQuery.Any();
		}

		public Int32 Count(Expression<Func<TEntity, Boolean>> filter = null)
		{
			IQueryable<TEntity> dbQuery = DbSet;
			if (filter != null)
			{
				return dbQuery.Count(filter);
			}

			if (_filter != null)
			{
				return dbQuery.Count(_filter);
			}
			return dbQuery.Count();
		}

		protected DbSet<TEntity> DbSet => _dbSet ?? (_dbSet = NewDbSet(Context.Set<TEntity>()));
		private DbSet<TEntity> _dbSet;

		protected abstract DateTime NowDateTime { get; }
		protected abstract DbSet<TEntity> NewDbSet(DbSet<TEntity> contextualDbSet);

		protected DbContext Context;
		private Expression<Func<TEntity, Boolean>> _filter;
	}
}