using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Spreadsheets;

namespace Singularity.Google
{
	public class SheetReader
	{
		public SheetReader(SpreadsheetsService authorisedSpreadsheetsheetsService)
		{
			_authorisedSpreadsheetsheetsService = authorisedSpreadsheetsheetsService;
		}

		public List<TDataRow> ReadAll<TDataRow>(String documentName, String sheetTitle) where TDataRow : GdataRow, new()
		{
			var sheetQuery = new SpreadsheetQuery();
			var sheetFeed = AuthorisedSpreadsheetsService.Query(sheetQuery);

			var affiliates = (from x in sheetFeed.Entries where x.Title.Text.Contains(documentName) select x).First();

			// Get the first Worksheet...
			var sheetLink = affiliates.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
			var workSheetQuery = new WorksheetQuery(sheetLink.HRef.ToString());
			var workSheetFeed = AuthorisedSpreadsheetsService.Query(workSheetQuery);

			var affiliateSheet = workSheetFeed.Entries.First(s => s.Title.Text.Equals(sheetTitle, StringComparison.InvariantCultureIgnoreCase));

			// Get the cells...
			UInt32 startRow = 2;
			var cellLink = affiliateSheet.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null);
			var cellQuery = new CellQuery(cellLink.HRef.ToString())
			{
				MinimumRow = startRow,
			};
			var cellFeed = AuthorisedSpreadsheetsService.Query(cellQuery);

			var table = new List<TDataRow>();
			TDataRow row = null;
			foreach (CellEntry currentCell in cellFeed.Entries)
			{
				if (currentCell.Column == 1)
				{
					if (currentCell.Value.Equals("-end-", StringComparison.InvariantCultureIgnoreCase))
					{
						break;
					}

					row = new TDataRow {RowNbr = currentCell.Row, Service = AuthorisedSpreadsheetsService, CellLink = cellLink};
					table.Add(row);
				}

				if (currentCell.Column > row.LastColumn)
				{
					continue;
				}

				row.SetValue(currentCell);
			}

			return table;
		}

		SpreadsheetsService AuthorisedSpreadsheetsService
		{
			get { return _authorisedSpreadsheetsheetsService; }
		}
		private SpreadsheetsService _authorisedSpreadsheetsheetsService ;
	}
}
