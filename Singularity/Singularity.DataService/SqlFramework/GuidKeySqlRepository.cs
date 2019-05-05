using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework
{
	public abstract class GuidKeySqlRepository<TSqlEntity> : SqlRepository<TSqlEntity, Guid>
		where TSqlEntity : class
	{
		protected GuidKeySqlRepository(SqlEntityContext context) : base(context)
		{ }

		protected override Boolean InsertCore(TSqlEntity sqlEntity)
		{
			var insertColumns = $"{GetIdentityInsertColumns()},{InsertColumns()}";
			var insertValues = $"{ObtainValue(GetPrimaryKey(sqlEntity))},{GetInsertValues(sqlEntity)}";
			var insertStatement = QueueIdentityInsertColumnsPattern.FormatX(TableName, insertColumns, insertValues);
			Context.ExecuteScalar(insertStatement/*, new SqlParameter[] { }*/);
			return false;
		}

		private const String QueueIdentityInsertColumnsPattern = "Insert [{0}] ({1}) Values({2}); ";
	}
}
