using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Runtime;
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
		public async Task DismissingModalNavigationPageDuringPushDoesntCrash()
		{
			SetupBuilder();
			var page = new ContentPage();
			var modalRootPage = new ContentPage()
			{
				Content = new Label()
			};

			var modalPage = new NavigationPage(modalRootPage);
			var window = new Window(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (_) =>
				{
					await page.Navigation.PushModalAsync(modalPage, false);
					await OnNavigatedToAsync(modalRootPage);

					var navHostFragmentManager = ((NavigationViewHandler)modalPage.Handler).StackNavigationManager.NavHost.ChildFragmentManager;

					// Android calls OnCreateView, so an exception thrown there would crash the app instead of failing the test
					System.Exception unhandledException = null;
					AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;

					try
					{
						// Dismissing the modal disconnects the NavigationPage before the fragment transaction of the push runs
						var pushTask = modalPage.PushAsync(new ContentPage(), false);
						var popTask = page.Navigation.PopModalAsync(false);
						navHostFragmentManager.ExecutePendingTransactions();
						await Task.WhenAll(pushTask, popTask);
					}
					finally
					{
						AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledException;
					}

					Assert.Null(unhandledException);

					void OnUnhandledException(object sender, RaiseThrowableEventArgs e)
					{
						unhandledException ??= e.Exception;
						e.Handled = true;
					}
				});
		}
	}
}
