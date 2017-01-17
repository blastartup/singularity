using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework
{
	public class SqlEntityContext : IDisposable
	{
		public SqlEntityContext(SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			_sqlConnectionStringBuilder = sqlConnectionStringBuilder;
			_sqlConnection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
			_sqlConnection.Open();
		}

		public SqlDataReader ExecDataReader(String query, SqlParameter[] filterParameters)
		{
			using (SqlCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return cmd.ExecuteReader();
			}
		}

		public Object ExecScalar(String query, SqlParameter[] filterParameters)
		{
			using (SqlCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return cmd.ExecuteScalar();
			}
		}

		public Int32 ExecuteNonQuery(String query, SqlParameter[] filterParameters)
		{
			using (SqlCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public SqlTransaction BeginTransaction()
		{
			return _sqlTransaction ?? (_sqlTransaction = _sqlConnection.BeginTransaction());
		}

		public Boolean Commit()
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
				finally
				{
					_sqlTransaction.Dispose();
					_sqlTransaction = null;
				}
			}
			return result;
		}

		public void Rollback()
		{
			if (_sqlTransaction != null)
			{
				_sqlTransaction.Rollback();
				_sqlTransaction.Dispose();
				_sqlTransaction = null;
			}
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
					_sqlTransaction?.Dispose();
					if (_sqlConnection != null)
					{
						_sqlConnection.Close();
						_sqlConnection.Dispose();
					}

					_disposed = true;
				}
			}
		}

		private SqlCommand CreateCommand(String query, CommandType commandType, SqlParameter[] filterParameters)
		{
			var sqlCommand = new SqlCommand(query, _sqlConnection)
			{
				CommandType = commandType,
				Transaction = _sqlTransaction
			};
			sqlCommand.Parameters.AddRange(filterParameters);
			return sqlCommand;
		}

		public Boolean AutomaticTransactions { get; set; }
		public String Name => _sqlConnectionStringBuilder.InitialCatalog;

		public SqlConnection SqlConnection => _sqlConnection;
		private readonly SqlConnection _sqlConnection;

		private SqlTransaction _sqlTransaction;
		private Boolean _disposed;
		private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;
	}
}
