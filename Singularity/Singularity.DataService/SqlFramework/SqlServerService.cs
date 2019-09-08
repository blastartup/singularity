using System;
using System.Data.SqlClient;
using Singularity.DataService.SqlFramework;

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
			if (tableName.IsEmpty())
			{
				sqlQuery = "select top 1 modify_date from sys.objects where (type = 'U') order by modify_date desc";
			}
			else
			{
				sqlQuery = $"select modify_date from sys.objects where type = 'U' and name = '{tableName}'";
			}
			var sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			return sqlCommand.ExecuteScalar().ToNullableDateTime();
		}

		public String DatabaseName => _sqlConnection.Database;
		public String DataSource => _sqlConnection.DataSource;

		public SqlServer SqlServer()
		{
			return _sqlServer ?? (_sqlServer = NewSqlServer());
		}
		private SqlServer _sqlServer;

		public SqlDatabase SqlDatabase()
		{
			return _sqlDatabase ?? (_sqlDatabase = NewSqlDatabase());
		}
		private SqlDatabase _sqlDatabase;

		private SqlServer NewSqlServer()
		{
			var results = new SqlServer();

			var sqlQuery = "SELECT @@servername";
			var sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			results.Name = sqlCommand.ExecuteScalar().ToString();

			sqlQuery = "SELECT @@servicename";
			sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			results.InstanceName = sqlCommand.ExecuteScalar().ToString();

			sqlQuery = "SELECT serverproperty('Edition')";
			sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			results.IsSqlServerExpress = sqlCommand.ExecuteScalar().ToString().StartsWith("Express");

			sqlQuery = "select ServiceName from sys.dm_server_services";
			sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			results.DisplayName = sqlCommand.ExecuteScalar().ToString();

			sqlQuery = "select Service_Account from sys.dm_server_services";
			sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			results.ServiceAccount = sqlCommand.ExecuteScalar().ToString();

			results.ServiceName = new Words(results.ServiceAccount, ValueLib.ForwardSlash.StringValue)[1];

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

		private readonly SqlConnection _sqlConnection;
	}
}
