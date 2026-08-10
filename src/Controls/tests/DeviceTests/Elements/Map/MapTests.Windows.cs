#nullable enable

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		// Regression test for https://github.com/dotnet/maui/issues/37096
		[Fact]
		public async Task RemovingMapFromVisualTreeDoesNotCrash()
		{
			var map = new Map();
			var layout = new VerticalStackLayout
			{
				map
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				var mapHandler = Assert.IsType<MapHandler>(map.Handler);
				await AssertEventually(
					() => GetWebViewReady(mapHandler),
					timeout: 15_000,
					message: "WebView2 never became ready");

				var webViewBeforeReload = GetWebView(mapHandler);
				Assert.NotNull(webViewBeforeReload);

				layout.Remove(map);
				await OnUnloadedAsync(map);

				layout.Add(map);
				await OnLoadedAsync(map);

				Assert.Same(webViewBeforeReload, GetWebView(mapHandler));

				map.Pins.Add(new Pin
				{
					Label = "Pin",
					Location = new Location(47.6458, -122.1419)
				});

				var platformMap = Assert.IsType<MapControl>(map.Handler.PlatformView);
				var pinsLayer = Assert.IsType<MapElementsLayer>(platformMap.Layers[0]);
				Assert.Single(pinsLayer.MapElements);

				map.MoveToRegion(MapSpan.FromCenterAndRadius(
					new Location(47.6062, -122.3321),
					Distance.FromKilometers(5)));

				Assert.Equal(47.6062, platformMap.Center.Position.Latitude, 3);
				Assert.Equal(-122.3321, platformMap.Center.Position.Longitude, 3);
				Assert.NotNull(map.VisibleRegion);
				await AssertEventually(
					() => GetWebViewReady(mapHandler) &&
						GetFieldValue<MapSpan>(mapHandler, "_pendingSpan") == null,
					timeout: 15_000,
					message: "Map state was not restored after reload");

				layout.Remove(map);
				await OnUnloadedAsync(map);
			});
		}

		[Fact]
		public async Task DisconnectHandlerCleansUpResources()
		{
			var map = new Map();
			var layout = new VerticalStackLayout
			{
				map
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				var mapHandler = Assert.IsType<MapHandler>(map.Handler);
				await AssertEventually(
					() => GetWebViewReady(mapHandler),
					timeout: 15_000,
					message: "WebView2 never became ready");

				map.Pins.Add(new Pin
				{
					Label = "Pin",
					Location = new Location(47.6458, -122.1419)
				});
				Assert.Single(GetRequiredFieldValue<List<MapIcon>>(mapHandler, "_mapIcons"));
				var platformMap = Assert.IsType<MapControl>(map.Handler.PlatformView);
				var layerCount = platformMap.Layers.Count;

				layout.Remove(map);
				await OnUnloadedAsync(map);

				map.Handler?.DisconnectHandler();

				Assert.Null(map.Handler);
				Assert.Null(GetWebView(mapHandler));
				Assert.False(GetWebViewReady(mapHandler));
				Assert.Null(GetFieldValue<MapElementsLayer>(mapHandler, "_pinsLayer"));
				Assert.Null(GetFieldValue<MapControl>(mapHandler, "_mapControl"));
				Assert.Empty(GetRequiredFieldValue<List<MapIcon>>(mapHandler, "_mapIcons"));
				Assert.Equal(layerCount, platformMap.Layers.Count);
			});
		}

		static WebView2? GetWebView(MapHandler handler)
		{
			var field = typeof(MapHandler).GetField("_webView", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			return field.GetValue(handler) as WebView2;
		}

		static bool GetWebViewReady(MapHandler handler)
		{
			var field = typeof(MapHandler).GetField("_webViewReady", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			return field.GetValue(handler) is bool value && value;
		}

		static T? GetFieldValue<T>(MapHandler handler, string fieldName)
			where T : class
		{
			var field = typeof(MapHandler).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			return field.GetValue(handler) as T;
		}

		static T GetRequiredFieldValue<T>(MapHandler handler, string fieldName)
			where T : class
		{
			var value = GetFieldValue<T>(handler, fieldName);
			Assert.NotNull(value);
			return value;
		}
	}
}
