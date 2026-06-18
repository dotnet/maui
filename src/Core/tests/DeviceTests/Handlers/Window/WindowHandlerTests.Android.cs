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
		public async Task SystemBarIconAppearanceFollowsBarColorButRespectsDeveloperOverride()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (Activity)MauiProgramDefaults.DefaultContext;
				var platformWindow = activity.Window;
				var windowInsetsController = WindowCompat.GetInsetsController(platformWindow, platformWindow.DecorView);

				Assert.NotNull(windowInsetsController);

				// Start from a clean ownership state so the assertions are deterministic.
				platformWindow.ResetSystemBarIconAppearanceTracking();

				// 1) The developer hasn't touched the icon-appearance flags, so MAUI derives a readable
				//    icon style from the bar background. A light bar background needs dark icons, which
				//    maps to AppearanceLight*Bars == true.
				platformWindow.UpdateSystemBarAppearance(
					activity,
					updateStatusBar: true,
					updateNavigationBar: true,
					statusBarBackgroundColor: Colors.LightGreen,
					navigationBarBackgroundColor: Colors.LightGreen);

				Assert.True(windowInsetsController.AppearanceLightStatusBars);
				Assert.True(windowInsetsController.AppearanceLightNavigationBars);

				// 2) The developer now explicitly overrides the flags out-of-band...
				windowInsetsController.AppearanceLightStatusBars = false;
				windowInsetsController.AppearanceLightNavigationBars = false;

				// ...a subsequent MAUI chrome update must preserve the developer's explicit choice.
				platformWindow.UpdateSystemBarAppearance(
					activity,
					updateStatusBar: true,
					updateNavigationBar: true,
					statusBarBackgroundColor: Colors.LightGreen,
					navigationBarBackgroundColor: Colors.LightGreen);

				Assert.False(windowInsetsController.AppearanceLightStatusBars);
				Assert.False(windowInsetsController.AppearanceLightNavigationBars);
			});
		}
	}
}