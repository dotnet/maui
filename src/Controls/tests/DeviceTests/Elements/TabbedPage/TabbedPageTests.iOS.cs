using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using TabbedViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests
	{
		UITabBar GetTabBar(TabbedPage tabbedPage)
		{
			// TabbedRenderer IS a UITabBarController — get TabBar from ViewController
			var platformHandler = tabbedPage.Handler as IPlatformViewHandler;
			if (platformHandler?.ViewController is UITabBarController tbc)
				return tbc.TabBar;

			// Fallback: walk the responder chain
			var handler = tabbedPage.CurrentPage.Handler as IPlatformViewHandler;
			var platformView = handler?.PlatformView;

			if (platformView is null)
				throw new Exception("Unable to get platform view from handler");

			UIResponder responder = platformView;
			while (responder != null)
			{
				if (responder is UITabBarController controller)
					return controller.TabBar;
				responder = responder.NextResponder;
			}

			throw new Exception("Unable to find UITabBarController in view hierarchy");
		}

		async Task ValidateTabBarIconColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(
					GetTabBar(tabbedPage),
					tabText, iconColor, MauiContext);
			}
		}

		// Regression test for https://github.com/dotnet/maui/issues/34605
		// Verifies UnselectedItemTintColor is correctly set on iOS 26+ when BarTextColor is set
		[Fact(DisplayName = "iOS 26: UnselectedItemTintColor set when BarTextColor specified")]
		public async Task UnselectedItemTintColorSetFromBarTextColor()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26) && !OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;

			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(true, pages: new[]
			{
				new ContentPage() { Title = "Tab 1" },
				new ContentPage() { Title = "Tab 2" }
			});

			tabbedPage.BarTextColor = Colors.Red;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var tabBar = GetTabBar(tabbedPage);

				// On iOS 26+, our fix sets UnselectedItemTintColor as a workaround
				Assert.NotNull(tabBar.UnselectedItemTintColor);
				Assert.True(
					ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Red.ToPlatform(), 0.1),
					$"Expected UnselectedItemTintColor to be Red but got {tabBar.UnselectedItemTintColor}");

				// Change the color and verify it updates
				tabbedPage.BarTextColor = Colors.Blue;

				Assert.NotNull(tabBar.UnselectedItemTintColor);
				Assert.True(
					ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Blue.ToPlatform(), 0.1),
					$"Expected UnselectedItemTintColor to be Blue but got {tabBar.UnselectedItemTintColor}");

				return Task.CompletedTask;
			});
		}

		// Regression test for https://github.com/dotnet/maui/issues/32125
		// Verifies UnselectedItemTintColor is set from UnselectedTabColor on iOS 26+
		[Fact(DisplayName = "iOS 26: UnselectedItemTintColor set from UnselectedTabColor")]
		public async Task UnselectedItemTintColorSetFromUnselectedTabColor()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26) && !OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;

			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(true, pages: new[]
			{
				new ContentPage() { Title = "Tab 1" },
				new ContentPage() { Title = "Tab 2" }
			});

			tabbedPage.SelectedTabColor = Colors.Green;
			tabbedPage.UnselectedTabColor = Colors.Purple;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var tabBar = GetTabBar(tabbedPage);

				// UnselectedTabColor should take priority for UnselectedItemTintColor
				Assert.NotNull(tabBar.UnselectedItemTintColor);
				Assert.True(
					ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Purple.ToPlatform(), 0.1),
					$"Expected UnselectedItemTintColor to be Purple but got {tabBar.UnselectedItemTintColor}");

				return Task.CompletedTask;
			});
		}

		// Verifies changing BarTextColor updates UnselectedItemTintColor on iOS 26+
		[Fact(DisplayName = "iOS 26: Changing BarTextColor updates UnselectedItemTintColor")]
		public async Task ChangingBarTextColorUpdatesUnselectedItemTintColor()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26) && !OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;

			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(true, pages: new[]
			{
				new ContentPage() { Title = "Tab 1" },
				new ContentPage() { Title = "Tab 2" }
			});

			tabbedPage.BarTextColor = Colors.Red;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var tabBar = GetTabBar(tabbedPage);

				// Should be Red after BarTextColor is applied
				Assert.NotNull(tabBar.UnselectedItemTintColor);
				Assert.True(
					ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Red.ToPlatform(), 0.1),
					$"Expected Red but got {tabBar.UnselectedItemTintColor}");

				// Change to Green
				tabbedPage.BarTextColor = Colors.Green;

				Assert.NotNull(tabBar.UnselectedItemTintColor);
				Assert.True(
					ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Green.ToPlatform(), 0.1),
					$"Expected Green but got {tabBar.UnselectedItemTintColor}");

				// Clear the color — should no longer be Red or Green
				tabbedPage.BarTextColor = null;

				// After clearing, the tint should not be Red or Green anymore
				if (tabBar.UnselectedItemTintColor is not null)
				{
					Assert.False(
						ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Red.ToPlatform(), 0.1),
						"UnselectedItemTintColor should not still be Red after clearing");
					Assert.False(
						ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, Colors.Green.ToPlatform(), 0.1),
						"UnselectedItemTintColor should not still be Green after clearing");
				}

				return Task.CompletedTask;
			});
		}
	}
}
