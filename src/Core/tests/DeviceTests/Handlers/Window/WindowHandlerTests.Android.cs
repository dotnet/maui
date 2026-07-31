using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.Res;
using AndroidX.AppCompat.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
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

		[Theory]
		[InlineData(StatusBarTheme.Light, true)]
		[InlineData(StatusBarTheme.Dark, false)]
		public async Task StatusBarThemeAppliesWhenHandlerConnects(StatusBarTheme theme, bool expectedLightStatusBars)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				Assert.NotNull(activity);

				var platformWindow = activity.Window;
				Assert.NotNull(platformWindow);

				var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(platformWindow, platformWindow.DecorView);
				Assert.NotNull(controller);

				var originalLightStatusBars = controller.AppearanceLightStatusBars;
				var originalLightNavigationBars = controller.AppearanceLightNavigationBars;
				var handler = new WindowHandler(new PropertyMapper<IWindow, IWindowHandler>
				{
					[nameof(IWindow.StatusBarTheme)] = WindowHandler.MapStatusBarTheme
				});

				try
				{
					InitializeViewHandler(new WindowStub { StatusBarTheme = theme }, handler);
					Assert.Equal(expectedLightStatusBars, controller.AppearanceLightStatusBars);
				}
				finally
				{
					controller.AppearanceLightStatusBars = originalLightStatusBars;
					controller.AppearanceLightNavigationBars = originalLightNavigationBars;
					((IElementHandler)handler).DisconnectHandler();
				}
			});
		}

		[Theory]
		[InlineData(StatusBarTheme.Light, true)]
		[InlineData(StatusBarTheme.Dark, false)]
		public async Task StatusBarThemeSetsAppearanceLightStatusBars(StatusBarTheme theme, bool expectedLightStatusBars)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				Assert.True(activity is not null, "Activity is Null");

				var window = activity.Window;
				Assert.True(window is not null, "Window is Null");

				var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
				Assert.True(controller is not null, "InsetsController is Null");

				var originalLightStatusBars = controller.AppearanceLightStatusBars;
				try
				{
					window.UpdateStatusBarTheme(activity, theme);
					Assert.Equal(expectedLightStatusBars, controller.AppearanceLightStatusBars);
				}
				finally
				{
					controller.AppearanceLightStatusBars = originalLightStatusBars;
				}
			});
		}

		[Fact]
		public async Task StatusBarThemeDefaultFollowsSystemTheme()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				Assert.True(activity is not null, "Activity is Null");

				var window = activity.Window;
				Assert.True(window is not null, "Window is Null");

				var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
				Assert.True(controller is not null, "InsetsController is Null");

				var originalLightStatusBars = controller.AppearanceLightStatusBars;
				try
				{
					window.UpdateStatusBarTheme(activity, StatusBarTheme.Default);

					// Default should match the system theme
					var configuration = activity.Resources?.Configuration;
					var isLightTheme = configuration is null ||
						(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

					Assert.Equal(isLightTheme, controller.AppearanceLightStatusBars);
				}
				finally
				{
					controller.AppearanceLightStatusBars = originalLightStatusBars;
				}
			});
		}
	}
}