using System.Threading.Tasks;
using Android.Views;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class FlyoutPageTests
	{
		DrawerLayout FindPlatformFlyoutView(AView aView) =>
			aView.GetParentOfType<DrawerLayout>();

		[Fact]
		// https://github.com/dotnet/maui/issues/14609
		public async Task TapOnTheOpenFlyoutDoesNotReachTheDetailPage()
		{
			SetupBuilder();

			var flyoutPage = CreateFlyoutPage(
				typeof(FlyoutPage),
				new ContentPage() { Title = "Detail", Content = new Label() { Text = "Detail" } },
				new ContentPage() { Title = "Flyout", Content = new Label() { Text = "Flyout" } });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Controls.Window(flyoutPage), async (handler) =>
			{
				bool detailTouched = false;
				var detailView = flyoutPage.Detail.ToPlatform();
				detailView.Touch += (_, e) =>
				{
					detailTouched = true;
					e.Handled = false;
				};

				var flyoutView = flyoutPage.Flyout.ToPlatform();
				var drawerLayout = FindPlatformFlyoutView(flyoutView);

				flyoutPage.IsPresented = true;
				await AssertHelpers.AssertEventually(() => drawerLayout.IsDrawerOpen(flyoutView), timeout: 5000, message: "Flyout did not open");

				// Tap the middle of the open flyout
				var x = flyoutView.GetX() + flyoutView.Width / 2f;
				var y = flyoutView.GetY() + flyoutView.Height / 2f;
				var time = global::Android.OS.SystemClock.UptimeMillis();
				using (var down = MotionEvent.Obtain(time, time, MotionEventActions.Down, x, y, 0))
				using (var up = MotionEvent.Obtain(time, time, MotionEventActions.Up, x, y, 0))
				{
					drawerLayout.DispatchTouchEvent(down);
					drawerLayout.DispatchTouchEvent(up);
				}

				Assert.False(detailTouched, "A tap on the open flyout reached the detail page behind it");
			});
		}

		[Fact]
		public async Task SwappingDetailPageKeepsASingleToolbar()
		{
			SetupBuilder();

			var flyoutPage = CreateFlyoutPage(
					typeof(FlyoutPage),
					new NavigationPage(new ContentPage() { Content = new Border(), Title = "Detail" }),
					new ContentPage() { Title = "Flyout" });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Controls.Window(flyoutPage), async (handler) =>
			{
				var currentDetailPage = flyoutPage.Detail;

				// Set with new page
				var navPage = new NavigationPage(new ContentPage()) { Title = "App Page" };
				flyoutPage.Detail = navPage;
				await OnNavigatedToAsync(navPage);

				var appbarLayout =
				flyoutPage.ToPlatform()?.FindViewById<ViewGroup>(Resource.Id.navigationlayout_appbar) ??
				handler.MauiContext?.GetNavigationRootManager()?.RootView?.FindViewById<ViewGroup>(Resource.Id.navigationlayout_appbar);

				Assert.True(appbarLayout.ChildCount == 2);
				Assert.True(appbarLayout.GetChildAt(0) is AndroidX.AppCompat.Widget.Toolbar, "The first child of the view group should be the Toolbar");
				Assert.True(appbarLayout.GetChildAt(1) is global::Android.Widget.FrameLayout, "The second child of the view group should be a FrameLayout");
			});
		}

		[Fact]
		public async Task SwappingDetailThenReplacingWindowRootDoesNotCrash()
		{
			SetupBuilder();

			var flyoutPage = CreateFlyoutPage(
				typeof(FlyoutPage),
				new NavigationPage(new ContentPage { Title = "Initial Detail" }),
				new ContentPage { Title = "Flyout" });
			var window = new Controls.Window(flyoutPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var oldRootView = Assert.IsType<ContainerView>(rootManager.RootView);

				flyoutPage.Detail = new NavigationPage(new ContentPage { Title = "Replacement Detail" });

				var replacementRoot = new ContentPage { Title = "Replacement Root" };
				window.Page = replacementRoot;
				await OnLoadedAsync(replacementRoot);

				Assert.Null(oldRootView.CurrentView);
				Assert.Null(oldRootView.MainView);
				AssertPageAttachedToRoot(replacementRoot, rootManager);
			});
		}

		[Fact]
		public async Task NestedFlyoutPageDetailSurvivesRootReplacement()
		{
			SetupBuilder();

			var nestedDetail = new ContentPage
			{
				Title = "Nested Detail",
				Content = new Label { Text = "Nested detail content" }
			};
			var nestedFlyout = CreateFlyoutPage(
				typeof(FlyoutPage),
				new NavigationPage(nestedDetail),
				new ContentPage { Title = "Nested Flyout" });
			var rootNavigation = new NavigationPage(new ContentPage { Title = "Root Page" });
			var window = new Controls.Window(rootNavigation);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await rootNavigation.PushAsync(nestedFlyout);
				await OnLoadedAsync(nestedDetail);
				Assert.True(nestedDetail.Handler?.PlatformView is AView { IsAttachedToWindow: true });

				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var replacementRoot = new ContentPage { Title = "Replacement Root" };

				window.Page = replacementRoot;
				await OnLoadedAsync(replacementRoot);

				AssertPageAttachedToRoot(replacementRoot, rootManager);
			});
		}

		static void AssertPageAttachedToRoot(Page page, NavigationRootManager rootManager)
		{
			var rootView = rootManager.RootView;
			var platformView = page.ToPlatform();

			Assert.NotNull(rootView);
			Assert.NotNull(platformView);
			Assert.True(platformView.IsAttachedToWindow);

			for (AView current = platformView; current is not null; current = current.Parent as AView)
			{
				if (ReferenceEquals(current, rootView))
					return;
			}

			Assert.Fail("The replacement page's platform view is not hosted by the navigation root.");
		}
	}
}