using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	public sealed class MapArea
	{
		const double EarthRadiusKm = 6371;
		const double EarthCircumferenceKm = EarthRadiusKm * 2 * Math.PI;
		const double MinimumRangeDegrees = 0.001 / EarthCircumferenceKm * 360; // 1 meter

		public MapArea(Location center, double latitudeDegrees, double longitudeDegrees)
		{
			Center = center;
			LatitudeDegrees = Math.Min(Math.Max(latitudeDegrees, MinimumRangeDegrees), 90.0);
			LongitudeDegrees = Math.Min(Math.Max(longitudeDegrees, MinimumRangeDegrees), 180.0);
		}

		public Location Center { get; }

		public double LatitudeDegrees { get; }

		public double LongitudeDegrees { get; }
	}
}