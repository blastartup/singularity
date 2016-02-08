using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Singularity.DataService
{
	public abstract class BaseRepository<TEntity> where TEntity : class
	{
		protected DbContext Context;
		protected DbSet<TEntity> DbSet;

		public BaseRepository(DbContext context)
		{
			Context = context;
			DbSet = context.Set<TEntity>();
		}

		public virtual List<TEntity> GetList(Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, object>>[] navigationProperties)
		{
			return Get(filter, orderBy, paging, navigationProperties).ToList();
		}

		public virtual TEntity GetEntity(Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, object>>[] navigationProperties)
		{
			return Get(filter, orderBy, null, navigationProperties).FirstOrDefault();
		}

		protected IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, object>>[] navigationProperties)
		{
			IQueryable<TEntity> dbQuery = DbSet;
			_filter = filter;

			//Apply eager loading
			foreach (Expression<Func<TEntity, object>> navigationProperty in navigationProperties)
			{
				dbQuery = dbQuery.Include(navigationProperty).AsNoTracking();
			}

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

		public virtual TEntity GetById(object id)
		{
			if (id is Guid || id is int)
			{
				TEntity entity = DbSet.Find(id);
				if (entity is IDeleteion && ((IDeleteion)entity).IsDeleted)
				{
					return null;
				}
				return entity;
			}
			return null;
		}

		public virtual Boolean Exists(Expression<Func<TEntity, bool>> filter)
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

		public virtual void Deactivate(object id)
		{
			TEntity entityToDeactivate = DbSet.Find(id);
			Deactivate(entityToDeactivate);
		}

		public virtual void Deactivate(TEntity entityToDeactivate)
		{
			if (entityToDeactivate is IDeleteion)
			{
				((IDeleteion)entityToDeactivate).IsDeleted = true;
			}

			if (entityToDeactivate is IModifiable)
			{
				((IModifiable)entityToDeactivate).ModifiedDate = NowDateTime;
			}

			DbSet.Attach(entityToDeactivate);
			Context.Entry(entityToDeactivate).State = EntityState.Modified;
		}

		public virtual void Delete(object id)
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

		public bool Any(Expression<Func<TEntity, bool>> filter = null)
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

		public int Count(Expression<Func<TEntity, bool>> filter = null)
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

		protected abstract DateTime NowDateTime { get; }
		private Expression<Func<TEntity, bool>> _filter;
	}
}