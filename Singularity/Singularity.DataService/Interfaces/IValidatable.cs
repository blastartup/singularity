using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public interface IValidatable
	{
		IEnumerable<ValidationResult> Validate();
	}
}
