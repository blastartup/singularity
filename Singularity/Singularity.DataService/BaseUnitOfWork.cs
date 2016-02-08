using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace Singularity.DataService
{
	public abstract class BaseUnitOfWork<TDbContext> : IDisposable
		where TDbContext : DbContext
	{
		public Lazy<TDbContext> Context = new Lazy<TDbContext>(false);

		public bool Save()
		{
			return Context.Value.SaveChanges() > 0;
		}

		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Context.Value.Dispose();
				}
			}
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		public bool LazyLoadingEnabled
		{
			get { return Context.Value.Configuration.LazyLoadingEnabled; }
			set { Context.Value.Configuration.LazyLoadingEnabled = value; }
		}

	}
}
