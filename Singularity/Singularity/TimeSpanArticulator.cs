using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity
{
	internal static class TimeSpanArticulator
	{
		// static constructor
		static TimeSpanArticulator()
		{
			_mGroupTypes = new List<TemporalGroupFlags>(Enum.GetValues(typeof(TemporalGroupFlags)) as IEnumerable<TemporalGroupFlags>);
		}

		/// <summary>
		/// Articulates a given TimeSpan using the default accuracy
		/// </summary>
		/// <param name="span">The TimeSpan to articulate</param>
		public static String Articulate(TimeSpan span)
		{
			return Articulate(span, DefaultAccuracy);
		}

		/// <summary>
		/// Articulates a given TimeSpan with a given accuracy
		/// </summary>
		/// <param name="span">The TimeSpan to articulate</param>
		/// <param name="accuracy">Accuracy Flags</param>
		public static String Articulate(TimeSpan span, TemporalGroupFlags accuracy)
		{
			// populate a list with temporalgroupings. Each temporal grouping
			// represents a particular element of the articulation, ordered
			// according to the temporal duration of each element.

			var lGroupingCollection = new List<TemporalGrouping>(4);

			// foreach possible temporal type (day/hour/minute etc.)
			foreach (var lType in _mGroupTypes)
			{
				// if the temporal type isn't specified in the accuracy, skip.
				if ((accuracy & lType) != lType) continue;

				// get the timespan for this temporal type
				var lTimeSpan = TimeSpanAttribute.RetrieveAttribute(lType).GetTimeSpan();

				if (span.Ticks >= lTimeSpan.Ticks)
				{
					// divide the current timespan with the temporal group span
					var magnitude = (Int32)(span.Ticks / lTimeSpan.Ticks);

					lGroupingCollection.Add(new TemporalGrouping(lType, magnitude));

					span = new TimeSpan(span.Ticks % lTimeSpan.Ticks);
				}
			}

			return Textify(lGroupingCollection);
		}

		/// <summary>
		/// converts a list of groupings into text
		/// </summary>
		private static String Textify(IList<TemporalGrouping> groupingsCollection)
		{
			var lStringBuilder = new StringBuilder();

			for (var idx = 0; idx < groupingsCollection.Count; idx++)
			{
				var lGroupingStr = groupingsCollection[idx].ToString();

				if (idx > 0)
				{
					if (idx == groupingsCollection.Count - 1)
					{
						// this is the last element. Add an "and"
						// between this and the last.
						lStringBuilder.Append(Space).Append(And).Append(Space);
					}
					else
					{
						// add comma between this and the next element
						lStringBuilder.Append(Seperator).Append(Space);
					}
				}
				lStringBuilder.Append(lGroupingStr);
			}

			return lStringBuilder.ToString();
		}

		// a cache of all the TemporalGroupTypes
		private static List<TemporalGroupFlags> _mGroupTypes;

		private static readonly String Seperator = ",";
		private static readonly String Plural = "s";
		private static readonly String And = "and";
		private static readonly String Space = ValueLib.Space.StringValue;

		private static readonly TemporalGroupFlags DefaultAccuracy =
			 TemporalGroupFlags.Hour | TemporalGroupFlags.Day |
			 TemporalGroupFlags.Week | TemporalGroupFlags.Month |
			 TemporalGroupFlags.Year;

		internal class TemporalGrouping
		{
			internal TemporalGrouping(TemporalGroupFlags type, Int32 magnitude)
			{
				this.Type = type;
				this.Magnitude = magnitude;
			}

			/// <summary>
			/// The type of the temporal grouping
			/// e.g. 'hour' or 'day'
			/// </summary>
			internal TemporalGroupFlags Type
			{
				get;
				private set;
			}

			/// <summary>
			/// The size of the temporal grouping.
			/// e.g. '1' hour, or '3' hours
			/// </summary>
			internal Int32 Magnitude
			{
				get;
				private set;
			}

			public override String ToString()
			{
				var result = this.Magnitude.ToString();
				result += ValueLib.Space.StringValue + TimeSpanAttribute.RetrieveAttribute(this.Type).Name;

				if (this.Magnitude > 1)
				{
					result += Plural;
				}

				return result;
			}
		}
	}
}
