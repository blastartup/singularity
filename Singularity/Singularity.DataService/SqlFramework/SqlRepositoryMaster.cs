using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Singularity.DataService
{
	public abstract class SqlRepositoryMaster<TSqlEntity, TIdentity, TSqlEntityContext>
		where TSqlEntity : class
		where TIdentity : struct
		where TSqlEntityContext : SqlEntityContext, new()
	{
		protected TSqlEntityContext Context;

		protected SqlRepositoryMaster(TSqlEntityContext context)
		{
			Context = context;
		}

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

		public Int64 GetCount()
		{
			return Context.ExecuteScalar(GetCountQuery).ToInt64();
		}


		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		protected virtual String GetEntityQuery => @"SELECT tbl.[name], create_date 'CreatedDate', modify_date 'ModifiedDate', p.[rows] 'RowCount'
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where tbl.[name] = @pk";

		//eg: "select [PrimaryKeyName], {0} from [TableName] where dbo.[TableName].[PrimaryKeyName] = @pk";
		protected virtual String GetPartialEntityQuery => @"SELECT tbl.[name], {0}
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where tbl.[name] = @pk";

		//eg: "select top 1 [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where {0 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{1 = 'order by {orderBy}'}";
		protected virtual String FindEntityQuery => @"SELECT top 1 tbl.[name], create_date 'CreatedDate', modify_date 'ModifiedDate', p.[rows] 'RowCount'
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where {0}{1}";

		//eg: "select top 1 [PrimaryKeyName], {0} from dbo.[TableName] where {1 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{2 = 'order by {orderBy}'}";
		protected virtual String FindPartialEntityQuery => @"SELECT top 1 tbl.[name], {0}
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where {1}{2}";

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] {0 = 'order by {orderBy}'}{1 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected virtual String GetAllEntitiesQuery => @"SELECT tbl.[name], create_date 'CreatedDate', modify_date 'ModifiedDate', p.[rows] 'RowCount'
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
{0}{1}";

		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] {1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected virtual String GetAllPartialEntitiesQuery => @"SELECT tbl.[name], {0}
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
{1}{2}";

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where dbo.[TableName].[PrimaryKeyName] in (@pk1, @pk2, @pk3){0 = 'order by {orderBy}'}{1 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected virtual String GetEntitiesQuery => @"SELECT tbl.[name], create_date 'CreatedDate', modify_date 'ModifiedDate', p.[rows] 'RowCount'
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where tbl.[name] in ({0}){1}{2}";

		//eg: "select [PrimaryKeyName], {0} from [TableName] where dbo.[TableName].[PrimaryKeyName] in (@pk1, @pk2, @pk3){1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected virtual String GetPartialEntitiesQuery => @"SELECT tbl.[name], {0}
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where tbl.[name] in ({0}){1}{2}";

		//eg: "select [PrimaryKeyName], [Col1Name], [Col2Name], [Col3Name] from dbo.[TableName] where {0 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{1 = 'order by {orderBy}'}{2 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}";
		protected virtual String FindEntitiesQuery => @"SELECT tbl.[name], create_date 'CreatedDate', modify_date 'ModifiedDate', p.[rows] 'RowCount'
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where {0}{1}{2}";

		//eg: "select [PrimaryKeyName], {0} from dbo.[TableName] where {1 = '[Col1Name] = @Col1Name and [Col2Name] = @Col2Name'}{2 = 'order by {orderBy}'}{3 = 'offset (@Skip) Rows Fetch Next (@Take) Rows Only'}"
		protected virtual String FindPartialEntitiesQuery => @"SELECT tbl.[name], {0}
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where {1}{2}{3}";

		protected virtual String GetCountQuery => @"SELECT count(tbl.[name])
FROM sys.tables AS tbl
  INNER JOIN sys.indexes AS idx ON idx.object_id = tbl.object_id and idx.index_id < 2
  INNER JOIN sys.partitions AS p ON p.object_id=CAST(tbl.object_id AS int) AND p.index_id=idx.index_id
where left(tbl.[name], 1) != '_'";

		public IEnumerable<TSqlEntity> GetListByQuery(String sqlQuery) => AssembleClasses(Context.ExecuteDataReader(sqlQuery));

		#endregion

		protected virtual String FindAnyQuery => "If Exists (Select {0}Id from {0} where {1}) select 1 Else select 0";
		protected abstract IEnumerable<TSqlEntity> AssembleClasses(SqlDataReader dataReader);
		protected abstract TSqlEntity ReadAndAssembleClass(SqlDataReader dataReader);
		protected abstract String TableName { get; }

		private const String OrderBySubClause = " Order By ";
		private const String PagingSubClause = " OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY ";
	}
}