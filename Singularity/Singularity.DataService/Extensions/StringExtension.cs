using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.Extensions
{
	public static class StringExtension
	{
		public static Boolean IsNumericSqlDataType(this String datatype)
		{
			return datatype.ToLower().In("bigint", "int", "smallint", "tinyint", "bit", "money", "decimal", "float", "real", "smallmoney");
		}

		public static Boolean IsAlphabeticSqlDataType(this String datatype)
		{
			return datatype.ToLower().In("nvarchar", "nchar", "varchar", "char", "text", "ntext");
		}

		public static Boolean IsDateTimeSqlDataType(this String datatype)
		{
			return datatype.ToLower().In("datetime", "datetime2", "date", "time");
		}
	}
}
