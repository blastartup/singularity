using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public struct DecimalUnit : IUnitOfMeasure, IStateEmpty, IComparable, IComparable<Decimal>, IComparable<DecimalUnit>, IFormattable
	{
		public DecimalUnit(Decimal value, IUnitOfMeasure unitOfMeasure, Func<Decimal, Decimal> formatConversion)
		{
			_value = value;
			_abbreviation = unitOfMeasure.UnitAbbreviation;
			_mask = unitOfMeasure.Mask;
			_name = unitOfMeasure.Name;
			_description = unitOfMeasure.Description;
			_formatConversion = formatConversion;
		}

		public DecimalUnit(Decimal value, IUnitOfMeasure unitOfMeasure)
		{
			_value = value;
			_abbreviation = unitOfMeasure.UnitAbbreviation;
			_mask = unitOfMeasure.Mask;
			_name = unitOfMeasure.Name;
			_description = unitOfMeasure.Description;
			_formatConversion = null;
		}

		public DecimalUnit(Decimal value)
		{
			_value = value;
			_abbreviation = String.Empty;
			_mask = String.Empty;
			_name = String.Empty;
			_description = String.Empty;
			_formatConversion = null;
		}
		private readonly Decimal _value;
		private readonly Func<Decimal, Decimal> _formatConversion;

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
			return (obj is DecimalUnit && (DecimalUnit)obj == this) ||
				 (obj is Decimal && _value.Equals(Convert.ToDecimal(obj)));
		}

		public override Int32 GetHashCode()
		{
			return _value.GetHashCode();
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator DecimalUnit(Decimal aValue)
		{
			return new DecimalUnit(aValue);
		}

		[DebuggerStepThrough]
		public static implicit operator DecimalUnit(BaseN aValue)
		{
			return new DecimalUnit(aValue.MValue);
		}

		[DebuggerStepThrough]
		public static implicit operator Decimal(DecimalUnit value)
		{
			return value._value;
		}

		[DebuggerStepThrough]
		public static implicit operator DecimalUnit(Byte value)
		{
			return new DecimalUnit(Convert.ToDecimal(value));
		}

		[DebuggerStepThrough]
		public static implicit operator DecimalUnit(Int16 value)
		{
			return new DecimalUnit(Convert.ToDecimal(value));
		}

		[DebuggerStepThrough]
		public static implicit operator DecimalUnit(Int32 value)
		{
			return new DecimalUnit(Convert.ToDecimal(value));
		}

		[DebuggerStepThrough]
		public static explicit operator Int16(DecimalUnit value)
		{
			return Convert.ToInt16(value._value);
		}

		[DebuggerStepThrough]
		public static explicit operator Int32(DecimalUnit value)
		{
			return Convert.ToInt32(value._value);
		}

		[DebuggerStepThrough]
		public static explicit operator Double(DecimalUnit value)
		{
			return Convert.ToDouble(value);
		}

		#endregion

		#region Operator Overloads

		[DebuggerStepThrough]
		public static Boolean operator ==(DecimalUnit leftUnit, DecimalUnit rightLeftDecimalUnit)
		{
			return leftUnit._value.Equals(rightLeftDecimalUnit._value);
		}

		[DebuggerStepThrough]
		public static Boolean operator !=(DecimalUnit leftUnit, DecimalUnit rightDoubleUnit)
		{
			return !(leftUnit._value.Equals(rightDoubleUnit._value));
		}

		[DebuggerStepThrough]
		public static DecimalUnit operator ++(DecimalUnit aValue)
		{
			return aValue._value + 1m;
		}

		[DebuggerStepThrough]
		public static DecimalUnit operator --(DecimalUnit aValue)
		{
			return aValue._value - 1m;
		}

		#endregion

		#region IComparable and Generic Members

		public Int32 CompareTo(Object obj)
		{
			return _value.CompareTo(obj);
		}

		public Int32 CompareTo(Decimal other)
		{
			return _value.CompareTo(other);
		}

		public Int32 CompareTo(DecimalUnit other)
		{
			return _value.CompareTo(other._value);
		}

		#endregion IComparable and Generic Members

		public override String ToString()
		{
			return !Mask.IsEmpty() ? Mask.FormatX(FormattedValue, UnitAbbreviation) : String.Empty;
		}

		private Decimal FormattedValue => _formatConversion == null ? _value : _formatConversion(_value);

		#region IFormattable Members

		public String ToString(String format, IFormatProvider formatProvider)
		{
			return _value.ToString(format, formatProvider);
		}

		#endregion

		public static DecimalUnit Zero => new DecimalUnit();
	}
}
