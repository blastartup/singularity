using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace Singularity.DataService.OleDbFramework
{
	public abstract class OleDbRepository
	{
		public static String ObtainValue<T>(T nativeValue)
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
		private static String StringValuePattern = "'{0}'";
		private static String DateTimeFormat = "yyyy/MM/dd HH:mm:ss.fff";
	}

	public abstract class OleDbRepository<TOleDbEntity> : OleDbRepository, IDisposable
		where TOleDbEntity : class
	{
		protected OleDbEntityContext Context;

		protected OleDbRepository(OleDbEntityContext context)
		{
			Context = context;
		}

		public virtual List<TOleDbEntity> GetList(String filter = "", OleDbParameter[] filterParameters = null, String selectColumns = null, 
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
			return AssembleClassList(SelectQuery(selectColumns, FromTables(), "", filter, filterParameters, orderBy, paging));
		}

		public List<TOleDbEntity> GetListByIds<T>(IEnumerable<T> ids, String selectColumns = null, String orderBy = null, Paging paging = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return AssembleClassList(SelectQuery(selectColumns, FromTables(), "", FilterIn(ids), null, orderBy, paging));
		}

		public virtual TOleDbEntity GetEntity(String filter = "", OleDbParameter[] filterParameters = null, String selectColumns = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return ReadAndAssembleClass(SelectQuery(selectColumns, FromTables(), "", filter, filterParameters, null, new Paging(1)));
		}

		public TOleDbEntity GetById(Object id, String selectColumns = null)
		{
			selectColumns = selectColumns ?? SelectAllColunms();
			return ReadAndAssembleClass(SelectQuery(selectColumns, FromTables(), "", WhereClause(), Parameters(id)));
		}

		//public virtual Boolean Exists(String filter = "", SqlParameter[] filterParameters = null, String selectColumns = null)
		//{
		//	selectColumns = selectColumns ?? SelectAllColunms();
		//	return SelectQuery(selectColumns, filter, filterParameters, null, new Paging(1)).HasRows;
		//}

		public void Insert(TOleDbEntity sqlEntity)
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

		public virtual void Update(TOleDbEntity sqlEntity)
		{
			if (SaveChangesTransactionally)
			{
				Context.BeginTransaction();
			}

			if (sqlEntity is IModifiable)
			{
				((IModifiable)sqlEntity).ModifiedDate = NowDateTime;
			}

			UpdateCore(sqlEntity, GetUpdateColumnValuePairs(sqlEntity), GetUpdateKeyColumnValuePair(sqlEntity));
		}

		public Int64 Count(String filter = "", OleDbParameter[] filterParameters = null)
		{
			if (filterParameters != null && filter == null)
			{
				throw new ArgumentException("FilterParameters can only be applied to a filtered result.");
			}

			String query = null;

			if (!String.IsNullOrEmpty(filter))
			{
				filter = " where " + filter;
			}

			if (filterParameters == null)
			{
				filterParameters = new OleDbParameter[] { };
			}

			query = "Select Count(*) from {0}{1}".FormatX(FromTables(), filter);
			return Context.ExecuteNonQuery(query, filterParameters);
		}

		public Boolean Any(String filter = "", OleDbParameter[] filterParameters = null)
		{
			if (filterParameters != null && filter == null)
			{
				throw new ArgumentException("FilterParameters can only be applied to a filtered result.");
			}

			String query = null;

			if (!String.IsNullOrEmpty(filter))
			{
				filter = " where " + filter;
			}

			if (filterParameters == null)
			{
				filterParameters = new OleDbParameter[] { };
			}

			query = "If Exists (Select * from {0}{1}) Then Print 1 Else Print 0".FormatX(FromTables(), filter);
			return Context.ExecuteNonQuery(query, filterParameters) == 1;
		}


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

		public virtual Int32 Delete(Object id)
		{
			var query = "delete from {0} where {1} = @PrimaryKeyName".FormatX(TableName, PrimaryKeyName);
			return Context.ExecuteNonQuery(query, new OleDbParameter[]
			{
				new OleDbParameter("@PrimaryKeyName", id)
			});
		}

		public virtual Int32 Delete(String filter = "", OleDbParameter[] filterParameters = null)
		{
			if (filter.IsEmpty())
			{
				throw new ArgumentException("Filter is required.");
			}

			if (filterParameters != null && filter == null)
			{
				throw new ArgumentException("FilterParameters can only be applied to a filtered result.");
			}

			if (filterParameters == null)
			{
				filterParameters = new OleDbParameter[] { };
			}

			var query = $"delete from {TableName} where {filter}";
			return Context.ExecuteNonQuery(query, filterParameters);
		}

		public abstract void Delete(TOleDbEntity entityToDelete);

		protected OleDbDataReader SelectQuery(String selectColumns, String fromTables, String join = "", String filter = "", OleDbParameter[] filterParameters = null, String orderBy = null, 
			Paging paging = null)
		{
			String query = null;

			if (!join.IsEmpty())
			{
				join = join.Surround(ValueLib.Space.StringValue);
			}

			if (!String.IsNullOrEmpty(filter))
			{
				filter = " where " + filter;
			}

			if (filterParameters == null)
			{
				filterParameters = new OleDbParameter[] {};
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

			query = $"select {takeFilter}{selectColumns} from {fromTables}{join}{filter}{orderBy}";
			return Context.ExecDataReader(query, filterParameters);
		}

		protected void InsertCore(TOleDbEntity sqlEntity, String insertColumns, String insertValues)
		{
			String insertStatement = InsertColumnsPattern.FormatX(TableName, insertColumns, insertValues);
			SetEntityPrimaryKey(sqlEntity, Context.ExecScalar(insertStatement, new OleDbParameter[] {}));
		}

		protected void UpdateCore(TOleDbEntity sqlEntity, String updateColumnValuePairs, String updateKeyColumValuePair)
		{
			// NB: Don't need to update the primary key because it doesn't change nor is returned from an Update SQL query.
			String updateStatement = UpdateColumnsPattern.FormatX(TableName, updateColumnValuePairs, updateKeyColumValuePair);
			Context.ExecScalar(updateStatement, new OleDbParameter[] { });
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

		protected virtual OleDbParameter[] Parameters(Object primaryKeyValue)
		{
			var primaryKeyParameter = new OleDbParameter("@pk", primaryKeyValue);
			return new OleDbParameter[] { primaryKeyParameter };
		}

		private String FilterIn<T>(IEnumerable<T> ids)
		{
			return "{0} In ({1})".FormatX(PrimaryKeyName, String.Join(",", (from idItem in ids select ObtainValue<Object>(idItem)).ToArray()));
		}

		public Boolean SaveChangesTransactionally { get; set; }

		protected abstract List<TOleDbEntity> AssembleClassList(OleDbDataReader dataReader);
		protected abstract TOleDbEntity ReadAndAssembleClass(OleDbDataReader dataReader);
		protected abstract DateTime NowDateTime { get; }
		protected abstract String TableName { get; }
		protected abstract String PrimaryKeyName { get; }
		protected abstract String InsertColunms();
		protected abstract String GetInsertValues(TOleDbEntity sqlEntity);
		protected abstract String GetUpdateColumnValuePairs(TOleDbEntity sqlEntity);
		protected abstract String GetUpdateKeyColumnValuePair(TOleDbEntity sqlEntity);
		protected abstract void SetEntityPrimaryKey(TOleDbEntity sqlEntity, Object newPrimaryKey);

		#region IDisposable Support

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(Boolean disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Context?.Dispose();
				}
			}
			_disposedValue = true;
		}

		private Boolean _disposedValue;

		#endregion

		protected const String UpdateColumnValuePattern = "{0} = {1}";
		private const String InsertColumnsPattern = "Insert {0} ({1}) Values({2})";
		private const String UpdateColumnsPattern = "Update {0} Set {1} Where {2}";
	}
}