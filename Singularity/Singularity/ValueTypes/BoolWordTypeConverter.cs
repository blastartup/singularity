using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

// ReSharper disable CheckNamespace

namespace Singularity
{
	/// <summary>
	/// Support for English like Boolean Words
	/// </summary>
	[DebuggerStepThrough]
	public sealed class BoolWordTypeConverter : TypeConverter
	{
		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// 
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture. </param><param name="value">The <see cref="T:System.Object"/> to convert. </param><exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			try
			{
				if (value is String)
				{
					return Convert.ToBoolean(value.ToString());
				}
				if (value is Char)
				{
					return Convert.ToBoolean(value);
				}
				if (value is Boolean)
				{
					return (Boolean)value;
				}
				if (value is BoolWord)
				{
					return (BoolWord)value;
				}
				if (value is Int32)
				{
					return Convert.ToBoolean(value);
				}
				if (value == DBNull.Value || value == null)
				{
					return BoolWord.False;
				}
				return base.ConvertFrom(context, culture, value);
			}
			catch (FormatException ex)
			{
				throw new FormatException("aValue {0}".FormatX(value), ex);
			}
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// 
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from. </param>
		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(Boolean) ||
					 sourceType == typeof(BoolWord) ||
				sourceType == typeof(Char) ||
				sourceType == typeof(String) ||
				sourceType == typeof(Int32) ||
				base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// 
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed. </param><param name="value">The <see cref="T:System.Object"/> to convert. </param><param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to. </param><exception cref="T:System.ArgumentNullException">The <paramref name="destinationType"/> parameter is null. </exception><exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
		{
			try
			{
				var boolValue = (BoolWord)value;

				if (destinationType == typeof(Boolean))
				{
					return (Boolean)boolValue;
				}
				if (destinationType == typeof(BoolWord))
				{
					return boolValue;
				}
				if (destinationType == typeof(String))
				{
					return boolValue.ToString();
				}
				if (destinationType == typeof(Char))
				{
					return boolValue.ToChar();
				}
				if (destinationType == typeof(Int32))
				{
					return boolValue.ToInt();
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
			catch (FormatException ex)
			{
				throw new FormatException("Unable to convert aValue {0} to type {1}".FormatX(value, destinationType.Name), ex);
			}
		}

		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// 
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to. </param>
		public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(Boolean)
				|| destinationType == typeof(BoolWord)
				|| destinationType == typeof(String)
				|| destinationType == typeof(Char)
				|| destinationType == typeof(Int32);
		}
	}
}
