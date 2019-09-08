using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	public class SqlServer
	{
		public SqlServer()
		{
			Name = "Unknown";
			InstanceName = "Unknown";
			IsSqlServerExpress = false;
			ServiceAccount = "Unknown";
			ServiceName = "Unknown";
			DisplayName = "Unknown";
			Description = "Unknown";
		}

		// SELECT @@servername
		public String Name { get; set; }

		// SELECT @@servicename
		public String InstanceName { get; set; }

		// ExecScalar("SELECT serverproperty('Edition')").ToString().StartsWith("Express")
		public Boolean IsSqlServerExpress { get; set; }

		// new Words(ServiceAccount, ValueLib.ForwardSlash.StringValue)[1]
		public String ServiceName { get; set; }

		// select ServiceName from sys.dm_server_services
		public String DisplayName { get; set; }

		public String Description { get; set; }

		// select Service_Account from sys.dm_server_services
		public String ServiceAccount { get; set; }
	}
}
