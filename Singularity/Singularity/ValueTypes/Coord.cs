using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public struct Coord : IStateEmpty
	{
		public Coord(Double? latitude, Double? longitude)
			: this(latitude.ValueOnNull(0d), longitude.ValueOnNull(0d))
		{
			_latitude = (Single)latitude.ValueOnNull(0d);
			_longitude = (Single)longitude.ValueOnNull(0d);
		}

		public Coord(Single latitude, Single longitude)
			: this((Double)latitude, (Double)longitude)
		{
		}

		public Coord(Double latitude, Double longitude)
		{
			_latitude = latitude;
			_longitude = longitude;
		}

		public Double Latitude
		{
			get => _latitude;
			set => _latitude = value;
		}
		private Double _latitude;

		public Double Longitude
		{
			get => _longitude;
			set => _longitude = value;
		}
		private Double _longitude;

		public Double Distance(Coord position)
		{
			return Distance(position, DistanceUnit.Kilometers);
		}

		public Double Distance(Coord position, DistanceUnit unit)
		{
			Double r = 6371;

			switch (unit)
			{
				case DistanceUnit.Miles:
					r = 3960;
					break;
				case DistanceUnit.Kilometers:
					r = 6371;
					break;
				case DistanceUnit.Meters:
					r = 6371000;
					break;
			}

			Double dLat = (position.Latitude - Latitude).ToRadians();
			Double dLon = (position.Longitude - Longitude).ToRadians();
			Double d1 = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				 Math.Cos(Latitude.ToRadians()) * Math.Cos(position.Latitude.ToRadians()) *
				 Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			Double d2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(d1)));
			return r * d2;
		}

		public static readonly Coord Empty = new Coord(0, 0);
		
		public Boolean IsEmpty => Longitude.IsEmpty() && Latitude.IsEmpty();
	}
}