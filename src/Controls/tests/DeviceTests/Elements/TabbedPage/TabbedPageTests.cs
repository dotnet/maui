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
#if IOS
using TabbedViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer;
#endif
#if WINDOWS
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
#endif

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


#if WINDOWS
		[Fact(DisplayName = "BarBackground Color")]
		public async Task BarBackgroundColor()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(true);
			tabbedPage.BarBackground = SolidColorBrush.Purple;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), (handler) =>
			{
				var navView = GetMauiNavigationView(tabbedPage.Handler.MauiContext);
				var platformBrush = (WSolidColorBrush)((Paint)tabbedPage.BarBackground).ToPlatform();
				Assert.Equal(platformBrush.Color, ((WSolidColorBrush)navView.TopNavArea.Background).Color);
				return Task.CompletedTask;
			});
		}
#endif


		[Fact(DisplayName = "Bar Text Color"
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		public async Task BarTextColor()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(true, pages: new[]
			{
				new ContentPage() { Title = "Page 1" },
				new ContentPage() { Title = "Page 2" }
			});

			tabbedPage.BarTextColor = Colors.Red;
			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				// Pre iOS15 you couldn't set the text color of the unselected tab
				// so only android/windows currently set the color of both

#if IOS
				bool unselectedMatchesSelected = false;
#else
				bool unselectedMatchesSelected = true;
#endif

				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[0].Title, Colors.Red, true);
				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[1].Title, Colors.Red, unselectedMatchesSelected);
				tabbedPage.BarTextColor = Colors.Blue;
				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[0].Title, Colors.Blue, true);
				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[1].Title, Colors.Blue, unselectedMatchesSelected);
			});
		}

		[Fact(DisplayName = "Selected/Unselected Color"
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
		public async Task SelectedAndUnselectedTabColor()
		{
			SetupBuilder();
			var tabbedPage = CreateBasicTabbedPage(true);
			tabbedPage.Children.Add(new ContentPage() { Title = "Page 2", IconImageSource = "white.png" });

			tabbedPage.SelectedTabColor = Colors.Red;
			tabbedPage.UnselectedTabColor = Colors.Purple;

			await CreateHandlerAndAddToWindow<TabbedViewHandler>(tabbedPage, async handler =>
			{
				// Pre iOS15 you couldn't set the text color of the unselected tab
				// so only android/windows currently set the color of both
#if IOS
				bool unselectedMatchesTabColor = false;
#else
				bool unselectedMatchesTabColor = true;
#endif
				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[0].Title, Colors.Red, true);
				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[1].Title, Colors.Purple, unselectedMatchesTabColor);
				await ValidateTabBarIconColor(tabbedPage, tabbedPage.Children[0].Title, Colors.Red, true);
				await ValidateTabBarIconColor(tabbedPage, tabbedPage.Children[1].Title, Colors.Purple, true);

				tabbedPage.CurrentPage = tabbedPage.Children[1];
				await OnNavigatedToAsync(tabbedPage.CurrentPage);

				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[0].Title, Colors.Purple, true);
				await ValidateTabBarTextColor(tabbedPage, tabbedPage.Children[1].Title, Colors.Red, unselectedMatchesTabColor);
				await ValidateTabBarIconColor(tabbedPage, tabbedPage.Children[0].Title, Colors.Purple, true);
				await ValidateTabBarIconColor(tabbedPage, tabbedPage.Children[1].Title, Colors.Red, true);
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

		[Theory("Remove CurrentPage And Then Re-Add Doesnt Crash"
#if WINDOWS
		, Skip = "Fails on Windows"
#endif
		)]
		[ClassData(typeof(TabbedPagePivots))]
		public async Task RemoveCurrentPageAndThenReAddDoesntCrash(bool bottomTabs, bool isSmoothScrollEnabled)
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage(bottomTabs, isSmoothScrollEnabled);

			var firstPage = new NavigationPage(new ContentPage()
			{
				Content = new VerticalStackLayout()
				{
					new Label()
					{
						Text = "Page one",
						Background = Colors.Purple
					}
				}
			})
			{
				Title = "First Page"
			};

			tabbedPage.Children.Insert(0, firstPage);
			tabbedPage.CurrentPage = firstPage;
			var secondPage = tabbedPage.Children[1];

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				await OnNavigatedToAsync(firstPage);
				tabbedPage.Children.Remove(firstPage);
				await OnNavigatedToAsync(secondPage);
				await OnUnloadedAsync(firstPage);
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
#elif WINDOWS
		[Theory(Skip = "Test doesn't work on Windows")]
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

		[Fact(DisplayName = "Does Not Leak"
#if WINDOWS
			, Skip = "FIXME: fails on Windows"
#endif
		)]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference pageReference = null;
			var navPage = new NavigationPage(new ContentPage { Title = "Page 1" });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var page = new TabbedPage
				{
					Children =
					{
						new ContentPage
						{
							Content = new VerticalStackLayout { new Label() },
						}
					}
				};
				pageReference = new WeakReference(page);
				await navPage.Navigation.PushAsync(page);
				await navPage.Navigation.PopAsync();
			});

			await AssertionExtensions.WaitForGC(pageReference);
		}


		TabbedPage CreateBasicTabbedPage(bool bottomTabs = false, bool isSmoothScrollEnabled = true, IEnumerable<Page> pages = null)
		{
			pages = pages ?? new List<Page>()
			{
				new ContentPage() { Title = "Page 1", IconImageSource = "white.png" }
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
