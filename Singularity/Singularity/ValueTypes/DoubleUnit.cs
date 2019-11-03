using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public struct DoubleUnit : IUnitOfMeasure, IStateEmpty, IComparable, IComparable<Double>, IComparable<DoubleUnit>, IFormattable
	{
		public DoubleUnit(Double value, IUnitOfMeasure unitOfMeasure)
		{
			_value = value;
			_abbreviation = unitOfMeasure.UnitAbbreviation;
			_mask = unitOfMeasure.Mask;
			_name = unitOfMeasure.Name;
			_description = unitOfMeasure.Description;
		}

		public DoubleUnit(Double value)
		{
			_value = value;
			_abbreviation = String.Empty;
			_mask = String.Empty;
			_name = String.Empty;
			_description = String.Empty;
		}
		private readonly Double _value;

		public String UnitAbbreviation => _abbreviation;
		private readonly String _abbreviation;

		public String Mask => _mask;
		private readonly String _mask;

		public String Name => _name;
		private readonly String _name;

		public String Description => _description;
		private readonly String _description;

		public Boolean IsEmpty => _value.IsEmpty();

		#region Object Overrides

		public override Boolean Equals(Object obj)
		{
			return (obj is DoubleUnit && (DoubleUnit)obj == this) ||
				 (obj is Double && _value.Equals(Convert.ToDouble(obj)));
		}

		public override Int32 GetHashCode()
		{
			return _value.GetHashCode();
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator DoubleUnit(Double value)
		{
			return new DoubleUnit(value);
		}

		[DebuggerStepThrough]
		public static implicit operator DoubleUnit(BaseN value)
		{
			return new DoubleUnit(value.MValue);
		}

		[DebuggerStepThrough]
		public static implicit operator Double(DoubleUnit value)
		{
			return value._value;
		}

		[DebuggerStepThrough]
		public static implicit operator DoubleUnit(Byte value)
		{
			return new DoubleUnit(Convert.ToDouble(value));
		}

		[DebuggerStepThrough]
		public static implicit operator DoubleUnit(Int16 value)
		{
			return new DoubleUnit(Convert.ToDouble(value));
		}

		[DebuggerStepThrough]
		public static implicit operator DoubleUnit(Int32 value)
		{
			return new DoubleUnit(Convert.ToDouble(value));
		}

		[DebuggerStepThrough]
		public static explicit operator Int16(DoubleUnit value)
		{
			return Convert.ToInt16(value._value);
		}

		[DebuggerStepThrough]
		public static explicit operator Int32(DoubleUnit value)
		{
			return Convert.ToInt32(value._value);
		}

		[DebuggerStepThrough]
		public static explicit operator Decimal(DoubleUnit value)
		{
			return Convert.ToDecimal(value);
		}

		#endregion

		#region Operator Overloads

		[DebuggerStepThrough]
		public static Boolean operator ==(DoubleUnit leftUnit, DoubleUnit rightUnit)
		{
			return leftUnit._value.Equals(rightUnit._value);
		}

		[DebuggerStepThrough]
		public static Boolean operator !=(DoubleUnit leftUnit, DoubleUnit rightUnit)
		{
			return !(leftUnit._value.Equals(rightUnit._value));
		}

		[DebuggerStepThrough]
		public static DoubleUnit operator ++(DoubleUnit aValue)
		{
			return aValue._value + 1d;
		}

		[DebuggerStepThrough]
		public static DoubleUnit operator --(DoubleUnit aValue)
		{
			return aValue._value - 1d;
		}

		#endregion

		#region IComparable and Generic Members

		public Int32 CompareTo(Object obj)
		{
			return _value.CompareTo(obj);
		}

		public Int32 CompareTo(Double other)
		{
			return _value.CompareTo(other);
		}

		public Int32 CompareTo(DoubleUnit other)
		{
			return _value.CompareTo(other._value);
		}

		#endregion IComparable and Generic Members

		public override String ToString()
		{
			return !Mask.IsEmpty() ? Mask.FormatX(_value, UnitAbbreviation) : String.Empty;
		}

		#region IFormattable Members

		public String ToString(String format, IFormatProvider formatProvider)
		{
			return _value.ToString(format, formatProvider);
		}

		#endregion

		public static DoubleUnit Zero => new DoubleUnit();
	}
}
