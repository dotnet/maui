namespace MauiApp._1.Desktop;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(bool useSingleViewLifetime = false)
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseSharedMauiApp()
			.UseAvaloniaApp(useSingleViewLifetime);

		return builder.Build();
	}
}
