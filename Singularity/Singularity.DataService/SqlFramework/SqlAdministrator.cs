using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class SqlAdministrator
	{
		public SqlAdministrator(SqlConnection masterSqlConnection)
		{
			_masterSqlConnection = masterSqlConnection;
		}

		public virtual IReply<Boolean> CreateDatabase(String databaseName)
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(CreateDatabaseQuery.FormatX(databaseName));

			try
			{
				_masterSqlConnection.Open();
				return ExecuteMultiLinedSql(_masterSqlConnection, scriptBuilder.ToNewLineDelimitedString());
			}
			catch (SystemException ex)
			{
				return new ReplyMessage(ex.Message);
			}
			finally
			{
				_masterSqlConnection.Close();
			}

		}

		public virtual IReply<Boolean> DeleteDatabase(String databaseName)
		{
			var scriptBuilder = new DelimitedStringBuilder();
			scriptBuilder.AddIfNotEmpty(DeleteDatabaseQuery.FormatX(databaseName));

			try
			{
				_masterSqlConnection.Open();
				return ExecuteMultiLinedSql(_masterSqlConnection, scriptBuilder.ToNewLineDelimitedString());
			}
			catch (SystemException ex)
			{
				return new ReplyMessage(ex.Message);
			}
			finally
			{
				_masterSqlConnection.Close();
			}

		}

		public IReply<Boolean> DatabaseExists(SqlConnectionStringBuilder databaseConnectionString)
		{
			return DatabaseExists(databaseConnectionString.InitialCatalog);
		}

		public IReply<Boolean> DatabaseExists(String databaseName)
		{
			try
			{
				_masterSqlConnection.Open();
				var sqlCommand = new SqlCommand(DatabaseExistsQuery, _masterSqlConnection)
				{
					CommandType = CommandType.Text,
					Parameters = { new SqlParameter("@DatabaseName", databaseName) }
				};
				if (sqlCommand.ExecuteScalar().ToInt() == 1)
				{
					return new ReplySimple(true);
				}
				else
				{
					return new ReplyMessage($"{databaseName} doesn't exist.");
				}
			}
			catch (SystemException ex)
			{
				return new ReplyMessage(ex.Message);
			}
			finally
			{
				_masterSqlConnection.Close();
			}
		}

		/// <remarks> Backup fromDatabaseName (temporarily) and restore in the same folder but name it as cloneDatabaseName. </remarks>
		public IReply<Boolean> CloneDatabase(String databaseName, String cloneDatabaseName)
		{
			// eg: Clone Shexie to Shexie_Ab, using the same default database location.
			var temporaryDatabaseBackupName = BackupDatabase(databaseName);
			if (temporaryDatabaseBackupName.IsEmpty())
			{
				return new ReplyMessage($"Temporary database backup name ({temporaryDatabaseBackupName})");
			}

			return RestoreDatabase(temporaryDatabaseBackupName, cloneDatabaseName, DefaultBackupFolder());
		}

		public FileInfo BackupDatabase(String databaseName, FileInfo databaseFileInfo = null, DirectoryInfo backupFolder = null)
		{
			if (databaseName.IsEmpty())
			{
				throw new ArgumentException($"Argument databaseName cannot be empty.", "databaseName");
			}

			if (databaseFileInfo.IsEmpty())
			{
				databaseFileInfo = new FileInfo($"{databaseName}_{DateTime.Now:yyyyMMddHHmmss}.bak");
			}

			var scriptBuilder = new DelimitedStringBuilder();
			// ReSharper disable once AssignNullToNotNullAttribute
			scriptBuilder.AddIfNotEmpty(BackupDatabaseQuery.FormatX(databaseName, Path.Combine(backupFolder?.FullName ?? String.Empty, databaseFileInfo.FullName)));

			try
			{
				_masterSqlConnection.Open();
				if (ExecuteMultiLinedSql(_masterSqlConnection, scriptBuilder.ToNewLineDelimitedString()).Condition)
				{
					return databaseFileInfo;
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				_masterSqlConnection.Close();
			}
			return null;
		}

		public IReply<Boolean> RestoreDatabase(FileInfo backupFileInfo, String restoreAsDatabaseName, DirectoryInfo destinationFolder = null)
		{
			if (backupFileInfo == null)
			{
				throw new ArgumentNullException("backupFileName", "Argument backupFileName cannot be null");
			}

			if (restoreAsDatabaseName.IsEmpty())
			{
				throw new ArgumentException($"Argument databaseName cannot be empty.", "restoreAsDatabaseName");
			}

			var scriptBuilder = new DelimitedStringBuilder();
			// ReSharper disable once AssignNullToNotNullAttribute
			if (destinationFolder == null)
			{
				scriptBuilder.AddIfNotEmpty(RestoreDatabaseQuery.FormatX(backupFileInfo.FullName, restoreAsDatabaseName));
			}
			else
			{
				// For now we'll only support 1 MDF file and 1 LOG file.
				var logicalNames = new List<String>(2);
				SqlCommand sqlCommand = null;
				try
				{
					_masterSqlConnection.Open();
					sqlCommand = _masterSqlConnection.CreateCommand();
					sqlCommand.CommandType = CommandType.Text;
					sqlCommand.CommandText = RestoreFileListOnlyQuery.FormatX(backupFileInfo.FullName);
					var dataReader = sqlCommand.ExecuteReader();
					while (dataReader.Read())
					{
						logicalNames.Add(dataReader["LogicalName"].ToString());
					}
				}
				catch (SystemException ex)
				{
					return new ReplyMessage(ex.Message);
				}
				finally
				{
					_masterSqlConnection.Close();
					sqlCommand?.Dispose();
				}

				var mdfFileName = Path.Combine(destinationFolder.FullName, $"{restoreAsDatabaseName}.mdf");
				var ldfFileName = Path.Combine(destinationFolder.FullName, $"{restoreAsDatabaseName}_log.ldf");
				scriptBuilder.AddIfNotEmpty(RestoreToDatabaseQuery.FormatX(backupFileInfo.FullName, restoreAsDatabaseName, logicalNames[0], mdfFileName, logicalNames[1], ldfFileName));
			}

			try
			{
				_masterSqlConnection.Open();
				return ExecuteMultiLinedSql(_masterSqlConnection, scriptBuilder.ToNewLineDelimitedString());
			}
			catch (SystemException ex)
			{
				return new ReplyMessage(ex.Message);
			}
			finally
			{
				_masterSqlConnection.Close();
			}
		}

		public DirectoryInfo DefaultBackupFolder()
		{
			try
			{
				_masterSqlConnection.Open();
				var sqlCommand = new SqlCommand(DefaultBackupFolderQuery, _masterSqlConnection)
				{
					CommandType = CommandType.Text,
				};
				return new DirectoryInfo(sqlCommand.ExecuteScalar().ToString());
			}
			finally
			{
				_masterSqlConnection.Close();
			}
		}

		internal static IReply<Boolean> ExecuteMultiLinedSql(SqlConnection sqlConnection, String mulitLinedSqlScript, SqlTransaction sqlTransaction = null)
		{
			if (mulitLinedSqlScript.IsEmpty())
			{
				return new ReplyMessage("Given argument mulitLinedSqlScript cannot be empty.");
			}

			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = sqlConnection.CreateCommand();
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.Transaction = sqlTransaction;
				var commands = new Words(mulitLinedSqlScript.ReplaceCaseInsenstive(ValueLib.CrLf.StringValue + "GO", "|").Replace(ValueLib.CrLf.StringValue, ValueLib.Space.StringValue), "|");
				foreach (var command in commands)
				{
					sqlCommand.CommandText = command;
					sqlCommand.ExecuteNonQuery();
				}
				return new ReplySimple(true);
			}
			catch (SystemException ex)
			{
				return new ReplyMessage(ex.Message);
			}
			finally
			{
				sqlCommand?.Dispose();
			}
		}

		protected virtual String CreateDatabaseQuery => String.Empty;

		protected virtual String DeleteDatabaseQuery => String.Empty;  // Drop database.
																							//protected virtual String DeleteDatabaseTablesQuery => String.Empty;  // Drop tables.
																							//protected virtual String DetachDatabaseTablesQuery => String.Empty;  // Remove constraints, dependencies and indexes.
		protected virtual String BackupDatabaseQuery => @"USE master
