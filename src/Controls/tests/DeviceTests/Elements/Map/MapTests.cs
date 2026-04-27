using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	// Android requires a Google Maps API key to instantiate MapView,
	// which is not available in the test environment.
#if IOS || MACCATALYST
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		static Polygon CreatePolygon() => new()
		{
			Geopath =
			{
				new Location(47.6458, -122.1419),
				new Location(47.6458, -122.1119),
				new Location(47.6558, -122.1119),
				new Location(47.6558, -122.1419)
			}
		};

		// Regression test for https://github.com/dotnet/maui/issues/30097
		[Fact]
		public async Task ClearMapElementsResetsMapElementId()
		{
			var map = new Map();
			await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var polygon1 = CreatePolygon();
			var polygon2 = CreatePolygon();

			// Multiple add/clear cycles reproduce the original issue
			for (int cycle = 0; cycle < 3; cycle++)
			{
				await InvokeOnMainThreadAsync(() =>
				{
					map.MapElements.Add(polygon1);
					map.MapElements.Add(polygon2);
				});

				Assert.Equal(2, map.MapElements.Count);

				await InvokeOnMainThreadAsync(() => map.MapElements.Clear());

				Assert.Empty(map.MapElements);
				Assert.Null(polygon1.MapElementId);
				Assert.Null(polygon2.MapElementId);
			}
		}

		[Fact]
		public async Task ClearResetsMapElementIdForAllElementTypes()
		{
			var map = new Map();
			await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var polygon = CreatePolygon();

			var polyline = new Polyline
			{
				Geopath =
				{
					new Location(47.6358, -122.1319),
					new Location(47.6378, -122.1299),
					new Location(47.6398, -122.1279)
				}
			};

			var circle = new Circle
			{
				Center = new Location(47.6400, -122.1300),
				Radius = Distance.FromMeters(500)
			};

			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Add(polygon);
				map.MapElements.Add(polyline);
				map.MapElements.Add(circle);
			});

			Assert.Equal(3, map.MapElements.Count);

			await InvokeOnMainThreadAsync(() => map.MapElements.Clear());

			Assert.Empty(map.MapElements);
			Assert.Null(polygon.MapElementId);
			Assert.Null(polyline.MapElementId);
			Assert.Null(circle.MapElementId);
		}

		[Fact]
		public async Task RemoveSingleElementPreservesOtherMapElementIds()
		{
			var map = new Map();
			await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var polygon1 = CreatePolygon();
			var polygon2 = CreatePolygon();

			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Add(polygon1);
				map.MapElements.Add(polygon2);
			});

			await InvokeOnMainThreadAsync(() => map.MapElements.Remove(polygon1));

			Assert.Single(map.MapElements);
			Assert.Contains(polygon2, map.MapElements);
			Assert.NotNull(polygon2.MapElementId);
		}
	}
#endif
}