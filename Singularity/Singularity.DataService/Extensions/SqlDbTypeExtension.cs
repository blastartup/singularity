using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singularity.DataService.ReferenceType;

namespace Singularity.DataService.Extensions
{
	public static class SqlDbTypeExtension
	{
		public static Type ToNetType(this SqlDbType sqlDbType)
		{
			return TypeConverter.ToNetType(sqlDbType);
		}

		public static DbType ToDbType(this SqlDbType sqlDbType)
		{
			return TypeConverter.ToDbType(sqlDbType);
		}
	}
}
