using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.Test.SqlFramework
{

	/// <summary>
	/// Repository for TeeProject entity (TeeProject table).
	/// </summary>
	public class TeeProjectRepository : BaseGuidKeySqlRepositoryMock<TeeProjectDto>
	{

		public TeeProjectRepository(SqlEntityContextMock sqlEntityContext) : base(sqlEntityContext)
		{
		}

		protected override IEnumerable<TeeProjectDto> AssembleClasses(SqlDataReader dataReader)
		{
			var assembler = new TeeProjectAssembler(dataReader);
			return assembler.AssembleClassList();
		}

		protected override TeeProjectDto ReadAndAssembleClass(SqlDataReader dataReader)
		{
			var assembler = new TeeProjectAssembler(dataReader);
			return assembler.ReadAndAssembleClass();
		}

		protected override Guid GetPrimaryKeyValue(TeeProjectDto sqlEntity) => sqlEntity.TeeProjectId;

		protected override IEnumerable<SqlParameter> PopulateParameters(TeeProjectDto sqlEntity)
		{
			return new SqlParameter[]
			{
				new SqlParameter("@pk", SqlDbType.UniqueIdentifier, 100) { Value = sqlEntity.TeeProjectId },
				new SqlParameter("@Name", SqlDbType.VarChar, 100) { Value = sqlEntity.Name },
				new SqlParameter("@EDomainType", SqlDbType.TinyInt) { Value = sqlEntity.EDomainType }
			};
		}

		protected override void InsertRowValues(TeeProjectDto sqlEntity, DataRow row)
		{
			row["TeeProjectId"] = sqlEntity.TeeProjectId;
			row["Name"] = sqlEntity.Name;
			row["EDomainType"] = sqlEntity.EDomainType;
		}

		public override String CreateTableQuery => @"SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeeProject](
	[TeeProjectId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[EDomainType] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TeeProject] PRIMARY KEY CLUSTERED 
(
	[TeeProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
";
		public override String AttachTableQuery => @"CREATE UNIQUE NONCLUSTERED INDEX [IX_TeeProject_Name] ON [dbo].[TeeProject]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
";

		protected override String GetCountQuery => "Select Count(*) from [dbo].[TeeProject]";
		protected override String GetEntityQuery => "select [TeeProjectId], [Name], [EDomainType] from dbo.[TeeProject] where [dbo].[TeeProject].[TeeProjectId] = @pk";
		protected override String GetPartialEntityQuery => "select [TeeProjectId], {0} from [dbo].[TeeProject] where [dbo].[TeeProject].[TeeProjectId] = @pk";
		protected override String FindEntityQuery => "select top 1 [TeeProjectId], [Name], [EDomainType] from [dbo].[TeeProject] where {0}{1}";
		protected override String FindPartialEntityQuery => "select top 1 [TeeProjectId], {0} from [dbo].[TeeProject] where {1'}{2}";
		protected override String GetAllEntitiesQuery => "select [TeeProjectId], [Name], [EDomainType] from [dbo].[TeeProject] {0}{1}";
		protected override String GetAllPartialEntitiesQuery => "select [TeeProjectId], {0} from [dbo].[TeeProject] {1'}{2}";
		protected override String GetEntitiesQuery => "select [TeeProjectId], [Name], [EDomainType] from [dbo].[TeeProject] where [dbo].[TeeProject].[TeeProjectId] in (@pk1, @pk2, @pk3){0}{1}";
		protected override String GetPartialEntitiesQuery => "select [TeeProjectId], {0} from [dbo].[TeeProject] where [dbo].[TableName].[TeeProjectId] in (@pk1, @pk2, @pk3){1}{2}";
		protected override String FindEntitiesQuery => "select [TeeProjectId], [Name], [EDomainType] from [dbo].[TeeProject] where {0}{1}{2}";
		protected override String FindPartialEntitiesQuery =>
			"select [TeeProjectId], {0} from dbo.[TeeProject] where {1}{2}{3}";
		protected override String InsertQuery => "insert into [dbo].[TeeProject] ([TeeProjectId], [Name], [EDomainType]) values (@pk, @Name, @EDomainType)";
		protected override String InsertTableValuedQuery =>
			"insert into dbo.[TeeProject] ([TeeProjectId], [Name], [EDomainType]) select tv.[TeeProjectId], tv.[Name], tv.[EDomainType] from @TableNameValues as tv";
		protected override String UpdateQuery => "update [dbo].[TeeProject] set [Name] = @Name, [EDomainType] = @EDomainType where TeeProjectId = @pk";
		protected override String DeleteEntitiesQuery => "delete from [dbo].[TeeProject] where dbo.[TeeProject].TeeProjectId in (@pk1, @pk2, @pk3)";
		protected override String DeleteEntityQuery => "delete [dbo].[TeeProject] where [TeeProjectId] = @pk";
		protected override String TableName => "TeeProject";
		protected override String PrimaryKeyName => "TeeProjectId";

	}

}