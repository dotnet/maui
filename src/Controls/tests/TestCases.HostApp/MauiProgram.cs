using System.Diagnostics;
using Maui.Controls.Sample.Issues;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Maui.Controls.Sample
{
	public static partial class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();

#if IOS || ANDROID || MACCATALYST
			appBuilder.UseMauiMaps();
#endif
			appBuilder.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddEmbeddedResourceFont(typeof(MauiProgram).Assembly, "Dokdo-Regular.ttf", "Dokdo");
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("FontAwesome.ttf", "FA");
					fonts.AddFont("ionicons.ttf", "Ion");
					fonts.AddFont("MauiSampleFontIcon.ttf", "MauiSampleFontIcon");
					fonts.AddFont("Montserrat-Bold.otf", "MontserratBold");
				})
				.RenderingPerformanceAddMappers()
				.Issue21109AddMappers()
				.Issue18720AddMappers()
				.Issue18720EditorAddMappers()
				.Issue18720DatePickerAddMappers()
				.Issue18720TimePickerAddMappers()
				.Issue28945AddMappers()
				.Issue25436RegisterNavigationService();

#if IOS || MACCATALYST

			appBuilder.ConfigureCollectionViewHandlers();

#endif
			// Register the custom handler
			appBuilder.ConfigureMauiHandlers(handlers =>
			{
#if IOS || MACCATALYST || ANDROID || WINDOWS
				handlers.AddHandler(typeof(_60122Image), typeof(_60122ImageHandler));
				handlers.AddHandler(typeof(_57114View), typeof(_57114ViewHandler));
#endif
#if IOS || MACCATALYST
				handlers.AddHandler(typeof(Issue11132Control), typeof(Issue11132ControlHandler));
#endif
#if IOS || MACCATALYST || ANDROID
				handlers.AddHandler(typeof(UITestEditor), typeof(UITestEditorHandler));
				handlers.AddHandler(typeof(UITestEntry), typeof(UITestEntryHandler));
				handlers.AddHandler(typeof(UITestSearchBar), typeof(UITestSearchBarHandler));
#endif
#if IOS
				handlers.AddHandler(typeof(Issue30147CustomScrollView), typeof(Issue30147CustomScrollViewHandler));
#endif
			});

			appBuilder.Services.AddTransient<TransientPage>();
			appBuilder.Services.AddScoped<ScopedPage>();
			return appBuilder.Build();
		}

		static partial void OverrideMainPage(ref Page mainPage);

		public static Page CreateDefaultMainPage()
		{
			Page mainPage = null;
			OverrideMainPage(ref mainPage);
			return mainPage ?? new CoreNavigationPage();
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

		protected override void OnAppLinkRequestReceived(Uri uri)
		{
			base.OnAppLinkRequestReceived(uri);
		}

		protected override Window CreateWindow(IActivationState activationState)
		{
			var window = new Window(MauiProgram.CreateDefaultMainPage());
#if WINDOWS || MACCATALYST

			// For desktop use a fixed window size, so that screenshots are deterministic,
			// matching (as much as possible) between dev machines and CI. Currently
			// our Windows CI machines run at 1024x768 resolution and the window size can't
			// be larger than screen size. We'll investigate increasing CI screen
			// resolution in the future so we can have a bigger window, but for now
			// this size works.
			const int desktopWindowWidth = 1024;
			const int desktopWindowHeight = 768;

			var info = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo;
			int screenWidth = (int)(info.Width / info.Density);
			int screenHeight = (int)(info.Height / info.Density);

#if WINDOWS
			window.Width = desktopWindowWidth;
			window.Height = desktopWindowHeight;

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

			// Setting X and Y without delay doesn't work on Catalyst, Issue: https://github.com/dotnet/maui/issues/27304 
			window.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
			{
				window.X = (screenWidth - desktopWindowWidth) / 2;
				window.Y = (screenHeight - desktopWindowHeight) / 2;
			});
#endif

#endif
			return window;
		}
	}
}
