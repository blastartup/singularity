using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class EfDbContext : DbContext
	{
		protected EfDbContext() : base() { }

		protected EfDbContext(String nameOrConnectionString) : base(nameOrConnectionString)
		{ }

		protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<Object, Object> items)
		{
			if (ValidateEntryFunc == null)
			{
				return base.ValidateEntity(entityEntry, items);
			}
			return ValidateEntryFunc(entityEntry, items) ?? base.ValidateEntity(entityEntry, items);
		}

		public Func<DbEntityEntry, IDictionary<Object, Object>, DbEntityValidationResult> ValidateEntryFunc { get; set; }
	}
}
