using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlUnitOfWork<TSqlEntityContext> : IDisposable
		where TSqlEntityContext : SqlEntityContext, new()
	{
		// A quick and dirty way to create a database.  Normally used for temporary databases that are dropped (deleted) often.
		public virtual Boolean CreateDatabase(String databaseName = null, Boolean includeTables = false)
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(databaseName == null ? CreateDatabaseQuery : CreateDatabaseQuery.FormatX(databaseName));
			if (includeTables)
			{
				scriptBuilder.AddIfNotEmpty(CreateDatabaseTablesQuery);
				scriptBuilder.AddIfNotEmpty(AttachDatabaseTablesQuery);
			}

			return Context.ExecuteMultiLinedSql(scriptBuilder.ToNewLineDelimitedString());
		}

		public virtual Boolean CreateTables()
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(CreateDatabaseTablesQuery);
			scriptBuilder.AddIfNotEmpty(AttachDatabaseTablesQuery);

			return Context.ExecuteMultiLinedSql(scriptBuilder.ToNewLineDelimitedString());
		}

		protected virtual String CreateDatabaseQuery => String.Empty;
		protected virtual String CreateDatabaseTablesQuery => String.Empty;
		protected virtual String AttachDatabaseTablesQuery => String.Empty;

		public virtual Boolean DeleteDatabase(String databaseName = null)
		{
			return Context.ExecuteMultiLinedSql(databaseName == null ? DeleteDatabaseQuery : DeleteDatabaseQuery.FormatX(databaseName));
		}
		protected virtual String DeleteDatabaseQuery => String.Empty;  // Drop database.
		protected virtual String DeleteDatabaseTablesQuery => String.Empty;  // Drop tables.
		protected virtual String DetachDatabaseTablesQuery => String.Empty;  // Remove constraints, dependencies and indexes.


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

		public TSqlEntityContext Context => _context ?? (_context = NewDbContext());
		private TSqlEntityContext _context;

		protected virtual TSqlEntityContext NewDbContext()
		{
			return new TSqlEntityContext();	
		}

		protected virtual TSqlEntityContext ResetDbContext()
		{
			return NewDbContext();
		}

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

		protected abstract void ResetRepositories();
		private Boolean _disposed = false;
	}
}
