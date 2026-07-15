using System;
using System.Threading.Tasks;
using Android.App;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : CoreHandlerTestBase
	{

		[Fact]
		public async Task UsingTheSameWindowThrowsInvalidOperationException()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<WindowStub, WindowHandlerProxyStub>();
				});
			});

			await InvokeOnMainThreadAsync(() =>
			{
				var app = (CoreApplicationStub)MauiContext.Services.GetRequiredService<IApplication>();
				var handler = new ApplicationHandler();
				app.Handler = handler;
				handler.SetMauiContext(MauiContext);

				var activity1 = new MauiAppCompatActivity();
				var activity2 = new MauiAppCompatActivity();

				activity1.CreatePlatformWindow(app, null);

				var window = app.Windows[0];
				app.SetSingleWindow(window);

				Assert.Throws<InvalidOperationException>(() =>
				{
					activity2.CreatePlatformWindow(app, null);
				});

			});
		}


		[Fact]
		public async Task TitleSetsOnWindow()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				var testWindow = new Window();

				Assert.True(activity is not null, "Activity is Null");

				testWindow.Title = "Test Title";
				WindowExtensions.UpdateTitle(activity, testWindow);

				Assert.Equal("Test Title", activity.Title);
				testWindow.Title = null;

				WindowExtensions.UpdateTitle(activity, testWindow);
				Assert.Equal(activity.Title, ApplicationModel.AppInfo.Current.Name);
			});
		}

		[Fact]
		public async Task SystemBarAppearanceDoesNotOverwriteDeveloperAppearance()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (Activity)MauiProgramDefaults.DefaultContext;
				var platformWindow = activity.Window;
				var windowInsetsController = WindowCompat.GetInsetsController(platformWindow, platformWindow.DecorView);

				Assert.NotNull(windowInsetsController);

				var originalLightStatusBars = windowInsetsController.AppearanceLightStatusBars;
				var originalLightNavigationBars = windowInsetsController.AppearanceLightNavigationBars;

#pragma warning disable CA1422 // System bar color APIs still apply to older Android versions and are harmless on newer versions.
				var originalStatusBarColor = platformWindow.StatusBarColor;
				var originalNavigationBarColor = platformWindow.NavigationBarColor;
#pragma warning restore CA1422

				try
				{
					windowInsetsController.AppearanceLightStatusBars = false;
					windowInsetsController.AppearanceLightNavigationBars = false;

					platformWindow.UpdateSystemBarAppearance(
						activity,
						updateStatusBar: true,
						updateNavigationBar: true,
						statusBarBackgroundColor: Colors.LightGreen,
						navigationBarBackgroundColor: Colors.LightGreen);

#pragma warning disable CA1422 // System bar color APIs still apply to older Android versions and are harmless on newer versions.
					Assert.Equal(Colors.LightGreen.ToPlatform().ToArgb(), platformWindow.StatusBarColor);
					Assert.Equal(Colors.LightGreen.ToPlatform().ToArgb(), platformWindow.NavigationBarColor);
#pragma warning restore CA1422

					Assert.False(windowInsetsController.AppearanceLightStatusBars);
					Assert.False(windowInsetsController.AppearanceLightNavigationBars);
				}
				finally
				{
					windowInsetsController.AppearanceLightStatusBars = originalLightStatusBars;
					windowInsetsController.AppearanceLightNavigationBars = originalLightNavigationBars;

#pragma warning disable CA1422
					platformWindow.SetStatusBarColor(new global::Android.Graphics.Color(originalStatusBarColor));
					platformWindow.SetNavigationBarColor(new global::Android.Graphics.Color(originalNavigationBarColor));
#pragma warning restore CA1422
				}
			});
		}
	}
}
