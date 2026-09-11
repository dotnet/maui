using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using Java.Lang;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using WindowSoftInputModeAdjust = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
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

		[Fact]
		public async Task ModalWindowInheritsActivitySystemBarForegroundAppearance()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			SetupBuilder();
			var page = new ContentPage();
			var modalPage = new NavigationPage(new ContentPage())
			{
				BarBackgroundColor = Colors.LightGreen
			};
			var window = new Window(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window,
				async handler =>
				{
					var activityWindow = handler.PlatformView.Window;
					var activityWindowInsetsController = WindowCompat.GetInsetsController(activityWindow, activityWindow.DecorView);
					Assert.NotNull(activityWindowInsetsController);

					var originalLightStatusBars = activityWindowInsetsController.AppearanceLightStatusBars;
					var originalLightNavigationBars = activityWindowInsetsController.AppearanceLightNavigationBars;
#pragma warning disable CA1422 // System bar color APIs still apply to older Android versions and are harmless on newer versions.
					var originalStatusBarColor = activityWindow.StatusBarColor;

					try
					{
						activityWindowInsetsController.AppearanceLightStatusBars = true;
						activityWindowInsetsController.AppearanceLightNavigationBars = true;
						activityWindow.SetStatusBarColor(Colors.Red.ToPlatform());

						await page.Navigation.PushModalAsync(modalPage, animated: false);
						await OnLoadedAsync(modalPage.CurrentPage);

						var dialogWindow = GetModalDialogFragment(handler).Dialog?.Window;
						Assert.NotNull(dialogWindow);

						var dialogWindowInsetsController = WindowCompat.GetInsetsController(dialogWindow, dialogWindow.DecorView);
						Assert.NotNull(dialogWindowInsetsController);
						Assert.True(dialogWindowInsetsController.AppearanceLightStatusBars);
						Assert.True(dialogWindowInsetsController.AppearanceLightNavigationBars);
						Assert.Equal(Colors.LightGreen.ToPlatform().ToArgb(), dialogWindow.StatusBarColor);
						Assert.Equal(Colors.Red.ToPlatform().ToArgb(), activityWindow.StatusBarColor);

						modalPage.BarBackgroundColor = Colors.Blue;

						await AssertEventually(() => dialogWindow.StatusBarColor == Colors.Blue.ToPlatform().ToArgb());
						Assert.Equal(Colors.Red.ToPlatform().ToArgb(), activityWindow.StatusBarColor);
					}
					finally
					{
						activityWindowInsetsController.AppearanceLightStatusBars = originalLightStatusBars;
						activityWindowInsetsController.AppearanceLightNavigationBars = originalLightNavigationBars;
						activityWindow.SetStatusBarColor(new global::Android.Graphics.Color(originalStatusBarColor));
					}
#pragma warning restore CA1422
				});
		}

		static DialogFragment GetModalDialogFragment(IElementHandler handler)
		{
			var fragmentManager = handler.MauiContext.GetFragmentManager();
			return fragmentManager.Fragments.OfType<DialogFragment>().Single(fragment => fragment.Dialog?.Window is not null);
		}
	}
}
