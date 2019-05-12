using System;
using System.Data;
using System.Data.SqlClient;

namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlUnitOfWork<TSqlEntityContext> : IDisposable
		where TSqlEntityContext : SqlEntityContext, new()
	{
		// A quick and dirty way to create a database.  Normally used for temporary databases that are dropped (deleted) often.
		public virtual Boolean CreateDatabase(String projectName)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = Context.SqlConnection.CreateCommand();
				sqlCommand.CommandType = CommandType.Text;
				var commands = new Words(CreateDatabaseQuery.Replace(ValueLib.CrLf.StringValue + "GO", "|").Replace(ValueLib.CrLf.StringValue, ValueLib.Space.StringValue), "|");
				foreach(var command in commands)
				{
					sqlCommand.CommandText = command;
					sqlCommand.ExecuteNonQuery();
				}

				return true;
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}
		protected abstract String CreateDatabaseQuery { get; }

		public virtual Boolean DeleteDatabase(String projectName)
		{
			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = Context.SqlConnection.CreateCommand();
				sqlCommand.CommandType = CommandType.Text;
				var commands = new Words(DeleteDatabaseQuery.Replace(ValueLib.CrLf.StringValue + "GO", "|").Replace(ValueLib.CrLf.StringValue, ValueLib.Space.StringValue), "|");
				foreach (var command in commands)
				{
					sqlCommand.CommandText = command;
					sqlCommand.ExecuteNonQuery();
				}

				return true;
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}
		protected abstract String DeleteDatabaseQuery { get; }

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
