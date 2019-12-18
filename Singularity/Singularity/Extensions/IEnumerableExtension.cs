using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming

namespace Singularity
{
	/// <summary>
	/// Static class to hold extenstion methods for the IEnumberable type
	/// </summary>
	[DebuggerStepThrough]
	public static class IEnumerableExtension
	{
		/// <summary>
		/// Allowing item-based exception handling on collections and LINQ statements.
		/// </summary>
		/// <typeparam name="T">Generic type.</typeparam>
		/// <param name="items">Source enumberable items.</param>
		/// <param name="action">Action to perform upon exception captured by any item.</param>
		/// <returns>Original items unchanged.</returns>
		public static IEnumerable<T> CatchExceptions<T>(this IEnumerable<T> items, Action<Exception> action = null)
		{
			using (var enumerator = items.GetEnumerator())
			{
				var next = true;

				while (next)
				{
					try
					{
						next = enumerator.MoveNext();
					}
					catch (SystemException ex)
					{
						action?.Invoke(ex);
						continue;
					}

					if (next)
					{
						yield return enumerator.Current;
					}
				}
			}
		}

		/// <summary>
		/// Allowing item-based exception handling on collections and LINQ statements.
		/// </summary>
		/// <typeparam name="T">Generic type.</typeparam>
		/// <typeparam name="TException">Generic exception type to capture.</typeparam>
		/// <param name="items">Source enumberable items.</param>
		/// <param name="action">Action to perform upon exception captured by any item.</param>
		/// <returns>Original items unchanged.</returns>
		public static IEnumerable<T> CatchExceptions<T, TException>(this IEnumerable<T> items, Action<Exception> action = null) where TException : Exception
		{
			using (var enumerator = items.GetEnumerator())
			{
				var next = true;

				while (next)
				{
					try
					{
						next = enumerator.MoveNext();
					}
					catch (TException ex)
					{
						action?.Invoke(ex);
						continue;
					}

					if (next)
					{
						yield return enumerator.Current;
					}
				}
			}
		}

		/// <summary>
		/// Go through a list of disposable items and dispose each one.
		/// </summary>
		/// <param name="disposableItems">A list of disposable items.</param>
		public static void Dispose(this IEnumerable<IDisposable> disposableItems)
		{
			if (disposableItems != null)
			{
				var actualDisposableItems = disposableItems.Where(i => i != null);
				actualDisposableItems.ForEach(i => i.Dispose());
			}
		}

		/// <summary>
		/// Get a distinct list of values based on a known function.
		/// </summary>
		/// <typeparam name="TValue">Generic data type of value.</typeparam>
		/// <typeparam name="TKey">Generic data type of key.</typeparam>
		/// <param name="sequence">A set of values.</param>
		/// <param name="keySelector">A function that returns a key for a given value.</param>
		/// <returns>Return the a distinct set of sequence values as found in the given keySelector function.</returns>
		[DebuggerStepThrough]
		public static IEnumerable<TValue> DistinctOn<TValue, TKey>(this IEnumerable<TValue> sequence, Func<TValue, TKey> keySelector)
		{
			var dummyUniqueSet = new HashSet<TKey>();
			return sequence.Where(value => dummyUniqueSet.Add(keySelector(value)));
		}

		/// <summary>
		/// Remove a list of other items from a list of items based on the outcome of a given function, rather than Equality Comparer.
		/// </summary>
		/// <typeparam name="T">The data type common to all lists and comparison function.</typeparam>
		/// <typeparam name="TKey">The data type of the key to the comparison function.</typeparam>
		/// <param name="items">Original items list.</param>
		/// <param name="otherItems">Exclusion list of items.</param>
		/// <param name="getKey">Function comparer.</param>
		/// <returns>A list items except those other items based on the getKey equality function.</returns>
		public static IEnumerable<T> Except<T, TKey>(this IEnumerable<T> items, IEnumerable<T> otherItems, Func<T, TKey> getKey)
		{
			return from item in items
					 join otherItem in otherItems on getKey(item) equals getKey(otherItem) into tempItems
					 from temp in tempItems.DefaultIfEmpty()
					 where temp == null || temp.Equals(default(T))
					 select item;

		}

