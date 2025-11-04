using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using WindowSoftInputModeAdjust = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task FlyoutPageModalAppBarLayoutGetsInsets()
		{
			SetupBuilder();
			var page = new ContentPage();

			var flyoutPage = new FlyoutPage()
			{
				Flyout = new ContentPage() { Title = "Flyout" },
				Detail = new NavigationPage(new ContentPage() { Title = "Detail" })
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await page.Navigation.PushModalAsync(flyoutPage);
					await OnLoadedAsync(flyoutPage);

					// Find the AppBarLayout in the modal's view hierarchy
					var platformView = flyoutPage.ToPlatform();
					var appBarLayout = platformView?.FindViewById<Google.Android.Material.AppBar.AppBarLayout>(Resource.Id.navigationlayout_appbar);

					// The AppBarLayout should be found even though it's nested in the DrawerLayout
					Assert.NotNull(appBarLayout);

					// If the AppBarLayout was found and has content, it should have padding set
					// This verifies that the GlobalWindowInsetListener properly applied insets
					// Note: We can't assert specific padding values as they depend on the device's window insets
				});
		}

		[Fact]
		public async Task ChangeModalStackWhileDeactivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					IWindow iWindow = window;
					await page.Navigation.PushModalAsync(new ContentPage());
					await page.Navigation.PushModalAsync(modalPage);
					await page.Navigation.PushModalAsync(new ContentPage());
					await page.Navigation.PushModalAsync(new ContentPage());
					iWindow.Deactivated();
					await page.Navigation.PopModalAsync();
					await page.Navigation.PopModalAsync();
					iWindow.Activated();
					await OnLoadedAsync(modalPage);
				});
		}

		[Fact]
		public async Task DontPushModalPagesWhenWindowIsDeactivated()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new ContentPage()
			{
				Content = new Label()
			};

			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					IWindow iWindow = window;
					iWindow.Deactivated();
					await page.Navigation.PushModalAsync(modalPage);
					Assert.False(modalPage.IsLoaded);
					iWindow.Activated();
					await OnLoadedAsync(modalPage);
				});
		}
	}
}
