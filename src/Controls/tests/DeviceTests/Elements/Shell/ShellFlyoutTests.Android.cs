using System.Threading.Tasks;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls;
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
			},
			async (shell, handler) =>
			{
				// 1. Set FlyoutIsPresented=true to make the Shell Flyout visible.
				shell.FlyoutIsPresented = true;

				var dl = GetDrawerLayout(handler) as DrawerLayout;
				Assert.NotNull(dl);

				await AssertionExtensions.AssertEventually(() =>
				{
					// 2. Check that the Flyout has size.
					var flyoutFrame = GetFlyoutFrame(handler);
					return flyoutFrame.Width > 0 && flyoutFrame.Height > 0 && dl.IsOpen;
				});
			});
		}
	}
}
