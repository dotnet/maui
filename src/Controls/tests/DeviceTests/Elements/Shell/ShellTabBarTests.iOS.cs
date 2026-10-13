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
#if MACCATALYST
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task VisibleTabBarRestoresZeroAlpha(bool hidden)
		{
			if (!OperatingSystem.IsMacCatalystVersionAtLeast(18))
				return;

			await RunShellTabBarTests(_ => { }, shell =>
			{
				var context = (IShellContext)shell.Handler;
				var renderer = Assert.IsType<ShellItemRenderer>(context.CurrentShellItemRenderer);
				Assert.True(renderer.ShellItemController.ShowTabs);
				renderer.TabBar.Hidden = hidden;
				renderer.TabBar.Alpha = 0;

				renderer.ViewWillLayoutSubviews();

				Assert.False(renderer.TabBar.Hidden);
				Assert.Equal(1, renderer.TabBar.Alpha);

				Shell.SetTabBarIsVisible(shell.CurrentPage, false);
				renderer.ViewWillLayoutSubviews();
				Assert.True(renderer.TabBar.Hidden);
				return Task.CompletedTask;
			});
		}
#endif

		UITabBar GetTabBar(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

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
	}
}