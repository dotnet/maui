using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AndroidX.Core.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
		[Fact]
		public async Task WindowDestroyingPreservesWindowScopeOnAndroid()
		{
			// https://github.com/dotnet/maui/issues/33597
			SetupBuilder();

			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(window.Page);

				var mauiContext = handler.MauiContext as MauiContext;
				Assert.NotNull(mauiContext);

				var windowScopeField = typeof(MauiContext).GetField("_windowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				var setWindowScope = typeof(MauiContext).GetMethod("SetWindowScope", BindingFlags.NonPublic | BindingFlags.Instance);

				var newScope = mauiContext.Services.CreateScope();
				setWindowScope.Invoke(mauiContext, new[] { newScope });
				Assert.NotNull(windowScopeField.GetValue(mauiContext));

				((IWindow)window).Destroying();

				Assert.NotNull(windowScopeField.GetValue(mauiContext));
			});
		}

		[Fact]
		public async Task StatusBarThemeDoesNotChangeNavigationBarTheme()
		{
			SetupBuilder();

			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(window.Page);

				var platformWindow = handler.PlatformView.Window;
				Assert.NotNull(platformWindow);

				var controller = WindowCompat.GetInsetsController(platformWindow, platformWindow.DecorView);
				Assert.NotNull(controller);

				var originalLightStatusBars = controller.AppearanceLightStatusBars;
				var originalLightNavigationBars = controller.AppearanceLightNavigationBars;
				try
				{
					controller.AppearanceLightNavigationBars = true;
					window.StatusBarTheme = StatusBarTheme.Dark;

					Assert.False(controller.AppearanceLightStatusBars);
					Assert.True(controller.AppearanceLightNavigationBars);

					controller.AppearanceLightNavigationBars = false;
					window.StatusBarTheme = StatusBarTheme.Light;

					Assert.True(controller.AppearanceLightStatusBars);
					Assert.False(controller.AppearanceLightNavigationBars);
				}
				finally
				{
					controller.AppearanceLightStatusBars = originalLightStatusBars;
					controller.AppearanceLightNavigationBars = originalLightNavigationBars;
				}
			});
		}
	}
}
