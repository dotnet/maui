using Microsoft.Extensions.Logging;

namespace MauiApp._1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if (Linux)
//-:cnd:noEmit
#if GTK
			.UseMauiGtk()
#endif
//+:cnd:noEmit
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

#if (Linux)
//-:cnd:noEmit
#if GTK
		builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();
#else
		builder.Services.AddMauiBlazorWebView();
#endif
//+:cnd:noEmit
#else
		builder.Services.AddMauiBlazorWebView();
#endif

//-:cnd:noEmit
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
//+:cnd:noEmit

		return builder.Build();
	}
}
