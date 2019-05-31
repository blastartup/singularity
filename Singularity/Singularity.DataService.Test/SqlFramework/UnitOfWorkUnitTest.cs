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
		public void TestCreateDatabase()
		{
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				//InitialCatalog = "singularity",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};
			var uow = new UnitOfWorkMock(sqlConnectionBuilder.ToSqlConnection());
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 0, "No database yet.");

			uow.CreateDatabase("crap");
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 1, "Database exists.");

			uow.DeleteDatabase();
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 0, "Database deleted.");
		}

		[TestMethod]
		public void TestCreateTable()
		{
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				//InitialCatalog = "singularity",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};
			var uow = new UnitOfWorkMock(sqlConnectionBuilder.ToSqlConnection());
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 0, "No database yet.");

			uow.CreateDatabase("crap");
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 1, "Database exists.");

			Assert.IsFalse(uow.TeeProjectRepository.TableExists());
			uow.CreateTables();
			Assert.IsTrue(uow.TeeProjectRepository.TableExists());

			uow.Context.ExecuteSql(uow.TeeProjectRepository.DeleteTableQuery);
			Assert.IsFalse(uow.TeeProjectRepository.TableExists());

			uow.DeleteDatabase();
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 0, "Database deleted.");
		}

		[TestMethod]
		public void TestSimpleTableUsage()
		{
			var sqlConnectionBuilder = new SqlConnectionStringBuilder()
			{
				//InitialCatalog = "singularity",
				DataSource = "DeanPc2\\FunKey",
				IntegratedSecurity = false,
				Password = "FunK3y20!!",
				UserID = "sa"
			};
			var uow = new UnitOfWorkMock(sqlConnectionBuilder.ToSqlConnection());
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 0, "No database yet.");

			uow.CreateDatabase("crap");
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 1, "Database exists.");

			Assert.IsFalse(uow.TeeProjectRepository.TableExists());
			uow.CreateTables();
			Assert.IsTrue(uow.TeeProjectRepository.TableExists());

			Assert.IsTrue(uow.TeeProjectRepository.GetCount() == 0);
			var teeProject = new TeeProjectDto() {Name = "TestProject", EDomainType = 2};
			Assert.IsTrue(teeProject.TeeProjectId != Guid.Empty);
			Assert.IsTrue(uow.TeeProjectRepository.Insert(teeProject));
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

			uow.DeleteDatabase();
			Assert.IsTrue(uow.Context.ExecuteScalar("IF DB_ID('Singularity') IS NOT NULL select 1 else select 0").ToInt() == 0, "Database deleted.");
		}
	}
}
