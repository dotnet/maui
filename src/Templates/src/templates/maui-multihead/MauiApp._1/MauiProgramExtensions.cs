using MauiApp._1.Pages;
using Microsoft.Extensions.Logging;

namespace MauiApp._1;

public static class MauiProgramExtensions
{
	public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
	{
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// app
		builder.Services.AddTransient<AppWindow, AppWindow>();
		builder.Services.AddTransient<AppShell, AppShell>();

		// pages
		builder.Services.AddTransient<MainPage, MainPage>();
		builder.Services.AddTransient<PlatformsPage, PlatformsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder;
	}
}
