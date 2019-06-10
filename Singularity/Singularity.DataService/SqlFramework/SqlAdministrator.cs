using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlAdministrator
	{
		public SqlAdministrator(SqlConnection masterSqlConnection)
		{
			_masterSqlConnection = masterSqlConnection;
		}

		public virtual Boolean CreateDatabase(String databaseName)
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(CreateDatabaseQuery.FormatX(databaseName));

			try
			{
				_masterSqlConnection.Open();
				return ExecuteMultiLinedSql(_masterSqlConnection, scriptBuilder.ToNewLineDelimitedString());
			}
			catch
			{
				return false;
			}
			finally
			{
				_masterSqlConnection.Close();
			}

		}

		public Boolean DatabaseExists(SqlConnectionStringBuilder databaseConnectionString)
		{
			return DatabaseExists(databaseConnectionString.InitialCatalog);
		}

		public Boolean DatabaseExists(String databaseName)
		{
			try
			{
				_masterSqlConnection.Open();
				var sqlCommand = new SqlCommand(DatabaseExistsQuery, _masterSqlConnection)
				{
					CommandType = CommandType.Text,
					Parameters = { new SqlParameter("@DatabaseName", databaseName) }
				};
				return sqlCommand.ExecuteScalar().ToInt() == 1;
			}
			catch
			{
				return false;
			}
			finally
			{
				_masterSqlConnection.Close();
			}
		}

		internal static Boolean ExecuteMultiLinedSql(SqlConnection sqlConnection, String mulitLinedSqlScript, SqlTransaction sqlTransaction = null)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = sqlConnection.CreateCommand();
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.Transaction = sqlTransaction;
				var commands = new Words(mulitLinedSqlScript.Replace(ValueLib.CrLf.StringValue + "GO", "|").Replace(ValueLib.CrLf.StringValue, ValueLib.Space.StringValue), "|");
				foreach (var command in commands)
				{
					sqlCommand.CommandText = command;
					FK_dbo.ControlPoco_dbosqlCommand.ExecuteNonQuery();
				}
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}

		protected virtual String CreateDatabaseQuery => String.Empty;

		public virtual Boolean DeleteDatabase(String databaseName)
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(DeleteDatabaseQuery.FormatX(databaseName));

			try
			{
				_masterSqlConnection.Open();
				return ExecuteMultiLinedSql(_masterSqlConnection, scriptBuilder.ToNewLineDelimitedString());
			}
			catch 
			{
				return false;
			}
			finally
			{
				_masterSqlConnection.Close();
			}

		}
		protected virtual String DeleteDatabaseQuery => String.Empty;  // Drop database.
		//protected virtual String DeleteDatabaseTablesQuery => String.Empty;  // Drop tables.
		//protected virtual String DetachDatabaseTablesQuery => String.Empty;  // Remove constraints, dependencies and indexes.

		private const String DatabaseExistsQuery = "IF DB_ID(@DatabaseName) IS NOT NULL select 1 'Any' else select 0 'Any'";
		private readonly SqlConnection _masterSqlConnection;
	}
}
