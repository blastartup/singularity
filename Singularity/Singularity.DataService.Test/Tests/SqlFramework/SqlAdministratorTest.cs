using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.Test.SqlFramework
{
	[TestClass]
	public class SqlAdministratorTest
	{
		[TestMethod]
		public void TestBackupDatabase()
		{
			var database = "TurbineEncore_Tee";
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());
			if (!sa.DatabaseExists(database))
			{
				sa.CreateDatabase(database);
			}
			Assert.IsTrue(sa.DatabaseExists(database));

			Directory.EnumerateFiles(@"F:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\Backup", "TurbineEncore_Tee*.bak").ForEach(File.Delete);

			Assert.IsTrue(!sa.BackupDatabase("TurbineEncore_Tee").IsEmpty());
			Assert.IsTrue(Directory.EnumerateFiles(@"F:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\Backup", "TurbineEncore_Tee*.bak").Count() == 1);
		}

		[TestMethod]
		public void TestRestoreDatabase()
		{
			var database = "TurbineEncore_Tee";
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());
			if (sa.DatabaseExists(database))
			{
				sa.DeleteDatabase(database);
			}
			Assert.IsTrue(!sa.DatabaseExists(database));

			var backupFileName = Directory.EnumerateFiles(@"F:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\Backup", "TurbineEncore_Tee*.bak").FirstOrDefault();

			Assert.IsTrue(!backupFileName.IsEmpty());

			Assert.IsTrue(sa.RestoreDatabase(new FileInfo(backupFileName), "TurbineEncore_Tee"));
			Assert.IsTrue(sa.DatabaseExists(database));
		}

		[TestMethod]
		public void TestRestoreToDatabase()
		{
			var database = "TurbineEncore_Tee";
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());
			if (sa.DatabaseExists(database))
			{
				sa.DeleteDatabase(database);
			}
			Assert.IsTrue(!sa.DatabaseExists(database));

			var backupFileName = Directory.EnumerateFiles(@"F:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\Backup", "TurbineEncore_Tee*.bak").FirstOrDefault();

			Assert.IsTrue(!backupFileName.IsEmpty());

			Assert.IsTrue(sa.RestoreDatabase(new FileInfo(backupFileName), "TurbineEncore_Tee", new DirectoryInfo(@"F:\Temp")));
			Assert.IsTrue(sa.DatabaseExists(database));
		}

		[TestMethod]
		public void TestDefaultDatabaseBackupFolder()
		{
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());
			Assert.AreEqual(sa.DefaultBackupFolder().FullName, @"F:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\Backup");
		}

		[TestMethod]
		public void TestCloneDatabase()
		{
			var database = "TurbineEncore_Tee";
			var database2 = "TurbineEncore_Tee2";
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());
			if (!sa.DatabaseExists(database))
			{
				sa.CreateDatabase(database);
			}
			Assert.IsTrue(sa.DatabaseExists(database));

			Assert.IsTrue(sa.CloneDatabase(database, database2));
			Assert.IsTrue(sa.DatabaseExists(database2));
		}

	}
}
