using System;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>Distance unit for use in conversion.</summary>
	public enum DistanceUnits
	{
		/// <summary>Kilometers.</summary>
		Kilometers,

		/// <summary>Miles.</summary>
		Miles
	}

	/// <summary>
	/// Indicates the altitude reference system to be used in defining a location.
	/// </summary>
	public enum AltitudeReferenceSystem
	{
		/// <summary>The altitude reference system was not specified.</summary>
		Unspecified = 0,

		/// <summary>The altitude reference system is based on distance above terrain or ground level</summary>
		Terrain = 1,

		/// <summary>The altitude reference system is based on an ellipsoid (usually WGS84), which is a mathematical approximation of the shape of the Earth.</summary>
		Ellipsoid = 2,

		/// <summary>The altitude reference system is based on the distance above sea level (parametrized by a so-called Geoid).</summary>
		Geoid = 3,

		/// <summary>The altitude reference system is based on the distance above the tallest surface structures, such as buildings, trees, roads, etc., above terrain or ground level.</summary>
		Surface = 4
	}

	/// <summary>
	/// Represents a physical location with the latitude, longitude, altitude and time information reported by the device.
	/// </summary>
	public class Location
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Location"/> class.
		/// </summary>
		public Location()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Location"/> class with the specified latitude and longitude.
		/// </summary>
		/// <param name="latitude">Latitude in degrees. Must be in the interval [-90, 90].</param>
		/// <param name="longitude">Longitude in degrees. Will be projected to the interval (-180, 180].</param>
		public Location(double latitude, double longitude) : this(latitude, longitude, DateTimeOffset.UtcNow)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Location"/> class with the specified latitude, longitude, and timestamp.
		/// </summary>
		/// <param name="latitude">Latitude in degrees. Must be in the interval [-90, 90].</param>
		/// <param name="longitude">Longitude in degrees. Will be projected to the interval (-180, 180].</param>
		/// <param name="timestamp">UTC timestamp for the location.</param>
		public Location(double latitude, double longitude, DateTimeOffset timestamp)
		{
			// check if latitude is in [-90, 90]
			if (Math.Abs(latitude) > 90)
				throw new ArgumentOutOfRangeException(nameof(latitude));
			else
				Latitude = latitude;

			// make sure that longitude is in (-180, 180]
			Longitude = longitude;
			while (Longitude > 180)
				Longitude -= 360;
			while (Longitude <= -180)
				Longitude += 360;

			Timestamp = timestamp;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Location"/> class with the specified latitude, longitude, and altitude.
		/// </summary>
		/// <param name="latitude">Latitude in degrees. Must be in the interval [-90, 90].</param>
		/// <param name="longitude">Longitude in degrees. Will be projected to the interval (-180, 180].</param>
		/// <param name="altitude">Altitude in meters.</param>
		public Location(double latitude, double longitude, double altitude) : this(latitude, longitude)
		{
			Altitude = altitude;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Location"/> class from an existing instance.
		/// </summary>
		/// <param name="point">A <see cref="Location"/> instance that will be used to clone.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="point"/> is <see langword="null"/>.</exception>
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
			ReducedAccuracy = point.ReducedAccuracy;
			Speed = point.Speed;
			Course = point.Course;
			IsFromMockProvider = point.IsFromMockProvider;
		}

		/// <summary>
		/// Gets or sets the timestamp of the location in UTC.
		/// </summary>
		public DateTimeOffset Timestamp { get; set; }

		/// <summary>
		/// Gets or sets the latitude coordinate of this location.
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// Gets or sets the longitude coordinate of this location.
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Gets the altitude in meters (if available) in a reference system which is specified by <see cref="AltitudeReferenceSystem"/>.
		/// </summary>
		/// <remarks>Returns 0 or <see langword="null"/> if not available.</remarks>
		public double? Altitude { get; set; }

		/// <summary>
		/// Gets or sets the horizontal accuracy (in meters) of the location.
		/// </summary>
		public double? Accuracy { get; set; }

		/// <summary>
		/// Gets or sets the vertical accuracy (in meters) of the location.
		/// </summary>
		public double? VerticalAccuracy { get; set; }

		/// <summary>
		/// Gets or sets whether this location has a reduced accuracy reading.
		/// </summary>
		/// <remarks>This functionality only applies to iOS. Other platforms will always report false.</remarks>
		public bool ReducedAccuracy { get; set; }

		/// <summary>
		/// Gets or sets the current speed in meters per second at the time when this location was determined.
		/// </summary>
		/// <remarks>
		/// <para>Returns 0 or <see langword="null"/> if not available. Otherwise the value will range between 0-360.</para>
		/// <para>Requires <see cref="Accuracy"/> to be <see cref="GeolocationAccuracy.High"/> or better
		/// and may not be returned when calling <see cref="Geolocation.GetLastKnownLocationAsync"/>.</para>
		/// </remarks>
		public double? Speed { get; set; }

		/// <summary>
		/// Gets or sets the current degrees relative to true north at the time when this location was determined.
		/// </summary>
		/// <remarks>Returns 0 or <see langword="null"/> if not available.</remarks>
		public double? Course { get; set; }

		/// <summary>
		/// Gets or sets whether this location originates from a mocked sensor and thus might not be the real location of the device.
		/// </summary>
		public bool IsFromMockProvider { get; set; }

		/// <summary>
		/// Specifies the reference system in which the <see cref="Altitude"/> value is expressed.
		/// </summary>
		public AltitudeReferenceSystem AltitudeReferenceSystem { get; set; }

		/// <summary>
		/// Calculate distance between two locations.
		/// </summary>
		/// <param name="latitudeStart">Latitude coordinate of the starting location.</param>
		/// <param name="longitudeStart">Longitude coordinate of the starting location.</param>
		/// <param name="locationEnd">The end location.</param>
		/// <param name="units">The unit in which the result distance is returned.</param>
		/// <returns>Distance between two locations in the unit selected.</returns>
		public static double CalculateDistance(double latitudeStart, double longitudeStart, Location locationEnd, DistanceUnits units) =>
			CalculateDistance(latitudeStart, longitudeStart, locationEnd.Latitude, locationEnd.Longitude, units);

		/// <summary>
		/// Calculate distance between two locations.
		/// </summary>
		/// <param name="locationStart">The start location.</param>
		/// <param name="latitudeEnd">Latitude coordinate of the end location.</param>
		/// <param name="longitudeEnd">Longitude coordinate of the end location.</param>
		/// <param name="units">The unit in which the result distance is returned.</param>
		/// <returns>Distance between two locations in the unit selected.</returns>
		public static double CalculateDistance(Location locationStart, double latitudeEnd, double longitudeEnd, DistanceUnits units) =>
			CalculateDistance(locationStart.Latitude, locationStart.Longitude, latitudeEnd, longitudeEnd, units);

		/// <summary>
		/// Calculate distance between two locations.
		/// </summary>
		/// <param name="locationStart">The start location.</param>
		/// <param name="locationEnd">The end location.</param>
		/// <param name="units">The unit in which the result distance is returned.</param>
		/// <returns>Distance between two locations in the unit selected.</returns>
		public static double CalculateDistance(Location locationStart, Location locationEnd, DistanceUnits units) =>
			CalculateDistance(locationStart.Latitude, locationStart.Longitude, locationEnd.Latitude, locationEnd.Longitude, units);

		/// <summary>
		/// Calculate distance between two <see cref="Location"/> instances.
		/// </summary>
		/// <param name="latitudeStart">Latitude coordinate of the starting location.</param>
		/// <param name="longitudeStart">Longitude coordinate of the starting location.</param>
		/// <param name="latitudeEnd">Latitude coordinate of the end location.</param>
		/// <param name="longitudeEnd">Longitude coordinate of the end location.</param>
		/// <param name="units">The unit in which the result distance is returned.</param>
		/// <returns>Distance between two locations in the unit selected.</returns>
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

		/// <summary>
		/// Returns a string representation of the current values of <see cref="Location"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>Latitude: {value}, Longitude: {value}, Altitude: {value}, Accuracy: {value}, VerticalAccuracy: {value}, Speed: {value}, Course: {value}, Timestamp: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(Latitude)}: {Latitude}, " +
			$"{nameof(Longitude)}: {Longitude}, " +
			$"{nameof(Altitude)}: {Altitude}, " +
			$"{nameof(Accuracy)}: {Accuracy}, " +
			$"{nameof(VerticalAccuracy)}: {VerticalAccuracy}, " +
			$"{nameof(Speed)}: {Speed}, " +
			$"{nameof(Course)}: {Course}, " +
			$"{nameof(Timestamp)}: {Timestamp}";

		/// <inheritdoc cref="object.Equals(object)"/>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			if (obj.GetType() != GetType())
				return false;
			var other = (Location)obj;
			return Latitude == other.Latitude && Longitude == other.Longitude;
		}

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Latitude.GetHashCode();
				hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		///	Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(Location left, Location right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(Location left, Location right)
		{
			return !Equals(left, right);
		}
	}
}
