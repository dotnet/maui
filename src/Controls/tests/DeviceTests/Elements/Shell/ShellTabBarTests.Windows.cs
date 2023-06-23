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

				Shell.SetTabBarIsVisible(shell.Items[0].Items[1], false);
				shell.CurrentItem = shell.Items[0].Items[1];

				Assert.False(navView.IsPaneVisible);

				shell.Items[0].Items.RemoveAt(1);

				Assert.True(navView.IsPaneVisible);
				Assert.True(navView.PaneDisplayMode == UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top);
				return Task.FromResult(true);
			});
		}

		List<WNavigationViewItem> GetTabBarItems(Shell shell)
		{
			var shellItemHandler = shell.CurrentItem.Handler as ShellItemHandler;
			var navView = shellItemHandler.PlatformView as MauiNavigationView;

			return navView.TopNavArea.GetChildren<WNavigationViewItem>().ToList();
		}

		async Task ValidateTabBarItemColor(ShellSection item, Color expectedColor, bool hasColor)
		{
			var items = GetTabBarItems(item.FindParentOfType<Shell>());
			var platformItem =
				items.FirstOrDefault(x => x.Content.ToString().Equals(item.Title, StringComparison.OrdinalIgnoreCase));

			if (hasColor)
				await AssertionExtensions.AssertContainsColor(platformItem, expectedColor, item.FindMauiContext());
			else
				await AssertionExtensions.AssertDoesNotContainColor(platformItem, expectedColor, item.FindMauiContext());
		}
	}
}
