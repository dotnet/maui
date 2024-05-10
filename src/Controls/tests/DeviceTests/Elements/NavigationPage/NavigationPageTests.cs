using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.NavigationPage)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class NavigationPageTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
#if IOS || MACCATALYST
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedRenderer));
#else
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
#endif
					handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
					handlers.AddHandler(typeof(ScrollView), typeof(ScrollViewHandler));
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<CarouselView, CarouselViewHandler>();
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Frame, FrameRenderer>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<RadioButton, RadioButtonHandler>();
					handlers.AddHandler<Shape, ShapeViewHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
				});
			});
		}

		[Fact]
		public async Task PoppingNavigationPageDoesntCrash()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(new NavigationPage(new ContentPage() { Content = new Frame(), Title = "Detail" }));
				await navPage.PopAsync();
			});
		}

		[Fact]
		public async Task InitialPageFiresNavigatedEvent()
		{
			SetupBuilder();
			var page = new ContentPage();
			var navPage = new NavigationPage(page) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(page);
				Assert.True(page.HasNavigatedTo);
			});
		}

		[Fact]
		public async Task PushedPageFiresNavigatedEventOnInitialLoad()
		{
			SetupBuilder();

			bool pageFiredNavigated = false;
			var page = new ContentPage();
			page.NavigatedTo += (_, _) => pageFiredNavigated = true;

			var page2 = new ContentPage();

			var navPage = new NavigationPage(page) { Title = "App Page" };
			await navPage.PushAsync(page2);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await OnNavigatedToAsync(page2);
				Assert.True(page2.HasNavigatedTo);
			});

			Assert.False(pageFiredNavigated);
		}

#if !IOS && !MACCATALYST

		[Fact(DisplayName = "Swapping Navigation Toggles BackButton Correctly")]
		public async Task SwappingNavigationTogglesBackButtonCorrectly()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(new ContentPage());
				var navigation = navPage.Navigation;
				var stackNavigationView = navPage as IStackNavigationView;
				List<Page> _currentNavStack = navigation.NavigationStack.ToList();

				var singlePage = new ContentPage();
				stackNavigationView.RequestNavigation(
						new NavigationRequest(
							new List<ContentPage>
							{
								singlePage
							}, false));

				await OnLoadedAsync(singlePage);
				await (navPage.CurrentNavigationTask ?? Task.CompletedTask);

				// Wait for back button to hide
				await AssertEventually(() => !IsBackButtonVisible(handler));

				stackNavigationView.RequestNavigation(
					   new NavigationRequest(_currentNavStack, true));

				await OnLoadedAsync(_currentNavStack.Last());
				await (navPage.CurrentNavigationTask ?? Task.CompletedTask);

				// Wait for back button to reveal itself
				await AssertEventually(() => IsBackButtonVisible(handler));
			});
		}

		[Fact(DisplayName = "Back Button Visibility Changes with push/pop")]
		public async Task BackButtonVisibilityChangesWithPushPop()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				Assert.False(IsBackButtonVisible(handler));
				await navPage.PushAsync(new ContentPage());
				Assert.True(IsBackButtonVisible(handler));
				await navPage.PopAsync();
				Assert.False(IsBackButtonVisible(handler));
			});
		}

		[Fact(DisplayName = "Set Has Back Button")]
		public async Task SetHasBackButton()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.PushAsync(new ContentPage());
				NavigationPage.SetHasBackButton(navPage.CurrentPage, false);
				Assert.False(IsBackButtonVisible(handler));
				NavigationPage.SetHasBackButton(navPage.CurrentPage, true);
				Assert.True(IsBackButtonVisible(handler));
			});
		}
