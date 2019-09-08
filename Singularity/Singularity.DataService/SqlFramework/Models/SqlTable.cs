using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	public class SqlTable
	{
		public SqlTable()
		{
			Columns = new HashSet<SqlColumn>();
			ForeignKeyColumns = new HashSet<SqlColumn>();
		}

		public SqlTable(String name) : this()
		{
			Name = name;
		}

		public SqlTable(SqlDatabase database, String name) : this(name)
		{
			Database = database;
		}

		public String Name { get; set; }
		public SqlDatabase Database { get; set; }
		public ICollection<SqlColumn> Columns { get; set; }
		public ICollection<SqlColumn> ForeignKeyColumns { get; set; }
		public Int64 RowCount { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		public Double DataSpaceUsed { get; set; }
		public String DefaultSchema { get; set; }

		public IEnumerable<SqlColumn> PrimaryKeys
		{
			get => Columns.Where(f => f.InPrimaryKey);
		}


		//public bool AnsiNullsStatus { get; set; }
		//public bool ChangeTrackingEnabled { get; set; }
		//public string DataSourceName { get; set; }
		//public DurabilityType Durability { get; set; }
		//public ExternalTableDistributionType ExternalTableDistribution { get; set; }
		//public bool FakeSystemTable { get; set; }
		//public string FileFormatName { get; set; }
		//public string FileGroup { get; set; }
		//public string FileStreamFileGroup { get; set; }
		//public string FileStreamPartitionScheme { get; set; }
		//public string FileTableDirectoryName { get; set; }
		//public string FileTableNameColumnCollation { get; set; }
		//public bool FileTableNamespaceEnabled { get; set; }
		//public bool HasAfterTrigger { get; set; }
		//public bool HasClusteredColumnStoreIndex { get; set; }
		//public bool HasClusteredIndex { get; set; }
		//public bool HasCompressedPartitions { get; set; }
		//public bool HasDeleteTrigger { get; set; }
		//public bool HasHeapIndex { get; set; }
		//public bool HasIndex { get; set; }
		//public bool HasInsertTrigger { get; set; }
		//public bool HasInsteadOfTrigger { get; set; }
		//public bool HasNonClusteredColumnStoreIndex { get; set; }
		//public bool HasNonClusteredIndex { get; set; }
		//public bool HasPrimaryClusteredIndex { get; set; }
		//public bool HasSparseColumn { get; set; }
		//public bool HasSpatialData { get; set; }
		//public bool HasSystemTimePeriod { get; set; }
		//public bool HasUpdateTrigger { get; set; }
		//public int HistoryTableID { get; set; }
		//public string HistoryTableName { get; set; }
		//public string HistoryTableSchema { get; set; }
		//public int ID { get; set; }
		//public double IndexSpaceUsed { get; set; }
		//public bool IsExternal { get; set; }
		//public bool IsFileTable { get; set; }
		//public bool IsIndexable { get; set; }
		public bool IsMemoryOptimized { get; set; }
		//public bool IsPartitioned { get; set; }
		//public bool IsSchemaOwned { get; set; }
		public bool IsSystemObject { get; set; }
		//public bool IsSystemVersioned { get; set; }
		//public string Location { get; set; }
		//public LockEscalationType LockEscalation { get; set; }
		//public string Owner { get; set; }
		//public string PartitionScheme { get; set; }
		//public bool QuotedIdentifierStatus { get; set; }
		//public bool Replicated { get; set; }
		//public long RowCount { get; set; }
		//public string ShardingColumnName { get; set; }
		//public string SystemTimePeriodEndColumn { get; set; }
		//public string SystemTimePeriodStartColumn { get; set; }
		//public TableTemporalType TemporalType { get; set; }
		//public string TextFileGroup { get; set; }
		//public bool TrackColumnsUpdatedEnabled { get; set; }
		//public int HistoryRetentionPeriod { get; set; }
		//public TemporalHistoryRetentionPeriodUnit HistoryRetentionPeriodUnit { get; set; }

		//internal override string[] GetNonAlterableProperties()
		//{
		//	return new string[14]
		//	{
		//		"AnsiNullsStatus",
		//		"Durability",
		//		"FileGroup",
		//		"FileTableNameColumnCollation",
		//		"IsEdge",
		//		"IsFileTable",
		//		"IsMemoryOptimized",
		//		"IsNode",
		//		"PartitionScheme",
		//		"QuotedIdentifierStatus",
		//		"RemoteTableName",
		//		"RemoteTableProvisioned",
		//		"TextFileGroup",
		//		"RejectedRowLocation"
		//	};
		//}

		// /// <summary>
		// /// Tests the integrity of database pages implementing storage for the
		// /// referenced table and indexes defined on it.
		// /// </summary>
		// /// <returns></returns>
		//public StringCollection CheckTable()
		//{
		//	try
		//	{
		//		this.CheckObjectState();
		//		StringCollection queries = new StringCollection();
		//		this.InsertUseDbIfNeeded(queries);
		//		string s = this.FormatFullNameForScripting(new ScriptingPreferences()
		//		{
		//			ScriptForCreateDrop = true
		//		});
		//		queries.Add(string.Format((IFormatProvider)SmoApplication.DefaultCulture, "DBCC CHECKTABLE (N'{0}') WITH NO_INFOMSGS", (object)SqlSmoObject.SqlString(s)));
		//		return this.ExecutionManager.ExecuteNonQueryWithMessage(queries);
		//	}
		//	catch (Exception ex)
		//	{
		//		SqlSmoObject.FilterException(ex);
		//		throw new FailedOperationException(ExceptionTemplatesImpl.CheckTable, (object)this, ex);
		//	}
		//}

		//public StringCollection CheckIdentityValue()
		//{
		//	try
		//	{
		//		this.CheckObjectState();
		//		StringCollection queries = new StringCollection();
		//		this.InsertUseDbIfNeeded(queries);
		//		this.FormatFullNameForScripting(new ScriptingPreferences()
		//		{
		//			ScriptForCreateDrop = true
		//		});
		//		queries.Add(string.Format((IFormatProvider)SmoApplication.DefaultCulture, "DBCC CHECKIDENT(N'{0}')", (object)SqlSmoObject.SqlString(this.FullQualifiedName)));
		//		return this.ExecutionManager.ExecuteNonQueryWithMessage(queries);
		//	}
		//	catch (Exception ex)
		//	{
		//		SqlSmoObject.FilterException(ex);
		//		throw new FailedOperationException(ExceptionTemplatesImpl.CheckIdentityValues, (object)this, ex);
		//	}
		//}

		// /// <summary>
		// /// Tests the integrity of database pages implementing storage for the referenced table
		// /// </summary>
		// /// <returns></returns>
		//public StringCollection CheckTableDataOnly()
		//{
		//	try
		//	{
		//		this.CheckObjectState();
		//		StringCollection queries = new StringCollection();
		//		this.InsertUseDbIfNeeded(queries);
		//		string s = this.FormatFullNameForScripting(new ScriptingPreferences()
		//		{
		//			ScriptForCreateDrop = true
		//		});
		//		queries.Add(string.Format((IFormatProvider)SmoApplication.DefaultCulture, "DBCC CHECKTABLE (N'{0}', NOINDEX)", (object)SqlSmoObject.SqlString(s)));
		//		return this.ExecutionManager.ExecuteNonQueryWithMessage(queries);
		//	}
		//	catch (Exception ex)
		//	{
		//		SqlSmoObject.FilterException(ex);
		//		throw new FailedOperationException(ExceptionTemplatesImpl.CheckTable, (object)this, ex);
		//	}
		//}
		// /// <summary>
		// /// this function enumerates all the ForeignKeys that reference the primary key
		// /// of this table
		// /// </summary>
		// /// <returns></returns>
		//public DataTable EnumForeignKeys()
		//{
		//	try
		//	{
		//		Request req = new Request((Urn)((string)this.ParentColl.ParentInstance.Urn + string.Format((IFormatProvider)SmoApplication.DefaultCulture, "/Table/ForeignKey[@ReferencedTable='{0}']", (object)Urn.EscapeString(this.Name))), new string[1]
		//		{
		//	 "Name"
		//		});
		//		req.OrderByList = new OrderBy[1]
		//		{
		//	 new OrderBy("Name", OrderBy.Direction.Asc)
		//		};
		//		req.ParentPropertiesRequests = new PropertiesRequest[1];
		//		req.ParentPropertiesRequests[0] = new PropertiesRequest()
		//		{
		//			Fields = new string[2] { "Schema", "Name" },
		//			OrderByList = new OrderBy[2]
		//		  {
		//		new OrderBy("Schema", OrderBy.Direction.Asc),
		//		new OrderBy("Name", OrderBy.Direction.Asc)
		//		  }
		//		};
		//		return this.ExecutionManager.GetEnumeratorData(req);
		//	}
		//	catch (Exception ex)
		//	{
		//		SqlSmoObject.FilterException(ex);
		//		throw new FailedOperationException(ExceptionTemplatesImpl.EnumForeignKeys, (object)this, ex);
		//	}
		//}

		//public void RebuildIndexes(int fillFactor)
		//{
		//	try
		//	{
		//		this.CheckObjectState();
		//		StringCollection queries = new StringCollection();
		//		this.InsertUseDbIfNeeded(queries);
		//		queries.Add(string.Format((IFormatProvider)SmoApplication.DefaultCulture, "DBCC DBREINDEX(N'{0}', N'', {1})", (object)SqlSmoObject.SqlString(this.FullQualifiedName), (object)fillFactor));
		//		this.ExecutionManager.ExecuteNonQuery(queries);
		//	}
		//	catch (Exception ex)
		//	{
		//		SqlSmoObject.FilterException(ex);
		//		throw new FailedOperationException(ExceptionTemplatesImpl.RebuildIndexes, (object)this, ex);
		//	}
		//}

		//public void RecalculateSpaceUsage()
		//{
		//	try
		//	{
		//		this.CheckObjectState();
		//		StringCollection queries = new StringCollection();
		//		this.InsertUseDbIfNeeded(queries);
		//		queries.Add(string.Format((IFormatProvider)SmoApplication.DefaultCulture, "DBCC UPDATEUSAGE(0, N'[{0}].[{1}]') WITH NO_INFOMSGS", (object)SqlSmoObject.SqlString(this.Schema), (object)SqlSmoObject.SqlString(this.Name)));
		//		this.ExecutionManager.ExecuteNonQuery(queries);
		//	}
		//	catch (Exception ex)
		//	{
		//		SqlSmoObject.FilterException(ex);
		//		throw new FailedOperationException(ExceptionTemplatesImpl.RecalculateSpaceUsage, (object)this, ex);
		//	}
		//}


	}
}
