// ReSharper disable once CheckNamespace

using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Singularity
{
	public interface IWriteSqlParametersToShared
	{
		void SharedWrite(IEnumerable<SqlParameter> values);
	}
}
