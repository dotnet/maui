namespace Maui.Controls.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<CollectionViewMemoryLeaks.App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<CollectionViewMemoryLeaks.Views.MainPage>();
        builder.Services.AddTransient<CollectionViewMemoryLeaks.ViewModels.MainViewModel>();

        builder.Services.AddTransient<CollectionViewMemoryLeaks.Views.CollectionViewSamplePage>();
        builder.Services.AddTransient<CollectionViewMemoryLeaks.ViewModels.CollectionViewSampleViewModel>();
#if DEBUG
        //builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
