using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.Models
{
	public class SqlIndex
	{
		public SqlIndex()
		{
		}

		public SqlIndex(String name)
		{
			Name = name;
		}

		public String Name { get; set; }

		public Boolean IsPrimaryKey { get; set; }
		public Byte Ordinal { get; set; }
		public Boolean IsDescending { get; set; }
	}
}
