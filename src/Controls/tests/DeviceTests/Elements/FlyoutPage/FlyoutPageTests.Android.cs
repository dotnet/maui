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