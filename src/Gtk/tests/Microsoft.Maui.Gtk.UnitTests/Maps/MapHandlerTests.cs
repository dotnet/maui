using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Gtk;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;

namespace Microsoft.Maui.Gtk.UnitTests.Maps
{
	/// <summary>An app wired to the GTK backend and the GTK map, with a stub tile server (no network) and no disk cache.</summary>
	public sealed class MapsTestEnvironment
	{
		static MapsTestEnvironment? s_shared;

		public MapsTestEnvironment(Func<string, byte[]?>? serve = null, bool mapsFirst = false)
		{
			Fetcher = new StubTileFetcher(serve ?? (_ => TestImages.CreatePng(256, 256, 40, 160, 40)));

			void Configure(GtkMapOptions options)
			{
				options.CacheDirectory = null;
				options.TileFetcher = Fetcher;
				options.UserLocationProvider = () => UserLocation;
				Options = options;
			}

			var builder = MauiApp.CreateBuilder().UseMauiApp<TestApplication>();
			if (mapsFirst)
				builder.UseMauiMapsGtk(Configure).UseMauiGtkCore();
			else
				builder.UseMauiGtkCore().UseMauiMapsGtk(Configure);

			App = builder.Build();
		}

		public static MapsTestEnvironment Shared => s_shared ??= new MapsTestEnvironment();

		public MauiApp App { get; }

		public GtkMapOptions Options { get; private set; } = null!;

		public StubTileFetcher Fetcher { get; }

		public Location? UserLocation { get; set; }

		public MauiContext NewContext() => new(App.Services);

		public MapWindowHost Host(MauiMap map, int width = 400, int height = 300) => new(this, map, width, height);
	}

	/// <summary>Hosts a map in a real GTK window through the window handler (the full measure/arrange chain).</summary>
	public sealed class MapWindowHost : IDisposable
	{
		public MapWindowHost(MapsTestEnvironment environment, MauiMap map, int width, int height)
		{
			Map = map;
			PlatformWindow = new global::Gtk.Window { Decorated = false };
			var context = environment.NewContext();
			context.AddWeakSpecific<global::Gtk.Window>(PlatformWindow);

			var window = new Microsoft.Maui.Controls.Window(new ContentPage { Content = map });
			PlatformWindow.SetWindowHandler(window, context);
			PlatformWindow.SetDefaultSize(width, height);
			PlatformWindow.Present();

			GtkTestHost.Pump(PlatformWindow, 8);
			GtkTestHost.Resize(PlatformWindow, width, height);
		}

		public MauiMap Map { get; }

		public global::Gtk.Window PlatformWindow { get; }

		public MauiGtkMapView View => (MauiGtkMapView)Map.ToPlatform();

		public void Pump(int rounds = GtkTestHost.DefaultPumpRounds) => GtkTestHost.Pump(PlatformWindow, rounds);

		public bool PumpUntil(Func<bool> condition) => GtkTestHost.PumpUntil(condition, PlatformWindow);

		public void Dispose() => GtkTestHost.Retire(PlatformWindow);
	}

	/// <summary>The GTK map handler on real widgets: registration, mappers, input, painting.</summary>
	public class MapHandlerTests
	{
		static readonly Location Paris = new(48.8566, 2.3522);

		static MauiMap NewMap(double span = 0.01) => new(new MapSpan(Paris, span, span));

		/// <summary>
		/// Renders the window content (the root panel, like the other pixel tests: a widget rendered on its own has no render node until the headless
		/// frame clock painted it) and returns the pixels with the map's offset inside it.
		/// </summary>
		static byte[] Render(MauiGtkMapView view, out int stride, out int offsetX, out int offsetY)
		{
			var window = (global::Gtk.Window)view.Root!;
			var root = window.Child!;

			for (int attempt = 0; ; attempt++)
			{
				try
				{
					var bytes = GtkTestHost.RenderToBytes(root, out int width, out _);
					var bounds = GtkTestHost.BoundsIn(view, root);
					offsetX = bounds.X;
					offsetY = bounds.Y;
					stride = width * 4;
					return bytes;
				}
				catch (InvalidOperationException) when (attempt < 100)
				{
					// A repaint was queued (a tile arrived) and the frame clock has not run yet: let real time pass, then pump.
					System.Threading.Thread.Sleep(20);
					GtkTestHost.Pump(window, 2);
				}
			}
		}

