using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
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
			// As we don't know whether TEntity is IDeletable, it is up to the concrete class to override the the line below with this: return Get(filter, orderBy, paging, navigationProperties).Actives().ToList();
			return Get(filter, orderBy, paging, navigationProperties).ToList();
		}

		public virtual TEntity GetEntity(Expression<Func<TEntity, Boolean>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, Object>>[] navigationProperties)
		{
			TEntity entity = Get(filter, orderBy, null, navigationProperties).FirstOrDefault();
			if (entity is IDeletable && ((IDeletable)entity).DeletedDate.HasValue)
			{
				return null;
			}
			return entity;
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
			IModifiable modifiableEntity = entity as IModifiable;
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

		#region SqlCommand and DataReader support

		/// <summary>
		/// Begins a transaction
		/// </summary>
		/// <returns>The new SqlTransaction object</returns>
		public SqlTransaction BeginSqlTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
		{
			RollbackSqlTransaction();
			_sqlTransaction = SqlConnection.BeginTransaction(isolationLevel);
			return _sqlTransaction;
		}

		/// <summary>
		/// Commits any transaction in effect.
		/// </summary>
		public void CommitSqlTransaction()
		{
			if (_sqlTransaction != null)
			{
				_sqlTransaction.Commit();
				_sqlTransaction = null;
			}
		}

		/// <summary>
		/// Rolls back any transaction in effect.
		/// </summary>
		public void RollbackSqlTransaction()
		{
			if (_sqlTransaction != null)
			{
				_sqlTransaction.Rollback();
				_sqlTransaction = null;
			}
		}

		/// <summary>
		/// Executes a query that returns no results
		/// </summary>
		/// <param name="qry">Query text</param>
		/// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
		/// <returns>The number of rows affected</returns>
		public Int32 SqlExecuteNonQuery(String qry, params Object[] args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Executes a query that returns a single value
		/// </summary>
		/// <param name="qry">Query text</param>
		/// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
		/// <returns>Value of first column and first row of the results</returns>
		public object SqlExecuteScalar(String qry, params Object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				return cmd.ExecuteScalar();
			}
		}

		/// <summary>
		/// Executes a query and returns the results as a SqlDataReader
		/// </summary>
		/// <param name="qry">Query text</param>
		/// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
		/// <returns>Results as a SqlDataReader</returns>
		public SqlDataReader SqlExecuteReader(String qry, params Object[] args)
		{
			using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
			{
				return cmd.ExecuteReader();
			}
		}

		private SqlCommand CreateCommand(String qry, CommandType type, params Object[] args)
		{
			SqlCommand cmd = new SqlCommand(qry, SqlConnection)
			{
				Transaction = SqlTransaction,
				CommandType = type
			};

			// Construct SQL parameters
			for (int i = 0; i <= args.Length - 1; i++)
			{
				if (args[i] is String && i < (args.Length - 1))
				{
					SqlParameter parm = new SqlParameter
					{
						ParameterName = (String)args[i],
						Value = args[System.Threading.Interlocked.Increment(ref i)]
					};
					cmd.Parameters.Add(parm);
				}
				else if (args[i] is SqlParameter)
				{
					cmd.Parameters.Add((SqlParameter)args[i]);
				}
				else
				{
					throw new ArgumentException("Invalid number or type of arguments supplied");
				}
			}

			cmd.CommandTimeout = _defaultCommandTimeout;

			return cmd;
		}

		public SqlConnection SqlConnection => _sqlConnection ?? (_sqlConnection = new SqlConnection(Context.Database.Connection.ConnectionString));
		private SqlConnection _sqlConnection;

		public SqlTransaction SqlTransaction => _sqlTransaction;
		private SqlTransaction _sqlTransaction;

		#endregion

		protected DbSet<TEntity> DbSet => _dbSet ?? (_dbSet = NewDbSet(Context.Set<TEntity>()));
		private DbSet<TEntity> _dbSet;

		protected abstract DateTime NowDateTime { get; }
		protected abstract DbSet<TEntity> NewDbSet(DbSet<TEntity> contextualDbSet);

		protected EfDbContext Context;
		private Expression<Func<TEntity, Boolean>> _filter;
		private Int32 _defaultCommandTimeout = 30;
	}
}