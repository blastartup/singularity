using System;
using System.Data;
using Singularity.DataService.ReferenceType;

// ReSharper disable once CheckNamespace

namespace Singularity.DataService
{
	public static class DbTypeExtension
	{
		public static Type ToNetType(this DbType dbType)
		{
			return TypeConverter.ToNetType(dbType);
		}

		public static SqlDbType ToSqlDbType(DbType dbType)
		{
			return TypeConverter.ToSqlDbType(dbType);
		}
	}
}
