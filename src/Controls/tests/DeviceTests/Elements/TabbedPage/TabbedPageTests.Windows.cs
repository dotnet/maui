using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
				});
			});
		}

		[Fact(DisplayName = "Header Visible When Pushing To TabbedPage")]
		public async Task HeaderVisibleWhenPushingToTabbedPage()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
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

		[Fact(DisplayName = "BarBackground Color")]
		public async Task BarBackgroundColor()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.BarBackground = SolidColorBrush.Purple;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), (handler) =>
			{
				var navView = GetMauiNavigationView(tabbedPage.Handler.MauiContext);
				var platformBrush = (WSolidColorBrush)((Paint)tabbedPage.BarBackground).ToPlatform();
				Assert.Equal(platformBrush.Color, ((WSolidColorBrush)navView.TopNavArea.Background).Color);
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


		[Fact(DisplayName = "Bar Text Color")]
		public async Task BarTextColor()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.BarTextColor = Colors.Red;
			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				var navItem = GetNavigationViewItems(navView).ToList()[0];

				Assert.Equal(Colors.Red, ((WSolidColorBrush)navItem.Foreground).ToColor());
				tabbedPage.BarTextColor = Colors.Blue;
				Assert.Equal(Colors.Blue, ((WSolidColorBrush)navItem.Foreground).ToColor());

				return Task.CompletedTask;
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

		[Fact(DisplayName = "Selected/Unselected Color")]
		public async Task SelectedAndUnselectedTabColor()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.Children.Add(new ContentPage() { Title = "Page 2" });

			tabbedPage.SelectedTabColor = Colors.Red;
			tabbedPage.UnselectedTabColor = Colors.Purple;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, handler =>
			{
				var navView = GetMauiNavigationView(handler.MauiContext);
				var navItem1 = GetNavigationViewItems(navView).ToList()[0];
				var navItem2 = GetNavigationViewItems(navView).ToList()[1];

				Assert.Equal(Colors.Red, ((WSolidColorBrush)navItem1.Background).ToColor());
				Assert.Equal(Colors.Purple, ((WSolidColorBrush)navItem2.Background).ToColor());

				tabbedPage.CurrentPage = tabbedPage.Children[1];

				Assert.Equal(Colors.Purple, ((WSolidColorBrush)navItem1.Background).ToColor());
				Assert.Equal(Colors.Red, ((WSolidColorBrush)navItem2.Background).ToColor());

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


		TabbedPage CreateBasicTabbedPage()
		{
			return new TabbedPage()
			{
				Title = "Tabbed Page",
				Children =
				{
					new ContentPage() { Title = "Page 1" }
				}
			};
		}
	}
}
