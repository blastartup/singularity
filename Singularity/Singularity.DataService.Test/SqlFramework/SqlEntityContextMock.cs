using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.Test.SqlFramework
{
	public class SqlEntityContextMock : SqlEntityContext
	{
		// Parameterless constructor required for Generics even though not ever called.  Do not use.
		public SqlEntityContextMock() : base()
		{
		}

		public SqlEntityContextMock(SqlConnectionStringBuilder sqlConnectionStringBuilder) : base(sqlConnectionStringBuilder)
		{
		}

		public SqlEntityContextMock(SqlConnection sqlConnection) : base(sqlConnection)
		{
		}
	}
}
