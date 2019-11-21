using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.Models
{
	public class SqlServer
	{
		public SqlServer()
		{
			Name = "Unknown";
			IsSqlServerExpress = false;
			ServiceAccount = "Unknown";
			ServiceName = "Unknown";
			Description = "Unknown";
		}

		// SELECT @@servername
		public String Name { get; set; }

		// ExecScalar("SELECT serverproperty('Edition')").ToString().StartsWith("Express")
		public Boolean IsSqlServerExpress { get; internal set; }
		public String Description { get; internal set; }
		public ConnectionState State { get; internal set; }
		public String ServerVersion { get; internal set; }
		public String ServerVersionDescription { get; internal set; }
		public String Edition { get; internal set; }
		public String ProductLevel { get; internal set; }

		#region select Service_Account from sys.dm_server_services
		public String ServiceName { get; internal set; }
		public String ServiceFullName { get; internal set; }
		public String ServiceAccount { get; internal set; }
		public String ServiceStartupTypeDescription { get; internal set; }
		public String ServiceStatusDescription { get; internal set; }
		public String ServiceProcessId { get; internal set; }
		public DateTime ServiceLastStatupTime { get; internal set; }
		#endregion
	}
}
