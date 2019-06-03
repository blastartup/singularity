using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class SqlRepository<TSqlEntity, TIdentity, TSqlEntityContext>
		where TSqlEntity : class
		where TIdentity : struct
		where TSqlEntityContext : SqlEntityContext, new()
	{
		protected TSqlEntityContext Context;

		protected SqlRepository(TSqlEntityContext context) 
		{
			Context = context;
		}

		#region Local Transaction

		public SqlTransaction BeginTransaction()
		{
			return Context.BeginTransaction();
		}

		public Boolean Commit()
		{
			return Context.Commit();
		}

		public void Rollback()
		{
			Context.Rollback();
		}

		#endregion

		#region Selecting

		/// <remarks>If this table doesn't have a primary key, you must override this method and throw an InvalidOperation exception.</remarks>
		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		public TSqlEntity GetEntity(TIdentity id)
		{
			if (id.IsEmpty())
			{
				throw new ArgumentException("The id argument cannot be null or empty.  Please pass an id.");
			}

			SqlDataReader reader = null;
			try
			{
				reader = Context.ExecuteDataReader(GetEntityQuery, new SqlParameter[] { new SqlParameter("@pk", id), });
				return ReadAndAssembleClass(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		/// <remarks>If this table doesn't have a primary key, you must override this method and throw an InvalidOperation exception.</remarks>
		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		public TSqlEntity GetPartialEntity(TIdentity id, String selectColumns)
		{
			if (id.IsEmpty())
			{
				throw new ArgumentException("The id argument cannot be null or empty.  Please pass an id or use an new TSqlEntity instead.");
			}

			if (selectColumns.IsEmpty())
			{
				throw new ArgumentException("The selectColumns argument cannot be null or empty.  Please pass a criteria or use GetEntity() instead.");
			}

			SqlDataReader reader = null;
			try
			{
				reader = Context.ExecuteDataReader(GetPartialEntityQuery.FormatX(selectColumns), new SqlParameter[] { new SqlParameter("@pk", id), });
				return ReadAndAssembleClass(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		//eg: "select top 1 [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where {0 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{1 = 'order by {orderBy}'}";
		public TSqlEntity FindEntity(String where, String parameterName, Object parameterValue, String orderBy = null)
		{
			if (parameterName[0] != '@')
			{
				throw new ArgumentException("The parameterName argument must start with the '@' symbol.");
			}

			return FindEntity(where, (new SqlParameter(parameterName, parameterValue)).ToEnumerable(), orderBy);
		}

		public TSqlEntity FindEntity(String where, IEnumerable<SqlParameter> sqlParameters, String orderBy = null)
		{
			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria or use GetEntity() instead.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			SqlDataReader reader = null;
			try
			{
				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				reader = Context.ExecuteDataReader(FindEntityQuery.FormatX(where, orderBy), sqlParameters.ToArray());
				return ReadAndAssembleClass(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		//eg: "select top 1 [PrimaryKeyName], {0} from dbo.[TableName] where {1 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{2 = 'order by {orderBy}'}";
		public TSqlEntity FindPartialEntity(String selectColumns, String where, IEnumerable<SqlParameter> sqlParameters, String orderBy = null)
		{
			if (selectColumns.IsEmpty())
			{
				throw new ArgumentException("The selectColumns argument cannot be null or empty.  Please pass a criteria or use FindEntity() instead.");
			}

			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria or use GetEntity() instead.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			SqlDataReader reader = null;
			try
			{
				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				reader = Context.ExecuteDataReader(FindPartialEntityQuery.FormatX(selectColumns, where, orderBy), sqlParameters.ToArray());
				return ReadAndAssembleClass(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] {0 = 'order by {orderBy}'}{1 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		public IEnumerable<TSqlEntity> GetEntities(String orderBy = null, Paging paging = null)
		{
			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			SqlDataReader reader = null;
			try
			{
				SqlParameter[] sqlParameters;
				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				var pageBy = String.Empty;
				if (paging != null)
				{
					pageBy = PagingSubClause;
					sqlParameters = new SqlParameter[] { new SqlParameter("@Skip", paging.Skip), new SqlParameter("@Take", paging.Take) };
				}
				else
				{
					sqlParameters = new SqlParameter[] { };
				}

				reader = Context.ExecuteDataReader(GetAllEntitiesQuery.FormatX(orderBy, pageBy), sqlParameters);
				return AssembleClasses(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] {1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		public IEnumerable<TSqlEntity> GetPartialEntities(String selectColumns, String orderBy = null, Paging paging = null)
		{
			if (selectColumns.IsEmpty())
			{
				throw new ArgumentException("The selectColumns argument cannot be null or empty.  Please pass a criteria or use GetEntities() instead.");
			}

			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			SqlDataReader reader = null;
			try
			{
				SqlParameter[] sqlParameters;
				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				var pageBy = String.Empty;
				if (paging != null)
				{
					pageBy = PagingSubClause;
					sqlParameters = new SqlParameter[] { new SqlParameter("@Skip", paging.Skip), new SqlParameter("@Take", paging.Take) };
				}
				else
				{
					sqlParameters = new SqlParameter[] { };
				}

				reader = Context.ExecuteDataReader(GetAllPartialEntitiesQuery.FormatX(selectColumns, orderBy, pageBy), sqlParameters);
				return AssembleClasses(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		/// <remarks>If this table doesn't have a primary key, you must override this method and throw an InvalidOperation exception.</remarks>
		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] in (@pk1, @pk2, @pk3){0 = 'order by {orderBy}'}{1 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		public IEnumerable<TSqlEntity> GetEntities(IEnumerable<TIdentity> ids, String orderBy = null, Paging paging = null)
		{
			if (ids.IsEmpty())
			{
				throw new ArgumentException("The ids argument cannot be null or empty.  Please pass a list of ids.");
			}

			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			SqlDataReader reader = null;
			try
			{
				var index = 0;
				IEnumerable<SqlParameter> sqlParameters = ids.Select<TIdentity, SqlParameter>(x =>
				{
					var sqlParameter = new SqlParameter("@pk" + index.ToString(), x);
					index++;
					return sqlParameter;
				});

				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				var pageBy = String.Empty;
				if (paging != null)
				{
					pageBy = PagingSubClause;
					sqlParameters = sqlParameters.Concat(new SqlParameter[] { new SqlParameter("@Skip", paging.Skip), new SqlParameter("@Take", paging.Take) });
				}

				reader = Context.ExecuteDataReader(GetEntitiesQuery.FormatX(orderBy, pageBy), sqlParameters.ToArray());
				return AssembleClasses(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		/// <remarks>If this table doesn't have a primary key, you must override this method and throw an InvalidOperation exception.</remarks>
		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] in (@pk1, @pk2, @pk3){1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		public IEnumerable<TSqlEntity> GetPartialEntities(IEnumerable<TIdentity> ids, String selectColumns, String orderBy = null, Paging paging = null)
		{
			if (ids.IsEmpty())
			{
				throw new ArgumentException("The ids argument cannot be null or empty.  Please pass a list of ids.");
			}

			if (selectColumns.IsEmpty())
			{
				throw new ArgumentException("The selectColumns argument cannot be null or empty.  Please pass a criteria or use GetEntities() instead.");
			}

			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			SqlDataReader reader = null;
			try
			{
				var index = 0;
				IEnumerable<SqlParameter> sqlParameters = ids.Select<TIdentity, SqlParameter>(x =>
				{
					var sqlParameter = new SqlParameter("@pk" + index.ToString(), x);
					index++;
					return sqlParameter;
				});

				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				var pageBy = String.Empty;
				if (paging != null)
				{
					pageBy = PagingSubClause;
					sqlParameters = sqlParameters.Concat(new SqlParameter[] { new SqlParameter("@Skip", paging.Skip), new SqlParameter("@Take", paging.Take) });
				}

				reader = Context.ExecuteDataReader(GetPartialEntitiesQuery.FormatX(selectColumns, orderBy, pageBy), sqlParameters.ToArray());
				return AssembleClasses(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where {0 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		public IEnumerable<TSqlEntity> FindEntities(String where, IEnumerable<SqlParameter> sqlParameters, String orderBy = null, Paging paging = null)
		{
			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria or use GetEntity() instead.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			SqlDataReader reader = null;
			try
			{
				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				var pageBy = String.Empty;
				if (paging != null)
				{
					pageBy = PagingSubClause;
					sqlParameters = sqlParameters.Concat(new SqlParameter[] { new SqlParameter("@Skip", paging.Skip), new SqlParameter("@Take", paging.Take) });
				}

				reader = Context.ExecuteDataReader(FindEntitiesQuery.FormatX(where, orderBy, pageBy), sqlParameters.ToArray());
				return AssembleClasses(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] where {1 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{2 = 'order by {orderBy}'}{3 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		public IEnumerable<TSqlEntity> FindPartialEntities(String selectColumns, String where, IEnumerable<SqlParameter> sqlParameters, String orderBy = null, Paging paging = null)
		{
			if (selectColumns.IsEmpty())
			{
				throw new ArgumentException("The selectColumns argument cannot be null or empty.  Please pass a criteria or use GetEntities() instead.");
			}

			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria or use GetEntity() instead.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			if (paging != null && orderBy == null)
			{
				throw new ArgumentException("Paging can only be applied to an ordered result.");
			}

			SqlDataReader reader = null;
			try
			{
				orderBy = orderBy == null ? String.Empty : OrderBySubClause + orderBy;
				var pageBy = String.Empty;
				if (paging != null)
				{
					pageBy = PagingSubClause;
					sqlParameters = sqlParameters.Concat(new SqlParameter[] { new SqlParameter("@Skip", paging.Skip), new SqlParameter("@Take", paging.Take) });
				}

				reader = Context.ExecuteDataReader(FindPartialEntitiesQuery.FormatX(selectColumns, where, orderBy, pageBy), sqlParameters.ToArray());
				return AssembleClasses(reader);
			}
			finally
			{
				reader?.Dispose();
			}
		}

		public IEnumerable<TSqlEntity> GetListByQuery(String sqlQuery) => AssembleClasses(Context.ExecuteDataReader(sqlQuery));

		#endregion

		#region Inserting

		//eg: For Guid => "insert into dbo.[TableName] ([PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name]) values (@pk, @Col1Name, @Col2Name, @Col3Name)"
		//    For Identity => "insert into dbo.[TableName] ([Col1Name], [Col2Name], [Col3Name]) values (@Col1Name, @Col2Name, @Col3Name)  select @@identity"
		public Boolean Insert(TSqlEntity sqlEntity)
		{
			Boolean result;
			SqlCommand sqlCommand = Context.SqlConnection.CreateCommand();
			try
			{
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = InsertQuery;
				sqlCommand.Parameters.AddRange(PopulateInsertParameters(sqlEntity).ToArray());

				result = InsertCore(sqlCommand);
			}
			finally
			{
				sqlCommand.Dispose();
			}

			return result;
		}

		//eg: For Guid => "insert into dbo.[TableName] ([PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name]) values (@pk, @Col1Name, @Col2Name, @Col3Name)"
		//    For Identity => "insert into dbo.[TableName] ([Col1Name], [Col2Name], [Col3Name]) values (@Col1Name, @Col2Name, @Col3Name)  select @@identity"
		public Int32 Inserts(IEnumerable<TSqlEntity> sqlEntities)
		{
			var results = 0;
			SqlCommand sqlCommand = Context.SqlConnection.CreateCommand();
			try
			{
				sqlCommand.Transaction = Context.SqlTransaction;
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = InsertQuery;

				var asyncSqlCommand = new AsyncSqlCommand(sqlCommand);
				Parallel.ForEach(sqlEntities.AsParallel(), (e, loopState) =>
				{



					if (e is IModifiable modifiableEntity)
					{
						modifiableEntity.CreatedDate = NowDateTime;
						modifiableEntity.ModifiedDate = NowDateTime;
					}
					else if (e is ICreatable creatable)
					{
						creatable.CreatedDate = NowDateTime;
					}

					results++;
					asyncSqlCommand.Write(x => x.SharedWrite(PopulateInsertParameters(e)));

					var innerSqlCommand = asyncSqlCommand.Read(x => x.SharedRead());
					innerSqlCommand.Prepare();
					if (!InsertCore(innerSqlCommand))
					{
						results *= -1;
						loopState.Stop();
					}
				});
			}
			finally
			{
				sqlCommand.Dispose();
			}

			return results;
		}


		// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters
		// Only use for collections say greater than 100 but less than 1000.
		public Boolean InsertAsTable(IEnumerable<TSqlEntity> sqlEntities)
		{
			var table = new DataTable();
			SqlDataAdapter sqlDataAdapter = null;
			try
			{
				sqlDataAdapter = new SqlDataAdapter($"select top 0 * from {TableName}", Context.SqlConnection);
				sqlDataAdapter.Fill(table);
			}
			finally
			{
				sqlDataAdapter?.Dispose();
			}

			var asyncDataTable = new AsyncDataTable(table);
			sqlEntities.AsParallel().ForAll(e =>
			{
				DataRow row = asyncDataTable.Read(x => x.SharedRead());
				InsertRowValues(e, row);
				asyncDataTable.Write(x => x.SharedWrite(row));
			});

			SqlCommand insertCommand = null;
			try
			{
				insertCommand = new SqlCommand(InsertTableValuedQuery, Context.SqlConnection);
				insertCommand.Parameters.Add(new SqlParameter("@TableValued", table) { SqlDbType = SqlDbType.Structured, TypeName = "dbo." + TableName });
				insertCommand.ExecuteNonQuery();
			}
			finally
			{
				insertCommand?.Dispose();
				table.Clear();
			}

			return true;
		}


		// Only use for large collections say greater than 1000.
		public Boolean BulkInsert(IEnumerable<TSqlEntity> sqlEntities)
		{
			var table = new DataTable();
			SqlDataAdapter sqlDataAdapter = null;
			try
			{
				sqlDataAdapter = new SqlDataAdapter($"select top 0 * from {TableName}", Context.SqlConnection);
				sqlDataAdapter.Fill(table);
			}
			finally
			{
				sqlDataAdapter?.Dispose();
			}

			var asyncDataTable = new AsyncDataTable(table);
			sqlEntities.AsParallel().ForAll(e =>
			{
				DataRow row = asyncDataTable.Read(x => x.SharedRead());
				InsertRowValues(e, row);
				asyncDataTable.Write(x => x.SharedWrite(row));
			});

			SqlBulkCopy sqlBulkCopy = null;
			try
			{
				sqlBulkCopy = new SqlBulkCopy(Context.SqlConnection)
				{
					DestinationTableName = TableName
				};
				sqlBulkCopy.WriteToServer(table);
			}
			finally
			{
				sqlBulkCopy?.Close();
				table.Clear();
			}

			return true;
		}

		protected virtual Boolean InsertCore(SqlCommand sqlCommand, TSqlEntity sqlEntity = null)
		{
			return sqlCommand.ExecuteNonQuery() == 1;
		}

		#endregion

		#region Updating

		public Boolean UpdateEntity(TSqlEntity sqlEntity)
		{
			Boolean result;
			if (sqlEntity is IModifiable modifiable)
			{
				modifiable.ModifiedDate = NowDateTime;
			}

			SqlCommand sqlCommand = Context.SqlConnection.CreateCommand();
			try
			{
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = UpdateQuery;
				sqlCommand.Parameters.AddRange(PopulateUpdateParameters(sqlEntity).ToArray());

				result = UpdateCore(sqlCommand);
			}
			finally
			{
				sqlCommand.Dispose();
			}

			return result;
		}

		public Int32 UpdateEntities(IEnumerable<TSqlEntity> sqlEntities)
		{
			var results = 0;
			SqlCommand sqlCommand = Context.SqlConnection.CreateCommand();
			try
			{
				sqlCommand.Transaction = Context.SqlTransaction;
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = UpdateQuery;

				var asyncSqlCommand = new AsyncSqlCommand(sqlCommand);
				Parallel.ForEach(sqlEntities.AsParallel(), (e, loopState) =>
				{
					results++;
					asyncSqlCommand.Write(x => x.SharedWrite(PopulateInsertParameters(e)));

					var innerSqlCommand = asyncSqlCommand.Read(x => x.SharedRead());
					innerSqlCommand.Prepare();
					if (!UpdateCore(innerSqlCommand))
					{
						results *= -1;
						loopState.Stop();
					}
				});
			}
			finally
			{
				sqlCommand.Dispose();
			}

			return results;
		}

		protected virtual Boolean UpdateCore(SqlCommand sqlCommand, TSqlEntity sqlEntity = null)
		{
			return sqlCommand.ExecuteNonQuery() == 1;
		}

		#endregion

		#region Deactiviate

		public Boolean Deactivate(IDeletable deletableSqlEntity)
		{
			Boolean result;
			deletableSqlEntity.DeletedDate = NowDateTime;

			SqlCommand sqlCommand = Context.SqlConnection.CreateCommand();
			try
			{
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = UpdateQuery;
				sqlCommand.Parameters.AddRange(PopulateUpdateParameters((TSqlEntity)deletableSqlEntity).ToArray());

				result = sqlCommand.ExecuteNonQuery() == 1;
			}
			finally
			{
				sqlCommand.Dispose();
			}

			return result;
		}

		#endregion

		#region Deleting

		//eg: "delete dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		public Boolean DeleteEntity(TIdentity id)
		{
			if (id.IsEmpty())
			{
				throw new ArgumentException("The id argument cannot be null or empty.  Please pass an id.");
			}

			return Context.ExecuteNonQuery(DeleteEntityQuery, new SqlParameter[] { new SqlParameter("@pk", id), }) == 1;
		}

		public Boolean DeleteAllEntities() => Context.ExecuteNonQuery(DeleteAllEntitiesQuery, new SqlParameter[] { new SqlParameter("@TableName", TableName), }) == 1;
		private const String DeleteAllEntitiesQuery = "delete from dbo.@TableName";

		//eg: "delete dbo.[TableName] where {0 = dbo.[TableName].[Col1Name] = @Col1Name}";
		public Int64 DeleteSelectedEntities(String where, IEnumerable<SqlParameter> sqlParameters)
		{
			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria or use GetEntity() instead.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			return Context.ExecuteNonQuery(DeleteEntityQuery.FormatX(where), sqlParameters.ToArray());
		}

		public Int32 DeleteEntities(IEnumerable<TSqlEntity> sqlEntities)
		{
			var results = 0;
			SqlCommand sqlCommand = Context.SqlConnection.CreateCommand();
			try
			{
				sqlCommand.Transaction = Context.SqlTransaction;
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.CommandText = DeleteEntitiesQuery;

				var asyncSqlCommand = new AsyncSqlCommand(sqlCommand);
				sqlEntities.AsParallel().ForAll(e =>
				{
					results++;
					asyncSqlCommand.Write(x => x.SharedWrite(PopulateInsertParameters(e)));

				});

				var innerSqlCommand = asyncSqlCommand.Read(x => x.SharedRead());
				innerSqlCommand.Prepare();
				if (innerSqlCommand.ExecuteNonQuery() != 1)
				{
					results *= -1;
				}
			}
			finally
			{
				sqlCommand.Dispose();
			}

			return results;
		}

		public Boolean DeleteEntity(TSqlEntity entityToDelete)
		{
			return DeleteEntity(GetPrimaryKeyValue(entityToDelete));
		}

		#endregion

		#region Referencing

		public virtual void LoadReferences(TSqlEntity sqlEntity)
		{
			// Intentionally left blank.

			/* LoadReference example...
			 * var referenceRepository = new ReferenceRepository(Context);
			 *	sqlEntity.Reference = referenceRepository.GetById(sqlEntity.ReferenceId);
			 *
			 * foreach (TSqlEntity findEntity in FindEntities("TestId = @TestId", new SqlParameter[]{ new SqlParameter("@TestId", SqlDbType.UniqueIdentity) { Value = sqlEntity.TestId }}))
			 *	{
			 *		sqlEntity.TestCollection.Add(findEntity);
			 *	}
			 */
		}

		#endregion

		public Boolean TableExists() => Context.TableExists(TableName);

		public Int64 GetCount()
		{
			return Context.ExecuteScalar(GetCountQuery).ToInt64();
		}

		public Int64 FindCount(String where, IEnumerable<SqlParameter> sqlParameters)
		{
			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria or use GetCount() instead.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			return Context.ExecuteScalar(FindCountQuery.FormatX(TableName, where), sqlParameters.ToArray()).ToInt64();
		}
		private const String FindCountQuery = "Select Count(*) from {0} where {1}";

		public Boolean FindAny(String where, IEnumerable<SqlParameter> sqlParameters)
		{
			if (where.IsEmpty())
			{
				throw new ArgumentException("The where argument cannot be null or empty.  Please pass a criteria.");
			}

			if (sqlParameters == null)
			{
				throw new ArgumentException("The sqlParameters can be empty but not null.  Please pass an empty array.");
			}

			return Context.ExecuteScalar(FindAnyQuery.FormatX(TableName, where), sqlParameters.ToArray()).ToInt() == 1;
		}
		private const String FindAnyQuery = "If Exists (Select Count(*) from {0} where {1}) select 1 Else select 0";

		public void IdentityInsertOn()
		{
			SqlCommand cmd = new SqlCommand($"Set Identity_Insert dbo.{TableName} On", Context.SqlConnection);
			cmd.ExecuteNonQuery();
		}

		public void IdentityInsertOff()
		{
			SqlCommand cmd = new SqlCommand($"Set Identity_Insert dbo.{TableName} Off", Context.SqlConnection);
			cmd.ExecuteNonQuery();
		}


		//public List<TSqlEntity> GetDuplicates(String filter = "", SqlParameter[] filterParameters = null)
		//{
		//	if (filterParameters != null && filter == null)
		//	{
		//		throw new ArgumentException("FilterParameters can only be applied to a filtered result.");
		//	}

		//	if (ColumnsOfUniqueness.IsEmpty())
		//	{
		//		throw new InvalidOperationException("Indeterminate uniqueness.  ColumnOfUniqueness repository property not set.");
		//	}

		//	String dupeToOuterJoin = String.Join(" and ", ColumnsOfUniqueness.Split(ValueLib.Comma.CharValue).Select(v => $"o.{v} = dupes.{v}"));
		//	String orderByWithPk = String.Join(", ", ColumnsOfUniqueness.Split(ValueLib.Comma.CharValue).Select(v => $"o.{v}")) + $", o.{PrimaryKeyName}";

		//	if (!String.IsNullOrEmpty(filter))
		//	{
		//		filter = " where " + filter;
		//	}

		//	if (filterParameters == null)
		//	{
		//		filterParameters = new SqlParameter[] { };
		//	}

		//	String query =
		//		$"select {orderByWithPk} from {FromTables()} o inner join (select {ColumnsOfUniqueness} from {FromTables()} group by {ColumnsOfUniqueness} having count(*) > 1) dupes on {dupeToOuterJoin} {filter} order by {orderByWithPk}";
		//	return AssembleClassList(Context.ExecuteDataReader(query, filterParameters));
		//}


		//protected virtual String ColumnsOfUniqueness => String.Empty;

		//private String FilterIn<T>(IEnumerable<T> ids)
		//{
		//	return "{0} In ({1})".FormatX(PrimaryKeyName, String.Join(",", (from idItem in ids select ObtainValue<Object>(idItem)).ToArray()));
		//}

		//protected String ObtainValue<T>(T nativeValue)
		//{
		//	String result = null;
		//	if (nativeValue == null || Convert.IsDBNull(nativeValue))
		//	{
		//		result = "Null";
		//	}
		//	else if (nativeValue is String)
		//	{
		//		result = String.Format(StringValuePattern, nativeValue.ToString().Replace("'", "''"));
		//	}
		//	else if (nativeValue is DateTime)
		//	{
		//		result = String.Format(StringValuePattern, ((DateTime)(Object)nativeValue).ToString(DateTimeFormat));
		//	}
		//	else if (nativeValue is TimeSpan)
		//	{
		//		result = String.Format(StringValuePattern, new DateTime(((TimeSpan)(Object)nativeValue).Ticks).ToShortTimeString());
		//	}
		//	else if (nativeValue is Boolean)
		//	{
		//		result = Convert.ToInt32((Boolean)(Object)nativeValue).ToString();
		//	}
		//	else if (nativeValue is Guid)
		//	{
		//		result = String.Format(StringValuePattern, nativeValue.ToString());
		//	}
		//	else if (nativeValue.GetType().IsEnum)
		//	{
		//		result = Convert.ToInt32(nativeValue).ToString();
		//	}
		//	else if (nativeValue is Byte[])
		//	{
		//		result = (nativeValue as Byte[]).ToHexString();
		//	}
		//	else
		//	{
		//		result = nativeValue.ToString();
		//	}

		//	return result;
		//}
		//private const String StringValuePattern = "'{0}'";
		//private const String DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";


		public DateTime? SchemaModifiedDateTime => Context.TableSchemaModifiedDateTime(TableName);
		public virtual String CreateTableQuery => String.Empty;
		public virtual String AttachTableQuery => String.Empty;
		public String DeleteTableQuery => $"DROP TABLE [dbo].[{TableName}]";
		public virtual String DetachTableQuery => String.Empty;
		protected abstract String PrimaryKeyName { get; }
		protected abstract String GetCountQuery { get; }
		protected DateTime NowDateTime => Context.NowDateTime;

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		protected abstract String GetEntityQuery { get; }

		//eg: "select [PrimaryKeyName], {0} from [TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		protected abstract String GetPartialEntityQuery { get; }

		//eg: "select top 1 [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where {0 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{1 = 'order by {orderBy}'}";
		protected abstract String FindEntityQuery { get; }

		//eg: "select top 1 [PrimaryKeyName], {0} from dbo.[TableName] where {1 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{2 = 'order by {orderBy}'}";
		protected abstract String FindPartialEntityQuery { get; }

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] {0 = 'order by {orderBy}'}{1 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected abstract String GetAllEntitiesQuery { get; }

		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] {1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected abstract String GetAllPartialEntitiesQuery { get; }

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] in (@pk1, @pk2, @pk3){0 = 'order by {orderBy}'}{1 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected abstract String GetEntitiesQuery { get; }

		//eg: "select [PrimaryKeyName], {0} from [TableName] where dbo.[TableName].[PrimaryKeyName] in (@pk1, @pk2, @pk3){1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected abstract String GetPartialEntitiesQuery { get; }

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where {0 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected abstract String FindEntitiesQuery { get; }

		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] where {1 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{2 = 'order by {orderBy}'}{3 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}"
		protected abstract String FindPartialEntitiesQuery { get; }

		//eg: For Guid => "insert into dbo.[TableName] ([PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name]) values (@pk, @Col1Name, @Col2Name, @Col3Name)"
		//    For Identity => "insert into dbo.[TableName] ([Col1Name], [Col2Name], [Col3Name]) values (@Col1Name, @Col2Name, @Col3Name)  select @@identity"
		protected abstract String InsertQuery { get; }

		//eg: For Guid => "insert into dbo.[TableName] ([PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name]) select tv.[PrimaryKeyName], tv.[Col1Name], tv.[Col2Name], tv.[Col3Name] from @TableNameValues as tv;"
		//    For Identity => "insert into dbo.[TableName] ([Col1Name], [Col2Name], [Col3Name]) select tv.[PrimaryKeyName], tv.[Col1Name], tv.[Col2Name], tv.[Col3Name] from @TableValued as tv;"   Entities primary key will not be updated.
		protected abstract String InsertTableValuedQuery { get; }

		// Don't forget to populate the class with appropriate DateTime for ICreateable or IModifiable before populating the SqlParameters.
		// eg: For Guid => { new SqlParameter("@pk", SqlDbType.UniqueIdentifier) { Value = sqlEntity.PrimaryKey }, new SqlParameter("@Col1Name", SqlDbType.BigInt) { Value = sqlEntity.Col1Name }, ... }
		// This is effectively the same as PopulateUpdateParameters, with the exception that you need to include the PK for Guid PK'ed classes.
		protected abstract IEnumerable<SqlParameter> PopulateInsertParameters(TSqlEntity sqlEntity);

		//eg: "update dbo.[TableName] set [Col1Name] = @Col1Name, [Col2Name] = @Col2Name, [Col3Name] = @Col3Name"
		protected abstract String UpdateQuery { get; }

		//eg: "delete from dbo.[TableName] where dbo.[TableName].PrimaryKeyName in (@pk1, @pk2, @pk3)"
		protected abstract String DeleteEntitiesQuery { get; }

		// Don't forget to populate the class with appropriate DateTime for ICreateable or IModifiable before populating the SqlParameters.
		// NB: There are some columns an update doesn't and cannot update ever like the PrimaryKey and say a CreatedDateTime column.
		// eg: { new SqlParameter("@Col1Name", SqlDbType.BigInt) { Value = sqlEntity.Col1Name }, new SqlParameter("@Col2Name", SqlDbType.BigInt) { Value = sqlEntity.Col2Name }, ... }
		protected abstract IEnumerable<SqlParameter> PopulateUpdateParameters(TSqlEntity sqlEntity);

		// eg: row["PrimaryKeyName"] = sqlEntity.PrimaryKeyName; row["Col1Name"] = sqlEntity.Col1Name;
		protected abstract void InsertRowValues(TSqlEntity sqlEntity, DataRow row);

		//eg: "delete dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		protected abstract String DeleteEntityQuery { get; }

		protected abstract IEnumerable<TSqlEntity> AssembleClasses(SqlDataReader dataReader);
		protected abstract TSqlEntity ReadAndAssembleClass(SqlDataReader dataReader);
		protected abstract TIdentity GetPrimaryKeyValue(TSqlEntity entityToDelete);

		protected abstract String TableName { get; }

		private const String OrderBySubClause = " Order By ";
		private const String PagingSubClause = " OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY ";
	}

	public abstract class NumericKeySqlRepository<TSqlEntity, TSqlEntityContext> : SqlRepository<TSqlEntity, Int64, TSqlEntityContext>
		where TSqlEntity : class
		where TSqlEntityContext : SqlEntityContext, new()
	{
		protected NumericKeySqlRepository(TSqlEntityContext context) : base(context)
		{
		}


		public void Reseed(Int64 newPrimaryKey = 0)
		{
			newPrimaryKey--;
			SqlCommand cmd = new SqlCommand($"DBCC CheckIdent ({TableName}, reseed, {newPrimaryKey})", Context.SqlConnection);
			cmd.ExecuteNonQuery();
		}

		protected override Boolean InsertCore(SqlCommand sqlCommand, TSqlEntity sqlEntity = null)
		{
			Int64 newPk = sqlCommand.ExecuteScalar().ToInt64();
			SetPrimaryKey(sqlEntity, newPk);
			return true;  // If the insert command fails, an exception would be triggered and this line won't be executed.
		}

		protected abstract void SetPrimaryKey(TSqlEntity sqlEntity, Int64 newPrimaryKey);
	}

	public abstract class GuidKeySqlRepository<TSqlEntity, TSqlEntityContext> : SqlRepository<TSqlEntity, Guid, TSqlEntityContext>
		where TSqlEntity : class
		where TSqlEntityContext : SqlEntityContext, new()
	{
		protected GuidKeySqlRepository(TSqlEntityContext context) : base(context)
		{
		}

		protected void LoadRelatedEntities<TRelatedSqlEntity>(Func<String, IEnumerable<SqlParameter>, String, Paging, IEnumerable<TRelatedSqlEntity>> findEntities, Guid primaryKeyValue, ICollection<TRelatedSqlEntity> collection) where TRelatedSqlEntity : class
		{
			var relatedEntities = findEntities("{0} = @{0}".FormatX(PrimaryKeyName), new SqlParameter[]
			{
				new SqlParameter("@{0}".FormatX(PrimaryKeyName), SqlDbType.UniqueIdentifier) {Value = primaryKeyValue},
			}, null, null);
			foreach (TRelatedSqlEntity relatedEntity in relatedEntities)
			{
				collection.Add(relatedEntity);
			}
		}


		protected override IEnumerable<SqlParameter> PopulateInsertParameters(TSqlEntity sqlEntity)
		{
			return PopulateParameters(sqlEntity);
		}

		protected override IEnumerable<SqlParameter> PopulateUpdateParameters(TSqlEntity sqlEntity)
		{
			return PopulateParameters(sqlEntity);
		}

		protected abstract IEnumerable<SqlParameter> PopulateParameters(TSqlEntity sqlEntity);

	}


	/* Enhancements
	 *	============
	 *
	 *	Add a IDeleteable strategy so Get's and Updates must excluded deleted records and Deletes only deactive by default.
	 *	Add a security wrapper - does current user have access to insert/update/delete etc.
	 *	  Auditing - created by, modified by etc.
	 *	Add retry if timeout or other minor exceptions.
	 *	Add timestamp support for high volume concurrency support.
	 *
	 */
}