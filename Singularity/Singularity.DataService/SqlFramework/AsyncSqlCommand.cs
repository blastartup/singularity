using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework
{
	public sealed class AsyncSqlCommandAction : IReadSqlCommandFromShared, IWriteSqlParametersToShared
	{
		public AsyncSqlCommandAction(SqlCommand sqlCommand)
		{
			_sqlCommand = sqlCommand;
		}

		public SqlCommand SharedRead() => _sqlCommand;

		public void SharedWrite(IEnumerable<SqlParameter> values) => _sqlCommand.Parameters.AddRange(values.ToArray());

		private readonly SqlCommand _sqlCommand;
	}

	public sealed class AsyncSqlCommand : Synchronizer<AsyncSqlCommandAction, IReadSqlCommandFromShared, IWriteSqlParametersToShared>
	{
		public AsyncSqlCommand(SqlCommand sqlCommand) : base(new AsyncSqlCommandAction(sqlCommand))
		{
		}
	}


}
