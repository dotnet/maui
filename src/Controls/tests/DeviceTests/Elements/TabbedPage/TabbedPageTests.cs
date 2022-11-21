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

		[Theory]
#if ANDROID
		[InlineData(true)]
#endif
		[InlineData(false)]
		public async Task PoppingTabbedPageDoesntCrash(bool bottomTabs)
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(CreateBasicTabbedPage(bottomTabs));
				await navPage.PopAsync();
			});
		}

		[Theory("Remove CurrentPage And Then Re-Add Doesnt Crash")]
#if ANDROID
		[InlineData(true)] // Only android has this pivot
#endif
		[InlineData(false)]
		public async Task RemoveCurrentPageAndThenReAddDoesntCrash(bool bottomTabs)
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage(bottomTabs);

			var firstPage = new NavigationPage(new ContentPage());
			tabbedPage.Children.Insert(0, firstPage);
			tabbedPage.CurrentPage = firstPage;
			var secondPage = tabbedPage.Children[1];

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				await OnNavigatedToAsync(firstPage);
				tabbedPage.Children.Remove(firstPage);
				await OnNavigatedToAsync(secondPage);

				// Validate that the second page becomes the current active page
				Assert.Equal(secondPage, tabbedPage.CurrentPage);

				// add the removed page back
				tabbedPage.Children.Insert(0, firstPage);

				// Validate that the second page is still the current active page
				Assert.Equal(secondPage, tabbedPage.CurrentPage);

				// Validate that we can navigate back to the first page
				tabbedPage.CurrentPage = firstPage;
				await OnNavigatedToAsync(firstPage);
			});
		}

		[Theory]
#if ANDROID
		[InlineData(true)]
#endif
		[InlineData(false)]
		public async Task SettingCurrentPageToNotBePositionZeroWorks(bool bottomTabs)
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(bottomTabs);
			var firstPage = new NavigationPage(new ContentPage());
			tabbedPage.Children.Insert(0, firstPage);
			var secondPage = tabbedPage.Children[1];
			tabbedPage.CurrentPage = secondPage;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				await OnNavigatedToAsync(secondPage);
				Assert.Equal(tabbedPage.CurrentPage, secondPage);
			});
		}

#if !WINDOWS
		[Theory]
#if ANDROID
		[InlineData(true)]
#endif
		[InlineData(false)]
		public async Task RemovingAllPagesDoesntCrash(bool bottomTabs)
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(bottomTabs);
			var secondPage = new NavigationPage(new ContentPage()) { Title = "Second Page" };
			tabbedPage.Children.Add(secondPage);
			var firstPage = tabbedPage.Children[0];

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				await OnNavigatedToAsync(firstPage);

				tabbedPage.Children.Remove(firstPage);
				tabbedPage.Children.Remove(secondPage);

				await OnUnloadedAsync(secondPage);
				tabbedPage.Children.Insert(0, secondPage);
				await OnNavigatedToAsync(secondPage);

				Assert.Equal(tabbedPage.CurrentPage, secondPage);
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

		TabbedPage CreateBasicTabbedPage(bool bottomTabs = false)
		{
			var tabs = new TabbedPage()
			{
				Background = SolidColorBrush.Green
			};

			if (bottomTabs)
			{
				Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(tabs, Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
			}
			else
			{
				Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(tabs,
					Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Top);
			}

			return tabs;
		}
	}
}
