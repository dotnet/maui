using System;
using System.Collections;
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
		void SetupBuilder(Action<MauiAppBuilder> additionalCreationActions = null)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Label, LabelHandler>();

#if IOS || MACCATALYST
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
#else
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
#endif
				});

				additionalCreationActions?.Invoke(builder);
			});
		}

#if !IOS && !MACCATALYST
		// iOS currently can't handle recreating a handler if it's disconnecting
		// This is left over behavior from Forms and will be fixed by a different PR
		[Theory]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task DisconnectEachPageHandlerAfterNavigation(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();

			List<Page> navPages = new List<Page>();
			var pageCount = 5;
			for (int i = 0; i < pageCount; i++)
			{
				navPages.Add(new NavigationPage(new ContentPage()
				{
					Content = new Label() { Text = $"Page {i}" }
				})
				{ Title = $"App Page {i}" });
			}

			var tabbedPage =
				CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled, navPages);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				for (int i = 0; i < pageCount * 2; i++)
				{
					var currentPage = tabbedPage.CurrentPage;
					var previousPage = currentPage;

					await OnNavigatedToAsync(currentPage);
					int pageIndex = tabbedPage.Children.IndexOf(currentPage) + 1;
					if (pageIndex >= pageCount)
						pageIndex = 0;

					var nextPage = tabbedPage.Children[pageIndex];
					tabbedPage.CurrentPage = nextPage;
					await OnNavigatedToAsync(nextPage);
					previousPage.Handler.DisconnectHandler();
				}
			});
		}
#endif

		[Theory]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task PoppingTabbedPageDoesntCrash(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled));
				await navPage.PopAsync();
			});
		}

		[Theory("Remove CurrentPage And Then Re-Add Doesnt Crash")]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task RemoveCurrentPageAndThenReAddDoesntCrash(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled);

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
		[ClassData(typeof(TabbedPagePivots))]
		public async Task SettingCurrentPageToNotBePositionZeroWorks(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled);
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

		[Theory]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task MovingBetweenMultiplePagesWithNestedNavigationPages(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();

			var pages = new NavigationPage[5];

			for (var i = 0; i < pages.Length; i++)
			{
				string title = $"Tab {i} Root Page";
				var contentPage = new ContentPage()
				{
					Title = title,
					Content = new Button()
					{
						Text = title
					}
				};

				pages[i] = new NavigationPage(contentPage)
				{
					Title = title
				};
			};

			var tabbedPage = CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled, pages);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				// Navigate to each page and push a page on the stack
				// Android does a lot of fragment creating and destroying on us
				// So we're mainly validating that android all works alright here
				for (var i = 0; i < pages.Length; i++)
				{
					NavigationPage navigationPage = pages[i];
					tabbedPage.CurrentPage = navigationPage;
					await OnNavigatedToAsync(navigationPage.CurrentPage);
					await OnLoadedAsync((navigationPage.CurrentPage as ContentPage).Content);

					var nextPage = new ContentPage()
					{
						Content = new Button()
						{
							Text = $"Tab {i} Next Page"
						}
					};
					await navigationPage.PushAsync(nextPage);
					await OnNavigatedToAsync(nextPage);
					await OnLoadedAsync(nextPage.Content);
				}

				// Navigate back through and make sure nothing crashes
				// and that we can pop back to our root pages
				foreach (var navigationPage in pages)
				{
					tabbedPage.CurrentPage = navigationPage;
					await OnNavigatedToAsync(navigationPage.CurrentPage);
					await OnLoadedAsync((navigationPage.CurrentPage as ContentPage).Content);
					await Task.Delay(200);
					await navigationPage.PopAsync();
					await OnNavigatedToAsync(navigationPage.CurrentPage);
					await OnLoadedAsync((navigationPage.CurrentPage as ContentPage).Content);
				}
			});
		}

#if !WINDOWS
		[Theory]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task RemovingAllPagesDoesntCrash(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled);
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
#endif

#if IOS
		[Theory(Skip = "Test doesn't work on iOS yet; probably because of https://github.com/dotnet/maui/issues/10591")]
#else
		[Theory]
#endif
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

		public class TabbedPagePivots : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				//bottomtabs, isSmoothScrollEnabled
				yield return new object[] { false, true };
#if ANDROID
				yield return new object[] { false, false };
				yield return new object[] { true, false };
				yield return new object[] { true, true };
#endif
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}


		TabbedPage CreateBasicTabbedPage(bool bottomTabs = false, bool isSmoothScrollEnabled = true, IEnumerable<Page> pages = null)
		{
			pages = pages ?? new List<Page>()
			{
				new ContentPage() { Title = "Page 1" }
			};

			var tabs = new TabbedPage()
			{
				Title = "Tabbed Page",
				Background = SolidColorBrush.Green
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
