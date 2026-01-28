using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Provides geography-related utility methods.
	/// </summary>
	public static class GeographyUtils
	{
		internal const double EarthRadiusKm = 6371;

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		/// <param name="degrees">The value in degrees.</param>
		/// <returns>The value in radians.</returns>
		public static double ToRadians(this double degrees) => degrees * Math.PI / 180.0;

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="radians">The value in radians.</param>
		/// <returns>The value in degrees.</returns>
		public static double ToDegrees(this double radians) => radians / Math.PI * 180.0;

		/// <summary>
		/// Calculates the circumference positions that form the boundary of a circle on the map.
		/// </summary>
		/// <param name="circle">The circle element to calculate positions for.</param>
		/// <returns>A list of positions that approximate the circle's circumference.</returns>
		public static List<Location> ToCircumferencePositions(this ICircleMapElement circle)
		{
			var positions = new List<Location>();
			double centerLatitude = circle.Center.Latitude.ToRadians();
			double centerLongitude = circle.Center.Longitude.ToRadians();
			double distance = circle.Radius.Kilometers / EarthRadiusKm;

			for (int angle = 0; angle <= 360; angle++)
			{
				double angleInRadians = ((double)angle).ToRadians();
				double latitude = Math.Asin(Math.Sin(centerLatitude) * Math.Cos(distance) +
											Math.Cos(centerLatitude) * Math.Sin(distance) * Math.Cos(angleInRadians));
				double longitude = centerLongitude +
								   Math.Atan2(Math.Sin(angleInRadians) * Math.Sin(distance) * Math.Cos(centerLatitude),
									   Math.Cos(distance) - Math.Sin(centerLatitude) * Math.Sin(latitude));

				positions.Add(new Location(latitude.ToDegrees(), longitude.ToDegrees()));
			}

			return positions;
		}
	}
}