		/// <summary>
		/// An inline function to execute an action across all items in an IEnumable.
		/// </summary>
		/// <typeparam name="T">Enumerable type</typeparam>
		/// <param name="enumerable">The enumerable to loop through.</param>
		/// <param name="action">The action to perform on each item.</param>
		[DebuggerStepThrough]
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			if (enumerable == null) throw new ArgumentException("Given enumerable argument cannot be null.", nameof(enumerable));
			if (action == null) throw new ArgumentException("Given action argument cannot be null.", nameof(action));

			foreach (var item in enumerable) action(item);
		}

		/// <summary>
		/// Calculate the Median of a set of decimal values.
		/// </summary>
		/// <param name="values">A set of values.</param>
		/// <returns>The median.</returns>
		[DebuggerStepThrough]
		public static Decimal Median(this IEnumerable<Decimal> values)
		{
			var sortedList = new List<Decimal>(values);
			sortedList.Sort();

			Decimal median;

			var half = (sortedList.Count - 1m) / 2;
			if (sortedList.Count % 2 == 1)
			{
				median = sortedList[Convert.ToInt32(Math.Ceiling(half))];
			}
			else
			{
				var middleValue = Convert.ToInt32(Math.Floor(half));
				median = (sortedList[middleValue] + sortedList[middleValue + 1]) / 2;
			}
			return median;
		}

		/// <summary>
		/// Calculate the Median of a set of Int64 values.
		/// </summary>
		/// <param name="values">A set of values.</param>
		/// <returns>The median.</returns>
		[DebuggerStepThrough]
		public static Double Median(this IEnumerable<Int64> values)
		{
			IList<Int64> sortedList = new List<Int64>(values);
			sortedList.Sort();

			Double median;

			var half = (sortedList.Count - 1m) / 2;
			if (sortedList.Count % 2 == 1)
			{
				median = sortedList[Convert.ToInt32(Math.Ceiling(half))];
			}
			else
			{
				var middleValue = Convert.ToInt32(Math.Floor(half));
				median = (Double)(sortedList[middleValue] + sortedList[middleValue + 1]) / 2;
			}
			return median;
		}

		/// <summary>
		/// Calculate the Median of a set of Single values.
		/// </summary>
		/// <param name="values">A set of values.</param>
		/// <returns>The median.</returns>
		[DebuggerStepThrough]
		public static Single Median(this IEnumerable<Single> values)
		{
			IList<Single> sortedList = new List<Single>(values);
			sortedList.Sort();

			Single median;
			var half = (sortedList.Count - 1m) / 2;
			if (sortedList.Count % 2 == 1)
			{
				median = sortedList[Convert.ToInt32(Math.Ceiling(half))];
			}
			else
			{
				var middleValue = Convert.ToInt32(Math.Floor(half));
				median = (sortedList[middleValue] + sortedList[middleValue + 1]) / 2;
			}
			return median;
		}

		/// <summary>
		/// Calculate the Standard Deviation of a set of decimal values.
		/// </summary>
		/// <param name="values">A set of values.</param>
		/// <returns>The standard deviation.</returns>
		[DebuggerStepThrough]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public static Double StandardDeviation(this IEnumerable<Decimal> values)
		{
			/* NB:	The original calculation required to iterate through the 'values' 3 times.  In order to reduce this to only iterate twice, has required
						to calculate the average manually so as a consequence we also add the 'counter' within a single iteration. */

			Decimal average = 0;
			var counter = 0;
			Decimal total = 0;
			foreach (var value in values)
			{
				total += value;
				counter++;
			}
			average = total / counter;

			var sumOfSqrs = values.Sum(value => Math.Pow((Double)(value - average), 2));
			return Math.Sqrt(sumOfSqrs / (counter - 1));
		}

		public static String ToCsv(this IEnumerable<String> items)
		{
			if (items.IsEmpty())
			{
				return String.Empty;
			}

			var csvBuilder = new DelimitedStringBuilder();
			foreach (var item in items)
			{
				csvBuilder.Add(ToCsvValue(item));
			}
			return csvBuilder.ToDelimitedString(ValueLib.Comma.CharValue);
		}

