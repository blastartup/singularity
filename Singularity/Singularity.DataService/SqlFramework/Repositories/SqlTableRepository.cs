using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework.Repositories
{
	public class SqlTableRepository<TSqlEntityContext> : SqlRepositoryMaster<SqlTable, Int64, TSqlEntityContext>
		where TSqlEntityContext : SqlEntityContext, new()
	{
		public SqlTableRepository(TSqlEntityContext context) : base(context)
		{
		}

		protected override String GetEntityQuery { get; }
		protected override String GetPartialEntityQuery { get; }
		protected override String FindEntityQuery { get; }
		protected override String FindPartialEntityQuery { get; }
		protected override String GetAllEntitiesQuery { get; }
		protected override String GetAllPartialEntitiesQuery { get; }
		protected override String GetEntitiesQuery { get; }
		protected override String GetPartialEntitiesQuery { get; }
		protected override String FindEntitiesQuery { get; }
		protected override String FindPartialEntitiesQuery { get; }
		protected override IEnumerable<SqlTable> AssembleClasses(SqlDataReader dataReader)
		{
			throw new NotImplementedException();
		}

		protected override SqlTable ReadAndAssembleClass(SqlDataReader dataReader)
		{
			throw new NotImplementedException();
		}

		protected override String TableName { get; }
		protected override String GetCountQuery { get; }
	}
}
