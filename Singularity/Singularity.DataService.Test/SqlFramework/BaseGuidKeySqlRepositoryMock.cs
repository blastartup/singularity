using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singularity.DataService.SqlFramework;

namespace Singularity.DataService.Test.SqlFramework
{
	public abstract class BaseGuidKeySqlRepositoryMock<TSqlEntity> : GuidKeySqlRepository<TSqlEntity>
    where TSqlEntity : class
	{
		protected BaseGuidKeySqlRepositoryMock(SqlEntityContext context)
			: base(context)
		{
		}

	}
}
