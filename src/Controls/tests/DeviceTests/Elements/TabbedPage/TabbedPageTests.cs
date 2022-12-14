using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.TabbedPage)]
	public partial class TabbedPageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler<Page, PageHandler>();

#if IOS || MACCATALYST
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
#else
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
#endif
				});
			});
		}

		[Fact]
		public async Task PoppingTabbedPageDoesntCrash()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(CreateBasicTabbedPage());
				await navPage.PopAsync();
			});
		}

		[Theory]
#if ANDROID
		[InlineData(true)]
#endif
		[InlineData(false)]
		public async Task NavigatingAwayFromTabbedPageResizesContentPage(bool bottomTabs)
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage(bottomTabs);
			var navPage = new NavigationPage(tabbedPage);

			var layout1 = new VerticalStackLayout()
			{
				Background = SolidColorBrush.Green
			};

			(tabbedPage.CurrentPage as ContentPage).Content = layout1;
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnFrameSetToNotEmpty(layout1);
				var pageHeight = tabbedPage.CurrentPage.Height;
				var newPage = new ContentPage()
				{
					Background = SolidColorBrush.Purple,
					Content = new VerticalStackLayout()
				};

				await navPage.PushAsync(newPage);
				await OnFrameSetToNotEmpty(newPage);

				Assert.True(newPage.Content.Height > pageHeight);
			});
		}

		TabbedPage CreateBasicTabbedPage(bool bottomTabs = false, bool isSmoothScrollEnabled = true, IEnumerable<Page> pages = null)
		{
			pages = pages ?? new List<Page>()
			{
				new ContentPage() { Title = "Page 1" }
			};

			var tabs = new TabbedPage()
			{
				Title = "Tabbed Page"
			};

			foreach (var page in pages)
			{
				tabs.Children.Add(page);
			}

			if (bottomTabs)
			{
				Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(tabs, Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
			}
			else
			{
				Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(tabs,
					Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Top);
			}

			Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetIsSmoothScrollEnabled(tabs, isSmoothScrollEnabled);
			return tabs;
		}
	}
}
