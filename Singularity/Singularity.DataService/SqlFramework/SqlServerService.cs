using System;
using System.Data;
using System.Data.SqlClient;
using Singularity.DataService.SqlFramework;
using Singularity.DataService.SqlFramework.Models;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public sealed class SqlServerService
	{
		public SqlServerService(SqlConnection sqlConnection)
			=> _sqlConnection = sqlConnection;

		public DateTime? SchemaModifiedDateTime(String tableName = null)
		{
			String sqlQuery;
			SqlCommand sqlCommand;
			if (tableName.IsEmpty())
			{
				sqlQuery = "select top 1 modify_date from sys.objects where (type = 'U') order by modify_date desc";
				sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			}
			else
			{
				sqlQuery = "select modify_date from sys.objects where type = 'U' and name = '@TableName'";
				sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
				sqlCommand.Parameters.AddWithValue("@TableName", tableName);
			}
			return sqlCommand.ExecuteScalar().ToNullableDateTime();
		}

		public String DatabaseName => _sqlConnection.Database;
		public String DataSource => _sqlConnection.DataSource;

		public void LoadColumns(SqlTable sqlTable)
		{
			var sqlQuery = @"SELECT s.name [Schema], c.column_id, c.Name, st.Name As SystemDataType, ut.Name AS UserDataType, 
	Case When st.Name in ('nvarchar', 'nchar') THEN c.max_length / 2 Else c.max_length End [Length], 
	ut.precision AS Precision, ut.scale, c.is_nullable, IsNull(pk.is_primary_key, 0) is_primary_key, IsNull(pk.is_unique, 0) is_unique, c.is_computed, 
	Case When st.Name in ('nvarchar', 'varchar') Then 1 Else 0 End [IsVariableLength], dc.definition, dc.name constraint_Name,
	pk.[Name] IndexName, ic.key_ordinal, ic.is_descending_key
FROM sys.tables tbl
	inner join sys.columns c on tbl.object_id = c.object_id
	inner join sys.objects o ON o.object_id = c.object_id
	left join  sys.types ut on ut.user_type_id  = c.user_type_id   
	left join sys.types st on c.system_type_id = st.system_type_id and c.system_type_id = st.user_type_id
	left join sys.index_columns ic on c.column_id = ic.column_id and c.object_id = ic.object_id
	left join sys.indexes pk on ic.object_id = pk.object_id and ic.index_id = pk.index_id
	inner join sys.schemas s on tbl.schema_id = s.schema_id
	left join sys.default_constraints dc on c.column_id = dc.parent_column_id and tbl.object_id = dc.parent_object_id
WHERE o.type = 'U' and o.Name = @TableName
ORDER BY c.column_id";
			var sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			sqlCommand.Parameters.Add(new SqlParameter("@TableName", sqlTable.Name));
			SqlDataReader sqlDataReader = null;
			try
			{
				sqlDataReader = sqlCommand.ExecuteReader();
				var previousColumnId = -1;
				var newSqlColumn = new SqlColumn();
				while (sqlDataReader.Read())
				{
					var currentColumnId = sqlDataReader["column_id"].ToInt();
					if (currentColumnId != previousColumnId)
					{
						newSqlColumn = new SqlColumn(sqlDataReader["name"].ToString())
						{
							Name = sqlDataReader["Name"].ToString(),
							OrdinalPosition = currentColumnId,
							Computed = sqlDataReader["is_computed"].ToBool(),
							InPrimaryKey = sqlDataReader["is_primary_key"].ToBool(),
							Length = sqlDataReader["Length"].ToInt(),
							Precision = sqlDataReader["Precision"].ToInt(),
							Scale = sqlDataReader["scale"].ToInt(),
							Nullable = sqlDataReader["is_nullable"].ToBool(),
							VariableLength = sqlDataReader["IsVariableLength"].ToBool(),
							DefaultSchema = sqlDataReader["Schema"].ToString(),
							UserDefinedDataTypeName = sqlDataReader["UserDataType"].ToString()
						};
						ESqlDataTypes result = ESqlDataTypes.NVarChar;
						var systemDataType = sqlDataReader["SystemDataType"].ToString();
						if (Enum.TryParse(systemDataType, true, out result))
						{
							newSqlColumn.ESqlDataType = result;
						}

						if (newSqlColumn.UserDefinedDataTypeName.Equals(systemDataType, StringComparison.OrdinalIgnoreCase))
						{
							newSqlColumn.UserDefinedDataTypeName = null;
						}

						var defaultConstraintName = sqlDataReader["constraint_name"].ToString();
						if (!defaultConstraintName.IsEmpty())
						{
							newSqlColumn.DefaultConstraint = new SqlDefaultConstraint()
							{
								Name = defaultConstraintName,
								Text = sqlDataReader["definition"].ToString(),
							};
							newSqlColumn.DefaultConstraint.Value = newSqlColumn.DefaultConstraint.Text;
							while (newSqlColumn.DefaultConstraint.Value[0] == '(')
							{
								newSqlColumn.DefaultConstraint.Value = newSqlColumn.DefaultConstraint.Value.Desurround();
							}
						}

						sqlTable.Columns.Add(newSqlColumn);
						previousColumnId = currentColumnId;
					}

					if (sqlDataReader["IndexName"] != null)
					{
						var sqlIndex = new SqlIndex(sqlDataReader["IndexName"].ToString())
						{
							IsPrimaryKey = sqlDataReader["is_primary_key"].ToBool(),
							Ordinal = sqlDataReader["key_ordinal"].ToByte(),
							IsDescending = sqlDataReader["is_descending_key"].ToBool()
						};
						newSqlColumn.Indexes.Add(sqlIndex);
					}
				}
			}
			finally
			{
				sqlDataReader?.Dispose();
			}
		}

		public SqlServer SqlServer()
		{
			return _sqlServer ?? (_sqlServer = NewSqlServer());
		}
		private SqlServer _sqlServer;

		private SqlServer NewSqlServer()
		{
			var results = new SqlServer();
			results.State = _sqlConnection.State;
			results.ServerVersion = _sqlConnection.ServerVersion;

			var sqlQuery = "select * from sys.dm_server_services";
			var sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			SqlDataReader serviceDataReader = null;
			try
			{
				serviceDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				serviceDataReader.Read();
				if (serviceDataReader.HasRows)
				{
					results.ServiceFullName = serviceDataReader["servicename"].ToString();
					results.ServiceAccount = serviceDataReader["service_account"].ToString();
					results.ServiceStartupTypeDescription = serviceDataReader["startup_type_desc"].ToString();
					results.ServiceStatusDescription = serviceDataReader["status_desc"].ToString();
					results.ServiceProcessId = serviceDataReader["process_id"].ToString();
					results.ServiceLastStatupTime = serviceDataReader["last_startup_time"].ToDateTime();
				}
			}
			catch (SqlException e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				serviceDataReader?.Dispose();
			}

			sqlQuery = "select @@servername As ServerName, @@SERVICENAME As ServiceName, @@VERSION As [Version], SERVERPROPERTY('ProductLevel') AS ProductLevel, SERVERPROPERTY('Edition') AS Edition, SERVERPROPERTY('ProductVersion') AS ProductVersion";
			sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			SqlDataReader serverDataReader = null;
			try
			{
				serverDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				serverDataReader.Read();
				if (serverDataReader.HasRows)
				{
					results.Name = serverDataReader["ServerName"].ToString();
					results.ServiceName = serverDataReader["ServiceName"].ToString();
					results.Edition = serverDataReader["Edition"].ToString();
					results.IsSqlServerExpress = results.Edition.Contains("Express", StringComparison.OrdinalIgnoreCase);
					results.ServerVersionDescription = serverDataReader["Version"].ToString();
					results.ProductLevel = serverDataReader["ProductLevel"].ToString();
				}
			}
			catch (SqlException e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				serviceDataReader?.Dispose();
			}

			return results;
		}

		private SqlDatabase NewSqlDatabase()
		{
			var results = new SqlDatabase(DatabaseName);
			var sqlQuery = "select o.[type], o.[create_date], o.[modify_date], s.name [Schema], t.name from sys.objects o inner join sys.tables t on o.object_id = t.object_id inner join sys.schemas s on o.Schema_id = s.schema_id where o.type in ('U','S','IT')";
			var sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			SqlDataReader sqlDataReader = null;
			try
			{
				sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					results.SqlTables.Add(new SqlTable(sqlDataReader["name"].ToString())
					{
						IsSystemObject = sqlDataReader["type"].ToString().In("U", "IT"),
						CreateDate = sqlDataReader["create_date"].ToDateTime(),
						ModifiedDate = sqlDataReader["modify_date"].ToDateTime(),
						Database = _sqlDatabase,
						DefaultSchema = sqlDataReader["Schema"].ToString()
					});
				}
			}
			finally
			{
				sqlDataReader?.Dispose();
			}

			sqlQuery = "select ut.Name UserDefinedTypeName, st.Name SystemTypeName, ut.max_length, ut.precision, ut.scale, ut.is_nullable from sys.types ut inner join sys.types st on ut.system_type_id = st.user_type_id where ut.is_user_defined = 1";
			sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			sqlDataReader = null;
			try
			{
				sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					var udt = new SqlUserDefinedDataType(sqlDataReader["UserDefinedTypeName"].ToString())
					{
						Length = sqlDataReader["max_length"].ToInt(),
						Precision = sqlDataReader["precision"].ToInt(),
						Scale = sqlDataReader["scale"].ToInt(),
						IsNullable = sqlDataReader["is_nullable"].ToBool()
					};
					ESqlDataTypes systemDataType = ESqlDataTypes.None;
					if (Enum.TryParse(sqlDataReader["SystemTypeName"].ToString(), out systemDataType))
					{
						udt.ESqlDataType = systemDataType;
					}
					else
					{
						udt.ESqlDataType = ESqlDataTypes.None;
					}

					results.SqlUserDefinedDataTypes.Add(udt);
				}
			}
			finally
			{
				sqlDataReader?.Dispose();
			}

			return results;
		}

		public SqlDatabase SqlDatabase()
		{
			return _sqlDatabase ?? (_sqlDatabase = NewSqlDatabase());
		}
		private SqlDatabase _sqlDatabase;

		public SqlJobService SqlJobService => _sqlJobService ?? (_sqlJobService = new SqlJobService(this));
		private SqlJobService _sqlJobService;

		internal SqlConnection SqlConnection => _sqlConnection;
		private readonly SqlConnection _sqlConnection;
	}
}
