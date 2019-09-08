using System;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlObject
	{
		protected SqlObject()
		{
			DefaultSchema = "dbo";
			//RuleSchema = "dbo";
		}

		protected SqlObject(String name) : this()
		{
			Name = name;
		}

		public String Name { get; set; }
		public Int32 Length { get; set; }
		public Boolean Nullable { get; set; }
		public Int32 Precision { get; set; }
		public Int32 Scale { get; set; }
		public Boolean IsNumeric => Scale > 0;
		public Boolean VariableLength { get; set; }
		public String UserDefinedDataTypeName { get; set; }

		public String DefaultSchema { get; set; }
	}
}