#endif

		[Fact(DisplayName = "Set Has Navigation Bar")]
		public async Task SetHasNavigationBar()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage() { Title = "Nav Bar" });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await AssertEventually(() => IsNavigationBarVisible(handler));
				NavigationPage.SetHasNavigationBar(navPage.CurrentPage, false);
				await AssertEventually(() => !IsNavigationBarVisible(handler));
				NavigationPage.SetHasNavigationBar(navPage.CurrentPage, true);
				await AssertEventually(() => IsNavigationBarVisible(handler));
			});
		}

		[Fact(DisplayName = "NavigationBar Removes When MainPage Set To ContentPage")]
		public async Task NavigationBarRemovesWhenMainPageSetToContentPage()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage());
			var window = new Window(navPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var contentPage = new ContentPage();
				window.Page = contentPage;
				await OnLoadedAsync(contentPage);
				await AssertEventually(() => !IsNavigationBarVisible(handler));
			});
		}

		[Fact(DisplayName = "Insert Page Before Root Page and then PopToRoot")]
		public async Task InsertPageBeforeRootPageAndThenPopToRoot()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				navPage.Navigation.InsertPageBefore(new ContentPage(), navPage.RootPage);
				await navPage.PopToRootAsync(false);
			});

			// Just verifying that nothing crashes
		}


#if !IOS && !MACCATALYST
		[Fact(DisplayName = "Insert Page Before RootPage ShowsBackButton")]
		public async Task InsertPageBeforeRootPageShowsBackButton()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				navPage.Navigation.InsertPageBefore(new ContentPage(), navPage.RootPage);

				// InsertPageBefore is actually async in the background so we have to insert a pause
				// here to allow android to settle before testing
				await Task.Delay(100);

				Assert.True(IsBackButtonVisible(navPage.Handler));
			});
		}
#endif

		[Fact(DisplayName = "Remove Root Page Hides Back Button")]
		public async Task RemoveRootPageHidesBackButton()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.Navigation.PushAsync(new ContentPage());
				navPage.Navigation.RemovePage(navPage.RootPage);

				// RemovePage is actually async in the background so we have to insert a pause
				// here to allow android to settle before testing
				await Task.Delay(100);

				Assert.False(IsBackButtonVisible(navPage.Handler));
			});
		}

		[Fact(DisplayName = "Pushing a Tabbed Page Doesn't Throw Exception")]
		public async Task PushingATabbedPageDoesntThrowException()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()
			{
				Title = "Page Title"
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var tabbedPage1 = CreateTabbedPage("1");
				var tabbedPage2 = CreateTabbedPage("2");

				await navPage.PushAsync(tabbedPage1);
				tabbedPage1.SelectedItem = tabbedPage1.Children[1];
				await OnLoadedAsync(tabbedPage1.Children[1]);
				await navPage.PopAsync();
				await navPage.PushAsync(tabbedPage2);

				TabbedPage CreateTabbedPage(string title) => new TabbedPage()
				{
					Title = title,
					Children =
					{
						new ContentPage(),
						new ContentPage()
					}
				};
			});
		}

		[Fact(DisplayName = "NavigationPage Does Not Leak")]
		public async Task DoesNotLeak()
		{

#if ANDROID
			if (!OperatingSystem.IsAndroidVersionAtLeast(30))
				return;
#endif

			SetupBuilder();
			WeakReference pageReference = null;
			var navPage = new NavigationPage(new ContentPage { Title = "Page 1" });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var page = new ContentPage
				{
					Title = "Page 2",
					Content = new VerticalStackLayout
					{
						new Button(),
						new CarouselView(),
						new CollectionView(),
						new ContentView(),
						new Label(),
						new ScrollView(),
						new RadioButton(),
					}
				};
				pageReference = new WeakReference(page);
				await navPage.Navigation.PushAsync(page);
				await navPage.Navigation.PopAsync();
			});

			await AssertionExtensions.WaitForGC(pageReference);
		}

		[Fact(DisplayName = "Can Reuse Pages"
#if WINDOWS
			,Skip = "Failing"
#endif
			)]
		public async Task CanReusePages()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage { Title = "Page 1" });
			var reusedPage = new ContentPage
			{
				Content = new Label()
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				await navPage.Navigation.PushAsync(reusedPage);
				await navPage.Navigation.PopAsync();
				await navPage.Navigation.PushAsync(reusedPage);
				await OnLoadedAsync(reusedPage.Content);
			});
		}

		[Fact]
		public async Task SettingTitleIconImageSourceDoesntCrash()
		{
			SetupBuilder();
			var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var page = new ContentPage() { Content = new Frame(), Title = "Detail" };
				await navPage.PushAsync(new NavigationPage(page));
				NavigationPage.SetTitleIconImageSource(page, "red.png");
			});
		}
	}
}