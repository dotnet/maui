using System;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// Spherical Web-Mercator (EPSG:3857) in the "slippy map" pixel convention every XYZ tile server uses: the world is a
	/// square of <c>256 * 2^zoom</c> pixels with (0,0) at the north-west corner (latitude +85.05, longitude -180).
	/// </summary>
	internal static class MercatorProjection
	{
		/// <summary>Edge length of a tile in pixels.</summary>
		public const int TileSize = 256;

		/// <summary>The latitude where Mercator becomes square; every tile server clamps here.</summary>
		public const double MaxLatitude = 85.05112877980659;

		public const int MinZoomLevel = 0;
		public const int MaxZoomLevel = 19;

		// Metres per pixel at the equator, zoom 0.
		const double EquatorMetersPerPixel = 156543.033928041;

		public static double WorldSize(int zoom) => TileSize * Math.Pow(2.0, zoom);

		public static int TileCount(int zoom) => 1 << zoom;

		public static double ClampLatitude(double latitude) => Math.Max(-MaxLatitude, Math.Min(MaxLatitude, latitude));

		/// <summary>Wraps a longitude into [-180, 180).</summary>
		public static double NormalizeLongitude(double longitude)
		{
			var normalized = (longitude + 180.0) % 360.0;
			if (normalized < 0.0)
				normalized += 360.0;

			return normalized - 180.0;
		}

		public static double LongitudeToX(double longitude, int zoom) =>
			(NormalizeLongitude(longitude) + 180.0) / 360.0 * WorldSize(zoom);

		public static double LatitudeToY(double latitude, int zoom)
		{
			var radians = ClampLatitude(latitude) * Math.PI / 180.0;
			var mercator = Math.Log(Math.Tan(radians) + 1.0 / Math.Cos(radians));

			return (1.0 - mercator / Math.PI) / 2.0 * WorldSize(zoom);
		}

		public static double XToLongitude(double x, int zoom) => x / WorldSize(zoom) * 360.0 - 180.0;

		public static double YToLatitude(double y, int zoom)
		{
			var n = Math.PI - 2.0 * Math.PI * y / WorldSize(zoom);

			return 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
		}

		public static double MetersPerPixel(double latitude, int zoom) =>
			EquatorMetersPerPixel * Math.Cos(ClampLatitude(latitude) * Math.PI / 180.0) / Math.Pow(2.0, zoom);

		/// <summary>The largest zoom (up to <paramref name="maxZoom"/>) at which the span still fits the viewport.</summary>
		public static int ZoomForSpan(double centerLatitude, double latitudeDegrees, double longitudeDegrees, int widthPixels, int heightPixels, int maxZoom)
		{
			if (widthPixels <= 0 || heightPixels <= 0)
				return MinZoomLevel;

			var north = ClampLatitude(centerLatitude + latitudeDegrees / 2.0);
			var south = ClampLatitude(centerLatitude - latitudeDegrees / 2.0);

			for (var zoom = Math.Min(maxZoom, MaxZoomLevel); zoom > MinZoomLevel; zoom--)
			{
				var spanHeight = Math.Abs(LatitudeToY(south, zoom) - LatitudeToY(north, zoom));
				var spanWidth = Math.Min(longitudeDegrees, 360.0) / 360.0 * WorldSize(zoom);

				if (spanWidth <= widthPixels && spanHeight <= heightPixels)
					return zoom;
			}

			return MinZoomLevel;
		}
	}
}
