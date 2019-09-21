using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singularity.DataService.SqlFramework.Models;

namespace Singularity.DataService.SqlFramework
{
	public class SqlJobService
	{
		internal SqlJobService(SqlServerService sqlServerService)
			=> _sqlServerService = sqlServerService;

		public Boolean Drop(SqlJob job)
		{
			return Drop(job.Id);
		}

		public Boolean Drop(Guid jobId)
		{
			var sqlQuery = $"EXEC msdb.dbo.sp_delete_job @JOB_ID = '{jobId}'";
			var sqlCommand = new SqlCommand(sqlQuery, _sqlServerService.SqlConnection);
			return sqlCommand.ExecuteNonQuery() > 0;
		}

		public SqlJobServer SqlJobServer()
		{
			return _sqlServer ?? (_sqlServer = NewSqlJobServer());
		}
		private SqlJobServer _sqlServer;

		private SqlJobServer NewSqlJobServer()
		{
			SqlJobServer result = new SqlJobServer();

			var sqlQuery = "select job_id, [name], [enabled], date_created, date_modified from msdb.dbo.sysjobs";
			var sqlCommand = new SqlCommand(sqlQuery, _sqlServerService.SqlConnection);
			SqlDataReader serviceDataReader = null;
			try
			{
				serviceDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				while (serviceDataReader.Read())
				{
					var sqlJob = new SqlJob()
					{
						Id = serviceDataReader["job_id"].ToGuid(),
						Name = serviceDataReader["name"].ToString(),
						Enabled = serviceDataReader["enabled"].ToBool(),
						DateCreated = serviceDataReader["date_created"].ToDateTime(),
						DateModified = serviceDataReader["date_modified"].ToDateTime(),
					};
					result.Jobs.Add(sqlJob);
				}

			}
			catch (SqlException e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				serviceDataReader?.Dispose();
			}


			return result;
		}

		private readonly SqlServerService _sqlServerService;
	}
}
