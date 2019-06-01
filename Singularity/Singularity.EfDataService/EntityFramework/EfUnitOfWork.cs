using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Singularity.DataService;

// ReSharper disable once CheckNamespace
namespace Singularity.EfDataService
{
	public abstract class EfUnitOfWork<TDbContext> : IDisposable
		where TDbContext : DbContext, new()
	{
		protected EfUnitOfWork()
		{
			_dataValidationResults = new List<DataValidationResult>();
		}

		public Boolean Save(Boolean clearContext = false)
		{
			Boolean result;
			try
			{
				_dataValidationResults.Clear();
				result = Context.SaveChanges() > 0;
			}
			catch (DbUpdateException ex)
			{
				result = false;
				if (!(ex.InnerException is UpdateException) || !(ex.InnerException.InnerException is SqlException))
				{
					_dataValidationResults.Add(new DataValidationResult(ex.Message, null, ex.Entries));
				}
				else
				{
					var sqlException = (SqlException)ex.InnerException.InnerException;
					foreach (SqlError sqlExceptionError in sqlException.Errors)
					{
						var errorNumber = sqlExceptionError.Number;
						String errorText = null;
						if (_sqlErrorTextDict.TryGetValue(errorNumber, out errorText))
						{
							errorText = $"{errorText} (#{errorNumber}).";
						}
						else
						{
							errorText = $"{sqlExceptionError.Message} (~{errorNumber}).";
						}

						_dataValidationResults.Add(new DataValidationResult(errorText, ex.Entries.Select(f => f.Entity.GetType().Name), ex.Entries.Select(f => f.Entity)));
					}
				}
			}
			catch (DbEntityValidationException ex)
			{
				result = false;
				foreach (DbEntityValidationResult validationErrors in ex.EntityValidationErrors)
				{
					foreach (DbValidationError validationError in validationErrors.ValidationErrors)
					{
						_dataValidationResults.Add(new DataValidationResult(validationError.ErrorMessage, validationErrors.Entry.Entity.GetType().Name, validationError.PropertyName));
					}
				}
			}
			catch (EntityCommandCompilationException ex)
			{
				result = false;
				_dataValidationResults.AddRange(AddExceptionMessage(ex));
			}
			catch (Exception ex)
			{
				result = false;
				_dataValidationResults.AddRange(AddExceptionMessage(ex));
			}

			if (clearContext)
			{
				Context.Dispose();
				_context = ResetDbContext();
				ResetRepositories();
			}
			return result;
		}

		private IEnumerable<DataValidationResult> AddExceptionMessage(Exception exception)
		{
			var localResults = new List<DataValidationResult>()
			{
				new DataValidationResult(exception.Message),
			};

			if (exception.InnerException != null)
			{
				localResults.AddRange(AddExceptionMessage(exception.InnerException));
			}

			return localResults;
		}

		public void LoadReferenceIfRequired<TEntity, TEntityReference>(TEntity entity, Expression<Func<TEntity, TEntityReference>> property)
			where TEntity : class
			where TEntityReference : class
		{
			if (!Context.Entry(entity).Reference(property).IsLoaded)
			{
				Context.Entry(entity).Reference(property).Load();
			}
		}

		public void LoadCollectionIfRequired<TEntity, TEntityCollection>(TEntity entity, Expression<Func<TEntity, ICollection<TEntityCollection>>> collection)
			where TEntity : class
			where TEntityCollection : class
		{
			if (!Context.Entry(entity).Collection(collection).IsLoaded)
			{
				Context.Entry(entity).Collection(collection).Load();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public Boolean LazyLoadingEnabled
		{
			get => Context.Configuration.LazyLoadingEnabled;
			set => Context.Configuration.LazyLoadingEnabled = value;
		}

		public TDbContext Context => _context ?? (_context = NewDbContext());
		private TDbContext _context;

		protected virtual TDbContext NewDbContext()
		{
			return new TDbContext();
		}

		protected virtual TDbContext ResetDbContext()
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
				}
			}
			_disposed = true;
		}

		private static readonly Dictionary<Int32, String> _sqlErrorTextDict =
			 new Dictionary<Int32, String>
		{
		 {547, "This operation failed because another data entry uses this entry."},
		 {2601, "One of the properties is marked as Unique index and there is already an entry with that value."}
		};

		public List<DataValidationResult> DataValidationResults => _dataValidationResults;
		private readonly List<DataValidationResult> _dataValidationResults;

		protected abstract void ResetRepositories();
		private Boolean _disposed = false;
	}
}
