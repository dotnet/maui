using System;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Gtk;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Maps
{
	/// <summary>Projection, viewport and tile-source math: no GTK needed.</summary>
	public class MapMathTests
	{
		[Fact]
		public void WorldSizeDoublesPerZoomLevel()
		{
			Assert.Equal(256, MercatorProjection.WorldSize(0));
			Assert.Equal(512, MercatorProjection.WorldSize(1));
			Assert.Equal(256 * 1024, MercatorProjection.WorldSize(10));
			Assert.Equal(1 << 12, MercatorProjection.TileCount(12));
		}

		[Fact]
		public void TheEquatorAndGreenwichAreTheCentreOfTheWorld()
		{
			Assert.Equal(128, MercatorProjection.LongitudeToX(0, 0), 6);
			Assert.Equal(128, MercatorProjection.LatitudeToY(0, 0), 6);
			Assert.Equal(1 << 18, MercatorProjection.LongitudeToX(0, 11), 6);
		}

		[Theory]
		[InlineData(-180, 0)]
		[InlineData(180, 0)]
		[InlineData(-90, 64)]
		[InlineData(90, 192)]
		public void LongitudeMapsLinearly(double longitude, double expectedX)
		{
			// +180 wraps onto -180: the antimeridian is the west edge.
			Assert.Equal(expectedX, MercatorProjection.LongitudeToX(longitude, 0), 6);
		}

		[Fact]
		public void TheMercatorLimitIsTheNorthWestCornerOfTheWorld()
		{
			Assert.Equal(0, MercatorProjection.LatitudeToY(MercatorProjection.MaxLatitude, 0), 4);
			Assert.Equal(256, MercatorProjection.LatitudeToY(-MercatorProjection.MaxLatitude, 0), 4);
			// Beyond the limit is clamped, not infinite.
			Assert.Equal(0, MercatorProjection.LatitudeToY(89.9, 0), 4);
		}

		[Fact]
		public void KnownTileOfTheOsmDocumentation()
		{
			// OSM wiki: lat 51.5074, lon -0.1278 (London) at zoom 10 is tile 511/340.
			Assert.Equal(511, (int)(MercatorProjection.LongitudeToX(-0.1278, 10) / 256));
			Assert.Equal(340, (int)(MercatorProjection.LatitudeToY(51.5074, 10) / 256));
		}

		[Theory]
		[InlineData(0, 0, 3)]
		[InlineData(48.8566, 2.3522, 12)]
		[InlineData(-33.8688, 151.2093, 17)]
		[InlineData(70, -150, 6)]
		public void ProjectionRoundTrips(double latitude, double longitude, int zoom)
		{
			double x = MercatorProjection.LongitudeToX(longitude, zoom);
			double y = MercatorProjection.LatitudeToY(latitude, zoom);

			Assert.Equal(longitude, MercatorProjection.XToLongitude(x, zoom), 7);
			Assert.Equal(latitude, MercatorProjection.YToLatitude(y, zoom), 7);
		}

		[Theory]
		[InlineData(190, -170)]
		[InlineData(-190, 170)]
		[InlineData(540, -180)]
		[InlineData(0, 0)]
		[InlineData(179.5, 179.5)]
		public void LongitudeIsNormalised(double input, double expected) =>
			Assert.Equal(expected, MercatorProjection.NormalizeLongitude(input), 9);

		[Fact]
		public void MetersPerPixelMatchesTheStandardTable()
		{
			// Equator, zoom 0: 156543.03 m/px; it halves per zoom and shrinks with cos(latitude).
			Assert.Equal(156543.033928041, MercatorProjection.MetersPerPixel(0, 0), 3);
			Assert.Equal(78271.516964, MercatorProjection.MetersPerPixel(0, 1), 3);
			Assert.Equal(156543.033928041 * Math.Cos(60 * Math.PI / 180), MercatorProjection.MetersPerPixel(60, 0), 3);
		}

		[Fact]
		public void ZoomForSpanPicksTheLargestZoomThatFits()
		{
			// The whole world (360 degrees) in 256 px is zoom 0; in 1024 px it is zoom 2.
			Assert.Equal(0, MercatorProjection.ZoomForSpan(0, 170, 360, 256, 256, 19));
			Assert.Equal(2, MercatorProjection.ZoomForSpan(0, 10, 360, 1024, 1024, 19));
			// A tiny span is limited by the tile source.
			Assert.Equal(15, MercatorProjection.ZoomForSpan(40, 0.0001, 0.0001, 800, 600, 15));
			Assert.Equal(0, MercatorProjection.ZoomForSpan(40, 1, 1, 0, 600, 19));
		}

		[Fact]
		public void ZoomForSpanActuallyFitsTheSpan()
		{
			const int width = 800, height = 600;
			int zoom = MercatorProjection.ZoomForSpan(48.85, 0.2, 0.3, width, height, 19);

			double spanWidth = 0.3 / 360.0 * MercatorProjection.WorldSize(zoom);
			double spanHeight = Math.Abs(MercatorProjection.LatitudeToY(48.75, zoom) - MercatorProjection.LatitudeToY(48.95, zoom));
			Assert.True(spanWidth <= width && spanHeight <= height);

			// One level closer no longer fits.
			double biggerWidth = 0.3 / 360.0 * MercatorProjection.WorldSize(zoom + 1);
			double biggerHeight = Math.Abs(MercatorProjection.LatitudeToY(48.75, zoom + 1) - MercatorProjection.LatitudeToY(48.95, zoom + 1));
			Assert.True(biggerWidth > width || biggerHeight > height);
		}

		[Fact]
		public void TheCentreIsInTheMiddleOfTheViewport()
		{
			var viewport = new MapViewport(10, 20, 8, 400, 300);
			var (x, y) = viewport.ToPixel(10, 20);

			// The origin is rounded to whole pixels, so the centre is within half a pixel of the middle.
			Assert.InRange(x, 199.5, 200.5);
			Assert.InRange(y, 149.5, 150.5);
		}

		[Theory]
		[InlineData(10, 20, 8, 5, 5)]
		[InlineData(-45, 100, 12, 399, 299)]
		[InlineData(60, -10, 3, 200, 150)]
		public void PixelsAndPositionsRoundTrip(double latitude, double longitude, int zoom, double px, double py)
		{
			var viewport = new MapViewport(latitude, longitude, zoom, 400, 300);
			var (lat, lon) = viewport.FromPixel(px, py);
			var (x, y) = viewport.ToPixel(lat, lon);

			Assert.Equal(px, x, 4);
			Assert.Equal(py, y, 4);
		}

		[Fact]
		public void PositionsWrapAroundTheAntimeridian()
		{
			var viewport = new MapViewport(0, 179.9, 3, 400, 300);

			// -179.9 is 0.2 degrees east of 179.9, i.e. a few pixels to the right of the centre, not a whole world to the left.
			var (x, _) = viewport.ToPixel(0, -179.9);
			Assert.InRange(x, 200, 220);
		}

		[Fact]
		public void TheVisibleRegionMatchesTheViewportSize()
		{
			var viewport = new MapViewport(0, 0, 2, 1024, 512);
			var region = viewport.VisibleRegion;

			// Zoom 2 is 1024 px around the world (MapSpan caps a span at 180 degrees of longitude).
			Assert.Equal(180, region.LongitudeDegrees, 6);
			Assert.Equal(0, region.Center.Latitude);
			Assert.InRange(region.LatitudeDegrees, 80, 100);
		}

		[Fact]
		public void TileUrlsSubstituteAllPlaceholders()
		{
			Assert.Equal("https://t.example/5/17/11.png", GtkMapTileSource.FormatUrl("https://t.example/{z}/{x}/{y}.png", 5, 17, 11));
			// Esri orders y before x.
			Assert.Equal("https://e.example/5/11/17", GtkMapTileSource.FormatUrl("https://e.example/{z}/{y}/{x}", 5, 17, 11));
		}

		[Fact]
		public void TileSourceValidatesAndClampsItsInput()
		{
			Assert.Throws<ArgumentException>(() => new GtkMapTileSource("", "x"));
			Assert.Equal(19, new GtkMapTileSource("u/{z}", null, 99).MaxZoom);
			Assert.Equal(0, new GtkMapTileSource("u/{z}", null, -4).MaxZoom);
			Assert.Equal(string.Empty, new GtkMapTileSource("u/{z}", null).Attribution);
			Assert.Null(new GtkMapTileSource("u/{z}", "a", 5, "").OverlayUrlTemplate);
		}

		[Fact]
		public void DefaultSourcesNameTheirProviderAndOnlyHybridHasAnOverlay()
		{
			Assert.Contains("OpenStreetMap", GtkMapTileSource.OpenStreetMap.Attribution);
			Assert.Contains("Esri", GtkMapTileSource.EsriSatellite.Attribution);
			Assert.Null(GtkMapTileSource.OpenStreetMap.OverlayUrlTemplate);
			Assert.Null(GtkMapTileSource.EsriSatellite.OverlayUrlTemplate);
			Assert.NotNull(GtkMapTileSource.EsriHybrid.OverlayUrlTemplate);
		}

		[Fact]
		public void OptionsPickTheSourceOfTheMapType()
		{
			var street = new GtkMapTileSource("s/{z}/{x}/{y}", null);
			var satellite = new GtkMapTileSource("a/{z}/{x}/{y}", null);
			var hybrid = new GtkMapTileSource("h/{z}/{x}/{y}", null);
			var options = new GtkMapOptions { StreetTiles = street, SatelliteTiles = satellite, HybridTiles = hybrid };

			Assert.Same(street, options.GetTileSource(MapType.Street));
			Assert.Same(satellite, options.GetTileSource(MapType.Satellite));
			Assert.Same(hybrid, options.GetTileSource(MapType.Hybrid));
		}

		[Fact]
		public void DefaultOptionsIdentifyTheApplication()
		{
			var options = new GtkMapOptions();

			Assert.False(string.IsNullOrWhiteSpace(options.UserAgent));
			Assert.Contains("Microsoft.Maui.Controls.Maps.Gtk", options.UserAgent);
			Assert.Equal("https://nominatim.openstreetmap.org", options.NominatimUrl);
			Assert.True(options.CacheMaxAge >= TimeSpan.FromDays(7));
			Assert.EndsWith("map-tiles", options.CacheDirectory);
		}

		[Fact]
		public void LruCacheEvictsTheLeastRecentlyUsed()
		{
			var cache = new LruCache<string, int>(2);
			cache.Set("a", 1);
			cache.Set("b", 2);
			Assert.True(cache.TryGet("a", out _)); // a is now the most recent
			cache.Set("c", 3);

			Assert.False(cache.TryGet("b", out _));
			Assert.True(cache.TryGet("a", out var a));
			Assert.Equal(1, a);
			Assert.True(cache.TryGet("c", out _));
			Assert.Equal(2, cache.Count);

			cache.Set("a", 10);
			Assert.True(cache.TryGet("a", out a));
			Assert.Equal(10, a);
			Assert.Equal(2, cache.Count);

			cache.Clear();
			Assert.Equal(0, cache.Count);
			Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<string, int>(0));
		}
	}
}
