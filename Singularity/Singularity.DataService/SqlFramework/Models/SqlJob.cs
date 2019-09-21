using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework.Models
{
	public class SqlJob : INamable
	{
		public Guid Id { get; set; }
		public String Name { get; set; }
		public Boolean Enabled { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateModified { get; set; }
	}
}
