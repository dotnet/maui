using System.Diagnostics;
using Maui.Controls.Sample.Issues;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();

#if IOS || ANDROID
			appBuilder.UseMauiMaps();
#endif
			appBuilder.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("FontAwesome.ttf", "FA");
					fonts.AddFont("ionicons.ttf", "Ion");
				})
				.RenderingPerformanceAddMappers()
				.Issue21109AddMappers()
				.Issue18720AddMappers()
				.Issue18720EditorAddMappers()
				.Issue18720DatePickerAddMappers()
				.Issue18720TimePickerAddMappers();

#if IOS || MACCATALYST

			appBuilder.ConfigureCollectionViewHandlers();

#endif

			appBuilder.Services.AddTransient<TransientPage>();
			appBuilder.Services.AddScoped<ScopedPage>();
			return appBuilder.Build();
		}
	}

	class App : Application
	{
		public const string AppName = "CompatibilityGalleryControls";
		public const string DefaultMainPageId = "ControlGalleryMainPage";

		public App()
		{
		}

		public static bool PreloadTestCasesIssuesList { get; set; } = true;

		public Page CreateDefaultMainPage()
		{
			return new CoreNavigationPage();
		}

		protected override void OnAppLinkRequestReceived(Uri uri)
		{
			base.OnAppLinkRequestReceived(uri);
		}

		protected override Window CreateWindow(IActivationState activationState)
		{
			var window = new Window(CreateDefaultMainPage());
#if WINDOWS || MACCATALYST

			// For desktop use a fixed window size, so that screenshots are deterministic,
			// matching (as much as possible) between dev machines and CI. Currently
			// our Windows CI machines run at 1024x768 resolution and the window size can't
			// be larger than screen size. We'll investigate increasing CI screen
			// resolution in the future so we can have a bigger window, but for now
			// this size works.
			const int desktopWindowWidth = 1024;
			const int desktopWindowHeight = 768;

#if WINDOWS
			window.Width = desktopWindowWidth;
			window.Height = desktopWindowHeight;

			var info = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo;
			int screenWidth = (int)(info.Width / info.Density);
			int screenHeight = (int)(info.Height / info.Density);

			// Center the window on the screen, to ensure no part of it goes off screen in CI
			window.X = (screenWidth - desktopWindowWidth) / 2;
			window.Y = (screenHeight - desktopWindowHeight) / 2;
#elif MACCATALYST
			// Setting max and min is currently needed to force the size on Catalyst;
			// just setting width/height has no effect on Catalyst
			window.MaximumWidth = desktopWindowWidth;
			window.MinimumWidth = desktopWindowWidth;

			window.MaximumHeight = desktopWindowHeight;
			window.MinimumHeight = desktopWindowHeight;
#endif

#endif


			return window;
		}
	}
}
