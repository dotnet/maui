using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		static UIView GetTabControllerBackingView(Shell shell) =>
			(shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController)
				.NextResponder is UITabBarController tabController ? tabController.View : null;

		Task<Shell> CreateTwoTabShellAsync(ContentPage page1, ContentPage page2) =>
			CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar
				{
					Items =
					{
						new Tab { Title = "Tab 1", Items = { new ShellContent { Content = page1 } } },
						new Tab { Title = "Tab 2", Items = { new ShellContent { Content = page2 } } }
					}
				});
			});

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
			var shell = await CreateTwoTabShellAsync(page, new ContentPage());

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(page);
				await OnNavigatedToAsync(page);

				var backingView = GetTabControllerBackingView(shell);
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

		[Theory(DisplayName = "Tab controller backing view applies preconfigured backgrounds across tab switches")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task TabControllerBackingViewAppliesPreconfiguredBackgroundAcrossTabSwitches(bool useShellHandlerHierarchy)
		{
			// Issue 39800's actual scenario: the pages are dark before the Shell
			// attaches and the flash shows while switching tabs. No background is
			// mutated after attach, so this covers the initial and displayed-page
			// update paths rather than the PropertyChanged path.
			if (!OperatingSystem.IsIOSVersionAtLeast(18) && !OperatingSystem.IsMacCatalystVersionAtLeast(18))
				return;

			if (useShellHandlerHierarchy)
				SetupShellHandlerHierarchy();
			else
				SetupBuilder();

			var page1 = new ContentPage { BackgroundColor = Colors.Black };
			var page2 = new ContentPage { BackgroundColor = Colors.Blue };
			var shell = await CreateTwoTabShellAsync(page1, page2);

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(page1);
				await OnNavigatedToAsync(page1);

				var backingView = GetTabControllerBackingView(shell);
				Assert.NotNull(backingView);

				await AssertEventually(
					() => ColorComparison.ARGBEquivalent(Colors.Black.ToPlatform(), backingView.BackgroundColor),
					message: "The backing view should adopt Tab 1's preconfigured black background without any post-attach mutation.");

				shell.CurrentItem.CurrentItem = shell.CurrentItem.Items[1];
				await OnNavigatedToAsync(page2);

				await AssertEventually(
					() => ColorComparison.ARGBEquivalent(Colors.Blue.ToPlatform(), backingView.BackgroundColor),
					message: "Switching tabs should move the backing view to Tab 2's preconfigured background.");
			});
		}

		[Theory(DisplayName = "Tab controller backing view ignores partially transparent pages")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task TabControllerBackingViewRespectsPageOpacity(bool useShellHandlerHierarchy)
		{
			// A page with Opacity < 1 is meant to show what is behind it, so its
			// background must not be mirrored onto the opaque backing view.
			if (!OperatingSystem.IsIOSVersionAtLeast(18) && !OperatingSystem.IsMacCatalystVersionAtLeast(18))
				return;

			if (useShellHandlerHierarchy)
				SetupShellHandlerHierarchy();
			else
				SetupBuilder();

			var page = new ContentPage();
			var shell = await CreateTwoTabShellAsync(page, new ContentPage());

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(page);
				await OnNavigatedToAsync(page);

				var backingView = GetTabControllerBackingView(shell);
				Assert.NotNull(backingView);

				var defaultBackground = backingView.BackgroundColor;

				page.BackgroundColor = Colors.Black;
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Black.ToPlatform(), backingView.BackgroundColor),
					"The backing view should adopt the displayed page's opaque black background.");

				page.Opacity = 0.5;
				if (defaultBackground is null)
					Assert.Null(backingView.BackgroundColor);
				else
					Assert.True(ColorComparison.ARGBEquivalent(defaultBackground, backingView.BackgroundColor),
						"A partially transparent page should not mirror its background; the captured default must be restored.");

				page.Opacity = 1;
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Black.ToPlatform(), backingView.BackgroundColor),
					"Restoring full opacity should reapply the page's opaque black background.");
			});
		}
	}
}
