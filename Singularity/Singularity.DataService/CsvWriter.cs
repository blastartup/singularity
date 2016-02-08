using System;
using System.Data;
using System.IO;

namespace Singularity.DataService
{
	public class CsvWriter
	{
		readonly bool _isFirstRowHeadings;
		readonly String _seperator;
		
		public CsvWriter(bool isFirstRowHeadings = false, String seperator = ",")
		{
			_isFirstRowHeadings = isFirstRowHeadings;
			_seperator = seperator;
		}

		private void OutputToStreamWriter(DataTable dataSource, StreamWriter sw)
		{
			int columnCount = dataSource.Columns.Count;
			int lastColumn = GetLastColumn(columnCount - 1);

			if (_isFirstRowHeadings)
			{
				for (var idx = 0; idx < columnCount; idx++)
				{
					if (!IsRequiredColumn(idx)) continue;
					sw.Write(dataSource.Columns[idx]);
					if (idx < lastColumn)
					{
						sw.Write(_seperator);
					}
				}
				sw.Write(sw.NewLine);
			}

			foreach (DataRow dataRow in dataSource.Rows)
			{
				for (var idx = 0; idx < columnCount; idx++)
				{
					if (!IsRequiredColumn(idx)) continue;
					if (!Convert.IsDBNull(dataRow[idx]))
					{
						sw.Write(dataRow[idx].ToStringSafe().RemoveNoise().Replace(_seperator, ValueLib.Space.StringValue).
						                      Replace(ValueLib.DoubleQuotes.StringValue, String.Empty));
					}

					if (idx < lastColumn)
					{
						sw.Write(_seperator);
					}
				}
				sw.Write(sw.NewLine);
			}
		}

		public Stream ToStream(DataTable dataSource)
		{
			Stream res = new MemoryStream();
			WriteToStream(dataSource, res);
			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="stream"></param>
		public bool WriteToStream(DataTable dataSource, Stream stream)
		{
			var sw = new StreamWriter(stream);
			OutputToStreamWriter(dataSource, sw);
			sw.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			return true;
		}

		public bool WriteToFile(DataTable dataSource, string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Create))
			{
				return WriteToStream(dataSource, fs);
			}
		}

		protected virtual int GetLastColumn(int defaultValue)
		{
			return defaultValue;
		}

		protected virtual bool IsRequiredColumn(int idx)
		{
			return true;
		}
	}
}
