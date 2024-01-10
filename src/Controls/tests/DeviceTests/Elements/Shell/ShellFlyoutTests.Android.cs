using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact(DisplayName = "FlyoutIsPresented=true sets the visible status of the Shell Flyout.")]
		public async Task FlyoutIsPresentedOpenDrawer()
		{
			await RunShellTest(shell =>
			{
				shell.FlyoutContent = new VerticalStackLayout() { new Label() { Text = "Flyout Content" } };

				// 1. Set FlyoutIsPresented=true to make the Shell Flyout visible.
				shell.FlyoutIsPresented = true;
			},
			async (shell, handler) =>
			{
				await Task.Delay(100);

				var dl = GetDrawerLayout(handler) as DrawerLayout;
				Assert.NotNull(dl);

				// 2. Check that the Flyout has size.
				var flyoutFrame = GetFlyoutFrame(handler);
				Assert.True(flyoutFrame.Width > 0);
				Assert.True(flyoutFrame.Height > 0);

				// 3. Check that the Flyout status. It must be open.
				Assert.True(dl.IsOpen);
			});
		}
	}
}
