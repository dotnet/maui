using System;

using static System.Math;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DistanceUnits.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DistanceUnits']/Docs" />
	public enum DistanceUnits
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DistanceUnits.xml" path="//Member[@MemberName='Kilometers']/Docs" />
		Kilometers,
		/// <include file="../../docs/Microsoft.Maui.Essentials/DistanceUnits.xml" path="//Member[@MemberName='Miles']/Docs" />
		Miles
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AltitudeReferenceSystem.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AltitudeReferenceSystem']/Docs" />
	public enum AltitudeReferenceSystem
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AltitudeReferenceSystem.xml" path="//Member[@MemberName='Unspecified']/Docs" />
		Unspecified = 0,
		/// <include file="../../docs/Microsoft.Maui.Essentials/AltitudeReferenceSystem.xml" path="//Member[@MemberName='Terrain']/Docs" />
		Terrain = 1,
		/// <include file="../../docs/Microsoft.Maui.Essentials/AltitudeReferenceSystem.xml" path="//Member[@MemberName='Ellipsoid']/Docs" />
		Ellipsoid = 2,
		/// <include file="../../docs/Microsoft.Maui.Essentials/AltitudeReferenceSystem.xml" path="//Member[@MemberName='Geoid']/Docs" />
		Geoid = 3,
		/// <include file="../../docs/Microsoft.Maui.Essentials/AltitudeReferenceSystem.xml" path="//Member[@MemberName='Surface']/Docs" />
		Surface = 4
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Location']/Docs" />
	public class Location
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public Location()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public Location(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
			Timestamp = DateTimeOffset.UtcNow;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='.ctor'][4]/Docs" />
		public Location(double latitude, double longitude, DateTimeOffset timestamp)
		{
			Latitude = latitude;
			Longitude = longitude;
			Timestamp = timestamp;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='.ctor'][5]/Docs" />
		public Location(double latitude, double longitude, double altitude)
		{
			Latitude = latitude;
			Longitude = longitude;
			Altitude = altitude;
			Timestamp = DateTimeOffset.UtcNow;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public Location(Location point)
		{
			if (point == null)
				throw new ArgumentNullException(nameof(point));

			Latitude = point.Latitude;
			Longitude = point.Longitude;
			Timestamp = DateTime.UtcNow;
			Altitude = point.Altitude;
			Accuracy = point.Accuracy;
			VerticalAccuracy = point.VerticalAccuracy;
			Speed = point.Speed;
			Course = point.Course;
			IsFromMockProvider = point.IsFromMockProvider;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Timestamp']/Docs" />
		public DateTimeOffset Timestamp { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Latitude']/Docs" />
		public double Latitude { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Longitude']/Docs" />
		public double Longitude { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Altitude']/Docs" />
		public double? Altitude { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Accuracy']/Docs" />
		public double? Accuracy { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='VerticalAccuracy']/Docs" />
		public double? VerticalAccuracy { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Speed']/Docs" />
		public double? Speed { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='Course']/Docs" />
		public double? Course { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='IsFromMockProvider']/Docs" />
		public bool IsFromMockProvider { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='AltitudeReferenceSystem']/Docs" />
		public AltitudeReferenceSystem AltitudeReferenceSystem { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='CalculateDistance'][2]/Docs" />
		public static double CalculateDistance(double latitudeStart, double longitudeStart, Location locationEnd, DistanceUnits units) =>
			CalculateDistance(latitudeStart, longitudeStart, locationEnd.Latitude, locationEnd.Longitude, units);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='CalculateDistance'][3]/Docs" />
		public static double CalculateDistance(Location locationStart, double latitudeEnd, double longitudeEnd, DistanceUnits units) =>
		   CalculateDistance(locationStart.Latitude, locationStart.Longitude, latitudeEnd, longitudeEnd, units);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='CalculateDistance'][1]/Docs" />
		public static double CalculateDistance(Location locationStart, Location locationEnd, DistanceUnits units) =>
			CalculateDistance(locationStart.Latitude, locationStart.Longitude, locationEnd.Latitude, locationEnd.Longitude, units);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='CalculateDistance'][4]/Docs" />
		public static double CalculateDistance(
			double latitudeStart,
			double longitudeStart,
			double latitudeEnd,
			double longitudeEnd,
			DistanceUnits units)
		{
			switch (units)
			{
				case DistanceUnits.Kilometers:
					return UnitConverters.CoordinatesToKilometers(latitudeStart, longitudeStart, latitudeEnd, longitudeEnd);
				case DistanceUnits.Miles:
					return UnitConverters.CoordinatesToMiles(latitudeStart, longitudeStart, latitudeEnd, longitudeEnd);
				default:
					throw new ArgumentOutOfRangeException(nameof(units));
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Location.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(Latitude)}: {Latitude}, " +
			$"{nameof(Longitude)}: {Longitude}, " +
			$"{nameof(Altitude)}: {Altitude}, " +
			$"{nameof(Accuracy)}: {Accuracy}, " +
			$"{nameof(VerticalAccuracy)}: {VerticalAccuracy}, " +
			$"{nameof(Speed)}: {Speed}, " +
			$"{nameof(Course)}: {Course}, " +
			$"{nameof(Timestamp)}: {Timestamp}";
	}
}
