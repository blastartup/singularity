// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Singularity
{
	public interface IReadDataRowFromShared
	{
		DataRow SharedRead();
	}
}
