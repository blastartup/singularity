using Singularity.DataService.Extensions;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlEntityContext : IDisposable
	{
		// Made class abstract, to force implementation to automatically cater for multiple contexts out of the box.

		// Parameterless constructor required for Generics even though not ever called.  Do not use.
		protected SqlEntityContext()
		{
		}

		protected SqlEntityContext(SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			_sqlConnectionStringBuilder = sqlConnectionStringBuilder;
			_sqlConnection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
			_sqlConnection.OpenEx(sqlConnectionStringBuilder.ConnectTimeout * 1000);
			_transactionCounter = 0;
		}

		protected SqlEntityContext(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
			_sqlConnectionStringBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
			if (_sqlConnection.State == ConnectionState.Closed)
			{
				_sqlConnection.OpenEx(_sqlConnectionStringBuilder.ConnectTimeout * 1000);
			}
			_transactionCounter = 0;
		}

		public SqlDataReader ExecuteDataReader(String query, SqlParameter[] filterParameters = null)
		{
			using (SqlCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return ExecuteWithRetry(cmd, cmd.ExecuteReader);
			}
		}

		public Object ExecuteScalar(String query, SqlParameter[] filterParameters = null)
		{
			using (SqlCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return ExecuteWithRetry(cmd, cmd.ExecuteScalar);
			}
		}

		public Int32 ExecuteNonQuery(String query, SqlParameter[] filterParameters = null)
		{
			using (SqlCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return ExecuteWithRetry(cmd, cmd.ExecuteNonQuery);
			}
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

		public Boolean TableExists(String table)
		{
			return ExecuteScalar(TableExistsQuery, new SqlParameter[]{ new SqlParameter("@TableName",  table)}).ToInt() == 1;
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

		private T ExecuteWithRetry<T>(SqlCommand cmd, Func<T> executeSql)
		{
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
							_sqlConnection = new SqlConnection(_sqlConnectionStringBuilder.ConnectionString);
							_sqlConnection.Open();
							cmd.Connection = _sqlConnection;
							_transactionCounter = 0;
						}
					}
					else if (_sqlConnection.State == ConnectionState.Closed)
					{
						if (_sqlConnection.ConnectionString.IsEmpty())
						{
							_sqlConnection = new SqlConnection(_sqlConnectionStringBuilder.ConnectionString);
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
		public String Name => _sqlConnectionStringBuilder.InitialCatalog;
		public String ErrorMessage => _errorMessage;

		public SqlConnection SqlConnection => _sqlConnection;
		private SqlConnection _sqlConnection;

		internal SqlTransaction SqlTransaction => _sqlTransaction;
		private SqlTransaction _sqlTransaction;

		protected SqlServerSystem SqlServerSystem => _sqlServerSystem ?? (_sqlServerSystem = new SqlServerSystem(SqlConnection));
		protected internal virtual DateTime NowDateTime => DateTime.Now;

		private SqlServerSystem _sqlServerSystem;

		private const Int32 MaximumRetries = 3;
		private const Int32 DelayOnError = 500;
		private Int32 _transactionCounter;
		private const Int32 MaximumTransactionsBeforeReconnect = 10000;
		private Boolean _disposed;
		private String _errorMessage;
		private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;
		private const String TableExistsQuery = "If Exists (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName) Begin Select 1 'Any' End Else Begin Select 0 'Any' End";

	}
}
