using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Singularity.DataService.Interfaces
{
	public interface IValidatable
	{
		IEnumerable<ValidationResult> Validate();
	}
}
