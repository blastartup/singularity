using System;
using System.Data.SqlClient;

namespace Singularity.DataService
{
	public sealed class SqlServerSystem
	{
		public SqlServerSystem(SqlConnection sqlConnection)
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
				sqlQuery = $"select modify_date from sys.objects where (type = 'U') and name = '{tableName}'";
			}
			var sqlCommand = new SqlCommand(sqlQuery, _sqlConnection);
			return sqlCommand.ExecuteScalar().ToNullableDateTime();
		}

		private readonly SqlConnection _sqlConnection;
	}
}
