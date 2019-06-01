using System;
using System.Data;

namespace Singularity.DataService
{
	public class ReplyDataTable : ReplyErrorMessage
	{
		public ReplyDataTable() : this(false)
		{
		}

		public ReplyDataTable(Boolean condition) : base(condition)
		{
		}

		public DataTable Value { get; set; }

	}
}
