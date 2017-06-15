﻿using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace Singularity.DataService.OleDbFramework
{
	public class OleDbEntityContext : IDisposable
	{
		public OleDbEntityContext(OleDbConnectionStringBuilder oleDbConnectionStringBuilder)
		{
			_oleDbConnectionStringBuilder = oleDbConnectionStringBuilder;
			_oleDbConnection = new OleDbConnection(oleDbConnectionStringBuilder.ConnectionString);
			_oleDbConnection.Open();
		}

		public OleDbDataReader ExecDataReader(String query, OleDbParameter[] filterParameters)
		{
			using (OleDbCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return cmd.ExecuteReader();
			}
		}

		public Object ExecScalar(String query, OleDbParameter[] filterParameters)
		{
			using (OleDbCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return cmd.ExecuteScalar();
			}
		}

		public Int32 ExecuteNonQuery(String query, OleDbParameter[] filterParameters)
		{
			using (OleDbCommand cmd = CreateCommand(query, CommandType.Text, filterParameters))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public OleDbTransaction BeginTransaction()
		{
			return _oleDbTransaction ?? (_oleDbTransaction = _oleDbConnection.BeginTransaction());
		}

		public Boolean Commit()
		{
			var result = false;
			if (_oleDbTransaction != null)
			{
				try
				{
					_oleDbTransaction.Commit();
					result = true;
				}
				catch (InvalidOperationException)
				{
					_oleDbTransaction.Rollback();
				}
				finally
				{
					_oleDbTransaction.Dispose();
					_oleDbTransaction = null;
				}
			}
			return result;
		}

		public void Rollback()
		{
			if (_oleDbTransaction != null)
			{
				_oleDbTransaction.Rollback();
				_oleDbTransaction.Dispose();
				_oleDbTransaction = null;
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
					_oleDbTransaction?.Dispose();
					if (_oleDbConnection != null)
					{
						_oleDbConnection.Close();
						_oleDbConnection.Dispose();
					}

					_disposed = true;
				}
			}
		}

		private OleDbCommand CreateCommand(String query, CommandType commandType, OleDbParameter[] filterParameters)
		{
			var oleDbCommand = new OleDbCommand(query, _oleDbConnection)
			{
				CommandType = commandType,
				Transaction = _oleDbTransaction
			};
			oleDbCommand.Parameters.AddRange(filterParameters);
			return oleDbCommand;
		}

		public Boolean AutomaticTransactions { get; set; }
		public String Name => _oleDbConnectionStringBuilder.DataSource;

		public OleDbConnection OleDbConnection => _oleDbConnection;
		private readonly OleDbConnection _oleDbConnection;

		private OleDbTransaction _oleDbTransaction;
		private Boolean _disposed;
		private readonly OleDbConnectionStringBuilder _oleDbConnectionStringBuilder;
	}
}
