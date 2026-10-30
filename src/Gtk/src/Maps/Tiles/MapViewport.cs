using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// The pure geometry of the map view: a centre, a zoom level and a pixel size. Converts between geographic positions and
	/// widget pixels (the same rounded origin the view paints with, so hit testing matches what is drawn).
	/// </summary>
	internal readonly struct MapViewport
	{
		public MapViewport(double centerLatitude, double centerLongitude, int zoom, int width, int height)
		{
			CenterLatitude = centerLatitude;
			CenterLongitude = centerLongitude;
			Zoom = zoom;
			Width = Math.Max(1, width);
			Height = Math.Max(1, height);
		}

		public double CenterLatitude { get; }
		public double CenterLongitude { get; }
		public int Zoom { get; }
		public int Width { get; }
		public int Height { get; }

		/// <summary>World pixel x of the widget's left edge (rounded to whole pixels so tiles stay sharp).</summary>
		public double OriginX => Math.Round(MercatorProjection.LongitudeToX(CenterLongitude, Zoom) - Width / 2.0);

		/// <summary>World pixel y of the widget's top edge.</summary>
		public double OriginY => Math.Round(MercatorProjection.LatitudeToY(CenterLatitude, Zoom) - Height / 2.0);

		/// <summary>Widget pixel of a position. The longitude copy nearest the centre is used so the world wraps around.</summary>
		public (double X, double Y) ToPixel(double latitude, double longitude)
		{
			double world = MercatorProjection.WorldSize(Zoom);
			double centerX = MercatorProjection.LongitudeToX(CenterLongitude, Zoom);
			double x = MercatorProjection.LongitudeToX(longitude, Zoom);

			double delta = x - centerX;
			if (delta > world / 2)
				x -= world;
			else if (delta < -world / 2)
				x += world;

			return (x - OriginX, MercatorProjection.LatitudeToY(latitude, Zoom) - OriginY);
		}

		/// <summary>The position under a widget pixel.</summary>
		public (double Latitude, double Longitude) FromPixel(double px, double py) =>
			(MercatorProjection.YToLatitude(OriginY + py, Zoom),
			 MercatorProjection.NormalizeLongitude(MercatorProjection.XToLongitude(OriginX + px, Zoom)));

		/// <summary>The region currently shown.</summary>
		public MapSpan VisibleRegion
		{
			get
			{
				double north = MercatorProjection.YToLatitude(OriginY, Zoom);
				double south = MercatorProjection.YToLatitude(OriginY + Height, Zoom);
				double longitudeDegrees = Math.Min(360.0, Width / MercatorProjection.WorldSize(Zoom) * 360.0);

				return new MapSpan(new Location(CenterLatitude, CenterLongitude), Math.Abs(north - south), longitudeDegrees);
			}
		}
	}
}
