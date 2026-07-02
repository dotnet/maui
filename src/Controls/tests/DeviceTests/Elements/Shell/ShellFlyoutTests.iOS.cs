using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
#if !MACCATALYST
		[Theory]
		[InlineData(null)]
		[InlineData(0)]
		[InlineData(100)]
		public async Task FlyoutHeaderRendererHasTheRightHeight(int? topMargin)
		{
			var flyoutHeaderHeight = 250;
			var layout = new Grid() { HeightRequest = flyoutHeaderHeight };
			layout.Children.Add(new Button() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill });

			if (topMargin is not null)
			{
				layout.Margin = new Thickness(0, topMargin.Value, 0, 0);
			}

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				shell.FlyoutHeader = layout;
				shell.FlyoutContent = new ScrollView() { Content = new Label() { Text = "FlyoutContent" } };
			},
			// RunShellTest's callback signature was changed from Func<Shell, IShellContext, Task>
			// to Func<Shell, Task> so Android handler subclasses can inherit the test without
			// a platform-specific handler type leaking into the shared callback signature.
			// The handler is now accessed directly from shell.Handler inside the lambda.
			async shell =>
			{
				var handler = (ShellRenderer)shell.Handler;
				await OpenFlyout(handler);
				var flyout = GetFlyoutPlatformView(handler);
				var header = flyout.Subviews.OfType<ShellFlyoutHeaderContainer>().First();

				// The flyout header's height should be equal to the requested height + the top margin.
				// The safe area should not be accounted for.
				Assert.Equal(flyoutHeaderHeight + (topMargin ?? 0), header.Frame.Height);
			});
		}
#endif
	}
}