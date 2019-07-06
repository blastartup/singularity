using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.Test.SqlFramework
{

	/// <summary>
	/// Data Transfer Object (DTO) for TeeProject entity (TeeProject table).
	/// </summary>
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	//[EntityDefinition(EEntityImportanceTypes.Master, EEntityVolumeTypes.Tiny)]
	[Table("TeeProject")]
	public class TeeProjectDto
	{

		public TeeProjectDto()
		{
			TeeProjectId = Guid.NewGuid();

			//TeeProviders = new HashSet<TeeProviderDto>();
			//TeeBuilders = new HashSet<TeeBuilderDto>();
		}

		/// <remarks>Originally known as TeeProject.TeeProjectId</remarks>
		[Key]
		public Guid TeeProjectId { get; set; }

		/// <remarks>Originally known as TeeProject.Name</remarks>
		[Required]
		[StringLength(100)]
		//[Index(IsUnique = true)]
		public String Name { get; set; }
		
		/// <remarks>Originally known as TeeProject.EDomainType</remarks>
		public Int32 EDomainType { get; set; }
		
		/// <remarks>Originally known as TeeProject.TeeDomainId</remarks>
		//[ForeignKey(nameof(TeeDataDomain))]
		//public Guid? TeeDataDomainId { get; set; }

		//public ICollection<TeeProvider> TeeProviders { get; set; }
		//public ICollection<TeeBuilder> TeeBuilders { get; set; }

		//[NotMapped]
		//[ForeignKey(nameof(TeeDataDomainId))]
		//public TeeDataDomain TeeDataDomain { get; set; }
	}

}