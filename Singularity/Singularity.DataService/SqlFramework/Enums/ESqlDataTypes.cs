using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	// Type: Microsoft.SqlServer.Management.Smo.SqlDataType
	// Assembly: Microsoft.SqlServer.Smo, Version=15.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91

	/// <summary>
	/// The SqlDataType specifies the type of the DataType object.
	/// </summary>
	public enum ESqlDataTypes
	{
		None = 0,
		BigInt = 1,
		Binary = 2,
		Bit = 3,
		Char = 4,
		DateTime = 6,
		Decimal = 7,
		Float = 8,
		Image = 9,
		Int = 10, // 0x0000000A
		Money = 11, // 0x0000000B
		NChar = 12, // 0x0000000C
		NText = 13, // 0x0000000D
		NVarChar = 14, // 0x0000000E
		NVarCharMax = 15, // 0x0000000F
		Real = 16, // 0x00000010
		SmallDateTime = 17, // 0x00000011
		SmallInt = 18, // 0x00000012
		SmallMoney = 19, // 0x00000013
		Text = 20, // 0x00000014
		Timestamp = 21, // 0x00000015
		TinyInt = 22, // 0x00000016
		UniqueIdentifier = 23, // 0x00000017
		//UserDefinedDataType = 24, // 0x00000018
		//UserDefinedType = 25, // 0x00000019
		VarBinary = 28, // 0x0000001C
		VarBinaryMax = 29, // 0x0000001D
		VarChar = 30, // 0x0000001E
		VarCharMax = 31, // 0x0000001F
		Variant = 32, // 0x00000020
		Xml = 33, // 0x00000021
		SysName = 34, // 0x00000022
		Numeric = 35, // 0x00000023
		Date = 36, // 0x00000024
		Time = 37, // 0x00000025
		DateTimeOffset = 38, // 0x00000026
		DateTime2 = 39, // 0x00000027
		UserDefinedTableType = 40, // 0x00000028
		HierarchyId = 41, // 0x00000029
		Geometry = 42, // 0x0000002A
		Geography = 43, // 0x0000002B
	}
}
