using System;
using System.Data.SqlClient;

namespace Singularity.DataService.Test.SqlFramework
{

	/// <summary>
	/// Data Transfer Object (DTO) for TeeProject entity (TeeProject table).
	/// </summary>
	public class TeeProjectAssembler : SqlAssembler<TeeProjectDto>
	{

		public TeeProjectAssembler(SqlDataReader sqlDataReader) : base(sqlDataReader)
		{
		}

		protected override void AssembleClassCore(TeeProjectDto teeProjectDto)
		{
			teeProjectDto.TeeProjectId = ReadValue<Guid>("TeeProjectId");
			teeProjectDto.Name = ReadValue<String>("Name");
			teeProjectDto.EDomainType = ReadValue<Int32>("EDomainType");
			//teeProjectDto.TeeDataDomainId = ReadValue<Guid?>("TeeDomainId");
		}

	}

}