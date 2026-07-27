#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Xunit;
using ACircle = Android.Gms.Maps.Model.Circle;
using APolygon = Android.Gms.Maps.Model.Polygon;
using APolyline = Android.Gms.Maps.Model.Polyline;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public class MapElementOptionsLifecycleTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task PolylineHandlerIsNotRetainedAfterAdd()
		{
			var polyline = CreatePolyline();

			await AddMapElementAndWaitForNativeIdAsync(polyline);
		}

		[Fact]
		public async Task PolygonHandlerIsNotRetainedAfterAdd()
		{
			var polygon = CreatePolygon();

			await AddMapElementAndWaitForNativeIdAsync(polygon);
		}

		[Fact]
		public async Task CircleHandlerIsNotRetainedAfterAdd()
		{
			var circle = CreateCircle();

			await AddMapElementAndWaitForNativeIdAsync(circle);
		}

		[Fact]
		public async Task MutatingPolylineAfterAddDoesNotKeepTemporaryHandler()
		{
			var polyline = CreatePolyline();

			await AddMapElementAndWaitForNativeIdAsync(polyline);

			await InvokeOnMainThreadAsync(() => polyline.Add(new Location(47.6398, -122.1279)));

			await AssertNoTemporaryHandlerAsync(polyline);
		}

		[Fact]
		public async Task MutatingPolylineAfterAddUpdatesNativePolyline()
		{
			var polyline = CreatePolyline();

			var mapHandler = await AddMapElementAndWaitForNativeIdAsync(polyline);

			await InvokeOnMainThreadAsync(() =>
			{
				polyline.StrokeWidth = 11;
				polyline.Add(new Location(47.6398, -122.1279));
			});

			await AssertEventually(
				() => InvokeOnMainThreadAsync(() =>
				{
					var nativePolyline = mapHandler.GetPlatformPolyline(polyline);
					return nativePolyline is not null &&
						nativePolyline.Points.Count == 4 &&
						AreClose(nativePolyline.Width, 11);
				}),
				timeout: 5000,
				interval: 100,
				message: "Timed out waiting for native polyline updates.");
		}

		[Fact]
		public async Task MutatingPolygonAfterAddUpdatesNativePolygon()
		{
			var polygon = CreatePolygon();
			var expectedPoint = new Location(47.6508, -122.1269);

			var mapHandler = await AddMapElementAndWaitForNativeIdAsync(polygon);

			await InvokeOnMainThreadAsync(() =>
			{
				polygon.StrokeWidth = 9;
				polygon.Add(expectedPoint);
			});

			await AssertEventually(
				() => InvokeOnMainThreadAsync(() =>
				{
					var nativePolygon = mapHandler.GetPlatformPolygon(polygon);
					return nativePolygon is not null &&
						nativePolygon.Points.Count >= 5 &&
						nativePolygon.Points.Any(point =>
							AreClose(point.Latitude, expectedPoint.Latitude) &&
							AreClose(point.Longitude, expectedPoint.Longitude)) &&
						AreClose(nativePolygon.StrokeWidth, 9);
				}),
				timeout: 5000,
				interval: 100,
				message: "Timed out waiting for native polygon updates.");
		}

		[Fact]
		public async Task MutatingCircleAfterAddUpdatesNativeCircle()
		{
			var circle = CreateCircle();
			var expectedCenter = new Location(47.6420, -122.1320);

			var mapHandler = await AddMapElementAndWaitForNativeIdAsync(circle);

			await InvokeOnMainThreadAsync(() =>
			{
				circle.Center = expectedCenter;
				circle.Radius = Distance.FromMeters(750);
				circle.StrokeWidth = 6;
			});

			await AssertEventually(
				() => InvokeOnMainThreadAsync(() =>
				{
					var nativeCircle = mapHandler.GetPlatformCircle(circle);
					var nativeCenter = nativeCircle?.Center;

					return nativeCircle is not null &&
						nativeCenter is not null &&
						AreClose(nativeCenter.Latitude, expectedCenter.Latitude) &&
						AreClose(nativeCenter.Longitude, expectedCenter.Longitude) &&
						AreClose(nativeCircle.Radius, 750) &&
						AreClose(nativeCircle.StrokeWidth, 6);
				}),
				timeout: 5000,
				interval: 100,
				message: "Timed out waiting for native circle updates.");
		}

		async Task<TestMapHandler> AddMapElementAndWaitForNativeIdAsync(MapElement mapElement)
		{
			var map = new Map();
			await InvokeOnMainThreadAsync(() => map.MapElements.Add(mapElement));

			var mapHandler = await CreateHandlerAsync<TestMapHandler>(map);

			await AssertEventually(
				() => InvokeOnMainThreadAsync(() => mapHandler.Map is not null),
				timeout: 10000,
				interval: 100,
				message: "Timed out waiting for the native GoogleMap.");

			await AssertEventually(
				() => InvokeOnMainThreadAsync(() => mapElement.MapElementId is not null),
				timeout: 10000,
				interval: 100,
				message: "Timed out waiting for the map element to be added to the native map.");

			await AssertNoTemporaryHandlerAsync(mapElement);

			return mapHandler;
		}

		async Task AssertNoTemporaryHandlerAsync(MapElement mapElement)
		{
			await AssertEventually(
				() => InvokeOnMainThreadAsync(() => mapElement.Handler is null),
				timeout: 5000,
				interval: 100,
				message: "The temporary MapElementHandler should not remain attached after adding the element.");
		}

		static Polyline CreatePolyline()
		{
			var polyline = new Polyline
			{
				StrokeColor = Colors.Red,
				StrokeWidth = 7
			};

			polyline.Add(new Location(47.6358, -122.1319));
			polyline.Add(new Location(47.6378, -122.1299));
			polyline.Add(new Location(47.6388, -122.1289));

			return polyline;
		}

		static Polygon CreatePolygon()
		{
			var polygon = new Polygon
			{
				StrokeColor = Colors.Blue,
				StrokeWidth = 4,
				FillColor = Colors.LightBlue
			};

			polygon.Add(new Location(47.6458, -122.1419));
			polygon.Add(new Location(47.6458, -122.1119));
			polygon.Add(new Location(47.6558, -122.1119));
			polygon.Add(new Location(47.6558, -122.1419));

			return polygon;
		}

		static Circle CreateCircle()
		{
			return new Circle
			{
				Center = new Location(47.6400, -122.1300),
				Radius = Distance.FromMeters(500),
				StrokeColor = Colors.Green,
				StrokeWidth = 3,
				FillColor = Colors.LightGreen
			};
		}

		static bool AreClose(double actual, double expected)
		{
			return Math.Abs(actual - expected) < 0.0001;
		}

		class TestMapHandler : MapHandler
		{
			public APolyline? GetPlatformPolyline(Polyline polyline) => GetNativePolyline(polyline);

			public APolygon? GetPlatformPolygon(Polygon polygon) => GetNativePolygon(polygon);

			public ACircle? GetPlatformCircle(Circle circle) => GetNativeCircle(circle);
		}
	}
}
