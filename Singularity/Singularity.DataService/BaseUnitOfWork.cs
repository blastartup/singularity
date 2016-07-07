using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace Singularity.DataService
{
	public abstract class BaseUnitOfWork<TDbContext> : IDisposable
		where TDbContext : DbContext, new()
	{
		public Boolean Save(Boolean clearContext = false)
		{
			var result = Context.SaveChanges() > 0;
			if (clearContext)
			{
				Context.Dispose();
				_context = ResetDbContext();
				ResetRepositories();
			}
			return result;
		}

		private Boolean _disposed = false;

		protected virtual void Dispose(Boolean disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Context.Dispose();
				}
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		public Boolean LazyLoadingEnabled
		{
			get { return Context.Configuration.LazyLoadingEnabled; }
			set { Context.Configuration.LazyLoadingEnabled = value; }
		}

		public TDbContext Context
		{
			get { return _context ?? (_context = NewDbContext()); }
		}
		private TDbContext _context;

		protected virtual TDbContext NewDbContext()
		{
			return new TDbContext();	
		}

		protected virtual TDbContext ResetDbContext()
		{
			return NewDbContext();
		}

		protected abstract void ResetRepositories();
	}
}
