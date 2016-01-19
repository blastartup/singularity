using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace Singularity
{
	/// <LastModified Date="25-Jul-13" User="dean">New Boolean Word Structure.</LastModified>
	[TypeConverter(typeof(BoolWordTypeConverter))]
	[DebuggerDisplay("{_mValue}")]
	public struct BoolWord : IComparable, IComparable<BoolWord>, IComparable<Boolean>, IStateEmpty, IStateValid
	{
		/// <summary>
		/// Initialise BoolWord with a default value.
		/// </summary>
		/// <param name="aValue">A value if convertible to Boolean will be the default value otherwise False will be used.</param>
		/// <exception cref="FormatException"></exception>
		[DebuggerStepThrough]
		public BoolWord(Object aValue)
		{
			if (aValue is Boolean)
			{
				this = new BoolWord((Boolean)aValue);
			}
         else
         {
				var typeConverter = new BoolWordTypeConverter();
				if (aValue != null && typeConverter.CanConvertFrom(aValue.GetType()))
				{
					this = (BoolWord)typeConverter.ConvertFrom(aValue);
				}
				else
				{
					throw new FormatException("The given argument (aValue) type ({0}) is not supported by {1}.".FormatX(aValue.GetType(), typeof(BoolWord)));
				}
			}
		}

		[DebuggerStepThrough]
		public BoolWord(Boolean aValue)
		{
			_mValue = aValue;
			_mDefault = false;
			_mType = EBoolWordType.TrueFalse;
		}

		[DebuggerStepThrough]
		public BoolWord(Boolean aValue, EBoolWordType aType)
		{
			_mValue = aValue;
			_mDefault = false;
			_mType = aType;
		}

		[DebuggerStepThrough]
		public BoolWord(Char aValue)
		{
			var idx = (((IList)MFalseCollection).IndexOf(aValue.ToString().ToUpper()) + 1) * -1;
			idx += ((IList)MTrueCollection).IndexOf(aValue.ToString().ToUpper()) + 1;
			_mValue = Convert.ToBoolean(idx);
			if (!idx.Equals(0))
			{
				_mType = (EBoolWordType)idx;
			}
			else
			{
				_mType = EBoolWordType.TrueFalse;
			}
			_mDefault = false;
		}

		[DebuggerStepThrough]
		public BoolWord(String value)
		{
			var lookupValue = value.FirstCaps();
			var idx = (((IList)MFalseCollection).IndexOf(lookupValue) + 1) * -1;
			idx += ((IList)MTrueCollection).IndexOf(lookupValue) + 1;
			_mValue = Convert.ToBoolean(idx);
			if (!idx.Equals(0))
			{
				_mType = (EBoolWordType)idx;
			}
			else
			{
				_mType = EBoolWordType.TrueFalse;
			}
			_mDefault = false;
		}

		private static readonly String[] MTrueCollection = { "True", "T", "Yes", "Y", "+", "On", "Positive", "Up", "Right", "Open" };
		private static readonly String[] MFalseCollection = { "False", "F", "No", "N", "-", "Off", "Negative", "Down", "Left", "Close" };

		#region Object Overrides
		public override Boolean Equals(Object obj)
		{
			return (obj is BoolWord && (BoolWord)obj == this) ||
					(obj is Boolean && (Boolean)obj == _mValue);
		}

		public override Int32 GetHashCode()
		{
			return _mValue.GetHashCode();
		}

		public Char ToChar()
		{
			var result = 'F';
			if (_mType < EBoolWordType.OnOff)
			{
				result = _mValue ? MTrueCollection[(Int32)_mType][0] : MFalseCollection[(Int32)_mType][0];
			}
			return result;
		}

		public override String ToString()
		{
			return _mValue ? MTrueCollection[(Int32)_mType] : MFalseCollection[(Int32)_mType];
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator BoolWord(Boolean aValue)
		{
			return aValue ? BoolWord.True : BoolWord.False;
		}

		[DebuggerStepThrough]
		public static implicit operator Boolean(BoolWord aValue)
		{
			return aValue._mValue;
		}

		#endregion

		#region True/False Constants
		public static readonly BoolWord True = new BoolWord(true);
		public static readonly BoolWord False = new BoolWord(false);
		#endregion

		public Boolean IsEmpty
		{
			get { return false; }
		}

		public Boolean IsDefault
		{
			get { return this == _mDefault; }
		}

		public BoolWord Default
		{
			get { return _mDefault; }
			set { _mDefault = value._mValue; }
		}

		private Boolean _mDefault;

		#region IFStateValid Members

		public Boolean IsValid
		{
			get { return true; }
		}

		#endregion

		#region IComparable and Generic Members

		public Int32 CompareTo(Object aOther)
		{
			return _mValue.CompareTo(aOther);
		}

		public Int32 CompareTo(BoolWord aOther)
		{
			return _mValue.CompareTo(aOther._mValue);
		}

		public Int32 CompareTo(Boolean aOther)
		{
			return _mValue.CompareTo(aOther);
		}

		#endregion

		private readonly Boolean _mValue;
		private readonly EBoolWordType _mType;
	}
}
