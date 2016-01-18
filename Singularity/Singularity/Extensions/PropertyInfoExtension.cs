﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Singularity
{
	[DebuggerStepThrough]
	public static class PropertyInfoExtension
	{
		public static T GetAttribute<T>(this PropertyInfo property) where T : Attribute
		{
			T result = null;
			if (Attribute.IsDefined(property, typeof(T)))
			{
				result = property.GetCustomAttributes(typeof(T), false)[0] as T;
			}
			return result;
		}

		public static T GetField<T>(this PropertyInfo property) where T : Attribute
		{
			T result = null;
			if (Attribute.IsDefined(property, typeof(T)))
			{
				result = property.GetCustomAttributes(typeof(T), false)[0] as T;
			}
			return result;
		}

		public static IList<T> GetFields<T>(this PropertyInfo property) where T : Attribute
		{
			var result = new List<T>();
			if (Attribute.IsDefined(property, typeof(T)))
			{
				result.AddRange(property.GetCustomAttributes(typeof(T), false).Cast<T>());
			}
			return result;
		}

		public static void AssertSingleAttribute<TAttribute>(this PropertyInfo property, Type parentType) where TAttribute : Attribute
		{
			if (property.GetCustomAttributes(typeof(TAttribute), true).Length > 1)
			{
				throw new InvalidOperationException("{0}.{1} has too many [{2}] - it should only have one.".FormatX(parentType.FullName, property.Name, typeof(TAttribute).Name));
			}
		}

		public static void AssertAttributeOccurrence<TAttribute>(this PropertyInfo property, Type parentType, Int32 maxOccurrence) where TAttribute : Attribute
		{
			if (property.GetCustomAttributes(typeof(TAttribute), true).Length > maxOccurrence)
			{
				throw new InvalidOperationException("{0}.{1} has too many [{2}] - it should only have {3} occurrence(s).".FormatX(parentType.FullName, property.Name, typeof(TAttribute).Name, maxOccurrence));
			}
		}

	}
}
