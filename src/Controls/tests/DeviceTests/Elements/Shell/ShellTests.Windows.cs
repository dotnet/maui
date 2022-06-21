using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : HandlerTestBase
	{
		protected Task CheckFlyoutState(ShellHandler handler, bool desiredState)
		{
			Assert.Equal(desiredState, handler.PlatformView.IsPaneOpen);
			return Task.CompletedTask;
		}

		[Fact(DisplayName = "Back Button Enabled/Disabled")]
		public async Task BackButtonEnabledAndDisabled()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				Assert.False(rootNavView.IsBackEnabled);
				await shell.Navigation.PushAsync(new ContentPage());
				Assert.True(rootNavView.IsBackEnabled);
				Shell.SetBackButtonBehavior(shell.CurrentPage,
					new BackButtonBehavior()
					{
						IsEnabled = false
					});
				Assert.False(rootNavView.IsBackEnabled);
			});
		}

		[Fact(DisplayName = "Shell with Single Content Page Has Panes Disabled")]
		public async Task BasicShellHasPaneDisplayModeDisabled()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				var shellItemView = shell.CurrentItem.Handler.PlatformView as UI.Xaml.Controls.NavigationView;
				var expected = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;

				Assert.False(rootNavView.IsPaneToggleButtonVisible);
				Assert.False(shellItemView.IsPaneToggleButtonVisible);

				Assert.Equal(expected, rootNavView.PaneDisplayMode);
				Assert.Equal(expected, shellItemView.PaneDisplayMode);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Shell With Only Flyout Items")]
		public async Task ShellWithOnlyFlyoutItems()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				var shellItem1 = new FlyoutItem();
				shellItem1.Items.Add(new ContentPage());

				var shellItem2 = new FlyoutItem();
				shellItem2.Items.Add(new ContentPage());

				shell.Items.Add(shellItem1);
				shell.Items.Add(shellItem2);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				var shellItemView = shell.CurrentItem.Handler.PlatformView as UI.Xaml.Controls.NavigationView;
				var expectedRoot = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
				var expectedShellItems = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;

				Assert.True(rootNavView.IsPaneToggleButtonVisible);
				Assert.False(shellItemView.IsPaneToggleButtonVisible);
				Assert.Equal(expectedRoot, rootNavView.PaneDisplayMode);
				Assert.Equal(expectedShellItems, shellItemView.PaneDisplayMode);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Shell With Only Top Tabs")]
		public async Task ShellWithOnlyTopTabs()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				var shellItem = new TabBar();
				var shellSection1 = new ShellSection();
				shellSection1.Items.Add(new ContentPage());

				var shellSection2 = new ShellSection();
				shellSection2.Items.Add(new ContentPage());

				shellItem.Items.Add(shellSection1);
				shellItem.Items.Add(shellSection2);

				shell.Items.Add(shellItem);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				var shellItemView = shell.CurrentItem.Handler.PlatformView as UI.Xaml.Controls.NavigationView;
				var expectedRoot = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
				var expectedShellItems = UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top;

				Assert.False(rootNavView.IsPaneToggleButtonVisible);
				Assert.False(shellItemView.IsPaneToggleButtonVisible);
				Assert.Equal(expectedRoot, rootNavView.PaneDisplayMode);
				Assert.Equal(expectedShellItems, shellItemView.PaneDisplayMode);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Flyout Locked Offset from AppTitleBar")]
		public async Task FlyoutLockedOffsetFromAppTitleBar()
		{
			SetupBuilder();
			var label = new StackLayout()
			{
				HeightRequest = 10
			};

			var shell = await InvokeOnMainThreadAsync(() =>
			{
				return new Shell()
				{
					FlyoutHeader = label,
					Items =
					{
						new ContentPage()
					},
					FlyoutBehavior = FlyoutBehavior.Locked
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var position = label.GetLocationRelativeTo(rootManager.AppTitleBar);
				var distance = rootManager.AppTitleBar.ActualHeight - position.Value.Y;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Flyout Locked Offsets Correctly from Content")]
		public async Task FlyoutLockedOffsetsCorrectlyFromContent()
		{
			SetupBuilder();
			var flyoutContent = new VerticalStackLayout()
			{
				HorizontalOptions = LayoutOptions.Fill
			};

			var contentPage = new ContentPage()
			{
				Content = new VerticalStackLayout()
			};

			var shell = await InvokeOnMainThreadAsync(() =>
			{
				return new Shell()
				{
					FlyoutContent = flyoutContent,
					CurrentItem = contentPage,
					FlyoutBehavior = FlyoutBehavior.Locked
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var position = contentPage.GetLocationRelativeTo(flyoutContent);

				var distance = position.Value.X - flyoutContent.Width;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Content Offsets Correctly From App Title Bar")]
		public async Task ContentOffsetsCorrectlyFromAppTitleBar()
		{
			SetupBuilder();
			var flyoutContent = new VerticalStackLayout()
			{
				HorizontalOptions = LayoutOptions.Fill
			};

			var contentPage = new ContentPage()
			{
				Content = new VerticalStackLayout()
			};

			var shell = await InvokeOnMainThreadAsync(() =>
			{
				return new Shell()
				{
					FlyoutContent = flyoutContent,
					CurrentItem = contentPage,
					FlyoutBehavior = FlyoutBehavior.Locked
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();

				var position = contentPage.GetLocationRelativeTo(rootManager.AppTitleBar);
				var distance = rootManager.AppTitleBar.ActualHeight - position.Value.Y;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Single Shell Section with Multiple Children")]
		public async Task SingleShellSectionWithMultipleChildren()
		{
			SetupBuilder();
			var page1 = new ContentPage();
			var page2 = new ContentPage();

			var shell = await CreateShellAsync((shell) =>
			{
				var shellSection1 = new ShellSection()
				{
					Items =
					{
						new ShellContent(){ Content = page1, Title = "Content 1"},
						new ShellContent(){ Content = page2, Title = "Content 2"},
					}
				};

				shell.Items.Add(shellSection1);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var navigationView =
					shell.CurrentItem.Handler.PlatformView as MauiNavigationView;

				// Ensure that top tabs are displayed because there are multiple child tabs
				Assert.Equal(UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);
				var navViewItem = navigationView.SelectedItem as NavigationViewItemViewModel;

				// Make sure the correct ShellContent is set as the selected item on NavigationView
				Assert.Equal(shell.CurrentContent.Title, (navViewItem.Data as ShellContent)?.Title);

				// Change to other tab
				shell.CurrentItem = shell.CurrentItem.CurrentItem.Items[1];
				await OnLoadedAsync(page2);

				// Ensure that top tabs are maintained
				Assert.Equal(UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);

				// Make sure the selected item is correctly updated
				navViewItem = navigationView.SelectedItem as NavigationViewItemViewModel;
				Assert.Equal(shell.CurrentContent.Title, (navViewItem.Data as ShellContent)?.Title);

			});
		}

		[Fact(DisplayName = "Single Tab Bar with Multiple Children")]
		public async Task SingleTabBarWithMultipleChildren()
		{
			SetupBuilder();
			var page1 = new ContentPage();
			var page2 = new ContentPage();

			var shell = await CreateShellAsync((shell) =>
			{
				var shellSection1 = new TabBar()
				{
					Items =
					{
						new ShellContent() { Content = page1, Title = "Content 1"},
						new ShellContent() { Content = page2, Title = "Content 2"},
					}
				};

				shell.Items.Add(shellSection1);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var navigationView =
					shell.CurrentItem.Handler.PlatformView as MauiNavigationView;

				var menuItems = navigationView.MenuItemsSource as IList<NavigationViewItemViewModel>;
				Assert.Equal(2, menuItems.Count);

				// Ensure that top tabs are displayed because there are multiple child tabs
				Assert.Equal(UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);
				var navViewItem = navigationView.SelectedItem as NavigationViewItemViewModel;

				// Make sure the correct ShellContent is set as the selected item on NavigationView
				Assert.Equal(shell.CurrentContent.Title, (navViewItem.Data as ShellContent)?.Title);

				// Change to other tab
				shell.CurrentItem = shell.CurrentItem.Items[1].CurrentItem;
				await OnLoadedAsync(page2);

				// Ensure that top tabs are maintained
				Assert.Equal(UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);

				// Make sure the selected item is correctly updated
				navViewItem = navigationView.SelectedItem as NavigationViewItemViewModel;
				Assert.Equal(shell.CurrentContent.Title, (navViewItem.Data as ShellContent)?.Title);

			});
		}


		[Fact(DisplayName = "Selected Item On ShellView Correct With Implict Flyout Item")]
		public async Task SelectedItemOnShellViewCorrectWithImplictFlyoutItem()
		{
			SetupBuilder();
			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var page3 = new ContentPage();

			var flyoutItem1 = new FlyoutItem()
			{
				Items =
				{
					new ShellContent() { Content = page1, Title = "Content 1"},
				}
			};

			var flyoutItem2ShellSection = new ShellSection()
			{
				Items =
				{
					new ShellContent() { Content = page2, Title = "Content 2"},
				}
			};


			var flyoutItem3ShellContent = new ShellContent()
			{
				Content = page3,
				Title = "Content 3"
			};


			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(flyoutItem1);
				shell.Items.Add(flyoutItem2ShellSection);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var navigationView =
					handler.PlatformView as MauiNavigationView;

				var menuItems = navigationView.MenuItemsSource as IList<NavigationViewItemViewModel>;

				Assert.Equal(flyoutItem1, navigationView.SelectedItem);

				// Switch to Shell Section 
				shell.CurrentItem = flyoutItem2ShellSection;
				await OnLoadedAsync(page2);
				Assert.Equal(flyoutItem2ShellSection, navigationView.SelectedItem);

				// Switch to Shell Content 
				shell.CurrentItem = flyoutItem3ShellContent;
				await OnLoadedAsync(page3);
				Assert.Equal(flyoutItem3ShellContent, navigationView.SelectedItem);
			});
		}

		[Fact(DisplayName = "Shell Toolbar With Only MenuBarItems Is Visible")]
		public async Task ShellToolbarWithOnlyMenuBarItemsIsVisible()
		{
			SetupBuilder();

			var shell = await CreateShellAsync((shell) =>
			{
				var contentPage = new ContentPage();
				var menuFlyoutItem = new MenuFlyoutItem { Text = "Test" };
				var menuBarItem = new MenuBarItem { Text = "File" };
				menuBarItem.Add(menuFlyoutItem);
				contentPage.MenuBarItems.Add(menuBarItem);

				shell.Items.Add(contentPage);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				Assert.True(IsNavigationBarVisible(handler));

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Shell Toolbar With Only ToolbarItems Is Visible")]
		public async Task ShellToolbarWithOnlyToolbarItemsIsVisible()
		{
			SetupBuilder();

			var shell = await CreateShellAsync((shell) =>
			{
				var contentPage = new ContentPage();
				var toolbarItem = new ToolbarItem { Text = "Test" };
				contentPage.ToolbarItems.Add(toolbarItem);

				shell.Items.Add(contentPage);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				Assert.True(IsNavigationBarVisible(handler));

				return Task.CompletedTask;
			});
		}


		// this is only relevant on windows where the title/backbutton aren't in the same
		// area
		[Fact(DisplayName = "Shell Toolbar not visible when only back button is present")]
		public async Task ShellToolbarNotVisibleWhenOnlyBackButtonIsPresent()
		{
			SetupBuilder();

			var shell = await CreateShellAsync((shell) =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await shell.Navigation.PushAsync(new ContentPage());
				Assert.False(IsNavigationBarVisible(handler));
			});
		}
	}
}
