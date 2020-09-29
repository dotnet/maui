using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Maps
{
	public static class GeographyUtils
	{
		internal const double EarthRadiusKm = 6371;

		public static double ToRadians(this double degrees)
		{
			return degrees * Math.PI / 180.0;
		}

		public static double ToDegrees(this double radians)
		{
			return radians / Math.PI * 180.0;
		}

		public static List<Position> ToCircumferencePositions(this Circle circle)
		{
			var positions = new List<Position>();
			double centerLatitude = circle.Center.Latitude.ToRadians();
			double centerLongitude = circle.Center.Longitude.ToRadians();
			double distance = circle.Radius.Kilometers / GeographyUtils.EarthRadiusKm;

			for (int angle = 0; angle <= 360; angle++)
			{
				double angleInRadians = ((double)angle).ToRadians();
				double latitude = Math.Asin(Math.Sin(centerLatitude) * Math.Cos(distance) +
											Math.Cos(centerLatitude) * Math.Sin(distance) * Math.Cos(angleInRadians));
				double longitude = centerLongitude +
								   Math.Atan2(Math.Sin(angleInRadians) * Math.Sin(distance) * Math.Cos(centerLatitude),
									   Math.Cos(distance) - Math.Sin(centerLatitude) * Math.Sin(latitude));

				positions.Add(new Position(latitude.ToDegrees(), longitude.ToDegrees()));
			}

			return positions;
		}
	}
}