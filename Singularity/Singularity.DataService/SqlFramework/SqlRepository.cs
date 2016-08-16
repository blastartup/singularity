using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Singularity.DataService.Interfaces;

namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlRepository<TSqlEntity> where TSqlEntity : ISqlEntity
	{
		protected SqlEntityContext Context;

		protected SqlRepository(SqlEntityContext context)
		{
			Context = context;
		}

		public virtual List<TSqlEntity> GetList(String filter = "", SqlParameter[] filterParameters = null, String selectColumns = null, 
			String orderBy = null, Paging paging = null)
		{
			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			if (filterParameters != null && filter == null)
			{
				throw new ArgumentException("FilterParameters can only be applied to a filtered result.");
			}

			selectColumns = selectColumns ?? SelectAllColunms();
			return Select(selectColumns, filter, filterParameters, orderBy, paging).ToList();
		}

		public List<TSqlEntity> GetListByIds<T>(IEnumerable<T> ids, String selectColumns = null, String orderBy = null, Paging paging = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return Select(selectColumns, FilterIn(ids), null, orderBy, paging).ToList();
		}

		public virtual TSqlEntity GetEntity(String filter = "", SqlParameter[] filterParameters = null, String selectColumns = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return Select(selectColumns, filter, filterParameters, null, new Paging(1)).FirstOrDefault();
		}

		public TSqlEntity GetById(Object id, String selectColumns = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return Select(selectColumns, WhereClause(), Parameters(id)).FirstOrDefault();
		}

		public virtual Boolean Exists(String filter = "", SqlParameter[] filterParameters = null, String selectColumns = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return Select(selectColumns, filter, filterParameters, null, new Paging(1)).Any();
		}

		public void Insert(TSqlEntity sqlEntity)
		{
			if (SaveChangesTransactionally)
			{
				Context.BeginTransaction();
			}

			var modifiableEntity = sqlEntity as IModifiable;
			if (modifiableEntity != null)
			{
				modifiableEntity.CreatedDate = NowDateTime;
				modifiableEntity.ModifiedDate = NowDateTime;
			}
			else if (sqlEntity is ICreatable)
			{
				((ICreatable)sqlEntity).CreatedDate = NowDateTime;
			}

			InsertCore(sqlEntity, InsertColunms(), GetInsertValues(sqlEntity));
		}

		//public virtual void Update(TEntity entityToUpdate)
		//{
		//	if (entityToUpdate is IModifiable)
		//	{
		//		((IModifiable)entityToUpdate).ModifiedDate = NowDateTime;
		//	}
		//	DbSet.Attach(entityToUpdate);
		//	Context.Entry(entityToUpdate).State = EntityState.Modified;
		//}

		//public Int32 Count(Expression<Func<TEntity, Boolean>> filter = null)
		//{
		//	IQueryable<TEntity> dbQuery = DbSet;
		//	if (filter != null)
		//	{
		//		return dbQuery.Count(filter);
		//	}

		//	if (_filter != null)
		//	{
		//		return dbQuery.Count(_filter);
		//	}
		//	return dbQuery.Count();
		//}

		//public virtual void Deactivate(Object id)
		//{
		//	TEntity entityToDeactivate = DbSet.Find(id);
		//	Deactivate(entityToDeactivate);
		//}

		//public virtual void Deactivate(TEntity entityToDeactivate)
		//{
		//	var deletable = entityToDeactivate as IDeletable;
		//	if (deletable != null)
		//	{
		//		deletable.IsDeleted = true;
		//	}

		//	var modifiable = entityToDeactivate as IModifiable;
		//	if (modifiable != null)
		//	{
		//		modifiable.ModifiedDate = NowDateTime;
		//	}

		//	DbSet.Attach(entityToDeactivate);
		//	Context.Entry(entityToDeactivate).State = EntityState.Modified;
		//}

		//public virtual void Delete(Object id)
		//{
		//	TEntity entityToDelete = DbSet.Find(id);
		//	Delete(entityToDelete);
		//}

		//public virtual void Delete(TEntity entityToDelete)
		//{
		//	if (Context.Entry(entityToDelete).State == EntityState.Detached)
		//	{
		//		DbSet.Attach(entityToDelete);
		//	}
		//	DbSet.Remove(entityToDelete);
		//}

		protected IList<TSqlEntity> Select(String selectColumns, String filter = "", SqlParameter[] filterParameters = null, String orderBy = null, 
			Paging paging = null)
		{
			String query = null;

			if (!String.IsNullOrEmpty(filter))
			{
				filter = " where " + filter;
			}

			if (filterParameters == null)
			{
				filterParameters = new SqlParameter[] {};
			}

			if (!String.IsNullOrEmpty(orderBy))
			{
				orderBy = " Order By " + orderBy;
			}
			else
			{
				orderBy = "";
			}

			String takeFilter = "";
			if (paging != null)
			{
				takeFilter = $"Top {paging.Take} ";
			}

			query = "select {0}{1} from {2}{3}{4}".FormatX(takeFilter, selectColumns, FromTables(), filter, orderBy);
			return AssembleList(Context.ExecDataReader(query, filterParameters));
		}

		protected void InsertCore(TSqlEntity sqlEntity, String insertColumns, String insertValues)
		{
			String insertStatement = InsertColumnsPattern.FormatX(((ISqlEntity)sqlEntity).Name, insertColumns, insertValues);
			SetEntityPrimaryKey(sqlEntity, Context.ExecScalar(insertStatement, new SqlParameter[] {}));
		}

		protected virtual String SelectAllColunms()
		{
			return "*";
		}

		protected virtual String FromTables()
		{
			return TableName;
		}

		protected virtual String WhereClause()
		{
			return $"{PrimaryKeyName} = @pk";
		}

		protected virtual SqlParameter[] Parameters(Object primaryKeyValue)
		{
			return new SqlParameter[] { new SqlParameter("@pk", primaryKeyValue) };
		}

		private String FilterIn<T>(IEnumerable<T> ids)
		{
			return "{0} In ({1})".FormatX(PrimaryKeyName, String.Join(",", (from idItem in ids select ObtainValue<Object>(idItem)).ToArray()));
		}

		protected String ObtainValue<T>(T nativeValue)
		{
			String result = null;
			if (nativeValue == null || Convert.IsDBNull(nativeValue))
			{
				result = "Null";
			}
			else if (nativeValue is String)
			{
				result = String.Format(StringValuePattern, nativeValue.ToString().Replace("'", "''"));
			}
			else if (nativeValue is DateTime)
			{
				result = String.Format(StringValuePattern, ((DateTime)(Object)nativeValue).ToString(DateTimeFormat));
			}
			else if (nativeValue is TimeSpan)
			{
				result = String.Format(StringValuePattern, new DateTime(((TimeSpan)(Object)nativeValue).Ticks).ToShortTimeString());
			}
			else if (nativeValue is Boolean)
			{
				result = Convert.ToInt32((Boolean)(Object)nativeValue).ToString();
			}
			else if (nativeValue is Guid)
			{
				result = String.Format(StringValuePattern, nativeValue.ToString());
			}
			else
			{
				result = nativeValue.ToString();
			}
			return result;
		}

		public Boolean SaveChangesTransactionally { get; set; }

		protected abstract List<TSqlEntity> AssembleList(SqlDataReader dataReader);
		protected abstract DateTime NowDateTime { get; }
		protected abstract String TableName { get; }
		protected abstract String PrimaryKeyName { get; }
		protected abstract String InsertColunms();
		protected abstract String GetInsertValues(TSqlEntity entity);
		protected abstract void SetEntityPrimaryKey(TSqlEntity entity, Object newPrimaryKey);


		private const String InsertColumnsPattern = "Insert [{0}] ({1}) Values({2}) SELECT @@IDENTITY";
		private const String StringValuePattern = "'{0}'";
		private const String DateTimeFormat = "yyyy/MM/dd HH:mm:ss.fff";
	}
}