using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact(DisplayName = "Tab controller backing view tracks the displayed page's solid background")]
		public async Task TabControllerBackingViewTracksDisplayedPageBackground()
		{
			// The iOS 18+ tab-switch cross-fade composites the pages over the tab
			// controller's own view, so a mismatched backing color flashes during
			// every switch (Issue 39800).
			if (!OperatingSystem.IsIOSVersionAtLeast(18) && !OperatingSystem.IsMacCatalystVersionAtLeast(18))
				return;

			SetupBuilder();

			var page = new ContentPage();
			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar
				{
					Items =
					{
						new Tab { Title = "Tab 1", Items = { new ShellContent { Content = page } } },
						new Tab { Title = "Tab 2", Items = { new ShellContent { Content = new ContentPage() } } }
					}
				});
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(page);
				await OnNavigatedToAsync(page);

				var backingView = (shell.CurrentPage.Handler as IPlatformViewHandler)
					.PlatformView.FindParent(x => x.NextResponder is UITabBarController)
					.NextResponder is UITabBarController tabController ? tabController.View : null;
				Assert.NotNull(backingView);

				var defaultBackground = backingView.BackgroundColor;

				page.BackgroundColor = Colors.Black;
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Black.ToPlatform(), backingView.BackgroundColor),
					"The backing view should adopt the displayed page's opaque black background.");

				page.BackgroundColor = null;
				if (defaultBackground is null)
					Assert.Null(backingView.BackgroundColor);
				else
					Assert.True(ColorComparison.ARGBEquivalent(defaultBackground, backingView.BackgroundColor),
						"Clearing the page background should restore the captured default backing color.");
			});
		}
	}
}
