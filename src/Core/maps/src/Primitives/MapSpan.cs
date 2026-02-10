using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a rectangular region on the map, defined by a center point and span.
	/// </summary>
	public sealed class MapSpan
	{
		const double EarthCircumferenceKm = GeographyUtils.EarthRadiusKm * 2 * Math.PI;
		const double MinimumRangeDegrees = 0.001 / EarthCircumferenceKm * 360; // 1 meter

		/// <summary>
		/// Initializes a new instance of the <see cref="MapSpan"/> class with the specified center and span in degrees.
		/// </summary>
		/// <param name="center">The center location of the span.</param>
		/// <param name="latitudeDegrees">The latitude span in degrees.</param>
		/// <param name="longitudeDegrees">The longitude span in degrees.</param>
		public MapSpan(Location center, double latitudeDegrees, double longitudeDegrees)
		{
			Center = center;
			LatitudeDegrees = Math.Min(Math.Max(latitudeDegrees, MinimumRangeDegrees), 90.0);
			LongitudeDegrees = Math.Min(Math.Max(longitudeDegrees, MinimumRangeDegrees), 180.0);
		}

		/// <summary>
		/// Gets the center location of this span.
		/// </summary>
		public Location Center { get; }

		/// <summary>
		/// Gets the latitude span in degrees.
		/// </summary>
		public double LatitudeDegrees { get; }

		/// <summary>
		/// Gets the longitude span in degrees.
		/// </summary>
		public double LongitudeDegrees { get; }

		/// <summary>
		/// Gets the approximate radius of the span.
		/// </summary>
		public Distance Radius
		{
			get
			{
				double latKm = LatitudeDegreesToKm(LatitudeDegrees);
				double longKm = LongitudeDegreesToKm(Center, LongitudeDegrees);
				return new Distance(1000 * Math.Min(latKm, longKm) / 2);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MapSpan"/> with latitude clamped to the specified bounds.
		/// </summary>
		/// <param name="north">The northern boundary (will be clamped to 0 to 90).</param>
		/// <param name="south">The southern boundary (will be clamped to -90 to 0).</param>
		/// <returns>A new <see cref="MapSpan"/> with the clamped latitude.</returns>
		public MapSpan ClampLatitude(double north, double south)
		{
			north = Math.Min(Math.Max(north, 0), 90);
			south = Math.Max(Math.Min(south, 0), -90);
			double lat = Math.Max(Math.Min(Center.Latitude, north), south);
			double maxDLat = Math.Min(north - lat, -south + lat) * 2;
			return new MapSpan(new Location(lat, Center.Longitude), Math.Min(LatitudeDegrees, maxDLat), LongitudeDegrees);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj is MapSpan && Equals((MapSpan)obj);
		}

		/// <summary>
		/// Creates a new <see cref="MapSpan"/> from a center location and radius.
		/// </summary>
		/// <param name="center">The center location of the span.</param>
		/// <param name="radius">The radius of the span.</param>
		/// <returns>A new <see cref="MapSpan"/> instance.</returns>
		public static MapSpan FromCenterAndRadius(Location center, Distance radius)
		{
			return new MapSpan(center, 2 * DistanceToLatitudeDegrees(radius), 2 * DistanceToLongitudeDegrees(center, radius));
		}

		/// <summary>
		/// Creates a <see cref="MapSpan"/> that encompasses all of the specified locations.
		/// </summary>
		/// <param name="locations">The locations to include in the span.</param>
		/// <returns>A new <see cref="MapSpan"/> that fits all locations, with 10% padding.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="locations"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="locations"/> is empty.</exception>
		public static MapSpan FromLocations(IEnumerable<Location> locations)
		{
			if (locations is null)
				throw new ArgumentNullException(nameof(locations));

			var locationList = locations.ToList();
			if (locationList.Count == 0)
				throw new ArgumentException("At least one location is required.", nameof(locations));

			if (locationList.Count == 1)
				return FromCenterAndRadius(locationList[0], Distance.FromKilometers(1));

			double minLat = double.MaxValue, maxLat = double.MinValue;
			double minLon = double.MaxValue, maxLon = double.MinValue;

			foreach (var loc in locationList)
			{
				minLat = Math.Min(minLat, loc.Latitude);
				maxLat = Math.Max(maxLat, loc.Latitude);
				minLon = Math.Min(minLon, loc.Longitude);
				maxLon = Math.Max(maxLon, loc.Longitude);
			}

			double centerLat = (minLat + maxLat) / 2;
			double centerLon = (minLon + maxLon) / 2;
			double latDegrees = (maxLat - minLat) * 1.1; // 10% padding
			double lonDegrees = (maxLon - minLon) * 1.1;

			// Ensure minimum span
			latDegrees = Math.Max(latDegrees, MinimumRangeDegrees);
			lonDegrees = Math.Max(lonDegrees, MinimumRangeDegrees);

			return new MapSpan(new Location(centerLat, centerLon), latDegrees, lonDegrees);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Center.GetHashCode();
				hashCode = (hashCode * 397) ^ LongitudeDegrees.GetHashCode();
				hashCode = (hashCode * 397) ^ LatitudeDegrees.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Determines whether two <see cref="MapSpan"/> instances are equal.
		/// </summary>
		/// <param name="left">The first span.</param>
		/// <param name="right">The second span.</param>
		/// <returns><see langword="true"/> if the spans are equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(MapSpan? left, MapSpan? right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Determines whether two <see cref="MapSpan"/> instances are not equal.
		/// </summary>
		/// <param name="left">The first span.</param>
		/// <param name="right">The second span.</param>
		/// <returns><see langword="true"/> if the spans are not equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(MapSpan? left, MapSpan? right)
		{
			return !Equals(left, right);
		}

		/// <summary>
		/// Creates a new <see cref="MapSpan"/> with the specified zoom factor applied.
		/// </summary>
		/// <param name="zoomFactor">The zoom factor. Values greater than 1 zoom in, values less than 1 zoom out.</param>
		/// <returns>A new <see cref="MapSpan"/> with the zoom applied.</returns>
		public MapSpan WithZoom(double zoomFactor)
		{
			double maxDLat = Math.Min(90 - Center.Latitude, 90 + Center.Latitude) * 2;
			return new MapSpan(Center, Math.Min(LatitudeDegrees / zoomFactor, maxDLat), LongitudeDegrees / zoomFactor);
		}

		static double DistanceToLatitudeDegrees(Distance distance)
		{
			return distance.Kilometers / EarthCircumferenceKm * 360;
		}

		static double DistanceToLongitudeDegrees(Location location, Distance distance)
		{
			double latCircumference = LatitudeCircumferenceKm(location);
			return distance.Kilometers / latCircumference * 360;
		}

		bool Equals(MapSpan other)
		{
			return Center.Equals(other.Center) && LongitudeDegrees.Equals(other.LongitudeDegrees) && LatitudeDegrees.Equals(other.LatitudeDegrees);
		}

		static double LatitudeCircumferenceKm(Location location)
		{
			return EarthCircumferenceKm * Math.Cos(location.Latitude * Math.PI / 180.0);
		}

		static double LatitudeDegreesToKm(double latitudeDegrees)
		{
			return EarthCircumferenceKm * latitudeDegrees / 360;
		}

		static double LongitudeDegreesToKm(Location location, double longitudeDegrees)
		{
			double latCircumference = LatitudeCircumferenceKm(location);
			return latCircumference * longitudeDegrees / 360;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Center}, {LatitudeDegrees}, {LongitudeDegrees}";
		}
	}
}