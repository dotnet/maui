using Maui.Controls.Legacy.Sample.Pages;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Legacy.Sample;

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

		// App
		builder.Services.AddTransient<AppWindow, AppWindow>();
		builder.Services.AddTransient<AppShell, AppShell>();

		// Pages
		builder.Services.AddTransient<MainPage, MainPage>();
		builder.Services.AddTransient<PlatformsPage, PlatformsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder;
	}
}
