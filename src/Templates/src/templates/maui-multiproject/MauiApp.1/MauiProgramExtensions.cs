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

//-:cnd:noEmit
#if DEBUG
		builder.Logging.AddDebug();
#endif
//+:cnd:noEmit

		return builder;
	}
}
