using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Singularity.DataService
{
	public class DataValidationResult : ValidationResult
	{
		public DataValidationResult(String errorMessage) : base(errorMessage, null)
		{ }

		public DataValidationResult(String errorMessage, IEnumerable<String> memberNames, IEnumerable<Object> entities) : base(errorMessage, memberNames)
		{
			_entities = entities;
		}

		public DataValidationResult(String errorMessage, String memberName, Object entity, String propertyName = null) : base(errorMessage, null)
		{
			_memberName = memberName;
			_propertyName = propertyName;
			_entities = new List<Object>() { entity };
		}


		public override String ToString()
		{
			if (MemberName.IsEmpty() && PropertyName.IsEmpty() && MemberNames.IsEmpty())
			{
				return $"Error: {ErrorMessage}";
			}
			if (!MemberNames.IsEmpty())
			{
				return $"Error: {ErrorMessage} Member Names: " + String.Join(ValueLib.CommaSpace.StringValue, MemberNames);
			}
			if (PropertyName.IsEmpty())
			{
				return $"Error: {ErrorMessage}, Member Name: {MemberName}, ";
			}
			return $"Error: {ErrorMessage}, Member Name: {MemberName}, Property: {PropertyName}, ";
		}

		public String MemberName => _memberName;
		private String _memberName;

		public String PropertyName => _propertyName;
		private String _propertyName;

		public IEnumerable<Object> Entities => _entities;
		private IEnumerable<Object> _entities;
	}
}