		/// <summary>
		/// Reduce the set of items into a single delimited string.
		/// </summary>
		/// <typeparam name="T">Generic data type of the list of items.</typeparam>
		/// <param name="items">The set of items.</param>
		/// <returns>A comma delimited string.</returns>
		public static String ToCsv<T>(this IEnumerable<T> items)
			where T : class
		{
			if (items.IsEmpty())
			{
				return String.Empty;
			}

			var csvBuilder = new StringBuilder();
			var properties = typeof(T).GetProperties().Where(p => !typeof(IEnumerable).IsAssignableFrom(p.ReflectedType)).ToArray();
			var header = String.Join(",", properties.Select(p => p.Name));
			csvBuilder.AppendLine(header);
			foreach (var item in items)
			{
				var idx = 0;
				var array = new String[properties.Length];
				foreach (var property in properties)
				{
					array[idx] = ToCsvValue(property.GetValue(item, null));
					idx++;
				}

				var line = String.Join(",", array);
				csvBuilder.AppendLine(line);
			}
			return csvBuilder.ToString();
		}

		/// <summary>
		/// Reduce the set of objects into a single delimited string.
		/// </summary>
		/// <param name="items">The set of items.</param>
		/// <returns>A comma delimited string.</returns>
		public static String ToCsv(this IEnumerable<Object> items)
		{
			if (items.IsEmpty())
			{
				return String.Empty;
			}

			var csvBuilder = new StringBuilder();
			var first = true;
			PropertyInfo[] properties = null;
			foreach (var item in items)
			{
				if (first)
				{
					first = false;
					properties = item.GetType().GetProperties();
					var header = String.Join(",", properties.Select(p => p.Name));
					csvBuilder.AppendLine(header);
				}
				var line = String.Join(",", properties.Select(p => p.GetValue(item, null).ToCsvValue()).ToArray());
				csvBuilder.AppendLine(line);
			}
			return csvBuilder.ToString();
		}

		/// <summary>
		/// Reduce the set of records into a single delimited string.
		/// </summary>
		/// <param name="items">The set of dictionary type items.</param>
		/// <returns>A comma delimited string.</returns>
		public static String ToCsv(this IEnumerable<IDictionary<String, Object>> items)
		{
			if (items.IsEmpty())
			{
				return String.Empty;
			}

			var csvBuilder = new StringBuilder();

			List<String> properties = null;
			var first = true;

			foreach (var item in items)
			{
				if (first) // add header
				{
					first = false;
					properties = item.Keys.ToList();
					var header = String.Join(",", properties);
					csvBuilder.AppendLine(header);
				}

				var line = String.Join(",",
					properties.Select(p =>
					{
						item.TryGetValue(p, out var value);
						return value.ToCsvValue();
					}).ToArray()
					);
				csvBuilder.AppendLine(line);
			}
			return csvBuilder.ToString();
		}

		/// <summary>
		/// Reduce the set of dynamic records into a single delimited string.
		/// </summary>
		/// <param name="items">The set of dynamic items.</param>
		/// <returns>A comma delimited string.</returns>
		public static String ToCsv(this IEnumerable<ExpandoObject> items)
		{
			if (items.IsEmpty()) return String.Empty;

			var csvBuilder = new StringBuilder();

			List<String> properties = null;
			var first = true;

			foreach (var eoItem in items)
			{
				var item = eoItem as IDictionary<String, Object>;
				if (first) // add header
				{
					first = false;
					properties = item.Keys.ToList();
					var header = String.Join(",", properties);
					csvBuilder.AppendLine(header);
				}

				var line = String.Join(",",
					properties.Select(p =>
					{
						item.TryGetValue(p, out var value);
						return value.ToCsvValue();
					}
					)
					.ToArray());
				csvBuilder.AppendLine(line);
			}
			return csvBuilder.ToString();
		}

