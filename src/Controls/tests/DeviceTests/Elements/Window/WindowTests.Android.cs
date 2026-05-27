using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using AndroidX.Core.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

		[Theory]
		[Description("Bright boundary colors (yellow, cyan) should produce light status bar appearance (dark icons), not light icons on a bright background")]
		[InlineData(1f, 1f, 0f, true)]   // Yellow  — HSL lightness = 0.5 (wrong threshold), perceptual = 0.886 (clearly light)
		[InlineData(0f, 1f, 1f, true)]   // Cyan    — HSL lightness = 0.5 (wrong threshold), perceptual = 0.701 (clearly light)
		[InlineData(1f, 0f, 0f, true)]   // Red     — perceptual = 0.299 (light)
		[InlineData(0f, 0f, 1f, false)]  // Blue    — perceptual = 0.114 (dark, needs light icons)
		[InlineData(0f, 0f, 0f, false)]  // Black   — perceptual = 0 (dark)
		[InlineData(1f, 1f, 1f, true)]   // White   — perceptual = 1.0 (light)
		public async Task SystemBarAppearanceIsLightForBrightBoundaryColors(float r, float g, float b, bool expectedLight)
		{
			SetupBuilder();

			var barColor = new Color(r, g, b);
			var page = new ContentPage { BackgroundColor = barColor };
			var window = new Window(new NavigationPage(page) { BarBackgroundColor = barColor });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(page);

				var activity = handler.PlatformView;
				var platformWindow = activity.Window;
				Assert.NotNull(platformWindow);

				var insetsController = WindowCompat.GetInsetsController(platformWindow, platformWindow.DecorView);
				Assert.NotNull(insetsController);

				await AssertEventually(
					() => insetsController.AppearanceLightStatusBars == expectedLight,
					message: $"Expected AppearanceLightStatusBars={expectedLight} for color ({r},{g},{b}) but was {insetsController.AppearanceLightStatusBars}");
			});
		}

	}
}