go

BACKUP DATABASE {0} TO  DISK = '{1}' WITH NOFORMAT, INIT,  NAME = N'{0} Database Backup', SKIP, REWIND, NOUNLOAD, STATS = 10
go
";

		protected virtual String RestoreDatabaseQuery => @"USE master
go

IF DB_ID('{1}') IS NOT NULL 
 ALTER DATABASE {1} SET SINGLE_USER WITH ROLLBACK IMMEDIATE
go

restore database {1} From Disk = '{0}' 
go

ALTER DATABASE {1} SET MULTI_USER
go
";

		protected virtual String RestoreToDatabaseQuery => @"USE master
go

IF DB_ID('{1}') IS NOT NULL 
 ALTER DATABASE {1} SET SINGLE_USER WITH ROLLBACK IMMEDIATE
go

restore database {1} From Disk = '{0}' with move '{2}' to '{3}', move '{4}' to '{5}';
go

ALTER DATABASE {1} SET MULTI_USER
go
";

		protected virtual String DefaultBackupFolderQuery => @"declare @DefaultBackup nvarchar(512)
exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'BackupDirectory', @DefaultBackup output

declare @MasterLog nvarchar(512)
exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer\Parameters', N'SqlArg2', @MasterLog output
select @MasterLog=substring(@MasterLog, 3, 255)
select @MasterLog=substring(@MasterLog, 1, len(@MasterLog) - charindex('\', reverse(@MasterLog)))

select isnull(@DefaultBackup, @MasterLog) DefaultBackup";

		protected virtual String RestoreFileListOnlyQuery => @"RESTORE FILELISTONLY FROM DISK='{0}'";

		private const String DatabaseExistsQuery = "IF DB_ID(@DatabaseName) IS NOT NULL select 1 'Any' else select 0 'Any'";
		private readonly SqlConnection _masterSqlConnection;
	}
}
