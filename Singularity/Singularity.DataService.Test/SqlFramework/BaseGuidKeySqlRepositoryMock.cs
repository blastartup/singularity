using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singularity.DataService;

namespace Singularity.DataService.Test.SqlFramework
{
	public abstract class BaseGuidKeySqlRepositoryMock<TSqlEntity> : GuidKeySqlRepository<TSqlEntity, SqlEntityContextMock>
    where TSqlEntity : class
	{
		protected BaseGuidKeySqlRepositoryMock(SqlEntityContextMock context)
			: base(context)
		{
		}

	}
}
