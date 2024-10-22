using Microsoft.Extensions.Logging;
#if (SampleContent)
using MauiApp._1.Shared.Services;
using MauiApp._1.Services;
#endif

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

        #if (SampleContent)
        // Add device-specific services used by the MauiApp._1.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        #endif
        builder.Services.AddMauiBlazorWebView();

//-:cnd:noEmit
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
//+:cnd:noEmit

        return builder.Build();
    }
}
