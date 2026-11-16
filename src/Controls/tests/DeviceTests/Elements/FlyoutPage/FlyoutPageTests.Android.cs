using System.Threading.Tasks;
using Android.Views;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
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

		[Fact]
		public async Task ToolbarItemsShowWhenSameFlyoutPageReturnsAsWindowRoot()
		{
			SetupBuilder();

			var firstItem = new ToolbarItem { Text = "Page 1 Item" };
			var firstPage = new ContentPage { Title = "Page 1", ToolbarItems = { firstItem } };
			var firstDetail = new NavigationPage(firstPage);
			var secondItem = new ToolbarItem { Text = "Page 2 Item" };
			var secondPage = new ContentPage { Title = "Page 2", ToolbarItems = { secondItem } };
			var secondDetail = new NavigationPage(secondPage);
			var flyoutPage = CreateFlyoutPage(
				typeof(FlyoutPage),
				new NavigationPage(new ContentPage { Title = "Initial Detail" }),
				new ContentPage { Title = "Flyout" });
			var window = new Controls.Window(flyoutPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				flyoutPage.Detail = firstDetail;
				await OnLoadedAsync(firstPage);
				AssertToolbarShows(handler, firstItem);

				// Swap the window away from the FlyoutPage and back to the same instance, selecting
				// the same detail and then a different one. A stale toolbar kept the old page's items.
				await ReturnToFlyoutPage(firstDetail, firstPage);
				AssertToolbarShows(handler, firstItem);

				await ReturnToFlyoutPage(secondDetail, secondPage);
				AssertToolbarShows(handler, secondItem);
			});

			void AssertToolbarShows(IElementHandler handler, ToolbarItem item)
			{
				Assert.True(GetPlatformToolbar(handler)?.IsShown, "The toolbar is not shown");
				ToolbarItemsMatch(handler, item);
			}

			async Task ReturnToFlyoutPage(NavigationPage detail, ContentPage detailPage)
			{
				var otherRoot = new ContentPage { Title = "Other Root" };
				window.Page = otherRoot;
				await OnLoadedAsync(otherRoot);
				await AssertEventually(() => flyoutPage.Handler is null, message: "The outgoing FlyoutPage was not disconnected");

				window.Page = flyoutPage;
				flyoutPage.Detail = detail;
				await OnLoadedAsync(detailPage);
			}
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