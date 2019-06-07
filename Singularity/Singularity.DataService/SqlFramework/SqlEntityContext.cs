using Singularity.DataService.Extensions;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Singularity.DataService.SqlFramework;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class SqlEntityContext : IDisposable
	{
		// Made class abstract, to force implementation to automatically cater for multiple contexts out of the box.

		// Parameterless constructor required for Generics even though not ever called.  Do not use.
		protected SqlEntityContext()
		{
		}

		protected SqlEntityContext(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
			if (_sqlConnection.State == ConnectionState.Closed)
			{
				_sqlConnection.OpenEx(_sqlConnection.ConnectionTimeout * 1000);
			}
			_transactionCounter = 0;
		}


		public SqlDataReader ExecuteDataReader(String query, SqlParameter[] filterParameters = null)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = CreateCommand(query, CommandType.Text, filterParameters);
				return ExecuteWithRetry(sqlCommand, sqlCommand.ExecuteReader);
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}

		public Object ExecuteScalar(String query, SqlParameter[] filterParameters = null)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = CreateCommand(query, CommandType.Text, filterParameters);
				return ExecuteWithRetry(sqlCommand, sqlCommand.ExecuteScalar);
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}

		public Int32 ExecuteNonQuery(String query, SqlParameter[] filterParameters = null)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = CreateCommand(query, CommandType.Text, filterParameters);
				return ExecuteWithRetry(sqlCommand, sqlCommand.ExecuteNonQuery);
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}

		public Boolean ExecuteSql(String singleLinedSqlStatement)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = CreateCommand(singleLinedSqlStatement, CommandType.Text, null);
				ExecuteWithRetry(sqlCommand, sqlCommand.ExecuteNonQuery);
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

		public Boolean ExecuteMultiLinedSql(String mulitLinedSqlScript)
		{
			return SqlAdministrator.ExecuteMultiLinedSql(SqlConnection, mulitLinedSqlScript, SqlTransaction);
		}

		internal SqlTransaction BeginTransaction()
		{
			return _sqlTransaction ?? (_sqlTransaction = _sqlConnection.BeginTransaction());
		}

		internal Boolean Commit()
		{
			var result = false;
			if (_sqlTransaction != null)
			{
				try
				{
					_sqlTransaction.Commit();
					result = true;
				}
				catch (InvalidOperationException)
				{
					_sqlTransaction.Rollback();
				}
			}
			return result;
		}

		internal void Rollback() => _sqlTransaction?.Rollback();

		public Boolean TableExists(String tableName)
		{
			return ExecuteScalar(TableExistsQuery, new SqlParameter[]{ new SqlParameter("@TableName",  tableName)}).ToInt() == 1;
		}

		public Boolean CreateTables()
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(CreateDatabaseTablesQuery);
			scriptBuilder.AddIfNotEmpty(AttachDatabaseTablesQuery);

			return ExecuteMultiLinedSql(scriptBuilder.ToNewLineDelimitedString());
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(Boolean disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_sqlConnection != null)
					{
						_sqlConnection.Close();
						_sqlConnection.Dispose();
						_sqlConnection = null;
					}

					_disposed = true;
				}
			}
		}

		private SqlCommand CreateCommand(String query, CommandType commandType, SqlParameter[] filterParameters)
		{
			SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection)
			{
				CommandType = commandType,
				Transaction = _sqlTransaction
			};
			if (filterParameters != null)
			{
				sqlCommand.Parameters.AddRange(filterParameters);
			}
			return sqlCommand;
		}

		private T ExecuteWithRetry<T>(NonNullable<SqlCommand> sqlCommand, Func<T> executeSql)
		{
			if (sqlCommand.Value.Connection == null)
			{
				throw new ArgumentException("Connection property of given sqlCommand argument cannot be null.");
			}

			_errorMessage = String.Empty;
			Boolean reconnect = false;
			for (Int32 idx = 0; idx < MaximumRetries; idx++)
			{
				try
				{
					if (reconnect)
					{
						if (_sqlConnection != null)
						{
							_sqlConnection.Close();
							_sqlConnection = new SqlConnection(SqlConnection.ConnectionString);
							_sqlConnection.Open();
							//cmd.Connection = _sqlConnection;  All cmd's passed in are already pointing to _sqlConnection.
							_transactionCounter = 0;
						}
					}
					else if (_sqlConnection.State == ConnectionState.Closed)
					{
						if (_sqlConnection.ConnectionString.IsEmpty())
						{
							_sqlConnection = new SqlConnection(SqlConnection.ConnectionString);
						}
						_sqlConnection.Open();
						_transactionCounter = 0;
					}
					_transactionCounter++;
					return executeSql();
				}
				catch (InvalidOperationException)
				{
					reconnect = true;
				}
				catch (SqlException ex)
				{
					if (ex.Errors.Cast<SqlError>().All(CanRetry))
					{
						Thread.Sleep(DelayOnError);
						reconnect = ex.Errors.Cast<SqlError>().Any(f => f.Class >= 20);
						continue;
					}
					break;
				}
			}

			return default(T);
		}

		private Boolean CanRetry(SqlError error)
		{
			if (error.Class == 16)
			{
				// Invalid object name 'InvalidObjectName'.
				_errorMessage = error.Message;
				return false;
			}
			return true;
		}


		public DateTime? SchemaModifiedDateTime => SqlServerSystem.SchemaModifiedDateTime();
		internal DateTime? DatabaseSchemaModifiedDateTime() => SqlServerSystem.SchemaModifiedDateTime();
		internal DateTime? TableSchemaModifiedDateTime(String tableName) => SqlServerSystem.SchemaModifiedDateTime(tableName);

		public Boolean AutomaticTransactions { get; set; }
		public String ComputerName => SqlServerSystem.DataSource;
		public String DatabaseName => SqlServerSystem.DatabaseName;
		public String ErrorMessage => _errorMessage;

		public SqlConnection SqlConnection => _sqlConnection;
		private SqlConnection _sqlConnection;

		internal SqlTransaction SqlTransaction => _sqlTransaction;
		private SqlTransaction _sqlTransaction;

		protected SqlServerSystem SqlServerSystem => _sqlServerSystem ?? (_sqlServerSystem = new SqlServerSystem(SqlConnection));
		private SqlServerSystem _sqlServerSystem;

		protected virtual String CreateDatabaseTablesQuery => String.Empty;
		protected virtual String AttachDatabaseTablesQuery => String.Empty;

		protected internal virtual DateTime NowDateTime => DateTime.Now;

		private const Int32 MaximumRetries = 3;
		private const Int32 DelayOnError = 500;
		private Int32 _transactionCounter;
		private const Int32 MaximumTransactionsBeforeReconnect = 10000;
		private Boolean _disposed;
		private String _errorMessage;
		private const String TableExistsQuery = "If Exists (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName) Begin Select 1 'Any' End Else Begin Select 0 'Any' End";

	}
}
