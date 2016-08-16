using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.Interfaces
{
	public interface ISqlEntity
	{
		String Name { get; set; }
		String PrimaryKeyName { get; set; }
	}
}
