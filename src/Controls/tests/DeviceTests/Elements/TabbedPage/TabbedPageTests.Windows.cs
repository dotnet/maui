using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using WContentPresenter = Microsoft.UI.Xaml.Controls.ContentPresenter;
using WFrame = Microsoft.UI.Xaml.Controls.Frame;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WPage = Microsoft.UI.Xaml.Controls.Page;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Toolbar Visible When Pushing To TabbedPage")]
		public async Task ToolbarVisibleWhenPushingToTabbedPage()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				// When the current active page is a TabbedPage then
				// we put to toolbar inside the PaneFooter so it's
				// to the right of the tabs

				await navPage.PushAsync(CreateBasicTabbedPage());
				var navView = GetMauiNavigationView(handler.MauiContext);
				var header = (WFrameworkElement)navView.PaneFooter;
				Assert.NotNull(header);
				Assert.True(header.ActualHeight > 0);
				Assert.True(header.ActualWidth > 0);
				await navPage.PopAsync();
				header = (WFrameworkElement)navView.Header;
				Assert.NotNull(header);
				Assert.True(header.ActualHeight > 0);
				Assert.True(header.ActualWidth > 0);
				await navPage.PushAsync(CreateBasicTabbedPage());
				header = (WFrameworkElement)navView.PaneFooter;
				Assert.NotNull(header);
				Assert.True(header.ActualHeight > 0);
				Assert.True(header.ActualWidth > 0);
			});
		}

		[Fact(DisplayName = "TabbedPage Disconnects")]
		public async Task TabbedViewHandlerDisconnects()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage();

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, (handler) =>
			{
				// Validate that no exceptions are thrown
				((IElementHandler)handler).DisconnectHandler();
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Swapping Root Window Content for New Tabbed Page")]
		public async Task SwapWindowContentForNewTabbedPage()
		{
			SetupBuilder();
			var window = new Window()
			{
				Page = CreateBasicTabbedPage()
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async windowHandler =>
			{
				window.Page.Handler.DisconnectHandler();

				// Swap out main page 
				window.Page = CreateBasicTabbedPage();

				// wait for new handler to finish loading
				await ((IPlatformViewHandler)window.Page.Handler).PlatformView.OnLoadedAsync();
				var navView = GetMauiNavigationView(window.Page.Handler.MauiContext);

				// make sure root view is displaying as top tabs
				Assert.Equal(UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top, navView.PaneDisplayMode);
			});
		}

		[Fact(DisplayName = "Tab Title")]
		public async Task TabTitle()
		{
			SetupBuilder();
			await CreateHandlerAndAddToWindow<TabbedViewHandler>(CreateBasicTabbedPage(), handler =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				var navItem = GetNavigationViewItems(navView).ToList()[0];
				Assert.Equal("Page 1", navItem.Content);
				(handler.VirtualView as TabbedPage).Children[0].Title = "New Page Name";
				Assert.Equal("New Page Name", navItem.Content);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Adding and Removing Pages Propagates Correctly")]
		public async Task AddingAndRemovingPagesPropagatesCorrectly()
		{
			SetupBuilder();
			await CreateHandlerAndAddToWindow<TabbedViewHandler>(CreateBasicTabbedPage(), async handler =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				var items = GetNavigationViewItems(navView).ToList();
				Assert.Single(items);
				(handler.VirtualView as TabbedPage).Children.Add(new ContentPage());

				// Wait for the navitem to propagate
				await Task.Delay(100);
				items = GetNavigationViewItems(navView).ToList();
				Assert.Equal(2, items.Count);
				(handler.VirtualView as TabbedPage).Children.RemoveAt(1);

				// Wait for the navitem to propagate
				await Task.Delay(100);
				items = GetNavigationViewItems(navView).ToList();
				Assert.Single(items);
			});
		}

		[Fact(DisplayName = "Selected Item Changed Propagates to CurrentPage")]
		public async Task SelectedItemChangedPropagatesToCurrentPage()
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.Children.Add(new ContentPage());



			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				var secondItem = (navView.MenuItemsSource as IEnumerable<NavigationViewItemViewModel>).Skip(1).FirstOrDefault();
				navView.SelectedItem = secondItem;

				Assert.Equal(tabbedPage.CurrentPage, tabbedPage.Children[1]);
				return Task.CompletedTask;
			});
		}

		MauiNavigationView GetMauiNavigationView(TabbedPage tabbedPage)
		{
			return (tabbedPage.Handler as IPlatformViewHandler)
				.PlatformView
				.GetParentOfType<MauiNavigationView>();
		}

		async Task ValidateTabBarIconColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(
					GetMauiNavigationView(tabbedPage),
					tabText, iconColor, tabbedPage.FindMauiContext());
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(
					GetMauiNavigationView(tabbedPage),
					tabText, iconColor, tabbedPage.FindMauiContext());
			}
		}

		async Task ValidateTabBarTextColor(
			TabbedPage tabbedPage,
			string tabText,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(
					GetMauiNavigationView(tabbedPage),
					tabText, iconColor, tabbedPage.FindMauiContext());
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(
					GetMauiNavigationView(tabbedPage),
					tabText, iconColor, tabbedPage.FindMauiContext());
			}
		}

		[Fact(DisplayName = "Issue 32824 - Tab Switch Clears Old Content To Prevent Crash")]
		public async Task TabSwitchClearsOldContentToPreventCrash()
		{
			// https://github.com/dotnet/maui/issues/32824
			// When switching tabs, the old ContentPresenter.Content must be cleared
			// before navigation to prevent "Element is already the child of another element" crash.
			SetupBuilder();

			var page1 = new ContentPage { Title = "Tab 1", Content = new Label { Text = "Page 1" } };
			var page2 = new ContentPage { Title = "Tab 2", Content = new Label { Text = "Page 2" } };
			var tabbedPage = new TabbedPage { Children = { page1, page2 } };

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var frame = typeof(TabbedPage)
					.GetField("_navigationFrame", BindingFlags.NonPublic | BindingFlags.Instance)
					?.GetValue(tabbedPage) as WFrame;

				Assert.NotNull(frame);

				var oldPresenter = (frame.Content as WPage)?.Content as WContentPresenter;
				Assert.NotNull(oldPresenter);
				Assert.NotNull(oldPresenter.Content);

				// Switch tabs - oldPresenter.Content should be cleared before navigation
				tabbedPage.CurrentPage = page2;

				//old presenter content must be null to prevent crash
				Assert.Null(oldPresenter.Content);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Issue 26214 - Nested TabbedPage Back Navigation Keeps Tabs Intact")]
		public async Task NestedTabbedPageBackNavigationKeepsTabsIntact()
		{
			// https://github.com/dotnet/maui/issues/26214
			// TabbedPages pushed onto a NavigationPage share the same root NavigationView.
			// Popping back after switching tabs must not let the hidden TabbedPage steal
			// the visible page's selection, and the tab bar must show the revealed page's tabs.
			SetupBuilder();

			var mainTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Home Tab 1" },
				new ContentPage() { Title = "Home Tab 2" }
			});
			var productTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Product Tab 1" },
				new ContentPage() { Title = "Product Tab 2" }
			});
			var navPage = new NavigationPage(mainTabbedPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);

				await navPage.PushAsync(productTabbedPage);
				productTabbedPage.CurrentPage = productTabbedPage.Children[1];
				await navPage.PopAsync();

				// The popped page must not have adopted the revealed page's tab
				Assert.Equal(productTabbedPage.Children[1], productTabbedPage.CurrentPage);
				Assert.DoesNotContain(mainTabbedPage.Children[0], productTabbedPage.Children);

				// The shared tab bar must be showing this page's tabs with its selection restored
				var items = (navView.MenuItemsSource as IEnumerable<NavigationViewItemViewModel>).ToList();
				Assert.Equal(mainTabbedPage.Children.Count, items.Count);
				Assert.All(items, item => Assert.Contains(item.Data, mainTabbedPage.Children));
				Assert.Equal(mainTabbedPage.CurrentPage, (navView.SelectedItem as NavigationViewItemViewModel)?.Data);
			});
		}

		[Fact(DisplayName = "Issue 26214 - ContentPage Push Pop Keeps TabbedPage Intact")]
		public async Task ContentPagePushPopKeepsTabbedPageIntact()
		{
			// A ContentPage pushed over a TabbedPage must not disturb the shared tab bar:
			// popping back restores this page's tabs and selection.
			SetupBuilder();

			var mainTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Home Tab 1" },
				new ContentPage() { Title = "Home Tab 2" }
			});
			var navPage = new NavigationPage(mainTabbedPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);

				await navPage.PushAsync(new ContentPage() { Title = "Overlay" });
				mainTabbedPage.CurrentPage = mainTabbedPage.Children[1];
				await navPage.PopAsync();

				var items = (navView.MenuItemsSource as IEnumerable<NavigationViewItemViewModel>).ToList();
				Assert.Equal(mainTabbedPage.Children.Count, items.Count);
				Assert.All(items, item => Assert.Contains(item.Data, mainTabbedPage.Children));
				Assert.Equal(mainTabbedPage.CurrentPage, (navView.SelectedItem as NavigationViewItemViewModel)?.Data);
			});
		}

		[Fact(DisplayName = "Issue 26214 - Revealed TabbedPage Switches Tabs After Pop")]
		public async Task RevealedTabbedPageSwitchesTabsAfterPop()
		{
			// After popping back, the revealed TabbedPage must be fully interactive and
			// the popped page must not observe its tab switches.
			SetupBuilder();

			var mainTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Home Tab 1" },
				new ContentPage() { Title = "Home Tab 2" }
			});
			var productTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Product Tab 1" },
				new ContentPage() { Title = "Product Tab 2" }
			});
			var navPage = new NavigationPage(mainTabbedPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);

				await navPage.PushAsync(productTabbedPage);
				productTabbedPage.CurrentPage = productTabbedPage.Children[1];
				await navPage.PopAsync();

				mainTabbedPage.CurrentPage = mainTabbedPage.Children[1];

				Assert.Equal(mainTabbedPage.Children[1], mainTabbedPage.CurrentPage);
				Assert.Equal(mainTabbedPage.Children[1], (navView.SelectedItem as NavigationViewItemViewModel)?.Data);
				Assert.Equal(productTabbedPage.Children[1], productTabbedPage.CurrentPage);
			});
		}

		[Fact(DisplayName = "Issue 26214 - Empty TabbedPage Pops Cleanly")]
		public async Task EmptyTabbedPagePopsCleanly()
		{
			// A TabbedPage with no children owns no tab items, so its teardown must
			// neither throw nor leave another page's tabs behind.
			SetupBuilder();

			var mainTabbedPage = new TabbedPage();
			var productTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Product Tab 1" },
				new ContentPage() { Title = "Product Tab 2" }
			});
			var navPage = new NavigationPage(mainTabbedPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);

				await navPage.PushAsync(productTabbedPage);
				productTabbedPage.CurrentPage = productTabbedPage.Children[1];
				await navPage.PopAsync();

				var items = (navView.MenuItemsSource as IEnumerable<NavigationViewItemViewModel>).ToList();
				Assert.Empty(items);
				Assert.Null(navView.SelectedItem);
			});
		}

		[Fact(DisplayName = "Issue 26214 - Disconnect Then Pop Restores Tabs")]
		public async Task DisconnectThenPopRestoresTabs()
		{
			// If the pushed page's handler is torn down before the pop (order
			// variation), the pop's Appearing pass must still restore this page's tabs.
			SetupBuilder();

			var mainTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Home Tab 1" },
				new ContentPage() { Title = "Home Tab 2" }
			});
			var productTabbedPage = CreateBasicTabbedPage(pages: new[]
			{
				new ContentPage() { Title = "Product Tab 1" },
				new ContentPage() { Title = "Product Tab 2" }
			});
			var navPage = new NavigationPage(mainTabbedPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);

				await navPage.PushAsync(productTabbedPage);
				productTabbedPage.CurrentPage = productTabbedPage.Children[1];

				((IElementHandler)productTabbedPage.Handler).DisconnectHandler();
				Assert.Null(navView.SelectedItem);

				await navPage.PopAsync();

				var items = (navView.MenuItemsSource as IEnumerable<NavigationViewItemViewModel>).ToList();
				Assert.Equal(mainTabbedPage.Children.Count, items.Count);
				Assert.All(items, item => Assert.Contains(item.Data, mainTabbedPage.Children));
				Assert.Equal(mainTabbedPage.CurrentPage, (navView.SelectedItem as NavigationViewItemViewModel)?.Data);
			});
		}
	}
}
