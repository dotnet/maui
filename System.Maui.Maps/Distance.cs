using System;
using System.Diagnostics;

namespace Xamarin.Forms.Maps
{
	public struct Distance
	{
		const double MetersPerMile = 1609.344;
		const double MetersPerKilometer = 1000.0;

		public Distance(double meters)
		{
			Meters = meters;
		}

		public double Meters { get; }

		public double Miles => Meters / MetersPerMile;

		public double Kilometers => Meters / MetersPerKilometer;

		public static Distance FromMiles(double miles)
		{
			if (miles < 0)
			{
				Debug.WriteLine("Negative values for distance not supported");
				miles = 0;
			}
			return new Distance(miles * MetersPerMile);
		}

		public static Distance FromMeters(double meters)
		{
			if (meters < 0)
			{
				Debug.WriteLine("Negative values for distance not supported");
				meters = 0;
			}
			return new Distance(meters);
		}

		public static Distance FromKilometers(double kilometers)
		{
			if (kilometers < 0)
			{
				Debug.WriteLine("Negative values for distance not supported");
				kilometers = 0;
			}
			return new Distance(kilometers * MetersPerKilometer);
		}

		public static Distance BetweenPositions(Position position1, Position position2)
		{
			var latitude1 = position1.Latitude.ToRadians();
			var longitude1 = position1.Longitude.ToRadians();

			var latitude2 = position2.Latitude.ToRadians();
			var longitude2 = position2.Longitude.ToRadians();

			var distance = Math.Sin((latitude2 - latitude1) / 2.0);
			distance *= distance;

			var intermediate = Math.Sin((longitude2 - longitude1) / 2.0);
			intermediate *= intermediate;

			distance = distance + Math.Cos(latitude1) * Math.Cos(latitude2) * intermediate;
			distance = 2 * GeographyUtils.EarthRadiusKm * Math.Atan2(Math.Sqrt(distance), Math.Sqrt(1 - distance));

			return FromKilometers(distance);
		}

		public bool Equals(Distance other)
		{
			return Meters.Equals(other.Meters);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is Distance && Equals((Distance)obj);
		}

		public override int GetHashCode()
		{
			return Meters.GetHashCode();
		}

		public static bool operator ==(Distance left, Distance right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Distance left, Distance right)
		{
			return !left.Equals(right);
		}
	}
}