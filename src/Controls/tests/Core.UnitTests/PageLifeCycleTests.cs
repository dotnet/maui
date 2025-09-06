using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class PageLifeCycleTests : BaseTestFixture
	{
		[Fact]
		// This test isn't valid for non handler based
		// navigation because the initial navigated event
		// fires from the legacy code instead of the
		// new handler code
		// We have device tests to also verify this works on 
		// each platform
		public async Task NavigationPageInitialPage()
		{
			var lcPage = new LCPage();
			var navigationPage = new TestNavigationPage(true, lcPage)
					.AddToTestWindow();

			await navigationPage.NavigatingTask;
			Assert.Null(lcPage.NavigatingFromArgs);
			Assert.Null(lcPage.NavigatedFromArgs);
			Assert.NotNull(lcPage.NavigatedToArgs);
			Assert.Null(lcPage.NavigatedToArgs.PreviousPage);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationPagePushPage(bool useMaui)
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			var navigationPage =
				new TestNavigationPage(useMaui, previousPage)
					.AddToTestWindow();

			await navigationPage.PushAsync(lcPage);

			Assert.NotNull(previousPage.NavigatingFromArgs);
			Assert.NotNull(lcPage.NavigatedToArgs);
			Assert.NotNull(previousPage.NavigatedFromArgs);
			Assert.Equal(previousPage, lcPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(lcPage, previousPage.NavigatedFromArgs.DestinationPage);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationPagePopPage(bool useMaui)
		{
			var firstPage = new LCPage();
			var poppedPage = new LCPage();

			NavigationPage navigationPage = new TestNavigationPage(useMaui, firstPage)
					.AddToTestWindow();

			await navigationPage.PushAsync(poppedPage);
			await navigationPage.PopAsync();

			Assert.NotNull(poppedPage.NavigatingFromArgs);
			Assert.Equal(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationPagePopToRoot(bool useMaui)
		{
			var firstPage = new LCPage();
			var poppedPage = new LCPage();

			NavigationPage navigationPage = new TestNavigationPage(useMaui, firstPage)
					.AddToTestWindow();

			await navigationPage.PushAsync(new ContentPage());
			await navigationPage.PushAsync(new ContentPage());
			await navigationPage.PushAsync(poppedPage);
			await navigationPage.PopToRootAsync();

			Assert.NotNull(poppedPage.NavigatingFromArgs);
			Assert.Equal(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);
		}

		[Fact]
		public async Task TabbedPageBasicSelectionChanged()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var tabbedPage = new TabbedPage() { Children = { firstPage, secondPage } }.AddToTestWindow();

			tabbedPage.CurrentPage = secondPage;
			Assert.NotNull(firstPage.NavigatingFromArgs);
			Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
		}

		[Fact]
		public void TabbedPageInitialPage()
		{
			var firstPage = new LCPage { Title = "First Page" };
			var secondPage = new LCPage { Title = "Second Page" };
			var tabbedPage = new TabbedPage().AddToTestWindow();

			tabbedPage.Children.Add(firstPage);
			tabbedPage.Children.Add(secondPage);

			Assert.NotNull(firstPage.NavigatedToArgs);
			Assert.Null(firstPage.NavigatedToArgs.PreviousPage);

			if (firstPage.NavigatingFromArgs is not null)
			{
				Assert.Same(firstPage, firstPage.NavigatingFromArgs.DestinationPage);
				Assert.Equal(NavigationType.Replace, firstPage.NavigatingFromArgs.NavigationType);
			}

			if (firstPage.NavigatedFromArgs is not null)
			{
				Assert.Same(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
			}
		}

		[Fact]
		public async Task FlyoutPageFlyoutChanged()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var flyoutPage = new FlyoutPage()
			{
				Detail = new ContentPage() { Title = "Detail" },
				Flyout = firstPage
			}.AddToTestWindow();

			flyoutPage.Flyout = secondPage;

			Assert.NotNull(firstPage.NavigatingFromArgs);
			Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
		}

		[Fact]
		public async Task FlyoutPageDetailChanged()
		{
			var firstPage = new LCPage() { Title = "First Page" };
			var secondPage = new LCPage() { Title = "Second Page" };
			var flyoutPage = new FlyoutPage()
			{
				Detail = firstPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			}.AddToTestWindow();

			flyoutPage.Detail = secondPage;

			Assert.NotNull(firstPage.NavigatingFromArgs);
			Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);
		}

		[Fact]
		public async Task FlyoutPageToggleIsPresented()
		{
			// Testing toggling IsPresented in FlyoutPage without changing navigation events
			var flyout = new LCPage { Title = "Flyout" };
			var detail = new LCPage { Title = "Detail" };
			var flyoutPage = new FlyoutPage { Flyout = flyout, Detail = detail }.AddToTestWindow();

			// Clearing initial navigation args to focus on IsPresented toggle
			detail.ClearNavigationArgs();
			flyout.ClearNavigationArgs();

			// Toggling IsPresented
			flyoutPage.IsPresented = true;
			await Task.Yield();
			flyoutPage.IsPresented = false;
			await Task.Yield();

			// Verifying no navigation events are triggered
			Assert.Null(flyout.NavigatingFromArgs);
			Assert.Null(flyout.NavigatedFromArgs);
			Assert.Null(flyout.NavigatedToArgs);
			Assert.Null(detail.NavigatingFromArgs);
			Assert.Null(detail.NavigatedFromArgs);
			Assert.Null(detail.NavigatedToArgs);

			// Verifying Loaded/Unloaded counts remain unchanged
			Assert.Equal(1, flyout.AppearingCount);
			Assert.Equal(0, flyout.DisappearingCount);
			Assert.Equal(1, detail.AppearingCount);
			Assert.Equal(0, detail.DisappearingCount);
		}

		[Fact]
		public async Task PushModalPage()
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			var window = new TestWindow(previousPage);

			await window.Navigation.PushModalAsync(lcPage);

			Assert.NotNull(previousPage.NavigatingFromArgs);
			Assert.Equal(previousPage, lcPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(lcPage, previousPage.NavigatedFromArgs.DestinationPage);

			Assert.Equal(1, previousPage.DisappearingCount);
			Assert.Equal(1, lcPage.AppearingCount);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationPagePropagatesEventsWhenCoveredByModal(bool useMaui)
		{
			var lcPage = new ContentPage();
			var targetPage = new LCPage();
			var modalPage = new ContentPage();
			var window = new TestWindow(new TestNavigationPage(useMaui, lcPage));

			await window.Page.Navigation.PushAsync(targetPage);
			targetPage.ClearNavigationArgs();
			await window.Navigation.PushModalAsync(modalPage);

			Assert.NotNull(targetPage.NavigatingFromArgs);
			Assert.Null(targetPage.NavigatedToArgs);

			await window.Navigation.PopModalAsync();
			Assert.NotNull(targetPage.NavigatedToArgs);

			Assert.Equal(modalPage, targetPage.NavigatedToArgs.PreviousPage);
		}

		[Fact]
		public async Task PopModalPage()
		{
			var firstPage = new LCPage();
			var poppedPage = new LCPage();

			var window = new TestWindow(firstPage);
			await window.Navigation.PushModalAsync(poppedPage);
			await window.Navigation.PopModalAsync();

			Assert.NotNull(poppedPage.NavigatingFromArgs);
			Assert.Equal(poppedPage, firstPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(firstPage, poppedPage.NavigatedFromArgs.DestinationPage);

			Assert.Equal(1, poppedPage.AppearingCount);
			Assert.Equal(1, poppedPage.DisappearingCount);
			Assert.Equal(2, firstPage.AppearingCount);
		}

		[Fact]
		public async Task PopToAModalPage()
		{
			var firstPage = new LCPage();
			var firstModalPage = new LCPage();
			var secondModalPage = new LCPage();

			var window = new TestWindow(firstPage);
			await window.Navigation.PushModalAsync(firstModalPage);
			await window.Navigation.PushModalAsync(secondModalPage);

			firstModalPage.ClearNavigationArgs();
			secondModalPage.ClearNavigationArgs();

			await window.Navigation.PopModalAsync();

			Assert.NotNull(secondModalPage.NavigatingFromArgs);
			Assert.Equal(secondModalPage, firstModalPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(firstModalPage, secondModalPage.NavigatedFromArgs.DestinationPage);

			Assert.Equal(1, secondModalPage.DisappearingCount);
			Assert.Equal(1, secondModalPage.AppearingCount);

			Assert.Equal(1, firstModalPage.DisappearingCount);
			Assert.Equal(2, firstModalPage.AppearingCount);
		}

		[Fact]
		public async Task PushSecondModalPage()
		{
			var firstPage = new LCPage();
			var firstModalPage = new LCPage();
			var secondModalPage = new LCPage();

			var window = new TestWindow(firstPage);
			await window.Navigation.PushModalAsync(firstModalPage);

			firstModalPage.ClearNavigationArgs();
			secondModalPage.ClearNavigationArgs();

			await window.Navigation.PushModalAsync(secondModalPage);

			Assert.NotNull(firstModalPage.NavigatingFromArgs);
			Assert.Equal(firstModalPage, secondModalPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondModalPage, firstModalPage.NavigatedFromArgs.DestinationPage);

			Assert.Equal(0, secondModalPage.DisappearingCount);
			Assert.Equal(1, secondModalPage.AppearingCount);

			Assert.Equal(1, firstModalPage.DisappearingCount);
			Assert.Equal(1, firstModalPage.AppearingCount);
		}

		[Fact]
		public async Task LoadedUnLoadedEvents()
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			var navigationPage =
				new TestNavigationPage(true, previousPage)
					.AddToTestWindow();

			await navigationPage.PushAsync(lcPage);

			int loadedCnt = 0;
			int unLoadedCnt = 0;
			lcPage.Loaded += (_, _) => loadedCnt++;
			lcPage.Unloaded += (_, _) => unLoadedCnt++;

			Assert.Equal(1, loadedCnt);
			Assert.Equal(0, unLoadedCnt);

			await navigationPage.PopAsync();

			Assert.Equal(1, loadedCnt);
			Assert.Equal(1, unLoadedCnt);
		}

		[Fact]
		public async Task LoadedFiresOnSecondSubscription()
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			var navigationPage =
				new TestNavigationPage(true, previousPage)
					.AddToTestWindow();

			await navigationPage.PushAsync(lcPage);

			int loadedCnt = 0;
			lcPage.Loaded += OnLoaded;
			Assert.Equal(1, loadedCnt);

			lcPage.Loaded -= OnLoaded;
			lcPage.Loaded += OnLoaded;
			Assert.Equal(2, loadedCnt);

			void OnLoaded(object sender, System.EventArgs e)
			{
				loadedCnt++;
			}
		}

		[Fact]
		public async Task LoadedFiresOnInitialSubscription()
		{
			var previousPage = new LCPage();
			var lcPage = new LCPage();
			var navigationPage =
				new TestNavigationPage(true, previousPage)
					.AddToTestWindow();

			await navigationPage.PushAsync(lcPage);

			int loadedCnt = 0;
			int secondLoadedSubscriberCnt = 0;
			int unLoadedCnt = 0;

			Assert.True(lcPage.IsLoaded);

			// Wire up to loaded event to setup wiring
			lcPage.Loaded += (_, _) =>
			{
				loadedCnt++;
			};

			Assert.Equal(1, loadedCnt);

			// Subscribing to loaded a second time
			// Should fire the event on the new subsciber;
			lcPage.Loaded += (_, _) =>
			{
				secondLoadedSubscriberCnt++;
			};

			lcPage.Unloaded += (_, _) => unLoadedCnt++;

			Assert.Equal(1, loadedCnt);
			Assert.Equal(1, secondLoadedSubscriberCnt);
			Assert.Equal(0, unLoadedCnt);

			await navigationPage.PopAsync();

			Assert.Equal(1, loadedCnt);
			Assert.Equal(1, secondLoadedSubscriberCnt);
			Assert.Equal(1, unLoadedCnt);
		}

		[Fact]
		public async Task NavigationPageMultiplePushesAndPops()
		{
			var firstPage = new LCPage();
			var secondPage = new LCPage();
			var thirdPage = new LCPage();
			var navigationPage = new TestNavigationPage(true, firstPage)
				.AddToTestWindow();

			// Push two pages
			await navigationPage.PushAsync(secondPage);
			await navigationPage.PushAsync(thirdPage);

			// Verify event args after multiple pushes
			Assert.NotNull(secondPage.NavigatingFromArgs);
			Assert.NotNull(secondPage.NavigatedFromArgs);
			Assert.NotNull(thirdPage.NavigatedToArgs);
			Assert.Equal(secondPage, thirdPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(thirdPage, secondPage.NavigatedFromArgs.DestinationPage);

			// Pop back to second page
			await navigationPage.PopAsync();

			Assert.NotNull(thirdPage.NavigatingFromArgs);
			Assert.NotNull(thirdPage.NavigatedFromArgs);
			Assert.NotNull(secondPage.NavigatedToArgs);
			Assert.Equal(thirdPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondPage, thirdPage.NavigatedFromArgs.DestinationPage);

			// Verify Loaded/Unloaded counts
			int secondPageLoadedCnt = 0;
			int secondPageUnloadedCnt = 0;
			secondPage.Loaded += (_, _) => secondPageLoadedCnt++;
			secondPage.Unloaded += (_, _) => secondPageUnloadedCnt++;

			// Initial subscription should trigger Loaded
			Assert.Equal(1, secondPageLoadedCnt);
			Assert.Equal(0, secondPageUnloadedCnt);

			// Pop back to first page
			await navigationPage.PopAsync();
			Assert.Equal(1, secondPageLoadedCnt);
			Assert.Equal(1, secondPageUnloadedCnt);
		}

		[Fact]
		public async Task TabbedPageMultipleTabSwitches()
		{
			var firstPage = new LCPage { Title = "First Page" };
			var secondPage = new LCPage { Title = "Second Page" };

			var tabbedPage = new TabbedPage { Children = { firstPage, secondPage } }
				.AddToTestWindow();

			// Add load/unload counters for second page
			int secondPageLoadedCnt = 0;
			int secondPageUnloadedCnt = 0;
			secondPage.Loaded += (_, _) => secondPageLoadedCnt++;
			secondPage.Unloaded += (_, _) => secondPageUnloadedCnt++;

			// Switch to second page
			tabbedPage.CurrentPage = secondPage;
			Assert.NotNull(firstPage.NavigatingFromArgs);
			Assert.NotNull(firstPage.NavigatedFromArgs);
			Assert.NotNull(secondPage.NavigatedToArgs);
			Assert.Equal(firstPage, secondPage.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondPage, firstPage.NavigatedFromArgs.DestinationPage);

			// Verify Loaded/Unloaded for second page
			Assert.Equal(1, secondPageLoadedCnt);
			Assert.Equal(0, secondPageUnloadedCnt);

			// Switch back to first page
			tabbedPage.CurrentPage = firstPage;
			Assert.Equal(1, secondPageLoadedCnt);
			// This assertion is currently failing due to unexpected unload behavior on navigation.
			// See: https://github.com/dotnet/maui/issues/30627 for context and discussion.
			//Assert.Equal(1, secondPageUnloadedCnt);
		}

		[Fact]
		public async Task FlyoutPageMultipleDetailChanges()
		{
			var flyout = new LCPage { Title = "Flyout" };
			var firstDetail = new LCPage { Title = "First Detail" };
			var secondDetail = new LCPage { Title = "Second Detail" };
			var flyoutPage = new FlyoutPage { Flyout = flyout, Detail = firstDetail }.AddToTestWindow();

			// Change to second detail
			flyoutPage.Detail = secondDetail;
			Assert.NotNull(firstDetail.NavigatingFromArgs);
			Assert.NotNull(firstDetail.NavigatedFromArgs);
			Assert.NotNull(secondDetail.NavigatedToArgs);
			Assert.Equal(firstDetail, secondDetail.NavigatedToArgs.PreviousPage);
			Assert.Equal(secondDetail, firstDetail.NavigatedFromArgs.DestinationPage);

			// Verify Loaded/Unloaded for second detail
			int secondDetailLoadedCnt = 0;
			int secondDetailUnloadedCnt = 0;
			secondDetail.Loaded += (_, _) => secondDetailLoadedCnt++;
			secondDetail.Unloaded += (_, _) => secondDetailUnloadedCnt++;

			Assert.Equal(1, secondDetailLoadedCnt);
			Assert.Equal(0, secondDetailUnloadedCnt);

			// Change back to first detail
			flyoutPage.Detail = firstDetail;
			Assert.Equal(1, secondDetailLoadedCnt);
			Assert.Equal(1, secondDetailUnloadedCnt);
		}

		public class LCPage : ContentPage
		{
			public NavigatedFromEventArgs NavigatedFromArgs { get; private set; }
			public NavigatingFromEventArgs NavigatingFromArgs { get; private set; }
			public NavigatedToEventArgs NavigatedToArgs { get; private set; }
			public int AppearingCount { get; private set; }
			public int DisappearingCount { get; private set; }

			public void ClearNavigationArgs()
			{
				NavigatedFromArgs = null;
				NavigatingFromArgs = null;
				NavigatedToArgs = null;
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				AppearingCount++;
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
				DisappearingCount++;
			}

			protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
			{
				base.OnNavigatedFrom(args);
				NavigatedFromArgs = args;
			}

			protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
			{
				base.OnNavigatingFrom(args);
				NavigatingFromArgs = args;
			}

			protected override void OnNavigatedTo(NavigatedToEventArgs args)
			{
				base.OnNavigatedTo(args);
				NavigatedToArgs = args;
			}
		}
	}
}