using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.HybridWebApp;

static class MauiProgram
{
	internal static string StartupId { get; } = Guid.NewGuid().ToString("N");
	internal static DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
#if DEBUG
	internal static DevelopmentSettings? Development { get; private set; }
#endif

	public static MauiApp CreateMauiApp()
	{
#if DEBUG
		Development = DevelopmentSettings.Read();
#if !HYBRIDWEBAPP_IN_TREE
		if (Development?.Upstream is not null)
			throw new InvalidOperationException("Development forwarding requires the in-tree framework build with its stopped-task guard.");
#endif
		var mode = Development?.Upstream is null ? "packaged" : "vite";
#else
		var mode = "packaged";
#endif
		Console.WriteLine($"HYBRIDWEBAPP_START pid={Environment.ProcessId} startup={StartupId} utc={StartedAt:O} content={mode}");

		return MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.Build();
	}
}
