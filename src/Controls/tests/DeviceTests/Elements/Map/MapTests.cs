using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	// Note: Map tests are iOS/MacCatalyst only because Android requires an API key
	// to instantiate the MapView, which is not available in the test environment.
#if IOS || MACCATALYST
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Polygon Clearing Resets MapElementId")]
		public async Task PolygonClearingResetsMapElementId()
		{
			// This test reproduces the issue reported in #30097
			// where polygons would persist after clearing because MapElementId wasn't reset
			
			var map = new Map();
			var handler = await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			// Verify initial state
			Assert.Empty(map.MapElements);

			// Create test polygons
			var polygon1 = new Polygon
			{
				StrokeColor = Colors.Blue,
				StrokeWidth = 5,
				FillColor = Colors.Red.WithAlpha(0.2f),
				Geopath =
				{
					new Location(47.6458, -122.1419),
					new Location(47.6458, -122.1119),
					new Location(47.6558, -122.1119),
					new Location(47.6558, -122.1419)
				}
			};

			var polygon2 = new Polygon
			{
				StrokeColor = Colors.Green,
				StrokeWidth = 3,
				FillColor = Colors.Blue.WithAlpha(0.3f),
				Geopath =
				{
					new Location(47.6358, -122.1319),
					new Location(47.6358, -122.1019),
					new Location(47.6458, -122.1019),
					new Location(47.6458, -122.1319)
				}
			};

			// Perform multiple add/clear cycles to reproduce the issue
			for (int cycle = 0; cycle < 3; cycle++)
			{
				// Add polygons
				await InvokeOnMainThreadAsync(() =>
				{
					map.MapElements.Add(polygon1);
					map.MapElements.Add(polygon2);
				});

				// Verify polygons were added
				Assert.Equal(2, map.MapElements.Count);
				Assert.Contains(polygon1, map.MapElements);
				Assert.Contains(polygon2, map.MapElements);

				// Before fix: MapElementId would remain set after clearing
				// After fix: MapElementId should be cleared when clearing map elements

				// Clear all elements
				await InvokeOnMainThreadAsync(() =>
				{
					map.MapElements.Clear();
				});

				// Verify elements are cleared from the map
				Assert.Empty(map.MapElements);

				// Verify MapElementId is reset (this is the key fix)
				// If MapElementId is not null, it means the fix didn't work
				Assert.Null(polygon1.MapElementId);
				Assert.Null(polygon2.MapElementId);
			}

			// Final verification: Add polygons one more time to ensure they work correctly
			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Add(polygon1);
				map.MapElements.Add(polygon2);
			});

			Assert.Equal(2, map.MapElements.Count);
			Assert.Contains(polygon1, map.MapElements);
			Assert.Contains(polygon2, map.MapElements);

			// Clean up
			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Clear();
			});

			Assert.Empty(map.MapElements);
			Assert.Null(polygon1.MapElementId);
			Assert.Null(polygon2.MapElementId);
		}

		[Fact(DisplayName = "Mixed MapElement Types Clearing Resets All MapElementIds")]
		public async Task MixedMapElementTypesClearingResetsAllMapElementIds()
		{
			// Test that clearing works for different types of map elements
			var map = new Map();
			var handler = await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var polygon = new Polygon
			{
				StrokeColor = Colors.Blue,
				StrokeWidth = 2,
				FillColor = Colors.Red.WithAlpha(0.3f),
				Geopath =
				{
					new Location(47.6458, -122.1419),
					new Location(47.6458, -122.1319),
					new Location(47.6558, -122.1319),
					new Location(47.6558, -122.1419)
				}
			};

			var polyline = new Polyline
			{
				StrokeColor = Colors.Green,
				StrokeWidth = 3,
				Geopath =
				{
					new Location(47.6358, -122.1319),
					new Location(47.6378, -122.1299),
					new Location(47.6398, -122.1279),
					new Location(47.6418, -122.1259)
				}
			};

			var circle = new Circle
			{
				Center = new Location(47.6400, -122.1300),
				Radius = Distance.FromMeters(500),
				StrokeColor = Colors.Purple,
				StrokeWidth = 2,
				FillColor = Colors.Yellow.WithAlpha(0.2f)
			};

			// Add all elements
			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Add(polygon);
				map.MapElements.Add(polyline);
				map.MapElements.Add(circle);
			});

			Assert.Equal(3, map.MapElements.Count);

			// Clear all elements
			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Clear();
			});

			// Verify all elements are cleared and MapElementIds are reset
			Assert.Empty(map.MapElements);
			Assert.Null(polygon.MapElementId);
			Assert.Null(polyline.MapElementId);
			Assert.Null(circle.MapElementId);
		}

		[Fact(DisplayName = "Individual Element Removal Maintains Other MapElementIds")]
		public async Task IndividualElementRemovalMaintainsOtherMapElementIds()
		{
			// Test that removing individual elements doesn't affect other elements' MapElementIds
			var map = new Map();
			var handler = await CreateHandlerAsync<Microsoft.Maui.Maps.Handlers.MapHandler>(map);

			var polygon1 = new Polygon
			{
				StrokeColor = Colors.Blue,
				StrokeWidth = 2,
				FillColor = Colors.Red.WithAlpha(0.3f),
				Geopath =
				{
					new Location(47.6458, -122.1419),
					new Location(47.6458, -122.1319),
					new Location(47.6558, -122.1319),
					new Location(47.6558, -122.1419)
				}
			};

			var polygon2 = new Polygon
			{
				StrokeColor = Colors.Green,
				StrokeWidth = 2,
				FillColor = Colors.Blue.WithAlpha(0.3f),
				Geopath =
				{
					new Location(47.6358, -122.1319),
					new Location(47.6358, -122.1219),
					new Location(47.6458, -122.1219),
					new Location(47.6458, -122.1319)
				}
			};

			// Add both polygons
			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Add(polygon1);
				map.MapElements.Add(polygon2);
			});

			Assert.Equal(2, map.MapElements.Count);

			// Remove only the first polygon
			await InvokeOnMainThreadAsync(() =>
			{
				map.MapElements.Remove(polygon1);
			});

			Assert.Single(map.MapElements);
			Assert.Contains(polygon2, map.MapElements);
			Assert.DoesNotContain(polygon1, map.MapElements);

			// polygon2 should still have its MapElementId set (it's still on the map)
			// This verifies that individual removal doesn't clear all MapElementIds
			Assert.NotNull(polygon2.MapElementId);
		}
	}
#endif
}