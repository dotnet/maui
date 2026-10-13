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
				Assert.True(navView.PaneDisplayMode == UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal);

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

				Assert.True(navView.PaneDisplayMode == UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal);

				// Tabbar should now be hidden, remove the current page which should cause
				// us to switch to another page where the tabbar is not hidden (default)
				shell.Items[0].Items.RemoveAt(1);

				Assert.True(navView.PaneDisplayMode == UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top);
				return Task.FromResult(true);
			});
		}

		[Fact(DisplayName = "Disabled tab added dynamically after appearance inherits TabBarDisabledColor (Windows)")]
		public async Task DynamicallyAddedDisabledTabInheritsTabBarDisabledColor()
		{
			SetupBuilder();

			var disabledColor = Color.FromArgb("#FF00FF00");

			var tab1 = new ShellContent
			{
				Route = "Tab1",
				Title = "Tab1",
				Content = new ContentPage(),
				Icon = "white_tab.png"
			};

			var shell = await CreateShellAsync((shell) =>
			{
				Shell.SetTabBarDisabledColor(shell, disabledColor);
				shell.Items.Add(new TabBar() { Items = { tab1 } });
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				// The appearance observer has fired and _shellAppearanceElement is set.
				// Add a disabled tab after the Shell handler has been initialized.
				var tab2 = new ShellContent
				{
					Route = "Tab2",
					Title = "Tab2",
					Content = new ContentPage(),
					Icon = "white_tab.png",
					IsEnabled = false
				};

				((TabBar)shell.Items[0]).Items.Add(tab2);

				var navigationView = GetTabBarItems(shell.CurrentSection);
				NavigationViewItemViewModel disabledItem = null;


				var found = await WaitUntilAsync(() =>
		{
			disabledItem = GetNavigationViewItem(navigationView, tab2);

			return disabledItem is not null &&
				HasDisabledColor(disabledItem, disabledColor);
		});

				Assert.True(
			found,
			"The dynamically added disabled tab should use TabBarDisabledColor.");


				Assert.NotNull(disabledItem);
				Assert.True(
					HasDisabledColor(disabledItem!, disabledColor),
					"The dynamically added disabled tab should use TabBarDisabledColor.");
			});
		}

		static async Task<bool> WaitUntilAsync(
			Func<bool> condition,
			int timeoutMilliseconds = 5000)
		{
			var start = DateTime.UtcNow;

			while (DateTime.UtcNow - start <
				   TimeSpan.FromMilliseconds(timeoutMilliseconds))
			{
				if (condition())
					return true;

				await Task.Delay(100);
			}

			return condition();
		}

		static NavigationViewItemViewModel GetNavigationViewItem(
			NavigationView navigationView,
			BaseShellItem item) =>
			(navigationView.MenuItemsSource as
				IEnumerable<NavigationViewItemViewModel>)?
				.FirstOrDefault(viewModel => viewModel.Data == item);

		static bool HasDisabledColor(
			NavigationViewItemViewModel item,
			Color expectedColor) =>
			HasColor(item.DisabledForeground, expectedColor) &&
			HasColor(item.Foreground, expectedColor) &&
			HasColor(item.Icon?.Foreground, expectedColor);

		static bool HasColor(
			Microsoft.UI.Xaml.Media.Brush brush,
			Color expectedColor) =>
			brush is Microsoft.UI.Xaml.Media.SolidColorBrush solidColorBrush &&
			expectedColor.Equals(solidColorBrush.Color.ToColor());

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
