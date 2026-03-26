using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		UITabBar GetTabBar(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem?.Parent as Shell;

			if (shell?.Handler is IShellContext shellContext)
			{
				if (shellContext.CurrentShellItemRenderer is UITabBarController tabBarController)
					return tabBarController.TabBar;
			}

			// Fallback: walk the view hierarchy from the current page
			var platformView = (shell?.CurrentPage?.Handler as IPlatformViewHandler)?.PlatformView;
			if (platformView is null)
				return null;

			var pagerParent = platformView.FindParent(x => x.NextResponder is UITabBarController);

			if (pagerParent is null)
			{
				// iOS 26+: walk the responder chain to find the UITabBarController directly
				UIResponder responder = platformView;
				while (responder != null)
				{
					if (responder is UITabBarController tbc)
						return tbc.TabBar;
					responder = responder.NextResponder;
				}
				return null;
			}

			// In macOS 15 Sequoia, the UITabBar is nested within the second subview (index 1) of the pagerParent.
			if (OperatingSystem.IsMacCatalystVersionAtLeast(15, 0) || OperatingSystem.IsMacOSVersionAtLeast(15, 0))
			{
				var subview = pagerParent.Subviews.ElementAtOrDefault(1);

				if (subview?.Subviews is null)
					return null;

				return subview.Subviews.OfType<UITabBar>().FirstOrDefault();
			}

			return pagerParent.Subviews.OfType<UITabBar>().FirstOrDefault();
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTabBar(item),
					item.Title, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTabBar(item),
					item.Title, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
				ShellSection item,
				Color textColor,
				bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTabBar(item),
					item.Title, textColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTabBar(item),
					item.Title, textColor, MauiContext);
			}
		}

		Task ValidateTabBarUnselectedTintColorProperty(ShellSection item, Color expectedColor)
		{
			var tabBar = GetTabBar(item);
			Assert.NotNull(tabBar);
			Assert.NotNull(tabBar.UnselectedItemTintColor);
			Assert.True(
				ColorComparison.ARGBEquivalent(tabBar.UnselectedItemTintColor, expectedColor.ToPlatform(), 0.1),
				$"Expected UnselectedItemTintColor to be {expectedColor} but got {tabBar.UnselectedItemTintColor}");
			return Task.CompletedTask;
		}
	}
}