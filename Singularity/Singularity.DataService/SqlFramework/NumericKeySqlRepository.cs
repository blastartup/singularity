using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework
{
	public abstract class NumericKeySqlRepository<TSqlEntity> : SqlRepository<TSqlEntity, Int64>
		where TSqlEntity : class
	{
		protected NumericKeySqlRepository(SqlEntityContext context) : base(context)
		{ }

		public void Reseed(Int32 newPrimaryKey = 0)
		{
			newPrimaryKey--;
			SqlCommand cmd = new SqlCommand($"DBCC CheckIdent ({TableName}, reseed, {newPrimaryKey})", Context.SqlConnection);
			cmd.ExecuteNonQuery();
		}

		protected override Boolean InsertCore(TSqlEntity sqlEntity)
		{
			var insertStatement = InsertColumnsPattern.FormatX(TableName, InsertColumns(), GetInsertValues(sqlEntity));
			SetPrimaryKey(sqlEntity, Context.ExecuteScalar(insertStatement/*, new SqlParameter[] { }*/).ToInt64());
			return false;
		}

		private const String InsertColumnsPattern = "Insert [{0}] ({1}) Values({2}) SELECT @@IDENTITY";
	}
}
