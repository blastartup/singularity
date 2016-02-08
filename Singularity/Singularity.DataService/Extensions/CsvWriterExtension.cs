using System.Data;
using System.IO;

// ReSharper disable once CheckNamespace

namespace Singularity.DataService
{
	public static class CsvWriterExtension
	{
		public static MemoryStream ToStream(this DataTable dataTable)
		{
			var res = new MemoryStream();
			var writer = new CsvWriter();
			writer.WriteToStream(dataTable, res);
			res.Seek(0, SeekOrigin.Begin);
			return res;
		}
	}
}