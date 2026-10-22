namespace MauiApp._1.Droid;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
#if (avalonia)
			.UseSharedMauiApp()
			.UseAvaloniaEmbedding<AvaloniaApp>();
#else
			.UseSharedMauiApp();
#endif

		return builder.Build();
	}
}
