#if isCsharpMarkupProject
using CommunityToolkit.Maui.Markup;
#endif
using Microsoft.Extensions.Logging;

namespace MauiApp._1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if isCsharpMarkupProject
			.UseMauiCommunityToolkitMarkup()
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

//-:cnd:noEmit
#if DEBUG
		builder.Logging.AddDebug();
#endif
//+:cnd:noEmit

		return builder.Build();
	}
}
