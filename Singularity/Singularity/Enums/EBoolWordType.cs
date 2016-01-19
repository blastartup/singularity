using System;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public enum EBoolWordType
	{
		[EnumAdditional("TrueFalse")]
		TrueFalse,
		[EnumAdditional("TF")]
		Tf,
		[EnumAdditional("YesNo")]
		YesNo,
		[EnumAdditional("YN")]
		Yn,
		[EnumAdditional("PlusMinus")]
		PlusMinus,
		[EnumAdditional("OnOff")]
		OnOff,
		[EnumAdditional("PositiveNegative")]
		PositiveNegative,
		[EnumAdditional("UpDown")]
		UpDown,
		[EnumAdditional("RightLeft")]
		RightLeft,
		[EnumAdditional("OpenClose")]
		OpenClose
	}
}
