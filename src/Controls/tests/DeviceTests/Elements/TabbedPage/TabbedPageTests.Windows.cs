using System;
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
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WWindow = Microsoft.UI.Xaml.Window;
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
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedPageHandler));
					handlers.AddHandler(typeof(Controls.Window), typeof(WindowHandler));
					handlers.AddHandler<Page, PageHandler>();
				});
			});
		}

		[Fact(DisplayName = "TabbedPage Disconnects")]
		public async Task TabbedPageHandlerDisconnects()
		{
			var tabbedPage = CreateBasicTabbedPage();

			await CreateHandlerAndAddToWindow<TabbedPageHandler>(tabbedPage, (handler) =>
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

			await CreateHandlerAndAddToWindow<WindowHandler>(new Window(tabbedPage), (handler) =>
			{
				var navView = GetMauiNavigationView(tabbedPage.Handler.MauiContext);
				var platformBrush = (WSolidColorBrush)((Paint)tabbedPage.BarBackground).ToNative();
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

			await CreateHandlerAndAddToWindow<WindowHandler>(window, async windowHandler =>
			{
				window.Page.Handler.DisconnectHandler();

				// Swap out main page 
				window.Page = CreateBasicTabbedPage();

				// wait for new handler to finish loading
				await ((INativeViewHandler)window.Page.Handler).NativeView.LoadedAsync();
				var navView = GetMauiNavigationView(window.Page.Handler.MauiContext);

				// make sure root view is displaying as top tabs
				Assert.Equal(UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top, navView.PaneDisplayMode);
			});
		}

		TabbedPage CreateBasicTabbedPage()
		{
			return new TabbedPage()
			{
				Children =
					{
						new ContentPage()
					}
			};
		}
	}
}
