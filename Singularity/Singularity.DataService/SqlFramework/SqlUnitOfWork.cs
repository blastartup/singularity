﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting.Contexts;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class SqlUnitOfWork<TSqlEntityContext> : IDisposable
		where TSqlEntityContext : SqlEntityContext, new()
	{
		protected SqlUnitOfWork()
		{
		}

		protected SqlUnitOfWork(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
		}

		public virtual Boolean CreateTables()
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(CreateDatabaseTablesQuery);
			scriptBuilder.AddIfNotEmpty(AttachDatabaseTablesQuery);

			return Context.ExecuteMultiLinedSql(scriptBuilder.ToNewLineDelimitedString());
		}

		public Boolean Refresh(Boolean clearContext = false)
		{
			Context.Rollback();  // If you haven't committed by now...  we need to clear out any crap.
			if (clearContext)
			{
				Context.Dispose();
				_context = ResetDbContext();
				ResetRepositories();
			}
			return true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual TSqlEntityContext ResetDbContext()
		{
			return NewDbContext();
		}

		protected abstract TSqlEntityContext NewDbContext();

		protected virtual void Dispose(Boolean disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Context.Dispose();
					_disposed = true;
				}
			}
		}

		protected virtual String CreateDatabaseTablesQuery => String.Empty;
		protected virtual String AttachDatabaseTablesQuery => String.Empty;
		protected virtual String DeleteDatabaseTablesQuery => String.Empty;  // Drop tables.
		protected virtual String DetachDatabaseTablesQuery => String.Empty;  // Remove constraints, dependencies and indexes.

		public TSqlEntityContext Context => _context ?? (_context = NewDbContext());
		private TSqlEntityContext _context;

		// This is used by concrete classes of this one.
		protected SqlConnection SqlConnection => _sqlConnection;
		private readonly SqlConnection _sqlConnection;
	
		protected abstract void ResetRepositories();
		private Boolean _disposed = false;

	}
}
