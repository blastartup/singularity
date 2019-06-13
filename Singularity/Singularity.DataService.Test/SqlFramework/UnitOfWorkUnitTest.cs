using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Singularity.DataService.Extensions;

namespace Singularity.DataService.Test.SqlFramework
{
	[TestClass]
	public class UnitOfWorkUnitTest
	{
		[TestMethod]
		public void TestDatabaseExists()
		{
			var database = "TurbineEncore_Tee";
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = database,
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());

			Assert.IsFalse(sa.DatabaseExists(database));
			sa.CreateDatabase(database);
			Assert.IsTrue(sa.DatabaseExists(sqlConnectionBuilder));
		}

		[TestMethod]
		public void TestCreateDatabase()
		{
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "singularity",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());

			Assert.IsFalse(sa.DatabaseExists(sqlConnectionBuilder), "Database doesn't exist yet.");

			sa.CreateDatabase(sqlConnectionBuilder.InitialCatalog);
			Assert.IsTrue(sa.DatabaseExists(sqlConnectionBuilder), "Database exists.");

			sa.DeleteDatabase("singularity");
			Assert.IsTrue(sa.DatabaseExists(sqlConnectionBuilder), "Database deleted.");
		}

		[TestMethod]
		public void TestCreateTable()
		{
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "singularity",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());
			
			var uow = new UnitOfWorkMock(sqlConnectionBuilder.ToSqlConnection());
			Assert.IsFalse(sa.DatabaseExists(sqlConnectionBuilder), "No database yet.");

			Assert.IsTrue(sa.CreateDatabase("singularity"));
			Assert.IsTrue(sa.DatabaseExists(sqlConnectionBuilder), "Database exists.");

			Assert.IsFalse(uow.TeeProjectRepository.TableExists());
			Assert.IsTrue(uow.CreateTables());
			Assert.IsTrue(uow.TeeProjectRepository.TableExists());

			Assert.IsTrue(uow.Context.ExecuteSql(uow.TeeProjectRepository.DeleteTableQuery));
			Assert.IsFalse(uow.TeeProjectRepository.TableExists());

			uow.Dispose();
			Assert.IsTrue(sa.DeleteDatabase("singularity"));
			Assert.IsFalse(sa.DatabaseExists(sqlConnectionBuilder), "Database now deleted.");
		}

		[TestMethod]
		public void TestSimpleTableUsage()
		{
			var masterSqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "master",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = true,
			};
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				InitialCatalog = "singularity",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};

			var sa = new SqlAdministratorMock(masterSqlConnectionBuilder.ToSqlConnection());

			var uow = new UnitOfWorkMock(sqlConnectionBuilder.ToSqlConnection());
			Assert.IsFalse(sa.DatabaseExists(sqlConnectionBuilder), "No database yet.");

			sa.CreateDatabase("singularity");
			Assert.IsTrue(sa.DatabaseExists(sqlConnectionBuilder), "Database exists.");

			Assert.IsFalse(uow.TeeProjectRepository.TableExists());
			uow.CreateTables();
			Assert.IsTrue(uow.TeeProjectRepository.TableExists());

			Assert.IsTrue(uow.TeeProjectRepository.GetCount() == 0);
			var teeProject = new TeeProjectDto() {Name = "TestProject", EDomainType = 2};
			Assert.IsTrue(teeProject.TeeProjectId != Guid.Empty);
			Assert.IsTrue(uow.TeeProjectRepository.InsertEntity(teeProject));
			Assert.IsFalse(uow.TeeProjectRepository.GetCount() == 0);
			Assert.IsTrue(uow.TeeProjectRepository.GetCount() == 1);

			var teeProject2 = uow.TeeProjectRepository.GetEntity(teeProject.TeeProjectId);
			Assert.IsNotNull(teeProject2);
			Assert.AreEqual("TestProject", teeProject2.Name);
			Assert.AreEqual(2, teeProject2.EDomainType);

			teeProject2.EDomainType = 3;
			teeProject2.Name = "TestProject2";
			Assert.IsTrue(uow.TeeProjectRepository.UpdateEntity(teeProject2));
			Assert.IsTrue(uow.TeeProjectRepository.GetCount() == 1);

			var teeProject3 = uow.TeeProjectRepository.FindEntity("Name = @Name", "@Name", "TestProject2");
			Assert.IsNotNull(teeProject3);
			Assert.AreEqual("TestProject2", teeProject2.Name);
			Assert.AreEqual(3, teeProject2.EDomainType);

			var teeProject4 = uow.TeeProjectRepository.FindEntity("Name = @Name", "@Name", "Project2");
			Assert.IsNull(teeProject4);

			var teeProject5 = uow.TeeProjectRepository.GetEntity(Guid.NewGuid());
			Assert.IsNull(teeProject5);

			Assert.IsTrue(uow.TeeProjectRepository.DeleteEntity(teeProject3));
			Assert.IsTrue(uow.TeeProjectRepository.GetCount() == 0);

			uow.Context.ExecuteSql(uow.TeeProjectRepository.DeleteTableQuery);
			Assert.IsFalse(uow.TeeProjectRepository.TableExists());

			uow.Dispose();
			Assert.IsTrue(sa.DeleteDatabase("singularity"));
			Assert.IsFalse(sa.DatabaseExists(sqlConnectionBuilder), "Database deleted.");
		}
	}
}
