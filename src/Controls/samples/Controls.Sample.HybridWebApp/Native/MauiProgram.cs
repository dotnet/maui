using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.HybridWebApp;

static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Console.WriteLine($"HYBRIDWEBAPP_START pid={Environment.ProcessId} startup={Guid.NewGuid():N} utc={DateTimeOffset.UtcNow:O} content=packaged");

		return MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.Build();
	}
}
