using Bielik.Core;
using Bielik.ViewModels;
using Bielik.Views;
using Microsoft.Extensions.Logging;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace Bielik;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>();
        builder.Services.AddSingleton(_ => BielikClient.CreateLocalHttpClient());
        builder.Services.AddSingleton<BielikClient>();
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<ChatViewModel>();
        builder.Services.AddSingleton<DiscoverPage>();
        builder.Services.AddSingleton<ChatPage>();
        builder.Services.AddSingleton<ModelPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<Func<AppShell>>(services => () => services.GetRequiredService<AppShell>());
#if DEBUG
        builder.Logging.AddDebug();
        builder.AddMauiDevFlowAgent(options => options.Port = 9235);
#endif
        return builder.Build();
    }
}
