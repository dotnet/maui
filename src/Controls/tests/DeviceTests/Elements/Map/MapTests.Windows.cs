using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		// Regression test for https://github.com/dotnet/maui/issues/37096
		[Fact]
		public async Task RemovingMapFromVisualTreeDoesNotCrash()
		{
			var map = new Map { IsEnabled = false };
			var layout = new VerticalStackLayout
			{
				map
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				await Task.Delay(1000);
				layout.Remove(map);
				await OnUnloadedAsync(map);

				layout.Add(map);
				await OnLoadedAsync(map);

				map.Pins.Add(new Pin
				{
					Label = "Pin",
					Location = new Location(47.6458, -122.1419)
				});

				var platformMap = Assert.IsType<MapControl>(map.Handler.PlatformView);
				var pinsLayer = Assert.IsType<MapElementsLayer>(platformMap.Layers[0]);
				Assert.Single(pinsLayer.MapElements);

				layout.Remove(map);
				await OnUnloadedAsync(map);
			});
		}
	}
}
