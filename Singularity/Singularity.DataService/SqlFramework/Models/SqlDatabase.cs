using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	public class SqlDatabase
	{
		public SqlDatabase()
		{
		}

		public SqlDatabase(String name) : this()
		{
			Name = name;
		}

		public String Name { get; set;}

		//public Int32 ActiveConnections { get; set; }
		//public Boolean AnsiNullDefault { get; set; }
		//public Boolean AnsiNullsEnabled { get; set; }
		//public Boolean AnsiPaddingEnabled { get; set; }
		//public Boolean AnsiWarningsEnabled { get; set; }
		//public Boolean AutoClose { get; set; }
		//public Boolean AutoCreateIncrementalStatisticsEnabled { get; set; }
		//public Boolean AutoCreateStatisticsEnabled { get; set; }
		//public Boolean AutoShrink { get; set; }
		//public Boolean AutoUpdateStatisticsAsync { get; set; }
		//public Boolean AutoUpdateStatisticsEnabled { get; set; }
		//public Boolean CaseSensitive { get; set; }
		//public Boolean ChangeTrackingEnabled { get; set; }
		//public Boolean ChangeTrackingAutoCleanUp { get; set; }
		//public Int32 ChangeTrackingRetentionPeriod { get; set; }
		//public String Collation { get; set; }
		//public Boolean CloseCursorsOnCommitEnabled { get; set; }
		//public CompatibilityLevel CompatibilityLevel { get; set; }
		//public DateTime CreateDate { get; set; }
		//public Guid DatabaseGuid { get; set; }
		//public Double DataSpaceUsage { get; set; }
		//public String DefaultSchema { get; set; }
		//public Int32 ID { get; set; }
		//public Double IndexSpaceUsage { get; set; }
		//public DateTime LastBackupDate { get; set; }
		//public DateTime LastDifferentialBackupDate { get; set; }
		//public Boolean ReadOnly { get; set; }
		//public Double Size { get; set; }
		//public DatabaseStatus Status { get; set; }

	}
}
