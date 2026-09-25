using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		// Regression test for https://github.com/dotnet/maui/issues/37096
		[Fact]
		public async Task RemovingAndReaddingMapDoesNotMutateNativeLayers()
		{
			var map = new Map
			{
				// Match the XAML declaration in the public reproduction.
				IsEnabled = false
			};
			var layout = new VerticalStackLayout
			{
				map
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				var mapHandler = Assert.IsType<MapHandler>(map.Handler);
				var webView = await WaitForMapReady(mapHandler);
				var platformMap = Assert.IsType<MapControl>(map.Handler.PlatformView);
				var layerCount = platformMap.Layers.Count;

				layout.Remove(map);
				await OnUnloadedAsync(map);
				Assert.Equal(layerCount, platformMap.Layers.Count);

				layout.Add(map);
				await OnLoadedAsync(map);

				map.Pins.Add(new Pin
				{
					Label = "Pin",
					Location = new Location(47.6458, -122.1419)
				});

				var pinsLayer = Assert.IsType<MapElementsLayer>(platformMap.Layers[0]);
				Assert.Single(pinsLayer.MapElements);

				// The embedded document allocates pin IDs from 1, before any map-service response.
				await AssertEventually(
					async () => await webView.ExecuteScriptAsync("id === 2") == "true",
					timeout: 15_000,
					message: "MapControl did not finish creating the pin");

				layout.Remove(map);
				await OnUnloadedAsync(map);
				Assert.Equal(layerCount, platformMap.Layers.Count);
			});
		}

		[Fact]
		public async Task DisconnectHandlerDoesNotMutateNativeLayers()
		{
			var map = new Map
			{
				IsEnabled = false
			};
			var layout = new VerticalStackLayout
			{
				map
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				var mapHandler = Assert.IsType<MapHandler>(map.Handler);
				await WaitForMapReady(mapHandler);
				var platformMap = Assert.IsType<MapControl>(map.Handler.PlatformView);
				var layerCount = platformMap.Layers.Count;

				layout.Remove(map);
				await OnUnloadedAsync(map);

				map.Handler?.DisconnectHandler();

				Assert.Null(map.Handler);
				Assert.Equal(layerCount, platformMap.Layers.Count);
			});
		}

		static async Task<WebView2> WaitForMapReady(MapHandler handler)
		{
			var webViewReadyField = typeof(MapHandler).GetField("_webViewReady", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(webViewReadyField);

			await AssertEventually(
				() => webViewReadyField.GetValue(handler) is true,
				timeout: 15_000,
				message: "MapControl's WebView2 never finished loading");

			var platformMap = Assert.IsType<MapControl>(handler.PlatformView);
			var webView = Assert.IsType<WebView2>(VisualTreeHelper.GetChild(platformMap, 0));

			// NavigationCompleted precedes native InitializeWebMap/OnLayerAdded continuations.
			// Wait for their script work, not map tiles, before allowing the test window to close.
			await AssertEventually(
				async () => await webView.ExecuteScriptAsync($"getSymbolLayers().length === {platformMap.Layers.Count}") == "true",
				timeout: 15_000,
				message: "MapControl did not finish initializing its native layers");

			return webView;
		}
	}
}
