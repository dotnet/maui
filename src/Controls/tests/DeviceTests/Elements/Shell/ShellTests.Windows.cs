using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : ControlsHandlerTestBase
	{
		protected Task CheckFlyoutState(ShellHandler handler, bool desiredState)
		{
			Assert.Equal(desiredState, handler.PlatformView.IsPaneOpen);
			return Task.CompletedTask;
		}

		[Fact(DisplayName = "Shell Title Updates Correctly")]
		public async Task ShellTitleUpdatesCorrectly()
		{
			SetupBuilder();

			var page1 = new ContentPage()
			{ Content = new Label() { Text = "Page 1" }, Title = "Page 1" };
			var page2 = new ContentPage()
			{ Content = new Label() { Text = "Page 2" }, Title = "Page 2" };

			var shell = await CreateShellAsync((shell) =>
			{
				var tabBar = new TabBar()
				{
					Items =
					{
						new ShellContent(){ Content = page1 },
						new ShellContent(){ Content = page2 },
					}
				};

				shell.Items.Add(tabBar);
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Controls.Window(shell), (handler) =>
			{
				Assert.Equal("Page 1", GetToolbarTitle(handler));

				string newTitle = "New Page 1";
				page1.Title = newTitle;
				Assert.Equal(newTitle, GetToolbarTitle(handler));
			});
		}

		static bool IsViewLaidOut(object platformView)
		{
			var frameworkElement = platformView as WFrameworkElement;
			return frameworkElement is not null && (frameworkElement.ActualHeight > 0 || frameworkElement.ActualWidth > 0);
		}

		[Fact(DisplayName = "Shell FlyoutIcon Initializes Correctly")]
		public async Task ShellFlyoutIconInitializesCorrectly()
		{
			SetupBuilder();

			var shell = await CreateShellAsync((shell) =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				shell.FlyoutIcon = "red.png";

				var shellItem = new FlyoutItem();
				shellItem.Items.Add(new ContentPage());
				shell.Items.Add(shellItem);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var rootNavView = handler.PlatformView;
				var shellItemView = shell.CurrentItem.Handler.PlatformView as MauiNavigationView;

				Assert.NotNull(shellItemView);

				await AssertEventually(() => IsViewLaidOut(shell.Handler.PlatformView));

				var animatedIcon = GetNativeAnimatedIcon(handler);
				Assert.NotNull(animatedIcon);
				Assert.NotNull(animatedIcon.FallbackIconSource);
			});
		}

		[Theory(DisplayName = "Shell FlyoutBackground Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		[InlineData("#000000")]
		public async Task ShellFlyoutBackgroundInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Color.FromArgb(colorHex);

			var shell = await CreateShellAsync((shell) =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.FlyoutBackground = new SolidColorBrush(expectedColor);

				var shellItem = new FlyoutItem();
				shellItem.Items.Add(new ContentPage());
				shell.Items.Add(shellItem);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var rootNavView = handler.PlatformView;
				var shellItemView = shell.CurrentItem.Handler.PlatformView as MauiNavigationView;
				var expectedRoot = UI.Xaml.Controls.NavigationViewPaneDisplayMode.Left;
				var expectedShellItems = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;

				Assert.Equal(expectedRoot, rootNavView.PaneDisplayMode);
				Assert.NotNull(shellItemView);
				Assert.Equal(expectedShellItems, shellItemView.PaneDisplayMode);

				await AssertEventually(() => IsViewLaidOut(shell.Handler.PlatformView));

				await ValidateHasColor(shell, expectedColor, typeof(ShellHandler));
			});
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
			var label = new Controls.StackLayout()
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

		[Fact(DisplayName = "Shell Has Correct Item Count")]
		public async Task ShellContentHasCorrectItemCount()
		{
			SetupBuilder();

			var content1 = new ShellContent();
			content1.Title = "Hello";
			content1.Route = $"...";
			content1.Content = new ContentPage();

			var content2 = new ShellContent();
			content2.Title = "World";
			content2.Route = $"...";
			content2.Content = new ContentPage();

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(content1);
				shell.Items.Add(content2);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				handler.PlatformView.UpdateMenuItemSource();

				var items = handler.PlatformView.MenuItemsSource as ObservableCollection<object>;
				Assert.True(items.Count == 2);
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

				Assert.Equal(flyoutItem1, (navigationView.SelectedItem as NavigationViewItemViewModel).Data);

				// Switch to Shell Section 
				shell.CurrentItem = flyoutItem2ShellSection;
				await OnLoadedAsync(page2);
				Assert.Equal(flyoutItem2ShellSection, (navigationView.SelectedItem as NavigationViewItemViewModel).Data);

				// Switch to Shell Content 
				shell.CurrentItem = flyoutItem3ShellContent;
				await OnLoadedAsync(page3);
				Assert.Equal(flyoutItem3ShellContent, (navigationView.SelectedItem as NavigationViewItemViewModel).Data);
			});
		}

		[Fact(DisplayName = "Shell Toolbar With Only MenuBarItems Is Visible")]
		public async Task ShellToolbarWithOnlyMenuBarItemsIsVisible()
		{
			SetupBuilder();

			var shell = await CreateShellAsync((shell) =>
			{
				var contentPage = new ContentPage();
				var menuFlyoutItem = new Controls.MenuFlyoutItem { Text = "Test" };
				var menuBarItem = new Controls.MenuBarItem { Text = "File" };
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

		[Fact]
		public async Task EmptyShellHasNoTopMargin()
		{
			SetupBuilder();

			var mainPage = new ContentPage();
			var shell = new Shell() { CurrentItem = mainPage };

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await AssertEventually(() => mainPage.ToPlatform().GetLocationOnScreen().Value.Y > 0);
				var appTitleBarHeight = GetWindowRootView(handler).AppTitleBarActualHeight;
				var position = mainPage.ToPlatform().GetLocationOnScreen();

				Assert.True(Math.Abs(position.Value.Y - appTitleBarHeight) < 1);
			});
		}

		protected async Task OpenFlyout(ShellHandler shellRenderer, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);

			var navView = (shellRenderer.PlatformView as ShellView);

			if (navView.IsPaneOpen)
				return;

			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			navView.PaneOpened += OnPaneOpened;
			navView.IsPaneOpen = true;

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);

			void OnPaneOpened(NavigationView sender, object args)
			{
				navView.PaneOpened -= OnPaneOpened;
				taskCompletionSource.SetResult(true);
			}
		}

		internal Graphics.Rect GetFrameRelativeToFlyout(ShellHandler handler, IView view)
		{
			var shellView = handler.PlatformView as ShellView;
			var flyoutContainer = shellView.RootSplitView.FindName("PaneContentGrid") as UIElement;
			var titleBar = handler.MauiContext.GetNavigationRootManager().AppTitleBar;
			var platformView = view.Handler.PlatformView as FrameworkElement;
			var point = platformView.GetLocationRelativeTo(flyoutContainer);

			// We subtract the titlebar height because the PaneContentGrid extends into the titlebar area but
			// our flyout content is already offset from the title bar
			return new Graphics.Rect(point.Value.X, point.Value.Y - titleBar.ActualHeight, platformView.ActualWidth, platformView.ActualHeight);
		}

		internal Graphics.Rect GetFlyoutFrame(ShellHandler shellRenderer)
		{
			throw new NotImplementedException();
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

		[Fact]
		public async Task SelectingTabUpdatesSelectedFlyoutItem()
		{
			SetupBuilder();

			var flyoutItem = new FlyoutItem()
			{
				FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
				Items =
				{
					new ShellContent()
					{
						Content = new ContentPage()
					}
				}
			};

			var tabItems = new Tab()
			{
				Items =
				{
					new ShellContent()
					{
						Content = new ContentPage()
					},
					new ShellContent()
					{
						Content = new ContentPage()
					}
				}
			};

			var shell = await CreateShellAsync((shell) =>
			{
				flyoutItem.Items.Add(tabItems);
				shell.Items.Add(flyoutItem);
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var flyoutItems = shell.FlyoutItems.Cast<IReadOnlyList<Element>>().ToList();
				var rootView = handler.PlatformView as MauiNavigationView;
				var tabbedView = (flyoutItem.Handler.PlatformView as MauiNavigationView);

				var platformTabItems = tabbedView.MenuItemsSource as IList<NavigationViewItemViewModel>;
				var platformFlyoutItems = rootView.MenuItemsSource as IList<object>;

				var selectedTabItem = tabbedView.SelectedItem as NavigationViewItemViewModel;

				// check that the initial flyout item is selected
				Assert.Equal((rootView.SelectedItem as NavigationViewItemViewModel).Data, flyoutItems[0][0]);
				Assert.Equal(selectedTabItem.Data, flyoutItem.Items[0].Items[0]);

				tabbedView.SelectedItem = platformTabItems[1].MenuItemsSource[1];

				// Wait for the selected item to propagate to the rootview
				await AssertEventually(() => (rootView.SelectedItem as NavigationViewItemViewModel).Data == flyoutItems[0][1]);

				// Verify that the flyout item updates
				Assert.Equal((rootView.SelectedItem as NavigationViewItemViewModel).Data, flyoutItems[0][1]);
			});
		}

		async Task TapToSelect(ContentPage page)
		{
			var shellContent = page.Parent as ShellContent;
			var shellSection = shellContent.Parent as ShellSection;
			var shellItem = shellSection.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;

			await OnNavigatedToAsync(shell.CurrentPage);

			if (shellItem != shell.CurrentItem)
				throw new NotImplementedException();

			if (shellSection != shell.CurrentItem.CurrentItem)
				throw new NotImplementedException();

			var mauiNavigationView = shellItem.Handler.PlatformView as MauiNavigationView;
			var navSource = mauiNavigationView.MenuItemsSource as IEnumerable;

			bool found = false;
			foreach (NavigationViewItemViewModel item in navSource)
			{
				if (item.Data == shellContent)
				{
					mauiNavigationView.SelectedItem = item;
					found = true;
					break;
				}
				else if (item.MenuItemsSource is IEnumerable children)
				{
					foreach (NavigationViewItemViewModel childContent in children)
					{
						if (childContent.Data == shellContent)
						{
							mauiNavigationView.SelectedItem = childContent;
							found = true;
							break;
						}
					}
				}

				if (found)
					break;
			}

			if (!found)
				throw new InvalidOperationException("Unable to locate page inside platform shell components");

			await OnNavigatedToAsync(page);
		}

		MauiNavigationView GetNavigationView(ShellHandler shellHandler) =>
			shellHandler.PlatformView;

		AnimatedIcon GetNativeAnimatedIcon(ShellHandler shellHandler)
		{
			var mauiNavigationView = GetNavigationView(shellHandler);

			var togglePaneButton = mauiNavigationView.TogglePaneButton;

			if (togglePaneButton is null)
				return null;

			var animatedIcon = togglePaneButton.GetFirstDescendant<AnimatedIcon>();

			return animatedIcon;
		}
	}
}
