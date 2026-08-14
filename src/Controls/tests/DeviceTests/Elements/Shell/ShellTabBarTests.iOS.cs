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
		UITabBar GetTabBar(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			return (pagerParent?.NextResponder as UITabBarController)?.TabBar;
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

		[Fact(DisplayName = "Shell TabBar Background Color Preserves iOS 26 Floating Tab Bar")]
		public async Task ShellTabBarBackgroundColorPreservesIOS26FloatingTabBar()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
				return;

			var expectedColor = Colors.Red;

			await RunShellTabBarTests(
				shell => Shell.SetTabBarBackgroundColor(shell, expectedColor),
				shell =>
				{
					var tabBar = GetTabBar(shell.CurrentSection);

					if (OperatingSystem.IsMacCatalyst())
						Assert.Equal(expectedColor, tabBar.BackgroundColor.ToColor());
					else
						Assert.Null(tabBar.BackgroundColor);

					return Task.CompletedTask;
				});
		}
	}
}