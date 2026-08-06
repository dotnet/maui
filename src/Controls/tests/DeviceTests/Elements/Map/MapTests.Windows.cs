using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Handlers;
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
			});
		}
	}
}
