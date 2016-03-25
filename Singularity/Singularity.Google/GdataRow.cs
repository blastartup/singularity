using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace Singularity.Google
{
	public abstract class GdataRow
	{
		public void SetValue(CellEntry cellEntry)
		{
			Cells.Add(cellEntry.Cell.Column, cellEntry);
		}

		protected String GetValue(uint index)
		{
			CellEntry returnCell;
			if (Cells.TryGetValue(index, out returnCell))
			{
				return returnCell.Cell.Value;
			}
			return String.Empty;
		}

		protected void SetValue(uint index, String value)
		{
			CellEntry cellEntry;
			if (Cells.TryGetValue(index, out cellEntry))
			{
				cellEntry.InputValue = value;
				cellEntry.Update();
			}
			else if (CellLink != null && Service != null)
			{
				var cellQuery = new CellQuery(CellLink.HRef.ToString());
				cellQuery.MinimumRow = cellQuery.MaximumRow = RowNbr;
				cellQuery.MinimumColumn = cellQuery.MaximumColumn = index;
				cellQuery.ReturnEmpty = ReturnEmptyCells.yes;

				var cellFeed = Service.Query(cellQuery);
				cellEntry = cellFeed.Entries[0] as CellEntry;
				if (cellEntry != null)
				{
					cellEntry.InputValue = value;
					cellEntry.Update();
					Cells.Add(cellEntry.Column, cellEntry);
				}
			}
		}

		public AtomLink CellLink { get; set; }

		public SpreadsheetsService Service { get; set; }

		public uint RowNbr { get; set; }

		public abstract int LastColumn { get; }

		private IDictionary<uint, CellEntry> Cells
		{
			get { return _cells ?? (_cells = new Dictionary<uint, CellEntry>()); }
		}
		private Dictionary<uint, CellEntry> _cells;
	}
}
