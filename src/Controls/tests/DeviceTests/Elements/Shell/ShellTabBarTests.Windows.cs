using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using WButton = Microsoft.UI.Xaml.Controls.Button;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using WVisualTreeHelper = Microsoft.UI.Xaml.Media.VisualTreeHelper;


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

		[Fact(DisplayName = "Tab With Several ShellContents Opens From TabBar Overflow Menu")]
		public async Task TabWithSeveralShellContentsOpensFromTabBarOverflowMenu()
		{
			// https://github.com/dotnet/maui/issues/37985
			// Clicking such a tab in the overflow menu of the tab bar crashed the app with E_BOUNDS
			SetupBuilder();

			var subTabB = new ShellContent() { Title = "Sub B", Content = new ContentPage() };
			var nestedTab = new Tab()
			{
				Title = "Nested",
				Items =
				{
					new ShellContent() { Title = "Sub A", Content = new ContentPage() },
					subTabB
				}
			};

			var shell = await CreateShellAsync((shell) =>
			{
				var tabBar = new TabBar();
				for (int i = 1; i <= 7; i++)
				{
					tabBar.Items.Add(new Tab()
					{
						Title = $"Tab {i}",
						Items = { new ShellContent() { Content = new ContentPage() } }
					});
				}

				tabBar.Items.Add(nestedTab);
				shell.Items.Add(tabBar);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var navView = shell.CurrentItem.Handler.PlatformView as MauiNavigationView;
				var overflowButton = navView.GetDescendantByName<WButton>("TopNavOverflowButton");

				// Make the tab bar narrow enough to push "Nested" into the overflow menu
				navView.Width = 400;
				await AssertEventually(() => overflowButton.Visibility == UI.Xaml.Visibility.Visible, message: "The tab bar has no overflow menu");

				try
				{
					overflowButton.Flyout.ShowAt(overflowButton);

					WNavigationViewItem nestedItem = null;
					await AssertEventually(() => (nestedItem = GetOverflowMenuItem(navView, nestedTab)) is not null, message: "\"Nested\" is not in the overflow menu");

					// When the click selects this tab, WinUI fails fast and takes the whole test app down.
					// Check it first, so the test just fails instead.
					Assert.False(nestedItem.SelectsOnInvoked, "Clicking \"Nested\" in the overflow menu selects it");

					ClickNavigationViewItem(nestedItem);

					// The tab only expands to show its sub tabs, the current page stays the same
					Assert.True(nestedItem.IsExpanded);
					Assert.NotEqual(nestedTab, shell.CurrentItem.CurrentItem);

					WNavigationViewItem subTabBItem = null;
					await AssertEventually(() => (subTabBItem = nestedItem.GetChildren<WNavigationViewItem>().FirstOrDefault(item => IsNavigationViewItemFor(item, subTabB))) is not null, message: "\"Sub B\" is not shown");

					ClickNavigationViewItem(subTabBItem);
					await AssertEventually(() => shell.CurrentItem.CurrentItem == nestedTab && nestedTab.CurrentItem == subTabB, message: "Did not navigate to \"Sub B\"");

					// Opening and closing the overflow menu again keeps working
					overflowButton.Flyout.ShowAt(overflowButton);
					await AssertEventually(() => overflowButton.Flyout.IsOpen, message: "The overflow menu did not open again");
					overflowButton.Flyout.Hide();
					await AssertEventually(() => !overflowButton.Flyout.IsOpen, message: "The overflow menu did not close");
					Assert.Equal(subTabB, nestedTab.CurrentItem);
				}
				finally
				{
					overflowButton.Flyout.Hide();
				}
			});
		}

		static WNavigationViewItem GetOverflowMenuItem(NavigationView navView, BaseShellItem shellItem) =>
			WVisualTreeHelper.GetOpenPopupsForXamlRoot(navView.XamlRoot)
				.Where(popup => popup.Child is not null)
				.SelectMany(popup => popup.Child.GetChildren<WNavigationViewItem>())
				.FirstOrDefault(item => IsNavigationViewItemFor(item, shellItem));

		static bool IsNavigationViewItemFor(WNavigationViewItem item, BaseShellItem shellItem) =>
			(item?.DataContext as NavigationViewItemViewModel)?.Data == shellItem;

		// Same path as a mouse click. The automation peer doesn't report the Invoke pattern, but it implements it.
		static void ClickNavigationViewItem(WNavigationViewItem item) =>
			((IInvokeProvider)FrameworkElementAutomationPeer.CreatePeerForElement(item)).Invoke();

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
