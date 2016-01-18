using System;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable CheckNamespace

namespace Singularity
{
	/// <summary>
	/// Simple attribute class for storing String Values
	/// </summary>
	/// <remarks>Either needs to be used internally only, or if to be visible to users,
	/// it needs to return a culture specific string based on the given enum code - hence the name of the attribute.</remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class EnumAdditionalAttribute : Attribute, IComparable, IComparable<EnumAdditionalAttribute>
	{
		/// <summary>
		/// Creates a new <see cref="EnumAdditionalAttribute"/> instance.
		/// </summary>
		/// <param name="name">A name or heading to assign to Enum.</param>
		/// <param name="code">A code to assign to Enum.</param>
		/// <param name="description">Describe the Enum.</param>
		/// <param name="keyGuid">A primary key (Guid) as a string matching the equivelant enum in the database type table.</param>
		public EnumAdditionalAttribute(String name, String code = "", String description = "", String keyGuid = null, String additionalValue = null)
		{
			_name = name;
			_code = code;
			_description = description;
			_additionalValue = additionalValue;

			if (!keyGuid.IsEmpty() && keyGuid.IsGuid())
			{
				_key = keyGuid.ToGuid();
			}
		}

		/// <summary>
		/// Gets the enum Code.
		/// </summary>
		public String AdditionalValue => _additionalValue;
		private readonly String _additionalValue;

		/// <summary>
		/// Gets the enum Code.
		/// </summary>
		public String Code => _code;
		private readonly String _code;

		/// <summary>
		/// Gets the enum Description.
		/// </summary>
		public String Description => _description;
		private readonly String _description;

		/// <summary>
		/// Gets the enum Name.
		/// </summary>
		public String Name => _name;
		private readonly String _name;

		/// <summary>
		/// Gets the enum primary Key.
		/// </summary>
		public Guid? Key => _key;
		private readonly Guid? _key;

		/// <summary>
		/// Get or Set an alternate integer to the enum.
		/// </summary>
		public Int32 Value
		{
			get { return _value; }
			set { _value = value; }
		}

		private Int32 _value;

		/// <summary>
		/// Get or Set an alternate value to the enum.
		/// </summary>
		public String ValueName
		{
			get { return _valueName; }
			set { _valueName = value; }
		}
		private String _valueName;

		/// <summary>
		/// Compare to another EnumAdditionalAttribute
		/// </summary>
		/// <param name="other">Given EnumAdditionalAttribute to compare to.</param>
		/// <returns>Standard comparison values.</returns>
		public Int32 CompareTo(EnumAdditionalAttribute other)
		{
			return String.Compare(Name, other.Name, StringComparison.Ordinal);
		}

		/// <summary>
		/// Compare to another Object
		/// </summary>
		/// <param name="obj">Given Object to compare to.</param>
		/// <returns>Standard comparison values.</returns>
		public Int32 CompareTo(Object obj)
		{
			var attribute = obj as EnumAdditionalAttribute;
			return attribute != null ? String.Compare(Name, attribute.Name, StringComparison.Ordinal) : 0;
		}
	}
}
