using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.Models
{
	public class SqlJobServer
	{

		public IList<SqlJob> Jobs => _jobs ?? (_jobs = new List<SqlJob>());
		private IList<SqlJob> _jobs;

	}
}