		static byte[] Render(MauiGtkMapView view) => Render(view, out _, out _, out _);

		static (byte R, byte G, byte B) Pixel(MauiGtkMapView view, double x, double y)
		{
			var bytes = Render(view, out int stride, out int offsetX, out int offsetY);
			int offset = ((offsetY + (int)Math.Round(y)) * stride) + ((offsetX + (int)Math.Round(x)) * 4);
			return (bytes[offset + 2], bytes[offset + 1], bytes[offset]);
		}

		static void AssertColor((byte R, byte G, byte B) actual, int r, int g, int b, int tolerance = 3)
		{
			Assert.True(
				Math.Abs(actual.R - r) <= tolerance && Math.Abs(actual.G - g) <= tolerance && Math.Abs(actual.B - b) <= tolerance,
				$"expected ({r},{g},{b}) but was ({actual.R},{actual.G},{actual.B})");
		}

		static MapsTestEnvironment Env => MapsTestEnvironment.Shared;

		[GtkFact]
		public void TheMapGetsTheGtkHandlerAndAMapView() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var handler = (IElementHandler)map.ToHandler(Env.NewContext());

			Assert.IsType<GtkMapHandler>(handler);
			Assert.IsAssignableFrom<MapHandler>(handler);
			Assert.IsType<MauiGtkMapView>(handler.PlatformView);
		});

		[GtkFact]
		public void TheHandlerIsSubstitutedWhateverTheRegistrationOrder() => GtkTestHost.Run(() =>
		{
			var environment = new MapsTestEnvironment(mapsFirst: true);
			var handler = (IElementHandler)NewMap().ToHandler(environment.NewContext());

			Assert.IsType<GtkMapHandler>(handler);
		});

		[GtkFact]
		public void TheEntryPointRegistersTheOptionsAndANominatimGeocoder() => GtkTestHost.Run(() =>
		{
			var services = Env.App.Services;

			Assert.Same(Env.Options, services.GetRequiredService<GtkMapOptions>());
			Assert.IsType<NominatimGeocoding>(services.GetRequiredService<IGeocoding>());
		});

		[GtkFact]
		public void MapTypeSelectsTheTileSource() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var view = (MauiGtkMapView)map.ToPlatform(Env.NewContext());
			Assert.Same(Env.Options.StreetTiles, view.TileSource);

			map.MapType = MapType.Satellite;
			Assert.Same(Env.Options.SatelliteTiles, view.TileSource);

			map.MapType = MapType.Hybrid;
			Assert.Same(Env.Options.HybridTiles, view.TileSource);
			Assert.NotNull(view.TileSource.OverlayUrlTemplate);

			map.MapType = MapType.Street;
			Assert.Same(Env.Options.StreetTiles, view.TileSource);
		});

		[GtkFact]
		public void ScrollZoomAndUserFlagsAreMapped() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var view = (MauiGtkMapView)map.ToPlatform(Env.NewContext());
			Assert.True(view.IsScrollEnabled);
			Assert.True(view.IsZoomEnabled);
			Assert.False(view.IsShowingUser);

			map.IsScrollEnabled = false;
			map.IsZoomEnabled = false;
			map.IsShowingUser = true;
			map.IsTrafficEnabled = true; // accepted and ignored

			Assert.False(view.IsScrollEnabled);
			Assert.False(view.IsZoomEnabled);
			Assert.True(view.IsShowingUser);
		});

		[GtkFact]
		public void TheUserLocationComesFromTheProvider() => GtkTestHost.Run(() =>
		{
			var environment = new MapsTestEnvironment { UserLocation = new Location(1, 2) };
			var map = NewMap();
			var view = (MauiGtkMapView)map.ToPlatform(environment.NewContext());
			Assert.Null(view.UserLocation);

			map.IsShowingUser = true;
			Assert.Equal(1, view.UserLocation!.Latitude);

			map.IsShowingUser = false;
			Assert.Null(view.UserLocation);
		});

		[GtkFact]
		public void PinsAndElementsAreMappedAndUpdated() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			map.Pins.Add(new Pin { Label = "a", Location = Paris });
			var view = (MauiGtkMapView)map.ToPlatform(Env.NewContext());
			Assert.Single(view.Pins);

			map.Pins.Add(new Pin { Label = "b", Location = new Location(1, 1) });
			Assert.Equal(2, view.Pins.Count);
			Assert.Equal("b", view.Pins[1].Label);

			map.Pins.RemoveAt(0);
			Assert.Single(view.Pins);

			map.MapElements.Add(new Circle { Center = Paris, Radius = Distance.FromMeters(50) });
			map.MapElements.Add(new Polyline());
			map.MapElements[1].StrokeColor = Colors.Red; // reaches UpdateMapElement: must not throw
		});

		[GtkFact]
		public void MoveToRegionIsDeferredUntilTheMapHasASizeThenFitsTheSpan() => GtkTestHost.Run(() =>
		{
			var map = NewMap(0.02);
			var view = (MauiGtkMapView)map.ToPlatform(Env.NewContext());

			// No allocation yet: nothing moved.
			Assert.Equal(2, view.Zoom);
			using var host = Env.Host(map, 800, 600);
			host.Pump();

			var expected = MercatorProjection.ZoomForSpan(Paris.Latitude, 0.02, 0.02, 800, 600, 19);
			Assert.Equal(expected, host.View.Zoom);
			Assert.Equal(Paris.Latitude, host.View.CenterLatitude, 6);
			Assert.Equal(Paris.Longitude, host.View.CenterLongitude, 6);
		});

		[GtkFact]
		public void MoveToRegionAfterTheMapHasASizeAppliesImmediately() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			using var host = Env.Host(map);

			map.MoveToRegion(new MapSpan(new Location(35.68, 139.69), 0.5, 0.5));
			Assert.Equal(35.68, host.View.CenterLatitude, 6);
			Assert.Equal(139.69, host.View.CenterLongitude, 6);
			Assert.Equal(MercatorProjection.ZoomForSpan(35.68, 0.5, 0.5, 400, 300, 19), host.View.Zoom);
		});

		[GtkFact]
		public void TheVisibleRegionIsReportedBackToTheMap() => GtkTestHost.Run(() =>
		{
			var map = NewMap(0.02);
			using var host = Env.Host(map, 800, 600);

			Assert.True(host.PumpUntil(() => map.VisibleRegion is not null));
			var region = map.VisibleRegion!;
			Assert.Equal(Paris.Latitude, region.Center.Latitude, 6);
			// The region is the one the widget shows: it contains the requested span.
			Assert.True(region.LongitudeDegrees >= 0.02 * 0.99, $"longitude span {region.LongitudeDegrees}");
			Assert.True(region.LatitudeDegrees >= 0.02 * 0.99, $"latitude span {region.LatitudeDegrees}");

			host.View.PanByPixels(100, 0);
			Assert.True(host.PumpUntil(() => Math.Abs(map.VisibleRegion!.Center.Longitude - Paris.Longitude) > 1e-5));
		});

		[GtkFact]
		public void TheMeasureReportsAZeroMinimumAndA256Natural() => GtkTestHost.Run(() =>
		{
			var view = new MauiGtkMapView(Env.Options);
			view.Measure(global::Gtk.Orientation.Horizontal, -1, out int minimum, out int natural, out _, out _);

			Assert.Equal(0, minimum);
			Assert.Equal(256, natural);
		});

		[GtkFact]
		public void DraggingPansTheMapAndLeavesNoClick() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			int clicks = 0;
			map.MapClicked += (_, _) => clicks++;
			using var host = Env.Host(map);
			var view = host.View;
			double longitude = view.CenterLongitude;

			view.BeginDrag(200, 150);
			view.UpdateDrag(2, 1); // below the threshold: still a click
			Assert.Equal(longitude, view.CenterLongitude);

			view.UpdateDrag(-60, 0); // the map follows the pointer: dragging left moves the centre east
			Assert.True(view.CenterLongitude > longitude);

			view.HandleClick(200, 150); // the release after a drag is not a click
			Assert.Equal(0, clicks);
		});

		[GtkFact]
		public void ADragPansByExactlyThePointerDistance() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			using var host = Env.Host(map);
			var view = host.View;
			var before = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);

			view.BeginDrag(100, 100);
			view.UpdateDrag(40, 25);

			var after = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);
			Assert.Equal(before.X + 40, after.X, 0);
			Assert.Equal(before.Y + 25, after.Y, 0);
		});

		[GtkFact]
		public void PanningCanBeDisabled() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			map.IsScrollEnabled = false;
			using var host = Env.Host(map);
			var view = host.View;
			double latitude = view.CenterLatitude;

			view.BeginDrag(100, 100);
			view.UpdateDrag(80, 80);

			Assert.Equal(latitude, view.CenterLatitude);
		});

		[GtkFact]
		public void TheDragGestureDrivesThePan() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			using var host = Env.Host(map);
			var view = host.View;
			var gesture = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(view);
			Assert.NotNull(gesture);
			double longitude = view.CenterLongitude;

			GLib.Signal.Emit(gesture!, "drag-begin", 200.0, 150.0);
			GLib.Signal.Emit(gesture!, "drag-update", -50.0, 0.0);

			Assert.True(view.CenterLongitude > longitude);
		});

		[GtkFact]
		public void TheWheelZoomsAboutThePointerAndKeepsThePositionUnderIt() => GtkTestHost.Run(() =>
		{
			var map = NewMap(0.5);
			using var host = Env.Host(map, 800, 600);
			var view = host.View;
			int zoom = view.Zoom;

			var anchor = view.Viewport.FromPixel(650, 120);
			view.ZoomAt(1, 650, 120);

			Assert.Equal(zoom + 1, view.Zoom);
			var moved = view.Viewport.ToPixel(anchor.Latitude, anchor.Longitude);
			Assert.Equal(650, moved.X, 0);
			Assert.Equal(120, moved.Y, 0);

			view.ZoomAt(-1, 650, 120);
			Assert.Equal(zoom, view.Zoom);
		});

		[GtkFact]
		public void ZoomIsLimitedByTheTileSourceAndCanBeDisabled() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			using var host = Env.Host(map);
			var view = host.View;

			view.TileSource = new GtkMapTileSource("x/{z}/{x}/{y}", null, 5);
			Assert.Equal(5, view.Zoom);
			view.ZoomAt(3, 10, 10);
			Assert.Equal(5, view.Zoom);

			view.MoveTo(Paris, 0);
			view.ZoomAt(-1, 10, 10);
			Assert.Equal(0, view.Zoom);

			view.IsZoomEnabled = false;
			view.HandleDoubleClick(10, 10);
			view.HandleWheel(1);
			Assert.Equal(0, view.Zoom);

			view.IsZoomEnabled = true;
			view.HandleDoubleClick(10, 10);
			Assert.Equal(1, view.Zoom);
		});

		[GtkFact]
		public void ClickingTheEmptyMapReportsThePosition() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			Location? clicked = null;
			map.MapClicked += (_, e) => clicked = e.Location;
			using var host = Env.Host(map);

			host.View.HandleClick(100, 80);

			Assert.NotNull(clicked);
			var expected = host.View.Viewport.FromPixel(100, 80);
			Assert.Equal(expected.Latitude, clicked!.Latitude, 9);
			Assert.Equal(expected.Longitude, clicked.Longitude, 9);
		});

		[GtkFact]
		public void ClickingAPinRaisesMarkerClickedAndOpensTheInfoWindowThatCanBeClicked() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var pin = new Pin { Label = "Eiffel Tower", Address = "Champ de Mars", Location = Paris };
			int markerClicks = 0, infoClicks = 0, mapClicks = 0;
			pin.MarkerClicked += (_, _) => markerClicks++;
			pin.InfoWindowClicked += (_, _) => infoClicks++;
			map.MapClicked += (_, _) => mapClicks++;
			map.Pins.Add(pin);
			using var host = Env.Host(map);
			var view = host.View;
			var (x, y) = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);

			view.HandleClick(x, y - 20); // the marker head

			Assert.Equal(1, markerClicks);
			Assert.Equal(0, mapClicks);
			Assert.Same(pin, view.SelectedPin);

			Render(view); // paints, which lays the info window out
			Assert.NotNull(view.InfoWindowBounds);
			var bounds = view.InfoWindowBounds!.Value;
			view.HandleClick(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			Assert.Equal(1, infoClicks);
			Assert.Equal(1, markerClicks);

			// Clicking elsewhere closes it and reports a map click.
			view.HandleClick(5, 5);
			Assert.Null(view.SelectedPin);
			Assert.Equal(1, mapClicks);
		});

		[GtkFact]
		public void AMarkerHandlerCanHideTheInfoWindow() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var pin = new Pin { Label = "x", Location = Paris };
			pin.MarkerClicked += (_, e) => e.HideInfoWindow = true;
			map.Pins.Add(pin);
			using var host = Env.Host(map);
			var (x, y) = host.View.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);

			host.View.HandleClick(x, y - 20);

			Assert.Null(host.View.SelectedPin);
		});

		[GtkFact]
		public void TheTopmostPinWinsAndRemovedPinsAreDeselected() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var below = new Pin { Label = "below", Location = Paris };
			var above = new Pin { Label = "above", Location = Paris };
			map.Pins.Add(below);
			map.Pins.Add(above);
			using var host = Env.Host(map);
			var view = host.View;
			var (x, y) = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);

			Assert.Same(above, view.PinAt(x, y - 20));
			Assert.Null(view.PinAt(x + 60, y - 20));

			view.HandleClick(x, y - 20);
			Assert.Same(above, view.SelectedPin);
			map.Pins.Remove(above);
			Assert.Null(view.SelectedPin);
		});

		[GtkFact]
		public void VisibleTilesAreRequestedFromTheConfiguredServer() => GtkTestHost.Run(() =>
		{
			var environment = new MapsTestEnvironment();
			environment.Options.StreetTiles = new GtkMapTileSource("https://tiles.example/{z}/{x}/{y}.png", "test", 19);
			var map = NewMap(0.01);
			using var host = environment.Host(map);
			host.Pump();

			Assert.True(host.PumpUntil(() => environment.Fetcher.Requests.Count > 0));
			var view = host.View;
			int tileX = (int)(MercatorProjection.LongitudeToX(Paris.Longitude, view.Zoom) / 256);
			int tileY = (int)(MercatorProjection.LatitudeToY(Paris.Latitude, view.Zoom) / 256);

			// The tile under the centre is among the requests; and the URL uses the configured template.
			Assert.Contains($"https://tiles.example/{view.Zoom}/{tileX}/{tileY}.png", environment.Fetcher.Requests);
			Assert.All(environment.Fetcher.Requests, url => Assert.StartsWith("https://tiles.example/", url));
		});

		[GtkFact]
		public void LoadedTilesArePaintedAndPlaceholdersShowUntilThen() => GtkTestHost.Run(() =>
		{
			using var gate = new System.Threading.SemaphoreSlim(0);
			var environment = new MapsTestEnvironment();
			environment.Fetcher.Gate = gate;
			var map = NewMap();
			using var host = environment.Host(map);
			host.Pump();
			var view = host.View;

			// Nothing downloaded yet: the land-coloured placeholder (234,231,224), not the tile.
			AssertColor(Pixel(view, 100, 100), 234, 231, 224, 4);

			gate.Release(100);
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			host.Pump();

			// The stub serves a flat green tile.
			AssertColor(Pixel(view, 100, 100), 40, 160, 40);
			AssertColor(Pixel(view, 200, 150), 40, 160, 40);
		});

		[GtkFact]
		public void BrokenTileBodiesAreRejectedAndKeepThePlaceholder() => GtkTestHost.Run(() =>
		{
			var environment = new MapsTestEnvironment(url => System.Text.Encoding.UTF8.GetBytes("<html>not an image</html>"));
			var map = NewMap();
			using var host = environment.Host(map);

			Assert.True(host.PumpUntil(() => environment.Fetcher.Requests.Count > 0));
			host.Pump(20);

			Assert.Equal(0, host.View.LoadedTileCount);
			AssertColor(Pixel(host.View, 100, 100), 234, 231, 224, 4);
		});

		[GtkFact]
		public void PinsAreDrawnInTheirTypeColour() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			map.Pins.Add(new Pin { Label = "p", Location = Paris, Type = PinType.Place });
			using var host = Env.Host(map);
			var view = host.View;
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			host.Pump();
			var (x, y) = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);

			// The head is a disc of radius 8 centred 20 px above the position, with a white dot in the middle.
			AssertColor(Pixel(view, x + 5, y - 20), 33, 107, 204, 6); // PinType.Place blue
			AssertColor(Pixel(view, x, y - 20), 255, 255, 255, 6);
			AssertColor(Pixel(view, x + 40, y - 20), 40, 160, 40); // the tile around it
		});

		[GtkFact]
		public void PolygonsPolylinesAndCirclesAreDrawn() => GtkTestHost.Run(() =>
		{
			var map = NewMap(0.004);
			var polygon = new Polygon { FillColor = Color.FromRgb(255, 0, 0), StrokeColor = Color.FromRgb(0, 0, 255), StrokeWidth = 4 };
			polygon.Geopath.Add(new Location(Paris.Latitude - 0.0005, Paris.Longitude - 0.0005));
			polygon.Geopath.Add(new Location(Paris.Latitude - 0.0005, Paris.Longitude + 0.0005));
			polygon.Geopath.Add(new Location(Paris.Latitude + 0.0005, Paris.Longitude + 0.0005));
			polygon.Geopath.Add(new Location(Paris.Latitude + 0.0005, Paris.Longitude - 0.0005));

			var line = new Polyline { StrokeColor = Color.FromRgb(255, 255, 0), StrokeWidth = 6 };
			line.Geopath.Add(new Location(Paris.Latitude - 0.0015, Paris.Longitude - 0.0020));
			line.Geopath.Add(new Location(Paris.Latitude - 0.0015, Paris.Longitude + 0.0020));

			var circle = new Circle { Center = new Location(Paris.Latitude + 0.0015, Paris.Longitude), Radius = Distance.FromMeters(30), FillColor = Color.FromRgb(255, 0, 255), StrokeColor = Color.FromRgb(0, 0, 0), StrokeWidth = 2 };

			map.MapElements.Add(polygon);
			map.MapElements.Add(line);
			map.MapElements.Add(circle);
			using var host = Env.Host(map, 400, 300);
			var view = host.View;
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			host.Pump();
			var viewport = view.Viewport;

			var centre = viewport.ToPixel(Paris.Latitude, Paris.Longitude);
			AssertColor(Pixel(view, centre.X, centre.Y), 255, 0, 0);

			var onLine = viewport.ToPixel(Paris.Latitude - 0.0015, Paris.Longitude);
			AssertColor(Pixel(view, onLine.X, onLine.Y), 255, 255, 0);

			var inCircle = viewport.ToPixel(circle.Center.Latitude, circle.Center.Longitude);
			AssertColor(Pixel(view, inCircle.X, inCircle.Y), 255, 0, 255);

			// Outside every shape the tile shows.
			AssertColor(Pixel(view, 10, 10), 40, 160, 40);

			// The circle radius is metres, not pixels: 30 m at this zoom.
			double radius = 30 / MercatorProjection.MetersPerPixel(circle.Center.Latitude, view.Zoom);
			AssertColor(Pixel(view, inCircle.X + radius * 0.5, inCircle.Y), 255, 0, 255);
			AssertColor(Pixel(view, inCircle.X + radius + 8, inCircle.Y), 40, 160, 40);
		});

		[GtkFact]
		public void ChangingAnElementRepaintsIt() => GtkTestHost.Run(() =>
		{
			var map = NewMap(0.004);
			var circle = new Circle { Center = Paris, Radius = Distance.FromMeters(30), FillColor = Color.FromRgb(255, 0, 0), StrokeColor = Color.FromRgb(255, 0, 0) };
			map.MapElements.Add(circle);
			using var host = Env.Host(map);
			var view = host.View;
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			host.Pump();
			var centre = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);
			AssertColor(Pixel(view, centre.X, centre.Y), 255, 0, 0);

			circle.FillColor = Color.FromRgb(0, 0, 255);
			circle.StrokeColor = Color.FromRgb(0, 0, 255);
			host.Pump();

			AssertColor(Pixel(view, centre.X, centre.Y), 0, 0, 255);
		});

		[GtkFact]
		public void TheUserMarkerIsDrawnWhenRequested() => GtkTestHost.Run(() =>
		{
			var environment = new MapsTestEnvironment { UserLocation = Paris };
			var map = NewMap();
			map.IsShowingUser = true;
			using var host = environment.Host(map);
			var view = host.View;
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			host.Pump();
			var centre = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);

			AssertColor(Pixel(view, centre.X, centre.Y), 26, 102, 217, 8);
		});

		[GtkFact]
		public void TheAttributionIsPaintedInTheLowerRightCorner() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			using var host = Env.Host(map);
			var view = host.View;
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			host.Pump();

			// A translucent white box over the green tile: lighter than the tile.
			var corner = Pixel(view, view.Width - 3, view.Height - 3);
			Assert.True(corner.R > 100 && corner.G > 190, $"({corner.R},{corner.G},{corner.B})");
			AssertColor(Pixel(view, view.Width - 200, view.Height - 3), 40, 160, 40);
		});

		[GtkFact]
		public void DrawingAndInputProduceNoGtkComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var map = NewMap();
			map.Pins.Add(new Pin { Label = "a", Address = "b", Location = Paris });
			using var host = Env.Host(map);
			var view = host.View;
			Assert.True(host.PumpUntil(() => view.LoadedTileCount > 0));
			var (x, y) = view.Viewport.ToPixel(Paris.Latitude, Paris.Longitude);
			view.HandleClick(x, y - 20);
			view.BeginDrag(10, 10);
			view.UpdateDrag(30, 30);
			view.ZoomAt(2, 100, 100);
			host.Pump(10);
			Render(view);

			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void DisconnectingStopsTheViewAndIsSafeToRepeat() => GtkTestHost.Run(() =>
		{
			var map = NewMap();
			var handler = (IElementHandler)map.ToHandler(Env.NewContext());
			var view = (MauiGtkMapView)handler.PlatformView!;

			handler.DisconnectHandler();

			Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(view));
			Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(view));
			view.Detach();
			// Late tile completions are ignored.
			view.Invalidate();
		});

		[GtkFact]
		public void TwoMapsShareTheTileStoreButNotTheViewState() => GtkTestHost.Run(() =>
		{
			var first = NewMap();
			var second = NewMap();
			var a = (MauiGtkMapView)first.ToPlatform(Env.NewContext());
			var b = (MauiGtkMapView)second.ToPlatform(Env.NewContext());

			first.MapType = MapType.Satellite;
			first.Pins.Add(new Pin { Label = "x", Location = Paris });

			Assert.Same(Env.Options.StreetTiles, b.TileSource);
			Assert.Empty(b.Pins);
			Assert.NotSame(a, b);
		});
	}
}
