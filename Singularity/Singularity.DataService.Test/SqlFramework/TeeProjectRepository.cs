using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Singularity.DataService.SqlFramework;

namespace Singularity.DataService.Test.SqlFramework
{

	/// <summary>
	/// Repository for TeeProject entity (TeeProject table).
	/// </summary>
	public class TeeProjectRepository : BaseGuidKeySqlRepositoryMock<TeeProjectDto>
	{

		public TeeProjectRepository(SqlEntityContext sqlEntityContext) : base(sqlEntityContext)
		{
		}

		//public override void Delete(TeeProjectDto teeProjectDto) => Delete(teeProjectDto.TeeProjectId);

		//protected override String SelectAllColumns() => _selectAllColumns;
		//private static String _selectAllColumns = "[TeeProject].[TeeProjectId],[TeeProject].[Name],[TeeProject].[EDomainType],[TeeProject].[TeeDomainId]";

		//protected override String InsertColumns() => _insertColumns;
		//private static String _insertColumns = "[Name],[EDomainType],[TeeDomainId]";

		//protected override List<TeeProjectDto> AssembleClassList(SqlDataReader dataReader)
		//{
		//	var assembler = new TeeProjectAssembler(dataReader);
		//	return assembler.AssembleClassList();
		//}

		//protected override IEnumerable<TeeProjectDto> AssembleClasses(SqlDataReader dataReader)
		//{
		//	throw new NotImplementedException();
		//}

		//protected override TeeProjectDto ReadAndAssembleClass(SqlDataReader dataReader)
		//{
		//	var assembler = new TeeProjectAssembler(dataReader);
		//	return assembler.ReadAndAssembleClass();
		//}

		//protected override Guid GetPrimaryKeyValue(TeeProjectDto entityToDelete)
		//{
		//	throw new NotImplementedException();
		//}

		//protected override String GetInsertValues(TeeProjectDto teeProjectDto)
		//{
		//	var valueList = new List<String>()
		//	{
		//		ObtainValue<String>(teeProjectDto.Name),
		//		ObtainValue<Int32>(teeProjectDto.EDomainType),
		//		ObtainValue<Guid?>(teeProjectDto.TeeDataDomainId),
		//	};

		//	return String.Join(", ", valueList.ToArray());
		//}

		//protected override String GetUpdateColumnValuePairs(TeeProjectDto teeProjectDto)
		//{
		//	var valueList = new List<String>()
		//	{
		//		String.Format(UpdateColumnValuePattern, "[Name]", ObtainValue<String>(teeProjectDto.Name)),
		//		String.Format(UpdateColumnValuePattern, "[EDomainType]", ObtainValue<Int32>(teeProjectDto.EDomainType)),
		//		String.Format(UpdateColumnValuePattern, "[TeeDomainId]", ObtainValue<Guid?>(teeProjectDto.TeeDataDomainId)),
		//	};

		//	return String.Join(", ", valueList.ToArray());
		//}

		//protected override String GetUpdateKeyColumnValuePair(TeeProjectDto teeProjectDto) => String.Format(UpdateColumnValuePattern, "TeeProjectId", ObtainValue(teeProjectDto.TeeProjectId));

		//protected override void SetPrimaryKey(TeeProjectDto teeProjectDto, Guid newPrimaryKey) => teeProjectDto.TeeProjectId = newPrimaryKey;

		//protected override String ColumnsOfUniqueness => "{TeeProject.Acc_type.Name},{TeeProject.Name.Name},{TeeProject.Valid.Name},{TeeProject.Fund.Name}";

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

		protected override IEnumerable<SqlParameter> PopulateInsertParameters(TeeProjectDto sqlEntity)
		{
			return new SqlParameter[]
			{
				new SqlParameter("@pk", SqlDbType.UniqueIdentifier, 100) { Value = sqlEntity.TeeProjectId },
			}.Concat(PopulateUpdateParameters(sqlEntity));
		}

		protected override IEnumerable<SqlParameter> PopulateUpdateParameters(TeeProjectDto sqlEntity)
		{
			return new SqlParameter[] 
			{
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

		protected override String CreateTableQuery => @"SET ANSI_NULLS ON
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
		protected override String DeleteTableQuery => @"DROP TABLE [dbo].[TeeProject]
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
		protected override String UpdateQuery => "update [dbo].[TeeProject] set [Name] = @Name, [EDomainType] = @EDomainType";
		protected override String DeleteEntitiesQuery => "delete from [dbo].[TeeProject] where dbo.[TeeProject].TeeProjectId in (@pk1, @pk2, @pk3)";
		protected override String DeleteEntityQuery => "delete [dbo].[TeeProject] where [TeeProjectId] = @pk";
		protected override String TableName => "TeeProject";
		protected override String PrimaryKeyName => "TeeProjectId";
	}

}