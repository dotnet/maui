using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

#if IOS || MACCATALYST
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
using TabbedViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Modal)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class ModalTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Entry, EntryHandler>();
					SetupShellHandlers(handlers);
				});
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AppearingAndDisappearingFireOnWindowAndModal(bool useShell)
		{
			SetupBuilder();
			var windowPage = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Window" }
			};

			var modalPage = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Modal" }
			};

			Window window;

			if (useShell)
				window = new Window(new Shell() { CurrentItem = windowPage });
			else
				window = new Window(windowPage);

			int modalAppearing = 0;
			int modalDisappearing = 0;
			int windowAppearing = 0;
			int windowDisappearing = 0;

			modalPage.Appearing += (_, _) => modalAppearing++;
			modalPage.Disappearing += (_, _) => modalDisappearing++;

			windowPage.Appearing += (_, _) => windowAppearing++;
			windowPage.Disappearing += (_, _) => windowDisappearing++;

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await windowPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage.Content);
					await windowPage.Navigation.PopModalAsync();
				});

			Assert.Equal(1, modalAppearing);
			Assert.Equal(1, modalDisappearing);
			Assert.Equal(2, windowAppearing);
			Assert.Equal(2, windowDisappearing);
		}

		[Theory]
		// Currently broken on Shell
		// Shane will fix on a separate PR
		//[InlineData(true)]
		[InlineData(false)]
		public async Task AppearingAndDisappearingFireOnMultipleModals(bool useShell)
		{
			SetupBuilder();
			var windowPage = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Window" }
			};

			var modalPage1 = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Modal1" }
			};

			var modalPage2 = new ContentPage()
			{
				Content = new Label() { Text = "AppearingAndDisappearingFireOnWindowAndModal.Modal2" }
			};

			Window window;

			if (useShell)
				window = new Window(new Shell() { CurrentItem = windowPage });
			else
				window = new Window(windowPage);

			int modal1Appearing = 0;
			int modal1Disappearing = 0;
			int modal2Appearing = 0;
			int modal2Disappearing = 0;
			int windowAppearing = 0;
			int windowDisappearing = 0;

			modalPage1.Appearing += (_, _) => modal1Appearing++;
			modalPage1.Disappearing += (_, _) => modal1Disappearing++;

			modalPage2.Appearing += (_, _) => modal2Appearing++;
			modalPage2.Disappearing += (_, _) => modal2Disappearing++;

			windowPage.Appearing += (_, _) => windowAppearing++;
			windowPage.Disappearing += (_, _) => windowDisappearing++;

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await windowPage.Navigation.PushModalAsync(modalPage1);
					await windowPage.Navigation.PushModalAsync(modalPage2);
					await windowPage.Navigation.PopModalAsync();
					await windowPage.Navigation.PopModalAsync();
				});

			Assert.Equal(2, modal1Appearing);
			Assert.Equal(2, modal1Disappearing);

			Assert.Equal(1, modal2Appearing);
			Assert.Equal(1, modal2Disappearing);

			Assert.Equal(2, windowAppearing);
			Assert.Equal(2, windowDisappearing);
		}

		[Fact]
		public async Task PushingNavigationPageModallyWithShellShowsToolbarCorrectly()
		{
			SetupBuilder();
			var windowPage = new LifeCycleTrackingPage()
			{
				Title = "Window Page Title"
			};

			var modalPage = new NavigationPage(new LifeCycleTrackingPage()
			{
				Content = new Label() { Text = "Modal page with navigation" }
			})
			{ Title = "modal page" };

			Window window = new Window(new Shell() { CurrentItem = windowPage })
			{
				Title = "PushingNavigationPageModallyWithShellShowsToolbarCorrectly Window Title"
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await windowPage.Navigation.PushAsync(new ContentPage() { Title = "Second Page on PushingNavigationPageModallyWithShellShowsToolbarCorrectly" });
					await windowPage.Navigation.PushModalAsync(modalPage);

					// Navigation Bar is visible
					Assert.True(await AssertionExtensions.Wait(() => IsNavigationBarVisible(modalPage.Handler)));
					Assert.False(IsBackButtonVisible(modalPage.Handler));

					// Verify that new navigation bar can gain a back button
					var secondModalPage = new ContentPage();
					await modalPage.Navigation.PushAsync(secondModalPage);
					Assert.True(await AssertionExtensions.Wait(() => IsBackButtonVisible(secondModalPage.Handler)));
					await secondModalPage.Navigation.PopAsync();

					// Remove the modal page and validate the root window pages toolbar is still setup correctly
					await modalPage.Navigation.PopModalAsync();

					Assert.True(await AssertionExtensions.Wait(() => IsNavigationBarVisible(windowPage.Handler)));
					Assert.True(await AssertionExtensions.Wait(() => IsBackButtonVisible(windowPage.Handler)));
				});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task LifeCycleEventsFireOnModalPagesPushedBeforeWindowHasLoaded(bool useShell)
		{
			SetupBuilder();
			var windowPage = new LifeCycleTrackingPage();
			var modalPage = new LifeCycleTrackingPage()
			{
				Content = new Label()
			};

			Window window;

			if (useShell)
				window = new Window(new Shell() { CurrentItem = windowPage });
			else
				window = new Window(windowPage);

			await windowPage.Navigation.PushModalAsync(modalPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await OnNavigatedToAsync(modalPage);
					await OnLoadedAsync(modalPage.Content);

					Assert.Equal(0, windowPage.AppearingCount);

					Assert.Equal(1, modalPage.AppearingCount);
					Assert.Equal(1, modalPage.OnNavigatedToCount);
				});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task PushModalFromAppearing(bool useShell)
		{
			SetupBuilder();
			var windowPage = new ContentPage()
			{
				Content = new Label()
				{
					Text = "Root Page"
				}
			};

			var modalPage = new ContentPage()
			{
				Content = new Label()
				{
					Text = "last modal page"
				}
			};

			Window window;

			if (useShell)
				window = new Window(new Shell() { CurrentItem = windowPage });
			else
				window = new Window(new NavigationPage(windowPage));


			bool appearingFired = false;
			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					ContentPage contentPage = new ContentPage()
					{
						Content = new Label()
						{
							Text = "Second Page"
						}
					};

					contentPage.Appearing += async (_, _) =>
					{
						if (appearingFired)
							return;

						appearingFired = true;

						await windowPage.Navigation.PushModalAsync(new ContentPage()
						{
							Content = new Label()
							{
								Text = "First modal page"
							}
						});

						await windowPage.Navigation.PushModalAsync(modalPage);
					};

					await window.Page.Navigation.PushAsync(contentPage);
					await OnLoadedAsync(modalPage);
					await window.Navigation.PopModalAsync();
					await window.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
					await OnLoadedAsync(contentPage);
				});

			Assert.True(appearingFired);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task PushModalModalWithoutAwaiting(bool useShell)
		{
			SetupBuilder();
			var windowPage = new LifeCycleTrackingPage();
			var modalPage = new LifeCycleTrackingPage()
			{
				Content = new Label()
				{
					Text = "last page"
				}
			};

			Window window;

			if (useShell)
				window = new Window(new Shell() { CurrentItem = windowPage });
			else
				window = new Window(windowPage);


			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					_ = windowPage.Navigation.PushModalAsync(new ContentPage()
					{
						Content = new Label()
						{
							Text = "First page"
						}
					});

					_ = windowPage.Navigation.PushModalAsync(new ContentPage()
					{
						Content = new Label()
						{
							Text = "Second page"
						}
					});

					_ = windowPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
				});
		}

		[Fact]
		public async Task LoadModalPagesBeforeWindowHasLoaded()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
				{
					Text = "Modal Page"
				}
			};

			var window = new Window(page);
			await page.Navigation.PushModalAsync(modalPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await OnNavigatedToAsync(modalPage);
					await OnLoadedAsync(modalPage.Content);
				});
		}

		[Fact]
		public async Task SwapWindowPageDuringModalAppearing()
		{
			SetupBuilder();
			var page = new ContentPage();
			var newRootPage = new ContentPage()
			{
				Content = new Label() { Text = "New Root Page" }
			};

			var modalPage = new ContentPage()
			{
				Content = new Label() { Text = "SwapWindowPageDuringModalAppearing" }
			};

			var window = new Window(page);

			modalPage.Appearing += (_, _) =>
			{
				window.Page = newRootPage;
			};
			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await page.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(newRootPage.Content);
				});
		}

		[Fact]
		public async Task ChangePageOnWindowRemovesModalStack()
		{
			SetupBuilder();
			var page = new ContentPage()
			{
				Content = new Label()
				{
					Background = SolidColorBrush.Purple,
					Text = "Initial Page"
				}
			};

			var modalPage = new ContentPage()
			{
				Content = new Label() { Text = "Modal Page" }
			};

			var window = new Window(page);
			await page.Navigation.PushModalAsync(modalPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await OnLoadedAsync(modalPage.Content);
					var nextPage = new ContentPage()
					{
						Content = new Label() { Text = "Next Page" }
					};

					window.Page = nextPage;
					await OnUnloadedAsync(modalPage.Content);
					await OnLoadedAsync(nextPage.Content);
					Assert.Empty(window.Navigation.ModalStack);
				});
		}

		[Fact]
		public async Task RecreatingStackCorrectlyRecreatesModalStack()
		{
			SetupBuilder();

			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
				{
					Text = "Hello from the modal page"
				}
			};

			var window = new Window(page);
			await page.Navigation.PushModalAsync(modalPage);

			var mauiContextStub1 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnNavigatedToAsync(modalPage);
				await OnLoadedAsync(modalPage.Content);

			}, mauiContextStub1);

			var mauiContextStub2 = ContextStub.CreateNew(MauiContext);
			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnNavigatedToAsync(modalPage);
				await OnLoadedAsync(modalPage.Content);
			}, mauiContextStub2);

		}

		[Theory]
		[ClassData(typeof(PageTypes))]
		public async Task SwappingRootPageWhileModalPageIsOpenDoesntCrash(Page rootPage, Page newRootPage)
		{
			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var modalPage = new NavigationPage(new ContentPage());
					await rootPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					rootPage.Window.Page = newRootPage;
					await OnLoadedAsync(newRootPage);
				});
		}

		[Theory]
		[ClassData(typeof(PageTypes))]
		public async Task BasicPushAndPop(Page rootPage, Page modalPage)
		{
			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var currentPage = rootPage.GetCurrentPage();

					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					Assert.Single(currentPage.Navigation.ModalStack);
					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
				});


			Assert.Empty(rootPage.GetCurrentPage().Navigation.ModalStack);
		}

		class PageTypes : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				for (int i = 0; i < 3; i++)
				{
					Func<Page> rootPage;

					if (i == 0)
						rootPage = () => new NavigationPage(new ContentPage());
					else if (i == 1)
						rootPage = () => new Shell() { CurrentItem = new ContentPage() };
					else
						rootPage = () => new FlyoutPage()
						{
							Flyout = new ContentPage() { Title = "Flyout" },
							Detail = new NavigationPage(new ContentPage()) { Title = "Detail" },
						};

					yield return new object[] {
						rootPage(), new NavigationPage(new ContentPage())
					};

					yield return new object[] {
						rootPage(), new ContentPage()
					};

					yield return new object[] {
						rootPage(), new TabbedPage()
						{
							Children =
							{
								new ContentPage(),
								new NavigationPage(new ContentPage())
							}
						}
					};

					yield return new object[] {
						rootPage(), new FlyoutPage()
						{
							Flyout = new ContentPage() { Title = "Flyout" },
							Detail = new ContentPage() { Title = "Detail" },
						}
					};

				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
