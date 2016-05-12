using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singularity.DataService.ReferenceType;

namespace Singularity.DataService.Extensions
{
	public static class TypeExtension
	{
		public static DbType ToNetType(this Type type)
		{
			return TypeConverter.ToDbType(type);
		}

		public static SqlDbType ToSqlDbType(Type type)
		{
			return TypeConverter.ToSqlDbType(type);
		}
	}
}
