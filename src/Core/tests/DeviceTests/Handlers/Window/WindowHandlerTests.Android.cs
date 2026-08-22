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
using ContextThemeWrapper = Android.Views.ContextThemeWrapper;

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

		[Theory]
		[InlineData(false, true)]
		[InlineData(true, false)]
		public async Task Material3StatusBarAppearanceUsesSurfaceColor(bool isDarkTheme, bool expectedLightAppearance)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				using var configuration = new Configuration(activity.Resources?.Configuration);
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(isDarkTheme ? UiMode.NightYes : UiMode.NightNo);
				using var configurationContext = activity.CreateConfigurationContext(configuration);
				using var themedContext = new ContextThemeWrapper(
					configurationContext,
					Resource.Style.Maui_Material3_Theme_NoActionBar);

				var appearance = WindowExtensions.GetStatusBarAppearance(
					themedContext,
					isLightTheme: isDarkTheme,
					isMaterial3: true);

				Assert.Equal(expectedLightAppearance, appearance);
			});
		}

		[Fact]
		public async Task Material2StatusBarAppearanceUsesPrimaryColor()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = (AppCompatActivity)MauiProgramDefaults.DefaultContext;
				using var themedContext = new ContextThemeWrapper(
					activity,
					Resource.Style.Maui_MainTheme_NoActionBar);

				var appearance = WindowExtensions.GetStatusBarAppearance(
					themedContext,
					isLightTheme: true,
					isMaterial3: false);

				Assert.False(appearance);
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
	}
}