using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact(DisplayName = "Shell TabBar visibility toggle")]
		public async Task ShellTabBarVisibilityToggleWorks()
		{
			await RunShellTabBarTests(shell => { },
			(shell) =>
			{
				var shellItemHandler = shell.CurrentItem.Handler as ShellItemHandler;
				var navView = shellItemHandler.PlatformView as MauiNavigationView;

				Shell.SetTabBarIsVisible(shell.CurrentPage, false);
				Assert.False(navView.IsPaneVisible);

				Shell.SetTabBarIsVisible(shell.CurrentPage, true);
				Assert.True(navView.IsPaneVisible);
				Assert.True(navView.PaneDisplayMode == UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top);

				return Task.FromResult(true);
			});
		}

		[Fact(DisplayName = "Shell TabBar visibility toggles when removing current")]
		public async Task ShellTabBarVisibilityToggleWorksRemovingCurrentItem()
		{
			await RunShellTabBarTests(shell => { },
			(shell) =>
			{
				var shellItemHandler = shell.CurrentItem.Handler as ShellItemHandler;
				var navView = shellItemHandler.PlatformView as MauiNavigationView;

				// Set page to have the tabbar be hidden, then switch to it
				Shell.SetTabBarIsVisible(shell.Items[0].Items[1], false);
				shell.CurrentItem = shell.Items[0].Items[1];

				Assert.False(navView.IsPaneVisible);

				// Tabbar should now be hidden, remove the current page which should cause
				// us to switch to another page where the tabbar is not hidden (default)
				shell.Items[0].Items.RemoveAt(1);

				Assert.True(navView.IsPaneVisible);
				Assert.True(navView.PaneDisplayMode == UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top);
				return Task.FromResult(true);
			});
		}

		NavigationView GetTabBarItems(ShellSection section)
		{
			var shellItemHandler = section.FindParentOfType<Shell>().CurrentItem.Handler as ShellItemHandler;
			var navView = shellItemHandler.PlatformView as MauiNavigationView;

			return navView;
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTabBarItems(item),
					item.Title, iconColor, item.FindMauiContext());
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTabBarItems(item),
					item.Title, iconColor, item.FindMauiContext());
			}
		}

		async Task ValidateTabBarTextColor(
				ShellSection item,
				Color textColor,
				bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTabBarItems(item),
					item.Title, textColor, item.FindMauiContext());
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTabBarItems(item),
					item.Title, textColor, item.FindMauiContext());
			}
		}
	}
}
