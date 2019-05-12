// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using System.Data.SqlClient;

namespace Singularity
{
	public interface IReadSqlCommandFromShared
	{
		SqlCommand SharedRead();
	}
}
