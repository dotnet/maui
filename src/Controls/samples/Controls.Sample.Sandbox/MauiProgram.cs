using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
				fonts.AddFont("LobsterTwo-Regular.ttf", "Lobster Two");
				fonts.AddFont("LobsterTwo-Bold.ttf", "Lobster Two Bold");
				fonts.AddFont("LobsterTwo-Italic.ttf", "Lobster Two Italic");
				fonts.AddFont("LobsterTwo-BoldItalic.ttf", "Lobster Two BoldItalic");
				fonts.AddFont("ionicons.ttf", "Ionicons");
				fonts.AddFont("SegoeUI.ttf", "Segoe UI");
				fonts.AddFont("SegoeUI-Bold.ttf", "Segoe UI Bold");
				fonts.AddFont("SegoeUI-Italic.ttf", "Segoe UI Italic");
				fonts.AddFont("SegoeUI-Bold-Italic.ttf", "Segoe UI Bold Italic");
			});

		// MAUI already instruments every measure/arrange through
		// src/Core/src/Diagnostics/Instrumentation/LayoutDiagnosticMetrics.cs, but the counters are only
		// created when an IMeterFactory is present in DI. Registering one here lets the Sandbox read the
		// framework's own maui.layout.measure_count / maui.layout.arrange_count without touching src/.
		builder.Services.AddSingleton<IMeterFactory, LayoutMetrics.SandboxMeterFactory>();

		return builder.Build();
	}
}
