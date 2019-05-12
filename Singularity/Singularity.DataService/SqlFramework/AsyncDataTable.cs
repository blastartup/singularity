using System;
using System.Data;

namespace Singularity.DataService.SqlFramework
{
	public sealed class AsyncDataTableAction : IReadDataRowFromShared, IWriteDataRowToShared
	{
		public AsyncDataTableAction(DataTable dataTable)
		{
			_dataTable = dataTable;
		}

		public DataRow SharedRead() => _dataTable.NewRow();

		public void SharedWrite(DataRow value) => _dataTable.Rows.Add(value);

		private readonly DataTable _dataTable;
	}

	public sealed class AsyncDataTable : Synchronizer<AsyncDataTableAction, IReadDataRowFromShared, IWriteDataRowToShared>
	{
		public AsyncDataTable(DataTable dataTable) : base(new AsyncDataTableAction(dataTable))
		{
		}
	}


}
