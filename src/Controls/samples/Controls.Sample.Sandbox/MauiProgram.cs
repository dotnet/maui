using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp() =>
		MauiApp
			.CreateBuilder()
#if __ANDROID__ || __IOS__
			.UseMauiMaps()
#endif
				.UseMauiApp<App>()
				.Build();
	}

	class App : Microsoft.Maui.Controls.Application
	{
		protected override Window CreateWindow(IActivationState? activationState)
		{
			// To test shell scenarios, change this to true
			bool useShell = false;
			On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

			if (!useShell)
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
			})
			.Build();
}
