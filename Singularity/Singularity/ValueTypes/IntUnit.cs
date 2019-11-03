using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public struct IntUnit : IUnitOfMeasure, IStateEmpty, IComparable, IComparable<Int32>, IComparable<IntUnit>, IFormattable
	{
		public IntUnit(Int32 value, IUnitOfMeasure unitOfMeasure)
		{
			_value = value;
			_abbreviation = unitOfMeasure.UnitAbbreviation;
			_mask = unitOfMeasure.Mask;
			_name = unitOfMeasure.Name;
			_description = unitOfMeasure.Description;
		}

		public IntUnit(Int32 value)
		{
			_value = value;
			_abbreviation = String.Empty;
			_mask = String.Empty;
			_name = String.Empty;
			_description = String.Empty;
		}
		private readonly Int32 _value;

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
			return (obj is IntUnit && (IntUnit)obj == this) ||
				 (obj is Int32 && _value.Equals(obj.ToInt()));
		}

		public override Int32 GetHashCode()
		{
			return _value.GetHashCode();
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator IntUnit(Int32 aValue)
		{
			return new IntUnit(aValue);
		}

		[DebuggerStepThrough]
		public static implicit operator IntUnit(BaseN value)
		{
			return new IntUnit(value.MValue);
		}

		[DebuggerStepThrough]
		public static implicit operator Int32(IntUnit value)
		{
			return value._value;
		}

		[DebuggerStepThrough]
		public static implicit operator IntUnit(Byte value)
		{
			return new IntUnit(value.ToInt());
		}

		[DebuggerStepThrough]
		public static implicit operator IntUnit(Int16 value)
		{
			return new IntUnit(value.ToInt());
		}

		[DebuggerStepThrough]
		public static explicit operator Int16(IntUnit value)
		{
			return Convert.ToInt16(value._value);
		}

		[DebuggerStepThrough]
		public static explicit operator Decimal(IntUnit value)
		{
			return Convert.ToDecimal(value);
		}

		#endregion

		#region Operator Overloads

		[DebuggerStepThrough]
		public static IntUnit operator ++(IntUnit aValue)
		{
			return aValue._value + 1;
		}

		[DebuggerStepThrough]
		public static IntUnit operator --(IntUnit aValue)
		{
			return aValue._value - 1;
		}

		#endregion

		#region IComparable and Generic Members

		public Int32 CompareTo(Object obj)
		{
			return _value.CompareTo(obj);
		}

		public Int32 CompareTo(Int32 other)
		{
			return _value.CompareTo(other);
		}

		public Int32 CompareTo(IntUnit other)
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

		public static IntUnit Zero => new IntUnit();
	}
}
