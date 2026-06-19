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

					try
					{
						activityWindowInsetsController.AppearanceLightStatusBars = true;
						activityWindowInsetsController.AppearanceLightNavigationBars = true;

						await page.Navigation.PushModalAsync(modalPage, animated: false);
						await OnLoadedAsync(modalPage.CurrentPage);

						var dialogWindow = GetModalDialogFragment(handler).Dialog?.Window;
						Assert.NotNull(dialogWindow);

						var dialogWindowInsetsController = WindowCompat.GetInsetsController(dialogWindow, dialogWindow.DecorView);
						Assert.NotNull(dialogWindowInsetsController);
						Assert.True(dialogWindowInsetsController.AppearanceLightStatusBars);
						Assert.True(dialogWindowInsetsController.AppearanceLightNavigationBars);
					}
					finally
					{
						activityWindowInsetsController.AppearanceLightStatusBars = originalLightStatusBars;
						activityWindowInsetsController.AppearanceLightNavigationBars = originalLightNavigationBars;
					}
				});
		}

		static DialogFragment GetModalDialogFragment(IElementHandler handler)
		{
			var fragmentManager = handler.MauiContext.GetFragmentManager();
			return fragmentManager.Fragments.OfType<DialogFragment>().Single(fragment => fragment.Dialog?.Window is not null);
		}
	}
}
