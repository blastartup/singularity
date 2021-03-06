﻿using Singularity.DataService;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Singularity.EfDataService
{
	public abstract class EfRepository<TEntity> where TEntity : class
	{
		protected EfRepository(EfDbContext context)
		{
			Context = context;
		}

		public virtual List<TEntity> GetList(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			return GetQuery(filter, orderBy, paging, navigationProperties).ToList();
		}

		public virtual IQueryable<TEntity> GetQuery(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			// As we don't know whether TEntity is IDeletable, it is up to the concrete class to override the line below with this: return Get(filter, orderBy, paging, navigationProperties).Actives().ToList();
			return Get(filter, orderBy, paging, navigationProperties);
		}

		public virtual TEntity GetEntity(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			TEntity entity = Get(filter, orderBy, null, navigationProperties).FirstOrDefault();
			return ((entity as IDeletable)?.DeletedDate).HasValue ? null : entity;
		}

		protected IQueryable<TEntity> Get(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Paging paging = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			IQueryable<TEntity> dbQuery = DbSet;
			_filter = filter;

			IncludeNavigation(navigationProperties, ref dbQuery);

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

		protected virtual void IncludeNavigation(Expression<Func<TEntity, Object>>[] navigationProperties, ref IQueryable<TEntity> dbQuery)
		{
			if (Context.NoTracking)
			{
				dbQuery = navigationProperties.Aggregate(dbQuery, (current, navigationProperty) => current.Include(navigationProperty).AsNoTracking());
			}
			else
			{
				dbQuery = navigationProperties.Aggregate(dbQuery, (current, navigationProperty) => current.Include(navigationProperty));
			}
		}

		public virtual TEntity GetById(Object id)
		{
			if (id is Guid || id is Int32)
			{
				TEntity entity = DbSet.Find(id);
				if (entity is IDeletable deletable && deletable.DeletedDate.HasValue)
				{
					return null;
				}
				return entity;
			}
			return null;
		}

		public virtual Boolean Exists(Object id)
		{
			if (id is Guid || id is Int32)
			{
				TEntity entity = DbSet.Find(id);
				if (entity is IDeletable deletable && deletable.DeletedDate.HasValue)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public virtual Boolean Exists(Expression<Func<TEntity, Boolean>> filter)
		{
			return DbSet.Any(filter);
		}

		public virtual void Insert(TEntity entity)
		{
			if (entity is IModifiable modifiableEntity)
			{
				modifiableEntity.CreatedDate = NowDateTime;
				modifiableEntity.ModifiedDate = NowDateTime;
			}
			else if (entity is ICreatable creatable)
			{
				creatable.CreatedDate = NowDateTime;
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
			if (entityToDeactivate is IDeletable deletable)
			{
				deletable.DeletedDate = Context.Now;
			}

			if (entityToDeactivate is IModifiable modifiable)
			{
				modifiable.ModifiedDate = NowDateTime;
			}

			DbSet.Attach(entityToDeactivate);
			Context.Entry(entityToDeactivate).State = EntityState.Modified;
		}

		public virtual void Deactivate(IEnumerable<TEntity> entitiesToDeactivate)
		{
			entitiesToDeactivate.ForEach(Deactivate);
		}

		public virtual void DeleteById(Object id, Boolean safely = true)
		{
			if (safely)
			{
				TEntity entityToDelete = DbSet.Find(id);
				Delete(entityToDelete);
			}
			else
			{
				TEntity entityToDelete = DeleteEntity(id);
				Context.Entry(entityToDelete).State = EntityState.Deleted;
			}
		}

		public virtual void Delete(TEntity entityToDelete)
		{
			if (Context.Entry(entityToDelete).State == EntityState.Detached)
			{
				DbSet.Attach(entityToDelete);
			}
			DbSet.Remove(entityToDelete);
		}

		public virtual void Delete(IEnumerable<TEntity> entitiesToDelete)
		{
			entitiesToDelete.ForEach(Delete);
		}

		public void DeleteDirectlyById(Object id)
		{
		}

		public virtual void Update(TEntity entityToUpdate)
		{
			if (entityToUpdate is IModifiable modifiable)
			{
				modifiable.ModifiedDate = NowDateTime;
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

		public DateTime? SchemaModifiedDateTime => Context.TableSchemaModifiedDateTime(typeof(TEntity).Name);

		protected DbSet<TEntity> DbSet => _dbSet ?? (_dbSet = NewDbSet(Context.Set<TEntity>()));
		private DbSet<TEntity> _dbSet;

		protected abstract TEntity DeleteEntity(Object id);

		protected abstract DateTime NowDateTime { get; }
		protected abstract DbSet<TEntity> NewDbSet(DbSet<TEntity> contextualDbSet);

		protected EfDbContext Context;
		private Expression<Func<TEntity, Boolean>> _filter;
	}
}