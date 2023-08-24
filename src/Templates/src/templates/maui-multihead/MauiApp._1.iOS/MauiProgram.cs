using MauiApp._1.iOS.Services;
using MauiApp._1.Services;

namespace MauiApp._1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseSharedMauiApp();

		builder.Services.AddTransient<IPlatformSpecificService, iOSService>();

		return builder.Build();
	}
}
