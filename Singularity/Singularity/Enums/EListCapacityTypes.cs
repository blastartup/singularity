
namespace Singularity
{
	/// <summary>
	/// Capacity Types for the FList generic object.  These types set the default capacity for the generic list to minimise memory swapping as the list grows incrementally.
	/// </summary>
	public enum EListCapacityTypes
	{
		[EnumAdditional("MinimumCapacity")]
		Minimum = 10,
		[EnumAdditional("MedimumCapacity")]
		Medium = 100,
		[EnumAdditional("MaximumCapacity")]
		Maximum = 1000
	}
}
