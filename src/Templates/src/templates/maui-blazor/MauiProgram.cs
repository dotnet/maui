using Microsoft.AspNetCore.Components.WebView.Maui;
using MauiApp._1.Data;

namespace MauiApp._1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		//-:cnd:noEmit
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
		//+:cnd:noEmit

		builder.Services.AddSingleton<WeatherForecastService>();

		return builder.Build();
	}
}
