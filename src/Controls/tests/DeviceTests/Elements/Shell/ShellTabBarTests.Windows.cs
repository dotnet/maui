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

			await RunTabBarOverflowMenuTest(nestedTab, async (shell, navView, overflowButton) =>
			{
				// A tab with one ShellContent still gets selected right away
				var singleTab = shell.CurrentItem.Items[6];
				var singleTabItem = await OpenOverflowMenuAndGetItem(navView, overflowButton, singleTab);
				Assert.True(singleTabItem.SelectsOnInvoked);
				ClickNavigationViewItem(singleTabItem);
				await AssertEventually(() => shell.CurrentItem.CurrentItem == singleTab, message: "Did not navigate to \"Tab 7\"");

				var nestedItem = await OpenOverflowMenuAndGetItem(navView, overflowButton, nestedTab);

				// When the click selects this tab, WinUI fails fast and takes the whole test app down.
				// Check it first, so the test just fails instead.
				Assert.False(nestedItem.SelectsOnInvoked, "Clicking \"Nested\" in the overflow menu selects it");

				ClickNavigationViewItem(nestedItem);

				// The tab only expands to show its sub tabs, the current page stays the same
				Assert.True(nestedItem.IsExpanded);
				Assert.Equal(singleTab, shell.CurrentItem.CurrentItem);

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
			});
		}

		[Fact(DisplayName = "TabBar Overflow Menu Tab Follows Its ShellContent Changes")]
		public async Task TabBarOverflowMenuTabFollowsItsShellContentChanges()
		{
			// https://github.com/dotnet/maui/issues/37985
			// A tab can get or lose its sub tabs while it already sits in the overflow menu
			var subTabB = new ShellContent() { Title = "Sub B", Content = new ContentPage() };
			var changingTab = new Tab()
			{
				Title = "Changing",
				Items = { new ShellContent() { Title = "Sub A", Content = new ContentPage() } }
			};

			await RunTabBarOverflowMenuTest(changingTab, async (shell, navView, overflowButton) =>
			{
				var changingItem = await OpenOverflowMenuAndGetItem(navView, overflowButton, changingTab);
				Assert.True(changingItem.SelectsOnInvoked);

				// The tab bar picks up the sub tabs of other tabs when it refreshes, a title change is enough for that
				changingTab.Items.Add(subTabB);
				shell.CurrentItem.Title = "Refreshed";
				Assert.NotNull(changingItem.MenuItemsSource);
				Assert.False(changingItem.SelectsOnInvoked, "Clicking \"Changing\" in the overflow menu selects it");

				changingTab.Items.Remove(subTabB);
				shell.CurrentItem.Title = "Refreshed again";
				Assert.Null(changingItem.MenuItemsSource);
				Assert.True(changingItem.SelectsOnInvoked);

				ClickNavigationViewItem(changingItem);
				await AssertEventually(() => shell.CurrentItem.CurrentItem == changingTab, message: "Did not navigate to \"Changing\"");
			});
		}

		[Fact(DisplayName = "Tab With Several ShellContents Is Selected Again After Leaving TabBar Overflow Menu")]
		public async Task TabWithSeveralShellContentsIsSelectedAgainAfterLeavingTabBarOverflowMenu()
		{
			// https://github.com/dotnet/maui/issues/37985
			// Such a tab only expands while it sits in the overflow menu, back in the bar it gets selected like before
			var nestedTab = new Tab()
			{
				Title = "Nested",
				Items =
				{
					new ShellContent() { Title = "Sub A", Content = new ContentPage() },
					new ShellContent() { Title = "Sub B", Content = new ContentPage() }
				}
			};

			await RunTabBarOverflowMenuTest(nestedTab, async (shell, navView, overflowButton) =>
			{
				var nestedItem = await OpenOverflowMenuAndGetItem(navView, overflowButton, nestedTab);
				Assert.False(nestedItem.SelectsOnInvoked, "Clicking \"Nested\" in the overflow menu selects it");
				overflowButton.Flyout.Hide();

				// Wide enough for all three tabs, so "Nested" moves into the bar
				navView.Width = 400;
				WNavigationViewItem barItem = null;
				await AssertEventually(() => (barItem = GetNavigationViewItems(navView).FirstOrDefault(item => IsNavigationViewItemFor(item, nestedTab))) is not null, message: "\"Nested\" did not move into the tab bar");
				Assert.True(barItem.SelectsOnInvoked);

				ClickNavigationViewItem(barItem);
				await AssertEventually(() => shell.CurrentItem.CurrentItem == nestedTab, message: "Did not navigate to \"Nested\"");
			}, singleTabCount: 2, tabBarWidth: 150);
		}

		[Fact(DisplayName = "Tab With Several ShellContents Only Expands In New TabBar Overflow Menu After Template Is Applied Again")]
		public async Task TabWithSeveralShellContentsOnlyExpandsInNewTabBarOverflowMenuAfterTemplateIsAppliedAgain()
		{
			// https://github.com/dotnet/maui/issues/37985
			// Applying the template again creates a new overflow menu, the handler has to follow it
			var nestedTab = new Tab()
			{
				Title = "Nested",
				Items =
				{
					new ShellContent() { Title = "Sub A", Content = new ContentPage() },
					new ShellContent() { Title = "Sub B", Content = new ContentPage() }
				}
			};

			await RunTabBarOverflowMenuTest(nestedTab, async (shell, navView, overflowButton) =>
			{
				var nestedItem = await OpenOverflowMenuAndGetItem(navView, overflowButton, nestedTab);
				Assert.False(nestedItem.SelectsOnInvoked, "Clicking \"Nested\" in the overflow menu selects it");
				overflowButton.Flyout.Hide();
				await AssertEventually(() => !overflowButton.Flyout.IsOpen, message: "The overflow menu did not close");

				var template = navView.Template;
				navView.Template = null;
				navView.Template = template;
				navView.ApplyTemplate();

				WButton newOverflowButton = null;
				await AssertEventually(() => (newOverflowButton = navView.GetDescendantByName<WButton>("TopNavOverflowButton")) is not null && newOverflowButton != overflowButton && newOverflowButton.Visibility == UI.Xaml.Visibility.Visible, message: "The tab bar has no new overflow menu");

				nestedItem = await OpenOverflowMenuAndGetItem(navView, newOverflowButton, nestedTab);
				Assert.False(nestedItem.SelectsOnInvoked, "Clicking \"Nested\" in the new overflow menu selects it");

				ClickNavigationViewItem(nestedItem);

				// The tab only expands to show its sub tabs, the current page stays the same
				Assert.True(nestedItem.IsExpanded);
				Assert.Equal(shell.CurrentItem.Items[0], shell.CurrentItem.CurrentItem);
			});
		}

		// Tabs with one ShellContent each and then lastTab, in a tab bar narrow enough
		// to push the last tabs into the overflow menu
		async Task RunTabBarOverflowMenuTest(Tab lastTab, Func<Shell, MauiNavigationView, WButton, Task> runTest, int singleTabCount = 7, double tabBarWidth = 400)
		{
			SetupBuilder();

			var shell = await CreateShellAsync((shell) =>
			{
				var tabBar = new TabBar();
				for (int i = 1; i <= singleTabCount; i++)
				{
					tabBar.Items.Add(new Tab()
					{
						Title = $"Tab {i}",
						Items = { new ShellContent() { Content = new ContentPage() } }
					});
				}

				tabBar.Items.Add(lastTab);
				shell.Items.Add(tabBar);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var navView = shell.CurrentItem.Handler.PlatformView as MauiNavigationView;
				var overflowButton = navView.GetDescendantByName<WButton>("TopNavOverflowButton");

				navView.Width = tabBarWidth;
				await AssertEventually(() => overflowButton.Visibility == UI.Xaml.Visibility.Visible, message: "The tab bar has no overflow menu");

				try
				{
					await runTest(shell, navView, overflowButton);
				}
				finally
				{
					// The test can apply the template again, so look the button up again
					navView.GetDescendantByName<WButton>("TopNavOverflowButton")?.Flyout.Hide();
				}
			});
		}

		static async Task<WNavigationViewItem> OpenOverflowMenuAndGetItem(NavigationView navView, WButton overflowButton, BaseShellItem shellItem)
		{
			// Clicking a tab without sub tabs closes the menu, wait for that before opening it again
			await AssertEventually(() => !overflowButton.Flyout.IsOpen, message: "The overflow menu did not close");
			overflowButton.Flyout.ShowAt(overflowButton);

			WNavigationViewItem item = null;
			await AssertEventually(() => (item = GetOverflowMenuItem(navView, shellItem)) is not null, message: $"\"{shellItem.Title}\" is not in the overflow menu");
			return item;
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
