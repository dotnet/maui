using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShellTests : ControlsHandlerTestBase
    {
        [Theory]
		[InlineData(0)]
		[InlineData(100)]
		public async Task FlyoutHeaderRendererDoesNotGoOverContent(int topMargin)
		{
			var flyoutItem = new FlyoutItem() { Items = { new ContentPage() } };
			var flyoutItemGrid = new Grid();
			var flyoutHeaderHeight = 250;
			var layout = new Grid() { BackgroundColor = Colors.Pink, HeightRequest = flyoutHeaderHeight };
			layout.Children.Add(new Button() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, BackgroundColor = Colors.Orange });
			
			if (topMargin > 0)
			{
				layout.Margin = new Thickness(0, topMargin, 0, 0);
			}
			
			Shell.SetItemTemplate(flyoutItem, new DataTemplate(() => flyoutItemGrid));

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				shell.FlyoutHeader = layout;
				shell.Items.Add(flyoutItem);
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);
                var flyout = GetFlyoutPlatformView(handler);
				var header = flyout.Subviews.OfType<ShellFlyoutHeaderContainer>().First();

				// The flyout header's height should be equal to the requested height + the top margin.
				// The safe area should not be accounted for.
				Assert.Equal(flyoutHeaderHeight + topMargin, header.Frame.Height);
                Assert.Equal(topMargin > 0 ? topMargin : GetSafeArea().Top, header.Frame.Top);
			});
		}
    }
}