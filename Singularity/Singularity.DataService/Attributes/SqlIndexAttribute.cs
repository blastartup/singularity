using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class SqlIndexAttribute : Attribute
	{
		public SqlIndexAttribute(String name, Boolean isUnique = false, Byte order = 0)
		{
			Name = name;
			IsUnique = isUnique;
			Order = order;
		}

		public String Name { get; set; }
		public Boolean IsUnique { get; set; }
		public Byte Order { get; set; }
	}
}