		private static String ToCsvValue<T>(this T item)
		{
			if (item == null) return "\"\"";

			if (item is String)
			{
				return $"\"{item.ToString().Replace("\"", "\\\"").Replace(",", String.Empty)}\"";
			}

			if (Double.TryParse(item.ToString(), out _))
			{
				return $"{item}";
			}
			return $"\"{item}\"";
		}

		/// <summary>
		/// Convert a list into a human readable string delimited format.
		/// </summary>
		/// <typeparam name="T">Generic type that can be cast readily into a String.</typeparam>
		/// <param name="items">Items to convert into a String.</param>
		/// <param name="delimiter">Optional delimiter to use otherwise defaults to a comma followed by a space.</param>
		/// <returns>The list as a delimited string.</returns>
		public static String ToDelimitedString<T>(this IEnumerable<T> items, String delimiter = null)
		{
			var result = new DelimitedStringBuilder(items.Cast<String>());
			return result.ToDelimitedString(delimiter ?? ValueLib.CommaSpace.StringValue);
		}

		/// <summary>
		/// Build a simple HTML table base on the list.
		/// </summary>
		/// <typeparam name="T">Generic data type of the list of items.</typeparam>
		/// <param name="items">A List of generic items.</param>
		/// <param name="tableClass">The table overall style.</param>
		/// <param name="headerClass">The table header style.</param>
		/// <param name="rowClass">The primary table row style.</param>
		/// <param name="alternateRowClass">The alternate table row style.</param>
		/// <returns>A comma delimited string.</returns>
		public static String ToHtmlTable<T>(this IEnumerable<T> items, String tableClass, String headerClass, String rowClass, String alternateRowClass)
		{

			var result = new StringBuilder();
			if (String.IsNullOrEmpty(tableClass))
			{
				result.Append("<table id=\"" + typeof(T).Name + "Table\" >");
			}
			else
			{
				result.Append("<table id=\"" + typeof(T).Name + "Table\" class=\"" + tableClass + "\" >");
			}

			var propertyArray = typeof(T).GetProperties();
			foreach (var prop in propertyArray)
			{
				if (String.IsNullOrEmpty(headerClass))
				{
					result.AppendFormat("<th >{0}</th >", prop.Name);
				}
				else
				{
					result.AppendFormat("<th class=\"{0}\" >{1}</th >", headerClass, prop.Name);
				}
			}

			var lineCounter = 0;
			foreach (var item in items)
			{
				if (!String.IsNullOrEmpty(rowClass) && !String.IsNullOrEmpty(alternateRowClass))
				{
					result.AppendFormat("<tr class=\"{0}\" >", lineCounter % 2 == 0 ? rowClass : alternateRowClass);
				}
				else
				{
					result.AppendFormat("<tr >");
				}

				foreach (var prop in propertyArray)
				{
					var value = prop.GetValue(item, null);
					result.AppendFormat("<td >{0}</td >", value ?? String.Empty);
				}
				result.AppendLine("</tr >");
				lineCounter++;
			}
			result.Append("</table >");
			return result.ToString();
		}

		public static IOrderedEnumerable<T> OrderBys<T>(this IEnumerable<T> items, IEnumerable<SortElement> sortElements)
		{
			if (sortElements.IsEmpty())
			{
				return items.OrderBy(s => s);
			}

			var type = typeof(T);
			IOrderedEnumerable<T> result = null;
			var thenBy = false;


			foreach (var sortProperty in sortElements.Select(p => new { PropertyInfo = type.GetProperty(p.PropertyName), p.Descending }))
			{
				var parameter = Expression.Parameter(type, "o");
				var propertyInfo = sortProperty.PropertyInfo;
				var propertyType = propertyInfo.PropertyType;
				var descending = sortProperty.Descending;

				if (thenBy)
				{
					var prevExpr = Expression.Parameter(typeof(IOrderedEnumerable<T>), "prevExpr");
					var thenByExpression = Expression.Lambda<Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>>>(
						 Expression.Call(
							  (!descending ? ThenByMethod : ThenByDescendingMethod).MakeGenericMethod(type, propertyType),
							  prevExpr,
							  Expression.Lambda(
									typeof(Func<,>).MakeGenericType(type, propertyType), Expression.MakeMemberAccess(parameter, propertyInfo), parameter)
							  ),
						 prevExpr).Compile();

					result = thenByExpression(result);
				}
				else
				{
					var prevExpr = Expression.Parameter(typeof(IEnumerable<T>), "prevExpr");
					var orderByExpression = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(
						 Expression.Call(
							  (!descending ? OrderByMethod : OrderByDescendingMethod).MakeGenericMethod(type, propertyType),
							  prevExpr,
							  Expression.Lambda(
									typeof(Func<,>).MakeGenericType(type, propertyType),
									Expression.MakeMemberAccess(parameter, propertyInfo), parameter)
							  ),
						 prevExpr).Compile();

					result = orderByExpression(items);
					thenBy = true;
				}
			}
			return result;
		}

		private static readonly MethodInfo OrderByMethod =
			 MethodOf(() => default(IEnumerable<Object>).OrderBy(default(Func<Object, Object>))).GetGenericMethodDefinition();

		private static readonly MethodInfo OrderByDescendingMethod =
			 MethodOf(() => default(IEnumerable<Object>).OrderByDescending(default(Func<Object, Object>))).GetGenericMethodDefinition();

		private static readonly MethodInfo ThenByMethod =
			 MethodOf(() => default(IOrderedEnumerable<Object>).ThenBy(default(Func<Object, Object>))).GetGenericMethodDefinition();

		private static readonly MethodInfo ThenByDescendingMethod =
			 MethodOf(() => default(IOrderedEnumerable<Object>).ThenByDescending(default(Func<Object, Object>))).GetGenericMethodDefinition();

		public static MethodInfo MethodOf<T>(Expression<Func<T>> method)
		{
			var methodCallExpression = (MethodCallExpression)method.Body;
			return methodCallExpression.Method;
		}

		public static IEnumerable<TResult> LeftJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource> source,
																													IEnumerable<TInner> inner,
																													Func<TSource, TKey> pk,
																													Func<TInner, TKey> fk,
																													Func<TSource, TInner, TResult> result)
		{
			return from s in source
						 join i in inner
						 on pk(s) equals fk(i) into joinData
						 from left in joinData.DefaultIfEmpty()
						 select result(s, left);
		}


		public static IEnumerable<TResult> RightJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource> source,
																													IEnumerable<TInner> inner,
																													Func<TSource, TKey> pk,
																													Func<TInner, TKey> fk,
																													Func<TSource, TInner, TResult> result)
		{
			return from i in inner
						 join s in source
						 on fk(i) equals pk(s) into joinData
						 from right in joinData.DefaultIfEmpty()
						 select result(right, i);
		}


		public static IEnumerable<TResult> FullOuterJoinJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource> source,
																													IEnumerable<TInner> inner,
																													Func<TSource, TKey> pk,
																													Func<TInner, TKey> fk,
																													Func<TSource, TInner, TResult> result)
		{
			var left = source.LeftJoin(inner, pk, fk, result).ToList();
			var right = source.RightJoin(inner, pk, fk, result).ToList();

			return left.Union(right);
		}


		public static IEnumerable<TResult> LeftExcludingJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource> source,
																													IEnumerable<TInner> inner,
																													Func<TSource, TKey> pk,
																													Func<TInner, TKey> fk,
																													Func<TSource, TInner, TResult> result)
		{
			return from s in source
						 join i in inner
						 on pk(s) equals fk(i) into joinData
						 from left in joinData.DefaultIfEmpty()
						 where left == null
						 select result(s, left);
		}

		public static IEnumerable<TResult> RightExcludingJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource> source,
																										 IEnumerable<TInner> inner,
																										 Func<TSource, TKey> pk,
																										 Func<TInner, TKey> fk,
																										 Func<TSource, TInner, TResult> result)
		{
			return from i in inner
						 join s in source
						 on fk(i) equals pk(s) into joinData
						 from right in joinData.DefaultIfEmpty()
						 where right == null
						 select result(right, i);
		}


		public static IEnumerable<TResult> FulltExcludingJoin<TSource, TInner, TKey, TResult>(this IEnumerable<TSource> source,
																											 IEnumerable<TInner> inner,
																											 Func<TSource, TKey> pk,
																											 Func<TInner, TKey> fk,
																											 Func<TSource, TInner, TResult> result)
		{
			var left = source.LeftExcludingJoin(inner, pk, fk, result).ToList();
			var right = source.RightExcludingJoin(inner, pk, fk, result).ToList();

			return left.Union(right);
		}
	}
}


