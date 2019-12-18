using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity
{
	internal sealed class TimeSpanAttribute : Attribute
	{
		public Int32 Days { get; set; }
		public Int32 Hours { get; set; }
		public Int32 Minutes { get; set; }
		public Int32 Seconds { get; set; }

		public String Name
		{
			get;
			private set;
		}

		public TimeSpanAttribute(String name)
		{
			this.Name = name;
		}

		/// <summary>
		/// Calculates and returns the Timespan for this attributes state
		/// </summary>
		public TimeSpan GetTimeSpan()
		{
			return new TimeSpan(this.Days, this.Hours, this.Minutes, this.Seconds);
		}

		/// <summary>
		/// Uses reflection to retrieve an instance of this attribute 
		/// on a given enum
		/// </summary>
		public static TimeSpanAttribute RetrieveAttribute(ETemporalGroupFlag aTarget)
		{
			Object[] attributeCollection = aTarget.GetType().GetField(aTarget.ToString()).GetCustomAttributes(typeof(TimeSpanAttribute), true);

			if (attributeCollection.Length > 0)
			{
				return (TimeSpanAttribute)attributeCollection[0];
			}
			return null;
		}
	}

}
