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
	}
}