using System.Data.SqlClient;

// ReSharper disable once CheckNamespace

namespace Singularity.DataService
{
	public static class SqlConnectionStringBuilderExtension
	{
		public static SqlConnection ToSqlConnection(this SqlConnectionStringBuilder builder)
		{
			return new SqlConnection(builder.ConnectionString);
		}
	}
}
