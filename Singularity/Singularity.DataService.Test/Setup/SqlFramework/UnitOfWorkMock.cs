using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.Test.SqlFramework
{
	public class UnitOfWorkMock : SqlUnitOfWork<SqlEntityContextMock>
	{
		// Do not use.
		private UnitOfWorkMock() : base()
		{
		}

		public UnitOfWorkMock(SqlConnection sqlConnection) : base(sqlConnection)
		{
		}

		//public RepositoryMock RepositoryMock => _repositoryMock ?? (_repositoryMock = new RepositoryMock(Context));
		//private RepositoryMock _repositoryMock;

		public TeeProjectRepository TeeProjectRepository => _teeProjectRepository ?? (_teeProjectRepository = new TeeProjectRepository(Context));
		private TeeProjectRepository _teeProjectRepository;

		protected override SqlEntityContextMock NewDbContext() => new SqlEntityContextMock(SqlConnection);

		protected override void ResetRepositories()
		{
			_teeProjectRepository = null;
		}

		protected override String CreateDatabaseTablesQuery
		{
			get
			{
				var multiLinedSqlQueryBuilder = new StringBuilder();
				multiLinedSqlQueryBuilder.Append(TeeProjectRepository.CreateTableQuery);
				return multiLinedSqlQueryBuilder.ToString();
			}
		}

		protected override String AttachDatabaseTablesQuery
		{
			get
			{
				var multiLinedSqlQueryBuilder = new StringBuilder();
				multiLinedSqlQueryBuilder.Append(TeeProjectRepository.AttachTableQuery);
				return multiLinedSqlQueryBuilder.ToString();
			}
		}

		protected override String DeleteDatabaseTablesQuery
		{
			get
			{
				var multiLinedSqlQueryBuilder = new StringBuilder();
				multiLinedSqlQueryBuilder.Append(TeeProjectRepository.DeleteTableQuery);
				return multiLinedSqlQueryBuilder.ToString();
			}
		}

		protected override String DetachDatabaseTablesQuery
		{
			get
			{
				var multiLinedSqlQueryBuilder = new StringBuilder();
				multiLinedSqlQueryBuilder.Append(TeeProjectRepository.DetachTableQuery);
				return multiLinedSqlQueryBuilder.ToString();
			}
		}

	}
}
