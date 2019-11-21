using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public sealed class BaseNTypeConverter : TypeConverter
	{
		public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			try
			{
				if (value is String)
				{
					String lValue = (String)value;
					if (!lValue.IsEmpty())
					{
						return new BaseN(Convert.ToInt32(lValue, Factory.CurrentCultureInfo));
					}
					return BaseN.Zero;
				}

				if (value is Int32 || value is Int16 || value is Byte)
				{
					return new BaseN(Convert.ToInt32(value, Factory.CurrentCultureInfo));
				}

				if (value is BaseN)
				{
					return value;
				}

				if (value == DBNull.Value || value == null)
				{
					return BaseN.Zero;
				}
				return base.ConvertFrom(context, culture, value);
			}
			catch (FormatException ex)
			{
				throw new FormatException("aValue {0}".FormatX(value), ex);
			}
		}

		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(Int32)
			|| sourceType == typeof(BaseN)
			|| sourceType == typeof(Int16)
			|| sourceType == typeof(Byte)
			|| sourceType == typeof(String)
			|| base.CanConvertFrom(context, sourceType);
		}
	}
}
